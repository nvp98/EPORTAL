using EPORTAL.Common;
using EPORTAL.Models;
using EPORTAL.ModelsView360;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    /// <summary>
    /// Admin permission manager - 1 trang quan ly quyen cho Project/Virtual/Video.
    /// Thay the workflow phan tan (Excel upload + per-content Authorization modal).
    /// 3 tab: Cap quyen bulk, Theo user (drill), Theo phong ban.
    /// Re-use permission key "Projects" (admin context).
    /// </summary>
    public class PermissionController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = MyAuthentication.IDQuyenHT;
        const string PermKey = "Projects";

        // ContentType: 1=Project, 2=Virtual, 3=Video
        const byte TYPE_PROJECT = 1;
        const byte TYPE_VIRTUAL = 2;
        const byte TYPE_VIDEO   = 3;

        // GET: View360/Permission
        public ActionResult Index()
        {
            if (!HasPerm(A_Constants.VIEW_ALL))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            ViewBag.CanGrant  = HasPerm(A_Constants.ADD);
            ViewBag.CanRevoke = HasPerm(A_Constants.DELETE);
            return View();
        }

        // GET: View360/Permission/Audit
        public ActionResult Audit()
        {
            if (!HasPerm(A_Constants.VIEW_ALL))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            ViewBag.CanRevoke = HasPerm(A_Constants.DELETE);
            return View();
        }

        // ============= List grants (table main page) =============

        /// <summary>
        /// Aggregate grant theo (NhanVien, Type, GroupId). 1 row = 1 user co quyen 1 nhom.
        /// Vd user A co 54 quyen Project trong nhom "Du an Can 4" -> 1 row "Can 4 (54 du an)".
        /// </summary>
        public JsonResult ListGrants(string search, int? phongBanId, int? type,
                                       int? rootGroupId, int page = 1, int pageSize = 50)
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 10 || pageSize > 200) pageSize = 50;

                var paramValues = new Dictionary<string, object>();
                var where = " WHERE 1=1";
                if (phongBanId.HasValue)
                {
                    where += " AND n.IDPhongBan = @phongBanId";
                    paramValues["@phongBanId"] = phongBanId.Value;
                }
                if (type.HasValue && (type.Value == 1 || type.Value == 2 || type.Value == 3))
                {
                    where += " AND g.Type = @type";
                    paramValues["@type"] = type.Value;
                }
                // Filter theo parent group (chỉ áp dụng Project): include rootGroupId + tất cả descendants
                if (rootGroupId.HasValue && rootGroupId.Value > 0)
                {
                    var descendants = ExpandGroupDescendants(rootGroupId.Value);
                    if (descendants.Count > 0)
                    {
                        // SQL IN clause cần list literal - safe vì chỉ IDs từ DB
                        var idList = string.Join(",", descendants);
                        where += " AND (g.Type = 1 AND g.GroupId IN (" + idList + "))";
                    }
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    where += @" AND (n.HoTen LIKE @search OR n.MaNV LIKE @search OR g.GroupName LIKE @search)";
                    paramValues["@search"] = "%" + search.Trim() + "%";
                }

                // Hybrid: UNION
                //   (1) Group-grants moi tu AuthorizationUSER_Group
                //   (2) File-grants legacy aggregate (CHI khi user khong co group-grant tren cung (type, group))
                // GrantType: 1 = group-grant (auto-inherit), 0 = file-grants only (no inherit)
                var unionSql = @"
                    -- (1) GROUP-GRANTS
                    SELECT 1 AS Type, ag.NhanVienID, ag.IDGroup AS GroupId,
                           pg.GroupName AS GroupName, ag.Createdate,
                           1 AS GrantType, ag.[Recursive] AS IsRecursive
                      FROM dbo.AuthorizationUSER_Group ag
                      LEFT JOIN dbo.ProjectsGroup pg ON ag.IDGroup = pg.IDGroup
                     WHERE ag.ContentType = 1
                    UNION ALL
                    SELECT 2, ag.NhanVienID, ag.IDGroup,
                           vg.GroupName, ag.Createdate, 1, 0
                      FROM dbo.AuthorizationUSER_Group ag
                      LEFT JOIN dbo.VirtualGroup vg ON ag.IDGroup = vg.IDGroup
                     WHERE ag.ContentType = 2
                    UNION ALL
                    SELECT 3, ag.NhanVienID, ag.IDGroup,
                           a.TenAlbum, ag.Createdate, 1, 0
                      FROM dbo.AuthorizationUSER_Group ag
                      LEFT JOIN dbo.Album a ON ag.IDGroup = a.IDAlbum
                     WHERE ag.ContentType = 3
                    UNION ALL
                    -- (2) FILE-GRANTS legacy: chỉ show nếu KHÔNG có group-grant tương ứng
                    SELECT 1 AS Type, au.NhanVienID, p.IDGroup AS GroupId,
                           pg.GroupName AS GroupName, au.Createdate, 0, 0
                      FROM dbo.AuthorizationUSER au
                      JOIN dbo.Projects p ON au.ProjectID = p.ID
                      LEFT JOIN dbo.ProjectsGroup pg ON p.IDGroup = pg.IDGroup
                     WHERE au.NhanVienID IS NOT NULL AND p.IDGroup IS NOT NULL
                       AND NOT EXISTS (
                           SELECT 1 FROM dbo.AuthorizationUSER_Group g
                           WHERE g.NhanVienID = au.NhanVienID AND g.ContentType = 1 AND g.IDGroup = p.IDGroup
                       )
                    UNION ALL
                    SELECT 2, av.NhanVienID, v.IDGroup,
                           vg.GroupName, av.Createdate, 0, 0
                      FROM dbo.AuthorizationVitual av
                      JOIN dbo.Virtual v ON av.VirtualID = v.ID
                      LEFT JOIN dbo.VirtualGroup vg ON v.IDGroup = vg.IDGroup
                     WHERE av.NhanVienID IS NOT NULL AND v.IDGroup IS NOT NULL
                       AND NOT EXISTS (
                           SELECT 1 FROM dbo.AuthorizationUSER_Group g
                           WHERE g.NhanVienID = av.NhanVienID AND g.ContentType = 2 AND g.IDGroup = v.IDGroup
                       )
                    UNION ALL
                    SELECT 3, avi.NhanVienID, vd.AlbumID,
                           a.TenAlbum, avi.Createdate, 0, 0
                      FROM dbo.AuthorizationVideo avi
                      JOIN dbo.Video vd ON avi.VideoID = vd.IDVideo
                      LEFT JOIN dbo.Album a ON vd.AlbumID = a.IDAlbum
                     WHERE avi.NhanVienID IS NOT NULL AND vd.AlbumID IS NOT NULL
                       AND NOT EXISTS (
                           SELECT 1 FROM dbo.AuthorizationUSER_Group g
                           WHERE g.NhanVienID = avi.NhanVienID AND g.ContentType = 3 AND g.IDGroup = vd.AlbumID
                       )";

                var fromJoin = "FROM (" + unionSql + @") g
                    JOIN dbo.NhanVien n ON g.NhanVienID = n.ID
                    LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan";

                // Total count = number of DISTINCT (User, Type, Group) tuples
                var totalSql = "SELECT COUNT(*) FROM (SELECT g.NhanVienID, g.Type, g.GroupId " +
                               fromJoin + where + @" GROUP BY g.NhanVienID, g.Type, g.GroupId) x";
                var total = db.Database.SqlQuery<int>(totalSql, MakeParams(paramValues)).First();

                var offset = (page - 1) * pageSize;
                paramValues["@offset"] = offset;
                paramValues["@pageSize"] = pageSize;
                var pagedSql = @"
                    SELECT g.Type, g.NhanVienID, g.GroupId,
                           MAX(g.GroupName) AS GroupName,
                           COUNT(*) AS ContentCount,
                           MAX(g.Createdate) AS LastCreatedate,
                           MAX(n.MaNV) AS MaNV,
                           MAX(n.HoTen) AS HoTen,
                           MAX(pb.TenPhongBan) AS TenPhongBan,
                           MAX(CAST(g.GrantType AS INT)) AS GrantType,
                           MAX(CAST(g.IsRecursive AS INT)) AS IsRecursive "
                    + fromJoin + where +
                    @" GROUP BY g.Type, g.NhanVienID, g.GroupId
                       ORDER BY MAX(g.Createdate) DESC, g.NhanVienID, g.Type, g.GroupId
                       OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

                var sqlRows = db.Database.SqlQuery<GrantGroupRow>(pagedSql, MakeParams(paramValues)).ToList();

                // Resolve full path cho Project groups (hierarchy ProjectsGroup)
                // Vd: groupId 100 (Can 4) -> parent 50 (HPDQ 2) -> "HPDQ 2 / Can 4"
                var projectPathMap = new Dictionary<int, string>();
                if (sqlRows.Any(r => r.Type == 1))
                {
                    var flat = ProjectsGroupHierarchy.FlattenPreOrder(ProjectsGroupHierarchy.GetAllTree());
                    var byId = flat.ToDictionary(n => n.IDGroup);
                    foreach (var r in sqlRows.Where(r => r.Type == 1).Select(r => r.GroupId).Distinct())
                    {
                        if (!byId.ContainsKey(r)) continue;
                        var parts = new List<string>();
                        var node = byId[r];
                        // Walk up tree (max 10 levels safety)
                        for (int lv = 0; lv < 10 && node != null; lv++)
                        {
                            parts.Insert(0, node.GroupName);
                            node = node.ParentIDGroup.HasValue && byId.ContainsKey(node.ParentIDGroup.Value)
                                ? byId[node.ParentIDGroup.Value] : null;
                        }
                        projectPathMap[r] = string.Join(" / ", parts);
                    }
                }

                var rows = sqlRows.Select(r => {
                    string displayName;
                    if (r.Type == 1 && projectPathMap.ContainsKey(r.GroupId))
                        displayName = projectPathMap[r.GroupId];
                    else
                        displayName = r.GroupName ?? "(không tên nhóm)";
                    return new {
                        type = r.Type,
                        typeName = r.Type == 1 ? "Project" : (r.Type == 2 ? "Virtual" : "Video"),
                        groupId = r.GroupId,
                        groupName = displayName,
                        groupNameLeaf = r.GroupName ?? "(không tên nhóm)",
                        contentCount = r.ContentCount,
                        nhanVienId = r.NhanVienID,
                        maNV = r.MaNV,
                        hoTen = r.HoTen,
                        phongBan = r.TenPhongBan,
                        lastCreatedate = r.LastCreatedate.HasValue ? r.LastCreatedate.Value.ToString("yyyy-MM-dd HH:mm") : "",
                        grantType = r.GrantType,                // 1 = group-grant, 0 = file-grants only
                        isRecursive = r.IsRecursive == 1
                    };
                });

                return Json(new {
                    rows, total, page, pageSize,
                    totalPages = (int)Math.Ceiling((double)total / pageSize)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ============= Master data endpoints =============

        public JsonResult PhongBans()
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                var rows = db.PhongBans
                    .OrderBy(pb => pb.TenPhongBan)
                    .Select(pb => new { id = pb.IDPhongBan, name = pb.TenPhongBan })
                    .ToList();
                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        public JsonResult Groups(int type)
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                if (type == TYPE_PROJECT)
                {
                    var tree = ProjectsGroupHierarchy.GetAllTree();
                    var flat = ProjectsGroupHierarchy.FlattenPreOrder(tree);
                    var rows = flat
                        .Select(n => new {
                            id = n.IDGroup, name = n.GroupName,
                            parentId = n.ParentIDGroup, depth = n.Depth
                        })
                        .ToList();
                    return Json(rows, JsonRequestBehavior.AllowGet);
                }
                if (type == TYPE_VIRTUAL)
                {
                    var rows = db.VirtualGroups
                        .OrderBy(g => g.GroupName)
                        .Select(g => new { id = g.IDGroup, name = g.GroupName, parentId = (int?)null, depth = 0 })
                        .ToList();
                    return Json(rows, JsonRequestBehavior.AllowGet);
                }
                // Video grouped by Album
                if (type == TYPE_VIDEO)
                {
                    var rows = db.Albums
                        .OrderBy(a => a.TenAlbum)
                        .Select(a => new { id = a.IDAlbum, name = a.TenAlbum, parentId = (int?)null, depth = 0 })
                        .ToList();
                    return Json(rows, JsonRequestBehavior.AllowGet);
                }
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        /// <summary>Search active employees voi optional filter PhongBan + text.</summary>
        public JsonResult Users(string search, int? phongBanId, int limit = 500)
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                if (limit < 10 || limit > 2000) limit = 500;
                var q = db.NhanViens
                    .Where(n => n.IDTinhTrangLV == 1);
                if (phongBanId.HasValue)
                    q = q.Where(n => n.IDPhongBan == phongBanId.Value);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    q = q.Where(n => n.HoTen.Contains(s) || n.MaNV.Contains(s));
                }
                // Dedupe theo MaNV - data co the co duplicate (cung MaNV, khac NhanVienID).
                // Giu row co ID nho nhat (canonical = oldest).
                var rows = q.OrderBy(n => n.HoTen)
                    .Take(limit)
                    .Select(n => new {
                        id = n.ID,
                        maNV = n.MaNV,
                        hoTen = n.HoTen,
                        phongBanId = n.IDPhongBan,
                        phongBan = n.PhongBan != null ? n.PhongBan.TenPhongBan : null
                    })
                    .ToList()
                    .GroupBy(r => string.IsNullOrEmpty(r.maNV) ? ("__id_" + r.id) : r.maNV.Trim().ToUpper())
                    .Select(g => g.OrderBy(r => r.id).First())
                    .ToList();
                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        /// <summary>Search content theo type. Group filter optional. recursive=true se include descendants (cho Project).</summary>
        public JsonResult Contents(int type, int? groupId, string search, bool recursive = false, int limit = 1000)
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                if (limit < 10 || limit > 5000) limit = 1000;
                var s = (search ?? "").Trim();

                if (type == TYPE_PROJECT)
                {
                    HashSet<int> allowedGroups = null;
                    if (groupId.HasValue)
                    {
                        allowedGroups = recursive
                            ? ExpandGroupDescendants(groupId.Value)
                            : new HashSet<int> { groupId.Value };
                    }
                    var q = db.Projects.AsQueryable();
                    if (allowedGroups != null)
                        q = q.Where(p => p.IDGroup.HasValue && allowedGroups.Contains(p.IDGroup.Value));
                    if (!string.IsNullOrEmpty(s))
                        q = q.Where(p => p.Title.Contains(s));
                    var rows = q.OrderByDescending(p => p.Date)
                        .Take(limit)
                        .Select(p => new {
                            type = (int)TYPE_PROJECT, typeName = "Project",
                            id = p.ID, title = p.Title, groupId = p.IDGroup
                        })
                        .ToList();
                    return Json(rows, JsonRequestBehavior.AllowGet);
                }
                if (type == TYPE_VIRTUAL)
                {
                    var q = db.Virtuals.AsQueryable();
                    if (groupId.HasValue) q = q.Where(v => v.IDGroup == groupId.Value);
                    if (!string.IsNullOrEmpty(s)) q = q.Where(v => v.Title.Contains(s));
                    var rows = q.OrderByDescending(v => v.Date)
                        .Take(limit)
                        .Select(v => new {
                            type = (int)TYPE_VIRTUAL, typeName = "Virtual",
                            id = v.ID, title = v.Title, groupId = v.IDGroup
                        })
                        .ToList();
                    return Json(rows, JsonRequestBehavior.AllowGet);
                }
                if (type == TYPE_VIDEO)
                {
                    var q = db.Videos.AsQueryable();
                    if (groupId.HasValue) q = q.Where(vd => vd.AlbumID == groupId.Value);
                    if (!string.IsNullOrEmpty(s)) q = q.Where(vd => vd.Title.Contains(s));
                    var rows = q.OrderByDescending(vd => vd.Date)
                        .Take(limit)
                        .Select(vd => new {
                            type = (int)TYPE_VIDEO, typeName = "Video",
                            id = vd.IDVideo, title = vd.Title, groupId = vd.AlbumID
                        })
                        .ToList();
                    return Json(rows, JsonRequestBehavior.AllowGet);
                }
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ============= Bulk Grant =============

        /// <summary>
        /// Dry-run: tinh truoc nhung gi se xay ra neu commit Grant/Revoke.
        /// Khong write DB. Tra:
        ///   - existing: so cap (user, content) DA TON TAI trong DB
        ///   - notExisting: so cap CHUA co
        ///   - warnings: cac canh bao (user inactive, content deleted, ...)
        /// Frontend dung de hien preview truoc khi user click "Cap N quyen mới".
        /// </summary>
        [HttpPost]
        [SameOriginOnly]
        public JsonResult PreviewGrant()
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                var req = ReadJson<GrantRequest>();
                if (req == null || req.UserIds == null || req.UserIds.Length == 0)
                {
                    return Json(new { existing = 0, notExisting = 0, total = 0, warnings = new object[0] });
                }
                var userIds = req.UserIds.Distinct().ToList();

                // ===== Preview phải đếm CÙNG ĐƠN VỊ với Grant (xem action Grant) =====
                //   - Groups  -> Grant chèn 1 row/(user, nhóm) vào AuthorizationUSER_Group (auto-inherit).
                //               => đếm ở mức NHÓM, KHÔNG expand sang content.
                //   - Items   -> Grant chèn 1 row/(user, content) vào AuthorizationUSER/Vitual/Video.
                //               => đếm ở mức CONTENT (file-grant lẻ).
                // Nếu Preview expand groups -> content nhưng Grant lại ghi theo nhóm thì 2 con số
                // sẽ không bao giờ khớp (đây chính là bug đã xảy ra).

                // ----- A. GROUP-GRANT units (AuthorizationUSER_Group) -----
                var groups = (req.Groups ?? new GroupRef[0])
                    .Where(g => g.GroupId > 0)
                    .GroupBy(g => g.Type + "-" + g.GroupId)
                    .Select(g => g.First())
                    .ToList();
                int groupCount = groups.Count;
                int groupProjectCount = groups.Count(g => g.Type == TYPE_PROJECT);
                int groupVirtualCount = groups.Count(g => g.Type == TYPE_VIRTUAL);
                int groupVideoCount   = groups.Count(g => g.Type == TYPE_VIDEO);

                int groupExisting = 0;
                if (groupCount > 0 && userIds.Count > 0)
                {
                    var projG = groups.Where(g => g.Type == TYPE_PROJECT).Select(g => g.GroupId).ToList();
                    var virtG = groups.Where(g => g.Type == TYPE_VIRTUAL).Select(g => g.GroupId).ToList();
                    var vidG  = groups.Where(g => g.Type == TYPE_VIDEO ).Select(g => g.GroupId).ToList();
                    var conds = new List<string>();
                    // IN-list từ int (UserIds/GroupId) đã validate -> an toàn để inline.
                    if (projG.Count > 0) conds.Add("(g.ContentType=1 AND g.IDGroup IN (" + string.Join(",", projG) + "))");
                    if (virtG.Count > 0) conds.Add("(g.ContentType=2 AND g.IDGroup IN (" + string.Join(",", virtG) + "))");
                    if (vidG.Count  > 0) conds.Add("(g.ContentType=3 AND g.IDGroup IN (" + string.Join(",", vidG)  + "))");
                    var groupExistSql = "SELECT COUNT(*) FROM dbo.AuthorizationUSER_Group g WHERE g.NhanVienID IN ("
                        + string.Join(",", userIds) + ") AND (" + string.Join(" OR ", conds) + ")";
                    groupExisting = db.Database.SqlQuery<int>(groupExistSql).First();
                }

                // ----- B. FILE-GRANT units (Items, KHÔNG bao gồm content expand từ groups) -----
                var items = (req.Items ?? new ContentItem[0])
                    .GroupBy(i => i.Type + "-" + i.Id)
                    .Select(g => g.First())
                    .ToList();
                var projIds = items.Where(i => i.Type == TYPE_PROJECT).Select(i => i.Id).ToList();
                var virtIds = items.Where(i => i.Type == TYPE_VIRTUAL).Select(i => i.Id).ToList();
                var vidIds  = items.Where(i => i.Type == TYPE_VIDEO ).Select(i => i.Id).ToList();

                // DISTINCT count - tránh case AuthorizationUSER/Vitual/Video có row trùng (NhanVienID, ContentID).
                int fileExisting = 0;
                if (projIds.Count > 0)
                {
                    fileExisting += db.AuthorizationUSERs
                        .Where(au => au.NhanVienID.HasValue && au.ProjectID.HasValue
                            && userIds.Contains(au.NhanVienID.Value)
                            && projIds.Contains(au.ProjectID.Value))
                        .Select(au => new { au.NhanVienID, au.ProjectID })
                        .Distinct()
                        .Count();
                }
                if (virtIds.Count > 0)
                {
                    fileExisting += db.AuthorizationVituals
                        .Where(av => av.NhanVienID.HasValue && av.VirtualID.HasValue
                            && userIds.Contains(av.NhanVienID.Value)
                            && virtIds.Contains(av.VirtualID.Value))
                        .Select(av => new { av.NhanVienID, av.VirtualID })
                        .Distinct()
                        .Count();
                }
                if (vidIds.Count > 0)
                {
                    fileExisting += db.AuthorizationVideos
                        .Where(av => av.NhanVienID.HasValue && av.VideoID.HasValue
                            && userIds.Contains(av.NhanVienID.Value)
                            && vidIds.Contains(av.VideoID.Value))
                        .Select(av => new { av.NhanVienID, av.VideoID })
                        .Distinct()
                        .Count();
                }

                int itemCount = projIds.Count + virtIds.Count + vidIds.Count;
                int unitCount = groupCount + itemCount;
                if (unitCount == 0)
                    return Json(new { existing = 0, notExisting = 0, total = 0, warnings = new object[0] });

                int total = userIds.Count * unitCount;
                int existing = groupExisting + fileExisting;
                // Defensive: nếu DB có duplicate sẽ làm existing > total → clamp.
                if (existing > total) existing = total;
                int notExisting = total - existing;

                // Info-only: số content thực tế các NHÓM sẽ auto-inherit (để hiển thị, KHÔNG dùng để đếm).
                int groupContentCount = groupCount > 0 ? ExpandGroupsToItems(groups.ToArray()).Count : 0;

                // Warnings
                var warnings = new List<object>();
                int inactiveUsers = db.NhanViens
                    .Where(n => userIds.Contains(n.ID) && n.IDTinhTrangLV != 1)
                    .Count();
                if (inactiveUsers > 0)
                    warnings.Add(new { type = "inactive_user", count = inactiveUsers,
                        message = inactiveUsers + " người dùng không đang làm việc - vẫn cấp nếu xác nhận." });

                int noDeptUsers = db.NhanViens
                    .Where(n => userIds.Contains(n.ID) && n.IDPhongBan == null)
                    .Count();
                if (noDeptUsers > 0)
                    warnings.Add(new { type = "no_dept_user", count = noDeptUsers,
                        message = noDeptUsers + " người dùng chưa gán phòng ban." });

                return Json(new {
                    // Tổng quyền sẽ tác động = userCount × (số nhóm + số content lẻ)
                    total,
                    // Đã có sẵn trong DB (group-grant + file-grant) - sẽ skip
                    existing,
                    // Cần tạo mới
                    notExisting,
                    // Breakdown để hiển thị chi tiết
                    userCount = userIds.Count,
                    unitCount,
                    // Group-grant breakdown (đơn vị = nhóm)
                    groupCount,
                    groupProjectCount,
                    groupVirtualCount,
                    groupVideoCount,
                    groupContentCount,   // info-only: content được auto-inherit
                    // File-grant breakdown (đơn vị = content lẻ)
                    itemCount,
                    fileProjectCount = projIds.Count,
                    fileVirtualCount = virtIds.Count,
                    fileVideoCount   = vidIds.Count,
                    warnings
                });
            }
            catch (Exception ex) { return Err(ex); }
        }

        /// <summary>
        /// Tra ve danh sach content (Project/Virtual/Video) ma user nguon dang co quyen.
        /// Dung cho filter "Copy quyen tu user X" - pre-fill content selection.
        /// </summary>
        public JsonResult CopyFromUser(int userId)
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                var projects = db.AuthorizationUSERs
                    .Where(au => au.NhanVienID == userId && au.ProjectID.HasValue)
                    .Select(au => new { type = (int)TYPE_PROJECT, id = au.ProjectID.Value })
                    .ToList();
                var virtuals = db.AuthorizationVituals
                    .Where(av => av.NhanVienID == userId && av.VirtualID.HasValue)
                    .Select(av => new { type = (int)TYPE_VIRTUAL, id = av.VirtualID.Value })
                    .ToList();
                var videos = db.AuthorizationVideos
                    .Where(av => av.NhanVienID == userId && av.VideoID.HasValue)
                    .Select(av => new { type = (int)TYPE_VIDEO, id = av.VideoID.Value })
                    .ToList();

                // Resolve titles voi 1 query per type
                var pIds = projects.Select(x => x.id).ToList();
                var vIds = virtuals.Select(x => x.id).ToList();
                var vdIds = videos.Select(x => x.id).ToList();
                var pTitles = db.Projects.Where(p => pIds.Contains(p.ID))
                    .Select(p => new { p.ID, p.Title }).ToDictionary(x => x.ID, x => x.Title);
                var vTitles = db.Virtuals.Where(v => vIds.Contains(v.ID))
                    .Select(v => new { v.ID, v.Title }).ToDictionary(x => x.ID, x => x.Title);
                var vdTitles = db.Videos.Where(vd => vdIds.Contains(vd.IDVideo))
                    .Select(vd => new { vd.IDVideo, vd.Title }).ToDictionary(x => x.IDVideo, x => x.Title);

                var result = projects.Select(x => new { type = x.type, typeName = "Project",
                                                        id = x.id, title = pTitles.ContainsKey(x.id) ? pTitles[x.id] : "(đã xoá)" })
                    .Concat(virtuals.Select(x => new { type = x.type, typeName = "Virtual",
                                                        id = x.id, title = vTitles.ContainsKey(x.id) ? vTitles[x.id] : "(đã xoá)" }))
                    .Concat(videos.Select(x => new { type = x.type, typeName = "Video",
                                                        id = x.id, title = vdTitles.ContainsKey(x.id) ? vdTitles[x.id] : "(đã xoá)" }))
                    .ToList();
                return Json(new { total = result.Count, items = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        /// <summary>
        /// Resolve danh sach NhanVienID tu list MaNV (paste tu Excel).
        /// Tra ve matched + unmatched de admin biet MaNV nao khong tim thay.
        /// </summary>
        [HttpPost]
        [SameOriginOnly]
        public JsonResult ResolveMaNV()
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                var req = ReadJson<MaNVResolveRequest>();
                if (req == null || req.MaNVs == null || req.MaNVs.Length == 0)
                    return Json(new { matched = new object[0], unmatched = new string[0] });

                var inputs = req.MaNVs
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim().ToUpper())
                    .Distinct()
                    .ToList();
                var matched = db.NhanViens
                    .Where(n => n.IDTinhTrangLV == 1 && n.MaNV != null && inputs.Contains(n.MaNV.ToUpper()))
                    .Select(n => new {
                        id = n.ID,
                        maNV = n.MaNV,
                        hoTen = n.HoTen,
                        phongBan = n.PhongBan != null ? n.PhongBan.TenPhongBan : null
                    })
                    .ToList()
                    // Dedupe MaNV - keep first
                    .GroupBy(r => (r.maNV ?? "").ToUpper())
                    .Select(g => g.OrderBy(r => r.id).First())
                    .ToList();
                var matchedSet = new HashSet<string>(matched.Select(m => (m.maNV ?? "").ToUpper()));
                var unmatched = inputs.Where(s => !matchedSet.Contains(s)).ToList();

                return Json(new { matched, unmatched });
            }
            catch (Exception ex) { return Err(ex); }
        }

        /// <summary>
        /// Audit data: surface 4 problem areas cho trang Audit.
        ///   - Dept 0% coverage (phong khong co user nao co quyen)
        ///   - User inactive con quyen (NhanVien IDTinhTrangLV != 1 nhung con row trong Authorization*)
        ///   - Content orphan (publish > 30 ngay khong co ai duoc cap)
        ///   - Unused grants (user co quyen nhung chua mo - join voi View360_AccessLog)
        /// </summary>
        public JsonResult Anomalies()
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                var result = new Dictionary<string, object>();

                // 1. Dept 0% coverage
                try
                {
                    var sql1 = @"
                        SELECT pb.IDPhongBan AS Id, pb.TenPhongBan AS Name,
                               COUNT(n.ID) AS ActiveUsers
                          FROM dbo.PhongBan pb
                          LEFT JOIN dbo.NhanVien n ON n.IDPhongBan = pb.IDPhongBan AND n.IDTinhTrangLV = 1
                         WHERE NOT EXISTS (
                              SELECT 1 FROM (
                                  SELECT NhanVienID FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                                  UNION
                                  SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                                  UNION
                                  SELECT NhanVienID FROM dbo.AuthorizationVideo  WHERE NhanVienID IS NOT NULL
                              ) au
                              JOIN dbo.NhanVien n2 ON au.NhanVienID = n2.ID
                              WHERE n2.IDPhongBan = pb.IDPhongBan AND n2.IDTinhTrangLV = 1
                         )
                         GROUP BY pb.IDPhongBan, pb.TenPhongBan
                        HAVING COUNT(n.ID) > 0
                         ORDER BY COUNT(n.ID) DESC";
                    var deptZero = db.Database.SqlQuery<AnomalyDeptRow>(sql1).ToList()
                        .Select(r => new { id = r.Id, name = r.Name, activeUsers = r.ActiveUsers });
                    result["deptZeroCoverage"] = deptZero;
                }
                catch { result["deptZeroCoverage"] = new object[0]; }

                // 2. Inactive user con grant
                try
                {
                    var sql2 = @"
                        SELECT TOP 100 n.ID AS Id, n.MaNV, n.HoTen, pb.TenPhongBan,
                               ISNULL(gc.GrantCount, 0) AS GrantCount
                          FROM dbo.NhanVien n
                          LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan
                          LEFT JOIN (
                              SELECT NhanVienID, COUNT(*) AS GrantCount
                                FROM (
                                    SELECT NhanVienID FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                                    UNION ALL
                                    SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                                    UNION ALL
                                    SELECT NhanVienID FROM dbo.AuthorizationVideo  WHERE NhanVienID IS NOT NULL
                                ) au
                               GROUP BY NhanVienID
                          ) gc ON gc.NhanVienID = n.ID
                         WHERE n.IDTinhTrangLV <> 1 AND gc.GrantCount > 0
                         ORDER BY gc.GrantCount DESC";
                    var inactives = db.Database.SqlQuery<AnomalyUserRow>(sql2).ToList()
                        .Select(r => new {
                            id = r.Id, maNV = r.MaNV, hoTen = r.HoTen,
                            phongBan = r.TenPhongBan, grantCount = r.GrantCount
                        });
                    result["inactiveUsersWithGrant"] = inactives;
                }
                catch { result["inactiveUsersWithGrant"] = new object[0]; }

                // 3. Content orphan (>30d, 0 grant) - Project + Virtual
                try
                {
                    var orphan30d = DateTime.Now.AddDays(-30);
                    var pOrphan = db.Projects
                        .Where(p => p.Date < orphan30d
                            && !db.AuthorizationUSERs.Any(au => au.ProjectID == p.ID))
                        .OrderBy(p => p.Date)
                        .Take(20)
                        .Select(p => new {
                            type = (int)TYPE_PROJECT, typeName = "Project",
                            id = p.ID, title = p.Title, date = p.Date
                        })
                        .ToList()
                        .Select(p => new {
                            p.type, p.typeName, p.id, p.title,
                            date = p.date.HasValue ? p.date.Value.ToString("yyyy-MM-dd") : ""
                        });
                    var vOrphan = db.Virtuals
                        .Where(v => v.Date < orphan30d
                            && !db.AuthorizationVituals.Any(av => av.VirtualID == v.ID))
                        .OrderBy(v => v.Date)
                        .Take(20)
                        .Select(v => new {
                            type = (int)TYPE_VIRTUAL, typeName = "Virtual",
                            id = v.ID, title = v.Title, date = v.Date
                        })
                        .ToList()
                        .Select(v => new {
                            v.type, v.typeName, v.id, v.title,
                            date = v.date.HasValue ? v.date.Value.ToString("yyyy-MM-dd") : ""
                        });
                    result["orphanContent"] = pOrphan.Concat(vOrphan).Take(30).ToList();
                }
                catch { result["orphanContent"] = new object[0]; }

                // 4. Unused grants - user co grant nhung 0 row trong View360_AccessLog
                // Chi tinh sau grace period 30 ngay tracking
                try
                {
                    var hasLogTable = db.Database.SqlQuery<int>(
                        "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name='View360_AccessLog') THEN 1 ELSE 0 END")
                        .First() == 1;
                    if (hasLogTable)
                    {
                        var trackingStart = db.Database.SqlQuery<DateTime?>(
                            "SELECT MIN(AccessAt) FROM dbo.View360_AccessLog").FirstOrDefault();
                        if (trackingStart.HasValue && (DateTime.Now - trackingStart.Value).TotalDays >= 30)
                        {
                            var sql4 = @"
                                SELECT TOP 50 n.ID AS Id, n.MaNV, n.HoTen, pb.TenPhongBan,
                                       ISNULL(gc.GrantCount, 0) AS GrantCount
                                  FROM dbo.NhanVien n
                                  LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan
                                  JOIN (
                                      SELECT NhanVienID, COUNT(*) AS GrantCount
                                        FROM (
                                            SELECT NhanVienID FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                                            UNION ALL
                                            SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                                            UNION ALL
                                            SELECT NhanVienID FROM dbo.AuthorizationVideo  WHERE NhanVienID IS NOT NULL
                                        ) au
                                       GROUP BY NhanVienID
                                  ) gc ON gc.NhanVienID = n.ID
                                 WHERE n.IDTinhTrangLV = 1
                                   AND NOT EXISTS (SELECT 1 FROM dbo.View360_AccessLog al WHERE al.NhanVienID = n.ID)
                                 ORDER BY gc.GrantCount DESC";
                            var unused = db.Database.SqlQuery<AnomalyUserRow>(sql4).ToList()
                                .Select(r => new {
                                    id = r.Id, maNV = r.MaNV, hoTen = r.HoTen,
                                    phongBan = r.TenPhongBan, grantCount = r.GrantCount
                                });
                            result["unusedGrants"] = unused;
                            result["unusedGrantsReady"] = true;
                        }
                        else
                        {
                            result["unusedGrants"] = new object[0];
                            result["unusedGrantsReady"] = false;
                            result["unusedGrantsMessage"] = "Theo dõi chưa đủ 30 ngày để có dữ liệu chính xác.";
                        }
                    }
                    else
                    {
                        result["unusedGrants"] = new object[0];
                        result["unusedGrantsReady"] = false;
                        result["unusedGrantsMessage"] = "Chưa bật theo dõi truy cập (bảng dữ liệu chưa tồn tại).";
                    }
                }
                catch (Exception ex)
                {
                    result["unusedGrants"] = new object[0];
                    result["unusedGrantsReady"] = false;
                    result["unusedGrantsMessage"] = "Lỗi: " + ex.Message;
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        [HttpPost]
        [SameOriginOnly]
        public JsonResult Grant()
        {
            if (!HasPerm(A_Constants.ADD)) return Forbid();
            try
            {
                var req = ReadJson<GrantRequest>();
                if (req == null || req.UserIds == null || req.UserIds.Length == 0)
                    return Json(new { created = 0, skipped = 0, error = "Thiếu người dùng" });

                int created = 0, skipped = 0;
                var now = DateTime.Now;
                var userIds = req.UserIds.Distinct().ToList();

                // ===== A. GROUP-GRANT: insert AuthorizationUSER_Group =====
                // (Khong expand sang per-content nua. Schema moi auto-inherit qua SP.)
                if (req.Groups != null && req.Groups.Length > 0)
                {
                    foreach (var uid in userIds)
                    {
                        foreach (var gref in req.Groups)
                        {
                            if (gref.GroupId <= 0) continue;
                            // Check existed (UNIQUE constraint)
                            var existCnt = db.Database.SqlQuery<int>(
                                @"SELECT COUNT(*) FROM dbo.AuthorizationUSER_Group
                                  WHERE NhanVienID = @p0 AND ContentType = @p1 AND IDGroup = @p2",
                                uid, (byte)gref.Type, gref.GroupId).First();
                            if (existCnt > 0) { skipped++; continue; }
                            db.Database.ExecuteSqlCommand(
                                @"INSERT INTO dbo.AuthorizationUSER_Group
                                  (NhanVienID, ContentType, IDGroup, [Recursive], Createdate)
                                  VALUES (@p0, @p1, @p2, @p3, @p4)",
                                uid, (byte)gref.Type, gref.GroupId, gref.Recursive ? 1 : 0, now);
                            created++;
                        }
                    }
                }

                // ===== B. FILE-GRANT: insert legacy tables (Excel + per-content) =====
                var items = req.Items ?? new ContentItem[0];
                var projIds = items.Where(i => i.Type == TYPE_PROJECT).Select(i => i.Id).Distinct().ToList();
                var virtIds = items.Where(i => i.Type == TYPE_VIRTUAL).Select(i => i.Id).Distinct().ToList();
                var vidIds  = items.Where(i => i.Type == TYPE_VIDEO ).Select(i => i.Id).Distinct().ToList();

                // === Project grants ===
                if (projIds.Count > 0)
                {
                    var existing = new HashSet<string>(
                        db.AuthorizationUSERs
                            .Where(au => au.NhanVienID.HasValue && au.ProjectID.HasValue
                                && userIds.Contains(au.NhanVienID.Value)
                                && projIds.Contains(au.ProjectID.Value))
                            .Select(au => au.NhanVienID + "-" + au.ProjectID)
                            .ToList());
                    foreach (var uid in userIds)
                        foreach (var pid in projIds)
                        {
                            var k = uid + "-" + pid;
                            if (existing.Contains(k)) { skipped++; continue; }
                            db.AuthorizationUSERs.Add(new AuthorizationUSER {
                                NhanVienID = uid, ProjectID = pid, Createdate = now
                            });
                            created++;
                        }
                }

                // === Virtual grants ===
                if (virtIds.Count > 0)
                {
                    var existing = new HashSet<string>(
                        db.AuthorizationVituals
                            .Where(av => av.NhanVienID.HasValue && av.VirtualID.HasValue
                                && userIds.Contains(av.NhanVienID.Value)
                                && virtIds.Contains(av.VirtualID.Value))
                            .Select(av => av.NhanVienID + "-" + av.VirtualID)
                            .ToList());
                    foreach (var uid in userIds)
                        foreach (var vid in virtIds)
                        {
                            var k = uid + "-" + vid;
                            if (existing.Contains(k)) { skipped++; continue; }
                            db.AuthorizationVituals.Add(new AuthorizationVitual {
                                NhanVienID = uid, VirtualID = vid, Createdate = now
                            });
                            created++;
                        }
                }

                // === Video grants ===
                if (vidIds.Count > 0)
                {
                    var existing = new HashSet<string>(
                        db.AuthorizationVideos
                            .Where(av => av.NhanVienID.HasValue && av.VideoID.HasValue
                                && userIds.Contains(av.NhanVienID.Value)
                                && vidIds.Contains(av.VideoID.Value))
                            .Select(av => av.NhanVienID + "-" + av.VideoID)
                            .ToList());
                    foreach (var uid in userIds)
                        foreach (var vid in vidIds)
                        {
                            var k = uid + "-" + vid;
                            if (existing.Contains(k)) { skipped++; continue; }
                            db.AuthorizationVideos.Add(new AuthorizationVideo {
                                NhanVienID = uid, VideoID = vid, Createdate = now
                            });
                            created++;
                        }
                }

                if (created > 0) db.SaveChanges();
                return Json(new { created, skipped });
            }
            catch (Exception ex) { return Err(ex); }
        }

        [HttpPost]
        [SameOriginOnly]
        public JsonResult Revoke()
        {
            if (!HasPerm(A_Constants.DELETE)) return Forbid();
            try
            {
                var req = ReadJson<RevokeRequest>();
                if (req == null) return Json(new { deleted = 0 });

                int deletedTotal = 0;

                // ===== A. Revoke group-grants HOẶC file-grants in group =====
                if (req.Groups != null)
                {
                    foreach (var rg in req.Groups)
                    {
                        if (rg.ScopeFileOnly)
                        {
                            // Xoá legacy file-grants nằm trong group này
                            if (rg.Type == TYPE_PROJECT)
                            {
                                var pIds = db.Projects.Where(p => p.IDGroup == rg.GroupId)
                                    .Select(p => p.ID).ToList();
                                var rows = db.AuthorizationUSERs
                                    .Where(au => au.NhanVienID == rg.UserId
                                              && au.ProjectID.HasValue
                                              && pIds.Contains(au.ProjectID.Value)).ToList();
                                foreach (var r in rows) db.AuthorizationUSERs.Remove(r);
                                deletedTotal += rows.Count;
                            }
                            else if (rg.Type == TYPE_VIRTUAL)
                            {
                                var vIds = db.Virtuals.Where(v => v.IDGroup == rg.GroupId)
                                    .Select(v => v.ID).ToList();
                                var rows = db.AuthorizationVituals
                                    .Where(av => av.NhanVienID == rg.UserId
                                              && av.VirtualID.HasValue
                                              && vIds.Contains(av.VirtualID.Value)).ToList();
                                foreach (var r in rows) db.AuthorizationVituals.Remove(r);
                                deletedTotal += rows.Count;
                            }
                            else if (rg.Type == TYPE_VIDEO)
                            {
                                var vdIds = db.Videos.Where(vd => vd.AlbumID == rg.GroupId)
                                    .Select(vd => vd.IDVideo).ToList();
                                var rows = db.AuthorizationVideos
                                    .Where(avi => avi.NhanVienID == rg.UserId
                                               && avi.VideoID.HasValue
                                               && vdIds.Contains(avi.VideoID.Value)).ToList();
                                foreach (var r in rows) db.AuthorizationVideos.Remove(r);
                                deletedTotal += rows.Count;
                            }
                        }
                        else
                        {
                            // Xoá group-grant row trong AuthorizationUSER_Group
                            var rows = db.Database.ExecuteSqlCommand(
                                @"DELETE FROM dbo.AuthorizationUSER_Group
                                  WHERE NhanVienID = @p0 AND ContentType = @p1 AND IDGroup = @p2",
                                rg.UserId, (byte)rg.Type, rg.GroupId);
                            deletedTotal += rows;
                        }
                    }
                    if (deletedTotal > 0) db.SaveChanges();
                }

                // ===== B. Revoke file-grants legacy =====
                var allItems = req.Items != null ? req.Items.ToList() : new List<RevokeItem>();
                if (allItems.Count == 0)
                    return Json(new { deleted = deletedTotal });

                int deletedFile = 0;
                foreach (var grp in allItems.GroupBy(i => i.Type))
                {
                    if (grp.Key == TYPE_PROJECT)
                    {
                        foreach (var item in grp)
                        {
                            var rows = db.AuthorizationUSERs
                                .Where(au => au.NhanVienID == item.UserId && au.ProjectID == item.ContentId)
                                .ToList();
                            foreach (var r in rows) db.AuthorizationUSERs.Remove(r);
                            deletedFile += rows.Count;
                        }
                    }
                    else if (grp.Key == TYPE_VIRTUAL)
                    {
                        foreach (var item in grp)
                        {
                            var rows = db.AuthorizationVituals
                                .Where(av => av.NhanVienID == item.UserId && av.VirtualID == item.ContentId)
                                .ToList();
                            foreach (var r in rows) db.AuthorizationVituals.Remove(r);
                            deletedFile += rows.Count;
                        }
                    }
                    else if (grp.Key == TYPE_VIDEO)
                    {
                        foreach (var item in grp)
                        {
                            var rows = db.AuthorizationVideos
                                .Where(av => av.NhanVienID == item.UserId && av.VideoID == item.ContentId)
                                .ToList();
                            foreach (var r in rows) db.AuthorizationVideos.Remove(r);
                            deletedFile += rows.Count;
                        }
                    }
                }
                if (deletedFile > 0) db.SaveChanges();
                return Json(new { deleted = deletedTotal + deletedFile });
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ============= Tab 2: User-centric =============

        /// <summary>Lay tat ca quyen cua 1 user, 3 content types, kem title.</summary>
        public JsonResult UserGrants(int userId)
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                var nv = db.NhanViens.FirstOrDefault(n => n.ID == userId);
                if (nv == null) return Json(new { error = "Không tìm thấy người dùng" }, JsonRequestBehavior.AllowGet);

                var projects = (from au in db.AuthorizationUSERs
                                join p in db.Projects on au.ProjectID equals p.ID
                                where au.NhanVienID == userId
                                select new {
                                    type = (int)TYPE_PROJECT, typeName = "Project",
                                    contentId = p.ID, title = p.Title,
                                    createdate = au.Createdate
                                }).ToList();

                var virtuals = (from av in db.AuthorizationVituals
                                join v in db.Virtuals on av.VirtualID equals v.ID
                                where av.NhanVienID == userId
                                select new {
                                    type = (int)TYPE_VIRTUAL, typeName = "Virtual",
                                    contentId = v.ID, title = v.Title,
                                    createdate = av.Createdate
                                }).ToList();

                var videos = (from av in db.AuthorizationVideos
                              join vd in db.Videos on av.VideoID equals vd.IDVideo
                              where av.NhanVienID == userId
                              select new {
                                  type = (int)TYPE_VIDEO, typeName = "Video",
                                  contentId = vd.IDVideo, title = vd.Title,
                                  createdate = av.Createdate
                              }).ToList();

                var all = projects.Concat(virtuals).Concat(videos)
                    .OrderByDescending(g => g.createdate)
                    .Select(g => new {
                        g.type, g.typeName, g.contentId, g.title,
                        createdate = g.createdate.HasValue ? g.createdate.Value.ToString("yyyy-MM-dd HH:mm") : ""
                    })
                    .ToList();

                return Json(new {
                    user = new {
                        id = nv.ID, maNV = nv.MaNV, hoTen = nv.HoTen,
                        phongBan = nv.PhongBan != null ? nv.PhongBan.TenPhongBan : null
                    },
                    grants = all,
                    counts = new {
                        project = projects.Count, @virtual = virtuals.Count, video = videos.Count,
                        total = all.Count
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ============= Tab 3: Dept overview =============

        public JsonResult DeptOverview()
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                // Per dept: # active users, # ACTIVE users co quyen, total grants.
                // BUG fix: subquery UsersWithGrant phai filter IDTinhTrangLV=1 de coverage <= 100%.
                // Mau so va tu so phai cung pool: NV dang lam viec va co phong ban hop le.
                var sql = @"
                    SELECT pb.IDPhongBan AS Id, pb.TenPhongBan AS Name,
                           ISNULL(uc.ActiveUsers, 0) AS ActiveUsers,
                           ISNULL(gc.UsersWithGrant, 0) AS UsersWithGrant,
                           ISNULL(gc.GrantCount, 0) AS GrantCount
                      FROM dbo.PhongBan pb
                      LEFT JOIN (
                          SELECT IDPhongBan, COUNT(*) AS ActiveUsers
                            FROM dbo.NhanVien
                           WHERE IDTinhTrangLV = 1 AND IDPhongBan IS NOT NULL
                           GROUP BY IDPhongBan
                      ) uc ON uc.IDPhongBan = pb.IDPhongBan
                      LEFT JOIN (
                          SELECT n.IDPhongBan,
                                 COUNT(DISTINCT au.NhanVienID) AS UsersWithGrant,
                                 COUNT(*) AS GrantCount
                            FROM (
                                SELECT NhanVienID FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                                UNION ALL
                                SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                                UNION ALL
                                SELECT NhanVienID FROM dbo.AuthorizationVideo  WHERE NhanVienID IS NOT NULL
                            ) au
                            JOIN dbo.NhanVien n ON au.NhanVienID = n.ID
                           WHERE n.IDTinhTrangLV = 1 AND n.IDPhongBan IS NOT NULL
                           GROUP BY n.IDPhongBan
                      ) gc ON gc.IDPhongBan = pb.IDPhongBan
                     WHERE ISNULL(uc.ActiveUsers, 0) > 0
                     ORDER BY ActiveUsers DESC";
                var rows = db.Database.SqlQuery<DeptStatRow>(sql).ToList()
                    .Select(r => new {
                        id = r.Id,
                        name = r.Name ?? "(chưa đặt tên)",
                        activeUsers = r.ActiveUsers,
                        usersWithGrant = r.UsersWithGrant,
                        grantCount = r.GrantCount,
                        coverage = r.ActiveUsers > 0
                            ? Math.Round((double)r.UsersWithGrant / r.ActiveUsers * 100, 1) : 0
                    });
                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        /// <summary>List NV trong PhongBan + so quyen moi nguoi.</summary>
        public JsonResult DeptUsers(int phongBanId)
        {
            if (!HasPerm(A_Constants.VIEW_ALL)) return Forbid();
            try
            {
                var sql = @"
                    SELECT n.ID AS Id, n.MaNV, n.HoTen,
                           ISNULL(gc.GrantCount, 0) AS GrantCount
                      FROM dbo.NhanVien n
                      LEFT JOIN (
                          SELECT NhanVienID, COUNT(*) AS GrantCount
                            FROM (
                                SELECT NhanVienID FROM dbo.AuthorizationUSER  WHERE NhanVienID IS NOT NULL
                                UNION ALL
                                SELECT NhanVienID FROM dbo.AuthorizationVitual WHERE NhanVienID IS NOT NULL
                                UNION ALL
                                SELECT NhanVienID FROM dbo.AuthorizationVideo  WHERE NhanVienID IS NOT NULL
                            ) au
                           GROUP BY NhanVienID
                      ) gc ON gc.NhanVienID = n.ID
                     WHERE n.IDTinhTrangLV = 1 AND n.IDPhongBan = @p0
                     ORDER BY ISNULL(gc.GrantCount, 0) DESC, n.HoTen";
                var rows = db.Database.SqlQuery<DeptUserRow>(
                        sql, new System.Data.SqlClient.SqlParameter("@p0", phongBanId))
                    .ToList()
                    .Select(r => new {
                        id = r.Id, maNV = r.MaNV, hoTen = r.HoTen, grantCount = r.GrantCount
                    });
                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ============= helpers =============

        private bool HasPerm(string action)
        {
            try { return dbP.A_CheckQuyen(IDQuyenHT, PermKey, action).First() != 0; }
            catch { return false; }
        }

        private JsonResult Forbid()
        {
            Response.StatusCode = 403;
            return Json(new { error = "forbidden" }, JsonRequestBehavior.AllowGet);
        }

        private JsonResult Err(Exception ex)
        {
            Response.StatusCode = 200;
            return Json(new { error = true, message = ex.Message, type = ex.GetType().Name },
                JsonRequestBehavior.AllowGet);
        }

        private T ReadJson<T>() where T : class
        {
            try
            {
                Request.InputStream.Position = 0;
                using (var sr = new StreamReader(Request.InputStream))
                {
                    var json = sr.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(json)) return null;
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// Build fresh SqlParameter[] tu dict raw values. Goi moi lan truoc khi SqlQuery,
        /// tranh "parameter already contained by another collection" khi reuse SqlParameter.
        /// </summary>
        private static object[] MakeParams(Dictionary<string, object> values)
        {
            return values.Select(kv => (object)new System.Data.SqlClient.SqlParameter(kv.Key, kv.Value ?? DBNull.Value)).ToArray();
        }

        /// <summary>
        /// Expand list group refs sang list ContentItem.
        /// - Project group: query Projects WHERE IDGroup IN (group + descendants neu Recursive)
        /// - Virtual group: query Virtual WHERE IDGroup = X
        /// - Video album: query Video WHERE AlbumID = X
        /// Dedupe theo (type, id).
        /// </summary>
        private List<ContentItem> ExpandGroupsToItems(GroupRef[] groups)
        {
            var result = new List<ContentItem>();
            if (groups == null || groups.Length == 0) return result;
            var seen = new HashSet<string>();
            foreach (var g in groups)
            {
                if (g.Type == TYPE_PROJECT)
                {
                    var groupIds = g.Recursive
                        ? ExpandGroupDescendants(g.GroupId)
                        : new HashSet<int> { g.GroupId };
                    var ids = db.Projects
                        .Where(p => p.IDGroup.HasValue && groupIds.Contains(p.IDGroup.Value))
                        .Select(p => p.ID).ToList();
                    foreach (var pid in ids)
                    {
                        var k = "1-" + pid;
                        if (seen.Add(k)) result.Add(new ContentItem { Type = 1, Id = pid });
                    }
                }
                else if (g.Type == TYPE_VIRTUAL)
                {
                    var ids = db.Virtuals
                        .Where(v => v.IDGroup == g.GroupId)
                        .Select(v => v.ID).ToList();
                    foreach (var vid in ids)
                    {
                        var k = "2-" + vid;
                        if (seen.Add(k)) result.Add(new ContentItem { Type = 2, Id = vid });
                    }
                }
                else if (g.Type == TYPE_VIDEO)
                {
                    var ids = db.Videos
                        .Where(vd => vd.AlbumID == g.GroupId)
                        .Select(vd => vd.IDVideo).ToList();
                    foreach (var vid in ids)
                    {
                        var k = "3-" + vid;
                        if (seen.Add(k)) result.Add(new ContentItem { Type = 3, Id = vid });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Merge Items + expanded Groups, dedupe theo (type, id).
        /// </summary>
        private ContentItem[] MergeItemsAndGroups(ContentItem[] items, GroupRef[] groups)
        {
            var seen = new HashSet<string>();
            var result = new List<ContentItem>();
            if (items != null)
            {
                foreach (var it in items)
                {
                    var k = it.Type + "-" + it.Id;
                    if (seen.Add(k)) result.Add(it);
                }
            }
            foreach (var it in ExpandGroupsToItems(groups))
            {
                var k = it.Type + "-" + it.Id;
                if (seen.Add(k)) result.Add(it);
            }
            return result.ToArray();
        }

        private static HashSet<int> ExpandGroupDescendants(int rootGid)
        {
            var allowed = new HashSet<int> { rootGid };
            var tree = ProjectsGroupHierarchy.GetAllTree();
            ProjectGroupNode rootNode = null;
            Action<ProjectGroupNode> findRoot = null;
            findRoot = n => {
                if (rootNode != null) return;
                if (n.IDGroup == rootGid) { rootNode = n; return; }
                foreach (var c in n.Children) findRoot(c);
            };
            foreach (var n in tree) findRoot(n);
            if (rootNode == null) return allowed;

            Action<ProjectGroupNode> walk = null;
            walk = node => {
                foreach (var c in node.Children)
                    if (allowed.Add(c.IDGroup)) walk(c);
            };
            walk(rootNode);
            return allowed;
        }

        // ============= DTOs =============

        public class GrantRequest
        {
            public int[] UserIds { get; set; }
            public ContentItem[] Items { get; set; }
            // NEW: cap quyen theo NHOM. Server tu expand thanh content IDs roi merge voi Items.
            public GroupRef[] Groups { get; set; }
        }
        public class GroupRef
        {
            public byte Type { get; set; }       // 1=ProjectsGroup, 2=VirtualGroup, 3=Album
            public int GroupId { get; set; }
            public bool Recursive { get; set; }  // only meaningful cho ProjectsGroup (hierarchy)
        }
        public class RevokeRequest
        {
            public RevokeItem[] Items { get; set; }
            // NEW: revoke o cap NHOM. Server expand sang content IDs roi delete.
            public RevokeGroup[] Groups { get; set; }
        }
        public class ContentItem
        {
            public byte Type { get; set; }
            public int Id { get; set; }
        }
        public class RevokeItem
        {
            public int UserId { get; set; }
            public byte Type { get; set; }
            public int ContentId { get; set; }
        }
        public class RevokeGroup
        {
            public int UserId { get; set; }
            public byte Type { get; set; }       // 1=ProjectsGroup, 2=VirtualGroup, 3=Album
            public int GroupId { get; set; }
            public bool Recursive { get; set; }  // only Project (hierarchy)
            // NEW: nếu true, KHÔNG xoá group-grant mà xoá legacy file-grants in group
            public bool ScopeFileOnly { get; set; }
        }
        public class DeptStatRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ActiveUsers { get; set; }
            public int UsersWithGrant { get; set; }
            public int GrantCount { get; set; }
        }
        public class DeptUserRow
        {
            public int Id { get; set; }
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public int GrantCount { get; set; }
        }
        public class MaNVResolveRequest
        {
            public string[] MaNVs { get; set; }
        }
        public class GrantGroupRow
        {
            public int Type { get; set; }   // 1=Project, 2=Virtual, 3=Video
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public int ContentCount { get; set; }
            public int NhanVienID { get; set; }
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string TenPhongBan { get; set; }
            public DateTime? LastCreatedate { get; set; }
            public int GrantType { get; set; }    // 1=group-grant (auto-inherit), 0=file-grant only
            public int IsRecursive { get; set; }  // 1=recursive (Project), 0=direct only
        }
        public class AnomalyDeptRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ActiveUsers { get; set; }
        }
        public class AnomalyUserRow
        {
            public int Id { get; set; }
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string TenPhongBan { get; set; }
            public int GrantCount { get; set; }
        }
    }
}
