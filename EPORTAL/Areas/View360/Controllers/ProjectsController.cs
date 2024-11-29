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
        public ActionResult Index(int? page, string search, string IDGroup)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            if (IDGroup == null) IDGroup = "";
            ViewBag.search = search;

            var res = from a in db.Project_select(search)
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
                      };
            List<ProjectsGroup> pg = db.ProjectsGroups.ToList();
            if (IDGroup != "")
            {
                res = res.Where(x=>x.IDGroup== Convert.ToInt32(IDGroup));
                ViewBag.PGList = new SelectList(pg, "IDGroup", "GroupName", Convert.ToInt32(IDGroup));
            }else { ViewBag.PGList = new SelectList(pg, "IDGroup", "GroupName"); }
            
            
            ViewBag.listPG = pg;
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x=>x.Date).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<PhongBan> pb = db.PhongBans.ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            List<ProjectsGroup> pg = db.ProjectsGroups.ToList();
            ViewBag.PGList = new SelectList(pg, "IDGroup", "GroupName");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(ProjectValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.ImageFile != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";

                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.Images = "~/Images/" + FileName;
                }
                //Upload file pdf
                string filePath = string.Empty;
                if (Request != null)
                {
                    HttpPostedFileBase file = Request.Files["FileUpload"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string pathPDF = Server.MapPath("~/UploadedFiles/");
                        if (!Directory.Exists(pathPDF))
                        {
                            Directory.CreateDirectory(pathPDF);
                        }
                        filePath = pathPDF + Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm") + "-" + file.FileName);

                        file.SaveAs(filePath);
                        Stream stream = file.InputStream;
                        if (file.FileName.EndsWith(".pdf"))
                        {
                            _DO.FilePDF = "~/UploadedFiles/" + filePath;
                        }
                        else
                        {

                        }

                    }
                    else
                    {
                        //TempData["msgError"] = "<script>alert('Vui lòng nhập file Import');</script>";
                    }
                }
                else
                {
                    //TempData["msgError"] = "<script>alert('Vui lòng nhập file Import');</script>";
                }

                var a = db.Projects_insert(_DO.Title, _DO.URL, _DO.Images, _DO.Date, _DO.Note, _DO.IDPhongBan, _DO.FilePDF,_DO.IDGroup);
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
                List<ProjectsGroup> pg = db.ProjectsGroups.ToList();
                ViewBag.PGList = new SelectList(pg, "IDGroup", "GroupName",DO.IDGroup);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(ProjectValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/Images/");
                //string path ="~/Images/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                //string FileName = _DO.ImageFile !=null?Path.GetFileNameWithoutExtension(_DO.ImageFile.FileName):"";
                string FileName = _DO.ImageFile != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";
                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.Images = "~/Images/" + FileName;
                }

                //Upload file pdf
                string filePath = string.Empty;
                if (Request != null)
                {
                    HttpPostedFileBase file = Request.Files["FileUpload"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string pathPDF = Server.MapPath("~/UploadedFiles/");
                        if (!Directory.Exists(pathPDF))
                        {
                            Directory.CreateDirectory(pathPDF);
                        }
                        filePath = pathPDF + Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm") + "-" + file.FileName);

                        file.SaveAs(filePath);
                        Stream stream = file.InputStream;
                        if (file.FileName.EndsWith(".pdf"))
                        {
                            _DO.FilePDF = "~/UploadedFiles/" + filePath;
                        }

                    }
                    
                    var a = db.Projects_update(_DO.ID, _DO.Title, _DO.URL, _DO.Images, _DO.Date, _DO.Note, _DO.IDPhongBan, _DO.FilePDF,_DO.IDGroup);
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
                }
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
            string path = "/App_Data/Template_Permission.xlsx";
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
        public ActionResult ImportExcel(AuthorizationUSERValidation _DO)
        {
            
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
                    filePath = path + Path.GetFileName(file.FileName);

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
                      }).ToList().Count();

            return rs;
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
                    list_Projects = list_Projects.Where(x => x.IDGroup == Convert.ToInt32(IDGroup)).ToList();
                    ViewBag.PGList = new SelectList(listpg, "IDGroup", "GroupName", Convert.ToInt32(IDGroup));
                }
                else { ViewBag.PGList = new SelectList(listpg, "IDGroup", "GroupName"); }
                if (list_Projects.Count > 0)
                {
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


                        Worksheet.Cell("F" + row).Value = countListAuthorization(item.ID);
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

                        Worksheet.Cell("D" + row).Value = db.NhanViens.Where(x => x.ID == item.NhanVienID).Select(x => x.PhongBan.TenPhongBan);
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
    }
}