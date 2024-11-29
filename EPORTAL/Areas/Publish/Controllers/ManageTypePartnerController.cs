using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class ManageTypePartnerController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: ManageTypePartner
        public ActionResult Index(int? page)
        {
            var res = (from l in db_context.DB_LoaiDoiTac
                       select new ManageTypePartnerValidation
                       {
                           IDLoaiDT = l.IDLoaiDT,
                           TenLoaiDT = l.TenLoaiDT,
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }



        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Create(ManageTypePartnerValidation _DO)
        {

            try
            {

                db_context.DB_LoaiDoiTac_insert(_DO.TenLoaiDT);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ManageTypePartner");
        }



        public ActionResult Edit(int id)
        {
            var res = (from c in db_context.DB_LoaiDoiTac
                       select new ManageTypePartnerValidation
                       {
                           IDLoaiDT = c.IDLoaiDT,
                           TenLoaiDT = c.TenLoaiDT
                       }).ToList();
            ManageTypePartnerValidation DO = new ManageTypePartnerValidation();

            if (res.Count > 0)
            {
                foreach (var co in res)
                {

                    DO.IDLoaiDT = co.IDLoaiDT;
                    DO.TenLoaiDT = co.TenLoaiDT;

                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Edit(ManageTypePartnerValidation _DO)
        {
            try
            {

                db_context.DB_LoaiDoiTac_update(_DO.IDLoaiDT, _DO.TenLoaiDT);

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + e.Message + " ');</script>";
            }

            return RedirectToAction("Index", "ManageTypePartner");
        }


        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_LoaiDoiTac_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageTypePartner");
        }
    }
}