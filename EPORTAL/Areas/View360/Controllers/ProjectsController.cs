using EPORTAL.Common;
using EPORTAL.Models;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExcelDataReader;
using System.Data;
using ClosedXML.Excel;
using EPORTAL.ModelsTagSign;

namespace EPORTAL.Areas.View360.Controllers
{
    public class ProjectsController : Controller
    {
        // GET: View360/Projects
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Projects";
        // Folder-file explorer view:
        // gid    = group ID dang xem (null = root - hien tat ca top-level folders)
        // search = tu khoa (search recursive trong gid + descendants)
        // sort   = "date-desc" (default) | "date-asc" | "name-asc" | "name-desc"
        // view   = "grid" (default) | "list"
        public ActionResult Index(int? page, string search, string IDGroup, int? gid, string sort, string view)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            // Backward-compat: legacy IDGroup query param map sang gid
            if (!gid.HasValue && !string.IsNullOrEmpty(IDGroup))
            {
                int legacyGid;
                if (int.TryParse(IDGroup, out legacyGid)) gid = legacyGid;
            }
            ViewBag.search = search;
            var sortKey = string.IsNullOrEmpty(sort) ? "date-desc" : sort.ToLowerInvariant();
            var viewMode = string.IsNullOrEmpty(view) ? "grid" : view.ToLowerInvariant();
            if (viewMode != "list") viewMode = "grid";
            ViewBag.Sort = sortKey;
            ViewBag.View = viewMode;
            ViewBag.CurrentGid = gid;

            // Full project list (filtered by search keyword neu co)
            var all = (from a in db.Project_select(search)
                       select new ProjectValidation
                       {
                           ID = a.ID,
                           Title = a.Title,
                           Images = a.Images,
                           URL = a.URL,
                           Date = (DateTime)a.Date,
                           Note = a.Note,
                           FilePDF = a.FilePDF,
                           IDPhongBan = (int)a.IDPhongBan,
                           TenPhongBan = a.TenPhongBan,
                           IDGroup = a.IDGroup ?? default
                       }).ToList();

            List<ProjectsGroup> pg = db.ProjectsGroups.ToList();
            ViewBag.PGList = ProjectsGroupHierarchy.BuildSelectList(gid);
            ViewBag.listPG = pg;

            // Hierarchy tree day du - dung cho sidebar va breadcrumb
            var tree = ProjectsGroupHierarchy.GetAllTree();
            var flat = ProjectsGroupHierarchy.FlattenPreOrder(tree);
            var byId = flat.ToDictionary(n => n.IDGroup);

            // CONSISTENCY: override ProjectCount tu cung nguon voi content (`all` tu Project_select SP)
            // thay vi raw SQL trong LoadAllInternal. Tranh truong hop sidebar 172 vs content 170 (SP loc them).
            // ProjectCount = DIRECT count (projects co IDGroup == node.IDGroup).
            var directCountByGroup = all
                .Where(p => p.IDGroup > 0)
                .GroupBy(p => p.IDGroup)
                .ToDictionary(g => g.Key, g => g.Count());
            foreach (var n in flat)
            {
                n.ProjectCount = directCountByGroup.ContainsKey(n.IDGroup) ? directCountByGroup[n.IDGroup] : 0;
            }

            // SUBTREE count: tinh tong project trong node + tat ca descendants (recursive).
            // Hien song song voi direct count -> user thay duoc "quy mo ca cay con".
            var subtreeCount = new Dictionary<int, int>();
            Func<ProjectGroupNode, int> computeSubtree = null;
            computeSubtree = node => {
                if (subtreeCount.ContainsKey(node.IDGroup)) return subtreeCount[node.IDGroup];
                int total = node.ProjectCount;
                foreach (var c in node.Children) total += computeSubtree(c);
                subtreeCount[node.IDGroup] = total;
                return total;
            };
            foreach (var n in tree) computeSubtree(n);
            ViewBag.SubtreeCount = subtreeCount;

            ViewBag.HierarchyTree = tree;
            ViewBag.HierarchyFlat = flat;

