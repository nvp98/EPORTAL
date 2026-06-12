using EPORTAL.Models;
using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    /// <summary>
    /// Admin dashboard cho View360 - do "user su dung thuc" qua bang View360_AccessLog.
    /// Permission: tai su dung key "Report" trong A_QuyenCT (zero DB work).
    /// Scope phase 1: Project + Virtual (Video chua co tracking).
    ///
    /// VI SAO RAW SQL (Database.SqlQuery) cho toan bo endpoint KPI o day:
    ///  - Day la cac truy van AGGREGATE/ANALYTICS: GROUP BY theo ngay/gio/phong ban,
    ///    COUNT(DISTINCT ...), recursive CTE (cay nhom), NOT EXISTS (noi dung chua dung).
    ///    LINQ-to-Entities khong dien dat duoc CTE de quy va sinh SQL kem cho aggregate phuc tap.
    ///  - View360_AccessLog la BANG MOI, chua map vao EDMX -> khong query qua db.* (EF) duoc.
    ///  - Tat ca tham so hoa qua MakeParams (SqlParameter) -> khong co injection.
    /// </summary>
    public class DashboardController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = MyAuthentication.IDQuyenHT;
        const string PermissionKey = "Report";

        // GET: View360/Dashboard
        public ActionResult Index()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, PermissionKey, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return View();
        }

        // ----- JSON endpoints ------------------------------------------------

        /// <summary>
        /// Self-check: bang View360_AccessLog ton tai chua? Co data chua? Co bao nhieu PhongBan,
        /// NhanVien, AuthorizationUSER/Vitual? Dung de chan doan widget rong.
        /// </summary>
        public JsonResult Diagnostic()
        {
            if (!IsAuthorized()) return JsonForbidden();

            var diag = new Dictionary<string, object>();
            diag["accessLogExists"] = TableExists("View360_AccessLog");

            if ((bool)diag["accessLogExists"])
            {
                try { diag["accessLogRows"] = ScalarInt("SELECT COUNT(*) FROM dbo.View360_AccessLog"); }
                catch (Exception ex) { diag["accessLogError"] = ex.Message; }
            }

            try { diag["phongBanCount"] = ScalarInt("SELECT COUNT(*) FROM dbo.PhongBan"); }
            catch (Exception ex) { diag["phongBanError"] = ex.Message; }

            try { diag["nhanVienCount"] = ScalarInt("SELECT COUNT(*) FROM dbo.NhanVien"); }
            catch (Exception ex) { diag["nhanVienError"] = ex.Message; }

            try { diag["authUserRows"] = ScalarInt(
                "SELECT COUNT(*) FROM dbo.AuthorizationUSER WHERE NhanVienID IS NOT NULL"); }
            catch (Exception ex) { diag["authUserError"] = ex.Message; }

            try { diag["authVitualRows"] = ScalarInt(
                "SELECT COUNT(*) FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL"); }
            catch (Exception ex) { diag["authVitualError"] = ex.Message; }

            try { diag["nhanVienWithDept"] = ScalarInt(
                "SELECT COUNT(*) FROM dbo.NhanVien WHERE IDPhongBan IS NOT NULL"); }
            catch (Exception ex) { diag["nhanVienDeptError"] = ex.Message; }

            return Json(diag, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// KPI tinh trang ho thong (khong phu thuoc ky): tong content, tong grants, do phu PhongBan.
        /// </summary>
        public JsonResult Inventory()
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                var projectCount = ScalarInt("SELECT COUNT(*) FROM dbo.Projects");
                var virtualCount = ScalarInt("SELECT COUNT(*) FROM dbo.Virtual");
                var grantsProject = ScalarInt("SELECT COUNT(*) FROM dbo.AuthorizationUSER WHERE NhanVienID IS NOT NULL");
                var grantsVirtual = ScalarInt("SELECT COUNT(*) FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL");

                int activeDepts = 0, totalDepts = 0;
                if (TableExists("View360_AccessLog"))
                {
                    activeDepts = ScalarInt(
                        @"SELECT COUNT(DISTINCT n.IDPhongBan)
                            FROM dbo.View360_AccessLog al
                            JOIN dbo.NhanVien n ON al.NhanVienID = n.ID
                           WHERE n.IDPhongBan IS NOT NULL");
                }
                totalDepts = ScalarInt(
                    @"SELECT COUNT(DISTINCT pb.IDPhongBan)
                        FROM dbo.PhongBan pb
                       WHERE EXISTS (
                           SELECT 1 FROM dbo.NhanVien n
                            WHERE n.IDPhongBan = pb.IDPhongBan
                              AND (n.ID IN (SELECT NhanVienID FROM dbo.AuthorizationUSER WHERE NhanVienID IS NOT NULL)
                                OR n.ID IN (SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL))
                       )");

                double coverage = totalDepts > 0 ? (double)activeDepts / totalDepts : 0;

                return Json(new
                {
                    projectCount,
                    virtualCount,
                    totalContent = projectCount + virtualCount,
                    grantsProject,
                    grantsVirtual,
                    totalGrants = grantsProject + grantsVirtual,
                    activeDepts,
                    totalDepts,
                    coverage = Math.Round(coverage * 100, 1)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        public JsonResult Kpi(string period)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                var from = ResolveFrom(period);
                var hasLog = TableExists("View360_AccessLog");

                // Period truoc (cung do dai) de so sanh delta.
                var periodDays = (DateTime.Now.Date - from).Days + 1;
                var prevFrom = from.AddDays(-periodDays);
                var prevTo = from.AddSeconds(-1);

                var activeUsers = hasLog ? ScalarInt(
                    @"SELECT COUNT(DISTINCT NhanVienID) FROM dbo.View360_AccessLog WHERE AccessAt >= @from",
                    new SqlParameter("@from", from)) : 0;

                var prevActiveUsers = hasLog ? ScalarInt(
                    @"SELECT COUNT(DISTINCT NhanVienID) FROM dbo.View360_AccessLog
                      WHERE AccessAt >= @from AND AccessAt <= @to",
                    new SqlParameter("@from", prevFrom), new SqlParameter("@to", prevTo)) : 0;

                var totalViews = hasLog ? ScalarInt(
                    @"SELECT COUNT(*) FROM dbo.View360_AccessLog WHERE AccessAt >= @from",
                    new SqlParameter("@from", from)) : 0;

                var prevTotalViews = hasLog ? ScalarInt(
                    @"SELECT COUNT(*) FROM dbo.View360_AccessLog
                      WHERE AccessAt >= @from AND AccessAt <= @to",
                    new SqlParameter("@from", prevFrom), new SqlParameter("@to", prevTo)) : 0;

                // Mau so utilization: chi tinh user dang hoat dong (IDTinhTrangLV=1) co quyen.
                var grantedActiveUsers = ScalarInt(
                    @"SELECT COUNT(*) FROM (
                         SELECT au.NhanVienID FROM dbo.AuthorizationUSER  au
                          JOIN dbo.NhanVien n ON au.NhanVienID = n.ID AND n.IDTinhTrangLV = 1
                         UNION
                         SELECT av.NhanVienID FROM dbo.AuthorizationVitual av
                          JOIN dbo.NhanVien n ON av.NhanVienID = n.ID AND n.IDTinhTrangLV = 1
                      ) u");

                double utilization = grantedActiveUsers > 0 ? (double)activeUsers / grantedActiveUsers : 0;

                return Json(new
                {
                    period = period ?? "30d",
                    from = from.ToString("yyyy-MM-dd"),
                    activeUsers,
                    prevActiveUsers,
                    grantedUsers = grantedActiveUsers,
                    totalViews,
                    prevTotalViews,
                    utilization = Math.Round(utilization * 100, 1),
                    accessLogReady = hasLog
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        public JsonResult Trend(string period)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
            var from = ResolveFrom(period);

            if (!TableExists("View360_AccessLog"))
                return Json(new { days = new string[0], project = new int[0], @virtual = new int[0] },
                    JsonRequestBehavior.AllowGet);

            // Mot row / ngay / loai. Client xu ly fill missing days neu can.
            var rows = db.Database.SqlQuery<TrendRow>(
                @"SELECT CAST(AccessAt AS DATE) AS Day, ContentType, COUNT(*) AS Views
                    FROM dbo.View360_AccessLog
                   WHERE AccessAt >= @p0
                   GROUP BY CAST(AccessAt AS DATE), ContentType
                   ORDER BY Day", from).ToList();

            // Tao day day du cac ngay (de chart line khong bi nhay).
            var days = new List<string>();
            var projectByDay = new Dictionary<string, int>();
            var virtualByDay = new Dictionary<string, int>();
            for (var d = from.Date; d <= DateTime.Now.Date; d = d.AddDays(1))
            {
                var key = d.ToString("yyyy-MM-dd");
                days.Add(key);
                projectByDay[key] = 0;
                virtualByDay[key] = 0;
            }
            foreach (var r in rows)
            {
                var key = r.Day.ToString("yyyy-MM-dd");
                if (r.ContentType == 1 && projectByDay.ContainsKey(key)) projectByDay[key] = r.Views;
                else if (r.ContentType == 2 && virtualByDay.ContainsKey(key)) virtualByDay[key] = r.Views;
            }

            return Json(new
            {
                days,
                project = days.Select(k => projectByDay[k]).ToArray(),
                @virtual = days.Select(k => virtualByDay[k]).ToArray()
            }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        /// <summary>
        /// Explorer mode "time": paged raw access log voi filter search / type / drill-down.
        /// </summary>
        public JsonResult ExplorerLog(string period, string search, int? type,
                                       int? userId, int? contentType, int? contentId,
                                       int page = 1, int pageSize = 50)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 10 || pageSize > 200) pageSize = 50;
                if (!TableExists("View360_AccessLog"))
                    return Json(new { rows = new object[0], total = 0, page, pageSize, totalPages = 0 },
                        JsonRequestBehavior.AllowGet);

                var from = ResolveFrom(period);

                // Luu raw values - moi query se build SqlParameter MOI tu values nay.
                // Khong reuse SqlParameter object giua queries (EF6 quy dinh).
                var paramValues = new Dictionary<string, object> { { "@from", from } };
                var where = " WHERE al.AccessAt >= @from";

                if (type.HasValue && (type.Value == 1 || type.Value == 2))
                {
                    where += " AND al.ContentType = @type";
                    paramValues["@type"] = type.Value;
                }
                if (userId.HasValue)
                {
                    where += " AND al.NhanVienID = @userId";
                    paramValues["@userId"] = userId.Value;
                }
                if (contentType.HasValue && contentId.HasValue)
                {
                    where += " AND al.ContentType = @ct AND al.ContentID = @cid";
                    paramValues["@ct"] = contentType.Value;
                    paramValues["@cid"] = contentId.Value;
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    where += @" AND (n.HoTen LIKE @search OR n.MaNV LIKE @search
                                  OR p.Title LIKE @search OR v.Title LIKE @search)";
                    paramValues["@search"] = "%" + search.Trim() + "%";
                }

                var fromJoin = @"
                    FROM dbo.View360_AccessLog al
                    LEFT JOIN dbo.NhanVien   n  ON al.NhanVienID = n.ID
                    LEFT JOIN dbo.PhongBan   pb ON n.IDPhongBan  = pb.IDPhongBan
                    LEFT JOIN dbo.Projects   p  ON al.ContentType = 1 AND al.ContentID = p.ID
                    LEFT JOIN dbo.Virtual    v  ON al.ContentType = 2 AND al.ContentID = v.ID";

                // 1. Total count
                var totalSql = "SELECT COUNT(*) " + fromJoin + where;
                var total = db.Database.SqlQuery<int>(totalSql, MakeParams(paramValues)).First();

                // 2. Paged rows - them paging params, build SqlParameter[] MOI lan nua
                var offset = (page - 1) * pageSize;
                paramValues["@offset"] = offset;
                paramValues["@pageSize"] = pageSize;

                var pagedSql = @"SELECT al.AccessAt AS AccessAt, al.NhanVienID AS NhanVienID,
                                        al.ContentType AS ContentType, al.ContentID AS ContentID,
                                        n.MaNV AS MaNV, n.HoTen AS HoTen, pb.TenPhongBan AS TenPhongBan,
                                        CASE al.ContentType WHEN 1 THEN p.Title WHEN 2 THEN v.Title END AS Title "
                              + fromJoin + where +
                              @" ORDER BY al.AccessAt DESC
                                 OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

                var rows = db.Database.SqlQuery<LogRow>(pagedSql, MakeParams(paramValues))
                    .ToList()
                    .Select(r => new {
                        accessAt = r.AccessAt.ToString("yyyy-MM-dd HH:mm"),
                        accessAtIso = r.AccessAt.ToString("o"),
                        nhanVienId = r.NhanVienID,
                        maNV = r.MaNV,
                        hoTen = r.HoTen ?? "(không xác định)",
                        phongBan = r.TenPhongBan,
                        contentType = r.ContentType,
                        contentTypeName = r.ContentType == 1 ? "Project" : "Virtual",
                        contentId = r.ContentID,
                        title = r.Title ?? "(đã xoá)",
                        detailsUrl = r.ContentType == 1
                            ? Url.Action("Details", "ListProject", new { area = "View360", id = r.ContentID })
                            : Url.Action("Details", "ListVirtual", new { area = "View360", id = r.ContentID })
                    });

                return Json(new { rows, total, page, pageSize,
                    totalPages = (int)Math.Ceiling((double)total / pageSize) },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        /// <summary>
        /// Tra 3 count cho 3 tab Explorer trong 1 query - de JS refresh badge cung luc.
        /// Cung filter (period/search/type/drill) ap dung cho ca 3 -> dam bao count nhat quan.
        /// </summary>
        public JsonResult ExplorerCounts(string period, string search, int? type,
                                          int? userId, int? contentType, int? contentId)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                if (!TableExists("View360_AccessLog"))
                    return Json(new { time = 0, user = 0, content = 0 }, JsonRequestBehavior.AllowGet);

                var from = ResolveFrom(period);
                var paramValues = new Dictionary<string, object> { { "@from", from } };
                var where = " WHERE al.AccessAt >= @from";

                if (type.HasValue && (type.Value == 1 || type.Value == 2))
                {
                    where += " AND al.ContentType = @type";
                    paramValues["@type"] = type.Value;
                }
                if (userId.HasValue)
                {
                    where += " AND al.NhanVienID = @userId";
                    paramValues["@userId"] = userId.Value;
                }
                if (contentType.HasValue && contentId.HasValue)
                {
                    where += " AND al.ContentType = @ct AND al.ContentID = @cid";
                    paramValues["@ct"] = contentType.Value;
                    paramValues["@cid"] = contentId.Value;
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    where += @" AND (n.HoTen LIKE @search OR n.MaNV LIKE @search
                                  OR p.Title LIKE @search OR v.Title LIKE @search)";
                    paramValues["@search"] = "%" + search.Trim() + "%";
                }

                var sql = @"SELECT
                              COUNT(*) AS TimeCount,
                              COUNT(DISTINCT al.NhanVienID) AS UserCount,
                              COUNT(DISTINCT CAST(al.ContentType AS VARCHAR) + '-' + CAST(al.ContentID AS VARCHAR)) AS ContentCount
                            FROM dbo.View360_AccessLog al
                            LEFT JOIN dbo.NhanVien n ON al.NhanVienID = n.ID
                            LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan
                            LEFT JOIN dbo.Projects p ON al.ContentType = 1 AND al.ContentID = p.ID
                            LEFT JOIN dbo.Virtual  v ON al.ContentType = 2 AND al.ContentID = v.ID"
                          + where;

                var row = db.Database.SqlQuery<CountsRow>(sql, MakeParams(paramValues)).First();
                return Json(new { time = row.TimeCount, user = row.UserCount, content = row.ContentCount },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        /// <summary>
        /// Build fresh SqlParameter[] tu dict raw values. Goi moi lan truoc khi SqlQuery,
        /// tranh "parameter already contained by another collection" khi reuse SqlParameter.
        /// </summary>
        private static object[] MakeParams(Dictionary<string, object> values)
        {
            return values.Select(kv => (object)new SqlParameter(kv.Key, kv.Value ?? DBNull.Value)).ToArray();
        }

        /// <summary>
        /// Explorer mode "user": aggregate theo user, full list (khong limit Top N).
        /// </summary>
        public JsonResult ExplorerByUser(string period, string search, int? type, int n = 200)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                if (n < 10 || n > 1000) n = 200;
                if (!TableExists("View360_AccessLog"))
                    return Json(new object[0], JsonRequestBehavior.AllowGet);

                var from = ResolveFrom(period);
                var paramValues = new Dictionary<string, object> {
                    { "@from", from }, { "@top", n } };
                var where = " WHERE al.AccessAt >= @from";
                if (type.HasValue && (type.Value == 1 || type.Value == 2))
                {
                    where += " AND al.ContentType = @type";
                    paramValues["@type"] = type.Value;
                }
                // Search thong nhat: match CA user name/MaNV LAN content title.
                // Lam vay de search "X" tra ket qua nhat quan giua cac tab.
                if (!string.IsNullOrWhiteSpace(search))
                {
                    where += @" AND (n.HoTen LIKE @search OR n.MaNV LIKE @search
                                  OR p.Title LIKE @search OR v.Title LIKE @search)";
                    paramValues["@search"] = "%" + search.Trim() + "%";
                }

                var sql = @"SELECT TOP (@top) al.NhanVienID AS NhanVienID,
                                   n.MaNV AS MaNV, n.HoTen AS HoTen, pb.TenPhongBan AS TenPhongBan,
                                   COUNT(*) AS Views,
                                   COUNT(DISTINCT CAST(al.ContentType AS VARCHAR) + '-' + CAST(al.ContentID AS VARCHAR)) AS UniqueContent,
                                   MAX(al.AccessAt) AS LastAccess
                              FROM dbo.View360_AccessLog al
                              LEFT JOIN dbo.NhanVien n ON al.NhanVienID = n.ID
                              LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan
                              LEFT JOIN dbo.Projects p ON al.ContentType = 1 AND al.ContentID = p.ID
                              LEFT JOIN dbo.Virtual  v ON al.ContentType = 2 AND al.ContentID = v.ID"
                          + where +
                          @" GROUP BY al.NhanVienID, n.MaNV, n.HoTen, pb.TenPhongBan
                             ORDER BY COUNT(*) DESC";

                var rows = db.Database.SqlQuery<UserAggRow>(sql, MakeParams(paramValues))
                    .ToList()
                    .Select(r => new {
                        nhanVienId = r.NhanVienID,
                        maNV = r.MaNV,
                        hoTen = r.HoTen ?? "(không xác định)",
                        phongBan = r.TenPhongBan,
                        views = r.Views,
                        uniqueContent = r.UniqueContent,
                        lastAccess = r.LastAccess.ToString("yyyy-MM-dd HH:mm")
                    });

                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        /// <summary>
        /// Explorer mode "content": aggregate theo content (Project / Virtual), full list.
        /// </summary>
        public JsonResult ExplorerByContent(string period, string search, int? type, int n = 200)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                if (n < 10 || n > 1000) n = 200;
                if (!TableExists("View360_AccessLog"))
                    return Json(new object[0], JsonRequestBehavior.AllowGet);

                var from = ResolveFrom(period);
                var paramValues = new Dictionary<string, object> {
                    { "@from", from }, { "@top", n } };
                var where = " WHERE al.AccessAt >= @from";
                if (type.HasValue && (type.Value == 1 || type.Value == 2))
                {
                    where += " AND al.ContentType = @type";
                    paramValues["@type"] = type.Value;
                }
                // Search thong nhat: match CA content title LAN user name/MaNV.
                if (!string.IsNullOrWhiteSpace(search))
                {
                    where += @" AND (n.HoTen LIKE @search OR n.MaNV LIKE @search
                                  OR p.Title LIKE @search OR v.Title LIKE @search)";
                    paramValues["@search"] = "%" + search.Trim() + "%";
                }

                var sql = @"SELECT TOP (@top) al.ContentType AS ContentType, al.ContentID AS ContentID,
                                   CASE al.ContentType WHEN 1 THEN p.Title WHEN 2 THEN v.Title END AS Title,
                                   COUNT(*) AS Views,
                                   COUNT(DISTINCT al.NhanVienID) AS UniqueUsers,
                                   MIN(al.AccessAt) AS FirstAccess,
                                   MAX(al.AccessAt) AS LastAccess
                              FROM dbo.View360_AccessLog al
                              LEFT JOIN dbo.NhanVien n ON al.NhanVienID = n.ID
                              LEFT JOIN dbo.Projects p ON al.ContentType = 1 AND al.ContentID = p.ID
                              LEFT JOIN dbo.Virtual  v ON al.ContentType = 2 AND al.ContentID = v.ID"
                          + where +
                          @" GROUP BY al.ContentType, al.ContentID, p.Title, v.Title
                             ORDER BY COUNT(*) DESC";

                var rows = db.Database.SqlQuery<ContentAggRow>(sql, MakeParams(paramValues))
                    .ToList()
                    .Select(r => new {
                        contentType = r.ContentType,
                        contentTypeName = r.ContentType == 1 ? "Project" : "Virtual",
                        contentId = r.ContentID,
                        title = r.Title ?? "(đã xoá)",
                        views = r.Views,
                        uniqueUsers = r.UniqueUsers,
                        firstAccess = r.FirstAccess.ToString("yyyy-MM-dd HH:mm"),
                        lastAccess = r.LastAccess.ToString("yyyy-MM-dd HH:mm"),
                        detailsUrl = r.ContentType == 1
                            ? Url.Action("Details", "ListProject", new { area = "View360", id = r.ContentID })
                            : Url.Action("Details", "ListVirtual", new { area = "View360", id = r.ContentID })
                    });

                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        /// <summary>
        /// Combined per-PhongBan view: granted (period-independent), active in period, views in period.
        /// Replace 2 chart cu (DeptPermissions + DeptViews) bang 1 bullet chart goc nhin tot hon:
        /// gap giua "Cap quyen" va "Active" la "quyen chua dung".
        /// </summary>
        public JsonResult DeptUsage(string period, int top = 30)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                if (top < 1 || top > 100) top = 30;
                var from = ResolveFrom(period);
                var hasLog = TableExists("View360_AccessLog");

                var paramValues = new Dictionary<string, object> { { "@top", top } };
                if (hasLog) paramValues["@from"] = from;

                // 2 subquery: granted (UNION distinct), active+views (tu access log neu co)
                var sql = @"SELECT TOP (@top)
                                pb.IDPhongBan AS Id,
                                pb.TenPhongBan AS Name,
                                ISNULL(g.Granted, 0) AS Granted,
                                " + (hasLog ? "ISNULL(a.Active, 0)" : "0") + @" AS Active,
                                " + (hasLog ? "ISNULL(a.Views, 0)"  : "0") + @" AS Views
                              FROM dbo.PhongBan pb
                              LEFT JOIN (
                                  SELECT n.IDPhongBan, COUNT(DISTINCT u.NhanVienID) AS Granted
                                    FROM (
                                       SELECT NhanVienID FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                                       UNION
                                       SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                                    ) u
                                    JOIN dbo.NhanVien n ON u.NhanVienID = n.ID
                                   GROUP BY n.IDPhongBan
                              ) g ON g.IDPhongBan = pb.IDPhongBan"
                              + (hasLog ? @"
                              LEFT JOIN (
                                  SELECT n.IDPhongBan,
                                         COUNT(DISTINCT al.NhanVienID) AS Active,
                                         COUNT(*) AS Views
                                    FROM dbo.View360_AccessLog al
                                    JOIN dbo.NhanVien n ON al.NhanVienID = n.ID
                                   WHERE al.AccessAt >= @from
                                   GROUP BY n.IDPhongBan
                              ) a ON a.IDPhongBan = pb.IDPhongBan" : "") + @"
                             WHERE (ISNULL(g.Granted, 0) + " + (hasLog ? "ISNULL(a.Active, 0) + ISNULL(a.Views, 0)" : "0") + @") > 0
                             ORDER BY ISNULL(g.Granted, 0) DESC";

                var rows = db.Database.SqlQuery<DeptUsageRow>(sql, MakeParams(paramValues)).ToList();

                // Total dept co activity (de hien "X / Y" - co bao nhieu dept bi cat ra)
                var totalActive = ScalarInt(@"
                    SELECT COUNT(*) FROM dbo.PhongBan pb
                     WHERE EXISTS (
                        SELECT 1 FROM (
                            SELECT NhanVienID FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                            UNION
                            SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                        ) u
                        JOIN dbo.NhanVien n ON u.NhanVienID = n.ID
                        WHERE n.IDPhongBan = pb.IDPhongBan
                     )");

                return Json(new
                {
                    labels = rows.Select(r => r.Name ?? "(chưa đặt tên)").ToArray(),
                    granted = rows.Select(r => r.Granted).ToArray(),
                    active = rows.Select(r => r.Active).ToArray(),
                    views = rows.Select(r => r.Views).ToArray(),
                    shown = rows.Count,
                    totalDeptWithGrants = totalActive
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        public JsonResult DeptPermissions(int top = 15)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
            if (top < 1 || top > 50) top = 15;

            // Dem distinct NhanVienID trong moi PhongBan co quyen Project / Virtual.
            var rows = db.Database.SqlQuery<DeptRow>(
                @"SELECT TOP (@p0) IDPhongBan AS Id, TenPhongBan AS Name,
                         ProjectUsers AS A, VirtualUsers AS B
                  FROM (
                    SELECT pb.IDPhongBan, pb.TenPhongBan,
                      (SELECT COUNT(DISTINCT au.NhanVienID)
                         FROM dbo.AuthorizationUSER au
                         JOIN dbo.NhanVien n ON au.NhanVienID = n.ID
                        WHERE n.IDPhongBan = pb.IDPhongBan) AS ProjectUsers,
                      (SELECT COUNT(DISTINCT av.NhanVienID)
                         FROM dbo.AuthorizationVitual av
                         JOIN dbo.NhanVien n ON av.NhanVienID = n.ID
                        WHERE n.IDPhongBan = pb.IDPhongBan) AS VirtualUsers
                    FROM dbo.PhongBan pb
                  ) x
                  WHERE (ProjectUsers + VirtualUsers) > 0
                  ORDER BY (ProjectUsers + VirtualUsers) DESC", top).ToList();

            return Json(new
            {
                labels = rows.Select(r => r.Name ?? "(chưa đặt tên)").ToArray(),
                project = rows.Select(r => r.A).ToArray(),
                @virtual = rows.Select(r => r.B).ToArray()
            }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        public JsonResult DeptViews(string period, int top = 15)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
            var from = ResolveFrom(period);
            if (top < 1 || top > 50) top = 15;
            if (!TableExists("View360_AccessLog"))
                return Json(new { labels = new string[0], project = new int[0], @virtual = new int[0] },
                    JsonRequestBehavior.AllowGet);

            var rows = db.Database.SqlQuery<DeptRow>(
                @"SELECT TOP (@p1) n.IDPhongBan AS Id, pb.TenPhongBan AS Name,
                         SUM(CASE WHEN al.ContentType = 1 THEN 1 ELSE 0 END) AS A,
                         SUM(CASE WHEN al.ContentType = 2 THEN 1 ELSE 0 END) AS B
                    FROM dbo.View360_AccessLog al
                    JOIN dbo.NhanVien n ON al.NhanVienID = n.ID
                    LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan
                   WHERE al.AccessAt >= @p0
                   GROUP BY n.IDPhongBan, pb.TenPhongBan
                   ORDER BY COUNT(*) DESC", from, top).ToList();

            return Json(new
            {
                labels = rows.Select(r => r.Name ?? "(không xác định)").ToArray(),
                project = rows.Select(r => r.A).ToArray(),
                @virtual = rows.Select(r => r.B).ToArray()
            }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        public JsonResult HourlyActivity(string period)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
            var from = ResolveFrom(period);
            if (!TableExists("View360_AccessLog"))
                return Json(new { counts = new int[24] }, JsonRequestBehavior.AllowGet);

            var rows = db.Database.SqlQuery<HourRow>(
                @"SELECT DATEPART(HOUR, AccessAt) AS H, COUNT(*) AS C
                    FROM dbo.View360_AccessLog
                   WHERE AccessAt >= @p0
                   GROUP BY DATEPART(HOUR, AccessAt)", from).ToList();

            var counts = new int[24];
            foreach (var r in rows)
                if (r.H >= 0 && r.H < 24) counts[r.H] = r.C;

            return Json(new { counts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        public JsonResult StalePermissions(int n = 20)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
            if (n < 1 || n > 100) n = 20;
            if (!TableExists("View360_AccessLog"))
                return Json(new {
                    notReady = true,
                    message = "Bảng tracking chưa tồn tại — chạy migration trước.",
                    rows = new object[0]
                }, JsonRequestBehavior.AllowGet);

            // GRACE PERIOD: tracking moi bat thi MOI user "khong co AccessLog" la noise.
            // Lay MIN(AccessAt) lam reference - user phai "khong xem" sau >= 30 ngay tracking
            // moi tinh la stale. Truoc do, danh sach se trong (correct behavior).
            var trackingStart = db.Database.SqlQuery<DateTime?>(
                "SELECT MIN(AccessAt) FROM dbo.View360_AccessLog").FirstOrDefault();
            var trackingDays = trackingStart.HasValue
                ? (int)(DateTime.Now - trackingStart.Value).TotalDays : 0;

            const int GraceDays = 30;
            if (!trackingStart.HasValue || trackingDays < GraceDays)
            {
                return Json(new {
                    notReady = true,
                    trackingDays,
                    graceDays = GraceDays,
                    message = "Tracking mới chạy " + trackingDays + " ngày. "
                              + "Danh sách thu hồi quyền sẽ chính xác sau "
                              + (GraceDays - trackingDays) + " ngày nữa (đợi grace period).",
                    rows = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }

            // Sau grace period: user (1) la NV dang hoat dong (IDTinhTrangLV=1), (2) co quyen
            // cap truoc khi tracking bat dau, (3) trong suot thoi gian tracking khong mo lan nao.
            var rows = db.Database.SqlQuery<StaleRow>(
                @"SELECT TOP (@p0)
                         g.NhanVienID, n.MaNV, n.HoTen, pb.TenPhongBan,
                         g.FirstGrant,
                         DATEDIFF(DAY, g.FirstGrant, GETDATE()) AS DaysSinceGrant
                    FROM (
                        SELECT NhanVienID, MIN(COALESCE(Createdate, '2020-01-01')) AS FirstGrant
                          FROM (
                            SELECT NhanVienID, Createdate FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                            UNION ALL
                            SELECT NhanVienID, Createdate FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                          ) u
                         GROUP BY NhanVienID
                    ) g
                    JOIN dbo.NhanVien n ON g.NhanVienID = n.ID
                    LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan
                   WHERE n.IDTinhTrangLV = 1
                     AND g.FirstGrant <= @p1
                     AND NOT EXISTS (
                         SELECT 1 FROM dbo.View360_AccessLog al
                          WHERE al.NhanVienID = g.NhanVienID
                     )
                   ORDER BY g.FirstGrant ASC", n, trackingStart.Value).ToList();

            return Json(new {
                notReady = false,
                trackingDays,
                rows = rows.Select(r => new {
                    nhanVienId = r.NhanVienID,
                    maNV = r.MaNV,
                    hoTen = r.HoTen,
                    phongBan = r.TenPhongBan,
                    firstGrant = r.FirstGrant.ToString("yyyy-MM-dd"),
                    daysSinceGrant = r.DaysSinceGrant
                })
            }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        /// <summary>
        /// Roll-up theo top-level ProjectsGroup (parent groups). Voi moi root group,
        /// tinh tong content + views cua TAT CA descendants. Tan dung hierarchy moi.
        /// </summary>
        public JsonResult GroupAnalytics(string period)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                var from = ResolveFrom(period);
                var hasLog = TableExists("View360_AccessLog");

                // Recursive CTE: voi moi group, xac dinh root ancestor.
                var sql = @"
                    WITH GroupTree AS (
                        SELECT IDGroup, GroupName, ParentIDGroup, IDGroup AS RootID
                          FROM dbo.ProjectsGroup
                         WHERE ParentIDGroup IS NULL
                        UNION ALL
                        SELECT pg.IDGroup, pg.GroupName, pg.ParentIDGroup, gt.RootID
                          FROM dbo.ProjectsGroup pg
                          JOIN GroupTree gt ON pg.ParentIDGroup = gt.IDGroup
                    )
                    SELECT root.IDGroup AS Id,
                           root.GroupName AS Name,
                           ISNULL(stat.ProjectCount, 0) AS ProjectCount,
                           ISNULL(stat.ViewCount, 0) AS ViewCount,
                           ISNULL(stat.UniqueUsers, 0) AS UniqueUsers
                      FROM dbo.ProjectsGroup root
                      LEFT JOIN (
                          SELECT gt.RootID,
                                 COUNT(DISTINCT p.ID) AS ProjectCount,
                                 COUNT(al.ID) AS ViewCount,
                                 COUNT(DISTINCT al.NhanVienID) AS UniqueUsers
                            FROM GroupTree gt
                            LEFT JOIN dbo.Projects p ON p.IDGroup = gt.IDGroup
                            " + (hasLog
                                ? @"LEFT JOIN dbo.View360_AccessLog al
                                       ON al.ContentType = 1 AND al.ContentID = p.ID AND al.AccessAt >= @p0"
                                : @"LEFT JOIN (SELECT CAST(NULL AS BIGINT) AS ID,
                                                      CAST(NULL AS INT) AS NhanVienID,
                                                      CAST(NULL AS TINYINT) AS ContentType,
                                                      CAST(NULL AS INT) AS ContentID,
                                                      CAST(NULL AS DATETIME) AS AccessAt
                                               WHERE 1=0) al ON 1=0") + @"
                           GROUP BY gt.RootID
                      ) stat ON stat.RootID = root.IDGroup
                     WHERE root.ParentIDGroup IS NULL
                     ORDER BY ViewCount DESC, ProjectCount DESC";

                var rows = hasLog
                    ? db.Database.SqlQuery<GroupStatRow>(sql, from).ToList()
                    : db.Database.SqlQuery<GroupStatRow>(sql).ToList();

                return Json(rows.Select(r => new {
                    id = r.Id,
                    name = r.Name,
                    projectCount = r.ProjectCount,
                    viewCount = r.ViewCount,
                    uniqueUsers = r.UniqueUsers
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        /// <summary>
        /// Content publish > 30 ngay nhung 0 view tu khi tracking bat dau. Sunset candidates.
        /// Khong show khi tracking < grace period.
        /// </summary>
        public JsonResult UnusedContent(int n = 15)
        {
            if (!IsAuthorized()) return JsonForbidden();
            try
            {
                if (n < 1 || n > 50) n = 15;
                if (!TableExists("View360_AccessLog"))
                    return Json(new { notReady = true,
                        message = "Bảng tracking chưa tồn tại.",
                        rows = new object[0] }, JsonRequestBehavior.AllowGet);

                var trackingStart = db.Database.SqlQuery<DateTime?>(
                    "SELECT MIN(AccessAt) FROM dbo.View360_AccessLog").FirstOrDefault();
                const int GraceDays = 30;
                if (!trackingStart.HasValue
                    || (DateTime.Now - trackingStart.Value).TotalDays < GraceDays)
                {
                    return Json(new {
                        notReady = true,
                        message = "Tracking mới chạy. Danh sách 'content chưa ai xem' sẽ có ý nghĩa sau "
                                  + GraceDays + " ngày tracking liên tục.",
                        rows = new object[0]
                    }, JsonRequestBehavior.AllowGet);
                }

                // Project chua xem
                var projects = db.Database.SqlQuery<UnusedRow>(
                    @"SELECT TOP (@p0)
                             1 AS Type, p.ID AS ContentID, p.Title, p.Date AS PublishedAt,
                             DATEDIFF(DAY, p.Date, GETDATE()) AS DaysOld
                        FROM dbo.Projects p
                       WHERE p.Date < DATEADD(DAY, -30, GETDATE())
                         AND NOT EXISTS (
                             SELECT 1 FROM dbo.View360_AccessLog al
                              WHERE al.ContentType = 1 AND al.ContentID = p.ID)
                       ORDER BY p.Date ASC", n).ToList();

                var virtuals = db.Database.SqlQuery<UnusedRow>(
                    @"SELECT TOP (@p0)
                             2 AS Type, v.ID AS ContentID, v.Title, v.Date AS PublishedAt,
                             DATEDIFF(DAY, v.Date, GETDATE()) AS DaysOld
                        FROM dbo.Virtual v
                       WHERE v.Date < DATEADD(DAY, -30, GETDATE())
                         AND NOT EXISTS (
                             SELECT 1 FROM dbo.View360_AccessLog al
                              WHERE al.ContentType = 2 AND al.ContentID = v.ID)
                       ORDER BY v.Date ASC", n).ToList();

                var combined = projects.Concat(virtuals)
                    .OrderByDescending(r => r.DaysOld)
                    .Take(n)
                    .Select(r => new {
                        type = r.Type == 1 ? "Project" : "Virtual",
                        contentId = r.ContentID,
                        title = r.Title,
                        publishedAt = r.PublishedAt.ToString("yyyy-MM-dd"),
                        daysOld = r.DaysOld,
                        detailsUrl = r.Type == 1
                            ? Url.Action("Details", "ListProject", new { area = "View360", id = r.ContentID })
                            : Url.Action("Details", "ListVirtual", new { area = "View360", id = r.ContentID })
                    });

                return Json(new { notReady = false, rows = combined }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return JsonError(ex); }
        }

        // ----- helpers -------------------------------------------------------

        private bool IsAuthorized()
        {
            try
            {
                var check = dbP.A_CheckQuyen(IDQuyenHT, PermissionKey, A_Constants.VIEW_ALL).First();
                return check != 0;
            }
            catch { return false; }
        }

        private JsonResult JsonForbidden()
        {
            Response.StatusCode = 403;
            return Json(new { error = "forbidden" }, JsonRequestBehavior.AllowGet);
        }

        private JsonResult JsonError(Exception ex)
        {
            Response.StatusCode = 200; // ket qua loi nhung con dang JSON, JS render duoc.
            return Json(new
            {
                error = true,
                message = ex.Message,
                type = ex.GetType().Name
            }, JsonRequestBehavior.AllowGet);
        }

        private bool TableExists(string tableName)
        {
            try { return ScalarInt(
                @"SELECT CASE WHEN EXISTS (
                      SELECT 1 FROM sys.tables WHERE name = @p0 AND schema_id = SCHEMA_ID('dbo')
                  ) THEN 1 ELSE 0 END",
                new SqlParameter("@p0", tableName)) == 1; }
            catch { return false; }
        }

        private static DateTime ResolveFrom(string period)
        {
            var now = DateTime.Now;
            switch ((period ?? "30d").ToLowerInvariant())
            {
                case "7d":  return now.Date.AddDays(-6);
                case "90d": return now.Date.AddDays(-89);
                case "year": return new DateTime(now.Year, 1, 1);
                case "30d":
                default:    return now.Date.AddDays(-29);
            }
        }

        private int ScalarInt(string sql, params SqlParameter[] ps)
        {
            // SqlQuery<int> tra ve sequence - dung First() de unwrap.
            var args = ps.Select(p => (object)p).ToArray();
            return db.Database.SqlQuery<int>(sql, args).FirstOrDefault();
        }

        // ----- DTOs ----------------------------------------------------------

        public class TrendRow
        {
            public DateTime Day { get; set; }
            public byte ContentType { get; set; }
            public int Views { get; set; }
        }

        public class LogRow
        {
            public DateTime AccessAt { get; set; }
            public int NhanVienID { get; set; }
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string TenPhongBan { get; set; }
            public byte ContentType { get; set; }
            public int ContentID { get; set; }
            public string Title { get; set; }
        }

        public class UserAggRow
        {
            public int NhanVienID { get; set; }
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string TenPhongBan { get; set; }
            public int Views { get; set; }
            public int UniqueContent { get; set; }
            public DateTime LastAccess { get; set; }
        }

        public class CountsRow
        {
            public int TimeCount { get; set; }
            public int UserCount { get; set; }
            public int ContentCount { get; set; }
        }

        public class ContentAggRow
        {
            public byte ContentType { get; set; }
            public int ContentID { get; set; }
            public string Title { get; set; }
            public int Views { get; set; }
            public int UniqueUsers { get; set; }
            public DateTime FirstAccess { get; set; }
            public DateTime LastAccess { get; set; }
        }

        public class DeptUsageRow
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public int Granted { get; set; }
            public int Active { get; set; }
            public int Views { get; set; }
        }

        public class DeptRow
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        public class HourRow
        {
            public int H { get; set; }
            public int C { get; set; }
        }

        public class StaleRow
        {
            public int NhanVienID { get; set; }
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string TenPhongBan { get; set; }
            public DateTime FirstGrant { get; set; }
            public int DaysSinceGrant { get; set; }
        }

        public class GroupStatRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ProjectCount { get; set; }
            public int ViewCount { get; set; }
            public int UniqueUsers { get; set; }
        }

        public class UnusedRow
        {
            public byte Type { get; set; }
            public int ContentID { get; set; }
            public string Title { get; set; }
            public DateTime PublishedAt { get; set; }
            public int DaysOld { get; set; }
        }

        // Dispose EF context (MVC khong tu dispose field context -> giai phong connection pool ngay).
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null) db.Dispose();
                if (dbP != null) dbP.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
