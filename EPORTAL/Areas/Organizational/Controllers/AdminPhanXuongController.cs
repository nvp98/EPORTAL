using PagedList;
using SoDoToChuc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsOrganizational;
using EPORTAL.Models;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public class AdminPhanXuongController : Controller
    {
        // GET: AdminPhanXuong
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "AdminPhanXuong";
        public ActionResult Index(int? page, int? IDPhongBan, int? IDPhanXuong)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<PhongBan> pb1 = db_context.PhongBans.ToList();
            if (IDPhongBan == null) IDPhongBan = 0;
            ViewBag.PBList = new SelectList(pb1, "IDPhongBan", "TenPhongBan", IDPhongBan);

            List<PhanXuong> px1 = db_context.PhanXuongs.ToList();
            if (IDPhanXuong == null) IDPhanXuong = 0;
            ViewBag.PXList = new SelectList(px1, "IDPhanXuong", "TenPhanXuong", IDPhanXuong);

            var res = (from a in db_context.PhanXuongs
                       join pb in db_context.PhongBans on a.PhongBanID equals pb.IDPhongBan into ulist from pb in ulist.DefaultIfEmpty()
                       select new AdminPhanXuong
                       { 
                           PhanXuongID =a.IDPhanXuong,
                            TenPhanXuong =a.TenPhanXuong,
                           PhongBanID = (int?)a.PhongBanID ?? 0,
                           TenPhongBan = pb.TenPhongBan,
                           KTVDB = (int?)a.KTVDB??0 ,
                            PTDB =(int?)a.PTDB??0 ,
                            TPDB =(int?)a.TPDB??0,
                            NVDB =(int?)a.NVDB??0,
                           KTVTT = (int?)a.KTVTT ?? 0,
                           PTTT =  (int?)a.NVHCDB??0,
                           TPTT = (int?)a.TPTT ?? 0,
                           NVTT = (int?)a.NVTT ?? 0,
                           TPKip =(int?)a.TPKip??0,
                           TTTP =(int?)a.TTTPDB??0
                       });
            if (IDPhongBan != 0)
                res = res.Where(x => x.PhongBanID == IDPhongBan);
            if (IDPhanXuong != 0)
                res = res.Where(x => x.PhanXuongID == IDPhanXuong);
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public int? ToNullableInt(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<PhongBan> pb = db_context.PhongBans.ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(AdminPhanXuong _DO)
        {

            try
            {
                var a = db_context.PhanXuong_insert_Excel(_DO.TenPhanXuong, _DO.PhongBanID, ToNullableInt(_DO.TPDB.ToString()), ToNullableInt(_DO.PTDB.ToString()), ToNullableInt(_DO.TPKip.ToString()), ToNullableInt(_DO.KTVDB.ToString()), ToNullableInt(_DO.NVDB.ToString()), ToNullableInt(_DO.PTTT.ToString()), ToNullableInt(_DO.TTTP.ToString()));
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminPhanXuong");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db_context.PhanXuongs.Where(x => x.IDPhanXuong == id)
                       select new AdminPhanXuong
                       {
                           TenPhanXuong = a.TenPhanXuong,
                           PhongBanID = (int?)a.PhongBanID??0,
                           PTDB =(int?)a.PTDB??0,
                           TPDB =(int?)a.TPDB??0,
                           KTVDB =(int?)a.KTVDB??0,
                           NVDB =(int?)a.NVDB??0,
                           PTTT =  (int?)a.NVHCDB??0,
                           TPTT = (int?)a.TPTT ?? 0,
                           KTVTT = (int?)a.KTVTT ?? 0,
                           NVTT = (int?)a.NVTT ?? 0,
                           PhanXuongID =a.IDPhanXuong,
                           TPKip =(int?)a.TPKip??0,
                           TTTP =(int?)a.TTTPDB??0
                       }).ToList();
            AdminPhanXuong DO = new AdminPhanXuong();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.TenPhanXuong = a.TenPhanXuong;
                    DO.PhongBanID = a.PhongBanID;
                    DO.PTDB = a.PTDB;
                    DO.TPDB = a.TPDB;
                    DO.KTVDB = a.KTVDB;
                    DO.NVDB = a.NVDB;
                    DO.PTTT = a.PTTT;
                    DO.TPTT = a.TPTT;
                    DO.KTVTT = a.KTVTT;
                    DO.NVTT = a.NVTT;
                    DO.PhanXuongID = a.PhanXuongID;
                    DO.TPKip = a.TPKip;
                    DO.TTTP = a.TTTP;
                }

                List<PhongBan> pb = db_context.PhongBans.ToList();
                ViewBag.PhongBanID = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.PhongBanID);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AdminPhanXuong _DO)
        {

            try
            {
                var a = db_context.PhanXuong_update_Excel(_DO.PhanXuongID, _DO.TenPhanXuong, _DO.PhongBanID, ToNullableInt(_DO.TPDB.ToString()), ToNullableInt(_DO.PTDB.ToString()), ToNullableInt(_DO.TPKip.ToString()), ToNullableInt(_DO.KTVDB.ToString()), ToNullableInt(_DO.NVDB.ToString()), ToNullableInt(_DO.PTTT.ToString()), ToNullableInt(_DO.TTTP.ToString()));
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminPhanXuong", new { @IDPhongBan = _DO.PhongBanID });
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
                db_context.PhanXuong_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AdminPhanXuong");
        }
    }
}