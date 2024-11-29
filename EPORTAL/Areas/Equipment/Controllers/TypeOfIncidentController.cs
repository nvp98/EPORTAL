using EPORTAL.Models;
using EPORTAL.ModelsEquipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Equipment.Controllers
{
    public class TypeOfIncidentController : Controller
    {
        EquipmentEntities db = new EquipmentEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "TypeOfIncident";
        // GET: Equipment/TypeOfIncident
        public ActionResult Index()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var model = db.NV_LoiSuCoTB.ToList();
            return View(model);
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD_AD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(NV_LoiSuCoTB _DO)
        {

            try
            {
                var a = db.NV_LoiSuCoTB_insert(_DO.TenLoiSC);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "TypeOfIncident");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT_AD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NV_LoiSuCoTB.Where(x => x.IDSC == id)
                       select new NV_LoiSuCoTBValidation
                       {
                           IDSC = (int)a.IDSC,
                           TenLoiSC = a.TenLoiSC,
                       }).ToList();
            NV_LoiSuCoTBValidation DO = new NV_LoiSuCoTBValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDSC = (int)a.IDSC;
                    DO.TenLoiSC = a.TenLoiSC;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(NV_LoiSuCoTBValidation _DO)
        {

            try
            {
                var a = db.NV_LoiSuCoTB_update(_DO.IDSC, _DO.TenLoiSC);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "TypeOfIncident");
        }
        public ActionResult Delete(int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.NV_LoiSuCoTB_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "TypeOfIncident");
        }
    }
}