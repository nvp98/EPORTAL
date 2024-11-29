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
    public class ManageNewsController : Controller
    {
        // GET: Publish/ManageNews
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: ManageNews
        public ActionResult Index(int? page)
        {
            var res = (from l in db_context.DB_TinTuc
                       select new ManageNewsValidation
                       {
                           IDTinTuc = l.IDTintuc,

                           TenDeTai = l.TenDeTai,
                           TomTatTinTuc = l.TomTatTinTuc,
                           NoiDungTinTuc = l.NoiDungTinTuc,
                           ImageTinTuc = l.ImageTinTuc,
                           NgayDangTin = (DateTime)l.NgayDangTin,
                           NguoiDangTin = l.NguoiDangTin
                       }).OrderByDescending(x => x.IDTinTuc).ToList();
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
        public ActionResult Create(ManageNewsValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/UploadedFiles/News/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.TenDeTai : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";

                ////Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.ImageTinTuc = "/UploadedFiles/News/" + FileName;
                }
                db_context.DB_TinTuc_insert(_DO.TenDeTai, _DO.TomTatTinTuc, _DO.NoiDungTinTuc, _DO.ImageTinTuc, _DO.NguoiDangTin);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageNews");
        }


        public ActionResult Edit()
        {
            var res = (from d in db_context.DB_TinTuc
                       select new ManageNewsValidation
                       {
                           IDTinTuc = d.IDTintuc,

                           TenDeTai = d.TenDeTai,
                           TomTatTinTuc = d.TomTatTinTuc,
                           NoiDungTinTuc = d.NoiDungTinTuc,
                           ImageTinTuc = d.ImageTinTuc,
                           NgayDangTin = (DateTime)d.NgayDangTin,
                           NguoiDangTin = d.NguoiDangTin
                       }).ToList();
            ManageNewsValidation DO = new ManageNewsValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDTinTuc = co.IDTinTuc;
                    DO.TenDeTai = co.TenDeTai;
                    DO.TomTatTinTuc = co.TomTatTinTuc;
                    DO.NoiDungTinTuc = co.NoiDungTinTuc;
                    DO.ImageTinTuc = co.ImageTinTuc;
                    DO.NgayDangTin = co.NgayDangTin;
                    DO.NguoiDangTin = co.NguoiDangTin;
                }
                ViewBag.NgayHĐ = DO.NgayDangTin.ToString("yyyy-MM-dd");
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(ManageNewsValidation _DO)
        {
            try
            {
                string path = Server.MapPath("~/UploadedFiles/News/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.TenDeTai : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";


                //Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.ImageTinTuc = "/UploadedFiles/News/" + FileName;
                }
                db_context.DB_TinTuc_update(_DO.IDTinTuc, _DO.TenDeTai, _DO.TomTatTinTuc, _DO.NoiDungTinTuc, _DO.ImageTinTuc, _DO.NguoiDangTin);
                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageNews");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_TinTuc_delete(id);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageNews");
        }

        public ActionResult AddNews(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_TinTuc> tt = db_context.DB_TinTuc.Where(x => x.IDTintuc == id).ToList();
            ViewBag.TTList = new SelectList(tt, "IDTinTuc", "TenDeTai");
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddNews(ManageNewsValidation _DO)
        {
            db_context.DB_TinTuc_insert(_DO.TenDeTai, _DO.TomTatTinTuc, _DO.NoiDungTinTuc, _DO.ImageTinTuc, _DO.NguoiDangTin);
            TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            return RedirectToAction("ManageNews", new { id = _DO.IDTinTuc });
        }

    }
}