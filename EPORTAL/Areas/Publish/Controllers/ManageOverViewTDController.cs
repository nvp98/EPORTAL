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
    public class ManageOverViewTDController : Controller
    {
        DanhBaDoiTacEntities db = new DanhBaDoiTacEntities();
        // GET: Publish/ManageOverViewTD
        public ActionResult Index(int? page, int? id)
        {
            var res = (from o in db.DB_IntroductTD
                       select new OverViewTDValidation
                       {
                           ID = o.ID,
                           TieuDe = o.TieuDe,
                           NoiDung = o.NoiDung,
                           HinhAnh = o.HinhAnh,
                           Note1 = o.Note1,
                           Note2 = o.Note2,
                       }).OrderByDescending(x=>x.ID).ToList();
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
        public ActionResult Create(OverViewTDValidation _DO)
        {

            try
            {

                string path = Server.MapPath("~/UploadedFiles/UploadedFiles/Overviews/");
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
                    _DO.HinhAnh = "/UploadedFiles/UploadedFiles/Overviews/" + FileName;
                }


                db.DB_IntroductTD_insert(_DO.TieuDe, _DO.NoiDung, _DO.HinhAnh, _DO.Note1, _DO.Note2);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageOverViewTD");
        }
        public ActionResult Edit(int? id)
        {
            var res = (from o in db.DB_IntroductTD.Where(x => x.ID == id)
                       select new OverViewTDValidation
                       {
                           ID = o.ID,
                           TieuDe = o.TieuDe,
                           NoiDung = o.NoiDung,
                           HinhAnh = o.HinhAnh,
                           Note1 = o.Note1,
                           Note2 = o.Note2,
                       }).ToList();
            OverViewTDValidation DO = new OverViewTDValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.ID = co.ID;
                    DO.TieuDe = co.TieuDe;
                    DO.NoiDung = co.NoiDung;
                    DO.HinhAnh = co.HinhAnh;
                    DO.Note1 = co.Note1;
                    DO.Note2 = co.Note2;

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
        public ActionResult Edit(OverViewTDValidation _DO)
        {
            try
            {
                string path = Server.MapPath("~/UploadedFiles/UploadedFiles/Overviews/");
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
                    _DO.HinhAnh = "/UploadedFiles/UploadedFiles/Overviews/" + FileName;
                }

                db.DB_IntroductTD_update(_DO.ID, _DO.TieuDe, _DO.NoiDung, _DO.HinhAnh, _DO.Note1, _DO.Note2);
                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageOverViewTD");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                db.DB_IntroductTD_delete(id);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageOverViewTD");
        }
    }
}