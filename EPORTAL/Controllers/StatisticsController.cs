using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Controllers
{
    public class StatisticsController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        // GET: Statistics
        public ActionResult Index(int? page)
        {
            var res = (from a in db.PhongBans
                       select new StatisticsValidation()
                       {
                           IDPhongBan = a.IDPhongBan,
                           TenPhongBan = a.TenPhongBan,

                       });

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = ((int)(page ?? 1));
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        private List<EmployeeValidation> GetEmployee(int? id)
        {
            var res = (from a in db.NhanViens.Where(x => x.IDPhongBan == id && x.IDTinhTrangLV == 1)
                       join pb in db.PhongBans on a.IDPhongBan equals pb.IDPhongBan
                       join vt in db.Vitris on a.IDViTri equals vt.IDViTri
                       select new EmployeeValidation()
                       {
                           ID = a.ID,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           IDPhongBan = (int)a.IDPhongBan,
                           TenPhongBan = pb.TenPhongBan,
                           IDViTri = (int)a.IDViTri,
                           TenViTri = vt.TenViTri,
                       }).ToList();
            return res;
        }
        private List<InformationValidation> GetInformation(int? id)
        {
            var res = (from a in db.ThongTinCaNhans.Where(x => x.NhanVien.IDPhongBan == id)
                       join nv in db.NhanViens on a.IDNhanVien equals nv.ID
                       select new InformationValidation()
                       {
                           IIDThongTin = a.IDThongTin,
                           IDNhanVien = (int)a.IDNhanVien,
                           MaNV = nv.MaNV,
                           HoTen = nv.HoTen,
                           TenPB = nv.PhongBan.TenPhongBan,
                           TenViTri = nv.Vitri.TenViTri
                       }).ToList();
            return res;
        }
        private List<EmployeeValidation> GetNotdeclared(int? id)
        {
            var res = (from a in db.NhanVien_DaDangKy(id)
                       join pb in db.PhongBans on a.IDPhongBan equals pb.IDPhongBan
                       join vt in db.Vitris on a.IDViTri equals vt.IDViTri
                       select new EmployeeValidation()
                       {
                           ID = a.ID,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           IDPhongBan = (int)a.IDPhongBan,
                           TenPhongBan = pb.TenPhongBan,
                           IDViTri = (int)a.IDViTri,
                           TenViTri = vt.TenViTri
                       }).ToList();
            return res;
        }
        public ActionResult ExportDeclared(int? id)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DaKhaiBao.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DaKhaiBao_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DKB");
                List<EmployeeValidation> Data = GetEmployee(id);
                int row = 3, stt = 0, icol = 1;
                if (Data.Count > 0)
                {
                    var TTCN = db.ThongTinCaNhans.Where(x => x.NhanVien.IDPhongBan == id).ToList();
                    foreach (var nv in Data)
                    {
                        foreach (var cbnv in TTCN)
                        {
                            if (nv.ID == cbnv.IDNhanVien)
                            {
                                row++; stt++; icol = 1;

                                Worksheet.Cell(row, icol).Value = stt;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                icol++;
                                Worksheet.Cell(row, icol).Value = nv.MaNV;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = nv.HoTen;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = nv.TenPhongBan;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = cbnv.CMND;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = cbnv.CCCD;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = cbnv.NgayKhai;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                                Worksheet.Cell(row, icol).Style.DateFormat.Format = "MM/dd/yyyy hh:mm:ss";

                            }

                        }
                    }

                    Worksheet.Range("A4:F" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A4:F" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A4:F" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A4:F" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    Worksheet.Cell("A2").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách đã khai báo - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {

                    Worksheet.Cell("A3").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách đã khai báo - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Everyday/'</script>";
                return View(TempData);
            }
        }
        public ActionResult ExportConfirmed(int? id)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DaXacNhan.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DaXacNhan_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DXN");
                List<InformationValidation> Data = GetInformation(id);
                int row = 3, stt = 0, icol = 1;
                if (Data.Count > 0)
                {
                    var TTCN = db.ThongTinCaNhans.Where(x => x.NhanVien.IDPhongBan == id && x.XacNhan == true).ToList();
                    foreach (var nv in Data)
                    {
                        foreach (var cbnv in TTCN)
                        {
                            if (nv.IDNhanVien == cbnv.IDNhanVien)
                            {
                                row++; stt++; icol = 1;

                                Worksheet.Cell(row, icol).Value = stt;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                icol++;
                                Worksheet.Cell(row, icol).Value = nv.MaNV;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = nv.HoTen;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = nv.TenPB;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                                icol++;
                                Worksheet.Cell(row, icol).Value = nv.TenViTri;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            }

                        }
                    }

                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A4:E" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A4:E" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    Worksheet.Cell("A2").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách đã khai báo - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {

                    Worksheet.Cell("A3").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách đã khai báo - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Everyday/'</script>";
                return View(TempData);
            }
        }

        public ActionResult ExportNotdeclared(int? id)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ChuaKhaiBao.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ChuaKhaiBao_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("CKB");
                List<EmployeeValidation> Data = GetNotdeclared(id);
                int row = 3, stt = 0, icol = 1;
                if (Data.Count > 0)
                {

                    foreach (var nv in Data)
                    {
                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;
                        Worksheet.Cell(row, icol).Value = nv.MaNV;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = nv.HoTen;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = nv.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = nv.TenViTri;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                    }




                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A4:E" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A4:E" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    Worksheet.Cell("A2").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách đã khai báo - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {

                    Worksheet.Cell("A3").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách đã khai báo - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Everyday/'</script>";
                return View(TempData);
            }
        }
        public ActionResult ExportNotConfirmed(int id)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ChuaXacNhan.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ChuaXacNhan_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("CXN");
                //List<InformationValidation> Data = GetInformation(id);
                int row = 3, stt = 0, icol = 1;
                
                var TTCN = db.ThongTinCaNhans.Where(x => x.NhanVien.IDPhongBan == id && x.XacNhan == false).ToList();
                if(TTCN.Count > 0)
                {
                    foreach (var cbnv in TTCN)
                    {

                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;
                        Worksheet.Cell(row, icol).Value = cbnv.NhanVien.MaNV;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = cbnv.NhanVien.HoTen;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = cbnv.NhanVien.PhongBan.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = cbnv.NhanVien.Vitri.TenViTri;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;




                    }
                    

                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A4:E" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A4:E" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    Worksheet.Cell("A2").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sach da khai bao chua xac nhan" + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {

                    Worksheet.Cell("A3").Value = "Ngày xuất file: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sach da khai bao chua xac nhan " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }


            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Everyday/'</script>";
                return View(TempData);
            }
        }
    }
}