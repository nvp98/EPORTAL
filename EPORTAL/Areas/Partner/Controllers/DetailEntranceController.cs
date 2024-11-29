using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsView360;
using Oracle.ManagedDataAccess.Client;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class DetailEntranceController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        EPORTAL_UNISEntities dbUN = new EPORTAL_UNISEntities();
        String controll = "DetailEntrance";
        string connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ToString();
        // GET: Partner/DetailEntrance
        public ActionResult Index(int? page, DateTime? begind, DateTime? endd, string MaNhaThau, string MaNhanVien)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var NhaThau = (from nt in db.NT_Partner.Where(x => x.CodeUnis != "")
                           select new NT_PartnerValidation
                           {
                               ID = (int)nt.ID,
                               CodeUnis = nt.CodeUnis,
                               FullName = nt.CodeUnis + " : " + nt.ShortName
                           }).ToList();
            ViewBag.NTList = new SelectList(NhaThau, "CodeUnis", "FullName",MaNhaThau);

            var NhanVien = (from nt in db_nt.NT_NhanVienNT
                            select new ContractorStaffValidation
                            {
                                IDNT = (int)nt.IDNVNT,
                                SoThe = nt.SoThe,
                                HoTen = nt.HoTen + " : " + nt.SoThe
                            }).ToList();
            ViewBag.NVList = new SelectList(NhanVien, "SoThe", "HoTen", MaNhanVien);


            string begindcover = "";
            string enddcover = "";
            if (begind == null && endd == null)
            {
                begindcover = DateTime.Now.Date.ToString("yyyyMMdd");
                enddcover = DateTime.Now.Date.ToString("yyyyMMdd");
            }
            else
            {
                string cover = begind.ToString();
                begindcover = DateTime.Parse(cover).ToString("yyyyMMdd");
                string cover1 = endd.ToString();
                enddcover = DateTime.Parse(cover1).ToString("yyyyMMdd");
            }
            List<NT_CardTempENTRY> listNT = new List<NT_CardTempENTRY>();
            listNT = (from a in dbUN.LogUNIS_Query(begindcover, enddcover).Where(x => x.isNT == 1)
                          select new NT_CardTempENTRY
                          {
                              Mathe = a.MaThe,
                              MaNT = a.MaNT,
                              Hoten = a.HoTen,
                              TenNT = a.TenNT,
                              Thoigianquyet = a.Ngay.ToString() + a.Gio.ToString(),
                              Tenmayquyet = a.MQT,
                              CCCD =a.CCCD
                          }).ToList();

            if (MaNhanVien == null) MaNhanVien = "";
            if (MaNhanVien != "") listNT = listNT.Where(x => x.Mathe == MaNhanVien).ToList();
            if (MaNhaThau == null) MaNhaThau = "";
            if (MaNhaThau != "") listNT = listNT.Where(x => x.MaNT == MaNhaThau).ToList();


            string begindcoverNew = DateTime.Parse(DateTime.Now.AddDays(-1).ToString()).ToString("yyyyMMdd");
            string enddcoverNew = DateTime.Parse(DateTime.Now.ToString()).ToString("yyyyMMdd");
            var aa = dbUN.LogUNIS_Query(enddcoverNew, enddcoverNew).OrderByDescending(x => x.Gio).ToList();
            if (aa.Count == 0) aa = dbUN.LogUNIS_Query(begindcoverNew, begindcoverNew).OrderByDescending(x => x.Gio).ToList();
            ViewBag.LastTime = aa.First().Ngay.ToString() + aa.First().Gio.ToString();
            ViewBag.SUM = listNT.Count();


            if (page == null) page = 1;
            int pageSize = 150;
            int pageNumber = (page ?? 1);
            return View(listNT.OrderByDescending(x => x.Thoigianquyet).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ExportToExcel(DateTime? begind, DateTime? endd, string MaNhaThau, string MaNhanVien)
        {
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSQTNT.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSQTNT_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("QT");

                string begindcover = "";
                string enddcover = "";
                //if (MaNhaThau == null) MaNhaThau = 0;
                //if (MaNhanVien == null) MaNhanVien = 0;
                if (begind == null) begind = DateTime.Now;
                if (endd == null) endd = DateTime.Now;
                string cover = begind.ToString();
                begindcover = DateTime.Parse(cover).ToString("yyyyMMdd");
                string cover1 = endd.ToString();
                enddcover = DateTime.Parse(cover1).ToString("yyyyMMdd");


                var res = (from a in dbUN.LogUNIS_Query(begindcover, enddcover).Where(x => x.isNT == 1)
                           select new NT_CardTempENTRY
                           {
                               Mathe = a.MaThe,
                               MaNT = a.MaNT,
                               Hoten = a.HoTen,
                               TenNT = a.TenNT,
                               Thoigianquyet = a.Ngay.ToString() + a.Gio.ToString(),
                               Tenmayquyet = a.MQT,
                               CCCD = a.CCCD
                           }).ToList();
                if (MaNhanVien == null) MaNhanVien = "";
                if (MaNhanVien != "") res = res.Where(x => x.Mathe == MaNhanVien).ToList();
                if (MaNhaThau == null) MaNhaThau = "";
                if (MaNhaThau != "") res = res.Where(x => x.MaNT == MaNhaThau).ToList();

                //var List = db_nt.NT_NhanVienNT.ToList();
                if (res.Count > 0)
                {
                    int row = 4, rowlast = 1, stt = 0;
                    foreach (var item in res)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.Hoten;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.CCCD;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.Mathe;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.MaNT;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("F" + row).Value = item.TenNT;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("G" + row).Value = DateTime.ParseExact(item.Thoigianquyet, "yyyyMMddHHmmss", null).ToString("dd/MM/yyyy HH:mm:ss");
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("H" + row).Value = item.Tenmayquyet;
                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        row = rowlast - 1;
                    }

                    Worksheet.Cell("C2").Value = DateTime.ParseExact(begindcover, "yyyyMMdd", null).ToString("dd/MM/yyyy");
                    Worksheet.Cell("C2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    Worksheet.Cell("C2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    Worksheet.Cell("C2").Style.Alignment.WrapText = true;

                    Worksheet.Cell("E2").Value = DateTime.ParseExact(enddcover, "yyyyMMdd", null).ToString("dd/MM/yyyy");
                    Worksheet.Cell("E2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    Worksheet.Cell("E2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    Worksheet.Cell("E2").Style.Alignment.WrapText = true;


                    Worksheet.Range("A5:H" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A5:H" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A5:H" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A5:H" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Dữ liệu quẹt thẻ NT.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ListPartner'</script>";
                    return RedirectToAction("Index", "DetailEntrance");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
                return RedirectToAction("Index", "DetailEntrance");
            }

        }
    }
}