            // Build breadcrumb: tu root den current folder
            var crumbs = new List<ProjectGroupNode>();
            if (gid.HasValue && byId.ContainsKey(gid.Value))
            {
                var cur = byId[gid.Value];
                while (cur != null)
                {
                    crumbs.Insert(0, cur);
                    cur = cur.ParentIDGroup.HasValue && byId.ContainsKey(cur.ParentIDGroup.Value)
                          ? byId[cur.ParentIDGroup.Value] : null;
                }
            }
            ViewBag.Breadcrumb = crumbs;
            ViewBag.CurrentFolder = gid.HasValue && byId.ContainsKey(gid.Value) ? byId[gid.Value] : null;

            // Subfolders va Projects trong current folder
            List<ProjectGroupNode> subfolders;
            List<ProjectValidation> projects;
            bool isSearching = !string.IsNullOrEmpty(search);

            if (isSearching)
            {
                // Khi search: hien tat ca projects match (khong giu rang buoc folder)
                // Va hien tat ca folders match name
                subfolders = flat
                    .Where(n => (n.GroupName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(n => n.GroupName)
                    .ToList();
                projects = all;
            }
            else if (gid.HasValue)
            {
                subfolders = byId.ContainsKey(gid.Value) ? byId[gid.Value].Children.ToList() : new List<ProjectGroupNode>();
                projects = all.Where(p => p.IDGroup == gid.Value).ToList();
            }
            else
            {
                // Root: top-level folders + projects khong thuoc group nao
                subfolders = tree;
                projects = all.Where(p => p.IDGroup == 0 || !byId.ContainsKey(p.IDGroup)).ToList();
            }

            // ProjectCount cua subfolder da duoc fill boi LoadAllInternal (toan bo, khong search-filter).
            // Khi search, giu count tong de user thay quy mo folder.
            ViewBag.Subfolders = subfolders;

            // Apply sort
            switch (sortKey)
            {
                case "date-asc": projects = projects.OrderBy(x => x.Date).ToList(); break;
                case "name-asc": projects = projects.OrderBy(x => x.Title ?? "").ToList(); break;
                case "name-desc": projects = projects.OrderByDescending(x => x.Title ?? "").ToList(); break;
                case "date-desc":
                default: projects = projects.OrderByDescending(x => x.Date).ToList(); break;
            }

            // Grid view: 24 cards/page. List view: tat ca trong 1 trang (UI lazy-load via CSS content-visibility).
            int pageSize = viewMode == "list" ? Math.Max(projects.Count, 1) : 24;
            int pageNumber = viewMode == "list" ? 1 : (page ?? 1);
            return View(projects.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create(int? gid)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<PhongBan> pb = db.PhongBans.ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            // Pre-select group tu folder dang xem (gid query param) - SelectList da
            // co selectedValue, DropDownListFor se pick up cho user.
            ViewBag.PGList = ProjectsGroupHierarchy.BuildSelectList(gid);
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProjectValidation _DO)
        {
            var imageError = FileUploadValidator.ValidateImage(_DO.ImageFile);
            if (imageError != null)
            {
                TempData["msgError"] = "<script>alert('" + imageError + "');</script>";
                return RedirectToAction("Index", "Projects");
            }
            HttpPostedFileBase pdfFile = Request != null ? Request.Files["FileUpload"] : null;
            var pdfError = FileUploadValidator.ValidatePdf(pdfFile);
            if (pdfError != null)
            {
                TempData["msgError"] = "<script>alert('" + pdfError + "');</script>";
                return RedirectToAction("Index", "Projects");
            }

            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (_DO.ImageFile != null && _DO.ImageFile.ContentLength > 0)
                {
                    var safeName = FileUploadValidator.SafeFileName(_DO.ImageFile.FileName);
                    _DO.ImageFile.SaveAs(Path.Combine(path, safeName));
                    _DO.Images = "~/Images/" + safeName;
                }
                //Upload file pdf
                string filePath = string.Empty;
                if (pdfFile != null && pdfFile.ContentLength > 0)
                {
                    string pathPDF = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(pathPDF))
                    {
                        Directory.CreateDirectory(pathPDF);
                    }
                    var safePdfName = FileUploadValidator.SafeFileName(pdfFile.FileName);
                    filePath = Path.Combine(pathPDF, safePdfName);
                    pdfFile.SaveAs(filePath);
                    _DO.FilePDF = "~/UploadedFiles/" + filePath;
                }

                var a = db.Projects_insert(_DO.Title, _DO.URL, _DO.Images, _DO.Date, _DO.Note, _DO.IDPhongBan, _DO.FilePDF,_DO.IDGroup);

                // (Auto-sync legacy đã được xoá: hybrid permission qua AuthorizationUSER_Group
                //  + SP Project_select_USER mới tự xử lý group-grant inheritance. Project mới
                //  trong group được cấp group-grant tự động hiển thị cho user khi list.)

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Projects");
        }

        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
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
                           Title = a.Title,
                           IDGroup = a.IDGroup ?? default
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
                    DO.IDGroup = a.IDGroup;
                }


