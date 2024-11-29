using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsView360;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class EmployeeAPI_NTController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        // GET: Partner/EmployeeAPI_NT
        public ActionResult Index()
        {
            return View();
        }
        List<Employees_API_NT.data> GetAPI_NT()
        {
            string url = "http://192.168.120.112:60525/api/nhathau/thongtinnhathau";
            string userName = "IT_HPDQ";
            string passWord = "ithpdqAPI@2022";
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + passWord));
                webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                var result = webClient.DownloadString(url);
                result = result.Remove(0, 52);
                result = result.Replace("}]}", "}]");
                var List_NT = JsonConvert.DeserializeObject<List<Employees_API_NT.data>>(result);
                return List_NT.ToList();
            }
        }
        public ActionResult Sync_NT()
        {
            List<Employees_API_NT.data> listNV = GetAPI_NT();
            if (listNV.Count > 0)
            {
                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\NT_VP1C.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\NT_VP1C_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("NT_VP1C");
                int row = 3, rowlast = 1, stt = 0;
                foreach (var item in listNV)
                {
                    var CodeUnis = db.NT_Partner.Where(x => x.CodeUnis == item.maNhaThau).FirstOrDefault();
                    var TenNT = db.NT_Partner.Where(x => x.FullName == item.tenNhaThau).FirstOrDefault();
                    if (CodeUnis == null && TenNT != null)
                    {
                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.maNhaThau;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.tenVietTat;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.tenNhaThau;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.maSAP;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("F" + row).Value = "Chưa có CodeUnis";
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;
                    }
                    else if (CodeUnis == null && TenNT == null)
                    {
                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.maNhaThau;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.tenVietTat;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.tenNhaThau;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.maSAP;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("F" + row).Value = "Chưa có ở Portal";
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;
                    }

                }

                Worksheet.Range("A4:F" + (row)).Style.Font.SetFontName("Times New Roman");
                Worksheet.Range("A4:F" + (row)).Style.Font.SetFontSize(10);
                Worksheet.Range("A4:F" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                Worksheet.Range("A4:F" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                Workbook.SaveAs(fileNameMauTemp);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                string fileName = "Danh sách NT.xlsx";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            return RedirectToAction("Index", "ListPartner");
        }
        List<Employees_API_NV.data> GetAPI_NV()
        {
            string url = "http://192.168.120.112:60525/api/NhaThau/ThongTinNhanVien/";
            string userName = "IT_HPDQ";
            string passWord = "ithpdqAPI@2022";
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + passWord));
                webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                var result = webClient.DownloadString(url);
                result = result.Remove(0, 52);
                result = result.Replace("}]}", "}]");
                var List_NV = JsonConvert.DeserializeObject<List<Employees_API_NV.data>>(result);
                return List_NV.ToList();
            }
        }
        //public ActionResult Sync_NV()
        //{
        //    try
        //    {
        //        int insert = 0;
        //        int update = 0;
        //        List<Employees_API_NV.data> listNV = GetAPI_NV();
        //        if (listNV.Count > 0)
        //        {
        //            string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNVNT.xlsx";
        //            string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNVNT_Temp.xlsx";
        //            XLWorkbook Workbook = new XLWorkbook(fileNameMau);
        //            IXLWorksheet Worksheet = Workbook.Worksheet("NV");
        //            int row = 6, rowlast = 1, stt = 0;
        //            foreach (var item in listNV)
        //            {
        //                var Check = db_nt.NT_NhanVienNT.Where(x => x.CCCD == item.maNhanVien).FirstOrDefault();
        //                var IDNT = db.NT_Partner.Where(x => x.CodeUnis == item.maNhaThau).FirstOrDefault();
        //                if (IDNT != null)
        //                {
        //                    if (Check != null)
        //                    {
        //                        DateTime NgayCapConvert = DateTime.ParseExact(item.ngayCapThe, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
        //                        DateTime HanSuDungConvert = DateTime.ParseExact(item.hanSuDung, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
        //                        var IDHD = db_nt.NT_TTHD.Where(x => x.TenHD == item.tenTrangThai).FirstOrDefault();

        //                        var a = db_nt.NT_NhanVienNT_update(Check.IDNVNT, item.tenNhanVien, item.maNhanVien, null, null, item.maSoThe, IDNT.ID, NgayCapConvert, HanSuDungConvert, IDHD.IDHD);

        //                        if (Check.IDNT == IDNT.ID && Check.NgayCap != NgayCapConvert && Check.HanSuDung != HanSuDungConvert)
        //                        {
        //                            var Insert = db_nt.NT_HoatDongNV_insert(Check.IDNVNT, IDNT.ID, NgayCapConvert, HanSuDungConvert, null, null, null);
        //                        }
        //                        else if (Check.IDNT == IDNT.ID)
        //                        {
        //                            string Ghichu = "Chuyển nhà thầu";
        //                            var Insert = db_nt.NT_HoatDongNV_insert(Check.IDNVNT, IDNT.ID, Check.NgayCap, Check.HanSuDung, null, null, Ghichu);
        //                        }
        //                        else if (Check.NgayCap != NgayCapConvert && Check.HanSuDung != HanSuDungConvert)
        //                        {
        //                            string Ghichu = "Gia hạn thẻ";
        //                            var Insert = db_nt.NT_HoatDongNV_insert(Check.IDNVNT, Check.IDNT, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);

        //                        }
        //                        update++;

        //                    }
        //                    else
        //                    {
        //                        DateTime NgayCapConvert = DateTime.ParseExact(item.ngayCapThe, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
        //                        DateTime HanSuDungConvert = DateTime.ParseExact(item.hanSuDung, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
        //                        var IDHD = db_nt.NT_TTHD.Where(x => x.TenHD == item.tenTrangThai).FirstOrDefault();

        //                        System.Data.Entity.Core.Objects.ObjectParameter returnIDNVNT = new System.Data.Entity.Core.Objects.ObjectParameter("IDNVNT", typeof(int));
        //                        db_nt.NT_NhanVienNT_insertall(item.tenNhanVien, item.maNhanVien, null, null, item.maSoThe, IDNT.ID, NgayCapConvert, HanSuDungConvert, IDHD.IDHD, returnIDNVNT);
        //                        int IDNVNT = Convert.ToInt32(returnIDNVNT.Value);
        //                        var Insert = db_nt.NT_HoatDongNV_insert(IDNVNT, item.idNhaThau, NgayCapConvert, HanSuDungConvert, null, null, null);
        //                        insert++;

        //                    }
        //                }
        //                else
        //                {
        //                    row++; stt++;
        //                    rowlast = row + 1;

        //                    Worksheet.Cell("A" + row).Value = stt;
        //                    Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

        //                    Worksheet.Cell("B" + row).Value = item.maNhanVien;
        //                    Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //                    Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

        //                    Worksheet.Cell("C" + row).Value = item.maNhanVien;
        //                    Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

        //                    Worksheet.Cell("D" + row).Value = null;
        //                    Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


        //                    Worksheet.Cell("E" + row).Value = null;
        //                    Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;



        //                    Worksheet.Cell("F" + row).Value = item.maSoThe;
        //                    Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;


        //                    Worksheet.Cell("G" + row).Value = item.tenNhaThau;
        //                    Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

        //                    Worksheet.Cell("H" + row).Value = item.ngayCapThe;
        //                    Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("H" + row).Style.DateFormat.Format = "dd/MM/yyyy";
        //                    Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

        //                    Worksheet.Cell("I" + row).Value = item.hanSuDung;
        //                    Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("I" + row).Style.DateFormat.Format = "dd/MM/yyyy";
        //                    Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

        //                    var TTHD = db_nt.NT_TTHD.Where(x => x.TenHD == item.tenTrangThai).FirstOrDefault();
        //                    Worksheet.Cell("J" + row).Value = TTHD.TenHD;
        //                    Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //                    Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //                    Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;
        //                    row = rowlast - 1;
        //                }

        //            }
        //            TempData["msgSuccess"] = "<script>alert('Thêm mới: " + insert + " và Cập nhật:" + update + " nhân viên nhà thầu');</script>";

        //            Worksheet.Range("A7:J" + (row)).Style.Font.SetFontName("Times New Roman");
        //            Worksheet.Range("A7:J" + (row)).Style.Font.SetFontSize(10);
        //            Worksheet.Range("A7:J" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //            Worksheet.Range("A7:J" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //            Workbook.SaveAs(fileNameMauTemp);
        //            byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
        //            string fileName = "Danh sách NVNT.xlsx";
        //            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["msgSuccess"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
        //    }
        //    return RedirectToAction("Index", "ContractorStaff");
        //}

    }
}