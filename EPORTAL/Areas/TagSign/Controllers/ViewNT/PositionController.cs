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
    public class PositionController : Controller
    {
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTALEntities db = new EPORTALEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Position";

        // GET: TagSign/Position
        public ActionResult Index(int? page)
        {
            
            var res = from a in db.NT_Position
                      select new DataInformationValidation
                      {
                          IDCV = (int)a.IDCV,
                          TenCV = a.TenCV,
                          Phone =(int?)a.Phone??default
                      };
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDCV).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Index_HPDQ(int? page, string search)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var res = (from a in db.NT_Position
                      select new DataInformationValidation
                      {
                          IDCV = (int)a.IDCV,
                          TenCV = a.TenCV,
                          Phone = (int?)a.Phone ?? default,
                          ChuVuTQ = a.ChuVuTQ
                      }).ToList();
            if(search != null)
            {
                res = res.Where(x=>x.TenCV.Contains(search)).ToList();
            }
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDCV).ToList().ToPagedList(pageNumber, pageSize));
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
                NT_Position position = new NT_Position();
                position.TenCV = _DO.TenCV;
                position.Phone = _DO.Phone;
                position.ChuVuTQ = _DO.ChuVuTQ;
                db.NT_Position.Add(position);
                db.SaveChanges();
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index_HPDQ", "Position");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_Position.Where(x=>x.IDCV == id)
                      select new DataInformationValidation
                      {
                          IDCV = (int)a.IDCV,
                          TenCV = a.TenCV,
                          Phone = (int?)a.Phone ?? default,
                          ChuVuTQ = a.ChuVuTQ
                      }).ToList();
            DataInformationValidation DO = new DataInformationValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDCV = (int)a.IDCV;
                    DO.TenCV = a.TenCV;
                    DO.Phone = (int?)a.Phone ?? default;
                    DO.ChuVuTQ = a.ChuVuTQ;
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
                var position = db.NT_Position.Where(x => x.IDCV == _DO.IDCV).FirstOrDefault();
                if (position != null)
                {
                    position.TenCV = _DO.TenCV;
                    position.Phone = position.Phone;
                    position.ChuVuTQ = _DO.ChuVuTQ;
                   
                }
                db.SaveChanges();
                //var a = db.NT_Gate_update(_DO.IDGate, _DO.Gate, _DO.IDTinhTrang);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index_HPDQ", "Position");
        }
    }
}