using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class WorkPlaceController : Controller
    {
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTALEntities db = new EPORTALEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "WorkPlace";
        // GET: TagSign/WorkPlace
        public ActionResult Index(int? page)
        {

            var res = from a in db.NT_Workplace
                      select new DataInformationValidation
                      {
                          IDKV = (int)a.IDKV,
                          TenKV = a.TenKV
                      };
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDKV).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Index_HPDQ(int? page, string search)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var res = (from a in db.NT_Workplace
                      select new DataInformationValidation
                      {
                          IDKV = (int)a.IDKV,
                          TenKV = a.TenKV
                      }).ToList();
            if(search != null)
            {
                res = res.Where(x=>x.TenKV.Contains(search)).ToList();   
            }
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;

            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDKV).ToList().ToPagedList(pageNumber, pageSize));
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
        public ActionResult Create(DataInformationValidation _DO)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            try
            {
                NT_Workplace workplace = new NT_Workplace();
                workplace.TenKV = _DO.TenKV;
                db.NT_Workplace.Add(workplace);
                db.SaveChanges();
                //var a = db.NT_Gate_insert(_DO.Gate, 1);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index_HPDQ", "WorkPlace");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_Workplace.Where(x => x.IDKV == id)   
                      select new DataInformationValidation
                      {
                          IDKV = (int)a.IDKV,
                          TenKV = a.TenKV
                      }).ToList();
            DataInformationValidation DO = new DataInformationValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDKV = (int)a.IDKV;
                    DO.TenKV = a.TenKV;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);


        }
        [HttpPost]
        public ActionResult Edit(DataInformationValidation _DO)
        {

            try
            {
                var Workplace = db.NT_Workplace.Where(x => x.IDKV == _DO.IDKV).FirstOrDefault();
                if (Workplace != null)
                {
                    Workplace.TenKV = _DO.TenKV;
                }
                db.SaveChanges();
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index_HPDQ", "WorkPlace");
        }
    }
}