using EPORTAL.Models;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign
{
    public class GateController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Gate";
        // GET: TagSign/Gate
        public ActionResult Index(int? page, string search)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_Gate
                       select new GateValidation
                       {
                           IDGate = (int)a.IDGate,
                           Gate = a.Gate,
                           IDTinhTrang = a.TinhTrang,
                           TinhTrang = (a.TinhTrang == 0) ? "Đóng" : "Mở",
                       }).ToList();
            if(search != null)
            {
                res = res.Where(x => x.Gate.Contains(search)).ToList();
            }
                
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDGate).ToList().ToPagedList(pageNumber, pageSize));
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
        public ActionResult Create(GateValidation _DO)
        {

            try
            {
                NT_Gate gate = new NT_Gate();
                gate.Gate = _DO.Gate;
                db.NT_Gate.Add(gate);
                db.SaveChanges();
                //var a = db.NT_Gate_insert(_DO.Gate, 1);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Gate");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            GateValidation DO = db.NT_Gate.Where(x => x.IDGate == id).Select(x => new GateValidation() { IDGate = x.IDGate, Gate = x.Gate, IDTinhTrang = x.TinhTrang }).SingleOrDefault();

            if (DO != null)
            {
                return PartialView(DO);
            }
            else
            {
                return HttpNotFound();
            }


        }
        [HttpPost]
        public ActionResult Edit(GateValidation _DO)
        {

            try
            {

                var position = db.NT_Gate.Where(x => x.IDGate == _DO.IDGate).FirstOrDefault();
                if (position != null)
                {
                    position.Gate = _DO.Gate;

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
            return RedirectToAction("Index", "Gate");
        }
    }
}