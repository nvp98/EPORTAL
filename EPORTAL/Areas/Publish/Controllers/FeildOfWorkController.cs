using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class FeildOfWorkController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: FeildOfWork
        public ActionResult Index(int? page)
        {
            var res = (from d in db_context.DB_LinhVucHoatDong
                       select new FeildOfWorkValidation
                       {
                           IDLinhVuc = d.IDLinhVucHĐ,
                           TenLinhVucHĐ = d.TenLinhVucHĐ
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FeildOfWorkValidation _DO)
        {
            db_context.DB_LinhVucHoatDong_insert(_DO.TenLinhVucHĐ);
            TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            return RedirectToAction("Index", "FeildOfWork");
        }
        public ActionResult Edit(int id)
        {
            var res = (from a in db_context.DB_LinhVucHoatDong_searchByID(id)
                       select new FeildOfWorkValidation
                       {
                           IDLinhVuc = a.IDLinhVucHĐ,
                           TenLinhVucHĐ = a.TenLinhVucHĐ

                       }).ToList();
            FeildOfWorkValidation DO = new FeildOfWorkValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDLinhVuc = co.IDLinhVuc;
                    DO.TenLinhVucHĐ = co.TenLinhVucHĐ;
                }
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Edit(FeildOfWorkValidation _DO)
        {
            try
            {

                db_context.DB_LinhVucHoatDong_update(_DO.IDLinhVuc, _DO.TenLinhVucHĐ);

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + e.Message + " ');</script>";
            }

            return RedirectToAction("Index", "FeildOfWork");
        }
        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_LinhVucHoatDong_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "FeildOfWork");
        }
    }

}