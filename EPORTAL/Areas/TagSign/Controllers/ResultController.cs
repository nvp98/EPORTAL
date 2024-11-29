using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ResultController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Result";
        // GET: TagSign/Result
        public ActionResult Index(int? page, DateTime? begind, DateTime? endd)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var res = (from a in db_dk.DK_DetailCardRegistrationInfor
                      join cr in db_dk.DK_CardRegistrationInfor on a.IDDKT equals cr.IDDKT
                      select new DK_Detail_ListCardRegisInforNTValidation
                      {
                          IDCTDK = (int)a.IDCTDK,
                          HoTen = a.HoTen,
                          CCCD = a.CCCD,
                          NgaySinh = (DateTime)a.NgaySinh,
                          HoKhau = a.HoKhau,
                          ChucVu = (int)a.ChucVu,
                          SoDienThoai = a.SoDienThoai,
                          TheLuuTru = a.TheLuuTru,
                          TheRaVaoKLH = a.TheRaVaoKLH,
                          DienThoaiTM = a.DienThoaiTM,
                          RaVaoCang = a.RaVaoCang,
                          ThoiHanThe = (DateTime)a.ThoiHanThe,
                          KhuVucLamViec = a.KhuVucLamViec,
                          Cong = a.Cong,
                          NhomNT = (int)a.NhomNT,
                          GhiChu = a.GhiChu,
                          IDDKT = (int)a.IDDKT,
                          NTID = (int)cr.NTID,
                          TinhTrangID = (int)cr.TinhTrangID,
                          NgayDangKy = (DateTime)cr.NgayDangKy
                      }).ToList();
            if (begind != null && endd != null)
            {
                res = res.Where(x => x.NgayDangKy >= begind && x.NgayDangKy <= endd && x.TinhTrangID == 2 && x.GhiChu =="").ToList();
            }
            else
            {
                res = res.Where(x => x.NgayDangKy >= startDay && x.NgayDangKy <= endDay && x.TinhTrangID == 2 && x.GhiChu == "").ToList();
            }
            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        private List<DK_Detail_ListCardRegisInforNTValidation> CardRegistrationInfor(DateTime? begind, DateTime? endd)
        {

            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var res = (from a in db_dk.DK_DetailCardRegistrationInfor
                       join cr in db_dk.DK_CardRegistrationInfor on a.IDDKT equals cr.IDDKT
                       select new DK_Detail_ListCardRegisInforNTValidation
                       {
                           IDCTDK = (int)a.IDCTDK,
                           HoTen = a.HoTen,
                           CCCD = a.CCCD,
                           NgaySinh = (DateTime)a.NgaySinh,
                           HoKhau = a.HoKhau,
                           ChucVu = (int)a.ChucVu,
                           SoDienThoai = a.SoDienThoai,
                           TheLuuTru = a.TheLuuTru,
                           TheRaVaoKLH = a.TheRaVaoKLH,
                           DienThoaiTM = a.DienThoaiTM,
                           RaVaoCang = a.RaVaoCang,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           KhuVucLamViec = a.KhuVucLamViec,
                           Cong = a.Cong,
                           NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           IDDKT = (int)a.IDDKT,
                           NTID = (int)cr.NTID,
                           TinhTrangID = (int)cr.TinhTrangID,
                           NgayDangKy = (DateTime)cr.NgayDangKy
                       }).ToList();
            if (begind != null && endd != null)
            {
                res = res.Where(x => x.NgayDangKy >= begind && x.NgayDangKy <= endd && x.TinhTrangID == 2 && x.GhiChu == "").ToList();
            }
            else
            {
                res = res.Where(x => x.NgayDangKy >= startDay && x.NgayDangKy <= endDay && x.TinhTrangID == 2 && x.GhiChu == "").ToList();
            }

            return res;
        }
        public ActionResult ExportToExcel(int? IDTT, DateTime? begind, DateTime? endd)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\Import thong tin the NT.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\Import thong tin the NT_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("VP1C");
                List<DK_Detail_ListCardRegisInforNTValidation> Data = CardRegistrationInfor(begind, endd);
                if (Data.Count > 0)
                {
                    int row = 1, rowlast = 1, stt = 0;
                    foreach (var item in Data)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.CCCD;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.HoTen;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.HoKhau;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                        var IDNT = db.NT_Partner.Where(x => x.ID == item.NTID).FirstOrDefault();
                        if(IDNT.BPID != null)
                        {
                            Worksheet.Cell("E" + row).Value = IDNT.BPID;
                            Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;
                           
                        }
                        Worksheet.Cell("F" + row).Value = IDNT.ShortName;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("H" + row).Value = item.ThoiHanThe;
                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        if(item.DienThoaiTM != "")
                        {
                            Worksheet.Cell("K" + row).Value = "Y";
                            Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;
                        }    
                       else
                        {
                            Worksheet.Cell("K" + row).Value = "N";
                            Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;
                        }    

                        row = rowlast - 1;
                    }
                    Worksheet.Range("A2:K" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:K" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A2:K" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A2:K" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách nhân viên nhà thầu đã phê duyệt.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/Result'</script>";
                    return RedirectToAction("Index", "Result");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Result'</script>";
                return RedirectToAction("Index", "Result");
            }

        }
    }
}