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
    public class ManageOverViewHPDQController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: Publish/ManageOverViewHPDQ
        public ActionResult Index(int? page)
        {
            var res = (from o in db_context.DB_TongQuan
                       select new OverViewHPDQValidation
                       {
                           IDTQ = o.IDTQ,
                           TieuDe = o.TieuDe,
                           TomTat = o.TomTat,
                           DayDu = o.DayDu,
                           Note = o.Note,
                           Image = o.Image
                       }).OrderByDescending(x => x.IDTQ).ToList();
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
        public ActionResult Create(OverViewHPDQValidation _DO)
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
                    _DO.Image = "/UploadedFiles/UploadedFiles/Overviews/" + FileName;
                }


                db_context.DB_TongQuan_insert(_DO.TieuDe, _DO.TomTat,_DO.DayDu, _DO.Note ,_DO.Image);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageOverViewHPDQ");
        }
        public ActionResult Edit(int? id)
        {
            var res = (from o in db_context.DB_TongQuan.Where(x=>x.IDTQ == id)
                       select new OverViewHPDQValidation
                       {
                           IDTQ = o.IDTQ,
                           TieuDe = o.TieuDe,
                           TomTat = o.TomTat,
                           DayDu = o.DayDu,
                           Note = o.Note,
                           Image = o.Image
                       }).ToList();
            OverViewHPDQValidation DO = new OverViewHPDQValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDTQ = co.IDTQ;
                    DO.TieuDe = co.TieuDe;
                    DO.TomTat = co.TomTat;
                    DO.DayDu = co.DayDu;
                    DO.Note = co.Note;
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
        public ActionResult Edit(OverViewHPDQValidation _DO)
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
                    _DO.Image = "/UploadedFiles/UploadedFiles/Overviews/" + FileName;
                }

                db_context.DB_TongQuan_update( _DO.IDTQ, _DO.TieuDe,_DO.TomTat, _DO.DayDu, _DO.Note, _DO.Image);
                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageOverViewHPDQ");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_TongQuan_delete(id);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageOverViewHPDQ");
        }
    }
}
