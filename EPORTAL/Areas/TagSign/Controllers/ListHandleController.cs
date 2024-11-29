using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ListHandleController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_NTEntities dbNT = new EPORTAL_NTEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListHandle";
        // GET: TagSign/ListHandle
        public ActionResult Index(int? page, string search, DateTime? begind, DateTime? endd, int? IDNT)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;

            var NhaThau = (from nt in db.NT_Partner
                           select new NT_PartnerValidation
                           {
                               ID = (int)nt.ID,
                               FullName = nt.CodeUnis + " : " + nt.FullName
                           }).ToList();
            if (IDNT == null) IDNT = 0;
            ViewBag.BPList = new SelectList(NhaThau, "ID", "FullName", IDNT);

            var res = (from a in dbNT.NT_CarteTemporaire_Select(search)
                       select new NT_ListCarteTemporaireValidation
                       {
                           IDTTT = (int)a.IDTTT,
                           NoiDung = a.NoiDung,
                           NguoiTaoID = (int)a.NguoiTaoID,
                           NgayTao = (DateTime?)a.NgayTao ?? default,
                           TinhTrang = (int?)a.TinhTrang ?? default,
                           NTID = (int)a.NTID,
                           PhongBanID = (int)a.PhongBanID,
                           HangMuc = a.HangMuc,
                           ThoiHan = (DateTime)a.ThoiHan,
                           GhiChu = a.GhiChu,
                           TenPhongBan = a.TenPhongBan,
                           TenNT = a.FullName,
                           HoTenNT =a.isNT ==1?"NT: "+ dbNT.NT_UserTemp.Where(x=>x.ID == a.NguoiTaoID).First().UserName: a.MaNV+":"+a.HoTen,
                       }).ToList();

            if (IDNT != 0)
            {
                res = res.Where(x => x.NTID == IDNT).ToList();
            }    
      
            if (begind != null && endd != null)
            {
                res = res.Where(x => x.NgayTao >= begind && x.NgayTao <= endd).ToList();
            }    

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult ExportToExcel( DateTime? begind, DateTime? endd, int? IDNT)
        {
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_Downlod_TheTam.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_Downlod_TheTam_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("TT");
                var List = dbNT.NT_CarteTemporaire.Where(x => x.NgayTao >= begind && x.NgayTao <= endd && x.NTID == IDNT).ToList();
                if (List.Count > 0)
                {
                    int row = 4,stt = 0;
                    foreach (var ls in List)
                    {
                        var Detail = dbNT.NT_DetailCarteTemporaire.Where(x => x.IDTTT == ls.IDTTT).ToList();
                        foreach (var item in Detail)
                        {
                            var IDTT_XL= dbNT.NT_Handle.Where(x=>x.IDCTTTT == item.IDCTTTT && x.IDThe != null).FirstOrDefault();
                            if(IDTT_XL != null)
                            {
                                row++; stt++;

                                Worksheet.Cell("A" + row).Value = stt;
                                Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                                Worksheet.Cell("B" + row).Value = item.HoVaTen;
                                Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                                Worksheet.Cell("C" + row).Value = "'" + item.CCCD.ToString();
                                Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                                Worksheet.Cell("D" + row).Value = item.NgaySinh;
                                Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("D" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                                Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                                var ID_NT = db.NT_Partner.Where(x => x.ID == ls.NTID).FirstOrDefault();
                                if (ID_NT != null)
                                {
                                    Worksheet.Cell("E" + row).Value = ID_NT.ShortName;
                                    Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;
                                }

                                var TD_PB = db.PhongBans.Where(x => x.IDPhongBan == ls.PhongBanID).FirstOrDefault();
                                if (TD_PB != null)
                                {

                                    Worksheet.Cell("F" + row).Value = TD_PB.TenPhongBan;
                                    Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;
                                }


                                Worksheet.Cell("G" + row).Value = ls.NgayTao;
                                Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("G" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                                Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                                Worksheet.Cell("H" + row).Value = "'" + item.Sdt;
                                Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                                Worksheet.Cell("I" + row).Value = item.BienSo;
                                Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                                var ID_C = db.NT_Gate.Where(x => x.IDGate == item.Cong).FirstOrDefault();
                                if (ID_C != null)
                                {
                                    Worksheet.Cell("J" + row).Value = ID_C.Gate;
                                    Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;
                                }
                                else
                                {
                                    Worksheet.Cell("J" + row).Value = "";
                                    Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Fill.BackgroundColor = XLColor.Red;
                                    Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;
                                }

                                var ID_The = db_nt.NT_CardTemp.Where(x => x.IDThe == IDTT_XL.IDThe).FirstOrDefault();
                                if (ID_The != null)
                                {
                                    Worksheet.Cell("K" + row).Value = ID_The.TenThe;
                                    Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;
                                }


                                Worksheet.Cell("L" + row).Value = item.GhiChu;
                                Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;
                            }    
                         
                        }
                    }
                    Worksheet.Range("A5:L" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A5:L" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A5:L" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A5:L" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Danh sách đăng ký thẻ tạm.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }   
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ListPartner'</script>";
                    return RedirectToAction("Index", "ListHandle");
                }    
                
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
                return RedirectToAction("Index", "ListHandle");
            }
            return RedirectToAction("Index", "ListHandle");
        }
    }
}