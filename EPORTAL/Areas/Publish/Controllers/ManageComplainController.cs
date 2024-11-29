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
    public class ManageComplainController : Controller
    {
        // GET: Publish/ManageComplain
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: ManageComplain
        public ActionResult Index(int? page)
        {
            var res = (from d in db_context.DB_KhieuNai
                       join e in db_context.DB_TTDoiTac on d.IDDoiTac equals e.IDDoiTac
                       select new ComplainValidation
                       {
                           IDKhieuNai = d.IDKhieuNai,
                           MaKhieuNai = d.MaKhieuNai,
                           IDDoiTac = e.IDDoiTac,
                           TieuDeKN = d.TieuDeKN,
                           NoiDungKN = d.NoiDungKN,
                           FileDinhKem = d.FileDinhKem,
                           NguoiKN = d.NguoiKN,
                           NgayKN = (DateTime)d.NgayKN,
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult ComplainTo(int? page, int? id)
        {
            var model = db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id).FirstOrDefault();

            var res = (from b in db_context.DB_PhanHoiDoiTac.Where(x => x.KhieuNaiID == id)
                       join c in db_context.DB_KhieuNai on b.KhieuNaiID equals c.IDKhieuNai

                       select new RespondToComplaintsValidation
                       {

                           IDPhanHoi = b.IDPhanHoi,
                           IDKhieuNai = c.IDKhieuNai,

                           MaPhanHoi = b.MaPhanHoi,
                           TieuDePH = b.TieuDePH,
                           NoiDungPH = b.NoiDungPH,
                           FileDinhKem = b.FileDinhKem,
                           NgayPH = (DateTime)b.NgayPH,
                           IDNguoiPH = b.IDNguoiPH,
                           TenNguoiPH = b.TenNguoiPH,
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult AddRespond(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_KhieuNai> kn = db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id).ToList();
            ViewBag.KNList = new SelectList(kn, "IDKhieuNai", "MaKhieuNai", "NoiDungKhieuNai");

            var lastRecord = (from c in db_context.DB_PhanHoiDoiTac orderby c.IDPhanHoi descending select c).FirstOrDefault();
            if (lastRecord == null)
            {
                ViewBag.MaPHanHoi = "PH0000000" + 1;
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9)
            {
                ViewBag.MaPHanHoi = "PH0000000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 99)
            {
                ViewBag.MaPHanHoi = "PH00000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 999)
            {
                ViewBag.MaPHanHoi = "PH0000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9999)
            {
                ViewBag.MaPHanHoi = "PH000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 99999)
            {
                ViewBag.MaPHanHoi = "PH00" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 999999)
            {
                ViewBag.MaPHanHoi = "PH0" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else
            {
                ViewBag.MaPHanHoi = "PH" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }


            return PartialView();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddRespond(RespondToComplaintsValidation _DO)
        {
            var ten = MyAuthentication.TenNV;
            var idnv = MyAuthentication.Username;
            try
            {
                string path = Server.MapPath("~/UploadedFiles/Respond/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.MaPhanHoi : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";

                ////Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.FileDinhKem = "/UploadedFiles/Respond/" + FileName;
                }


                if (IsTBAvailable(_DO.MaPhanHoi) == false)
                {

                    db_context.DB_PhanHoiDoiTac_insert(_DO.MaPhanHoi, _DO.TieuDePH, _DO.NoiDungPH, _DO.FileDinhKem, DateTime.Now, idnv, ten, _DO.IDKhieuNai);
                    TempData["msgSuccess"] = "<script>alert('Phản hồi thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Mã Phản Hồi đã tồn tại');</script>";
                }
            }
            catch (Exception ex)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi phản hồi: " + ex.Message + "');</script>";
            }
            return RedirectToAction("ComplainTo", "ManageComplain", new { id = _DO.IDKhieuNai });
        }

        public ActionResult EditRespond(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_KhieuNai> kn = db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id).ToList();
            ViewBag.KNList = new SelectList(kn, "IDKhieuNai", "MaKhieuNai", "NoiDungKhieuNai");
            var res = (from d in db_context.DB_PhanHoiDoiTac_searchByID(id)
                       select new RespondToComplaintsValidation
                       {
                           IDPhanHoi = d.IDPhanHoi,
                           IDKhieuNai = d.KhieuNaiID,

                           MaPhanHoi = d.MaPhanHoi,
                           NoiDungPH = d.NoiDungPH,
                           FileDinhKem = d.FileDinhKem,
                           NgayPH = (DateTime)d.NgayPH,
                           IDNguoiPH = d.IDNguoiPH,
                           TenNguoiPH = d.TenNguoiPH,


                       }).ToList();
            RespondToComplaintsValidation DO = new RespondToComplaintsValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDPhanHoi = co.IDPhanHoi;
                    DO.IDKhieuNai = co.IDKhieuNai;
                    DO.MaPhanHoi = co.MaPhanHoi;
                    DO.NoiDungPH = co.NoiDungPH;
                    DO.FileDinhKem = co.FileDinhKem;
                    DO.NgayPH = co.NgayPH;
                    DO.IDNguoiPH = co.IDNguoiPH;
                    DO.TenNguoiPH = co.TenNguoiPH;

                }

                ViewBag.NgayKN = DO.NgayPH.ToString("yyyy-MM-dd");

            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]

        [ValidateInput(false)]
        public ActionResult EditRespond(RespondToComplaintsValidation _DO)
        {
            var ten = MyAuthentication.TenNV;
            var idnv = MyAuthentication.Username;

            try
            {
                string path = Server.MapPath("~/UploadedFiles/Respond/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.MaPhanHoi : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";

                ////Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.FileDinhKem = "/UploadedFiles/Respond/" + FileName;
                }
                db_context.DB_PhanHoiDoiTac_update(_DO.IDPhanHoi, _DO.MaPhanHoi, _DO.TieuDePH, _DO.NoiDungPH, _DO.FileDinhKem, DateTime.Now, idnv, ten, _DO.IDKhieuNai);
                TempData["msgSuccess"] = "<script>alert('Cập Nhật Phản hồi thành công');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgError"] = "<script>alert('Cập Nhật Thất Bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("ComplainTo", "ManageComplain", new { id = _DO.IDKhieuNai });
        }
        public ActionResult DeleteRespond(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            var idt = db_context.DB_PhanHoiDoiTac.Where(a => a.IDPhanHoi == id).Select(x => x.KhieuNaiID).FirstOrDefault();
            try
            {
                db_context.DB_PhanHoiDoiTac_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("ComplainTo", "ManageComplain", new { id = idt });
        }

        public void InsertDSPH(string MaPhanHoi, string TieuDePH, string NoiDungPH, string FileDinhKem, string IDNguoiPH, string TenNguoiPH, int IDKhieuNai)
        {
            db_context.DB_PhanHoiDoiTac_insert(MaPhanHoi, TieuDePH, NoiDungPH, FileDinhKem, DateTime.Now, IDNguoiPH, TenNguoiPH, IDKhieuNai);
        }

        public bool IsTBAvailable(string MaPhanHoi)
        {
            var IsCheck = (from t in db_context.DB_PhanHoiDoiTac
                           where (t.MaPhanHoi.ToLower() == MaPhanHoi)
                           select new { t.MaPhanHoi }).FirstOrDefault();
            bool status;
            if (IsCheck != null)
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return status;
        }
    }
}