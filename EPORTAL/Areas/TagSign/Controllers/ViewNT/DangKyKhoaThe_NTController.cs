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
            var userNameLogin = Models.MyAuthentication.ID;

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

            int nhanVienNT_ID = Models.MyAuthentication.ID; // ID tài khoản NT
            var nhanVien = db_nt.NT_UserTemp.FirstOrDefault(x => x.ID == nhanVienNT_ID); // thông tin tài khoản

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
            return View();
        }
        [HttpPost]
        public ActionResult TaoDonDangKy(TaoDonDangKyViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.NoiDung))
                    return Json(new { success = false, message = "Nội dung đơn không được để trống!" });

                // 1. Tạo JSON danh sách chi tiết từ model.ChiTiet
                var chiTietJson = Newtonsoft.Json.JsonConvert.SerializeObject(model.ChiTiet);

                var Business_Partner = Models.MyAuthentication.Username;
                var thangNam = DateTime.Now.ToString("yyyyMMdd");

                var prefix = $"{Business_Partner}_XCĐ{thangNam}-";

                var lastMaDon = db_dk.KTNT_DonDangKy
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
                // 2. Sinh mã đơn (ví dụ: "DDK-20251110-001")
                string maDon = "DDK-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

                // 3. Lấy thông tin người dùng hiện tại
                string userNameLogin = User.Identity.Name; // hoặc từ MyAuthentication

                // 4. Thực hiện gọi stored procedure

                var result = db.Database.SqlQuery<SPResult>(
                    @"EXEC dbo.KTNT_DonDangKy_Insert
                    @p_Ma_Don,
                    @p_NoiDung,
                    @p_NhaThau_ID,
                    //@p_VP1C_ID,
                    @p_UserNameLogin,
                    @JsonDanhSachChiTiet",
                    new SqlParameter("@p_Ma_Don", maDon),
                    new SqlParameter("@p_NoiDung", model.NoiDung ?? ""),
                    new SqlParameter("@p_NhaThau_ID", model.NhaThau_ID),
                    //new SqlParameter("@p_VP1C_ID", (object)model.VP1C_ID ?? DBNull.Value),
                    new SqlParameter("@p_UserNameLogin", userNameLogin ?? ""),
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
    }
}
