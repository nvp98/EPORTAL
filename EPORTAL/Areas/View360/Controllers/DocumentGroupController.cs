using EPORTAL.Models;
using EPORTAL.ModelsEquipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    public class DocumentGroupController : Controller
    {
        PhanQuyenHTEntities db = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "DocumentGroup";
        // GET: View360/DocumentGroup
        public ActionResult Index()
        {
            var check = db.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var model = db.L_NhomThuVienFile.ToList();
            return View(model);
        }
        public ActionResult Create()
        {
            var check = db.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(L_NhomThuVienFileValidation _DO)
        {

            try
            {
                var a = db.L_NhomThuVienFile_insert(_DO.TenNhomTV);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "DocumentGroup");
        }
        public ActionResult Edit(int id)
        {
            var check = db.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.L_NhomThuVienFile.Where(x => x.IDNhom == id)
                       select new L_NhomThuVienFileValidation
                       {
                           IDNhom = (int)a.IDNhom,
                           TenNhomTV = a.TenNhomTV,
                       }).ToList();
            L_NhomThuVienFileValidation DO = new L_NhomThuVienFileValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDNhom = (int)a.IDNhom;
                    DO.TenNhomTV = a.TenNhomTV;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(L_NhomThuVienFileValidation _DO)
        {

            try
            {
                var a = db.L_NhomThuVienFile_update(_DO.IDNhom, _DO.TenNhomTV);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "DocumentGroup");
        }
        public ActionResult Delete(int? id)
        {
            var check = db.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.L_NhomThuVienFile_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "DocumentGroup");
        }
    }
}