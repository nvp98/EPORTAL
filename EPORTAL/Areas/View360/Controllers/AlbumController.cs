using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    public class AlbumController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        // GET: View360/Album
        public ActionResult Index(int? page, string search)
        {
            if (search == null) search = "";
            ViewBag.search = search;
            var res = from a in db.Album_select(search)
                      select new AlbumValidation
                      {
                          IDAlbum = a.IDAlbum,
                          TenAlbum = a.TenAlbum,
                          Images = a.Images
                      };

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(AlbumValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.ImageFile != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";

                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.Images = "~/Images/" + FileName;
                }

                var a = db.Album_insert(_DO.TenAlbum, _DO.Images);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Album");
        }
        public ActionResult Edit(int id)
        {
            var res = (from a in db.Album_searchByID(id)
                       select new AlbumValidation
                       {
                           IDAlbum = a.IDAlbum,
                           TenAlbum = a.TenAlbum,
                           Images = a.Images
                       }).ToList();
            AlbumValidation DO = new AlbumValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDAlbum = a.IDAlbum;
                    DO.Images = a.Images;
                    DO.TenAlbum = a.TenAlbum;

                }

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AlbumValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/Images/");
                //string path ="~/Images/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string FileName = _DO.ImageFile != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";
                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";

                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.Images = "~/Images/" + FileName;
                }

                //Upload file pdf
           
                    var a = db.Album_update(_DO.IDAlbum, _DO.TenAlbum, _DO.Images);
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
           
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Album");
        }
        public ActionResult Delete(int? id)
        {
            try
            {
                db.Album_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Album");
        }
    }
}