using EPORTAL.Common;
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

            // 1 SP call lay TAT CA video accessible cua user, group theo album.
            // Truoc kia: view tu new EPORTALEntities() trong foreach => N+1.
            // Gio: 1 query + in-memory GroupBy, pass tinh sang view.
            var videoCounts = db.Video_select("", Models.MyAuthentication.ID)
                .Where(v => v.AlbumID.HasValue)
                .GroupBy(v => v.AlbumID.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Chi hien album co it nhat 1 video user thay duoc (giu hanh vi cu cua view).
            var res = (from a in db.Album_select(search)
                       let count = videoCounts.ContainsKey(a.IDAlbum) ? videoCounts[a.IDAlbum] : 0
                       where count > 0
                       select new AlbumValidation
                       {
                           IDAlbum = a.IDAlbum,
                           TenAlbum = a.TenAlbum,
                           Images = a.Images,
                           SoLuongVideo = count
                       }).ToList();

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AlbumValidation _DO)
        {
            var uploadError = FileUploadValidator.ValidateImage(_DO.ImageFile);
            if (uploadError != null)
            {
                TempData["msgError"] = "<script>alert('" + uploadError + "');</script>";
                return RedirectToAction("Index", "Album");
            }

            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (_DO.ImageFile != null && _DO.ImageFile.ContentLength > 0)
                {
                    var safeName = FileUploadValidator.SafeFileName(_DO.ImageFile.FileName);
                    _DO.ImageFile.SaveAs(Path.Combine(path, safeName));
                    _DO.Images = "~/Images/" + safeName;
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AlbumValidation _DO)
        {
            var uploadError = FileUploadValidator.ValidateImage(_DO.ImageFile);
            if (uploadError != null)
            {
                TempData["msgError"] = "<script>alert('" + uploadError + "');</script>";
                return RedirectToAction("Index", "Album");
            }

            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (_DO.ImageFile != null && _DO.ImageFile.ContentLength > 0)
                {
                    var safeName = FileUploadValidator.SafeFileName(_DO.ImageFile.FileName);
                    _DO.ImageFile.SaveAs(Path.Combine(path, safeName));
                    _DO.Images = "~/Images/" + safeName;
                }

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