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
    public class AccountDTController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: AccountNV
        public ActionResult Index(int? page, string search)
        {
            if (search == null) search = "";
            ViewBag.search = search;

            var res = (from a in db_context.DB_TaiKhoanDT_select(search)

                       select new ManagePartnerValidation
                       {
                           IDDoiTac = a.IDDoiTac,
                           IDBP = a.BPID,
                           ShortName = a.ShortName,
                           FullName = a.FullName,
 
                       });

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));

        }
        public ActionResult Resetpass(int id)
        {
            var res = (from dm in db_context.DB_TaiKhoanDT_ID(id)
                       select new LoginValidation
                       {
                           IDDoiTac = dm.IDDoiTac,
                           IDBP = dm.BPID
                       }).ToList();
            LoginValidation DO = new LoginValidation();
            if (res.Count > 0)
            {
                foreach (var dm in res)
                {
                    DO.IDDoiTac = dm.IDDoiTac;
                    DO.IDBP = dm.IDBP;
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
                db_context.DB_TaiKhoanDT_Update(_DO.IDDoiTac, Encryptor.MD5Hash(_DO.MatKhau));
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AccountDT");
        }

        public ActionResult Edit(int id)
        {
            List<DB_Quyen> pq = db_context.DB_Quyen.ToList();
            ViewBag.PList = new SelectList(pq, "IDQuyen", "TenQuyen");

            List<DB_LoaiDoiTac> dn = db_context.DB_LoaiDoiTac.ToList();
            ViewBag.DNList = new SelectList(dn, "IDLoaiDT", "TenLoaiDT");

            var res = (from a in db_context.DB_TaiKhoanDT_ID(id)
                       select new LoginValidation
                       {
                           IDDoiTac = a.IDDoiTac,
                           IDBP = a.BPID,
                           ShortName = a.ShortName,
                           IDQuyen = (int)a.IDQuyen
                       }).ToList();
            LoginValidation DO = new LoginValidation();
            if (res.Count > 0)
            {
                foreach (var nv in res)
                {
                    DO.IDDoiTac = nv.IDDoiTac;
                    DO.IDBP = nv.IDBP;
                    DO.ShortName = nv.ShortName;
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

                db_context.DB_TKDT_Update_Quyen(_DO.IDDoiTac, _DO.IDQuyen);
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại:" + e.Message + "');</script>";
            }

            return RedirectToAction("Index", "AccountDT");
        }

    }
}