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
    public class ResponToComplainController : Controller
    {
        // GET: Publish/ResponToComplain
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: ManageComplain
        public ActionResult Index(int? page)
        {
            var res = (from j in db_context.DB_KhieuNai
                       join d in db_context.DB_TTDoiTac on j.IDDoiTac equals d.IDDoiTac
                       join x in db_context.DB_TinhTrangKhieuNai on j.IDTTKN equals x.IDTTKN
                       join w in db_context.DB_PhanHoiDoiTac on j.IDKhieuNai equals w.KhieuNaiID into IDPhanHoi
                       select new ComplainValidation
                       {
                           IDDoiTac = d.IDDoiTac,
                           IDBP = d.BPID,
                           LinhVucHĐ = d.LinhVucHĐ,
                           FullName = d.FullName,
                           ShortName = d.ShortName,
                           TaxCode = d.Taxcode,
                           Address = d.Address,
                           City = (int)d.City,
                           Regions = (int)d.Regions,
                           Customer = d.Customer,
                           Vendor = d.Vender,
                           ImageLogo = d.ImageLogo,
                           Email = d.Email,
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
                           TieuDePH = b.TieuDePH,
                           MaPhanHoi = b.MaPhanHoi,
                           NoiDungPH = b.NoiDungPH,
                           FileDinhKem = b.FileDinhKem,
                           NgayPH = (DateTime)b.NgayPH,
                           IDNguoiPH = b.IDNguoiPH,
                           TenNguoiPH = b.TenNguoiPH,
                           TieuDeKN = c.TieuDeKN
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
            return RedirectToAction("ComplainTo", "ResponToComplain", new { id = _DO.IDKhieuNai });
        }
        public ActionResult EditRespond(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_KhieuNai> kn = db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id).ToList();
            ViewBag.KNList = new SelectList(kn, "IDKhieuNai", "MaKhieuNai", "NoiDungKhieuNai");
            var res = (from d in db_context.DB_PhanHoiDoiTac
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
            return RedirectToAction("ComplainTo", "ResponToComplain", new { id = _DO.IDKhieuNai });
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
            return RedirectToAction("ComplainTo", "ResponToComplain", new { id = idt });
        }
        public ActionResult EditTinhTrang(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_TinhTrangKhieuNai> tt = db_context.DB_TinhTrangKhieuNai.ToList();
            ViewBag.TTKNList = new SelectList(tt, "IDTTKN", "TenTTKN");
            var res = (from d in db_context.DB_KhieuNai
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
                db_context.DB_KhieuNai_update(_DO.IDKhieuNai, _DO.MaKhieuNai, iddt, _DO.TieuDeKN, _DO.NoiDungKN, _DO.FileDinhKem, ten, DateTime.Now, _DO.IDTTKN);
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + ex.Message + " ');</script>";
            }
            return RedirectToAction("Index", "ResponToComplain");
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
        public ActionResult RespondComplainDetail(int? id, int? page)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_TinhTrangKhieuNai> ttkn = db_context.DB_TinhTrangKhieuNai.ToList();
            ViewBag.TTKNList = new SelectList(ttkn, "IDTTKN", "TenTTKN");
            var res = (from d in db_context.DB_TTDoiTac
                       join j in db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id) on d.IDDoiTac equals j.IDDoiTac

                       join x in db_context.DB_TinhTrangKhieuNai on j.IDTTKN equals x.IDTTKN
                       join w in db_context.DB_PhanHoiDoiTac on j.IDKhieuNai equals w.KhieuNaiID into IDPhanHoi
                       select new ComplainValidation
                       {
                           IDDoiTac = d.IDDoiTac,
                           IDBP = d.BPID,
                           LinhVucHĐ = d.LinhVucHĐ,
                           LoaiDTID = d.LoaiIDDT,
                           //TenLoaiDT = l.TenLoaiDT,                       
                           FullName = d.FullName,
                           ShortName = d.ShortName,
                           TaxCode = d.Taxcode,
                           Address = d.Address,
                           City = (int)d.City,
                           Regions = (int)d.Regions,
                           Customer = d.Customer,
                           Vendor = d.Vender,
                           ImageLogo = d.ImageLogo,
                           Email = d.Email,
                           SLPH = IDPhanHoi.Count(),
                           IDTTKN = (int)x.IDTTKN,
                           TenTTKN = x.TenTTKN,
                           MaKhieuNai = j.MaKhieuNai,
                           NoiDungKN = j.NoiDungKN,
                           FileDinhKem = j.FileDinhKem,
                           NguoiKN = j.NguoiKN,
                           TieuDeKN = j.TieuDeKN,
                           NgayKN = (DateTime)j.NgayKN,
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult CreateToFeedback(int? id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_KhieuNai> kn = db_context.DB_KhieuNai.Where(x => x.IDKhieuNai == id).ToList();
            ViewBag.KNList = new SelectList(kn, "IDKhieuNai", "MaKhieuNai", "NoiDungKhieuNai");
            var lastRecord = (from c in db_context.DB_PhanHoiDoiTac orderby c.IDPhanHoi descending select c).FirstOrDefault();
            if (lastRecord == null)
            {
                ViewBag.MaPHanHoi = "PH00000000" + 1;
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9)
            {
                ViewBag.MaPHanHoi = "PH0000000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 99)
            {
                ViewBag.MaPHanHoi = "PH000000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 999)
            {
                ViewBag.MaPHanHoi = "PH00000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9999)
            {
                ViewBag.MaPHanHoi = "PH0000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 99999)
            {
                ViewBag.MaPHanHoi = "PH000" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 999999)
            {
                ViewBag.MaPHanHoi = "PH00" + (Convert.ToInt32(lastRecord.IDPhanHoi) + 1);
            }
            else if (Convert.ToInt32(lastRecord.IDPhanHoi) < 9999999)
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
            return RedirectToAction("ResponComplainDetail", "ResponToComplain", new { id = _DO.IDKhieuNai });
        }
        public ActionResult EditToFeedback(int? id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_KhieuNai> kn1 = db_context.DB_KhieuNai.ToList();
            ViewBag.KNList = new SelectList(kn1, "IDKhieuNai", "MaKhieuNai", "NguoiKN");
            List<DB_TinhTrangKhieuNai> tt = db_context.DB_TinhTrangKhieuNai.ToList();
            ViewBag.TTKNList = new SelectList(tt, "IDTTKN", "TenTTKN");
            var res = (from d in db_context.DB_KhieuNai
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
        public ActionResult EditToFeedback(ComplainValidation _DO)
        {
            try
            {
                db_context.DB_KhieuNai_updateTinhTrang(_DO.IDKhieuNai, _DO.IDTTKN);
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + ex.Message + " ');</script>";
            }
            return RedirectToAction("RespondComplainDetail", "ResponToComplain", new { id = _DO.IDKhieuNai });
        }
    }

}