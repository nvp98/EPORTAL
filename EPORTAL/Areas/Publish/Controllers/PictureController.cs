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
    public class PictureController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: Publish/Picture
        public ActionResult Index(int? id, int? page)
        {
            var res = (from i in db_context.DB_Picture
                       select new PictureValidation
                       {
                           ID = i.ID,
                           TenHinhAnh = i.TenHinhAnh,
                           Image = i.HinhAnh
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
        public ActionResult Create(PictureValidation _DO)
        {

            try
            {
                
                    string path = Server.MapPath("~/UploadedFiles/UploadedFiles/Pictures/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = _DO.NameFile != null ? _DO.TenHinhAnh + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                    if (_DO.NameFile != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.NameFile.SaveAs(path + FileName);
                        _DO.Image = "/UploadedFiles/UploadedFiles/Pictures/" + FileName;
                    }

               
                db_context.DB_Picture_insert(_DO.TenHinhAnh, _DO.Image);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Picture");
        }
        public ActionResult Edit(int? id)
        {
            var res = (from i in db_context.DB_Picture.Where(x=>x.ID == id)
                       select new PictureValidation
                       {
                           ID = i.ID,
                           TenHinhAnh = i.TenHinhAnh,
                           Image = i.HinhAnh
                       }).ToList();
            PictureValidation DO = new PictureValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.ID = co.ID;
                    DO.TenHinhAnh = co.TenHinhAnh;
                    DO.Image = co.Image;
                   
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
        public ActionResult Edit(PictureValidation _DO)
        {
            try
            {
                string path = Server.MapPath("~/UploadedFiles/UploadedFiles/Pictures/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string FileName = _DO.NameFile != null ? _DO.TenHinhAnh + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                if (_DO.NameFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NameFile.SaveAs(path + FileName);
                    _DO.Image = "/UploadedFiles/UploadedFiles/Pictures/" + FileName;
                }

                db_context.DB_Picture_update(_DO.ID, _DO.TenHinhAnh, _DO.Image);
                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Picture");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_Picture_delete(id);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "Picture");
        }
    }
}
