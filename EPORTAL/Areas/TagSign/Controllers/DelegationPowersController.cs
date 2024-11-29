using EPORTAL.Models;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class DelegationPowersController : Controller
    {
      
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "DelegationPowers";
        // GET: TagSign/DelegationPowers
        public ActionResult Index(int? page, string search, int? IDPhongBan, int? IDLKD)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var nd = db.AuthorizationContractors.Where(x => x.IDNhanVien == ModelsTagSign.MyAuthentication.ID).SingleOrDefault();

            if (search == null)
                search = "";


            List<PhongBan> pb = db.PhongBans.ToList();
            if (IDPhongBan == null) IDPhongBan = 0;
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan", IDPhongBan);

            List<KD_LoaiKyDuyet> l = db.KD_LoaiKyDuyet.ToList();
            if (IDLKD == null) IDLKD = 0;
            ViewBag.KDList = new SelectList(l, "IDLKD", "TenLoaiKyDuyet", IDLKD);

            var res = (from a in db.AuthorizationContractors_select(search)
                      join nv in db.NhanViens on a.IDNhanVien equals nv.ID
                      select new AuthorizationContractorsValidation
                      {
                          ID = (int)a.ID,                      
                          IDNhanVien = (int)a.IDNhanVien,
                          MaNV = nv.MaNV,
                          HoVaTen = nv.HoTen,
                          IDPhongBan = (int)nv.IDPhongBan,
                          TenPhongBan = nv.PhongBan.TenPhongBan,
                          IDCVKN = a.IDCVKN,
                          IDLKD = (int?)a.IDLKD??default
                      }).ToList();
            if(IDPhongBan != 0 && IDLKD != 0)
            {
                res = res.Where(x => x.IDPhongBan == IDPhongBan && x.IDLKD == IDLKD).ToList();
            }    
            else if(IDPhongBan != 0)
            {
                res = res.Where(x => x.IDPhongBan == IDPhongBan).ToList();
            }    
            else if(IDLKD != 0)
            {
                res = res.Where(x => x.IDLKD == IDLKD).ToList();
            }


            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderBy(x => x.IDNhanVien).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var Nhanvien = (from a in db.NhanViens
                        select new NhanVienView
                        {
                            ID = (int)a.ID,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            ViewBag.NVList = new SelectList(Nhanvien, "ID", "HoTen");

            List<KD_LoaiKyDuyet> q = db.KD_LoaiKyDuyet.ToList();
            ViewBag.LKDList = new SelectList(q, "IDLKD", "TenLoaiKyDuyet");

            List<PhongBan> pb = db.PhongBans.ToList();
            ViewBag.Selected = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(AuthorizationContractorsValidation _DO)
        {

            try
            {
                var Name = new List<string>();
                var check=db.AuthorizationContractors.Where(x=>x.IDNhanVien==_DO.IDNhanVien).FirstOrDefault();
                if (check == null) {
                    if (_DO.Selected !=null)
                    {
                        foreach (var bp in _DO.Selected)
                        {
                            Name.Add(bp);
                        }
                        var ListGate = string.Join(",", Name);

                        var a = db.AuthorizationContractors_insert(_DO.IDNhanVien, _DO.IDLKD, ListGate);
                    }
                    else
                    {
                        var a = db.AuthorizationContractors_insert(_DO.IDNhanVien, _DO.IDLKD, null);
                    }


                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('User đã được phân quyền trước đó');</script>";
                }
               
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "DelegationPowers");
        }
        public ActionResult Delete(int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.AuthorizationContractors_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "DelegationPowers");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.AuthorizationContractors.Where(x => x.ID == id)
                       select new AuthorizationContractorsValidation
                       {
                           ID = (int)a.ID,
                           IDNhanVien = (int)a.IDNhanVien,
                           IDLKD = (int)a.IDLKD,
                           IDCVKN = a.IDCVKN,

                       }).ToList();
            AuthorizationContractorsValidation DO = new AuthorizationContractorsValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.IDNhanVien = (int)a.IDNhanVien;
                    DO.IDQuyen = (int)a.IDQuyen;
                    DO.IDLKD = (int)a.IDLKD;
                    DO.IDCVKN = a.IDCVKN;  
                }

                var nhanvien = (from au in db.AuthorizationContractors.Where(x => x.IDNhanVien == DO.IDNhanVien)
                                join nv in db.NhanViens on au.IDNhanVien equals nv.ID
                                select new AuthorizationContractorsValidation
                                {
                                    IDNhanVien = nv.ID,
                                    HoVaTen = nv.MaNV + " : " + nv.HoTen,
                                }).ToList();

                ViewBag.NVList = new SelectList(nhanvien, "ID", "HoVaTen");

                List<NT_Permission> q = db.NT_Permission.ToList();
                ViewBag.IDQuyen = new SelectList(q, "IDQ", "Name", DO.IDQuyen);

                List<PhongBan> pb = db.PhongBans.ToList();
                ViewBag.Selected = new SelectList(pb, "IDPhongBan", "TenPhongBan");

                List<KD_LoaiKyDuyet> l = db.KD_LoaiKyDuyet.ToList();
                ViewBag.IDLKD = new SelectList(l, "IDLKD", "TenLoaiKyDuyet", DO.IDLKD);
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AuthorizationContractorsValidation _DO)
        {

            try
            {
                var Name = new List<string>();
                if(_DO.Selected != null)
                {
                    foreach (var bp in _DO.Selected)
                    {
                        Name.Add(bp);
                    }
                }    
                var ListGate = string.Join(",", Name);
                if (_DO.ImgChukynhay != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/Signature/");
                    //string path ="~/Images/";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var IDAU = db.AuthorizationContractors.Where(x => x.ID == _DO.ID).FirstOrDefault();
                    var IDNV = db.NhanViens.Where(x => x.ID == IDAU.IDNhanVien).FirstOrDefault();
                    string FileName = _DO.ImgChukynhay != null ? IDNV.MaNV + "_" + "Chukynhay" : "";
                    //To Get File Extension  
                    string FileExtension = _DO.ImgChukynhay != null ? Path.GetExtension(_DO.ImgChukynhay.FileName) : "";

                    if (_DO.ImgChukynhay != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.ImgChukynhay.SaveAs(path + FileName);
                        _DO.Chukynhay = "~/UploadedFiles/Signature/" + FileName;
                    }
                }
                if (_DO.ImgChukychinh != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/Signature/");
                    //string path ="~/Images/";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var IDAU = db.AuthorizationContractors.Where(x => x.ID == _DO.ID).FirstOrDefault();
                    var IDNV = db.NhanViens.Where(x => x.ID == IDAU.IDNhanVien).FirstOrDefault();
                    string FileName = _DO.ImgChukychinh != null ? IDNV.MaNV + "_" + "Chukychinh" : "";
                    //To Get File Extension  
                    string FileExtension = _DO.ImgChukychinh != null ? Path.GetExtension(_DO.ImgChukychinh.FileName) : "";

                    if (_DO.ImgChukychinh != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.ImgChukychinh.SaveAs(path + FileName);
                        _DO.Chukychinh = "~/UploadedFiles/Signature/" + FileName;
                    }
                }

                var ID = db.AuthorizationContractors.Where(x => x.ID == _DO.ID).FirstOrDefault();
                var Update = db.NhanViens.Where(x => x.ID == ID.IDNhanVien).FirstOrDefault();
                if (_DO.Chukychinh == null)
                {
                    db.TaiKhoan_Img(ID.IDNhanVien, _DO.Chukynhay, Update.Chukychinh);
                }
                else if (_DO.Chukynhay == null)
                {
                    db.TaiKhoan_Img(ID.IDNhanVien, Update.Chukynhay, _DO.Chukychinh);
                }
                else if (_DO.Chukychinh != null && _DO.Chukynhay != null)
                {
                    db.TaiKhoan_Img(ID.IDNhanVien, _DO.Chukynhay, _DO.Chukychinh);
                }
                if(_DO.Selected != null)
                {
                    var a = db.AuthorizationContractors_update(_DO.ID, ID.IDNhanVien, _DO.IDLKD, ListGate);
                }    
                else
                {
                    var a = db.AuthorizationContractors_update(_DO.ID, ID.IDNhanVien, _DO.IDLKD, null);
                }    
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "DelegationPowers");
        }
        public ActionResult ImportExcel()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            return PartialView();
        }
        [HttpPost]
        public ActionResult ImportExcels()
        {
            string filePath = string.Empty;
            string HoTenNT = "";
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
                    int imp = 0;
                    int stt = 0;

                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            EPORTALEntities db = new EPORTALEntities();
                            for (int i = 3; i < dt.Rows.Count; i++)
                            {
                                stt++;
                                string MNV = dt.Rows[i][1].ToString();
                                var IDNV = db.NhanViens.Where(x => x.MaNV == MNV).FirstOrDefault();
                                string HoTen = dt.Rows[i][2].ToString();
                                HoTenNT = HoTen.ToString();
                                string ViTri = dt.Rows[i][3].ToString();
                                string PhongBan = dt.Rows[i][4].ToString();
                                string TenQuyen = dt.Rows[i][5].ToString();
                                var IDLKD = db.KD_LoaiKyDuyet.Where(x => x.TenLoaiKyDuyet == TenQuyen).FirstOrDefault();
                                if (IDNV != null && IDLKD != null)
                                {
                                    var ID = db.AuthorizationContractors.Where(x => x.IDNhanVien == IDNV.ID).FirstOrDefault();
                                    if (ID == null)
                                    {
                                        var a = db.AuthorizationContractors_insert(IDNV.ID, IDLKD.IDLKD, null);
                                    }
                                    else
                                    {
                                        var a = db.AuthorizationContractors_update(ID.ID, ID.IDNhanVien, IDLKD.IDLKD, null);
                                    }

                                    imp++;
                                }

                            }


                            TempData["msgSuccess"] = "<script>alert('Import đươc: " + imp + " dòng');</script>";

                        }
                        catch (Exception ex)
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại nhân viên: " + HoTenNT + " dòng: " + stt + "');</script>";

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
            return RedirectToAction("Index", "DelegationPowers");
        }
    }
}