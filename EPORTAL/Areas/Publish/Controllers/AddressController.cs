using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class AddressController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: Address
        public ActionResult Index(int? page)
        {
            var res = (from a in db_context.DB_DiaChi
                       select new AddressValidation
                       {
                           DiaChi = a.DiaChi,
                           KhuVuc = a.Mien
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
        public ActionResult Create(AddressValidation _DO)
        {
            db_context.DB_DiaChi_insert(_DO.DiaChi, _DO.KhuVuc);
            TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            return RedirectToAction("Index", "Address");
        }

        public ActionResult Edit()
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            var res = (from a in db_context.DB_DiaChi
                       select new AddressValidation
                       {
                           IDDiaChi = a.IDDiaChi,
                           DiaChi = a.DiaChi,
                           KhuVuc = a.Mien,

                       }).ToList();
            AddressValidation DO = new AddressValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDDiaChi = co.IDDiaChi;
                    DO.DiaChi = co.DiaChi;
                    DO.KhuVuc = co.KhuVuc;
                }
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Edit(AddressValidation _DO)
        {
            try
            {

                db_context.DB_DiaChi_update(_DO.IDDiaChi, _DO.DiaChi, _DO.KhuVuc);

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + e.Message + " ');</script>";
            }

            return RedirectToAction("Index", "Address");
        }
        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_DiaChi_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "WorkItem");
        }
    }

}