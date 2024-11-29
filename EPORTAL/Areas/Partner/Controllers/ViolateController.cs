using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class ViolateController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Violate";
        string connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ToString();
        // GET: Partner/Violate
        public ActionResult Index(int? page, string search)
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

            var nvvp = db_nt.NT_NhanVienVP_Select(search);
            var nv = db.NhanViens.Where(x=>x.IDTinhTrangLV == 1);
            var res = (from vp in nvvp
                       join n in nv on vp.CreateIDNV equals n.ID
                       select new ViolateValidation
                       {
                           IDNVVP = vp.IDNVVP,
                           HoTen = vp.HoTen,
                           CCCD = vp.CCCD,
                           CMND =vp.CMND,
                           NgaySinh = (DateTime?)vp.NgaySinh??default,
                           LyDoCam = vp.LyDoCam,
                           GhiChu = vp.GhiChu,
                           CreateDay = (DateTime?)vp.CreateDay ?? default,
                           CreateIDNV = vp.CreateIDNV,
                           NVCreate = n.MaNV+"-"+ n.HoTen,
                           TinhTrang =vp.TinhTrang ==1?"Mở khóa":"Khóa",
                           IDTinhTrang =vp.TinhTrang
                       }).ToList();

            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BMDanhSachViPham.xlsx";
            return File(path, "application/vnd.ms-excel", "BMDanhSachViPham.xlsx");
        }
        public ActionResult ImportExcel()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return PartialView();
        }
        [HttpPost]
        public ActionResult ImportExcels()
        {
            string filePath = string.Empty;
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["FileUpload"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string path = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    filePath = path + Path.GetFileName(file.FileName);

                    file.SaveAs(filePath);
                    Stream stream = file.InputStream;

                    IExcelDataReader reader = null;
                    if (file.FileName.ToLower().EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (file.FileName.ToLower().EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        TempData["msg"] = "<script>alert('Vui lòng chọn đúng định dạng file Excel');</script>";
                        return View();
                    }
                    DataSet result = reader.AsDataSet();
                    DataTable dt = result.Tables[0];
                    reader.Close();
                    int dtc = 0;
                    int kk = 0;
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            EPORTALEntities db = new EPORTALEntities();
                            for (int i = 1; i < dt.Rows.Count; i++)
                            {
                                string HoTen = dt.Rows[i][1].ToString();
                                string CCCD = dt.Rows[i][2].ToString().Trim();
                                string CMND = dt.Rows[i][3].ToString().Trim();
                                string NgaySinh = dt.Rows[i][4].ToString();
                                string lyDoCam = dt.Rows[i][5].ToString();
                                string GhiChu = dt.Rows[i][6].ToString();
                                //var aa = db_nt.NT_NhanVienVP.Where(x =>x.HoTen ==HoTen && x.CCCD == CCCD && x.CMND == CMND).ToList();
                                if (NgaySinh == "" && HoTen != "")
                                {
                                    var a = db_nt.NT_NhanVienVP_insert(HoTen, CCCD, CMND, null, lyDoCam, GhiChu, DateTime.Now, MyAuthentication.ID, 0);
                                    dtc++;
                                }
                                else if (HoTen != "")
                                {
                                    DateTime DateDay = DateTime.ParseExact(NgaySinh, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    var a = db_nt.NT_NhanVienVP_insert(HoTen, CCCD, CMND, DateDay, lyDoCam, GhiChu, DateTime.Now, MyAuthentication.ID, 0);
                                    dtc++;
                                }
                                kk = i;
                            }
                            string msg = "";
                            if (dtc != 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu";
                            }
                            else { msg = "File import không có dữ liệu"; }

                            TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";
                        }
                        catch (Exception ex)
                        {
                            TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import: " + kk + ex.Message + "');</script>";
                        }
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                    }

                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
                }
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
            }

            return RedirectToAction("Index", "Violate");
        }

        public ActionResult ExportToExcel()
        {
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSVP.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSVP_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("VP");
                var List = db_nt.NT_NhanVienVP.ToList();
                if (List.Count > 0)
                {
                    int row = 5, rowlast = 1, stt = 0;
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

                        Worksheet.Cell("D" + row).Value = item.NgaySinh;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.LyDoCam;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;



                        Worksheet.Cell("F" + row).Value = item.GhiChu;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;
                        row = rowlast - 1;
                    }
                    Worksheet.Range("A7:F" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:F" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A7:F" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:F" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Danh sách nhân viên vi phạm.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ListPartner'</script>";
                    return RedirectToAction("Index", "Violate");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
                return RedirectToAction("Index", "Violate");
            }

        }

        public ActionResult Delete(int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db_nt.NT_NhanVienVP_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Violate");
        }

        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from vp in db_nt.NT_NhanVienVP.Where(x => x.IDNVVP == id)
                       select new ViolateValidation
                       {
                           IDNVVP = vp.IDNVVP,
                           HoTen = vp.HoTen,
                           CCCD = vp.CCCD,
                           NgaySinh = (DateTime?)vp.NgaySinh ?? default,
                           LyDoCam = vp.LyDoCam,
                           GhiChu = vp.GhiChu
                       }).ToList();
            ViolateValidation DO = new ViolateValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDNVVP = (int)a.IDNVVP;
                    DO.HoTen = a.HoTen;
                    DO.CCCD = a.CCCD;
                    DO.NgaySinh = a.NgaySinh;
                    DO.LyDoCam = a.LyDoCam;
                    DO.GhiChu = a.GhiChu;
                }
            }
            else
            {
                HttpNotFound();
            }
            ViewBag.NgaySinh = DO.NgaySinh.ToString("yyyy-MM-dd");
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(ViolateValidation _DO)
        {

            try
            {
                var a = db_nt.NT_NhanVienVP_update(_DO.IDNVVP, _DO.HoTen, _DO.CCCD, _DO.NgaySinh, _DO.LyDoCam, _DO.GhiChu);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Violate");
        }

        public ActionResult ResetViolate(int? IDNVVP, int? TinhTrang)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {

                var a = db_nt.NT_NhanVienVP_updateLock(IDNVVP, TinhTrang);
                var aa = db_nt.NT_HistoryBlackList_insert(IDNVVP, DateTime.Now, MyAuthentication.ID, TinhTrang);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Reset dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Violate");
        }
    }
}