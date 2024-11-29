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
    public class AdminNhomController : Controller
    {
        // GET: AdminNhom
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "AdminNhom";
        public ActionResult Index(int?page, int? nhomID, int? IDPhongBan)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NhomLV> nhom = db_context.NhomLVs.ToList();
            if (IDPhongBan != null) nhom = nhom.Where(x => x.IDPhongBan == IDPhongBan).ToList();
            if (nhomID == null) nhomID = 0;
            ViewBag.NhomList = new SelectList(nhom, "IDNhom", "TenNhom", nhomID);
            List<PhongBan> pb1 = db_context.PhongBans.ToList();
            if (IDPhongBan == null) IDPhongBan = 0;
            ViewBag.PBList = new SelectList(pb1, "IDPhongBan", "TenPhongBan", IDPhongBan);

            var res = (from a in db_context.NhomLVs
                       join b in db_context.PhongBans on a.IDPhongBan equals b.IDPhongBan into ulist from b in ulist.DefaultIfEmpty()
                       select new AdminNhom
                       {
                           IDNhom = a.IDNhom,
                           TenNhom = a.TenNhom,
                           IDPhongBan =(int?)a.IDPhongBan??0,
                           TenPhongBan =b.TenPhongBan,
                           //SLKTV =(int?)a.SLKTV??0,
                           PTDB =(int?)a.PTDB??0,
                           KTVDB =(int?)a.KTVDB??0,
                           NVDB =(int?)a.NVDB??0,
                           KTVCa =(int?)a.KTVCa??0,
                           NVCa =(int?)a.NVCa??0,
                           TPTDB =(int?)a.TPTDB??0,
                           TPKip =(int?)a.TPKipDB??0,
                           TTTPDB =(int?)a.TTTPDB??0
                       });
            if (nhomID != 0)
                res = res.Where(x => x.IDNhom == nhomID);
            if (IDPhongBan != 0)
                res = res.Where(x => x.IDPhongBan == IDPhongBan);
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
        public ActionResult Create(AdminNhom _DO)
        {

            try
            {
                var a = db_context.Nhom_insert(_DO.TenNhom,_DO.IDPhongBan, ToNullableInt(_DO.TPTDB.ToString()), ToNullableInt(_DO.PTDB.ToString()), ToNullableInt(_DO.TPKip.ToString()), ToNullableInt(_DO.KTVDB.ToString()), ToNullableInt(_DO.NVDB.ToString()), ToNullableInt(_DO.KTVCa.ToString()), ToNullableInt(_DO.NVCa.ToString()), ToNullableInt(_DO.TTTPDB.ToString()));
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminNhom");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db_context.NhomLVs.Where(x => x.IDNhom == id)
                       join b in db_context.PhongBans on a.IDPhongBan equals b.IDPhongBan into ulist from b in ulist.DefaultIfEmpty()
                       select new AdminNhom
                       {
                           TenNhom = a.TenNhom,
                           IDNhom =a.IDNhom,
                           IDPhongBan =(int?)a.IDPhongBan??0,
                           TenPhongBan = b.TenPhongBan,
                           //SLKTV =(int?)a.SLKTV??0,
                           PTDB = (int?)a.PTDB ?? 0,
                           KTVDB = (int?)a.KTVDB ?? 0,
                           NVDB = (int?)a.NVDB ?? 0,
                           KTVCa =(int?)a.KTVCa??0,
                           NVCa=(int?)a.NVCa??0,
                           TPTDB =(int?)a.TPTDB??0,
                           TPKip =(int?)a.TPKipDB??0,
                           TTTPDB =(int?)a.TTTPDB??0
                       }).ToList();
            AdminNhom DO = new AdminNhom();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.TenNhom= a.TenNhom;
                    DO.IDNhom = a.IDNhom;
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.TenPhongBan = a.TenPhongBan;
                    DO.SLKTV = a.SLKTV;
                    DO.PTDB = a.PTDB;
                    DO.KTVDB = a.KTVDB;
                    DO.NVDB = a.NVDB;
                    DO.KTVCa = a.KTVCa;
                    DO.NVCa = a.NVCa;
                    DO.TPTDB = a.TPTDB;
                    DO.TPKip = a.TPKip;
                    DO.TTTPDB = a.TTTPDB;
                }

                List<PhongBan> pb = db_context.PhongBans.ToList();
                ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AdminNhom _DO)
        {

            try
            {
                var a = db_context.Nhom_update(_DO.IDNhom,_DO.TenNhom,_DO.IDPhongBan, ToNullableInt(_DO.TPTDB.ToString()), ToNullableInt(_DO.PTDB.ToString()), ToNullableInt(_DO.TPKip.ToString()), ToNullableInt(_DO.KTVDB.ToString()), ToNullableInt(_DO.NVDB.ToString()), ToNullableInt(_DO.KTVCa.ToString()), ToNullableInt(_DO.NVCa.ToString()), ToNullableInt(_DO.TTTPDB.ToString()));
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminNhom", new { @IDPhongBan = _DO.IDPhongBan });
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
                db_context.Nhom_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AdminNhom");
        }



    }
}