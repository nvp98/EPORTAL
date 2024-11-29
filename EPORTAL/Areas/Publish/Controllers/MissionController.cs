using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class MissionController : Controller
    {
        DanhBaDoiTacEntities db = new DanhBaDoiTacEntities();
        // GET: Publish/Mission
        public ActionResult Index(int? page)
        {
            var res = (from m in db.DB_Mission
                       select new MissionValidation
                       {
                           ID = m.ID,
                           TieuDe = m.TieuDe,
                           NoiDung = m.NoiDung
                       }).OrderByDescending(x => x.ID).ToList();
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
        [ValidateInput(false)]
        public ActionResult Create(MissionValidation _DO)
        {

            try
            {

                db.DB_Mission_insert(_DO.TieuDe,_DO.NoiDung);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Mission");
        }
        public ActionResult Edit(int? id)
        {
            var res = (from m in db.DB_Mission.Where(x => x.ID == id)
                       select new MissionValidation
                       {
                           ID = m.ID,
                           TieuDe = m.TieuDe,
                           NoiDung = m.NoiDung
                       }).ToList();
            MissionValidation DO = new MissionValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.ID = co.ID;
                    DO.TieuDe = co.TieuDe;
                    DO.NoiDung = co.NoiDung;
                    
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(MissionValidation _DO)
        {
            try
            {              

                db.DB_Mission_update(_DO.ID, _DO.TieuDe, _DO.NoiDung);
                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Mission");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                db.DB_Mission_delete(id);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "Mission");
        }
    }
}