                List<PhongBan> pb = db.PhongBans.ToList();
                ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDPhongBan);
                ViewBag.Date = DO.Date.ToString("yyyy-MM-dd");
                ViewBag.PGList = ProjectsGroupHierarchy.BuildSelectList(DO.IDGroup);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProjectValidation _DO)
        {
            var imageError = FileUploadValidator.ValidateImage(_DO.ImageFile);
            if (imageError != null)
            {
                TempData["msgError"] = "<script>alert('" + imageError + "');</script>";
                return RedirectToAction("Index", "Projects");
            }
            HttpPostedFileBase pdfFile = Request != null ? Request.Files["FileUpload"] : null;
            var pdfError = FileUploadValidator.ValidatePdf(pdfFile);
            if (pdfError != null)
            {
                TempData["msgError"] = "<script>alert('" + pdfError + "');</script>";
                return RedirectToAction("Index", "Projects");
            }

            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (_DO.ImageFile != null && _DO.ImageFile.ContentLength > 0)
                {
                    var safeName = FileUploadValidator.SafeFileName(_DO.ImageFile.FileName);
                    _DO.ImageFile.SaveAs(Path.Combine(path, safeName));
                    _DO.Images = "~/Images/" + safeName;
                }

                //Upload file pdf
                string filePath = string.Empty;
                if (pdfFile != null && pdfFile.ContentLength > 0)
                {
                    string pathPDF = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(pathPDF))
                    {
                        Directory.CreateDirectory(pathPDF);
                    }
                    var safePdfName = FileUploadValidator.SafeFileName(pdfFile.FileName);
                    filePath = Path.Combine(pathPDF, safePdfName);
                    pdfFile.SaveAs(filePath);
                    _DO.FilePDF = "~/UploadedFiles/" + filePath;
                }

                var a = db.Projects_update(_DO.ID, _DO.Title, _DO.URL, _DO.Images, _DO.Date, _DO.Note, _DO.IDPhongBan, _DO.FilePDF,_DO.IDGroup);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Projects");
        }
        public ActionResult Delete( int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.Project_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Projects");
        }

        /// <summary>
        /// Di chuyen project sang group khac. targetId rong = chuyen ra ngoai (IDGroup=0 hoac NULL).
        /// Dung raw SQL de chi update IDGroup, khong dung lai SP Projects_update (can full payload).
        /// </summary>
        [HttpPost]
        public ActionResult Move(int id, string targetId)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0) return new HttpStatusCodeResult(403, "Không có quyền");
            try
            {
                int? newGroup = null;
                int parsed;
                if (!string.IsNullOrEmpty(targetId) && int.TryParse(targetId, out parsed)) newGroup = parsed;

                var entry = System.Configuration.ConfigurationManager.ConnectionStrings["EPORTALEntities"];
                if (entry == null) return new HttpStatusCodeResult(500, "Connection config missing");
                var raw = entry.ConnectionString;
                if (raw.IndexOf("metadata=", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    raw = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(raw).ProviderConnectionString;
                }
                using (var conn = new System.Data.SqlClient.SqlConnection(raw))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "UPDATE dbo.Projects SET IDGroup = @gid WHERE ID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@gid", (object)newGroup ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                return new HttpStatusCodeResult(200);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
        public ActionResult Authorization(int? page, int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var model = db.Projects.Where(x => x.ID == id).FirstOrDefault();
            ViewBag.Title_Project = model.Title;

            var rs = (from a in db.AuthorizationUSERs.Where(a => a.ProjectID == id)
                      join b in db.NhanViens on a.NhanVienID equals b.ID
                      select new AuthorizationUSERValidation
                      {
                          ID = a.ID,
                          NhanVienID = b.ID,
                          ProjectID = (int)a.ProjectID,
                          Createdate = (DateTime)a.Createdate,
                          MaNV = b.MaNV,
                          HoTen = b.HoTen
                      }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(rs.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult AddPermission(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NhanVienView> nv = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).Select(x => new NhanVienView { ID = x.ID, HoTen = x.MaNV + " - " + x.HoTen }).ToList();
            ViewBag.Selected = new SelectList(nv, "ID", "HoTen");
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPermission(AuthorizationUSERValidation _DO)
        {
            AuthorizationUSER aus = new AuthorizationUSER();

            for (int i = 0; i < _DO.Selected.Length; i++)
            {
                aus.NhanVienID = Convert.ToInt32(_DO.Selected[i]);
                aus.ProjectID = Convert.ToInt32(_DO.ID);
                aus.Createdate = DateTime.Now;
                db.AuthorizationUSERs.Add(aus);
                db.SaveChanges();
            }
            return RedirectToAction("Authorization", "Projects", new { id = _DO.ID });

        }
        public JsonResult DeleteAutho(string[] IDdata)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                return Json("Bạn không có quyền thực hiện chức năng này");
            }
            try
            {
                foreach(var data in IDdata)
                {
                    db.AuthorizationUSER_delete(Convert.ToInt32(data));
                }
                return Json("Xóa dữ liệu thành công");
                //TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                return Json("Xóa dữ liệu thất bại: " + e.Message + "");
                //TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            //return RedirectToAction("Authorization", "Projects", new { id = Convert.ToInt32(IDProject)});
            //return Json(TempData);
        }
        public FileResult DownloadExcel()
        {
            // Phai dung Server.MapPath de ra duong dan vat ly duoi web-app root.
            // Truoc day "/App_Data/..." bi TransmitFile resolve theo working dir -> tro ra
            // D:\Dev\EPORTAL\App_Data (thieu thu muc app "EPORTAL") -> DirectoryNotFound.
            string path = Server.MapPath("~/App_Data/Template_Permission.xlsx");
            return File(path, "application/vnd.ms-excel", "Template_Permission.xlsx");
        }
        public ActionResult ImportExcel()
        {
            //List<Project> nt = db.Projects.ToList();
            //ViewBag.ProjectID = new SelectList(nt, "ID", "Title");
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<ProjectView> pro = db.Projects.Select(x => new ProjectView { ID = x.ID, Title = x.Title}).ToList();
            ViewBag.Selected = new SelectList(pro, "ID", "Title");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcel(AuthorizationUSERValidation _DO)
        {
            HttpPostedFileBase excelFile = Request != null ? Request.Files["FileUpload"] : null;
            var excelError = FileUploadValidator.ValidateExcel(excelFile);
            if (excelError != null)
            {
                TempData["msgError"] = "<script>alert('" + excelError + "');</script>";
                return RedirectToAction("Index", "Projects");
            }

            string filePath = string.Empty;
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["FileUpload"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string path = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var safeName = FileUploadValidator.SafeFileName(file.FileName);
                    filePath = Path.Combine(path, safeName);

                    file.SaveAs(filePath);
                    Stream stream = file.InputStream;

                    IExcelDataReader reader = null;
                    if (file.FileName.ToLower().EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (file.FileName.ToLower().EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        TempData["msg"] = "<script>alert('Vui lòng chọn đúng định dạng file Excel');</script>";
                        return View();
                    }
                    DataSet result = reader.AsDataSet();
                    DataTable dt = result.Tables[0];
                    reader.Close();
                    int dtc = 0;
                    int IDNV = 0;
                    
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            for (int a = 0; a < _DO.Selected.Length; a++)
                            {
                                AuthorizationUSER aus = new AuthorizationUSER();
                                for (int i = 2; i < dt.Rows.Count; i++)
                                {
                                    DataRow dr = dt.Rows[i];
                                    IDNV = IDNhanVien(dr[1].ToString());
                                    int IDProject = Convert.ToInt32(_DO.Selected[a]);
                                    var NhanVien = db.AuthorizationUSERs.Where(x => x.ProjectID == IDProject && x.NhanVienID == IDNV).FirstOrDefault();
                                    if (IDNV > 0 && CheckPer(IDNV, _DO.ProjectID) == 0 && NhanVien == null)
                                    {
                                        aus.NhanVienID = IDNV;
                                        aus.ProjectID = Convert.ToInt32(_DO.Selected[a]);
                                        aus.Createdate = DateTime.Now;
                                        db.AuthorizationUSERs.Add(aus);
                                        db.SaveChanges();
                                        dtc++;
                                    }
                                }
                            }
                            string msg = "";
                            if (dtc != 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu";
                            }
                            else { msg = "File import không có dữ liệu"; }

                            TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";

                        }
                        catch (Exception ex)
                        {
                            TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import: "+ex.Message+"');</script>";
                        }
                       
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                    }

                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
                }
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
            }

            return RedirectToAction("Index", "Projects");
        }
        public int IDNhanVien(string MaNV)
        {
            var Regsbb = (from u in db.NhanViens
                          where u.MaNV.ToLower() == MaNV.ToLower()
                          select new { u.ID }).FirstOrDefault();
            if (Regsbb != null)
                return Regsbb.ID;
            return 0;
        }
        public int CheckPer(int NhanVienID,int ProjectID)
        {
            var Regsbb = (from u in db.AuthorizationUSERs
                          where u.NhanVienID==NhanVienID && u.ProjectID==ProjectID
                          select new { u.ID }).FirstOrDefault();
            if (Regsbb != null)
                return Regsbb.ID;
            return 0;
        }
        public int countListAuthorization(int id)
        {
            // COUNT truc tiep tren DB (truoc day ToList() ca bang join roi moi .Count() trong RAM).
            // Join NhanVien de chi dem quyen tro toi nhan vien con ton tai.
            return (from a in db.AuthorizationUSERs.Where(a => a.ProjectID == id)
                    join b in db.NhanViens on a.NhanVienID equals b.ID
                    select a.ID).Count();
        }
        // ==================================================================
        //  "NGUOI DUOC XEM" - loai tru quyen xem theo TUNG du an
        //  (bang AuthorizationUSER_Exclude - deny-list phu len group/file-grant).
        //  KHONG dung den du lieu phan quyen o View360/Permission: user giu nguyen
        //  group-grant, chi rieng du an bi exclude la an di (SP _select_USER loc).
        //
        //  VI SAO RAW SQL (Database.SqlQuery/ExecuteSqlCommand) o cac method duoi:
        //   - AuthorizationUSER_Exclude la BANG MOI, chua map vao EDMX -> khong co db.* (EF) de goi.
        //   - ViewersList con UNION file-grant + group-grant (gom ca to tien Recursive) + dedupe theo
        //     MaNV; IncludeViewer/Details guard phai resolve TAT CA NhanVienID cung MaNV (data trung
        //     MaNV) -> cau co self-join NhanVien, kho/khong gon neu lam bang LINQ.
        //   - Moi tham so qua SqlParameter -> khong injection.
        // ==================================================================

        /// <summary>Danh sach nguoi dang duoc xem du an (tu file-grant + group-grant phu du an).</summary>
        public JsonResult ViewersList(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                Response.StatusCode = 403;
                return Json(new { ok = false, error = "Không có quyền" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var proj = db.Projects.FirstOrDefault(p => p.ID == id);
                if (proj == null)
                    return Json(new { ok = false, error = "Không tìm thấy dự án" }, JsonRequestBehavior.AllowGet);
                int gid = proj.IDGroup ?? -1;

                // Grant nhom phu du an nay = grant tren CHINH group cua du an (moi Recursive)
                // + grant Recursive=1 tren BAT KY to tien nao (theo dung logic SP _select_USER).
                var ancestorCsv = "-1";
                if (gid > 0)
                {
                    var flat = ProjectsGroupHierarchy.GetAllFlat();
                    var byId = flat.ToDictionary(n => n.IDGroup);
                    var ancestors = new List<int>();
                    var cur = byId.ContainsKey(gid) ? byId[gid] : null;
                    for (int lv = 0; lv < 10 && cur != null && cur.ParentIDGroup.HasValue; lv++)
                    {
                        var pid = cur.ParentIDGroup.Value;
                        if (ancestors.Contains(pid)) break; // chong vong lap data ban
                        ancestors.Add(pid);
                        cur = byId.ContainsKey(pid) ? byId[pid] : null;
                    }
                    if (ancestors.Count > 0) ancestorCsv = string.Join(",", ancestors);
                }

                // ancestorCsv chi gom int tu hierarchy DB -> ghep literal an toan.
                var sql = @"
                    SELECT n.ID AS NhanVienID, n.MaNV, n.HoTen, pb.TenPhongBan,
                           MAX(src.IsGroup) AS HasGroupGrant,
                           MAX(1 - src.IsGroup) AS HasFileGrant,
                           MAX(CASE WHEN n.IDTinhTrangLV = 1 THEN 1 ELSE 0 END) AS IsActive
                    FROM (
                        SELECT au.NhanVienID, 0 AS IsGroup
                          FROM dbo.AuthorizationUSER au
                         WHERE au.ProjectID = @pid AND au.NhanVienID IS NOT NULL
                        UNION ALL
                        SELECT ag.NhanVienID, 1
                          FROM dbo.AuthorizationUSER_Group ag
                         WHERE ag.ContentType = 1
                           AND (ag.IDGroup = @gid OR (ag.[Recursive] = 1 AND ag.IDGroup IN (" + ancestorCsv + @")))
                    ) src
                    JOIN dbo.NhanVien n ON n.ID = src.NhanVienID
                    LEFT JOIN dbo.PhongBan pb ON n.IDPhongBan = pb.IDPhongBan
                    GROUP BY n.ID, n.MaNV, n.HoTen, pb.TenPhongBan";
                var raw = db.Database.SqlQuery<ViewerRow>(sql,
                    new System.Data.SqlClient.SqlParameter("@pid", id),
                    new System.Data.SqlClient.SqlParameter("@gid", gid)).ToList();

                // Exclusions hien co cua du an
                var exclusions = db.Database.SqlQuery<ViewerExcludeRow>(@"
                    SELECT e.NhanVienID, ISNULL(n.MaNV, '') AS MaNV
                      FROM dbo.AuthorizationUSER_Exclude e
                      LEFT JOIN dbo.NhanVien n ON n.ID = e.NhanVienID
                     WHERE e.ContentType = 1 AND e.ContentID = @pid",
                    new System.Data.SqlClient.SqlParameter("@pid", id)).ToList();
                var exclMaNV = new HashSet<string>(exclusions
                    .Where(e => !string.IsNullOrWhiteSpace(e.MaNV))
                    .Select(e => e.MaNV.Trim().ToUpper()));
                var exclIds = new HashSet<int>(exclusions.Select(e => e.NhanVienID));

                // Dedupe theo MaNV (data co MaNV trung, khac ID): giu ID nho nhat, OR cac flag.
                // Excluded check theo MaNV -> 1 ban ghi trung bi exclude = ca nguoi do bi exclude
                // (khop voi cach SP resolve @Ids theo MaNV).
                var rows = raw
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.MaNV) ? ("__id_" + r.NhanVienID) : r.MaNV.Trim().ToUpper())
                    .Select(g => {
                        var first = g.OrderBy(r => r.NhanVienID).First();
                        var key = string.IsNullOrWhiteSpace(first.MaNV) ? null : first.MaNV.Trim().ToUpper();
                        return new
                        {
                            nhanVienId = first.NhanVienID,
                            maNV = first.MaNV,
                            hoTen = first.HoTen,
                            phongBan = first.TenPhongBan,
                            hasGroupGrant = g.Max(r => r.HasGroupGrant) == 1,
                            hasFileGrant = g.Max(r => r.HasFileGrant) == 1,
                            active = g.Max(r => r.IsActive) == 1,
                            excluded = (key != null && exclMaNV.Contains(key)) || g.Any(r => exclIds.Contains(r.NhanVienID))
                        };
                    })
                    .OrderBy(r => r.hoTen)
                    .ToList();

                return Json(new { ok = true, rows = rows, total = rows.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>Loai tru 1 nguoi khoi DUY NHAT du an nay (khong dung quyen nhom/le khac).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ExcludeViewer(int id, int userId)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                Response.StatusCode = 403;
                return Json(new { ok = false, error = "Không có quyền" });
            }
            try
            {
                // 1 dong la du: SP check exclusion theo TAP ID cung MaNV cua user dang login.
                db.Database.ExecuteSqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM dbo.AuthorizationUSER_Exclude
                                    WHERE NhanVienID = @uid AND ContentType = 1 AND ContentID = @pid)
                        INSERT INTO dbo.AuthorizationUSER_Exclude (NhanVienID, ContentType, ContentID, CreatedByID)
                        VALUES (@uid, 1, @pid, @admin)",
                    new System.Data.SqlClient.SqlParameter("@uid", userId),
                    new System.Data.SqlClient.SqlParameter("@pid", id),
                    new System.Data.SqlClient.SqlParameter("@admin", EPORTAL.Models.MyAuthentication.ID));
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, error = ex.Message }); }
        }

        /// <summary>Bo loai tru - cho nguoi do xem lai du an (xoa theo MOI ID cung MaNV).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult IncludeViewer(int id, int userId)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                Response.StatusCode = 403;
                return Json(new { ok = false, error = "Không có quyền" });
            }
            try
            {
                var deleted = db.Database.ExecuteSqlCommand(@"
                    DELETE e FROM dbo.AuthorizationUSER_Exclude e
                     WHERE e.ContentType = 1 AND e.ContentID = @pid
                       AND e.NhanVienID IN (
                           SELECT n2.ID FROM dbo.NhanVien n1
                           JOIN dbo.NhanVien n2 ON n2.MaNV = n1.MaNV
                           WHERE n1.ID = @uid AND n1.MaNV IS NOT NULL AND LTRIM(RTRIM(n1.MaNV)) <> ''
                           UNION SELECT @uid)",
                    new System.Data.SqlClient.SqlParameter("@pid", id),
                    new System.Data.SqlClient.SqlParameter("@uid", userId));
                return Json(new { ok = true, deleted = deleted });
            }
            catch (Exception ex) { return Json(new { ok = false, error = ex.Message }); }
        }

        public class ViewerRow
        {
            public int NhanVienID { get; set; }
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string TenPhongBan { get; set; }
            public int HasGroupGrant { get; set; }
            public int HasFileGrant { get; set; }
            public int IsActive { get; set; }
        }
        public class ViewerExcludeRow
        {
            public int NhanVienID { get; set; }
            public string MaNV { get; set; }
        }

        public ActionResult ExportToExcel(String search, string IDGroup)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EX).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                if (search == null) search = "";
                if (IDGroup == null) IDGroup = "";
                ViewBag.search = search;
                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_Project360.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_Project360_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("360view");
                List<ProjectsGroup> listpg = db.ProjectsGroups.ToList();
                //IList<Project> list_Projects = db.Projects.ToList();
                var list_Projects = (from a in db.Project_select(search)
                                     select new ProjectValidation
                                     {
                                         ID = a.ID,
                                         Title = a.Title,
                                         Images = a.Images,
                                         URL = a.URL,
                                         Date = (DateTime)a.Date,
                                         Note = a.Note,
                                         FilePDF = a.FilePDF,
                                         IDPhongBan = (int)a.IDPhongBan,
                                         TenPhongBan = a.TenPhongBan,
                                         IDGroup = a.IDGroup ?? default
                                     }).OrderByDescending(x => x.Date).ToList();
                
                if (IDGroup != "")
                {
                    var rootGid = Convert.ToInt32(IDGroup);
                    // Recursive: include direct + all descendant groups de export "ca cay con".
                    // Tan dung hierarchy moi (ParentIDGroup). Neu group khong co children, set = {rootGid}.
                    var allowed = ExpandGroupDescendants(rootGid);
                    list_Projects = list_Projects.Where(x => allowed.Contains(x.IDGroup)).ToList();
                    ViewBag.PGList = new SelectList(listpg, "IDGroup", "GroupName", rootGid);
                }
                else { ViewBag.PGList = new SelectList(listpg, "IDGroup", "GroupName"); }
                if (list_Projects.Count > 0)
                {
                    // Dem so quyen theo TUNG du an bang 1 query gom nhom (tranh N+1: truoc day goi
                    // countListAuthorization() moi du an -> moi lan ToList().Count() ca bang join).
                    var projIds = list_Projects.Select(x => x.ID).ToList();
                    var authCounts = db.AuthorizationUSERs
                        .Where(a => a.ProjectID.HasValue && projIds.Contains(a.ProjectID.Value))
                        .GroupBy(a => a.ProjectID.Value)
                        .Select(g => new { ProjectID = g.Key, C = g.Count() })
                        .ToDictionary(x => x.ProjectID, x => x.C);

                    int row = 2, rowlast = 2, stt = 0;
                    foreach (var item in list_Projects)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.Title;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        var pg = listpg.Where(x => x.IDGroup == item.IDGroup).FirstOrDefault();
                        

                        Worksheet.Cell("C" + row).Value = (pg != null ? pg.GroupName : "");
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.Date;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("E" + row).Value = item.TenPhongBan;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("F" + row).Value = authCounts.TryGetValue(item.ID, out var ac) ? ac : 0;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        row = rowlast - 1;
                    }
                    Worksheet.Range("A2:F" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:F" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A2:F" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A2:F" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "ListProject360.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/Projects'</script>";
                    return RedirectToAction("Index", "Projects");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Projects'</script>";
                return RedirectToAction("Index", "Projects");
            }

        }
        public ActionResult ExportToExceldetail(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EX).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ProjectDetail.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ProjectDetail_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("Detail");

                var model = db.Projects.Where(x => x.ID == id).FirstOrDefault();


                var rs = (from a in db.AuthorizationUSERs.Where(a => a.ProjectID == id)
                          join b in db.NhanViens on a.NhanVienID equals b.ID
                          select new AuthorizationUSERValidation
                          {
                              ID = a.ID,
                              NhanVienID = b.ID,
                              ProjectID = (int)a.ProjectID,
                              Createdate = (DateTime)a.Createdate,
                              MaNV = b.MaNV,
                              HoTen = b.HoTen
                          }).ToList();

                if (rs.Count > 0)
                {
                    Worksheet.Cell("A1").Value = model.Title;
                    Worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    Worksheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    Worksheet.Cell("A1").Style.Alignment.WrapText = true;
                    int row = 2, rowlast = 2, stt = 0;
                    foreach (var item in rs)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.MaNV;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.HoTen;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = db.NhanViens.Where(x => x.ID == item.NhanVienID).Select(x => x.PhongBan.TenPhongBan).FirstOrDefault();
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.Createdate;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        row = rowlast - 1;
                    }
                    Worksheet.Range("A2:E" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:E" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A2:E" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A2:E" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "ListProjectDetail360.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/Projects'</script>";
                    return RedirectToAction("Index", "Projects");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Projects'</script>";
                return RedirectToAction("Index", "Projects");
            }

        }

        /// <summary>
        /// Tu root group ID, tra ve set gom chinh no + tat ca descendant IDGroup.
        /// Dung cho export/filter recursive theo cay hierarchy ProjectsGroup.
        /// </summary>
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
