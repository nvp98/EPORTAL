using EPORTAL.Models;
using EPORTAL.ModelsEquipment;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace EPORTAL.Areas.View360.Controllers
{
    public class DocumentLibraryController : Controller
    {
        PhanQuyenHTEntities db = new PhanQuyenHTEntities();
        EPORTALEntities dbE = new EPORTALEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "DocumentLibrary";
        List<string> listQuyen = new List<string>();
        public DocumentLibraryController()
        {
            listQuyen = db.A_CheckListQuyen(IDQuyenHT, controll).ToList();
        }
        // GET: View360/DocumentLibrary
        public ActionResult Index(int? page, string search)
        {
            //listQuyen = db.A_CheckListQuyen(IDQuyenHT, controll).ToList();
            if (listQuyen.Contains(A_Constants.VIEW_ALL) == false)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;
            var res = from a in db.L_ThuVienFile_select(search)
                      select new L_ThuVienFileValidation
                      {
                          ID = a.ID,
                          TenTaiLieu = a.TenTaiLieu,
                          FileName = a.FileName,
                          GhiChu = a.GhiChu,
                          Createdate = a.Createdate,
                          TenNhomTV = a.TenNhomTV,
                          IDNhom = a.IDNhom ?? default,
                      };
            ViewBag.Quyen = 1;
            if (listQuyen.Contains(A_Constants.ADD) == false)
            {
                ViewBag.Quyen = 0;
            }
            ViewBag.QuyenEdit = 1;
            if (listQuyen.Contains(A_Constants.EDIT) == false)
            {
                ViewBag.QuyenEdit = 0;
            }
            var listNhom = db.L_NhomThuVienFile.ToList();
            ViewBag.listNhom = listNhom;
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.ID).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            List<L_NhomThuVienFile> tb = db.L_NhomThuVienFile.ToList();
            ViewBag.NhomTV = new SelectList(tb, "IDNhom", "TenNhomTV");
            if (listQuyen.Contains(A_Constants.ADD) == false)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return PartialView();
        }
        public static string convertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        [HttpPost]
        public ActionResult Create(L_ThuVienFileValidation _DO)
        {
            try
            {
                if ((_DO.FileUpload != null) && (_DO.FileUpload.ContentLength > 0) && !string.IsNullOrEmpty(_DO.FileUpload.FileName))
                {
                    string path = Server.MapPath("~/UploadedFiles/Document/");
                    string filePath = string.Empty;
                    string fileName = string.Empty;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    fileName = Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm")) + "-" + convertToUnSign(_DO.FileUpload.FileName);
                    filePath = path + fileName;
                    _DO.FileUpload.SaveAs(path + fileName);
                    _DO.FileName = fileName;
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Vui lòng thêm file tài liệu');</script>";
                }

                var a = db.L_ThuVienFile_insert(_DO.TenTaiLieu, _DO.FileName, _DO.GhiChu, _DO.IDNhom, DateTime.Now);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "DocumentLibrary");
        }
        public ActionResult Edit(int id)
        {
            if (listQuyen.Contains(A_Constants.EDIT) == false)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.L_ThuVienFile_searchByID(id)
                       select new L_ThuVienFileValidation
                       {
                           TenTaiLieu = a.TenTaiLieu,
                           FileName = a.FileName,
                           GhiChu = a.GhiChu,
                           IDNhom = a.IDNhom ?? default,
                       }).ToList();
            L_ThuVienFileValidation DO = new L_ThuVienFileValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.TenTaiLieu = a.TenTaiLieu;
                    DO.FileName = a.FileName;
                    DO.GhiChu = a.GhiChu;
                    DO.IDNhom = a.IDNhom;
                }

            }
            else
            {
                HttpNotFound();
            }
            List<L_NhomThuVienFile> tb = db.L_NhomThuVienFile.ToList();
            ViewBag.NhomTV = new SelectList(tb, "IDNhom", "TenNhomTV",DO.IDNhom);
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(L_ThuVienFileValidation _DO)
        {

            try
            {
                if ((_DO.FileUpload != null) && (_DO.FileUpload.ContentLength > 0) && !string.IsNullOrEmpty(_DO.FileUpload.FileName))
                {
                    string path = Server.MapPath("~/UploadedFiles/Document/");
                    string filePath = string.Empty;
                    string fileName = string.Empty;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    fileName = Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm")) + "-" + convertToUnSign(_DO.FileUpload.FileName);
                    filePath = path + fileName;
                    _DO.FileUpload.SaveAs(path + fileName);
                    _DO.FileName = fileName;
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Vui lòng thêm file tài liệu');</script>";
                }


                //Upload file pdf

                var a = db.L_ThuVienFile_update(_DO.ID, _DO.TenTaiLieu, _DO.FileName, _DO.GhiChu, _DO.IDNhom);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "DocumentLibrary");
        }
        public ActionResult Delete(int? id)
        {
            if (listQuyen.Contains(A_Constants.ADD) == false)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.L_ThuVienFile_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "DocumentLibrary");
        }
        public ActionResult Authorization(int? page, int? id)
        {
            if (listQuyen.Contains(A_Constants.VIEW_ALL) == false)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var model = db.L_ThuVienFile.Where(x => x.ID == id).FirstOrDefault();
            ViewBag.Title_TV = model.TenTaiLieu;
            List<AuthorizationTVValidation> rs = new List<AuthorizationTVValidation>();

            rs = (from a in db.L_AuthorizationTV_searchByIDTV(id)
                  select new AuthorizationTVValidation
                  {
                      ID = a.ID,
                      NhanVienID = (int)a.NhanVienID,
                      IDThuVien = (int)a.IDThuVien,
                      Createdate = (DateTime)a.Createdate,
                      MaNV = a.MaNV,
                      HoTen = a.HoTen
                  }).ToList();


            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(rs.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult AddPermission(int id)
        {
            if (listQuyen.Contains(A_Constants.ADD) == false)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NhanVienView> nv = dbE.NhanViens.Where(x => x.IDTinhTrangLV == 1).Select(x => new NhanVienView { ID = x.ID, HoTen = x.MaNV + " - " + x.HoTen }).ToList();
            ViewBag.Selected = new SelectList(nv, "ID", "HoTen");
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddPermission(AuthorizationTVValidation _DO)
        {
            L_AuthorizationTV aus = new L_AuthorizationTV();

            for (int i = 0; i < _DO.Selected.Length; i++)
            {
                aus.NhanVienID = Convert.ToInt32(_DO.Selected[i]);
                aus.IDThuVien = Convert.ToInt32(_DO.ID);
                aus.Createdate = DateTime.Now;
                db.L_AuthorizationTV.Add(aus);
                db.SaveChanges();
            }
            return RedirectToAction("Authorization", "DocumentLibrary", new { id = _DO.ID });

        }
        public JsonResult DeleteAutho(string[] IDdata)
        {

            if (listQuyen.Contains(A_Constants.DELETE) == false)
            {
                return Json("Bạn không có quyền thực hiện chức năng này");
            }
            try
            {
                foreach (var data in IDdata)
                {
                    db.L_AuthorizationTV_delete(Convert.ToInt32(data));
                }
                return Json("Xóa dữ liệu thành công");
            }
            catch (Exception e)
            {
                return Json("Xóa dữ liệu thất bại: " + e.Message + "");
            }

        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/Template_Document.xlsx";
            return File(path, "application/vnd.ms-excel", "Template_Document.xlsx");
        }
        public ActionResult ImportExcel()
        {
            //List<Project> nt = db.Projects.ToList();
            //ViewBag.ProjectID = new SelectList(nt, "ID", "Title");
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            List<L_ThuVienFileValidation> pro = db.L_ThuVienFile.Select(x => new L_ThuVienFileValidation { ID = x.ID, TenTaiLieu = x.TenTaiLieu }).ToList();
            ViewBag.Selected = new SelectList(pro, "ID", "TenTaiLieu");

            return PartialView();
        }
        [HttpPost]
        public ActionResult ImportExcel(AuthorizationTVValidation _DO)
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

                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            for (int a = 0; a < _DO.Selected.Length; a++)
                            {
                                for (int i = 2; i < dt.Rows.Count; i++)
                                {
                                    string NhanVien = dt.Rows[i][1].ToString().Trim();
                                    var CheckNV = dbE.NhanViens.Where(x => x.MaNV == NhanVien).FirstOrDefault();
                                    if (NhanVien != "" && CheckNV != null)
                                    {
                                        int ThuVienID = Convert.ToInt32(_DO.Selected[a]);
                                        var Check = db.L_AuthorizationTV.Where(x => x.NhanVienID == CheckNV.ID && x.IDThuVien == ThuVienID).FirstOrDefault();
                                        DateTime DayNow = DateTime.Now;
                                        if (Check == null)
                                        {
                                            db.L_AuthorizationTV_insert(CheckNV.ID, DayNow, ThuVienID);
                                            dtc++;
                                        }

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
                            TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import: " + ex.Message + "');</script>";
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

            return RedirectToAction("Index", "DocumentLibrary");
        }
    }
}