using EPORTAL.Common;
using EPORTAL.Models;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    public class ListProjectController : Controller
    {
        // GET: View360/ListProject
        EPORTALEntities db = new EPORTALEntities();

        // Cache 60s/user/page/id - SP-result ton tien, cache dodge it.
        // VaryByCustom="User" -> moi user co cache key rieng (xem Global.asax.cs).
        // KHONG cache HTML output - dua vao Session cache SP `Project_select_USER` (per-user inherent).
        // id     = parent group ID (primary tab)
        // sid    = sub-group ID (secondary tab, optional)
        // sort   = "date-desc" (default) | "date-asc" | "name-asc" | "name-desc"
        // view   = "grid" (default) | "list"
        public ActionResult Index(int? page, string search, int? id, int? sid, int? ps, string sort, string view)
        {
            var swTotal = System.Diagnostics.Stopwatch.StartNew();
            var timings = new System.Text.StringBuilder();
            long t0;

            if (search == null) search = "";
            ViewBag.search = search;
            // Persistent ps: neu URL khong co ?ps, doc tu cookie. Tranh redirect lai
            // (truoc day moi navigation = 2 page loads vi JS phai do grid roi reload).
            if (!ps.HasValue)
            {
                var psCookie = Request.Cookies["v360_ps"];
                int psFromCookie;
                if (psCookie != null && int.TryParse(psCookie.Value, out psFromCookie)
                    && psFromCookie >= 6 && psFromCookie <= 60)
                {
                    ps = psFromCookie;
                }
            }
            int pageSize = (ps.HasValue && ps.Value >= 6 && ps.Value <= 60) ? ps.Value : 10;
            ViewBag.PageSize = pageSize;
            var sortKey = string.IsNullOrEmpty(sort) ? "date-desc" : sort.ToLowerInvariant();
            var viewMode = string.IsNullOrEmpty(view) ? "grid" : view.ToLowerInvariant();
            if (viewMode != "list") viewMode = "grid";
            ViewBag.Sort = sortKey;
            ViewBag.View = viewMode;
            ViewBag.CurrentId = id;
            ViewBag.CurrentSid = sid;

            // Cache project list (heavy SP) trong Session 120s.
            var sessKey = "v360_proj_list_" + MyAuthentication.ID + "_" + (search ?? "");
            var sess = System.Web.HttpContext.Current?.Session;
            var cached = sess != null ? sess[sessKey] as Tuple<DateTime, List<ProjectValidation>> : null;
            List<ProjectValidation> all;
            t0 = swTotal.ElapsedMilliseconds;
            if (cached != null && (DateTime.UtcNow - cached.Item1).TotalSeconds < 120)
            {
                all = cached.Item2;
                timings.Append("projects=CACHE ");
            }
            else
            {
                all = db.Project_select_USER(search, MyAuthentication.ID)
                    .OrderByDescending(a => a.Date)
                    .Select(a => new ProjectValidation
                    {
                        ID = a.ID, Title = a.Title, Images = a.Images, URL = a.URL,
                        Date = (DateTime)a.Date, Note = a.Note, FilePDF = a.FilePDF,
                        IDPhongBan = (int)a.IDPhongBan, IDGroup = a.IDGroup ?? 0
                    })
                    .ToList();
                // SP Project_select_USER co the tra duplicate rows khi user co multiple
                // authorization tren cung 1 project (JOIN inflate). Dedupe theo ID de:
                //  - Count chinh xac (tranh inflate)
                //  - Tranh duplicate cards trong grid
                all = all.GroupBy(p => p.ID).Select(g => g.First()).ToList();
                if (sess != null) sess[sessKey] = Tuple.Create(DateTime.UtcNow, all);
                timings.Append("projects=DB(").Append(swTotal.ElapsedMilliseconds - t0).Append("ms,").Append(all.Count).Append("rows) ");
            }

            // Cache hierarchy + user-group-permissions 5 phut (data hiem khi doi).
            var hierKey = "v360_hier_" + MyAuthentication.ID;
            var hierCached = sess != null ? sess[hierKey] as Tuple<DateTime, List<EPORTAL.Common.ProjectGroupNode>> : null;
            List<EPORTAL.Common.ProjectGroupNode> hierarchy;
            t0 = swTotal.ElapsedMilliseconds;
            if (hierCached != null && (DateTime.UtcNow - hierCached.Item1).TotalSeconds < 300)
            {
                hierarchy = hierCached.Item2;
                timings.Append("hier=CACHE ");
            }
            else
            {
                var userGroupIds = db.Get_IDGroup(MyAuthentication.ID)
                    .Select(g => g.IDGroup).Where(gid => gid.HasValue).Select(gid => gid.Value).Distinct().ToList();
                hierarchy = EPORTAL.Common.ProjectsGroupHierarchy.GetHierarchy(userGroupIds);
                if (sess != null) sess[hierKey] = Tuple.Create(DateTime.UtcNow, hierarchy);
                timings.Append("hier=DB(").Append(swTotal.ElapsedMilliseconds - t0).Append("ms) ");
            }

            // Active state cho tabs
            int activeParentId = id ?? 0;
            int activeSubId = sid ?? 0;
            foreach (var p in hierarchy)
            {
                p.Active = (p.IDGroup == activeParentId);
                foreach (var c in p.Children) c.Active = (c.IDGroup == activeSubId);
            }
            ViewBag.Hierarchy = hierarchy;
            // Backward-compat: TabGroups van expose (flat list cua parents) cho cac view khac
            ViewBag.TabGroups = hierarchy.Select(p => new TabGroupViewModel {
                IDGroup = p.IDGroup, GroupName = p.GroupName, Active = p.Active
            }).ToList();

            // Seen IDs - cards co ID trong set nay = user da view, hien badge khac
            ViewBag.SeenIds = View360AccessTracker.GetSeenIds(
                MyAuthentication.ID, View360AccessTracker.ContentType.Project);

            // Model thong nhat: MOI ProjectsGroup la FOLDER, moi Project la FILE.
            // - Root (id=null): hien top-level groups AS folders + project nao khong thuoc nhom nao AS files
            // - Folder voi children (no sid): hien sub-folders + direct projects cua folder do
            // - Folder khong children (leaf): chi co files (direct projects)
            // - sid set: drill xuong sub-folder cu the
            var parentForMode = id.HasValue ? hierarchy.FirstOrDefault(p => p.IDGroup == id.Value) : null;
            ViewBag.ActiveParent = parentForMode;

            // "Tien do" sub-tab count = projects USER co quyen xem voi IDGroup = parent.
            // KHONG dung ProjectCount tu LoadAllInternal vi do la DB-wide count (bao gom
            // projects user khong co permission) -> mismatch voi Model.TotalItemCount.
            ViewBag.TienDoCount = parentForMode != null
                ? all.Count(x => x.IDGroup == parentForMode.IDGroup)
                : 0;

            // Subfolders cua scope hien tai
            List<EPORTAL.Common.ProjectGroupNode> subfolders;
            if (sid.HasValue && sid.Value > 0)
            {
                // Drilled vao sub-folder cu the: hien sub-sub-folders cua no (neu co)
                var sub = parentForMode != null
                    ? parentForMode.Children.FirstOrDefault(c => c.IDGroup == sid.Value)
                    : null;
                subfolders = sub != null ? sub.Children.ToList() : new List<EPORTAL.Common.ProjectGroupNode>();
            }
            else if (parentForMode != null)
            {
                // O trong mot folder (root-level parent): hien children cua no
                subfolders = parentForMode.Children.ToList();
            }
            else if (id.HasValue && id.Value > 0)
            {
                // ID khong match parent (legacy fallback): khong co folders
                subfolders = new List<EPORTAL.Common.ProjectGroupNode>();
            }
            else
            {
                // Root view: hien TAT CA top-level groups AS folders (du chua co children)
                subfolders = hierarchy.ToList();
            }

            ViewBag.Subfolders = subfolders;
            ViewBag.ShowFolders = subfolders.Count > 0;

            // Home view detection: chua chon tab nao + khong search + chua bam "Xem tat ca" (page).
            // Khi user click "Xem tat ca" -> URL co ?page=1 -> KHONG con la home view -> chuyen sang
            // full paginated view giong nhu cac trang da chon tab.
            bool isHomeView = !id.HasValue && !sid.HasValue && string.IsNullOrEmpty(search) && !page.HasValue;
            ViewBag.IsHomeView = isHomeView;

            // Direct files cua scope hien tai
            // "Tien do" sub-tab logic: khi click parent tab (khong sid) -> hien DIRECT projects
            //   cua parent (cac bao cao tien do), KHONG recursive xuong descendants.
            //   Cac sub-folder con (Can 4, Duc 4, NM.LG 2, ...) la sibling cua "Tien do".
            List<ProjectValidation> res;
            if (sid.HasValue && sid.Value > 0)
            {
                res = all.Where(x => x.IDGroup == sid.Value).ToList();
            }
            else if (parentForMode != null)
            {
                // "Tien do": parent direct projects only (default khi click parent tab)
                res = all.Where(x => x.IDGroup == parentForMode.IDGroup).ToList();
            }
            else if (id.HasValue && id.Value > 0)
            {
                // Fallback ID khong match
                res = all.Where(x => x.IDGroup == id.Value).ToList();
            }
            else
            {
                // Root: hien TAT CA projects (kham pha tong quan)
                res = all.ToList();
            }

            // Compute project counts cho sub-tabs.
            // Sub-tabs LUON hien activeParent.Children (du sid co set hay khong) - nen count
            // phai tinh cho activeParent.Children, KHONG phai cho `subfolders` (vi subfolders
            // co the empty khi drill vao leaf, lam count tat ca sub-tab = 0).
            var folderCounts = new Dictionary<int, int>();
            if (parentForMode != null && parentForMode.Children.Count > 0)
            {
                foreach (var c in parentForMode.Children)
                {
                    folderCounts[c.IDGroup] = all.Count(x => x.IDGroup == c.IDGroup);
                }
            }
            else
            {
                // Fallback (root view hoac leaf): tinh cho subfolders array
                foreach (var f in subfolders)
                {
                    folderCounts[f.IDGroup] = all.Count(x => x.IDGroup == f.IDGroup);
                }
            }
            ViewBag.FolderCounts = folderCounts;

            // Apply sort
            switch (sortKey)
            {
                case "date-asc":  res = res.OrderBy(x => x.Date).ToList(); break;
                case "name-asc":  res = res.OrderBy(x => x.Title ?? "").ToList(); break;
                case "name-desc": res = res.OrderByDescending(x => x.Title ?? "").ToList(); break;
                case "date-desc":
                default:          res = res.OrderByDescending(x => x.Date).ToList(); break;
            }

            // List view: bo pagination - render het + lazy load qua CSS content-visibility
            if (viewMode == "list")
            {
                return View(res.ToPagedList(1, Math.Max(res.Count, 1)));
            }

            // Home view: hien Top N "Du an moi cap nhat" - khong phan trang (1 trang showcase)
            if (isHomeView)
            {
                int topN = 12; // 12 du an gan day nhat
                var top = res.Take(topN).ToList();
                var pagedTop = new PagedList.StaticPagedList<ProjectValidation>(top, 1, Math.Max(top.Count, 1), top.Count);
                return View(pagedTop);
            }

            // Grid view: folders da chuyen len tabs phia tren (KHONG render trong grid nua),
            // nen pagination chi don gian theo so files thuan. Khong tru folder count.
            int currentPage = page ?? 1;
            var subset = res.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            var paged = new PagedList.StaticPagedList<ProjectValidation>(subset, currentPage, pageSize, res.Count);
            swTotal.Stop();
            timings.Append("total=").Append(swTotal.ElapsedMilliseconds).Append("ms");
            // Server timing header - mo F12 -> Network -> chon request -> Response Headers de xem
            try { Response.AddHeader("X-Server-Timing", timings.ToString()); } catch { }
            return View(paged);
        }
        /// <summary>
        /// Diagnostic: kiem tra trang thai tracking seen/unseen cho current user.
        /// Truy cap: /View360/ListProject/SeenDiag
        /// </summary>
        public ActionResult SeenDiag()
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                var entry = System.Configuration.ConfigurationManager.ConnectionStrings["EPORTALEntities"];
                var raw = entry != null ? entry.ConnectionString : "(missing)";
                var providerConn = raw;
                if (raw.IndexOf("metadata=", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    providerConn = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(raw).ProviderConnectionString;
                }

                sb.AppendLine("=== Current User ===");
                sb.AppendLine("MyAuthentication.ID = " + MyAuthentication.ID);
                sb.AppendLine("Session.SessionID  = " + (Session != null ? Session.SessionID : "(null)"));
                sb.AppendLine();

                using (var conn = new System.Data.SqlClient.SqlConnection(providerConn))
                {
                    conn.Open();

                    sb.AppendLine("=== Server / DB ===");
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "SELECT @@SERVERNAME AS Srv, DB_NAME() AS DbName", conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            sb.AppendLine("Server   = " + rd["Srv"]);
                            sb.AppendLine("Database = " + rd["DbName"]);
                        }
                    }
                    sb.AppendLine();

                    sb.AppendLine("=== Table View360_AccessLog ton tai? ===");
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "SELECT 1 FROM sys.tables WHERE name='View360_AccessLog' AND schema_id=SCHEMA_ID('dbo')", conn))
                    {
                        var r = cmd.ExecuteScalar();
                        sb.AppendLine(r != null ? "YES - table exists"
                                                : "NO - migration v360-all.sql CHUA chay!");
                    }
                    sb.AppendLine();

                    sb.AppendLine("=== Tong so log rows cua current user ===");
                    try
                    {
                        using (var cmd = new System.Data.SqlClient.SqlCommand(
                            @"SELECT COUNT(*) FROM dbo.View360_AccessLog
                              WHERE NhanVienID=@nv AND ContentType=1", conn))
                        {
                            cmd.Parameters.AddWithValue("@nv", MyAuthentication.ID);
                            var n = cmd.ExecuteScalar();
                            sb.AppendLine("Rows for ContentType=Project: " + (n ?? "ERROR"));
                        }
                    }
                    catch (Exception ex) { sb.AppendLine("ERR: " + ex.Message); }
                    sb.AppendLine();

                    sb.AppendLine("=== 10 row gan day cua current user ===");
                    try
                    {
                        using (var cmd = new System.Data.SqlClient.SqlCommand(
                            @"SELECT TOP 10 ID, NhanVienID, ContentType, ContentID, AccessAt, SessionID
                              FROM dbo.View360_AccessLog
                              WHERE NhanVienID=@nv
                              ORDER BY ID DESC", conn))
                        {
                            cmd.Parameters.AddWithValue("@nv", MyAuthentication.ID);
                            using (var rd = cmd.ExecuteReader())
                            {
                                sb.AppendLine(string.Format("{0,-8} {1,-12} {2,-4} {3,-8} {4,-20} {5}",
                                    "ID", "NhanVienID", "Type", "Content", "AccessAt", "Session"));
                                sb.AppendLine(new string('-', 80));
                                while (rd.Read())
                                {
                                    sb.AppendLine(string.Format("{0,-8} {1,-12} {2,-4} {3,-8} {4,-20} {5}",
                                        rd["ID"], rd["NhanVienID"], rd["ContentType"], rd["ContentID"],
                                        Convert.ToDateTime(rd["AccessAt"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                        rd["SessionID"] == DBNull.Value ? "(null)" : rd["SessionID"].ToString().Substring(0, Math.Min(24, rd["SessionID"].ToString().Length))));
                                }
                            }
                        }
                    }
                    catch (Exception ex) { sb.AppendLine("ERR: " + ex.Message); }
                }
                sb.AppendLine();

                sb.AppendLine("=== GetSeenIds() output ===");
                var seenSet = EPORTAL.Common.View360AccessTracker.GetSeenIds(
                    MyAuthentication.ID, EPORTAL.Common.View360AccessTracker.ContentType.Project);
                sb.AppendLine("Count = " + seenSet.Count);
                sb.AppendLine("IDs   = " + string.Join(", ", seenSet));
            }
            catch (Exception ex)
            {
                sb.AppendLine("FATAL ERROR: " + ex.ToString());
            }
            return Content(sb.ToString(), "text/plain; charset=utf-8");
        }

        // Recursive collect tat ca IDGroup descendant cua mot node (de "Tat ca N" includ het)
        private static void CollectDescendantIds(EPORTAL.Common.ProjectGroupNode node, HashSet<int> acc)
        {
            if (node.Children == null) return;
            foreach (var c in node.Children)
            {
                if (acc.Add(c.IDGroup))
                    CollectDescendantIds(c, acc);
            }
        }

        // Duration 1800s (30 min) thay vi 300s - Project Details gan nhu khong doi.
        // Tradeoff: data refresh sau 30 phut. Neu user can refresh ngay -> manual reload (Ctrl+F5 va recycle pool).
        public ActionResult Details(int id)
        {
            View360AccessTracker.Log(MyAuthentication.ID,
                View360AccessTracker.ContentType.Project, id,
                Session != null ? Session.SessionID : null);

            var res = (from a in db.Projects.Where(x => x.ID == id)
                       select new ProjectValidation
                       {
                           ID = a.ID,
                           URL = a.URL,
                           Date = (DateTime?)a.Date ?? DateTime.Now,
                           Note = a.Note,
                           IDPhongBan = (int)a.IDPhongBan,
                           FilePDF = a.FilePDF,
                           Images = a.Images,
                           Title = a.Title
                       }).ToList();
            ProjectValidation DO = new ProjectValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.URL = a.URL;
                    DO.Date = a.Date;
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.FilePDF = a.FilePDF;
                    DO.Images = a.Images;
                    DO.Title = a.Title;
                    DO.Note = a.Note;
                }
            }
            else
            {
                HttpNotFound();
            }
            return View(DO);
        }
    }
}