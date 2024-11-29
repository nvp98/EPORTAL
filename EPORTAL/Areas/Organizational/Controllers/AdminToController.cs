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
    public class AdminToController : Controller
    {
        // GET: AdminTo
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "AdminTo";
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
            if (IDPhongBan != 0)
                px1 = px1.Where(x => x.PhongBanID == IDPhongBan).ToList();
            if (IDPhanXuong == null) IDPhanXuong = 0;
            ViewBag.PXList = new SelectList(px1, "IDPhanXuong", "TenPhanXuong", IDPhanXuong);

            var res = (from a in db_context.ToLVs
                       join px in db_context.PhanXuongs on a.PhanXuongID equals px.IDPhanXuong into ulist from px in ulist.DefaultIfEmpty()
                       join pb in db_context.PhongBans on a.PhongBanID equals pb.IDPhongBan
                       select new AdminTo
                       {
                           TenTo = a.TenTo,
                           PhanXuongID = (int?)a.PhanXuongID??0,
                           TenPhanXuong = px.TenPhanXuong,
                           PhongBanID =(int)a.PhongBanID,
                           TenPhongBan = pb.TenPhongBan,
                           IDTo =a.IDTo,
                           SLDB =(int?)a.SLDB??0,
                           SLTT =0
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
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<ToLV> to = db_context.ToLVs.ToList();
            ViewBag.ToList = new SelectList(to, "IDTo", "TenTo");

            List<PhanXuong> px = db_context.PhanXuongs.ToList();
            ViewBag.PXList = new SelectList(px, "IDPhanXuong", "TenPhanXuong");

            List<PhongBan> pb = db_context.PhongBans.Where(x=>x.NhomID ==2).ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(AdminTo _DO)
        {

            try
            {
                var a = db_context.ToLV_insert(_DO.TenTo, _DO.PhanXuongID, _DO.PhongBanID, _DO.SLDB,null);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminTo");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db_context.ToLVs.Where(x => x.IDTo == id)
                       select new AdminTo
                       {
                           TenTo = a.TenTo,
                           PhanXuongID = (int)a.PhanXuongID,
                           PhongBanID = (int)a.PhongBanID,
                           SLDB = (int?)a.SLDB??0,
                           SLTT = 0,
                           IDTo = a.IDTo,
                       }).ToList();
            AdminTo DO = new AdminTo();
            if(res.Count >0)
            {
                foreach (var a in res)
                {
                    DO.TenTo = a.TenTo;
                    DO.PhanXuongID = a.PhanXuongID;
                    DO.PhongBanID = a.PhongBanID;
                    DO.SLDB = a.SLDB;
                    DO.SLTT = a.SLTT;
                    DO.IDTo = a.IDTo;
                }
                List<PhanXuong> px = db_context.PhanXuongs.Where(x=>x.PhongBanID ==DO.PhongBanID).ToList();
                ViewBag.PhanXuongID = new SelectList(px, "IDPhanXuong", "TenPhanXuong",DO.PhanXuongID);

                List<PhongBan> pb = db_context.PhongBans.Where(x => x.NhomID == 2).ToList();
                ViewBag.PhongBanID = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.PhongBanID);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AdminTo _DO)
        {

            try
            {
                var a = db_context.ToLV_update(_DO.TenTo, _DO.PhanXuongID, _DO.PhongBanID, _DO.SLDB,null,_DO.IDTo);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminTo", new { @IDPhongBan = _DO.PhongBanID });
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
                db_context.ToLV_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AdminTo");
        }
        public JsonResult GetPXList(int id)
        {
            List<PXList> PXList = (from b in db_context.PhanXuongs.Where(x => x.PhongBanID == id)
                                     select new PXList()
                                     {
                                         IDPX = b.IDPhanXuong,
                                         TenPX = b.TenPhanXuong
                                     }).ToList();

            return Json(PXList, JsonRequestBehavior.AllowGet);
        }

    }
}