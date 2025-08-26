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

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class TheXeCoDong_NTController : Controller
    {
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        //int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        //String controll = "ContractorGroup";
        // GET: TagSign/ContractorGroup

        public ActionResult Index_Test(DateTime? begind, DateTime? endd, int? page)
        {

            // Phân trang ở Controller
            int pageNumber = page ?? 1;
            int pageSize = 10;

            // Gọi store, chỉ truyền filter ngày
            var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
                "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate",
                new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value)
            ).ToList();

            // Truyền vào PagedList để phân trang
            var pagedData = data.ToPagedList(pageNumber, pageSize);

            // Lưu lại giá trị filter để hiển thị lại trên view
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(pagedData);
        }
        public ActionResult HPDQ_Index(DateTime? begind, DateTime? endd, int? page)
        {

            // Phân trang ở Controller
            int pageNumber = page ?? 1;
            int pageSize = 10;

            // Gọi store, chỉ truyền filter ngày
            var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
                "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate",
                new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value)
            ).ToList();

            // Truyền vào PagedList để phân trang
            var pagedData = data.ToPagedList(pageNumber, pageSize);

            // Lưu lại giá trị filter để hiển thị lại trên view
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(pagedData);
        }
        public ActionResult HPDQ_Detail(string maDon)
        {
            var data = db_dk.Database.SqlQuery<CDNT_DonDangKyDetail>(
                "EXEC CDNT_DonDangKy_Detail @Ma_Don",
                new SqlParameter("@Ma_Don", maDon ?? (object)DBNull.Value)
            ).ToList();

            return View(data);
        }

        public ActionResult Create(List<ChiTietDonVM> danhSach = null)
        {
            string tenNhaThau = "";
            int nhanVienNT_ID = Models.MyAuthentication.ID;

            var nhanVien = db_nt.NT_NhanVienNT.FirstOrDefault(x => x.IDNVNT == nhanVienNT_ID);

            if (nhanVien != null && nhanVien.IDNT.HasValue)
            {
                int nhaThauID = nhanVien.IDNT.Value;

                var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == nhaThauID);

                if (nhaThau != null)
                {
                    tenNhaThau = nhaThau.FullName;

                    // Gán dropdown mặc định chọn nhà thầu hiện tại
                    ViewBag.IDNT = new SelectList(db.NT_Partner.ToList(), "ID", "FullName", nhaThauID);
                }
                else
                {
                    ViewBag.IDNT = new SelectList(db.NT_Partner.ToList(), "ID", "FullName");
                }
            }
            else
            {
                ViewBag.IDNT = new SelectList(db.NT_Partner.ToList(), "ID", "FullName");
            }

            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 3)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();
            ViewBag.VP1C_List = new SelectList(VP1C, "IDNhanVien", "HoTen");

           
            ViewBag.TenNhaThau = tenNhaThau;

            ViewBag.IDLTK = new SelectList(db_dk.SignerTypes.ToList(), "ID_LTK", "TenLoai");
            ViewBag.IDPhongBan = new SelectList(db.PhongBans.Where(x => x.status == 1).ToList(), "IDPhongBan", "TenPhongBan");
            
            return View(danhSach ?? new List<ChiTietDonVM>());
        }

        [HttpPost]
        public ActionResult Create(DonDangKyInsertModel model, HttpPostedFileBase FileHoSoXe, int? KTV_ID, int? TP_ID, int? VP1C_ID)
        {
            try
            {
                // 1. Kiểm tra danh sách xe
                if (string.IsNullOrWhiteSpace(model.JsonDanhSachXe))
                {
                    return Json(new { success = false, message = "Danh sách xe không được trống" });
                }

                // 2. Xử lý file upload (nếu có)
                if (FileHoSoXe != null && FileHoSoXe.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(FileHoSoXe.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
                    FileHoSoXe.SaveAs(filePath);

                    // Lưu tên file (đường dẫn tương đối)
                    model.FileHoSoXe = fileName;
                }

                // 3. Tạo mã đơn tự động nếu chưa có
                if (string.IsNullOrEmpty(model.Ma_Don))
                {
                    var thangNam = DateTime.Now.ToString("yyyyMMdd");
                    var prefix = $"DON_CDNT{thangNam}-";

                    var lastMaDon = db_dk.CDNT_DonDangKy
                        .Where(x => x.Ma_Don.StartsWith(prefix))
                        .OrderByDescending(x => x.Ma_Don)
                        .Select(x => x.Ma_Don)
                        .FirstOrDefault();

                    int stt = 1;
                    if (!string.IsNullOrEmpty(lastMaDon))
                    {
                        var soStr = lastMaDon.Substring(prefix.Length);
                        int.TryParse(soStr, out stt);
                        stt++;
                    }

                    model.Ma_Don = prefix + stt.ToString("D4");
                }

                // 4. Gán người tạo đơn là người đang đăng nhập
                model.NhanVienNT_ID = Models.MyAuthentication.ID;

                // 5. Insert đơn vào DB (thông qua stored procedure hoặc EF)
                var result = db_dk.CDNT_DonDangKy_Insert(
                    model.Ma_Don,
                    model.NoiDung,
                    model.PhongBan_ID,
                    model.NhanVienNT_ID,
                    model.NhaThau_ID,
                    model.HopDong,
                    model.NgayTrinhKy,
                    model.FileHoSoXe,
                    model.TrinhKy_ID,
                    model.TinhTrang_ID,
                    model.LoaiNT_ID,
                    model.JsonDanhSachXe
                ).FirstOrDefault();

                if (result?.Result == 1)
                {
                    string maDon = result.MaDon;
                    int nguoiTaoDon_ID = (int)model.NhanVienNT_ID;

                    // Trình ký từ nhà thầu (Cấp 0)
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = maDon,
                        CapDuyet = 0,
                        NguoiDuyet_ID = nguoiTaoDon_ID,
                        NgayDuyet = DateTime.Now,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                        GhiChu = "Trình ký từ nhà thầu"
                    });

                    // Trình ký cấp 1 - KTV
                    if (KTV_ID.HasValue)
                    {
                        db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                        {
                            Ma_Don = maDon,
                            CapDuyet = 1,
                            NguoiDuyet_ID = KTV_ID.Value,
                            TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                            GhiChu = "Chờ duyệt - Kỹ thuật viên"
                        });
                    }

                    // Trình ký cấp 1 - Trưởng/Phó phòng
                    if (TP_ID.HasValue)
                    {
                        db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                        {
                            Ma_Don = maDon,
                            CapDuyet = 1,
                            NguoiDuyet_ID = TP_ID.Value,
                            TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                            GhiChu = "Chờ duyệt cấp - Trưởng/Phó phòng"
                        });
                    }

                    // Trình ký cấp 2 - VP1C (CPT)
                    if (VP1C_ID.HasValue)
                    {
                        db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                        {
                            Ma_Don = maDon,
                            CapDuyet = 2,
                            NguoiDuyet_ID = VP1C_ID.Value,
                            TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                            GhiChu = "Chờ cấp phát thẻ từ CPT"
                        });
                    }
                    // 7. Lưu toàn bộ thay đổi
                    db_dk.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        message = "Tạo đơn thành công và khởi tạo trình ký",
                        maDon = maDon
                    });
                }
                else
                {
                    return Json(new { success = false, message = result?.Message ?? "Tạo đơn thất bại" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public ActionResult Detail(string maDon)
        {
            var data = db_dk.Database.SqlQuery<CDNT_DonDangKyDetail>(
                "EXEC CDNT_DonDangKy_Detail @Ma_Don",
                new SqlParameter("@Ma_Don", maDon ?? (object)DBNull.Value)
            ).ToList();

            return View(data);
        }
        [HttpPost]
        public ActionResult Delete(string maDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDon))
                {
                    return Json(new { success = false, message = "Mã đơn không hợp lệ." });
                }

                // Kiểm tra đơn đã trình ký chưa
                var don = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == maDon);
                if (don == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn." });
                }

                if (don.TrinhKy_ID.HasValue && don.TrinhKy_ID.Value > 0)
                {
                    return Json(new { success = false, message = "Đơn đã trình ký, không thể xóa." });
                }

                // Nếu chưa trình ký, gọi stored procedure xóa
                var result = db_dk.Database.SqlQuery<StoreResult>(
                    "EXEC CDNT_DonDangKy_Delete @p0", maDon
                ).FirstOrDefault();

                if (result != null && result.Result == 1)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result?.Message ?? "Xóa thất bại." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }      
        [HttpPost]
        public JsonResult ImportExcel(HttpPostedFileBase FileANH)
        {
            var danhSach = new List<ChiTietDonVM>();

            try
            {
                int nhanVienNT_ID = Models.MyAuthentication.ID;

                using (var db_nt = new EPORTAL_NTEntities())
                using (var db = new EPORTALEntities())
                {
                    // Lấy thông tin nhân viên theo nhanVienNT_ID
                    var nhanVien = db_nt.NT_NhanVienNT.FirstOrDefault(x => x.IDNVNT == nhanVienNT_ID);
                    if (nhanVien == null || !nhanVien.IDNT.HasValue)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Không tìm thấy thông tin Nhà thầu tương ứng với nhân viên đăng nhập."
                        }, JsonRequestBehavior.AllowGet);
                    }

                    int nhaThauID = nhanVien.IDNT.Value;

                    // (Bạn có thể lấy tên nhà thầu nếu cần)
                    var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == nhaThauID);
                    string tenNhaThau = nhaThau?.FullName ?? "";

                    // Bắt đầu đọc file Excel
                    if (FileANH != null && FileANH.ContentLength > 0)
                    {
                        using (var workbook = new XLWorkbook(FileANH.InputStream))
                        {
                            var ws = workbook.Worksheet(1);
                            int row = 5;

                            while (!string.IsNullOrWhiteSpace(ws.Cell(row, 5).GetString()))
                            {
                                string thoiHan = ws.Cell(row, 9).GetString().Replace("–", "-").Replace("—", "-");
                                DateTime? tuNgay = null, denNgay = null;

                                var parts = thoiHan.Split('-');
                                string format = "dd/MM/yyyy";
                                DateTime tempDate;

                                if (parts.Length == 2)
                                {
                                    if (DateTime.TryParseExact(parts[0].Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                                        tuNgay = tempDate;

                                    if (DateTime.TryParseExact(parts[1].Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                                        denNgay = tempDate;
                                }

                                var vm = new ChiTietDonVM
                                {
                                    ID_LoaiPhuongTien =
                                        ws.Cell(row, 2).GetString().Trim().ToUpper() == "X" ? 1 :
                                        ws.Cell(row, 3).GetString().Trim().ToUpper() == "X" ? 2 :
                                        ws.Cell(row, 4).GetString().Trim().ToUpper() == "X" ? 3 : (int?)null,

                                    BienSoXe = ws.Cell(row, 5).GetString(),
                                    CapMoi = ws.Cell(row, 6).GetString().Trim().ToUpper() == "X",
                                    CapLai = ws.Cell(row, 7).GetString().Trim().ToUpper() == "X",
                                    GiaHan = ws.Cell(row, 8).GetString().Trim().ToUpper() == "X",
                                    TuNgay = tuNgay,
                                    DenNgay = denNgay,
                                    GhiChu = ws.Cell(row, 10).GetString()
                                };

                                danhSach.Add(vm);
                                row++;
                            }
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Vui lòng chọn file Excel hợp lệ." }, JsonRequestBehavior.AllowGet);
                    }

                    var dinhBien = DinhBienPhuongTienService.LayDinhBienTheoNhaThau(nhaThauID);

                    // Đếm số xe máy và 3 gác đăng ký cấp mới trong file import
                    int soXeMayMoi = danhSach.Count(x => x.ID_LoaiPhuongTien == 1 && x.CapMoi);
                    int soXe3GacMoi = danhSach.Count(x => x.ID_LoaiPhuongTien == 3 && x.CapMoi);

                    // Kiểm tra vượt định biên
                    if (soXeMayMoi > dinhBien.XeMay_ConLai || soXe3GacMoi > dinhBien.Xe3Gac_ConLai)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Vượt định biên: Xe máy (còn lại {dinhBien.XeMay_ConLai}, đang đăng ký {soXeMayMoi}), " +
                                    $"Xe 3 gác (còn lại {dinhBien.Xe3Gac_ConLai}, đang đăng ký {soXe3GacMoi})"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    // Nếu OK, trả dữ liệu về client
                    return Json(new
                    {
                        success = true,
                        tenNhaThau = tenNhaThau,
                        data = danhSach.Select(x => new
                        {
                            x.ID_LoaiPhuongTien,
                            x.BienSoXe,
                            x.CapMoi,
                            x.CapLai,
                            x.GiaHan,
                            TuNgay = x.TuNgay?.ToString("yyyy-MM-dd"),
                            DenNgay = x.DenNgay?.ToString("yyyy-MM-dd"),
                            x.GhiChu
                        })
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Update(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound("Mã đơn không hợp lệ.");

            var donDangKi = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == id);
            if (donDangKi == null)
                return HttpNotFound("Không tìm thấy đơn đăng ký.");

            var danhSachXe = db_dk.CDNT_ChiTietDon
                .Where(x => x.Ma_Don == id)
                .ToList()
                .Select(ChiTietDonVM.FromModel)
                .ToList();

            ViewBag.IDNT = new SelectList(db.NT_Partner.ToList(), "ID", "FullName", donDangKi.NhaThau_ID);
            ViewBag.IDPhongBan = new SelectList(db.PhongBans.Where(x => x.status == 1).ToList(), "IDPhongBan", "TenPhongBan", donDangKi.BPQL_ID);

            var model = new DonDangKyModel
            {
                ID = donDangKi.ID,
                Ma_Don = donDangKi.Ma_Don,
                NoiDung = donDangKi.NoiDung,
                BPQL_ID = donDangKi.BPQL_ID,
                NhanVienNT_ID = donDangKi.NhanVienNT_ID,
                NhaThau_ID = donDangKi.NhaThau_ID,
                HopDong = donDangKi.HopDong,
                NgayTrinhKy = donDangKi.NgayTrinhKy,
                FileHoSoXe = donDangKi.FileHoSoXe,
                TrinhKy_ID = donDangKi.TrinhKy_ID,
                TinhTrang_ID = donDangKi.TinhTrang_ID,
                LoaiNT_ID = donDangKi.LoaiNT_ID,
                DanhSachXe = danhSachXe
            };

            return View(model);
        }
        [HttpGet]
        public ActionResult GetDinhBienPhuongTien()
        {
            try
            {
                int nhanVienNT_ID = Models.MyAuthentication.ID;
                var nhanVien = db_nt.NT_NhanVienNT.FirstOrDefault(x => x.IDNVNT == nhanVienNT_ID);
                if (nhanVien == null || nhanVien.IDNT == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhà thầu." }, JsonRequestBehavior.AllowGet);
                }

                int nhaThauID = nhanVien.IDNT.Value;
                var data = DinhBienPhuongTienService.LayDinhBienTheoNhaThau(nhaThauID);

                return Json(new
                {
                    success = true,
                    data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LuongTrinhKy(string Ma_Don = null)
        {
            var data = db_dk.CDNT_TrinhKy_Detail(Ma_Don).ToList();
            return View(data);
        }
        [HttpGet]
        public ActionResult GetNguoiKyTheoPhongBan(int idPhongBan)
        {
            var ktvList = (from au in db.AuthorizationContractors
                           where au.IDLKD == 2 &&
                                 (au.NhanVien.IDPhongBan == idPhongBan || au.IDCVKN.Contains(idPhongBan.ToString()))
                           join a in db.NhanViens on au.IDNhanVien equals a.ID
                           select new
                           {
                               ID = au.IDNhanVien,
                               HoTen = a.HoTen + " : " + a.MaNV
                           }).ToList();

            var tpList = (from au in db.AuthorizationContractors
                          where au.IDLKD == 1 &&
                                (au.NhanVien.IDPhongBan == idPhongBan || au.IDCVKN.Contains(idPhongBan.ToString()))
                          join a in db.NhanViens on au.IDNhanVien equals a.ID
                          select new
                          {
                              ID = au.IDNhanVien,
                              HoTen = a.HoTen + " : " + a.MaNV
                          }).ToList();

            return Json(new
            {
                success = true,
                ktvList,
                tpList
            }, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public ActionResult DuyetDon(string maDon, bool isApproved, string ghiChu = "")
        //{
        //    var userId = Models.MyAuthentication.ID;

        //    var don = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == maDon);
        //    if (don == null)
        //        return Json(new { success = false, message = "Không tìm thấy đơn" });

        //    // Kiểm tra người duyệt có trong danh sách CDNT_TrinhKy với trạng thái chờ xử lý không
        //    var phanCong = db_dk.CDNT_TrinhKy.FirstOrDefault(x =>
        //        x.Ma_Don == maDon &&
        //        x.NguoiDuyet_ID == userId &&
        //        x.TinhTrang_ID == (int)TinhTrangDonDangKy.ChoXuLy
        //    );

        //    if (phanCong == null)
        //    {
        //        return Json(new { success = false, message = "Bạn không có quyền duyệt đơn này hoặc đã duyệt rồi." });
        //    }

        //    // Nếu người dùng có quyền duyệt thì cập nhật trạng thái đơn và trạng thái trình ký
        //    if (isApproved)
        //    {
        //        // Ví dụ cập nhật đơn thành "Đã duyệt"
        //        don.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;
        //        phanCong.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;
        //    }
        //    else
        //    {
        //        // Ví dụ cập nhật đơn thành "Không đạt yêu cầu"
        //        don.TinhTrang_ID = (int)TinhTrangDonDangKy.KhongDatYeuCau;
        //        phanCong.TinhTrang_ID = (int)TinhTrangDonDangKy.KhongDatYeuCau;
        //    }

        //    phanCong.NgayDuyet = DateTime.Now;
        //    phanCong.GhiChu = ghiChu;

        //    db_dk.SaveChanges();

        //    return Json(new { success = true, message = "Đơn đã được xử lý" });
        //}

        // service tính định biên phương tiện dựa theo nhân viên còn hoạt động của nhà thầu

        [HttpPost]
        public ActionResult DuyetDon(string maDon, bool isApproved, string ghiChu = "")
        {
            var userId = Models.MyAuthentication.ID;

            var don = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == maDon);
            if (don == null)
                return Json(new { success = false, message = "Không tìm thấy đơn" });

            var phanCong = db_dk.CDNT_TrinhKy.FirstOrDefault(x =>
                x.Ma_Don == maDon &&
                x.NguoiDuyet_ID == userId &&
                x.TinhTrang_ID == (int)TinhTrangDonDangKy.ChoXuLy
            );

            if (phanCong == null)
            {
                return Json(new { success = false, message = "Bạn không có quyền duyệt đơn này hoặc đã duyệt rồi." });
            }

            if (isApproved)
            {
                don.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;
                phanCong.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;

                var dsXe = db_dk.CDNT_ChiTietDon.Where(x => x.Ma_Don == maDon).ToList();
                var bienSoBiTuChoi = Request.Form["bienSoBiTuChoi"]?.Split(',')?.ToList() ?? new List<string>();

                foreach (var xe in dsXe)
                {
                    if (bienSoBiTuChoi.Contains(xe.BienSoXe))
                    {
                        xe.TrangThaiDuyet_ID = 2; // Từ chối
                    }
                    else
                    {
                        xe.TrangThaiDuyet_ID = 1; // Được duyệt
                    }
                }
            }
            else
            {
                don.TinhTrang_ID = (int)TinhTrangDonDangKy.KhongDatYeuCau;
                phanCong.TinhTrang_ID = (int)TinhTrangDonDangKy.KhongDatYeuCau;

                var dsXe = db_dk.CDNT_ChiTietDon.Where(x => x.Ma_Don == maDon).ToList();
                foreach (var xe in dsXe)
                {
                    xe.TrangThaiDuyet_ID = 2; // Từ chối
                }
            }

            phanCong.NgayDuyet = DateTime.Now;
            phanCong.GhiChu = ghiChu;

            db_dk.SaveChanges();

            return Json(new { success = true, message = "Đơn đã được xử lý." });
        }
        public ActionResult Detail_PDF(string maDon)
        {
            var data = db_dk.Database.SqlQuery<CDNT_DonDangKyDetail>(
                "EXEC CDNT_DonDangKy_Detail @Ma_Don",
                new SqlParameter("@Ma_Don", maDon ?? (object)DBNull.Value)
            ).ToList();

            return View(data);
        }
        public ActionResult ExportDonDangKyPdf(string maDon)
        {
            // 1. Lấy dữ liệu chi tiết đơn từ DB  
            var data = db_dk.Database.SqlQuery<CDNT_DonDangKyDetail>(
                "EXEC CDNT_DonDangKy_Detail @Ma_Don",
                new SqlParameter("@Ma_Don", maDon ?? (object)DBNull.Value)
            ).ToList();
            if (data == null || !data.Any())
                return HttpNotFound("Không có dữ liệu.");

            // 2. Gọi Render View → Temporary PDF bằng Rotativa
            string tempPdfPath = GenerateTempPdf(maDon);

            // 3. Đọc file tạm và chèn watermark
            byte[] finalPdf = AddWatermarkToPdf(tempPdfPath, watermarkText: "TÀI LIỆU NỘI BỘ - HÒA PHÁT DUNG QUẤT");

            // 4. Xóa file tạm
            System.IO.File.Delete(tempPdfPath);

            // 5. Trả file PDF về người dùng
            string safeFileName = SanitizeFileName(data.First().FullName) + ".pdf";
            return File(finalPdf, "application/pdf", safeFileName);
        }
        private string GenerateTempPdf(string maDon)
        {
            string root = Server.MapPath("~/UploadedFiles/PDFDangKyThe/");
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            string tempPdf = Path.Combine(root, $"{Guid.NewGuid()}.pdf");
            string footerHtml = Server.MapPath("~/Views/Shared/footer.html");

            var actionPdf = new Rotativa.ActionAsPdf("Detail_PDF", new { maDon = maDon })
            {
                PageSize = Rotativa.Options.Size.A4,
                PageMargins = new Rotativa.Options.Margins(13, 5, 10, 5),
                CustomSwitches = $"--footer-html \"{footerHtml}\" --footer-spacing 5 --footer-font-size 9 --footer-line --encoding utf-8"
            };
            byte[] pdfBytes = actionPdf.BuildPdf(this.ControllerContext);
            System.IO.File.WriteAllBytes(tempPdf, pdfBytes);

            return tempPdf;
        }
        private byte[] AddWatermarkToPdf(string inputPath, string watermarkText)
        {
            using (var reader = new PdfReader(inputPath))
            using (var ms = new MemoryStream())
            {
                using (var stamper = new PdfStamper(reader, ms))
                {
                    int pageCount = reader.NumberOfPages;
                    iTextSharp.text.Font font = new iTextSharp.text.Font(
                         iTextSharp.text.Font.FontFamily.HELVETICA, 40, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.LIGHT_GRAY
                     );

                    PdfLayer layer = new PdfLayer("WatermarkLayer", stamper.Writer);

                    for (int i = 1; i <= pageCount; i++)
                    {
                        Rectangle rect = reader.GetPageSize(i);
                        PdfContentByte cb = stamper.GetUnderContent(i);
                        cb.BeginLayer(layer);
                        PdfGState gState = new PdfGState { FillOpacity = 0.1f };
                        cb.SetGState(gState);
                        cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false), 40);

                        float startY = rect.Height - 100;
                        while (startY > 0)
                        {
                            for (float x = 100; x < rect.Width; x += 300)
                            {
                                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                                    new Phrase(watermarkText, font),
                                    x, startY, 45);
                            }
                            startY -= 200;
                        }

                        cb.EndLayer();
                    }
                }
                return ms.ToArray();
            }
        }
        private string SanitizeFileName(string input)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars());
            string pattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(input, pattern, "_");
        }


        public class DinhBienPhuongTienService
        {
            public static DinhBienPhuongTienVM LayDinhBienTheoNhaThau(int nhaThauID)
            {
                var result = new DinhBienPhuongTienVM { NhaThauID = nhaThauID };

                var p_SoQuanLy = new ObjectParameter("p_SoQuanLy", typeof(int));
                var p_SoCongNhan = new ObjectParameter("p_SoCongNhan", typeof(int));
                var p_XeMay_DaCap = new ObjectParameter("p_XeMay_DaCap", typeof(int));
                var p_Xe3Gac_DaCap = new ObjectParameter("p_Xe3Gac_DaCap", typeof(int));
                var p_XeMay_ToiDa = new ObjectParameter("p_XeMay_ToiDa", typeof(int));
                var p_Xe3Gac_ToiDa = new ObjectParameter("p_Xe3Gac_ToiDa", typeof(int));
                var p_XeMay_ConLai = new ObjectParameter("p_XeMay_ConLai", typeof(int));
                var p_Xe3Gac_ConLai = new ObjectParameter("p_Xe3Gac_ConLai", typeof(int));

                using (var db = new EPORTAL_REGISTEREntities())
                {
                    db.CNDT_GetDinhBienPhuongTien(
                        nhaThauID,
                        p_SoQuanLy,
                        p_SoCongNhan,
                        p_XeMay_DaCap,
                        p_Xe3Gac_DaCap,
                        p_XeMay_ToiDa,
                        p_Xe3Gac_ToiDa,
                        p_XeMay_ConLai,
                        p_Xe3Gac_ConLai
                    );
                }

                result.SoQuanLy = (int)p_SoQuanLy.Value;
                result.SoCongNhan = (int)p_SoCongNhan.Value;
                result.XeMay_DaCap = (int)p_XeMay_DaCap.Value;
                result.Xe3Gac_DaCap = (int)p_Xe3Gac_DaCap.Value;
                result.XeMay_ToiDa = (int)p_XeMay_ToiDa.Value;
                result.Xe3Gac_ToiDa = (int)p_Xe3Gac_ToiDa.Value;
                result.XeMay_ConLai = (int)p_XeMay_ConLai.Value;
                result.Xe3Gac_ConLai = (int)p_Xe3Gac_ConLai.Value;

                return result;
            }
        }

    }
}