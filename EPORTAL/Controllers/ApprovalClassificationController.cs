using EPORTAL.Models;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Controllers
{
    public class ApprovalClassificationController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ApprovalClassification";
        // GET: ApprovalClassification
        public ActionResult Index(int? page)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from lkd in db.KD_LoaiKyDuyet
                       select new ApprovalClassification
                       {
                           id = lkd.IDLKD,
                           tenloaikyduyet = lkd.TenLoaiKyDuyet
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
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
            return PartialView();
        }

        [HttpPost]

        public ActionResult Create(ApprovalClassification _DO)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {

                    db.KD_LoaiKyDuyet_insert(_DO.tenloaikyduyet);
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
                }
                return RedirectToAction("Index", "ApprovalClassification", new { Areas = "" });

            }
            else
            {
                return RedirectToAction("", "Login");
            }
        }
        public ActionResult Edit()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from lkd in db.KD_LoaiKyDuyet
                       select new ApprovalClassification
                       {
                           id = lkd.IDLKD,
                           tenloaikyduyet=lkd.TenLoaiKyDuyet
                       }).ToList();
            ApprovalClassification DO = new ApprovalClassification();

            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.id = co.id;
                    DO.tenloaikyduyet = co.tenloaikyduyet;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]

        public ActionResult Edit(ApprovalClassification _DO)
        {
            if (User.Identity.IsAuthenticated)
            {

                try
                {
                    db.KD_LoaiKyDuyet_update(_DO.id, _DO.tenloaikyduyet);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
                }
                //return View();
                return RedirectToAction("Index", "ApprovalClassification", new { Areas = "" });
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
                    db.KD_LoaiKyDuyet_delete(id);
                }

            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ApprovalClassification", new { Areas = "" });
        }
    }
}

   
