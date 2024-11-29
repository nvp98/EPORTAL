using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.Models;
using System.IO;

namespace EPORTAL.Controllers
{
    public class DeclarationInfoController : Controller
    {
        // GET: DeclarationInfo
        EPORTALEntities db = new EPORTALEntities();
        public ActionResult Index()
        {
            var res = (from a in db.ThongTinCaNhans
                       join b in db.NhanViens.Where(x => x.ID == MyAuthentication.ID) on a.IDNhanVien equals b.ID
                       join c in db.PhongBans on b.IDPhongBan equals c.IDPhongBan
                       join d in db.Vitris on b.IDViTri equals d.IDViTri
                       select new EmployeesInfo
                       {
                           IDThongTin = a.IDThongTin,
                           IDNhanVien = (int)a.IDNhanVien,
                           MaNV = b.MaNV,
                           HoTen = b.HoTen,
                           IDPhongBan = c.IDPhongBan,
                           PhongBan = c.TenPhongBan,
                           ViTri = d.TenViTri,
                           CMND = a.CMND,
                           CCCD = a.CCCD,
                           FileHinh = a.FileHinh,
                           XacNhan = (bool)a.XacNhan,
                       }).ToList();
            return View(res);

        }
        public ActionResult Create()
        {
            int id = MyAuthentication.ID;

            List<NhanVien> NV = db.NhanViens.Where(x => x.ID == id).ToList();
            ViewBag.MaNVList = new SelectList(NV, "ID", "MaNV");
            ViewBag.HoTenNVList = new SelectList(NV, "ID", "HoTen");
            return PartialView();

        }

        [HttpPost]
        public ActionResult Create(EmployeesInfo _DO)
        {
            if (User.Identity.IsAuthenticated)
            {
                int id = MyAuthentication.ID;

                try
                {
                    string path = Server.MapPath("~/UploadedFiles/DeclarationInfo/");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Use Namespace called :  System.IO  
                    string FileName = _DO.ImageFile != null ? MyAuthentication.Username : "";

                    //To Get File Extension  
                    string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";

                    ////Add Current Date To Attached File Name  
                    if (_DO.ImageFile != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.ImageFile.SaveAs(path + FileName);
                        _DO.FileHinh = "/UploadedFiles/DeclarationInfo/" + FileName;
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Bạn chưa nhập cập nhập hình ảnh');</script>";
                        return RedirectToAction("Index", "DeclarationInfo", new { Areas = "" });
                    }

                    //Check trùng mã MaNV
                    if (IsMNVvailable(id) == false)
                    {
                        //db.ThongTinCaNhan_insert(id, _DO.CMND, _DO.CCCD, _DO.FileHinh, false,DateTime.Now);
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Mã Nhân Viên đã tồn tại');</script>";
                    }
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
                }

                return RedirectToAction("Index", "DeclarationInfo", new { Areas = "" });

            }
            else
            {
                return RedirectToAction("", "Login");
            }
            

        }


        public bool IsMNVvailable(int IDNhanVien)
        {
            var IsCheck = (from t in db.ThongTinCaNhans
                           where (t.IDNhanVien == IDNhanVien)
                           select new { t.IDNhanVien }).FirstOrDefault();
            bool status;
            if (IsCheck != null)
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return status;
        }




        public ActionResult Edit(int id)
        {
            var res = (from a in db.ThongTinCaNhan_searchByID(id)
                       select new EmployeesInfo
                       {
                           IDThongTin = a.IDThongTin,
                           IDNhanVien = (int)a.IDNhanVien,
                           CMND = a.CMND,
                           CCCD = a.CCCD,
                           FileHinh = a.FileHinh,
                           XacNhan = (bool)a.XacNhan,
                       }).ToList();
            EmployeesInfo DO = new EmployeesInfo();

            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDThongTin = co.IDThongTin;
                    DO.IDNhanVien = co.IDNhanVien;
                    DO.CMND = co.CMND;
                    DO.CCCD = co.CCCD;
                    DO.FileHinh = co.FileHinh;
                    DO.XacNhan = co.XacNhan;
                }

                var NVID = db.ThongTinCaNhans.Where(x => x.IDThongTin == id).Select(g => g.IDNhanVien).FirstOrDefault();

                List<NhanVien> NV = db.NhanViens.Where(x => x.ID == NVID).ToList();
                ViewBag.MaNVList = new SelectList(NV, "ID", "MaNV");

                ViewBag.HoTenNVList = new SelectList(NV, "ID", "HoTen");
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]

        public ActionResult Edit(EmployeesInfo _DO)
        {
            if (User.Identity.IsAuthenticated)
            {
                int id = MyAuthentication.ID;
                try
                {
                    string path = Server.MapPath("~/UploadedFiles/DeclarationInfo/");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Use Namespace called :  System.IO  
                    string FileName = _DO.ImageFile != null ? MyAuthentication.Username : "";

                    //To Get File Extension  
                    string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";

                    ////Add Current Date To Attached File Name  
                    if (_DO.ImageFile != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.ImageFile.SaveAs(path + FileName);
                        _DO.FileHinh = "/UploadedFiles/DeclarationInfo/" + FileName;
                    }

                    //db.ThongTinCaNhan_update(_DO.IDThongTin, id, _DO.CMND, _DO.CCCD, _DO.FileHinh, _DO.XacNhan);

                    TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";


                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
                }
                //return View();
                return RedirectToAction("Index", "DeclarationInfo", new { Areas = "" });
            }
            else { return RedirectToAction("", "Login"); }
        }




        public ActionResult Delete(int id)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    db.ThongTinCaNhan_delete(id);
                }    
                    
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "DeclarationInfo", new { Areas = "" });
        }


        public ActionResult Confirm(EmployeesInfo _DO, int id)
        {
            int idnv = MyAuthentication.ID;

            var CMND = db.ThongTinCaNhans.Where(x => x.IDThongTin == id).Select(g => g.CMND).FirstOrDefault();
            var CCCD = db.ThongTinCaNhans.Where(x => x.IDThongTin == id).Select(g => g.CCCD).FirstOrDefault();
            var FileHinh = db.ThongTinCaNhans.Where(x => x.IDThongTin == id).Select(g => g.FileHinh).FirstOrDefault();

            try
            {
                db.ThongTinCaNhan_update(id, idnv, CMND, CCCD, FileHinh, true);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xác nhận thông tin thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "DeclarationInfo", new { Areas = "" });
        }
    }
}