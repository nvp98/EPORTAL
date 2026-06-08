using EPORTAL.Common;
using EPORTAL.Models;
using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    public class ProjectsGroupController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ProjectsGroup";

        // GET: View360/ProjectsGroup
        public ActionResult Index()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            // Tree N-cap cho admin (khong filter quyen).
            var tree = ProjectsGroupHierarchy.GetAllTree();
            return View(tree);
        }

        public ActionResult Create(int? parentId, string returnUrl = null)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            ViewBag.ParentOptions = BuildParentOptions(excludeId: null);
            ViewBag.ReturnUrl = returnUrl;
            var model = new ProjectsGroupValidation { ParentIDGroup = parentId };
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProjectsGroupValidation _DO, string returnUrl = null)
        {
            try
            {
                // Dung raw SQL de luu ParentIDGroup - SP cu khong nhan param nay.
                ProjectsGroupHierarchy.Insert(_DO.GroupName, _DO.ParentIDGroup);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            return RedirectBackOrDefault(returnUrl);
        }

        public ActionResult Edit(int id, string returnUrl = null)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var node = ProjectsGroupHierarchy.GetById(id);
            if (node == null) return HttpNotFound();
            var DO = new ProjectsGroupValidation
            {
                IDGroup = node.IDGroup,
                GroupName = node.GroupName,
                ParentIDGroup = node.ParentIDGroup
            };
            ViewBag.ParentOptions = BuildParentOptions(excludeId: id);
            ViewBag.ReturnUrl = returnUrl;
            return PartialView(DO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProjectsGroupValidation _DO, string returnUrl = null)
        {
            try
            {
                ProjectsGroupHierarchy.Update(_DO.IDGroup, _DO.GroupName, _DO.ParentIDGroup);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            return RedirectBackOrDefault(returnUrl);
        }

        public ActionResult Delete(int? id, string returnUrl = null)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (!id.HasValue) return RedirectToAction("Index");
            try
            {
                ProjectsGroupHierarchy.Delete(id.Value);
                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message.Replace("'", "\\'") + "');</script>";
            }
            return RedirectBackOrDefault(returnUrl);
        }

        /// <summary>
        /// Neu returnUrl la local URL hop le -> redirect ve do (vd user den tu /Projects).
        /// Fallback: trang quan ly nhom truyen thong /ProjectsGroup.
        /// </summary>
        private ActionResult RedirectBackOrDefault(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "ProjectsGroup");
        }

        /// <summary>
        /// Diagnostic: tra ve text /JSON mo ta chinh xac:
        ///   - Connection string app dang dung (server + database)
        ///   - Co cot ParentIDGroup khong
        ///   - Raw data tu DB (cung cau truy van controller dung)
        ///   - Tree sau khi build
        /// Dung de so sanh voi SQL Server Management Studio: neu khac -> data o DB khac.
        /// </summary>
        public ActionResult Diagnose()
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
                // Strip password de an toan log
                var safe = System.Text.RegularExpressions.Regex.Replace(providerConn, @"(?i)password=[^;]+", "password=***");
                sb.AppendLine("=== Connection ===");
                sb.AppendLine(safe);
                sb.AppendLine();

                using (var conn = new System.Data.SqlClient.SqlConnection(providerConn))
                {
                    conn.Open();
                    sb.AppendLine("=== Server / DB ===");
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "SELECT @@SERVERNAME AS Server, DB_NAME() AS DbName, SUSER_NAME() AS User_, @@VERSION AS Ver", conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            sb.AppendLine("Server   = " + rd["Server"]);
                            sb.AppendLine("Database = " + rd["DbName"]);
                            sb.AppendLine("User     = " + rd["User_"]);
                        }
                    }
                    sb.AppendLine();

                    sb.AppendLine("=== ParentIDGroup column? ===");
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "SELECT 1 FROM sys.columns WHERE Name='ParentIDGroup' AND Object_ID=Object_ID('dbo.ProjectsGroup')", conn))
                    {
                        var r = cmd.ExecuteScalar();
                        sb.AppendLine(r != null ? "YES - column exists" : "NO - migration chua chay tren DB nay");
                    }
                    sb.AppendLine();

                    sb.AppendLine("=== Raw query result ===");
                    sb.AppendLine(string.Format("{0,5} {1,-35} {2,15}", "ID", "GroupName", "ParentIDGroup"));
                    sb.AppendLine(new string('-', 60));
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "SELECT IDGroup, GroupName, ParentIDGroup FROM dbo.ProjectsGroup ORDER BY ISNULL(ParentIDGroup, IDGroup), ParentIDGroup, GroupName", conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            sb.AppendLine(string.Format("{0,5} {1,-35} {2,15}",
                                rd["IDGroup"],
                                (rd["GroupName"] ?? "").ToString(),
                                rd["ParentIDGroup"] == DBNull.Value ? "NULL" : rd["ParentIDGroup"].ToString()));
                        }
                    }
                }
                sb.AppendLine();

                sb.AppendLine("=== Tree sau khi GetAllTree() build ===");
                var tree = ProjectsGroupHierarchy.GetAllTree();
                AppendTree(tree, sb, 0);
                sb.AppendLine();
                sb.AppendLine("Roots count = " + tree.Count + " (kỳ vọng = 9 neu data dung)");
            }
            catch (Exception e)
            {
                sb.AppendLine("ERROR: " + e.ToString());
            }
            return Content(sb.ToString(), "text/plain; charset=utf-8");
        }

        private void AppendTree(List<ProjectGroupNode> nodes, System.Text.StringBuilder sb, int depth)
        {
            foreach (var n in nodes)
            {
                sb.AppendLine(new string(' ', depth * 2) + "+ [" + n.IDGroup + "] " + n.GroupName +
                              " (Parent=" + (n.ParentIDGroup.HasValue ? n.ParentIDGroup.Value.ToString() : "NULL") +
                              ", Children=" + n.Children.Count + ")");
                AppendTree(n.Children, sb, depth + 1);
            }
        }

        [HttpPost]
        public ActionResult MoveUp(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0) return new HttpStatusCodeResult(403);
            try
            {
                var moved = ProjectsGroupHierarchy.MoveUp(id);
                return new HttpStatusCodeResult(moved ? 200 : 204);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(400, e.Message);
            }
        }

        [HttpPost]
        public ActionResult MoveDown(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0) return new HttpStatusCodeResult(403);
            try
            {
                var moved = ProjectsGroupHierarchy.MoveDown(id);
                return new HttpStatusCodeResult(moved ? 200 : 204);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(400, e.Message);
            }
        }

        [HttpPost]
        public ActionResult Move(int id, string targetId)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0) return new HttpStatusCodeResult(403, "Không có quyền");
            try
            {
                int? newParent = null;
                int parsed;
                if (!string.IsNullOrEmpty(targetId) && int.TryParse(targetId, out parsed)) newParent = parsed;
                var node = ProjectsGroupHierarchy.GetById(id);
                if (node == null) return new HttpStatusCodeResult(404, "Nhóm không tồn tại");
                ProjectsGroupHierarchy.Update(id, node.GroupName, newParent);
                return new HttpStatusCodeResult(200);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(400, e.Message);
            }
        }

        /// <summary>
        /// Build options cho dropdown parent picker. Loai bo chinh group dang edit + descendants
        /// (de tranh chu trinh).
        /// </summary>
        private List<SelectListItem> BuildParentOptions(int? excludeId)
        {
            var tree = ProjectsGroupHierarchy.GetAllTree();
            var flat = ProjectsGroupHierarchy.FlattenPreOrder(tree);
            HashSet<int> excludeSet = null;
            if (excludeId.HasValue)
            {
                excludeSet = ProjectsGroupHierarchy.GetDescendantIds(excludeId.Value);
                excludeSet.Add(excludeId.Value);
            }
            var items = new List<SelectListItem>();
            foreach (var n in flat)
            {
                if (excludeSet != null && excludeSet.Contains(n.IDGroup)) continue;
                // Indent voi non-breaking space cho dropdown - tao tree look
                var prefix = string.Concat(Enumerable.Repeat("    ", n.Depth));
                items.Add(new SelectListItem
                {
                    Value = n.IDGroup.ToString(),
                    Text = prefix + n.GroupName
                });
            }
            return items;
        }
    }
}
