using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class ManageBenefitController : Controller
    {
        DanhBaDoiTacEntities db = new DanhBaDoiTacEntities();
        // GET: Publish/ManageBenefit
        public ActionResult Index(int? page)
        {
            var res = (from b in db.DB_Benefit
                       select new EmployeBenefitValidation
                       {
                           ID = b.ID,
                           TieuDe = b.TieuDe,
                           NoiDung = b.NoiDung, 
                           HinhAnh = b.HinhAnh
                       }).ToList();
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
        public ActionResult Create(EmployeBenefitValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/UploadedFiles/UploadedFiles/Benefits/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string FileName = _DO.NameFile != null ? _DO.TieuDe + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                if (_DO.NameFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NameFile.SaveAs(path + FileName);
                    _DO.HinhAnh = "/UploadedFiles/UploadedFiles/Benefits/" + FileName;
                }


                db.DB_Benefit_insert(_DO.TieuDe, _DO.NoiDung, _DO.HinhAnh);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageBenefit");
        }
        public ActionResult Edit(int? id)
        {
            var res = (from b in db.DB_Benefit.Where(x => x.ID == id)
                       select new EmployeBenefitValidation
                       {
                           ID = b.ID,
                           TieuDe = b.TieuDe,
                           NoiDung = b.NoiDung,
                           HinhAnh = b.HinhAnh
                       }).ToList();
            EmployeBenefitValidation DO = new EmployeBenefitValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.ID = co.ID;
                    DO.TieuDe = co.TieuDe;
                    DO.NoiDung = co.NoiDung;
                    DO.HinhAnh = co.HinhAnh;
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
        public ActionResult Edit(EmployeBenefitValidation _DO)
        {
            try
            {
                string path = Server.MapPath("~/UploadedFiles/UploadedFiles/Benefits/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string FileName = _DO.NameFile != null ? _DO.TieuDe + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                if (_DO.NameFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NameFile.SaveAs(path + FileName);
                    _DO.HinhAnh = "/UploadedFiles/UploadedFiles/Benefits/" + FileName;
                }


                db.DB_Benefit_update(_DO.ID, _DO.TieuDe, _DO.NoiDung, _DO.HinhAnh);
                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageBenefit");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                db.DB_Benefit_delete(id);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageBenefit");
        }
    }
}
