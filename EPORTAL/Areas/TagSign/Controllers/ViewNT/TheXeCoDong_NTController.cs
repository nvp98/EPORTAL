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
using DocumentFormat.OpenXml.EMMA;
using System.Web.UI.WebControls;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class TheXeCoDong_NTController : Controller
    {
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "TheXeCoDong_NT";
        // GET: TagSign/ContractorGroup

        public ActionResult Index_Test(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {

            // Phân trang ở Controller
            int pageNumber = page ?? 1;
            int pageSize = 10;
            var userNameLogin = Models.MyAuthentication.Username;


            // Gọi store, chỉ truyền filter ngày
            var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
                "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate,@p_MaPhieu,@p_UserNameLogin",
                new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
                new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value),
                new SqlParameter("@p_UserNameLogin", (object)userNameLogin ?? DBNull.Value)
            ).ToList();
            foreach (var don in data)
            {
                // Lấy tất cả bước trình ký cấp > 0 của đơn này
                var steps = db_dk.CDNT_TrinhKy
                    .Where(x => x.Ma_Don == don.Ma_Don && (x.CapDuyet ?? 0) > 0)
                    .ToList();

                // Nếu có bước trình ký cấp > 0 nhưng TẤT CẢ đều là Nháp (TinhTrang_ID = 5)
                if (steps.Any() && steps.All(s => s.TinhTrang_ID == 5))
                {
                    don.TinhTrang_ID = 5; // Chưa trình ký
                    don.TenTinhTrang = "Chưa trình ký";
                    continue;
                }

                // Nếu chưa có bước trình ký cấp > 0 (tức là chưa tạo dòng trình ký cho cấp 1,2,3)
                if (!steps.Any())
                {
                    don.TinhTrang_ID = 5; // Chưa trình ký
                    don.TenTinhTrang = "Chưa trình ký";
                    continue;
                }

                // Nếu có bất kỳ bước nào bị từ chối
                if (steps.Any(s => s.TinhTrang_ID == 4))
                {
                    don.TinhTrang_ID = 4; // Không đạt yêu cầu
                    don.TenTinhTrang = "Không đạt yêu cầu";
                    continue;
                }

                // Nếu tất cả bước đều đã xử lý
                if (steps.All(s => s.TinhTrang_ID == 2))
                {
                    don.TinhTrang_ID = 2; // Đã xử lý
                    don.TenTinhTrang = "Đã xử lý";
                }
                else
                {
                    don.TinhTrang_ID = 1; // Chờ xử lý
                    don.TenTinhTrang = "Chờ xử lý";
                }
            }
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

        //public ActionResult HPDQ_Index(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        //{
        //    var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
        //    ViewBag.QUYENCN = ListQuyen;
        //    if (!ListQuyen.Contains("VIEW_ALL"))
        //    {
        //        TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
        //        return RedirectToAction("Logout", "Login", new { area = "" });
        //    }
        //    int pageNumber = page ?? 1;
        //    int pageSize = 10;
        //    int userId = Models.MyAuthentication.ID;

        //    // 1. Lấy đơn (trừ Nháp/Chưa trình ký)
        //    var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
        //        "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate, @p_MaPhieu",
        //        new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
        //        new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
        //        new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
        //    )
        //    .Where(d => d.TinhTrang_ID != 5) // Chưa trình ký/Nháp
        //    .ToList();
        //    foreach (var don in data)
        //    {
        //        // Lấy các bước trình ký cấp > 0
        //        var steps = db_dk.CDNT_TrinhKy
        //            .Where(x => x.Ma_Don == don.Ma_Don && (x.CapDuyet ?? 0) > 0)
        //            .ToList();

        //        // Nếu chưa có bước trình ký cấp > 0 hoặc tất cả đều Nháp
        //        if (!steps.Any() || steps.All(s => s.TinhTrang_ID == 5))
        //        {
        //            don.TinhTrang_ID = 5; // Chưa trình ký
        //            don.TenTinhTrang = "Chưa trình ký";
        //        }
        //        // Nếu có bất kỳ bước nào bị từ chối
        //        else if (steps.Any(s => s.TinhTrang_ID == 4))
        //        {
        //            don.TinhTrang_ID = 4; // Không đạt yêu cầu
        //            don.TenTinhTrang = "Không đạt yêu cầu";
        //        }
        //        // Nếu tất cả bước đều đã xử lý
        //        else if (steps.All(s => s.TinhTrang_ID == 2))
        //        {
        //            don.TinhTrang_ID = 2; // Đã xử lý
        //            don.TenTinhTrang = "Đã xử lý";
        //        }
        //        else
        //        {
        //            don.TinhTrang_ID = 1; // Chờ xử lý
        //            don.TenTinhTrang = "Chờ xử lý";
        //        }
        //    }
        //    if (!data.Any())
        //    {
        //        SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
        //        return View(new List<DonDangKyViewModel>().ToPagedList(pageNumber, pageSize));
        //    }

        //    // 2. Phân công user (chỉ lấy bước trình ký cấp 0-3 mà user là người duyệt)
        //    var userAssignRaw = db_dk.CDNT_TrinhKy
        //        .Where(x => x.NguoiDuyet_ID == userId && x.CapDuyet >= 0 && x.CapDuyet <= 3)
        //        .Select(x => new { x.Ma_Don, x.CapDuyet })
        //        .ToList();

        //    var userAssign = userAssignRaw
        //        .GroupBy(a => a.Ma_Don)
        //        .ToDictionary(g => g.Key, g => g.Select(z => z.CapDuyet ?? 0).OrderBy(c => c).ToList());

        //    // 3. Các bước duyệt của từng đơn (chỉ cấp 0-3)
        //    var maDonList = data.Select(d => d.Ma_Don).Distinct().ToList();
        //    var stepsRaw = db_dk.CDNT_TrinhKy
        //        .Where(x => maDonList.Contains(x.Ma_Don) && x.CapDuyet >= 0 && x.CapDuyet <= 3)
        //        .Select(x => new StepInfo
        //        {
        //            Ma_Don = x.Ma_Don,
        //            CapDuyet = x.CapDuyet,
        //            TinhTrang_ID = x.TinhTrang_ID
        //        })
        //        .ToList();

        //    var stepsByDon = stepsRaw
        //        .GroupBy(s => s.Ma_Don)
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    var result = new List<DonDangKyViewModel>();

        //    foreach (var don in data)
        //    {
        //        // Không phải người duyệt bước nào
        //        if (!userAssign.TryGetValue(don.Ma_Don, out var userCaps) || userCaps.Count == 0)
        //            continue;

        //        int userCap = userCaps.First();
        //        if (!stepsByDon.TryGetValue(don.Ma_Don, out var stepList))
        //            stepList = new List<StepInfo>();

        //        // Kiểm tra các cấp trước đã duyệt hết chưa (chỉ áp dụng cho cấp > 0)
        //        bool lowerAllApproved = true;
        //        if (userCap > 0)
        //        {
        //            lowerAllApproved = stepList
        //                .Where(s => (s.CapDuyet ?? 0) < userCap)
        //                .All(s => s.TinhTrang_ID == 2 || s.TinhTrang_ID == 3); // 2 = Đã xử lý, 3 = Hoàn thành
        //        }

        //        if (!lowerAllApproved) continue;

        //        var myStep = stepList.FirstOrDefault(s => s.CapDuyet == userCap);
        //        int highestCap = stepList.Select(s => s.CapDuyet ?? 0).DefaultIfEmpty(0).Max();
        //        bool hasReject = stepList.Any(s => s.TinhTrang_ID == 4);
        //        bool allApproved = stepList.Any() && stepList.All(s => s.TinhTrang_ID == 2 || s.TinhTrang_ID == 3);

        //        // Gán TenTinhTrang theo TinhTrang_ID của đơn
        //        don.TenTinhTrang = TranslateTinhTrang(don.TinhTrang_ID);
        //        string[] capNames = { "Nhà thầu", "NV", "T/P", "CPT" };
        //        // Góc nhìn user (tùy vào cấp và trạng thái step)
        //        if (hasReject)
        //        {
        //            don.PerUserStatusCode = 4;
        //            don.PerUserStatusText = "Không đạt yêu cầu";
        //        }
        //        else if (don.TinhTrang_ID == 3)
        //        {
        //            don.PerUserStatusCode = 6;
        //            don.PerUserStatusText = "Hoàn thành";
        //        }
        //        else if (myStep == null)
        //        {
        //            don.PerUserStatusCode = 0;
        //            don.PerUserStatusText = "Chưa vào lượt";
        //        }
        //        else if (myStep.TinhTrang_ID == 1)
        //        {
        //            don.PerUserStatusCode = 1;
        //            don.PerUserStatusText = $"Chờ xử lý ({capNames[Math.Max(0, Math.Min(userCap, capNames.Length - 1))]})";
        //        }
        //        else if (myStep.TinhTrang_ID == 2)
        //        {
        //            don.PerUserStatusCode = 2;
        //            don.PerUserStatusText = $"Đã xử lý ({capNames[Math.Max(0, Math.Min(userCap, capNames.Length - 1))]})";
        //        }
        //        else if (myStep.TinhTrang_ID == 3)
        //        {
        //            don.PerUserStatusCode = 3;
        //            don.PerUserStatusText = $"Hoàn thành ({capNames[Math.Max(0, Math.Min(userCap, capNames.Length - 1))]})";
        //        }
        //        else
        //        {
        //            don.PerUserStatusCode = 0;
        //            don.PerUserStatusText = "Chờ xử lý";
        //        }

        //        if (don.TinhTrang_ID == 5 || don.TinhTrang_ID == 1)
        //        {
        //            result.Add(don);
        //        }
        //    }

        //    var ordered = result
        //        .OrderBy(r => r.PerUserStatusCode == 1 ? 0 : 1)
        //        .ThenByDescending(r => r.NgayTrinhKy ?? DateTime.MinValue)
        //        .ToPagedList(pageNumber, pageSize);

        //    SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
        //    return View(ordered);
        //}

        public ActionResult HPDQ_Index(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;
            int userId = Models.MyAuthentication.ID;

            // 1) Lấy đơn theo bộ lọc (trừ Nháp/Chưa trình ký từ kết quả ban đầu)
            var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
                "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate, @p_MaPhieu",
                new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
                new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
            )
            .Where(d => d.TinhTrang_ID != 5)
            .ToList();

            if (!data.Any())
            {
                SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
                return View(new List<DonDangKyViewModel>().ToPagedList(pageNumber, pageSize));
            }

            // Lấy danh sách mã đơn 1 lần
            var maDonList = data.Select(d => d.Ma_Don).Distinct().ToList();

            // 2) Lấy toàn bộ steps (cap 0-3) 1 lần, group theo Ma_Don để dùng nhiều nơi
            var stepsRaw = db_dk.CDNT_TrinhKy
                .Where(x => maDonList.Contains(x.Ma_Don) && x.CapDuyet >= 0 && x.CapDuyet <= 3)
                .Select(x => new StepInfo
                {
                    Ma_Don = x.Ma_Don,
                    CapDuyet = x.CapDuyet,
                    TinhTrang_ID = x.TinhTrang_ID
                })
                .ToList();

            var stepsByDon = stepsRaw
                .GroupBy(s => s.Ma_Don)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 3) Đồng bộ trạng thái tổng thể của đơn dựa trên stepsByDon (không query trong vòng lặp nữa)
            foreach (var don in data)
            {
                var stepList = stepsByDon.TryGetValue(don.Ma_Don, out var lst) ? lst : new List<StepInfo>();
                var stepGt0 = stepList.Where(s => (s.CapDuyet ?? 0) > 0).ToList();

                if (!stepGt0.Any() || stepGt0.All(s => s.TinhTrang_ID == 5))
                {
                    don.TinhTrang_ID = 5;
                    don.TenTinhTrang = "Chưa trình ký";
                }
                else if (stepGt0.Any(s => s.TinhTrang_ID == 4))
                {
                    don.TinhTrang_ID = 4;
                    don.TenTinhTrang = "Không đạt yêu cầu";
                }
                else if (stepGt0.All(s => s.TinhTrang_ID == 2))
                {
                    don.TinhTrang_ID = 2;
                    don.TenTinhTrang = "Đã xử lý";
                }
                else
                {
                    don.TinhTrang_ID = 1;
                    don.TenTinhTrang = "Chờ xử lý";
                }
            }

            // 4) Phân công user (chỉ lấy bước trình ký cấp 0-3 mà user là người duyệt)
            var userAssignRaw = db_dk.CDNT_TrinhKy
                .Where(x => x.NguoiDuyet_ID == userId && x.CapDuyet >= 0 && x.CapDuyet <= 3)
                .Select(x => new { x.Ma_Don, x.CapDuyet })
                .ToList();

            var userAssign = userAssignRaw
                .GroupBy(a => a.Ma_Don)
                .ToDictionary(g => g.Key, g => g.Select(z => z.CapDuyet ?? 0).OrderBy(c => c).ToList());

            var capNames = new[] { "Nhà thầu", "NV", "T/P", "CPT" };

            // 5) Lọc theo góc nhìn “tới lượt mình”
            var result = new List<DonDangKyViewModel>();
            foreach (var don in data)
            {
                if (!userAssign.TryGetValue(don.Ma_Don, out var userCaps) || userCaps.Count == 0)
                    continue;

                int userCap = userCaps.First();

                var stepList = stepsByDon.TryGetValue(don.Ma_Don, out var lst) ? lst : new List<StepInfo>();

                bool lowerAllApproved = true;
                if (userCap > 0)
                {
                    lowerAllApproved = stepList
                        .Where(s => (s.CapDuyet ?? 0) < userCap)
                        .All(s => s.TinhTrang_ID == 2 || s.TinhTrang_ID == 3);
                }
                if (!lowerAllApproved) continue;

                var myStep = stepList.FirstOrDefault(s => s.CapDuyet == userCap);
                bool hasReject = stepList.Any(s => s.TinhTrang_ID == 4);

                don.TenTinhTrang = TranslateTinhTrang(don.TinhTrang_ID);

                if (hasReject)
                {
                    don.PerUserStatusCode = 4;
                    don.PerUserStatusText = "Không đạt yêu cầu";
                }
                else if (don.TinhTrang_ID == 3)
                {
                    don.PerUserStatusCode = 6;
                    don.PerUserStatusText = "Hoàn thành";
                }
                else if (myStep == null)
                {
                    don.PerUserStatusCode = 0;
                    don.PerUserStatusText = "Chưa vào lượt";
                }
                else if (myStep.TinhTrang_ID == 1)
                {
                    don.PerUserStatusCode = 1;
                    don.PerUserStatusText = $"Chờ xử lý ({capNames[Math.Max(0, Math.Min(userCap, capNames.Length - 1))]})";
                }
                else if (myStep.TinhTrang_ID == 2)
                {
                    don.PerUserStatusCode = 2;
                    don.PerUserStatusText = $"Đã xử lý ({capNames[Math.Max(0, Math.Min(userCap, capNames.Length - 1))]})";
                }
                else if (myStep.TinhTrang_ID == 3)
                {
                    don.PerUserStatusCode = 3;
                    don.PerUserStatusText = $"Hoàn thành ({capNames[Math.Max(0, Math.Min(userCap, capNames.Length - 1))]})";
                }
                else
                {
                    don.PerUserStatusCode = 0;
                    don.PerUserStatusText = "Chờ xử lý";
                }

                if (don.TinhTrang_ID == 5 || don.TinhTrang_ID == 1)
                {
                    result.Add(don);
                }
            }

            var ordered = result
                .OrderBy(r => r.PerUserStatusCode == 1 ? 0 : 1)
                .ThenByDescending(r => r.NgayTrinhKy ?? DateTime.MinValue)
                .ToPagedList(pageNumber, pageSize);

            SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
            return View(ordered);
        }

        // Mapping trạng thái mới
        private string TranslateTinhTrang(int? stt)
        {
            if (!stt.HasValue) return "Không xác định";
            switch (stt.Value)
            {
                case 1: return "Chờ xử lý";
                case 2: return "Đã xử lý";
                case 3: return "Hoàn thành";
                case 4: return "Không đạt yêu cầu";
                case 5: return "Chưa trình ký";
                default: return "Không xác định";
            }
        }

        private void SetFilters(DateTime? begind, DateTime? endd, string maPhieu, int page, int size)
        {
            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Page = page;
            ViewBag.PageSize = size;
        }

        public ActionResult HPDQ_Detail(string maDon)
        {
            if (string.IsNullOrWhiteSpace(maDon))
            {
                TempData["msgError"] = "<div class='alert alert-danger'>Thiếu mã đơn.</div>";
                return RedirectToAction("HPDQ_Index");
            }

            var userId = Models.MyAuthentication.ID;

            // Lấy chi tiết (SP bạn đã có)
            var data = db_dk.Database.SqlQuery<CDNT_DonDangKyDetail>(
                   "EXEC CDNT_DonDangKy_Detail @Ma_Don",
                   new SqlParameter("@Ma_Don", (object)maDon ?? DBNull.Value)
               )
               .Where(x => x.TrangThaiDuyet_ID != 2)
               .ToList();

            if (!data.Any())
            {
                TempData["msgError"] = "<div class='alert alert-warning'>Không tìm thấy dữ liệu đơn.</div>";
                return RedirectToAction("HPDQ_Index");
            }

            if (!data.Any())
            {
                TempData["msgError"] = "<div class='alert alert-warning'>Không tìm thấy dữ liệu đơn.</div>";
                return RedirectToAction("HPDQ_Index");
            }

            // Tìm step của user
            var myStep = db_dk.CDNT_TrinhKy
                .FirstOrDefault(x => x.Ma_Don == maDon
                                  && x.NguoiDuyet_ID == userId
                                  && x.CapDuyet > 0);

            bool canAct = false;
            int? myStepStatus = null;
            int? myStepId = null;
            int? myCap = null;

            if (myStep != null)
            {
                myStepStatus = myStep.TinhTrang_ID;
                myStepId = myStep.ID;
                myCap = myStep.CapDuyet;

                if (myStep.TinhTrang_ID == (int)TinhTrangDonDangKy.ChoXuLy)
                {
                    // Các cấp trước phải đã duyệt
                    bool lowerAllApproved = db_dk.CDNT_TrinhKy
                        .Where(s => s.Ma_Don == maDon && s.CapDuyet > 0 && s.CapDuyet < myStep.CapDuyet)
                        .All(s => s.TinhTrang_ID == (int)TinhTrangDonDangKy.DaXuLy);

                    canAct = lowerAllApproved;
                }
            }

            ViewBag.CanAct = canAct;
            ViewBag.MyStepStatus = myStepStatus; // 1 / 2 / 4 / null
            ViewBag.MyStepId = myStepId;
            ViewBag.MyStepCap = myCap;
            ViewBag.MaDon = maDon;

            return View(data); // Model: List<CDNT_DonDangKyDetail>
        }

        public ActionResult Create(List<ChiTietDonVM> danhSach = null)
        {
            string tenNhaThau = "";
            int? nhaThauID = null;

            int nhanVienNT_ID = Models.MyAuthentication.ID; 
            var nhanVien = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == nhanVienNT_ID); 

            if (nhanVien != null && nhanVien.IDNT.HasValue)
            {
                nhaThauID = nhanVien.IDNT.Value;

                var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == nhaThauID.Value);
                if (nhaThau != null)
                {
                    tenNhaThau = nhaThau.FullName;
                }
            }

            // Truyền ID và Tên nhà thầu ra View (để hiển thị + submit)
            ViewBag.NhaThau_ID = nhaThauID;
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
                // 2. Kiểm tra nội dung trình ký
                if (string.IsNullOrWhiteSpace(model.NoiDung))
                {
                    return Json(new { success = false, message = "Nội dung trình ký không được để trống!" });
                }
                // 3. Kiểm tra file hồ sơ xe
                if (FileHoSoXe == null || FileHoSoXe.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Bạn phải chọn file hồ sơ xe!" });
                }

                var danhSachXe = JsonConvert.DeserializeObject<List<ChiTietDonVM>>(model.JsonDanhSachXe);
                var bienSoList = danhSachXe.Select(x => x.BienSoXe?.Trim().ToUpper()).ToList();
                if (bienSoList.Count != bienSoList.Distinct().Count())
                {
                    return Json(new { success = false, message = "Không được phép có hai biển số xe trùng nhau!" });
                }

                // 2. Xử lý file upload (nếu có)
                if (FileHoSoXe != null && FileHoSoXe.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(FileHoSoXe.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/UploadedFiles/XeCoDong/"), fileName);
                    FileHoSoXe.SaveAs(filePath);

                    // Lưu tên file (đường dẫn tương đối)
                    model.FileHoSoXe = fileName;
                }

                // 3. Tạo mã đơn tự động nếu chưa có
                if (string.IsNullOrEmpty(model.Ma_Don))
                {
                    var Business_Partner = Models.MyAuthentication.Username;
                    var thangNam = DateTime.Now.ToString("yyyyMMdd");

                    var prefix = $"{Business_Partner}_XCD{thangNam}-";

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

                    model.Ma_Don = prefix + stt.ToString("D6");
                }

                // 4. Gán người tạo đơn là người đang đăng nhập
                //model.NhanVienNT_ID = Models.MyAuthentication.ID;
                //var ntUser = db.NT_UserTemp.FirstOrDefault(x => x.ID == NhanVienNT_ID);

                //var Id_NT =  
                model.UserNameLogin = Models.MyAuthentication.Username;
                var username = Models.MyAuthentication.Username;
                //var ntUser = db.NT_UserTemp.FirstOrDefault(x => x.UserName == username);

                //model.NhanVienNT_ID = ntUser.IDNT;
                var ntUser = db_nt.NT_UserTemp.FirstOrDefault(x => x.UserName == username);
                model.NhanVienNT_ID = ntUser.IDNT;
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
                    model.TinhTrang_ID = 5,
                    model.LoaiNT_ID,
                    model.UserNameLogin,
                    model.JsonDanhSachXe
                ).FirstOrDefault();

                if (result?.Result == 1)
                {
                    string maDon = result.MaDon;
                    int nguoiTaoDon_ID = (int)model.NhanVienNT_ID;

                    // Trình ký cấp 0 - Nhà thầu
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = maDon,
                        CapDuyet = 0,
                        NguoiDuyet_ID = nguoiTaoDon_ID,
                        NgayDuyet = DateTime.Now,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
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
                            TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
                            GhiChu = "Chờ duyệt - Nhân viên"
                        });
                    }

                    // Trình ký cấp 2 - Trưởng/Phó phòng
                    if (TP_ID.HasValue)
                    {
                        db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                        {
                            Ma_Don = maDon,
                            CapDuyet = 2,
                            NguoiDuyet_ID = TP_ID.Value,
                            TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
                            GhiChu = "Chờ duyệt cấp - Trưởng/Phó Bộ phận"
                        });
                    }

                    // Trình ký cấp 3 - CPT (VP1C)
                    if (VP1C_ID.HasValue)
                    {
                        db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                        {
                            Ma_Don = maDon,
                            CapDuyet = 3,
                            NguoiDuyet_ID = VP1C_ID.Value,
                            TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
                            GhiChu = "Chờ cấp phát thẻ từ CPT"
                        });
                    }

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
                 .Any(x => x.Ma_Don == maDon && x.TinhTrang_ID != (int)TinhTrangDonDangKy.Nhap);

                if (daKy)
                {
                    return Json(new { success = false, message = "Đơn đã được ký, không thể xóa." });
                }
                var trinhKyList = db_dk.CDNT_TrinhKy.Where(x => x.Ma_Don == maDon).ToList();
                if (trinhKyList.Any())
                {
                    db_dk.CDNT_TrinhKy.RemoveRange(trinhKyList);
                    db_dk.SaveChanges(); // commit trước khi xóa đơn chính
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
        //[HttpPost]
        //public JsonResult ImportExcel(HttpPostedFileBase FileANH)
        //{
        //    var danhSach = new List<ChiTietDonVM>();

        //    try
        //    {
        //        int nhanVienNT_ID = Models.MyAuthentication.ID;

        //        using (var db_nt = new EPORTAL_NTEntities())
        //        using (var db = new EPORTALEntities())
        //        {
        //            // Lấy thông tin nhân viên theo nhanVienNT_ID
        //            var nhanVien = db_nt.NT_NhanVienNT.FirstOrDefault(x => x.IDNVNT == nhanVienNT_ID);
        //            if (nhanVien == null || !nhanVien.IDNT.HasValue)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    message = "Không tìm thấy thông tin Nhà thầu tương ứng với nhân viên đăng nhập."
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            int nhaThauID = nhanVien.IDNT.Value;

        //            // (Bạn có thể lấy tên nhà thầu nếu cần)
        //            var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == nhaThauID);
        //            string tenNhaThau = nhaThau?.FullName ?? "";

        //            // Bắt đầu đọc file Excel
        //            if (FileANH != null && FileANH.ContentLength > 0)
        //            {
        //                using (var workbook = new XLWorkbook(FileANH.InputStream))
        //                {
        //                    var ws = workbook.Worksheet(1);
        //                    int row = 7;

        //                    while (!string.IsNullOrWhiteSpace(ws.Cell(row, 5).GetString()))
        //                    {
        //                        DateTime? denNgay = null;
        //                        DateTime tempDate;
        //                        string format = "dd/MM/yyyy";

        //                        // Cột I = thời hạn thẻ (ngày bắt đầu)
        //                        string cellValue = ws.Cell(row, 9).GetString().Trim();

        //                        if (DateTime.TryParseExact(cellValue, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
        //                        {
        //                            denNgay = tempDate;
        //                        }

        //                        var vm = new ChiTietDonVM
        //                        {
        //                            ID_LoaiPhuongTien =
        //                                ws.Cell(row, 2).GetString().Trim().ToUpper() == "X" ? 1 :
        //                                ws.Cell(row, 3).GetString().Trim().ToUpper() == "X" ? 2 :
        //                                ws.Cell(row, 4).GetString().Trim().ToUpper() == "X" ? 3 : (int?)null,

        //                            BienSoXe = ws.Cell(row, 5).GetString(),
        //                            CapMoi = ws.Cell(row, 6).GetString().Trim().ToUpper() == "X",
        //                            CapLai = ws.Cell(row, 7).GetString().Trim().ToUpper() == "X",
        //                            GiaHan = ws.Cell(row, 8).GetString().Trim().ToUpper() == "X",
        //                            TuNgay = null,
        //                            DenNgay = denNgay,
        //                            GhiChu = ws.Cell(row, 10).GetString()
        //                        };

        //                        danhSach.Add(vm);
        //                        row++;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                return Json(new { success = false, message = "Vui lòng chọn file Excel hợp lệ." }, JsonRequestBehavior.AllowGet);
        //            }

        //            var dinhBien = DinhBienPhuongTienService.LayDinhBienTheoNhaThau(nhaThauID);

        //            // Đếm số xe máy và 3 gác đăng ký cấp mới trong file import
        //            int soXeMayMoi = danhSach.Count(x => x.ID_LoaiPhuongTien == 1 && x.CapMoi);
        //            int soXe3GacMoi = danhSach.Count(x => x.ID_LoaiPhuongTien == 3 && x.CapMoi);

        //            // Kiểm tra vượt định biên
        //            if (soXeMayMoi > dinhBien.XeMay_ConLai || soXe3GacMoi > dinhBien.Xe3Gac_ConLai)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    message = $"Vượt định biên: Xe máy (còn lại {dinhBien.XeMay_ConLai}, đang đăng ký {soXeMayMoi}), " +
        //                            $"Xe 3 gác (còn lại {dinhBien.Xe3Gac_ConLai}, đang đăng ký {soXe3GacMoi})"
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Nếu OK, trả dữ liệu về client
        //            return Json(new
        //            {
        //                success = true,
        //                tenNhaThau = tenNhaThau,
        //                data = danhSach.Select(x => new
        //                {
        //                    x.ID_LoaiPhuongTien,
        //                    x.BienSoXe,
        //                    x.CapMoi,
        //                    x.CapLai,
        //                    x.GiaHan,
        //                    TuNgay = x.TuNgay?.ToString("yyyy-MM-dd"),
        //                    DenNgay = x.DenNgay?.ToString("yyyy-MM-dd"),
        //                    x.GhiChu
        //                })
        //            }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        [HttpPost]
        public JsonResult ImportExcel(HttpPostedFileBase FileANH)
        {
            var danhSach = new List<ChiTietDonVM>();

            try
            {
                int loginID = Models.MyAuthentication.ID;

                var ntUser = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == loginID);
                if (ntUser == null)
                    return Json(new { success = false, message = "Không tìm thấy người dùng đăng nhập" });

                int? idntLogin = ntUser.IDNT;


                var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == idntLogin);
                string tenNhaThau = nhaThau?.FullName ?? "";

                if (FileANH == null || FileANH.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file Excel hợp lệ." }, JsonRequestBehavior.AllowGet);
                }

                using (var workbook = new XLWorkbook(FileANH.InputStream))
                {
                    var ws = workbook.Worksheet(1);
                    int row = 7;

                    while (!string.IsNullOrWhiteSpace(ws.Cell(row, 5).GetString()))
                    {
                        DateTime? denNgay = ParseExcelDate(ws.Cell(row, 9));

                        var vm = new ChiTietDonVM
                        {
                            ID_LoaiPhuongTien =
                                ws.Cell(row, 2).GetString().Trim().ToUpper() == "X" ? 1 :
                                ws.Cell(row, 3).GetString().Trim().ToUpper() == "X" ? 2 :
                                ws.Cell(row, 4).GetString().Trim().ToUpper() == "X" ? 3 : (int?)null,

                            BienSoXe = NormalizeBienSo(ws.Cell(row, 5).GetString()),
                            CapMoi = ws.Cell(row, 6).GetString().Trim().ToUpper() == "X",
                            CapLai = ws.Cell(row, 7).GetString().Trim().ToUpper() == "X",
                            GiaHan = ws.Cell(row, 8).GetString().Trim().ToUpper() == "X",
                            TuNgay = null,
                            DenNgay = denNgay,
                            GhiChu = ws.Cell(row, 10).GetString().Trim()
                        };

                        danhSach.Add(vm);
                        row++;
                    }


                    var dinhBien = DinhBienPhuongTienService.LayDinhBienTheoNhaThau(idntLogin.Value);

                    int soXeMayMoi = danhSach.Count(x => x.ID_LoaiPhuongTien == 1 && x.CapMoi);
                    int soXe3GacMoi = danhSach.Count(x => x.ID_LoaiPhuongTien == 2 && x.CapMoi);

                    if (soXeMayMoi > dinhBien.XeMay_ConLai || soXe3GacMoi > dinhBien.Xe3Gac_ConLai)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Vượt định biên: Xe máy (còn lại {dinhBien.XeMay_ConLai}, đăng ký {soXeMayMoi}), " +
                                      $"Xe 3 gác (còn lại {dinhBien.Xe3Gac_ConLai}, đăng ký {soXe3GacMoi})"
                        }, JsonRequestBehavior.AllowGet);
                    }

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
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private DateTime? ParseExcelDate(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return null;

            try
            {
                // Nếu là kiểu ngày gốc
                if (cell.DataType == XLDataType.DateTime)
                {
                    return cell.GetDateTime();
                }

                // Nếu là số serial của Excel
                if (cell.DataType == XLDataType.Number)
                {
                    return DateTime.FromOADate(cell.GetDouble());
                }

                // Nếu là chuỗi - thử parse nhiều định dạng
                string raw = cell.GetString().Trim();
                string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };

                if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }

                // Nếu vẫn chưa được, thử parse tự do
                if (DateTime.TryParse(raw, out parsedDate))
                {
                    return parsedDate;
                }
            }
            catch
            {
                // Log nếu cần
            }

            return null;
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
            // Lấy ID đăng nhập (UserTemp.ID)
            var loginID = Models.MyAuthentication.ID;

            // Tìm IDNT
            var ntUser = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == loginID);
            if (ntUser == null)
            {
                TempData["msgError"] = "Không xác định được người dùng hiện tại.";
                return RedirectToAction("Index_Test");
            }

            var idntLogin = ntUser.IDNT;

            // KIỂM TRA QUYỀN
            if (donDangKy.NhanVienNT_ID != idntLogin)
            {
                TempData["msgError"] = "Bạn không có quyền chỉnh sửa đơn này";
                return RedirectToAction("Index_Test");
            }

            // Kiểm tra trạng thái đơn
            if (donDangKy.TinhTrang_ID != (int)TinhTrangDonDangKy.Nhap)
            {
                TempData["msgError"] = "Đơn đã được duyệt, không thể chỉnh sửa";
                return RedirectToAction("Index_Test");
            }

            // Lấy tên nhà thầu từ đơn
            int? nhaThauID = donDangKy.NhaThau_ID;
            string tenNhaThau = "";
            if (nhaThauID.HasValue)
            {
                var nhaThau = db.NT_Partner.FirstOrDefault(x => x.ID == nhaThauID.Value);
                tenNhaThau = nhaThau?.FullName ?? "";
            }
            // Truyền ra View để hiển thị+submit
            ViewBag.NhaThau_ID = nhaThauID;
            ViewBag.TenNhaThau = tenNhaThau;

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
            ViewBag.KTV_ID = trinhKy.FirstOrDefault(x => x.CapDuyet == 1)?.NguoiDuyet_ID;
            ViewBag.TP_ID = trinhKy.FirstOrDefault(x => x.CapDuyet == 2)?.NguoiDuyet_ID;
            ViewBag.VP1C_ID = trinhKy.FirstOrDefault(x => x.CapDuyet == 3)?.NguoiDuyet_ID;

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
                danhSachXe = new List<ChiTietDonVM>();
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

                var bienSoList = model.DanhSachXe.Select(x => x.BienSoXe?.Trim().ToUpper()).ToList();
                if (bienSoList.Count != bienSoList.Distinct().Count())
                {
                    return Json(new { success = false, message = "Không được phép có hai biển số xe trùng nhau trong cùng một đơn!" });
                }

                // Tìm đơn đăng ký
                var donDangKy = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == model.Ma_Don);
                if (donDangKy == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn đăng ký" });
                }

                // Kiểm tra quyền chỉnh sửa
                var loginID = Models.MyAuthentication.ID;
                var ntUser = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == loginID);
                if (ntUser == null)
                    return Json(new { success = false, message = "Không tìm thấy người dùng đăng nhập" });

                var idntLogin = ntUser.IDNT;

                if (donDangKy.NhanVienNT_ID != idntLogin)
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa đơn này" });


                // Kiểm tra trạng thái đơn
                if (donDangKy.TinhTrang_ID != (int)TinhTrangDonDangKy.Nhap)
                {
                    return Json(new { success = false, message = "Đơn đã được duyệt, không thể chỉnh sửa" });
                }

                // Xử lý file upload
                if (FileHoSoXE != null && FileHoSoXE.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(FileHoSoXE.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/UploadedFiles/XeCoDong/"), fileName);
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
                        TuNgay = xe.TuNgay ?? DateTime.Now,
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

                // Thêm lại luồng trình ký mới theo cấp 0-1-2-3
                // Cấp 0: Nhà thầu (người tạo đơn)
                db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                {
                    Ma_Don = model.Ma_Don,
                    CapDuyet = 0,
                    NguoiDuyet_ID = model.NhanVienNT_ID ?? idntLogin,
                    NgayDuyet = DateTime.Now,
                    TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
                    GhiChu = "Trình ký từ nhà thầu"
                });

                // Cấp 1: Kỹ thuật viên
                if (KTV_ID.HasValue)
                {
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = model.Ma_Don,
                        CapDuyet = 1,
                        NguoiDuyet_ID = KTV_ID.Value,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
                        GhiChu = "Chờ duyệt - Nhân viên"
                    });
                }

                // Cấp 2: Trưởng/Phó phòng
                if (TP_ID.HasValue)
                {
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = model.Ma_Don,
                        CapDuyet = 2,
                        NguoiDuyet_ID = TP_ID.Value,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
                        GhiChu = "Chờ duyệt cấp - Trưởng/Phó Bộ phận"
                    });
                }

                // Cấp 3: CPT (VP1C)
                if (VP1C_ID.HasValue)
                {
                    db_dk.CDNT_TrinhKy.Add(new CDNT_TrinhKy
                    {
                        Ma_Don = model.Ma_Don,
                        CapDuyet = 3,
                        NguoiDuyet_ID = VP1C_ID.Value,
                        TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap,
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
                var nhanVien = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == nhanVienNT_ID);

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

            var dsXe = db_dk.CDNT_ChiTietDon.Where(x => x.Ma_Don == maDon && x.TrangThaiDuyet_ID != 2).ToList();
            if (isApproved)
            {
                don.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;
                phanCong.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;

                var bienSoBiTuChoiRaw = Request.Form["bienSoBiTuChoi"];
                var bienSoBiTuChoi = string.IsNullOrWhiteSpace(bienSoBiTuChoiRaw)
                    ? new List<string>()  // Không có giá trị => không từ chối xe nào
                    : bienSoBiTuChoiRaw.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
                // Nếu tất cả xe đều bị từ chối -> coi như không hợp lệ
                if (bienSoBiTuChoi.Count == dsXe.Count)
                {
                    return Json(new { success = false, message = "Bạn phải chọn ít nhất một xe để duyệt." });
                }

                foreach (var xe in dsXe)
                {
                    // xe.TrangThaiDuyet_ID = bienSoBiTuChoi.Contains(xe.BienSoXe) ? 2 : 1;
                    var bs = (xe.BienSoXe ?? "").Trim().ToUpper();
                    if (bienSoBiTuChoi.Contains(bs))
                    {
                        xe.TrangThaiDuyet_ID = 2; // Từ chối
                    }
                    else
                    {
                        xe.TrangThaiDuyet_ID = 1; // Được duyệt
                        xe.TTHD = 1;              // YÊU CẦU MỚI: đánh dấu hoạt động = 1 khi xe được duyệt
                    }
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

            return Json(new
            {
                success = true,
                message = "Đơn đã được xử lý.",
                redirectUrl = Url.Action("HPDQ_Index", "TheXeCoDong_NT", new { area = "TagSign" })
            });
        }
        public ActionResult Detail_PDF(string maDon)
        {
            var model = GetDonDangKyPdfViewModel(maDon);
            if (model == null || model.ChiTietDon == null || !model.ChiTietDon.Any())
                return HttpNotFound("Không có dữ liệu.");

            return View(model);
        }
        public ActionResult DanhSachXe(DateTime? begind, DateTime? endd, string search, int? nhaThauId, int? loaiPhuongTien, int? TrangThai, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 30;
            // Lấy danh sách nhà thầu cho dropdown
            var listNhaThau = db.NT_Partner
                .OrderBy(x => x.FullName)
                .Select(x => new { x.ID, x.FullName, x.BPID, x.ShortName })
                .ToList();
            ViewBag.ListNhaThau = new SelectList(listNhaThau, "ID", "FullName", nhaThauId);

            // Lấy danh sách loại phương tiện cho dropdown
            var listLoaiPT = db_dk.CDNT_LoaiPhuongTien
                .OrderBy(x => x.LoaiPhuongTien)
                .Select(x => new { x.ID, x.LoaiPhuongTien })
                .ToList();
            ViewBag.ListLoaiPhuongTien = new SelectList(listLoaiPT, "ID", "LoaiPhuongTien", loaiPhuongTien);

            var data = db_dk.Database.SqlQuery<XeCoDongModel>(
                @"EXEC CDNT_ChiTietDon_Search 
                @p_NhaThauID, 
                @p_LoaiPhuongTien, 
                @p_BienSoXe,
                @p_TuNgay,
                @p_DenNgay,
                @p_TTHD",
            new SqlParameter("@p_NhaThauID", nhaThauId ?? (object)DBNull.Value),
            new SqlParameter("@p_LoaiPhuongTien", SqlDbType.Int) { Value = (object)loaiPhuongTien ?? DBNull.Value },
            new SqlParameter("@p_BienSoXe", SqlDbType.NVarChar, 50) { Value = search ?? (object)DBNull.Value },
            new SqlParameter("@p_TuNgay", SqlDbType.Date) { Value = begind ?? (object)DBNull.Value },
            new SqlParameter("@p_DenNgay", SqlDbType.Date) { Value = endd ?? (object)DBNull.Value },
            new SqlParameter("@p_TTHD", SqlDbType.Int) { Value = (object)TrangThai ?? DBNull.Value }
             ).ToList();

            var pagedData = data.ToPagedList(pageNumber, pageSize);

            ViewBag.BeginDate = begind?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endd?.ToString("yyyy-MM-dd");
            ViewBag.NhaThauId = nhaThauId;
            ViewBag.LoaiPhuongTien = loaiPhuongTien;
            ViewBag.Search = search;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TrangThai = TrangThai;

            return View(pagedData);
        }
        public ActionResult ExportDonDangKyPdf(string maDon)
        {
            var vm = GetDonDangKyPdfViewModel(maDon);
            if (vm == null || vm.ChiTietDon == null || !vm.ChiTietDon.Any())
                return HttpNotFound("Không có dữ liệu.");

            string tempPdfPath = GenerateTempPdf(maDon);

            byte[] pdfBytes = System.IO.File.ReadAllBytes(tempPdfPath);
            System.IO.File.Delete(tempPdfPath);

            // ưu tiên lấy tên nhà thầu, fallback sang mã đơn
            var safe = SanitizeFileName(
                vm.ChiTietDon.FirstOrDefault()?.FullName ?? maDon ?? "don_dang_ky") + ".pdf";

            return File(pdfBytes, "application/pdf", safe);
        }
        private DonDangKyPDFViewModel GetDonDangKyPdfViewModel(string maDon)
        {
            var vm = new DonDangKyPDFViewModel
            {
                ChiTietDon = new List<CDNT_DonDangKyDetail>(),
                TrinhKy = new List<TrinhKyModel>()
            };

            // Lưu ý: dùng đúng tên SP của bạn
            const string spName = "[dbo].[CDNT_DonDangKy_PDF_test]"; // hoặc CDNT_DonDangKy_PDF

            var conn = db_dk.Database.Connection;
            bool mustClose = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    mustClose = true;
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var p = cmd.CreateParameter();
                    p.ParameterName = "@Ma_Don";
                    p.Value = (object)maDon ?? DBNull.Value;
                    p.DbType = DbType.String;
                    cmd.Parameters.Add(p);

                    using (var reader = cmd.ExecuteReader())
                    {
                        // RS1: ChiTietDon
                        while (reader.Read())
                        {
                            vm.ChiTietDon.Add(new CDNT_DonDangKyDetail
                            {
                                // chú ý tên cột khớp SELECT trong SP
                                ID = reader.GetInt32(reader.GetOrdinal("Don_ID")),
                                Ma_Don = reader["Ma_Don"] as string,
                                NoiDung = reader["NoiDung"] as string,
                                BPQL_ID = reader["BPQL_ID"] as int?,
                                TenPhongBan = reader["TenPhongBan"] as string,
                                NhanVienNT_ID = reader["NhanVienNT_ID"] as int?,
                                NhaThau_ID = reader["NhaThau_ID"] as int?,
                                FullName = reader["FullName"] as string,
                                HopDong = reader["HopDong"] as string,
                                NgayTrinhKy = reader["NgayTrinhKy"] as DateTime?,
                                FileHoSoXe = reader["FileHoSoXe"] as string,
                                TrinhKy_ID = reader["TrinhKy_ID"] as int?,
                                TinhTrang_ID = reader["TinhTrang_ID"] as int?,
                                TenTinhTrang = reader["TenTinhTrang"] as string,
                                LoaiNT_ID = reader["LoaiNT_ID"] as int?,

                                ChiTiet_ID = (int)reader["ChiTiet_ID"],
                                ID_LoaiPhuongTien = reader["ID_LoaiPhuongTien"] as int?,
                                LoaiPhuongTien = reader["LoaiPhuongTien"] as string,
                                BienSoXe = reader["BienSoXe"] as string,
                                CapMoi = reader["CapMoi"] as bool?,
                                CapLai = reader["CapLai"] as bool?,
                                GiaHan = reader["GiaHan"] as bool?,
                                TuNgay = reader["TuNgay"] as DateTime?,
                                DenNgay = reader["DenNgay"] as DateTime?,
                                GhiChu = reader["GhiChu"] as string,
                                HoSoTheoXe = reader["HoSoTheoXe"] as string,
                                TrangThaiDuyet_ID = reader["TrangThaiDuyet_ID"] as int?
                            });
                        }

                        // RS2: TrinhKy
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                vm.TrinhKy.Add(new TrinhKyModel
                                {
                                    TrinhKy_ID = (int)reader["TrinhKy_ID"],
                                    Ma_Don = reader["Ma_Don"] as string,
                                    CapDuyet = reader["CapDuyet"] as int?,
                                    NguoiDuyet_ID = reader["NguoiDuyet_ID"] as int?,
                                    NgayDuyet = reader["NgayDuyet"] as DateTime?,
                                    TinhTrang_ID = reader["TinhTrang_ID"] as int?,
                                    GhiChu = reader["GhiChu"] as string,
                                    TenNguoiDuyet = reader["TenNguoiDuyet"] as string,
                                    ChuKyNguoiDuyet = reader["ChuKyNguoiDuyet"] as string
                                });
                            }
                        }
                    }
                }
            }
            finally
            {
                if (mustClose) conn.Close();
            }

            return vm;
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
                var p_DinhBienXinThem_XeMay = new ObjectParameter("p_DinhBienXinThem_XeMay", typeof(int));
                var p_DinhBienXinThem_Xe3Gac = new ObjectParameter("p_DinhBienXinThem_Xe3Gac", typeof(int));


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
                        p_Xe3Gac_ConLai,
                        p_DinhBienXinThem_XeMay,
                        p_DinhBienXinThem_Xe3Gac
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
                result.DinhBienXinThem_XeMay = (int)p_DinhBienXinThem_XeMay.Value;
                result.DinhBienXinThem_Xe3Gac = (int)p_DinhBienXinThem_Xe3Gac.Value;
                return result;
            }
        }
        public FileResult DownloadExcel()
        {
            // Đổi tên file đúng với biểu mẫu thẻ xe cơ động của bạn
            var fileName = "BM_09.QT20 Dondangkyxecodongnhathau.xlsx";
            var filePath = Server.MapPath("~/App_Data/" + fileName);

            if (!System.IO.File.Exists(filePath))
            {
                // Vì chữ ký trả về FileResult nên ném 404 thay vì HttpNotFound()
                throw new HttpException(404, "Không tìm thấy biểu mẫu thẻ xe cơ động để tải.");
            }

            // Lấy MIME theo đuôi file
            var mime = MimeMapping.GetMimeMapping(fileName);

            // Trả file cho người dùng tải xuống (Content-Disposition: attachment)
            return File(filePath, mime, fileName);
        }
        public ActionResult TrinhKy(string maDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDon))
                    return Json(new { success = false, message = "Thiếu mã đơn." });

                var don = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == maDon);
                if (don == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn." });

                //if (don.NhanVienNT_ID != Models.MyAuthentication.ID)

                //return Json(new { success = false, message = "Bạn không có quyền trình ký đơn này." });
                // Lấy ID của user khi đăng nhập
                var idLogin = Models.MyAuthentication.ID;

                var ntUser = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == idLogin);
                if (ntUser == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin NT_UserTemp." });

                var idntLogin = ntUser.IDNT;

                if (don.NhanVienNT_ID != idntLogin)
                    return Json(new { success = false, message = "Bạn không có quyền trình ký đơn này." });


                if (don.TinhTrang_ID != (int)TinhTrangDonDangKy.Nhap)
                    return Json(new { success = false, message = "Đơn không ở trạng thái nháp." });

                // 1. Cập nhật trạng thái đơn
                don.TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy;
                don.NgayTrinhKy = DateTime.Now;

                // 2. Lấy toàn bộ luồng ký hiện có
                var steps = db_dk.CDNT_TrinhKy
                    .Where(x => x.Ma_Don == maDon)
                    .ToList();

                // 3. Cập nhật cấp 0 (nếu tồn tại) – KHÔNG tạo mới
                var cap0 = steps.FirstOrDefault(x => x.CapDuyet == 0);
                if (cap0 != null)
                {
                    if (cap0.TinhTrang_ID != (int)TinhTrangDonDangKy.DaXuLy)
                    {
                        cap0.TinhTrang_ID = (int)TinhTrangDonDangKy.DaXuLy;
                        cap0.NgayDuyet = DateTime.Now;
                    }
                    if (string.IsNullOrWhiteSpace(cap0.GhiChu))
                        cap0.GhiChu = "Trình ký từ nhà thầu";
                }

                // 4. Chuyển các bước cấp > 0 từ Nháp -> Chờ xử lý
                var draftApprovals = steps
                    .Where(x => x.CapDuyet > 0 && x.CapDuyet <= 3 && x.TinhTrang_ID == (int)TinhTrangDonDangKy.Nhap)
                    .ToList();

                foreach (var step in draftApprovals)
                {
                    step.TinhTrang_ID = (int)TinhTrangDonDangKy.ChoXuLy;
                    // Ghi chú chuẩn hóa theo cấp
                    switch (step.CapDuyet)
                    {
                        case 1:
                            step.GhiChu = "Chờ duyệt - NV";
                            break;
                        case 2:
                            step.GhiChu = "Chờ duyệt - Tr/P";
                            break;
                        case 3:
                            step.GhiChu = "Chờ duyệt - CPT";
                            break;
                        default:
                            step.GhiChu = $"Chờ duyệt - Cấp {step.CapDuyet}";
                            break;
                    }
                }

                db_dk.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Trình ký thành công. Trạng thái luồng đã được cập nhật."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public ActionResult HuyTrinhKy(string maDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDon))
                    return Json(new { success = false, message = "Thiếu mã đơn." });

                var don = db_dk.CDNT_DonDangKy.FirstOrDefault(x => x.Ma_Don == maDon);
                if (don == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn." });

                var idLogin = Models.MyAuthentication.ID;

                var ntUser = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == idLogin);
                if (ntUser == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin NT_UserTemp." });

                var idntLogin = ntUser.IDNT;

                if (don.NhanVienNT_ID != idntLogin)
                    return Json(new { success = false, message = "Bạn không có quyền trình ký đơn này." });


                // Chỉ hủy khi đơn đang ở trạng thái Chờ xử lý
                if (don.TinhTrang_ID != (int)TinhTrangDonDangKy.ChoXuLy)
                    return Json(new { success = false, message = "Chỉ được hủy khi đơn đang ở trạng thái Chờ xử lý." });

                // Chặn hủy nếu đã có cấp > 0 xử lý (DaXuLy hoặc KhongDatYeuCau)
                bool daCoXuLy = db_dk.CDNT_TrinhKy.Any(x =>
                    x.Ma_Don == maDon &&
                    x.CapDuyet > 0 &&
                    (x.TinhTrang_ID == (int)TinhTrangDonDangKy.DaXuLy ||
                     x.TinhTrang_ID == (int)TinhTrangDonDangKy.KhongDatYeuCau));
                if (daCoXuLy)
                    return Json(new { success = false, message = "Không thể hủy: đã có cấp duyệt xử lý." });

                // Lấy tất cả các bước
                var steps = db_dk.CDNT_TrinhKy.Where(x => x.Ma_Don == maDon).ToList();

                // 1. CẤP 0 về Nháp
                var cap0 = steps.FirstOrDefault(x => x.CapDuyet == 0);
                if (cap0 != null)
                {
                    cap0.TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap;
                    cap0.NgayDuyet = null;
                    cap0.GhiChu = "Nhà thầu";
                }

                // 2. Các bước cấp > 0 đang Chờ xử lý (1) → Nháp (5)
                var waitingSteps = steps
                    .Where(x => x.CapDuyet > 0 && x.CapDuyet <= 3 && x.TinhTrang_ID == (int)TinhTrangDonDangKy.ChoXuLy)
                    .ToList();

                foreach (var s in waitingSteps)
                {
                    s.TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap;
                    // Ghi chú theo cấp
                    switch (s.CapDuyet)
                    {
                        case 1:
                            s.GhiChu = "Lưu nháp - NV";
                            break;
                        case 2:
                            s.GhiChu = "Lưu nháp - Tr/P";
                            break;
                        case 3:
                            s.GhiChu = "Lưu nháp - CPT";
                            break;
                        default:
                            s.GhiChu = $"Lưu nháp - Cấp {s.CapDuyet}";
                            break;
                    }
                    s.NgayDuyet = null;
                }

                // 3. Đơn về Nháp
                don.TinhTrang_ID = (int)TinhTrangDonDangKy.Nhap;
                // Nếu muốn reset thời điểm trình ký:
                // don.NgayTrinhKy = null;

                db_dk.SaveChanges();

                return Json(new { success = true, message = "Hủy trình ký thành công. Toàn bộ (kể cả cấp 0) đã về Nháp." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public JsonResult HPDQ_Notify()
        {
            int userId = Models.MyAuthentication.ID;

            // Lấy các bước trình ký mà user là người duyệt, đang chờ xử lý
            var mySteps = db_dk.CDNT_TrinhKy
                .Where(x => x.NguoiDuyet_ID == userId && x.TinhTrang_ID == 1)
                .Select(x => new { x.Ma_Don, x.CapDuyet })
                .ToList();

            int count = 0;
            foreach (var step in mySteps)
            {
                // Lấy các bước trước cùng đơn
                var lowerSteps = db_dk.CDNT_TrinhKy
                    .Where(x => x.Ma_Don == step.Ma_Don && (x.CapDuyet ?? 0) < (step.CapDuyet ?? 0))
                    .Select(x => x.TinhTrang_ID)
                    .ToList();

                // Nếu tất cả các bước trước đã xử lý hoặc hoàn thành (2, 3) thì tới lượt user
                if (lowerSteps.All(s => s == 2 || s == 3))
                {
                    count++;
                }
            }

            return Json(new
            {
                Total = count
            }, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult HPDQ_Index_DaXuLy(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        //{
        //    int pageNumber = page ?? 1;
        //    int pageSize = 10;
        //    int userId = Models.MyAuthentication.ID;

        //    // Lấy tất cả đơn đã xử lý hoặc hoàn thành
        //    var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
        //        "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate, @p_MaPhieu",
        //        new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
        //        new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
        //        new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
        //    ).ToList();
        //    var userSteps = db_dk.CDNT_TrinhKy.Where(x => x.NguoiDuyet_ID == userId && (x.TinhTrang_ID == 2 || x.TinhTrang_ID == 4))
        //       .Select(x => x.Ma_Don)
        //       .Distinct()
        //       .ToList();
        //    var filtered = data
        //    .Where(d => userSteps.Contains(d.Ma_Don) && (d.TinhTrang_ID == 2 || d.TinhTrang_ID == 3))
        //    .OrderByDescending(d => d.NgayTrinhKy ?? DateTime.MinValue)
        //    .ToList();
        //    if (!filtered.Any())
        //    {
        //        SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
        //        return View(new List<DonDangKyViewModel>().ToPagedList(pageNumber, pageSize));
        //    }

        //    var ordered = data.ToPagedList(pageNumber, pageSize);

        //    SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
        //    return View(ordered);
        //}
        public ActionResult HPDQ_Index_DaXuLy(DateTime? begind, DateTime? endd, string maPhieu, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;
            int userId = Models.MyAuthentication.ID;

            // Lấy tất cả đơn
            var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
                "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate, @p_MaPhieu",
                new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
                new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
            ).ToList();

            // Lấy các đơn mà user đã duyệt ở bước nào đó (và bước đó đã xử lý hoặc hoàn thành)
            var userSteps = db_dk.CDNT_TrinhKy
                .Where(x => x.NguoiDuyet_ID == userId && (x.TinhTrang_ID == 2 || x.TinhTrang_ID == 4))
                .Select(x => x.Ma_Don)
                .Distinct()
                .ToList();

            // Lấy danh sách các đơn mà tất cả các bước đều đã xử lý hoặc hoàn thành
            var maDonList = data.Select(d => d.Ma_Don).Distinct().ToList();
            var stepsByDon = db_dk.CDNT_TrinhKy
                .Where(x => maDonList.Contains(x.Ma_Don) && x.CapDuyet >= 0 && x.CapDuyet <= 3)
                .GroupBy(x => x.Ma_Don)
                .ToDictionary(g => g.Key, g => g.ToList());

            var filtered = data
                .Where(d =>
                    userSteps.Contains(d.Ma_Don) &&
                    stepsByDon.ContainsKey(d.Ma_Don) &&
                    stepsByDon[d.Ma_Don].All(s => s.TinhTrang_ID == 2 || s.TinhTrang_ID == 4)
                )
                .OrderByDescending(d => d.NgayTrinhKy ?? DateTime.MinValue)
                .ToList();

            if (!filtered.Any())
            {
                SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
                return View(new List<DonDangKyViewModel>().ToPagedList(pageNumber, pageSize));
            }

            var ordered = filtered.ToPagedList(pageNumber, pageSize);

            SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
            return View(ordered);
        }

        [HttpPost]
        public ActionResult ImportExcelXe(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return Json(new { success = false, message = "Vui lòng chọn file Excel!" });

            using (var workbook = new XLWorkbook(file.InputStream))
            {
                var ws = workbook.Worksheet(1); // Lấy sheet đầu tiên
                var rows = ws.RangeUsed().RowsUsed().Skip(1); // Bỏ qua dòng tiêu đề

                foreach (var row in rows)
                {
                    var tenDayDu = row.Cell(1).GetString().Trim();
                    string bpidStr = null;
                    int? idNhaThau = null;

                    if (!string.IsNullOrEmpty(tenDayDu))
                    {
                        var match = Regex.Match(tenDayDu, @"^\d+"); // lấy số ở đầu
                        if (match.Success)
                        {
                            bpidStr = match.Value; // => "10304"
                        }
                        if (!string.IsNullOrEmpty(bpidStr))
                        {
                            if (int.TryParse(bpidStr, out var bpidInt))
                            {
                                var partner = db.NT_Partner.FirstOrDefault(x => x.BPID == bpidInt);
                                idNhaThau = partner?.ID;
                            }
                        }
                    }

                    var bienSoXe = NormalizeBienSo(row.Cell(2).GetString().Trim());     // Cột B: BIỂN KIỂM SOÁT
                    var loaiPTStr = row.Cell(3).GetString().Trim();    // Cột C: LOẠI PHƯƠNG TIỆN
                    var tuNgay = ParseExcelDate(row.Cell(4));          // Cột D: NGÀY CẤP
                    var denNgay = ParseExcelDate(row.Cell(5));         // Cột E: THỜI HẠN
                    var ttHD = row.Cell(6).GetString().Trim();           // Cột F: TRẠNG THÁI HOẠT ĐỘNG

                    int? idLoaiPhuongTien = null;
                    switch (loaiPTStr.ToUpper())
                    {
                        case "XE MÁY": idLoaiPhuongTien = 1; break;
                        case "XE BA GÁC": idLoaiPhuongTien = 2; break;
                        case "XE Ô TÔ": idLoaiPhuongTien = 3; break;
                    }
                    int? idTrangThaiHoatDong = null;
                    switch (ttHD.ToUpper())
                    {

                        case "HẾT HẠN": idTrangThaiHoatDong = (int)(TrangThaiHoatDong.HetHan); break;// Hết hạn
                        case "ĐÃ KHÓA": idTrangThaiHoatDong = (int)TrangThaiHoatDong.DaKhoa; break; // Đã khóa
                        default:
                            idTrangThaiHoatDong = (int)TrangThaiHoatDong.HoatDong; // mặc định an toàn
                            break;
                    }
                    // Kiểm tra biển số xe đã tồn tại chưa
                    var chiTietDon = db_dk.CDNT_ChiTietDon.FirstOrDefault(x => x.BienSoXe == bienSoXe);

                    if (chiTietDon != null)
                    {
                        // Đã có, cập nhật thông tin
                        chiTietDon.TuNgay = tuNgay;
                        chiTietDon.DenNgay = denNgay;
                        chiTietDon.TrangThaiDuyet_ID = 1;
                        // Nếu muốn cập nhật luôn nhà thầu hoặc loại phương tiện:
                        chiTietDon.ID_NhaThau = idNhaThau;
                        chiTietDon.ID_LoaiPhuongTien = idLoaiPhuongTien;
                        chiTietDon.TTHD = idTrangThaiHoatDong;
                        chiTietDon.User_Edit = Models.MyAuthentication.ID;
                        // Không cần .Add(), Entity Framework sẽ tự nhận là update
                    }
                    else
                    {
                        // Chưa có, thêm mới
                        chiTietDon = new CDNT_ChiTietDon
                        {
                            BienSoXe = bienSoXe,
                            TuNgay = tuNgay,
                            DenNgay = denNgay,
                            ID_NhaThau = idNhaThau,
                            ID_LoaiPhuongTien = idLoaiPhuongTien,
                            TrangThaiDuyet_ID = 1,
                            TTHD = idTrangThaiHoatDong,
                            User_Edit = Models.MyAuthentication.ID,
                        };
                        db_dk.CDNT_ChiTietDon.Add(chiTietDon);
                    }
                }
                db_dk.SaveChanges();
            }
            return Json(new { success = true, message = "Import thành công!" });
        }

        public FileResult DownloadFile()
        {
            // Đổi tên file đúng với biểu mẫu thẻ xe cơ động của bạn
            var fileName = "Template_XCD.xlsx";
            var filePath = Server.MapPath("~/App_Data/" + fileName);

            if (!System.IO.File.Exists(filePath))
            {
                // Vì chữ ký trả về FileResult nên ném 404 thay vì HttpNotFound()
                throw new HttpException(404, "Không tìm thấy biểu mẫu thẻ xe cơ động để tải.");
            }

            // Lấy MIME theo đuôi file
            var mime = MimeMapping.GetMimeMapping(fileName);

            // Trả file cho người dùng tải xuống (Content-Disposition: attachment)
            return File(filePath, mime, fileName);
        }

        public ActionResult HPDQ_Index_All(DateTime? begind, DateTime? endd, string maPhieu, int? trangThai, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            // Chỉ cho phép user có VIEW_ALL
            int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
            string controll = "TheXeCoDong_NT";
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<div class='alert alert-danger'>Bạn không có quyền xem dữ liệu này.</div>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            // 1) Lấy dữ liệu theo filter
            var data = db_dk.Database.SqlQuery<DonDangKyViewModel>(
                "EXEC CDNT_DonDangKy_Search @p_BeginDate, @p_EndDate, @p_MaPhieu",
                new SqlParameter("@p_BeginDate", (object)begind ?? DBNull.Value),
                new SqlParameter("@p_EndDate", (object)endd ?? DBNull.Value),
                new SqlParameter("@p_MaPhieu", (object)maPhieu ?? DBNull.Value)
            ).ToList();

            if (!data.Any())
            {
                SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
                return View(new List<DonDangKyViewModel>().ToPagedList(pageNumber, pageSize));
            }

            // 2) Lấy steps và group theo Ma_Don (tính trạng thái tổng thể)
            var maDonList = data.Select(d => d.Ma_Don).Distinct().ToList();

            var stepsRaw = db_dk.CDNT_TrinhKy.AsNoTracking()
                .Where(x => maDonList.Contains(x.Ma_Don) && (x.CapDuyet ?? 0) > 0) // chỉ cấp > 0
                .Select(x => new StepInfo
                {
                    Ma_Don = x.Ma_Don,
                    CapDuyet = x.CapDuyet,
                    TinhTrang_ID = x.TinhTrang_ID
                })
                .ToList();

            var stepsByDon = stepsRaw
                .GroupBy(s => s.Ma_Don)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var don in data)
            {
                var stepGt0 = stepsByDon.TryGetValue(don.Ma_Don, out var lst) ? lst : new List<StepInfo>();

                if (!stepGt0.Any() || stepGt0.All(s => s.TinhTrang_ID == 5))
                {
                    don.TinhTrang_ID = 5; don.TenTinhTrang = "Chưa trình ký";
                }
                else if (stepGt0.Any(s => s.TinhTrang_ID == 4))
                {
                    don.TinhTrang_ID = 4; don.TenTinhTrang = "Không đạt yêu cầu";
                }
                else if (stepGt0.All(s => s.TinhTrang_ID == 2 || s.TinhTrang_ID == 3))
                {
                    don.TinhTrang_ID = stepGt0.Any(s => s.TinhTrang_ID == 3) ? 3 : 2;
                    don.TenTinhTrang = don.TinhTrang_ID == 3 ? "Hoàn thành" : "Đã xử lý";
                }
                else
                {
                    don.TinhTrang_ID = 1; don.TenTinhTrang = "Chờ xử lý";
                }
            }

            // 3) Combobox Trạng thái
            var trangThaiItems = new List<SelectListItem>
            {
                new SelectListItem{ Value = "", Text = "Tất cả trạng thái" },
                new SelectListItem{ Value = "1", Text = "Chờ xử lý" },
                new SelectListItem{ Value = "2", Text = "Đã xử lý" },
                new SelectListItem{ Value = "3", Text = "Hoàn thành" },
                new SelectListItem{ Value = "4", Text = "Không đạt yêu cầu" }
            };
            foreach (var it in trangThaiItems)
            {
                it.Selected = (!string.IsNullOrEmpty(it.Value) && trangThai.HasValue && it.Value == trangThai.Value.ToString());
            }
            ViewBag.TrangThaiList = trangThaiItems;

            // 4) Áp filter theo trạng thái
            if (trangThai.HasValue)
            {
                data = data.Where(d => d.TinhTrang_ID == trangThai.Value).ToList();
            }
            else
            {
                // Mặc định: lấy tất cả TRỪ "Chưa trình ký"
                data = data.Where(d => d.TinhTrang_ID != 5).ToList();
            }

            var paged = data
                .OrderByDescending(d => d.NgayTrinhKy ?? DateTime.MinValue)
                .ToPagedList(pageNumber, pageSize);

            SetFilters(begind, endd, maPhieu, pageNumber, pageSize);
            return View(paged);
        }


        /// hàm phụ trợ
        private string SanitizeFileName(string input)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars());
            string pattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(input, pattern, "_");
        }
        private static string NormalizeBienSo(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var s = input.Normalize(NormalizationForm.FormKC).Trim();

            // Thay NBSP -> space rồi sẽ xử lý qua regex
            s = s.Replace('\u00A0', ' '); // NBSP
                                          // Chuẩn hóa các loại gạch ngang về '-'
            s = s.Replace('–', '-').Replace('—', '-');

            // Bỏ khoảng trắng quanh dấu '-' -> còn lại một dấu '-'
            s = Regex.Replace(s, @"\s*-\s*", "-");

            // Bỏ mọi khoảng trắng còn lại (space, tab, …)
            s = Regex.Replace(s, @"\s+", "");

            // Đưa về uppercase
            s = s.ToUpperInvariant();

            return s;
        }

        // trong TheXeCoDong_NTController.cs
        public ActionResult HPDQ_DinhBienXinThem(int? nhaThauID, string q, int? page)
        {
            // --- populate dropdown options for Chosen ---
            var listNhaThau = db.NT_Partner
                .OrderBy(x => x.FullName)
                .Select(x => new { x.ID, x.FullName })
                .ToList();

            // If q contains an ID, preselect it; otherwise use nhaThauID
            int? selectedId = null;
            if (!string.IsNullOrWhiteSpace(q))
            {
                if (int.TryParse(q.Trim(), out int idFromQ))
                {
                    selectedId = idFromQ;
                }
            }
            if (!selectedId.HasValue && nhaThauID.HasValue) selectedId = nhaThauID;

            ViewBag.ListNhaThau = new SelectList(listNhaThau, "ID", "FullName", selectedId);

            // --- existing code: call SP, filter by q, paging ---
            var param = new SqlParameter("@p_NhaThauID", SqlDbType.Int)
            {
                Value = nhaThauID.HasValue ? (object)nhaThauID.Value : DBNull.Value
            };

            var list = db_dk.Database
                            .SqlQuery<DinhBienPhuongTienVM>("EXEC dbo.CNDT_GetDinhBienPhuongTien_All @p_NhaThauID", param)
                            .ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qTrim = q.Trim().ToLower();
                int idFilter;
                bool isNumber = int.TryParse(qTrim, out idFilter);

                list = list.Where(x =>
                            (isNumber && x.NhaThauID == idFilter)
                            || (!string.IsNullOrEmpty(x.NhaThauName) && x.NhaThauName.ToLower().Contains(qTrim))
                        ).ToList();
            }

            int pageNumber = page ?? 1;
            int pageSize = 20;
            var paged = list.OrderBy(x => x.NhaThauID).ToPagedList(pageNumber, pageSize);

            return View(paged);
        }

        [HttpPost]
        //public JsonResult CreateDinhBienXinThem(int NhaThau_ID, int DinhBienXinThem_XeMay, int DinhBienXinThem_Xe3Gac, string GhiChu)
        //{
        //    try
        //    {
        //        int userId = Models.MyAuthentication.ID;

        //        // Bảo đảm giá trị không âm (nếu cần)
        //        var xeMay = Math.Max(0, DinhBienXinThem_XeMay);
        //        var xe3Gac = Math.Max(0, DinhBienXinThem_Xe3Gac);

        //        var sql = @"
        //            INSERT INTO EPORTAL_REGISTER.dbo.CDNT_DinhBienXinThem
        //                (NhaThau_ID, DinhBienXinThem_XeMay, DinhBienXinThem_Xe3Gac, GhiChu, User_Edit, NgayCapNhat)
        //            VALUES
        //                (@NhaThau_ID, @XeMay, @Xe3Gac, @GhiChu, @User_Edit, GETDATE())";

        //        var parameters = new[]
        //        {
        //            new SqlParameter("@NhaThau_ID", SqlDbType.Int) { Value = NhaThau_ID },
        //            new SqlParameter("@XeMay", SqlDbType.Int) { Value = xeMay },
        //            new SqlParameter("@Xe3Gac", SqlDbType.Int) { Value = xe3Gac },
        //            new SqlParameter("@GhiChu", SqlDbType.NVarChar, 4000) { Value = (object)GhiChu ?? DBNull.Value },
        //            new SqlParameter("@User_Edit", SqlDbType.Int) { Value = userId }
        //        };

        //        // Thực thi (EF6)
        //        db_dk.Database.ExecuteSqlCommand(sql, parameters);

        //        return Json(new { success = true, message = "Gửi yêu cầu định biên xin thêm thành công." });
        //    }
        //    catch (Exception ex)
        //    {
        //        // TODO: log lỗi chi tiết vào hệ thống log của bạn
        //        return Json(new { success = false, message = "Lỗi khi lưu: " + ex.Message });
        //    }
        //}
        public JsonResult CreateDinhBienXinThem(int NhaThau_ID, int DinhBienXinThem_XeMay, int DinhBienXinThem_Xe3Gac, string GhiChu)
        {
            try
            {
                int userId = Models.MyAuthentication.ID;

                // Bảo đảm giá trị không âm
                var xeMay = Math.Max(0, DinhBienXinThem_XeMay);
                var xe3Gac = Math.Max(0, DinhBienXinThem_Xe3Gac);

                // SQL delete + insert trong 1 transaction để thay thế bản ghi trước đó của cùng user & nhà thầu
                // Nếu bạn muốn xóa tất cả bản ghi (không quan tâm user) thì thay điều kiện WHERE cho phù hợp.
                using (var tx = db_dk.Database.BeginTransaction())
                {
                    try
                    {
                        var deleteSql = @"
                    DELETE FROM EPORTAL_REGISTER.dbo.CDNT_DinhBienXinThem
                    WHERE NhaThau_ID = @NhaThau_ID
                      AND User_Edit = @User_Edit"; // chỉ xóa các request do chính user này tạo

                        var deleteParams = new[]
                        {
                    new SqlParameter("@NhaThau_ID", SqlDbType.Int) { Value = NhaThau_ID },
                    new SqlParameter("@User_Edit", SqlDbType.Int) { Value = userId }
                };

                        db_dk.Database.ExecuteSqlCommand(deleteSql, deleteParams);

                        var insertSql = @"
                    INSERT INTO EPORTAL_REGISTER.dbo.CDNT_DinhBienXinThem
                        (NhaThau_ID, DinhBienXinThem_XeMay, DinhBienXinThem_Xe3Gac, GhiChu, User_Edit, NgayCapNhat)
                    VALUES
                        (@NhaThau_ID, @XeMay, @Xe3Gac, @GhiChu, @User_Edit, GETDATE())";

                        var insertParams = new[]
                        {
                    new SqlParameter("@NhaThau_ID", SqlDbType.Int) { Value = NhaThau_ID },
                    new SqlParameter("@XeMay", SqlDbType.Int) { Value = xeMay },
                    new SqlParameter("@Xe3Gac", SqlDbType.Int) { Value = xe3Gac },
                    new SqlParameter("@GhiChu", SqlDbType.NVarChar, 4000) { Value = (object)GhiChu ?? DBNull.Value },
                    new SqlParameter("@User_Edit", SqlDbType.Int) { Value = userId }
                };

                        db_dk.Database.ExecuteSqlCommand(insertSql, insertParams);

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }

                return Json(new { success = true, message = "Gửi yêu cầu định biên xin thêm thành công." });
            }
            catch (Exception ex)
            {
                // TODO: log lỗi chi tiết vào hệ thống log của bạn
                return Json(new { success = false, message = "Lỗi khi lưu: " + ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetNhaThauList()
        {
            var list = db.NT_Partner
                .OrderBy(x => x.FullName)
                .Select(x => new { id = x.ID, text = x.FullName })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult XoaXe(int id)
        {
            try
            {
                var xe = db_dk.CDNT_ChiTietDon.Find(id);
                if (xe == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi." });
                }

                db_dk.CDNT_ChiTietDon.Remove(xe);
                db_dk.SaveChanges();

                return Json(new { success = true, message = "Xóa xe thành công." });
            }
            catch (Exception ex)
            {
                // log ex nếu cần
                return Json(new { success = false, message = "Có lỗi khi xóa: " + ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetXe(int id)
        {
            try
            {
                // Lấy dữ liệu thô (DateTime? không format) từ database — EF có thể dịch điều này
                var xeRaw = db_dk.CDNT_ChiTietDon
                    .Where(x => x.ID == id)
                    .Select(x => new
                    {
                        x.ID,
                        x.BienSoXe,
                        x.TuNgay,
                        x.DenNgay,
                        x.ID_LoaiPhuongTien,
                        x.TTHD
                    })
                    .FirstOrDefault();

                if (xeRaw == null)
                    return Json(new { success = false, message = "Không tìm thấy bản ghi." }, JsonRequestBehavior.AllowGet);

                // Chuyển đổi định dạng ngày trong bộ nhớ (sau khi đã materialize)
                var xe = new
                {
                    xeRaw.ID,
                    xeRaw.BienSoXe,
                    TuNgay = xeRaw.TuNgay.HasValue ? xeRaw.TuNgay.Value.ToString("yyyy-MM-dd") : null,
                    DenNgay = xeRaw.DenNgay.HasValue ? xeRaw.DenNgay.Value.ToString("yyyy-MM-dd") : null,
                    xeRaw.ID_LoaiPhuongTien,
                    xeRaw.TTHD
                };

                return Json(new { success = true, data = xe }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Cập nhật chỉ các cột: BienSoXe, TuNgay, DenNgay, ID_LoaiPhuongTien, TTHD
        [HttpPost]
        public JsonResult CapNhatXe(int ID, string BienSoXe, DateTime? TuNgay, DateTime? DenNgay, int? ID_LoaiPhuongTien, int? TTHD)
        {
            try
            {
                var xe = db_dk.CDNT_ChiTietDon.Find(ID);
                if (xe == null)
                    return Json(new { success = false, message = "Không tìm thấy bản ghi." });

                // CHỈ cập nhật những cột yêu cầu
                xe.BienSoXe = string.IsNullOrWhiteSpace(BienSoXe) ? xe.BienSoXe : BienSoXe.Trim();
                xe.TuNgay = TuNgay;
                xe.DenNgay = DenNgay;
                xe.ID_LoaiPhuongTien = ID_LoaiPhuongTien;
                xe.TTHD = TTHD;

                // Ghi lại người chỉnh sửa nếu muốn
                xe.User_Edit = Models.MyAuthentication.ID;

                db_dk.SaveChanges();

                // Lấy tên Loại phương tiện để client hiển thị
                var loaiPT = xe.ID_LoaiPhuongTien.HasValue
                    ? db_dk.CDNT_LoaiPhuongTien
                        .Where(l => l.ID == xe.ID_LoaiPhuongTien)
                        .Select(l => l.LoaiPhuongTien)
                        .FirstOrDefault()
                    : "";

                var userEditName = Models.MyAuthentication.ID; // hoặc query tên user nếu cần

                return Json(new
                {
                    success = true,
                    message = "Cập nhật thành công.",
                    data = new
                    {
                        ID = xe.ID,
                        BienSoXe = xe.BienSoXe,
                        TuNgay = xe.TuNgay.HasValue ? xe.TuNgay.Value.ToString("dd/MM/yyyy") : "",
                        DenNgay = xe.DenNgay.HasValue ? xe.DenNgay.Value.ToString("dd/MM/yyyy") : "",
                        TTHD = xe.TTHD,
                        LoaiPhuongTien = loaiPT,
                        User_EditName = userEditName
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi khi cập nhật: " + ex.Message });
            }
        }

    }
}


