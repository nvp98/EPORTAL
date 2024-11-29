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
    public class ComplainController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: ManageComplain
        public ActionResult Index(int? page, int? id)
        {

            if (MyAuthentication.IDQuyen == 1 || MyAuthentication.IDQuyen == 2)
            {
                var res = (from j in db_context.DB_KhieuNai
                           join d in db_context.DB_TTDoiTac on j.IDDoiTac equals d.IDDoiTac
                           join x in db_context.DB_TinhTrangKhieuNai on j.IDTTKN equals x.IDTTKN
                           join a in db_context.DB_PhanHoiDoiTac on j.IDKhieuNai equals a.KhieuNaiID into IDPhanHoi
                           select new ComplainValidation
                           {
                               IDDoiTac = d.IDDoiTac,
                               IDBP = d.BPID,
                               LinhVucHĐ = d.LinhVucHĐ,
                               ShortName = d.ShortName,
                               FullName = d.FullName,
                               TaxCode = d.Taxcode,
                               Address = d.Address,
                               City = (int)d.City,
                               Regions = (int)d.Regions,
                               ImageLogo = d.ImageLogo,
                               Customer = d.Customer,
                               Vendor = d.Vender,
                               SLPH = IDPhanHoi.Count(),
                               IDTTKN = (int)x.IDTTKN,
                               TenTTKN = x.TenTTKN,
                               MaKhieuNai = j.MaKhieuNai,
                               NoiDungKN = j.NoiDungKN,
                               FileDinhKem = j.FileDinhKem,
                               NguoiKN = j.NguoiKN,
                               TieuDeKN = j.TieuDeKN,
                               IDKhieuNai = j.IDKhieuNai,
                               NgayKN = (DateTime)j.NgayKN,

                           }).OrderByDescending(x => x.IDKhieuNai).ToList();
                if (page == null) page = 1;
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }
            else
            {
                var res = (from j in db_context.DB_KhieuNai.Where(x => x.IDDoiTac == MyAuthentication2.IDDoiTac)
                           join d in db_context.DB_TTDoiTac on j.IDDoiTac equals d.IDDoiTac
                           join x in db_context.DB_TinhTrangKhieuNai on j.IDTTKN equals x.IDTTKN
                           join a in db_context.DB_PhanHoiDoiTac on j.IDKhieuNai equals a.KhieuNaiID into IDPhanHoi
                           select new ComplainValidation
                           {
                               IDDoiTac = d.IDDoiTac,
                               IDBP = d.BPID,
                               LinhVucHĐ = d.LinhVucHĐ,
                               ShortName = d.ShortName,
                               FullName = d.FullName,
                               TaxCode = d.Taxcode,
                               Address = d.Address,
                               City = (int)d.City,
                               Regions = (int)d.Regions,
                               ImageLogo = d.ImageLogo,
                               Customer = d.Customer,
                               Vendor = d.Vender,
                               SLPH = IDPhanHoi.Count(),
                               IDTTKN = (int)x.IDTTKN,
                               TenTTKN = x.TenTTKN,
                               MaKhieuNai = j.MaKhieuNai,
                               NoiDungKN = j.NoiDungKN,
                               FileDinhKem = j.FileDinhKem,
                               NguoiKN = j.NguoiKN,
                               TieuDeKN = j.TieuDeKN,
                               IDKhieuNai = j.IDKhieuNai,
                               NgayKN = (DateTime)j.NgayKN,

                           }).OrderByDescending(x => x.IDKhieuNai).ToList();
                if (page == null) page = 1;
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }


        }
        public ActionResult ComplainDetail()
        {
            return View();
        }
        public ActionResult Create()
        {

            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_TTDoiTac> dt = db_context.DB_TTDoiTac.ToList();
            ViewBag.DTList = new SelectList(dt, "IDDoiTac", "MaDoiTac");
            List<DB_TinhTrangKhieuNai> tt = db_context.DB_TinhTrangKhieuNai.ToList();
            ViewBag.TTKNList = new SelectList(tt, "IDTTKN", "TenTTKN");
            var lastRecord = (from c in db_context.DB_KhieuNai orderby c.IDKhieuNai descending select c).FirstOrDefault();
            if (lastRecord == null)
            {
                ViewBag.MaKhieuNai = "KN" + 1;
            }
            else if (Convert.ToInt32(lastRecord.IDKhieuNai) < 9)
            {
                ViewBag.MaKhieuNai = "KN0" + (Convert.ToInt32(lastRecord.IDKhieuNai) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDKhieuNai) < 99)
            {
                ViewBag.MaKhieuNai = "KN00" + (Convert.ToInt32(lastRecord.IDKhieuNai) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDKhieuNai) < 999)
            {
                ViewBag.MaKhieuNai = "KN000" + (Convert.ToInt32(lastRecord.IDKhieuNai) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDKhieuNai) < 9999)
            {
                ViewBag.MaKhieuNai = "KN0000" + (Convert.ToInt32(lastRecord.IDKhieuNai) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDKhieuNai) < 99999)
            {
                ViewBag.MaKhieuNai = "KN00000" + (Convert.ToInt32(lastRecord.IDKhieuNai) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDKhieuNai) < 999999)
            {
                ViewBag.MaKhieuNai = "KN000000" + (Convert.ToInt32(lastRecord.IDKhieuNai) + 1);
            }
            else
            {
                ViewBag.MaKhieuNai = "KN0000000" + (Convert.ToInt32(lastRecord.IDKhieuNai) + 1);
            }


            return PartialView();
        }
        [HttpPost]
        [ValidateInput(false)]

        public ActionResult Create(ComplainValidation _DO)
        {
            int iddt = MyAuthentication2.IDDoiTac;
            var ten = MyAuthentication2.ShortName;
            try
            {
                string path = Server.MapPath("~/UploadedFiles/Complain/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.MaKhieuNai : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";

                ////Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.FileDinhKem = "/UploadedFiles/Complain/" + FileName;
                }
                //check trùng thông tin

                db_context.DB_KhieuNai_insert(_DO.MaKhieuNai, _DO.NoiDungKN, iddt, _DO.TieuDeKN, _DO.FileDinhKem, ten, DateTime.Now, _DO.IDTTKN);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception ex)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "Complain");
        }
        public ActionResult Edit(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_TinhTrangKhieuNai> tt = db_context.DB_TinhTrangKhieuNai.ToList();
            ViewBag.TTKNList = new SelectList(tt, "IDTTKN", "TenTTKN");
            var res = (from d in db_context.DB_KhieuNai_searchByID(id)
                       select new ComplainValidation
                       {
                           IDKhieuNai = d.IDKhieuNai,
                           MaKhieuNai = d.MaKhieuNai,

                           NoiDungKN = d.NoiDungKN,
                           FileDinhKem = d.FileDinhKem,
                           NguoiKN = d.NguoiKN,
                           NgayKN = (DateTime)d.NgayKN,



                       }).ToList();
            ComplainValidation DO = new ComplainValidation();
            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDKhieuNai = co.IDKhieuNai;
                    DO.MaKhieuNai = co.MaKhieuNai;

                    DO.NoiDungKN = co.NoiDungKN;
                    DO.FileDinhKem = co.FileDinhKem;
                    DO.NguoiKN = co.NguoiKN;

                    DO.NgayKN = co.NgayKN;


                }

                ViewBag.NgayKN = DO.NgayKN.ToString("yyyy-MM-dd");

            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);

        }

        [HttpPost]

        [ValidateInput(false)]
        public ActionResult Edit(ComplainValidation _DO)
        {
            int iddt = MyAuthentication2.IDDoiTac;
            var ten = MyAuthentication2.ShortName;
            try
            {

                string path = Server.MapPath("~/UploadedFiles/Complain/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.MaKhieuNai : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";

                ////Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.FileDinhKem = "/UploadedFiles/Complain/" + FileName;
                }
                db_context.DB_KhieuNai_update(_DO.IDKhieuNai, _DO.MaKhieuNai, iddt, _DO.TieuDeKN, _DO.NoiDungKN, _DO.FileDinhKem, ten, DateTime.Now, _DO.IDTTKN);
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + ex.Message + " ');</script>";
            }
            return RedirectToAction("Index", "ResponToComplain");
        }
        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_KhieuNai_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Complain");
        }


        public ActionResult AddFileKhieuNai(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_KhieuNai> kn = db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id).ToList();
            ViewBag.KNList = new SelectList(kn, "IDKhieuNai", "MaKhieuNai");
            return PartialView();
        }

        [HttpPost]

        public ActionResult AddFileKhieuNai(ComplainValidation _DO)
        {
            int iddt = MyAuthentication2.IDDoiTac;
            var ten = MyAuthentication2.ShortName;
            db_context.DB_KhieuNai_insert(_DO.MaKhieuNai, _DO.NoiDungKN, iddt, _DO.TieuDeKN, _DO.FileDinhKem, ten, DateTime.Now, _DO.IDTTKN);
            TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            return RedirectToAction("Complain", new { id = _DO.IDKhieuNai });
        }
        public ActionResult ReplyToFeedback(int? id, int? page)
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
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));




        }
        public ActionResult CreateToFeedback(int? id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_KhieuNai> kn = db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id).ToList();
            ViewBag.KNList = new SelectList(kn, "IDKhieuNai", "MaKhieuNai", "NoiDungKhieuNai");

            var lastRecord = (from c in db_context.DB_PhanHoiDoiTac orderby c.IDPhanHoi descending select c).FirstOrDefault();
            if (lastRecord == null)
            {
                ViewBag.MaPHanHoi = "PH" + 1;
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9)
            {
                ViewBag.MaPHanHoi = "PH0" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 99)
            {
                ViewBag.MaPHanHoi = "PH00" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 999)
            {
                ViewBag.MaPHanHoi = "PH000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9999)
            {
                ViewBag.MaPHanHoi = "PH0000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 99999)
            {
                ViewBag.MaPHanHoi = "PH00000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 999999)
            {
                ViewBag.MaPHanHoi = "PH000000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9999999)
            {
                ViewBag.MaPHanHoi = "PH0000000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else
            {
                ViewBag.MaPHanHoi = "PH00000000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }



            return PartialView();
        }
        [HttpPost]
        [ValidateInput(false)]

        public ActionResult CreateToFeedback(RespondToComplaintsValidation _DO)
        {
            var iddt = MyAuthentication2.Username;
            var tendt = MyAuthentication2.ShortName;
            var idnv = MyAuthentication.Username;
            var tennv = MyAuthentication.TenNV;
            try
            {
                string path = Server.MapPath("~/UploadedFiles/ReplyToFeedback/");

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
                    _DO.FileDinhKem = "~/UploadedFiles/ReplyToFeedback/" + FileName;
                }
                //check trùng thông tin

                if (iddt == MyAuthentication2.Username)
                {
                    db_context.DB_PhanHoiDoiTac_insert(_DO.MaPhanHoi, _DO.TieuDePH, _DO.NoiDungPH, _DO.FileDinhKem, DateTime.Now, iddt, tendt, _DO.IDKhieuNai);
                    TempData["msgSuccess"] = "<script>alert('Phản hồi thành công');</script>";
                }
                else
                {
                    db_context.DB_PhanHoiDoiTac_insert(_DO.MaPhanHoi, _DO.TieuDePH, _DO.NoiDungPH, _DO.FileDinhKem, DateTime.Now, idnv, tennv, _DO.IDKhieuNai);
                    TempData["msgSuccess"] = "<script>alert('Phản hồi thành công');</script>";
                }

            }
            catch (Exception ex)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + ex.Message + "');</script>";
            }
            return RedirectToAction("ComplainDetail", "Complain", new { id = _DO.IDKhieuNai });
        }
    }


}