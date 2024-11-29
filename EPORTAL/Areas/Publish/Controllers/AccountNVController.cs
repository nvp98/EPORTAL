using EPORTAL.Common;
using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class AccountNVController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: AccountNV
        public ActionResult Index(int? page, string search)
        {
            if (search == null) search = "";
            ViewBag.search = search;

            var res = from a in db_context.DB_TaiKhoanNV_select(search)
                      select new EmployeesValidation
                      {
                          ID = a.ID,
                          HoTen = a.HoTen,
                          MaNV = a.MaNV,
                          PhongBan = a.TenPhongBan,
                          TenQuyen = a.TenQuyen
                      };

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));

        }
        public ActionResult Resetpass(int id)
        {
            var res = (from dm in db_context.DB_TaiKhoanNV_ID(id)
                       select new LoginValidation
                       {
                           ID = dm.ID,
                           HoTen = dm.HoTen
                       }).ToList();
            LoginValidation DO = new LoginValidation();
            if (res.Count > 0)
            {
                foreach (var dm in res)
                {
                    DO.ID = dm.ID;
                    DO.HoTen = dm.HoTen;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Resetpass(LoginValidation _DO)
        {
            try
            {
                db_context.DB_TaiKhoanNV_Update(_DO.ID, Encryptor.MD5Hash(_DO.MatKhau));
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AccountNV");
        }

        public ActionResult Edit(int id)
        {
            List<DB_Quyen> pq = db_context.DB_Quyen.ToList();
            ViewBag.PList = new SelectList(pq, "IDQuyen", "TenQuyen");

            List<DB_PhongBan> dn = db_context.DB_PhongBan.ToList();
            ViewBag.DNList = new SelectList(dn, "IDDN", "TenDN");

            var res = (from a in db_context.DB_TaiKhoanNV_ID(id)
                       select new LoginValidation
                       {
                           ID = a.ID,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           IDQuyen = (int)a.IDQuyen
                       }).ToList();
            LoginValidation DO = new LoginValidation();
            if (res.Count > 0)
            {
                foreach (var nv in res)
                {
                    DO.ID = nv.ID;
                    DO.MaNV = nv.MaNV;
                    DO.HoTen = nv.HoTen;
                    DO.IDQuyen = nv.IDQuyen;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Edit(LoginValidation _DO)
        {
            try
            {

                db_context.DB_TKNV_Update_Quyen(_DO.ID, _DO.IDQuyen);
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại:" + e.Message + "');</script>";
            }

            return RedirectToAction("Index", "AccountNV");
        }

    }
}