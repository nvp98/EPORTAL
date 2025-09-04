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

        public ActionResult Index_Test(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {

            // Phân trang ở Controller
            int pageNumber = page ?? 1;
            int pageSize = 10;

            // Gọi store, chỉ truyền filter ngày
            var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
                "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate,@p_MaPhieu",
                new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
                new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
            ).ToList();

            // Truyền vào PagedList để phân trang
            var pagedData = data.ToPagedList(pageNumber, pageSize);

            // Lưu lại giá trị filter để hiển thị lại trên view
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.MaPhieu = maPhieu;
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
            if (data == null || !data.Any())
            {
                // Có thể trả View với model rỗng hoặc Redirect/Thông báo
                return View(new List<CDNT_DonDangKyDetail>());
            }
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
                var daKy = db_dk.CDNT_TrinhKy
                 .Any(x => x.Ma_Don == maDon && x.TinhTrang_ID != (int)TinhTrangDonDangKy.ChoXuLy);

                if (daKy)
                {
                    return Json(new { success = false, message = "Đơn đã được ký, không thể xóa." });
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
            {
                return HttpNotFound();
            }

            // Lấy thông tin đơn đăng ký
            var donDangKy = db_dk.CDNT_DonDangKy.AsNoTracking().FirstOrDefault(x => x.Ma_Don == id);
            if (donDangKy == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra quyền chỉnh sửa
            if (donDangKy.NhanVienNT_ID != Models.MyAuthentication.ID)
            {
                TempData["msgError"] = "Bạn không có quyền chỉnh sửa đơn này";
                return RedirectToAction("Index_Test");
            }

            // Kiểm tra trạng thái đơn
            if (donDangKy.TinhTrang_ID != (int)TinhTrangDonDangKy.ChoXuLy)
            {
                TempData["msgError"] = "Đơn đã được duyệt, không thể chỉnh sửa";
                return RedirectToAction("Index_Test");
            }

            // Lấy thông tin nhà thầu
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

            // Lấy danh sách phòng ban
            ViewBag.IDPhongBan = new SelectList(db.PhongBans.Where(x => x.status == 1).ToList(), "IDPhongBan", "TenPhongBan", donDangKy.BPQL_ID);

            // Lấy danh sách VP1C
            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 3)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();
            ViewBag.VP1C_List = new SelectList(VP1C, "IDNhanVien", "HoTen");

            // Lấy thông tin KTV, TP, VP1C từ CDNT_TrinhKy
            var trinhKy = db_dk.CDNT_TrinhKy.AsNoTracking().Where(x => x.Ma_Don == id).ToList();
            ViewBag.KTV_ID = trinhKy.FirstOrDefault(x => x.CapDuyet == 1 && x.GhiChu.Contains("Kỹ thuật viên"))?.NguoiDuyet_ID;
            ViewBag.TP_ID = trinhKy.FirstOrDefault(x => x.CapDuyet == 1 && x.GhiChu.Contains("Trưởng/Phó phòng"))?.NguoiDuyet_ID;
            ViewBag.VP1C_ID = trinhKy.FirstOrDefault(x => x.CapDuyet == 2)?.NguoiDuyet_ID;

            // Lấy danh sách KTV và TP theo phòng ban (BPQL_ID)
            var ktvList = (from au in db.AuthorizationContractors
                           where au.IDLKD == 2 && (au.NhanVien.IDPhongBan == donDangKy.BPQL_ID || au.IDCVKN.Contains(donDangKy.BPQL_ID.ToString()))
                           join a in db.NhanViens on au.IDNhanVien equals a.ID
                           select new { ID = au.IDNhanVien, HoTen = a.HoTen + " : " + a.MaNV }).ToList();
            var tpList = (from au in db.AuthorizationContractors
                          where au.IDLKD == 1 && (au.NhanVien.IDPhongBan == donDangKy.BPQL_ID || au.IDCVKN.Contains(donDangKy.BPQL_ID.ToString()))
                          join a in db.NhanViens on au.IDNhanVien equals a.ID
                          select new { ID = au.IDNhanVien, HoTen = a.HoTen + " : " + a.MaNV }).ToList();

            ViewBag.KTV_QL_List = new SelectList(ktvList, "ID", "HoTen", ViewBag.KTV_ID);
            ViewBag.TP_QL_List = new SelectList(tpList, "ID", "HoTen", ViewBag.TP_ID);
            ViewBag.TenNhaThau = tenNhaThau;

            // Lấy danh sách xe từ CDNT_ChiTietDon
            var danhSachXe = db_dk.CDNT_ChiTietDon
                .AsNoTracking()
                .Where(x => x.Ma_Don == id)
                .ToList()
                .Select(ChiTietDonVM.FromModel)
                .ToList();
            if (danhSachXe == null || danhSachXe.Count == 0)
            {
                danhSachXe = new List<ChiTietDonVM>(); // tránh null khi về View
            }
            // Tạo model cho view
            var model = new DonDangKyModel
            {
                ID = donDangKy.ID,
                Ma_Don = donDangKy.Ma_Don,
                NoiDung = donDangKy.NoiDung,
                BPQL_ID = donDangKy.BPQL_ID,
                NhanVienNT_ID = donDangKy.NhanVienNT_ID,
                NhaThau_ID = donDangKy.NhaThau_ID,
                HopDong = donDangKy.HopDong,
                NgayTrinhKy = donDangKy.NgayTrinhKy,
                FileHoSoXe = donDangKy.FileHoSoXe,
                TrinhKy_ID = donDangKy.TrinhKy_ID,
                TinhTrang_ID = donDangKy.TinhTrang_ID,
                LoaiNT_ID = donDangKy.LoaiNT_ID,
                DanhSachXe = danhSachXe
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult Update(DonDangKyModel model, HttpPostedFileBase FileHoSoXE, int? KTV_ID, int? TP_ID, int? VP1C_ID)
        {
            try
            {
                // Kiểm tra mã đơn
                if (string.IsNullOrEmpty(model.Ma_Don))
                {
                    return Json(new { success = false, message = "Mã đơn không hợp lệ" });
                }

                // Kiểm tra danh sách xe
                if (model.DanhSachXe == null || !model.DanhSachXe.Any())
                {
                    return Json(new { success = false, message = "Danh sách xe không được trống" });
                }

                // Kiểm tra phòng ban
                if (!model.BPQL_ID.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng chọn phòng ban quản lý" });
                }

                // Kiểm tra nhà thầu
                if (!model.NhaThau_ID.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng chọn nhà thầu" });
                }

                // Tìm đơn đăng ký
                var donDangKy = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == model.Ma_Don);
                if (donDangKy == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn đăng ký" });
                }

                // Kiểm tra quyền chỉnh sửa
                if (donDangKy.NhanVienNT_ID != Models.MyAuthentication.ID)
                {
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa đơn này" });
                }

                // Kiểm tra trạng thái đơn
                if (donDangKy.TinhTrang_ID != (int)TinhTrangDonDangKy.ChoXuLy)
                {
                    return Json(new { success = false, message = "Đơn đã được duyệt, không thể chỉnh sửa" });
                }

                // Xử lý file upload
                if (FileHoSoXE != null && FileHoSoXE.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(FileHoSoXE.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
                    FileHoSoXE.SaveAs(filePath);
                    donDangKy.FileHoSoXe = fileName;
                }

                // Cập nhật thông tin đơn
                donDangKy.NoiDung = model.NoiDung;
                donDangKy.NhaThau_ID = model.NhaThau_ID;
                donDangKy.BPQL_ID = model.BPQL_ID;

                // Xóa danh sách xe cũ
                var oldChiTietDon = db_dk.CDNT_ChiTietDon.Where(x => x.Ma_Don == model.Ma_Don).ToList();
                foreach (var item in oldChiTietDon)
                {
                    db_dk.CDNT_ChiTietDon.Remove(item);
                }

                // Thêm danh sách xe mới
                foreach (var xe in model.DanhSachXe)
                {
                    // Validate dữ liệu xe
                    if (xe.ID_LoaiPhuongTien == null || string.IsNullOrEmpty(xe.BienSoXe))
                    {
                        return Json(new { success = false, message = "Loại phương tiện hoặc biển số xe không được trống" });
                    }

                    db_dk.CDNT_ChiTietDon.Add(new CDNT_ChiTietDon
                    {
                        Ma_Don = model.Ma_Don,
                        ID_LoaiPhuongTien = xe.ID_LoaiPhuongTien,
                        BienSoXe = xe.BienSoXe,
                        CapMoi = xe.CapMoi,
                        CapLai = xe.CapLai,
                        GiaHan = xe.GiaHan,
                        TuNgay = xe.TuNgay,
                        DenNgay = xe.DenNgay,
                        GhiChu = xe.GhiChu
                    });
                }

                // Xóa luồng trình ký cũ
                var trinhKyList = db_dk.CDNT_TrinhKy.Where(x => x.Ma_Don == model.Ma_Don).ToList();
                foreach (var trinhKy in trinhKyList)
                {
                    db_dk.CDNT_TrinhKy.Remove(trinhKy);
                }

                // Thêm lại luồng trình ký
                db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                {
                    Ma_Don = model.Ma_Don,
                    CapDuyet = 0,
                    NguoiDuyet_ID = model.NhanVienNT_ID ?? Models.MyAuthentication.ID,
                    NgayDuyet = DateTime.Now,
                    TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                    GhiChu = "Trình ký từ nhà thầu"
                });

                if (KTV_ID.HasValue)
                {
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = model.Ma_Don,
                        CapDuyet = 1,
                        NguoiDuyet_ID = KTV_ID.Value,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                        GhiChu = "Chờ duyệt - Kỹ thuật viên"
                    });
                }

                if (TP_ID.HasValue)
                {
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = model.Ma_Don,
                        CapDuyet = 1,
                        NguoiDuyet_ID = TP_ID.Value,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                        GhiChu = "Chờ duyệt cấp - Trưởng/Phó phòng"
                    });
                }

                if (VP1C_ID.HasValue)
                {
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = model.Ma_Don,
                        CapDuyet = 2,
                        NguoiDuyet_ID = VP1C_ID.Value,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy,
                        GhiChu = "Chờ cấp phát thẻ từ CPT"
                    });
                }

                // Lưu thay đổi
                db_dk.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật đơn thành công",
                    maDon = model.Ma_Don
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
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
        [HttpGet]
        public ActionResult LuongTrinhKy(string Ma_Don = null)
        {
            var data = db_dk.CDNT_TrinhKy_Detail(Ma_Don).ToList();
            return View(data);
        }
        [HttpGet]
        public ActionResult HPDQ_LuongTrinhKy(string Ma_Don = null)
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

            var dsXe = db_dk.CDNT_ChiTietDon.Where(x => x.Ma_Don == maDon).ToList();
            if (isApproved)
            {
                don.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;
                phanCong.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;

                var bienSoBiTuChoiRaw = Request.Form["bienSoBiTuChoi"];
                var bienSoBiTuChoi = string.IsNullOrWhiteSpace(bienSoBiTuChoiRaw)
                    ? new List<string>()  // Không có giá trị => không từ chối xe nào
                    : bienSoBiTuChoiRaw.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();


                foreach (var xe in dsXe)
                {
                    xe.TrangThaiDuyet_ID = bienSoBiTuChoi.Contains(xe.BienSoXe) ? 2 : 1;
                }
            }
            else
            {
                don.TinhTrang_ID = (int)TinhTrangDonDangKy.KhongDatYeuCau;
                phanCong.TinhTrang_ID = (int)TinhTrangDonDangKy.KhongDatYeuCau;


                foreach (var xe in dsXe)
                {
                    xe.TrangThaiDuyet_ID = 2; // Từ chối tất cả
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

        public ActionResult DanhSachXe(DateTime? begind, DateTime? endd, string search, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var data = db_dk.Database.SqlQuery<XeCoDongModel>(
            @"CDNT_XeCoDong_Search 
            @p_ID_NT, 
            @p_BP_ID, 
            @p_TenNhaThau, 
            @p_TenDayDu, 
            @p_LoaiPhuongTien_ID, 
            @p_BienSoXe, 
            @p_TinhTrang, 
            @p_TuNgay, 
            @p_DenNgay",

                new SqlParameter("@p_ID_NT", DBNull.Value),
                new SqlParameter("@p_BP_ID", DBNull.Value),
                new SqlParameter("@p_TenNhaThau", DBNull.Value),
                new SqlParameter("@p_TenDayDu", DBNull.Value),
                new SqlParameter("@p_LoaiPhuongTien_ID", DBNull.Value),
                new SqlParameter("@p_BienSoXe", (object)search ?? DBNull.Value),
                new SqlParameter("@p_TinhTrang", DBNull.Value),
                new SqlParameter("@p_TuNgay", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_DenNgay", (object)endd ?? DBNull.Value)
            ).ToList();

            var pagedData = data.ToPagedList(pageNumber, pageSize);

            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.Search = search;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(pagedData);
        }
        //[HttpPost]
        //public ActionResult ImportExcelXe(HttpPostedFileBase file)
        //{
        //    if (file != null && file.ContentLength > 0)
        //    {
        //        try
        //        {
        //            using (var workbook = new XLWorkbook(file.InputStream))
        //            {
        //                var ws = workbook.Worksheet(1); // sheet đầu tiên
        //                var rows = ws.RangeUsed().RowsUsed().Skip(1); // bỏ header, bắt đầu từ dòng 2

        //                foreach (var row in rows)
        //                {
        //                    // Đọc dữ liệu an toàn
        //                    string fullName = row.Cell(1).Value == null ? "" : row.Cell(1).Value.ToString().Trim();
        //                    string bienso = row.Cell(2).Value == null ? "" : row.Cell(2).Value.ToString().Trim();
        //                    string loaipt = row.Cell(3).Value == null ? "" : row.Cell(3).Value.ToString().Trim();
        //                    string ngaycapStr = row.Cell(7).Value == null ? "" : row.Cell(7).Value.ToString().Trim();
        //                    string thoihanStr = row.Cell(8).Value == null ? "" : row.Cell(8).Value.ToString().Trim();

        //                    // Extract mant and tennt from fullName (e.g., "14211 - COMPANY NAME")
        //                    int mant = 0;
        //                    string tennt = "";
        //                    if (!string.IsNullOrEmpty(fullName))
        //                    {
        //                        var parts = fullName.Split(new[] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries);
        //                        if (parts.Length >= 1)
        //                        {
        //                            int.TryParse(parts[0].Trim(), out mant);
        //                        }
        //                        if (parts.Length >= 2)
        //                        {
        //                            tennt = parts[1].Trim();
        //                        }
        //                    }

        //                    // Map loại phương tiện
        //                    int loaiPhuongTienID = 0;
        //                    if (!string.IsNullOrEmpty(loaipt))
        //                    {
        //                        string lp = loaipt.ToUpper();
        //                        if (lp.Contains("XE MÁY"))
        //                            loaiPhuongTienID = 1;
        //                        else if (lp.Contains("XE BA GÁC") || lp.Contains("XE BA BÁNH"))
        //                            loaiPhuongTienID = 2;
        //                    }

        //                    // Ngày cấp và thời hạn
        //                    DateTime? ngaycap = null;
        //                    DateTime? thoihan = null;

        //                    // Nếu cell là kiểu ngày
        //                    if (row.Cell(7).DataType == XLDataType.DateTime)
        //                    {
        //                        ngaycap = row.Cell(7).GetDateTime();
        //                    }
        //                    else if (!string.IsNullOrEmpty(ngaycapStr) && DateTime.TryParseExact(ngaycapStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime nc))
        //                    {
        //                        ngaycap = nc;
        //                    }

        //                    if (row.Cell(8).DataType == XLDataType.DateTime)
        //                    {
        //                        thoihan = row.Cell(8).GetDateTime();
        //                    }
        //                    else if (!string.IsNullOrEmpty(thoihanStr) && DateTime.TryParseExact(thoihanStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime th))
        //                    {
        //                        thoihan = th;
        //                    }

        //                    // Tạo entity để insert
        //                    CDNT_XeCoDong xe = new CDNT_XeCoDong
        //                    {
        //                        IDNT = mant,
        //                        TenNT = tennt,
        //                        BienSoXe = bienso,
        //                        LoaiPhuongTien_ID = loaiPhuongTienID,
        //                        HanBatDau = ngaycap,
        //                        HanKetThuc = thoihan,
        //                        TinhTrang = true // mặc định đang hoạt động
        //                    };

        //                    db_dk.CDNT_XeCoDong.Add(xe);
        //                }

        //                db_dk.SaveChanges();
        //            }

        //            TempData["Success"] = "Import dữ liệu thành công!";
        //        }
        //        catch (Exception ex)
        //        {
        //            TempData["Error"] = "Lỗi khi import: " + ex.Message;
        //        }
        //    }
        //    else
        //    {
        //        TempData["Error"] = "Chưa chọn file!";
        //    }

        //    return RedirectToAction("Index");
        //}
        public ActionResult ExportDonDangKyPdf(string maDon)
        {
            // 1. Lấy dữ liệu chi tiết đơn từ DB  
            var data = db_dk.Database.SqlQuery<CDNT_DonDangKyDetail>(
                "EXEC CDNT_DonDangKy_Detail @Ma_Don",
                new SqlParameter("@Ma_Don", maDon ?? (object)DBNull.Value)
            ).ToList();

            if (data == null || !data.Any())
                return HttpNotFound("Không có dữ liệu.");

            // 2. Gọi Render View → Tạo PDF bằng Rotativa (không thêm watermark)
            string tempPdfPath = GenerateTempPdf(maDon);

            // 3. Đọc nội dung file PDF đã render
            byte[] pdfBytes = System.IO.File.ReadAllBytes(tempPdfPath);

            // 4. Xóa file tạm
            System.IO.File.Delete(tempPdfPath);

            // 5. Trả file PDF về người dùng
            string safeFileName = SanitizeFileName(data.First().FullName) + ".pdf";
            return File(pdfBytes, "application/pdf", safeFileName);
        }

        private string GenerateTempPdf(string maDon)
        {
            string root = Server.MapPath("~/UploadedFiles/PDFDangKyThe/");
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            string tempPdf = Path.Combine(root, $"{Guid.NewGuid()}.pdf");
            //string footerHtml = Server.MapPath("~/Views/Shared/footer.html");

            var actionPdf = new Rotativa.ActionAsPdf("Detail_PDF", new { maDon = maDon })
            {
                PageSize = Rotativa.Options.Size.A4,
                PageMargins = new Rotativa.Options.Margins(13, 5, 10, 5),
               // CustomSwitches = $"--footer-html \"{footerHtml}\" --footer-spacing 5 --footer-font-size 9 --footer-line --encoding utf-8"
            };
            byte[] pdfBytes = actionPdf.BuildPdf(this.ControllerContext);
            System.IO.File.WriteAllBytes(tempPdf, pdfBytes);

            return tempPdf;
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