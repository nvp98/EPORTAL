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
    public class LockController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Lock";
        // GET: Partner/Lock
        string connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ToString();
        public ActionResult Index(int? page, string search, int? IDNT)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");

            DateTime DayNow = DateTime.Now;
            if (search == null) search = "";
            ViewBag.search = search;

            //var SLNV = db_nt.NT_NhanVienNT.Where(x => x.TTLV == 1).ToList();
            var res = (from nv in db_nt.NT_NhanVienNT_Select(search).Where(x=>x.TTLV != 1 || x.HanSuDung <= DayNow)
                       select new ContractorStaffValidation
                       {
                           IDNVNT = nv.IDNVNT,
                           IDNT = (int)nv.IDNT,
                           HoTen = nv.HoTen,
                           CCCD = nv.CCCD,
                           DiaChi = nv.DiaChi,
                           Sdt = (int?)nv.Sdt ?? default,
                           SoThe = nv.SoThe,
                           NgayCap = (DateTime?)nv.NgayCap ?? default,
                           HanSuDung = (DateTime?)nv.HanSuDung ?? default,
                           TTLV = (int)nv.TTLV

                       }).ToList();
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x=>x.HanSuDung).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Lock(int? id, int? idlock)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                if (idlock == 1)
                {
                    db_nt.NT_NhanVienNT_lock(id, 0);
                }
                else
                {
                    db_nt.NT_NhanVienNT_lock(id, 1);
                }
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Lock");
        }
        public ActionResult ExportToExcel()
        {
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNVNT.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNVNT_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("NV");
                var List = db_nt.NT_NhanVienNT.Where(x => x.HanSuDung < DateTime.Now).ToList();
                if (List.Count > 0)
                {
                    int row = 6, rowlast = 1, stt = 0;
                    foreach (var item in List)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.HoTen;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.CCCD;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.DiaChi;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.Sdt;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;



                        Worksheet.Cell("F" + row).Value = item.SoThe;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        var NhaThau = db.NT_Partner.Where(x => x.ID == item.IDNT).FirstOrDefault();
                        var Unis = NhaThau.CodeUnis.ToString();
                        Worksheet.Cell("G" + row).Value = Unis + "-" + (NhaThau.FullName);
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("H" + row).Value = item.NgayCap;
                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("I" + row).Value = item.HanSuDung;
                        Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("I" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                        var TTHD = db_nt.NT_TTHD.Where(x => x.IDHD == item.TTLV).FirstOrDefault();
                        Worksheet.Cell("J" + row).Value = TTHD.TenHD;
                        Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;
                        row = rowlast - 1;
                    }
                    Worksheet.Range("A7:J" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:J" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A7:J" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:J" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Danh sách NVNT cần khóa thẻ.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ListPartner'</script>";
                    return RedirectToAction("Index", "ContractorStaff");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
                return RedirectToAction("Index", "Lock");
            }

        }
    }
}