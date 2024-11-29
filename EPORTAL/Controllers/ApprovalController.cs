using EPORTAL.ModelsView360;
using EPORTAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Approve = EPORTAL.Models.Approve;
using PagedList;

namespace EPORTAL.Controllers
{
    public class ApprovalController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Approval";
        // GET: Approval
        public ActionResult Index(int? page, string search)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;

            /*var res = (from kd in db.KD_KyDuyet
                       join nv in db.NhanViens on kd.IDNhanVien equals nv.ID
                       join lkd in db.KD_LoaiKyDuyet on kd.IDLoaiKD equals lkd.IDLKD
                       select new Approve
                       {
                           idnv = nv.ID,
                           id = (int)kd.IDKD,
                           idnhanvien = (int)nv.ID,
                           ghichu = kd.GhiChu,
                           idloaikd = (int)kd.IDLoaiKD,
                           manv = nv.MaNV,
                           hoten = nv.HoTen,
                           tenloaikyduyet = lkd.TenLoaiKyDuyet
                       }).ToList();*/
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            //return View(res.ToList().ToPagedList(pageNumber, pageSize));
            return View(SearchNV(search).ToPagedList(pageNumber, pageSize));
        }
        List<Approve> SearchNV (string search)
        {
            if(search == "" || search == null)
            {
                var res = (from kd in db.AuthorizationContractors
                           join nv in db.NhanViens on kd.IDNhanVien equals nv.ID
                           join lkd in db.KD_LoaiKyDuyet on kd.IDLKD equals lkd.IDLKD
                           select new Approve
                           {
                               idnv = nv.ID,
                               id = (int)kd.ID,
                               idnhanvien = (int)nv.ID,                             
                               idloaikd = (int)kd.IDLKD,
                               manv = nv.MaNV,
                               hoten = nv.HoTen,
                               tenloaikyduyet = lkd.TenLoaiKyDuyet
                           });
                return res.ToList();
            }
            else
            {
                var res = (from kd in db.AuthorizationContractors
                           join nv in db.NhanViens.Where(x=>x.MaNV.Contains(search) || x.HoTen.Contains(search))  on kd.IDNhanVien equals nv.ID
                           join lkd in db.KD_LoaiKyDuyet on kd.IDLKD equals lkd.IDLKD
                           select new Approve
                           {
                               idnv = nv.ID,
                               id = (int)kd.ID,
                               idnhanvien = (int)nv.ID,
                               idloaikd = (int)kd.IDLKD,
                               manv = nv.MaNV,
                               hoten = nv.HoTen,
                               tenloaikyduyet = lkd.TenLoaiKyDuyet
                           });
                return res.ToList();
            }     
        }
        //public ActionResult Create()
        //{
        //    List<NhanVien> nv = db.NhanViens.ToList();
        //    ViewBag.NVList = new SelectList(nv, "ID", "MaNV", "HoTen");
        //    List<KD_LoaiKyDuyet> lkd = db.KD_LoaiKyDuyet.ToList();
        //    ViewBag.LKDList = new SelectList(lkd, "IDLKD", "TenLoaiKyDuyet");
        //    return PartialView();
        //}

        //[HttpPost]

        //public ActionResult Create(Approve _DO)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        try
        //        {
        //            if (CheckIDNV(_DO.idnhanvien) == false)
        //            {
        //                db.KD_KyDuyet_insert(_DO.id, _DO.idnhanvien, _DO.ghichu, _DO.idloaikd);
        //                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
        //            }
        //            else
        //            {
        //                TempData["msgSuccess"] = "<script>alert('Nhân viên đã tồn tại');</script>";
        //            }

        //            TempData["msgSuccesss"] = "<script>alert('Nhân viên đã tồn tại');</script>";
        //        }
        //        catch (Exception e)
        //        {
        //            TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
        //        }
        //        return RedirectToAction("Index", "Approval", new { Areas = "" });

        //    }
        //    else
        //    {
        //        return RedirectToAction("", "Login");
        //    }
        //}
        public ActionResult Edit(int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from kd in db.AuthorizationContractors_searchByID(id)
                       join nv in db.NhanViens on kd.IDNhanVien equals nv.ID
                       join lkd in db.KD_LoaiKyDuyet on kd.IDLKD equals lkd.IDLKD
                       select new Approve
                       {
                           idnv = nv.ID,
                           id = (int)kd.ID,
                           idnhanvien = (int)nv.ID,
                           
                           idloaikd = (int)kd.IDLKD,
                           manv = nv.MaNV,
                           hoten = nv.HoTen,
                           tenloaikyduyet = lkd.TenLoaiKyDuyet
                       }).ToList();
            Approve DO = new Approve();

            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.id = co.id;
                    DO.idnhanvien = co.idnhanvien;
                    //DO.ghichu = co.ghichu;
                    DO.idloaikd = co.idloaikd;


                    List<NhanVien> nv = db.NhanViens.ToList();
                    ViewBag.NVList = new SelectList(nv, "ID", "MaNV", "HoTen");
                    List<KD_LoaiKyDuyet> lkd = db.KD_LoaiKyDuyet.ToList();
                    ViewBag.LKDList = new SelectList(lkd, "IDLKD", "TenLoaiKyDuyet");
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]

        public ActionResult Edit(Approve _DO)
        {
            if (User.Identity.IsAuthenticated)
            {

                try
                {
                    
                        db.AuthorizationContractorsLKD_update(_DO.id, _DO.idloaikd);
                        TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";                  
                   
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
                }
                //return View();
                return RedirectToAction("Index", "Approval", new { Areas = "" });
            }
            else { return RedirectToAction("", "Login"); }
        }
        public ActionResult Delete(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    db.AuthorizationContractors_delete(id);
                }

            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Approval", new { Areas = "" });
        }
        public bool CheckIDNV(int id)
        {
            var IsCheck = (from kd in db.KD_KyDuyet
                           join nv in db.NhanViens on kd.IDNhanVien equals nv.ID
                           join lkd in db.KD_LoaiKyDuyet on kd.IDLoaiKD equals lkd.IDLKD
                           where (nv.ID ==id)
                           select new { nv.ID}).FirstOrDefault();
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

    }

}