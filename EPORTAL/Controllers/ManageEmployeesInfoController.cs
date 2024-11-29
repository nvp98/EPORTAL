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
    public class ManageEmployeesInfoController : Controller
    {
        // GET: ManageEmployeesInfo
        EPORTALEntities db = new EPORTALEntities();



        private List<SelectListItem> getYNOptions()
        {
            List<SelectListItem> yn = new List<SelectListItem>();
            yn.Add(new SelectListItem() { Text = "True", Value = true.ToString(), Selected = true });
            yn.Add(new SelectListItem() { Text = "False", Value = false.ToString(), Selected = true });
            return yn;
        }

        //GET: ManageEmployeesInfo
        public ActionResult Index(int? page, string search, int? IDPhongBan)
        {
            if (search == null) search = "";
            ViewBag.search = search;
            if (IDPhongBan == null) IDPhongBan = 0;
            List<PhongBan> pb = db.PhongBans.ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan", IDPhongBan);
            var res = (from a in db.ThongTinCaNhan_selectNV(search)
                      select new EmployeesInfo
                      {
                          IDThongTin = a.IDThongTin,
                          IDNhanVien = (int)a.IDNhanVien,
                          MaNV = a.MaNV,
                          HoTen = a.HoTen,
                          IDPhongBan =(int)a.IDPhongBan,
                          PhongBan = a.TenPhongBan,
                          ViTri = a.TenViTri,
                          CMND = a.CMND,
                          CCCD = a.CCCD,
                          FileHinh = a.FileHinh,
                          XacNhan = (bool)a.XacNhan,
                      }).ToList();
            if(IDPhongBan!=0)
                res=res.Where(x=>x.IDPhongBan==IDPhongBan).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }


        List<EmployeesInfo> GetlistEmployeesInfo(int? IDPhongBan, string search)
        {
            if (IDPhongBan == null) IDPhongBan = 0;
            var res = (from a in db.ThongTinCaNhan_selectNV(search)
                       select new EmployeesInfo
                       {
                           IDThongTin = a.IDThongTin,
                           IDNhanVien = (int)a.IDNhanVien,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           IDPhongBan = (int)a.IDPhongBan,
                           PhongBan = a.TenPhongBan,
                           ViTri = a.TenViTri,
                           CMND = a.CMND,
                           CCCD = a.CCCD,
                           FileHinh = a.FileHinh,
                           XacNhan = (bool)a.XacNhan,
                       }).ToList();
            if (IDPhongBan != 0)
                res = res.Where(x => x.IDPhongBan == IDPhongBan).ToList();
            return res;
        }
        public ActionResult Upload(int? id)
        {
            try
            {
                db.ThongTinCaNhan_upload(id, false);

                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageEmployeesInfo", new { Areas = "" });
        }
        public ActionResult ExportToExcel(int? IDPhongBan, string search)
        {
            if (IDPhongBan == null) IDPhongBan = 0;
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_XDSTT.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_XDSTT_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DSHT");

                IList<EmployeesInfo> listEmployeesInfo = GetlistEmployeesInfo(IDPhongBan,search);
                if (listEmployeesInfo.Count > 0)
                {



                    int row = 8, rowlast = 8, stt = 0;
                    foreach (var item in listEmployeesInfo)
                    {




                        Worksheet.Cell("E5").Value = item.PhongBan;
                        Worksheet.Cell("E5" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E5" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E5" + row).Style.Alignment.WrapText = true;



                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.MaNV;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.HoTen;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.ViTri;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.PhongBan;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;



                        Worksheet.Cell("F" + row).Value = item.CMND;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("G" + row).Value = item.CCCD;
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;


                        if (item.XacNhan == false)
                            Worksheet.Cell("H" + row).Value = "Chưa Xác Nhận";
                        else
                            Worksheet.Cell("H" + row).Value = "Đã Xác Nhận";

                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        row = rowlast - 1;
                    }
                    Worksheet.Range("A9:H" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A9:H" + (row)).Style.Font.SetFontSize(11);
                    Worksheet.Range("A9:H" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A9:H" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "DSHT.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ManageEmployeesInfo'</script>";
                    return RedirectToAction("Index", "ManageEmployeesInfo", new { id = IDPhongBan });
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ManageEmployeesInfo'</script>";
                return RedirectToAction("Index", "ManageEmployeesInfo", new { id = IDPhongBan });
            }

        }
        List<EmployeesInfo> Getlistchuadangky(int? IDPhongBan)
        {
            if(IDPhongBan == null) IDPhongBan = 0;
            var res = (from a in db.NhanVien_KhaiBao_ChuaKhaiBao(IDPhongBan)
                       select new EmployeesInfo
                       {
                          
                           IDNhanVien = (int)a.ID,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           IDPhongBan = (int)a.IDPhongBan,
                           PhongBan = a.TenPhongBan,
                           ViTri = a.TenViTri,
                       }).ToList();
            return res;
        }
        public ActionResult ExportnotDeclaration(int? id)
        {
            if (id == null) id = 0;
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ChuaKhaiBao.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ChuaKhaiBao_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("CKB");

                IList<EmployeesInfo> listEmployeesInfo = Getlistchuadangky(id);
                if (listEmployeesInfo.Count > 0)
                {
                    int row = 3,  stt = 0, icol=1;
                    foreach (var item in listEmployeesInfo)
                    {
                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.MaNV;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.HoTen;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.PhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.ViTri;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                    }
                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A4:E" + (row)).Style.Font.SetFontSize(11);
                    Worksheet.Range("A4:E" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A4:E" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "DS_ChuaKhaiBao.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ManageEmployeesInfo'</script>";
                    return RedirectToAction("Index", "ManageEmployeesInfo", new { id = id });
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ManageEmployeesInfo'</script>";
                return RedirectToAction("Index", "ManageEmployeesInfo", new { id = id });
            }
        }
    }
}