using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsTagSign.TheXeCoDongVM;
using EPORTAL.ModelsView360;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using static EPORTAL.ModelsTagSign.TheXeCoDongVM.ChiTietDonVM;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class DangKyKhoaThe_NTController : Controller
    {
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "DangKyKhoaThe_NT";
        // GET: TagSign/ContractorGroup

        public ActionResult Index_NT(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;
            var userNameLogin = Models.MyAuthentication.Username;

            var data = db_dk.Database.SqlQuery<DonDangKyKhoaTheViewModel>(
               "EXEC KTNT_DonDangKy_Search @p_BeginDate, @p_EndDate,@p_MaPhieu,@p_UserNameLogin",
               new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
               new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
               new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value),
               new SqlParameter("@p_UserNameLogin", (object)userNameLogin ?? DBNull.Value)
           ).ToList();

            var pagedData = data.ToPagedList(pageNumber, pageSize);
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(pagedData);
        }
        [HttpGet]
        public ActionResult TaoDonDangKy()
        {

            string tenNhaThau = "";
            int? nhaThauID = null;

            int userid = Models.MyAuthentication.ID; // ID tài khoản NT
            var nt = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == userid); // thông tin tài khoản

            if (nt != null && nt.IDNT.HasValue)
            {
                nhaThauID = nt.IDNT.Value;

                var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == nhaThauID.Value);
                if (nhaThau != null)
                {
                    tenNhaThau = nhaThau.FullName;
                }
            }

            // Truyền ID và Tên nhà thầu ra View (để hiển thị + submit)
            ViewBag.ID_NhaThau = nhaThauID;
            ViewBag.TenNhaThau = tenNhaThau;
            // Các ViewBag khác bạn đang dùng
            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 3)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();
            ViewBag.VP1C_List = new SelectList(VP1C, "IDNhanVien", "HoTen");
            return View();
        }
        [HttpPost]
        public ActionResult TaoDonDangKy(TaoDonDangKyViewModel model, string ChiTietJson)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.NoiDung))
                    return Json(new { success = false, message = "Nội dung đơn không được để trống!" });

                // 1. Tạo JSON danh sách chi tiết từ model.ChiTiet
                // var chiTietJson = Newtonsoft.Json.JsonConvert.SerializeObject(model.ChiTiet);

                var chiTietJson = ChiTietJson;
                if (string.IsNullOrWhiteSpace(chiTietJson))
                {
                    chiTietJson = Newtonsoft.Json.JsonConvert.SerializeObject(model.ChiTiet ?? new List<KTNT_ChiTietVM>());
                }

                var Business_Partner = Models.MyAuthentication.Username;
                var thangNam = DateTime.Now.ToString("yyyyMMdd");
                var prefix = $"{Business_Partner}_DKTK{thangNam}-";

                var lastMaDon = db_dk.KTNT_DonDangKy
                    .Where(x => x.MaDon.StartsWith(prefix))
                    .OrderByDescending(x => x.MaDon)
                    .Select(x => x.MaDon)
                    .FirstOrDefault();

                int stt = 1;
                if (!string.IsNullOrEmpty(lastMaDon) && lastMaDon.Length > prefix.Length)
                {
                    var soStr = lastMaDon.Substring(prefix.Length);
                    if (int.TryParse(soStr, out int lastNum))
                        stt = lastNum + 1;
                }

                model.MaDon = prefix + stt.ToString("D3");

                // 3. Lấy thông tin người dùng hiện tại
                string userNameLogin = Models.MyAuthentication.Username;

                // 4. Thực hiện gọi stored procedure

                var result = db_dk.Database.SqlQuery<SPResult>(
                    @"EXEC KTNT_DonDangKy_Insert
                    @p_Ma_Don,
                    @p_NoiDung,
                    @p_NhaThau_ID,
                    @p_UserNameLogin,
                    @p_BP_XuLy_ID,
                    @JsonDanhSachChiTiet",
                    new SqlParameter("@p_Ma_Don", model.MaDon),
                    new SqlParameter("@p_NoiDung", model.NoiDung ?? ""),
                    new SqlParameter("@p_NhaThau_ID", model.ID_NhaThau),
                    new SqlParameter("@p_UserNameLogin", userNameLogin ?? ""),
                    new SqlParameter("@p_BP_XuLy_ID", model.BP_XuLy_ID),
                    new SqlParameter("@JsonDanhSachChiTiet", chiTietJson ?? "")
                ).FirstOrDefault();

                if (result != null && result.Result == 1)
                    return Json(new { success = true, message = result.Message, maDon = result.MaDon });
                else
                    return Json(new { success = false, message = result?.Message ?? "Lỗi không xác định!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        public ActionResult Update(string id)
        {
            if (string.IsNullOrEmpty(id)) return HttpNotFound();

            var donDangKy = db_dk.KTNT_DonDangKy.AsNoTracking().FirstOrDefault(x => x.MaDon == id);
            if (donDangKy == null) return HttpNotFound();

            var currentUser = Models.MyAuthentication.Username ?? string.Empty;
            var createdBy = donDangKy.UserNameLogin ?? string.Empty;
            if (!string.Equals(currentUser, createdBy, StringComparison.OrdinalIgnoreCase))
            {
                TempData["msgError"] = "Bạn không có quyền chỉnh sửa đơn này.";
                return RedirectToAction("Index_Test");
            }

            if (donDangKy.TinhTrang != (int)TinhTrangDonDangKyKhoaThe.ChuaTrinhKy)
            {
                TempData["msgError"] = "Đơn đã được duyệt, không thể chỉnh sửa";
                return RedirectToAction("Index_Test");
            }

            int? nhaThauID = donDangKy.ID_NhaThau;
            string tenNhaThau = "";
            if (nhaThauID.HasValue)
            {
                var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == nhaThauID.Value);
                tenNhaThau = nhaThau?.FullName ?? "";
            }
            ViewBag.NhaThau_ID = nhaThauID;
            ViewBag.TenNhaThau = tenNhaThau;

            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 3)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();
            ViewBag.VP1C_List = new SelectList(VP1C, "IDNhanVien", "HoTen");

            var chiTietList = db_dk.KTNT_ChiTiet.AsNoTracking()
                .Where(x => x.MaDon == id)
                .Select(x => new KTNT_ChiTietVM
                {
                    ID = x.ID,
                    MaDon = x.MaDon,
                    TN_HoTen = x.TN_HoTen,
                    TN_CCCD_HoChieu = x.TN_CCCD_HoChieu,
                    TX_LoaiXeCoDong = x.TX_LoaiXeCoDong,
                    TX_BienKiemSoat = x.TX_BienKiemSoat,
                    PT_LoaiPhuongTien = x.PT_LoaiPhuongTien,
                    PT_BienKiemSoat = x.PT_BienKiemSoat,
                    GhiChu = x.GhiChu,
                    NgayTao = x.NgayTao
                }).ToList();

            var model = new TaoDonDangKyViewModel
            {
                ID = donDangKy.ID,
                MaDon = donDangKy.MaDon,
                NoiDung = donDangKy.NoiDung,
                ID_NhaThau = donDangKy.ID_NhaThau,
                BP_XuLy_ID = donDangKy.BP_XuLy_ID,
                NgayTao = donDangKy.NgayTao,
                ChiTiet = chiTietList ?? new System.Collections.Generic.List<KTNT_ChiTietVM>()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(TaoDonDangKyViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.MaDon))
                    return Json(new { success = false, message = "Mã đơn không hợp lệ" });

                if (model.ChiTiet == null || !model.ChiTiet.Any())
                    return Json(new { success = false, message = "Danh sách chi tiết không được trống" });

                if (!model.BP_XuLy_ID.HasValue)
                    return Json(new { success = false, message = "Vui lòng chọn phòng ban quản lý" });

                var donDangKy = db_dk.KTNT_DonDangKy.FirstOrDefault(x => x.MaDon == model.MaDon);
                if (donDangKy == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn đăng ký" });

                if (donDangKy.UserNameLogin != Models.MyAuthentication.Username)
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa đơn này" });

                if (donDangKy.TinhTrang != (int)TinhTrangDonDangKyKhoaThe.ChuaTrinhKy)
                    return Json(new { success = false, message = "Đơn đã được duyệt, không thể chỉnh sửa" });

                // Cập nhật master
                donDangKy.NoiDung = model.NoiDung;
                donDangKy.ID_NhaThau = model.ID_NhaThau;
                donDangKy.BP_XuLy_ID = model.BP_XuLy_ID;

                // Xóa chi tiết cũ
                var oldChiTiet = db_dk.KTNT_ChiTiet.Where(x => x.MaDon == model.MaDon).ToList();
                db_dk.KTNT_ChiTiet.RemoveRange(oldChiTiet);

                // Thêm chi tiết mới
                foreach (var item in model.ChiTiet)
                {
                    db_dk.KTNT_ChiTiet.Add(new KTNT_ChiTiet
                    {
                        MaDon = model.MaDon,
                        TN_HoTen = (item.TN_HoTen ?? "").Trim(),
                        TN_CCCD_HoChieu = (item.TN_CCCD_HoChieu ?? "").Trim(),
                        TX_LoaiXeCoDong = item.TX_LoaiXeCoDong,
                        TX_BienKiemSoat = (item.TX_BienKiemSoat ?? "").Trim(),
                        PT_LoaiPhuongTien = (item.PT_LoaiPhuongTien ?? "").Trim(),
                        PT_BienKiemSoat = (item.PT_BienKiemSoat ?? "").Trim(),
                        GhiChu = item.GhiChu,
                        NgayTao = DateTime.Now
                    });
                }

                db_dk.SaveChanges();

                return Json(new { success = true, message = "Cập nhật đơn thành công", maDon = model.MaDon });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    
        public ActionResult Detail(string maDon)
        {
            var data = db_dk.Database.SqlQuery<KTNT_DonDangKy_Detail>(
                "EXEC KTNT_DonDangKy_Detail @MaDon",
                new SqlParameter("@MaDon", maDon ?? (object)DBNull.Value)
            ).ToList();
            if (data == null || !data.Any())
            {
                // Có thể trả View với model rỗng hoặc Redirect/Thông báo
                return View(new List<KTNT_DonDangKy_Detail>());
            }
            return View(data);
        }
        [HttpPost]
        public ActionResult TrinhKy(string maDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDon))
                    return Json(new { success = false, message = "Thiếu mã đơn." });

                var don = db_dk.KTNT_DonDangKy.FirstOrDefault(x => x.MaDon == maDon);
                if (don == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn." });

                // Chỉ người tạo được trình ký
                var currentUser = Models.MyAuthentication.Username ?? string.Empty;
                if (!string.Equals(currentUser, don.UserNameLogin ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, message = "Bạn không có quyền trình ký đơn này." });

                // Chỉ trình ký khi đang ở trạng thái Chưa trình ký
                if (don.TinhTrang != (int)TinhTrangDonDangKyKhoaThe.ChuaTrinhKy)
                    return Json(new { success = false, message = "Đơn không ở trạng thái 'Chưa trình ký'." });

                // Kiểm tra điều kiện tối thiểu
                if (don.ID_NhaThau == null)
                    return Json(new { success = false, message = "Chưa chọn nhà thầu." });

                if (don.BP_XuLy_ID <= 0)
                    return Json(new { success = false, message = "Chưa chọn bộ phận xử lý." });

                bool hasDetails = db_dk.KTNT_ChiTiet.Any(ct => ct.MaDon == maDon);
                if (!hasDetails)
                    return Json(new { success = false, message = "Danh sách chi tiết trống." });

                don.TinhTrang = (int)TinhTrangDonDangKyKhoaThe.ChoXuLy;
                db_dk.SaveChanges();

                return Json(new { success = true, message = "Đã trình ký. Trạng thái chuyển sang 'Chờ xử lý'." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult HuyTrinhKy(string maDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDon))
                    return Json(new { success = false, message = "Thiếu mã đơn." });

                var don = db_dk.KTNT_DonDangKy.FirstOrDefault(x => x.MaDon == maDon);
                if (don == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn." });

                var currentUser = Models.MyAuthentication.Username ?? string.Empty;
                if (!string.Equals(currentUser, don.UserNameLogin ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, message = "Bạn không có quyền hủy trình ký." });

                if (don.TinhTrang != (int)TinhTrangDonDangKyKhoaThe.ChoXuLy)
                    return Json(new { success = false, message = "Chỉ hủy khi trạng thái 'Chờ xử lý'." });

                don.TinhTrang = (int)TinhTrangDonDangKyKhoaThe.ChuaTrinhKy;
                db_dk.SaveChanges();

                return Json(new { success = true, message = "Đã hủy trình ký thành công." });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có hệ thống logging
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Delete(string maDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDon))
                    return Json(new { success = false, message = "Mã đơn không hợp lệ." });

                var don = db_dk.KTNT_DonDangKy.FirstOrDefault(x => x.MaDon == maDon);
                if (don == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn." });

                bool daKy = db_dk.KTNT_DonDangKy.Any(x =>
                    x.MaDon == maDon && x.TinhTrang != (int)TinhTrangDonDangKyKhoaThe.ChuaTrinhKy);

                if (daKy)
                    return Json(new { success = false, message = "Đơn đã được ký, không thể xóa." });

                // Gọi stored procedure với tham số an toàn
                var maDonParam = new SqlParameter("@p_MaDon", maDon);

                var result = db_dk.Database.SqlQuery<StoreResult>(
                    "EXEC KTNT_DonDangKy_Delete @p_MaDon", maDonParam
                ).FirstOrDefault();

                return Json(new
                {
                    success = (result?.Result == 1),
                    message = result?.Message ?? "Xóa thất bại."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        //[HttpPost]
        //public ActionResult DuyetDon(string maDon, bool isApproved, string ghiChu = "")
        //{
        //    try
        //    {
        //        var userId = Models.MyAuthentication.ID;

        //        var don = db_dk.KTNT_DonDangKy.FirstOrDefault(x => x.MaDon == maDon);
        //        if (don == null)
        //            return Json(new { success = false, message = "Không tìm thấy đơn" });

        //        if (don.BP_XuLy_ID != userId)
        //            return Json(new { success = false, message = "Bạn không có quyền xử lý đơn này." });

        //        if (don.TinhTrang != (int)TinhTrangDonDangKyKhoaThe.ChoXuLy)
        //            return Json(new { success = false, message = "Đơn không còn ở trạng thái chờ xử lý." });
        //        don.TinhTrang = isApproved
        //       ? (int)TinhTrangDonDangKyKhoaThe.DaXuLy
        //       : (int)TinhTrangDonDangKyKhoaThe.KhongDatYeuCau;
        //        don.GhiChu = ghiChu;

        //        db_dk.SaveChanges();
        //        string msg = isApproved ? "Đã duyệt đơn thành công." : "Đã từ chối đơn.";
        //        return Json(new { success = true, message = msg });

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
        //    }

        //}
        [HttpPost]
        public ActionResult DuyetDon(string maDon, bool isApproved, string ghiChu = "")
        {
            try
            {
                var userId = Models.MyAuthentication.ID;
                var don = db_dk.KTNT_DonDangKy.FirstOrDefault(x => x.MaDon == maDon);
                if (don == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn" });

                if (don.BP_XuLy_ID != userId)
                    return Json(new { success = false, message = "Bạn không có quyền xử lý đơn này." });

                if (don.TinhTrang != (int)TinhTrangDonDangKyKhoaThe.ChoXuLy)
                    return Json(new { success = false, message = "Đơn không còn ở trạng thái chờ xử lý." });

                don.TinhTrang = isApproved
                    ? (int)TinhTrangDonDangKyKhoaThe.DaXuLy
                    : (int)TinhTrangDonDangKyKhoaThe.KhongDatYeuCau;
                don.GhiChu = ghiChu;

                // Chỉ thực hiện cập nhật nhân viên khi đơn được duyệt
                if (isApproved)
                {
                    // Lấy danh sách chi tiết đơn
                    var lstChiTiet = db_dk.KTNT_ChiTiet.Where(x => x.MaDon == maDon).ToList();
                    foreach (var item in lstChiTiet)
                    {
                        var cccd = item.TN_CCCD_HoChieu;
                        if (!string.IsNullOrEmpty(cccd))
                        {
                            // Kết nối sang db nhân viên - giả định context là db_nv
                            var nv = db_nt.NT_NhanVienNT.FirstOrDefault(n => n.CCCD == cccd);
                            if (nv != null)
                            {
                                nv.TTLV = 2;
                            }
                        }
                    }
                    db_nt.SaveChanges();
                    // Lấy danh sách các biển số xe từ chi tiết đơn
                    foreach (var item in lstChiTiet)
                    {
                        string bienSoXe = item.TX_BienKiemSoat; // hoặc PT_BienKiemSoat, tuỳ thực tế

                        if (!string.IsNullOrEmpty(bienSoXe))
                        {
                            // Tìm các bản ghi cần cập nhật trong CDNT_ChiTietDon
                            var cdnt = db_dk.CDNT_ChiTietDon
                                .Where(x => x.BienSoXe == bienSoXe)
                                .ToList();

                            foreach (var dong in cdnt)
                            {
                                // Cập nhật trạng thái
                                dong.TTHD = 3;

                            }
                        }
                    }
                }
              
                db_dk.SaveChanges();
                string msg = isApproved ? "Đã duyệt đơn thành công." : "Đã từ chối đơn.";
                return Json(new { success = true, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        public ActionResult HPDQ_Detail(string maDon)
        {
            var data = db_dk.Database.SqlQuery<KTNT_DonDangKy_Detail>(
                "EXEC KTNT_DonDangKy_Detail @MaDon",
                new SqlParameter("@MaDon", maDon ?? (object)DBNull.Value)
            ).ToList();
            if (data == null || !data.Any())
            {
                
                return View(new List<KTNT_DonDangKy_Detail>());
            }
            return View(data);
        }
        public ActionResult HPDQ_Index(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;
            // var userNameLogin = Models.MyAuthentication.Username;
            int userId = Models.MyAuthentication.ID;

            var data = db_dk.Database.SqlQuery<DonDangKyKhoaTheViewModel>(
               "EXEC KTNT_DonDangKy_Search @p_BeginDate, @p_EndDate,@p_MaPhieu",
               new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
               new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
               new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
           ).ToList();

            var filtered = data
                .Where(d => d.TinhTrang == (int)TinhTrangDonDangKyKhoaThe.ChoXuLy && d.BP_XuLy_ID == userId) 
                .ToList();

            var pagedData = filtered.ToPagedList(pageNumber, pageSize);
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(pagedData);
        }
        public JsonResult HPDQ_KTNotify()
        {
            int userId = Models.MyAuthentication.ID;

            int total = db_dk.KTNT_DonDangKy.Count(x => x.BP_XuLy_ID == userId && x.TinhTrang == 1);
            return Json(new
            {
                Total = total
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportKhoaThePdf(string maDon)
        {
            var rows = GetKhoaTheData(maDon);
            if (rows == null || rows.Count == 0)
                return HttpNotFound("Không có dữ liệu.");

            var header = rows.First();
            string safe = SanitizeFileName(header.TenNhaThau ?? header.MaDon ?? "don_khoa_the") + ".pdf";

            // Dùng hàm tạo pdf tạm (giống GenerateTempPdf cũ)
            string tempPath = GenerateTempPdf_KhoaThe(maDon);

            byte[] pdfBytes = System.IO.File.ReadAllBytes(tempPath);
            System.IO.File.Delete(tempPath);

            return File(pdfBytes, "application/pdf", safe);
        }

        // Action để hiển thị preview (HTML) – Rotativa sẽ gọi lại Action này
        public ActionResult Detail_PDF(string maDon)
        {
            var rows = GetKhoaTheData(maDon);
            if (rows == null || rows.Count == 0)
                return HttpNotFound("Không có dữ liệu.");

            return View(rows); // View mạnh kiểu: List<KTNT_DonDangKy_Detail>
        }

        private List<KTNT_DonDangKy_Detail> GetKhoaTheData(string maDon)
        {
            if (string.IsNullOrWhiteSpace(maDon)) return new List<KTNT_DonDangKy_Detail>();

            // EXEC SP đơn giản
            var rows = db_dk.Database.SqlQuery<KTNT_DonDangKy_Detail>(
                "EXEC KTNT_DonDangKy_Detail_PDF @MaDon",
                new SqlParameter("@MaDon", maDon)
            ).ToList();

            return rows ?? new List<KTNT_DonDangKy_Detail>();
        }

        private string GenerateTempPdf_KhoaThe(string maDon)
        {
            string root = Server.MapPath("~/UploadedFiles/PDFDangKyThe/");
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            string tempPdf = Path.Combine(root, $"{Guid.NewGuid()}.pdf");

            var pdf = new Rotativa.ActionAsPdf("Detail_PDF", new { maDon })
            {
                PageSize = Rotativa.Options.Size.A4,
                PageMargins = new Rotativa.Options.Margins(13, 5, 10, 5),
                CustomSwitches = "--encoding utf-8"
            };

            byte[] bytes = pdf.BuildPdf(ControllerContext);
            System.IO.File.WriteAllBytes(tempPdf, bytes);
            return tempPdf;
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "don_khoa_the";
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            if (name.Length > 80) name = name.Substring(0, 80);
            return name.Trim('_');
        }

        public ActionResult HPDQ_IndexAll(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;
            // var userNameLogin = Models.MyAuthentication.Username;

            var data = db_dk.Database.SqlQuery<DonDangKyKhoaTheViewModel>(
               "EXEC KTNT_DonDangKy_Search @p_BeginDate, @p_EndDate,@p_MaPhieu",
               new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
               new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
               new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
           ).ToList();
            var filtered = data
                .Where(d => d.TinhTrang == (int)TinhTrangDonDangKyKhoaThe.DaXuLy
                         || d.TinhTrang == (int)TinhTrangDonDangKyKhoaThe.KhongDatYeuCau)
                .ToList();

            
            var pagedData = filtered.ToPagedList(pageNumber, pageSize);
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(pagedData);
        }
        public ActionResult HPDQ_GetAll(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;
            // var userNameLogin = Models.MyAuthentication.Username;
            int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
            string controll = "DangKyKhoaThe_NT";
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<div class='alert alert-danger'>Bạn không có quyền xem dữ liệu này.</div>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var data = db_dk.Database.SqlQuery<DonDangKyKhoaTheViewModel>(
               "EXEC KTNT_DonDangKy_Search @p_BeginDate, @p_EndDate,@p_MaPhieu",
               new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
               new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
               new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
           ).ToList();
            var filtered = data.ToList();

            var pagedData = filtered.ToPagedList(pageNumber, pageSize);
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(pagedData);
        }
        [HttpGet]
        public ActionResult SearchXeByNhaThau(
            int idNhaThau,
            string q = "",
            int page = 1,
            int pageSize = 20,
            int? loaiPhuongTienId = null  
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                q = (q ?? "").Trim();

                // Gọi SP (trả: ID, ID_LoaiPhuongTien, BienSoXe, ...)
                var data = db_dk.Database.SqlQuery<VehicleRow>(
                    "EXEC [dbo].[GetDanhSachXe_ByNhaThau] @ID_NhaThau",
                    new SqlParameter("@ID_NhaThau", idNhaThau)
                ).ToList();

                // Lọc theo loại (nếu có)
                if (loaiPhuongTienId.HasValue)
                {
                    data = data.Where(x => x.ID_LoaiPhuongTien == loaiPhuongTienId.Value).ToList();
                }

                // Lọc theo từ khóa (biển số)
                if (!string.IsNullOrWhiteSpace(q))
                {
                    data = data
                        .Where(x => !string.IsNullOrEmpty(x.BienSoXe)
                                    && x.BienSoXe.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                // Loại trùng theo biển số (nếu có)
                var distinct = data
                    .Where(x => !string.IsNullOrEmpty(x.BienSoXe))
                    .GroupBy(x => x.BienSoXe)
                    .Select(g => new
                    {
                        BienSoXe = g.Key,
                        LoaiPhuongTienId = g.Max(i => i.ID_LoaiPhuongTien) // giữ nguyên 1/2/3
                    });

                var total = distinct.Count();

                var paged = distinct
                    .OrderBy(x => x.BienSoXe)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var results = paged.Select(x => new
                {
                    id = x.BienSoXe,
                    text = x.BienSoXe,
                    loaiPhuongTienId = x.LoaiPhuongTienId
                });

                var more = page * pageSize < total;

                return Json(new
                {
                    results,
                    pagination = new { more }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    results = new object[0],
                    pagination = new { more = false },
                    error = "Lỗi tìm kiếm: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private class VehicleRow
        {
            public int Id { get; set; }
            public string BienSoXe { get; set; }
            public int? ID_LoaiPhuongTien { get; set; }
        }


        [HttpGet]
        public ActionResult GetNhanVienNTByCCCD(int idNT, string cccd)
         {
            try
            {
                // cccd null/empty => trả về TOÀN BỘ danh sách theo IDNT (TTLV=1)
                var list = db_dk.Database.SqlQuery<NhanVienNtRow>(
                    "EXEC [dbo].[GetNhanVienNT_ByIDNT] @p_IDNT, @p_CCCD",
                    new SqlParameter("@p_IDNT", idNT),
                    new SqlParameter("@p_CCCD", string.IsNullOrWhiteSpace(cccd) ? (object)DBNull.Value : cccd.Trim())
                ).ToList();

                var results = list.Select(nv =>
                {
                    var idValue = nv.CCCD ?? nv.CMND ?? "";
                    return new
                    {
                        id = idValue,                                   
                        text = idValue,                              
                        hoTen = nv.HoTen ?? ""
                    };
                });

                return Json(new { results }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { results = new object[0], error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POCO khớp với store
        private class NhanVienNtRow
        {
            public int IDNVNT { get; set; }
            public string HoTen { get; set; }
            public string CCCD { get; set; }
            public string CMND { get; set; }
            public string SoThe { get; set; }
            public int IDNT { get; set; }
            public DateTime? NgayCap { get; set; }
            public DateTime? HanSuDung { get; set; }
            public int? TTLV { get; set; }
            public int? ChucVuID { get; set; }
        }

    }
}


