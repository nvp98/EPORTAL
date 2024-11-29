using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class CalendarController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Calendar";
        // GET: TagSign/Calendar
        public ActionResult Index(int? page, DateTime? begind, DateTime? endd)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var nd = db.AuthorizationContractors.Where(x => x.IDNhanVien == ModelsTagSign.MyAuthentication.ID).SingleOrDefault();

            var res = from a in db.Calenders
                      join pb in db.PhongBans on a.PhongBanID equals pb.IDPhongBan
                      select new CalendarValidation
                      {
                          IDPlan = (int)a.IDPlan,
                          IDCalendar = (int)a.IDCalendar,
                          LongName = a.LongName,
                          ShortName = a.ShortName,
                          PhongBanID = (int)a.PhongBanID,
                          TenPhongBan = pb.TenPhongBan,
                          Date = (DateTime)a.Date,
                          Image = a.Image,
                          Note = a.Note
                      };

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<PhongBan> pb = db.PhongBans.ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(CalendarValidation _DO)
        {
            try
            {
                var Date = (DateTime)_DO.Date;
                var dd = Date.ToString("dd");
                var mm = Date.ToString("MM");
                var yyyy = Date.ToString("yyyy");
                var IDCalendar = dd + mm + yyyy;
                var check = checkCalendar(Convert.ToInt32(IDCalendar), _DO.ShortName);
                if (check == false)
                {
                    TempData["msgError"] = "<script>alert('Lỗi. Kể hoạch đã tồn tại!');</script>";
                    return RedirectToAction("Index", "Calendar");
                }
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.ImageFile != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";

                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.Image = "~/Images/" + FileName;
                }
                var a = db.Calender_insert(Convert.ToInt32(IDCalendar), _DO.LongName, _DO.ShortName, _DO.PhongBanID, _DO.Date, _DO.Image, _DO.Note, 0);

                TempData["msgSuccess"] = "<script>alert('Thêm mới kế hoạch thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Calendar");
        }

        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.Calenders.Where(x => x.IDPlan == id)
                       join pb in db.PhongBans on a.PhongBanID equals pb.IDPhongBan
                       select new CalendarValidation
                       {
                           IDPlan = a.IDPlan,
                           IDCalendar = (int)a.IDCalendar,
                           LongName = a.LongName,
                           ShortName = a.ShortName,
                           PhongBanID = (int)a.PhongBanID,
                           TenPhongBan = pb.TenPhongBan,
                           Date = (DateTime)a.Date,
                           Image = a.Image,
                           Note = a.Note
                       }).ToList();
            CalendarValidation DO = new CalendarValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDPlan = a.IDPlan;
                    DO.IDCalendar = (int)a.IDCalendar;
                    DO.LongName = a.LongName;
                    DO.ShortName = a.ShortName;
                    DO.PhongBanID = (int)a.PhongBanID;
                    DO.Date = (DateTime)a.Date;
                    DO.Image = a.Image;
                    DO.Note = a.Note;
                }
                ViewBag.Date = DO.Date.ToString("yyyy-MM-dd");
                List<PhongBan> pb = db.PhongBans.ToList();
                ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.PhongBanID);
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        //[HttpPost]
        //public ActionResult Edit(CalendarValidation _DO)
        //{
        //    var Date = (DateTime)_DO.Date;
        //    var dd = Date.ToString("dd");
        //    var mm = Date.ToString("MM");
        //    var yyyy = Date.ToString("yyyy");
        //    var IDCalendar = dd + mm + yyyy;
        //    try
        //    {
        //        string path = Server.MapPath("~/Images/");
        //        //string path ="~/Images/";
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //        string FileName = _DO.ImageFile != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";
        //        //To Get File Extension  
        //        string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";

        //        if (_DO.ImageFile != null)
        //        {
        //            FileName = FileName.Trim() + FileExtension;
        //            _DO.ImageFile.SaveAs(path + FileName);
        //            _DO.Image = "~/Images/" + FileName;
        //        }
        //        var a = db.Calender_update(_DO.IDPlan, Convert.ToInt32(IDCalendar), _DO.LongName, _DO.ShortName, _DO.PhongBanID, _DO.Date, _DO.Image, _DO.Note);

        //        TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');window.location.assign("+_DO.url+")</script>";

        //    }
        //    catch (Exception e)
        //    {
        //        TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
        //    }
        //    //return View();
        //    return RedirectToAction("Index", "Calendar");
        //}
        //public ActionResult Delete(int? id)
        //{
        //    var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
        //    if (check == 0)
        //    {
        //        TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
        //        return RedirectToAction("Logout", "Login", new { area = "" });
        //    }
        //    try
        //    {
        //        db.Calender_delete(id);
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
        //    }
        //    return RedirectToAction("Index", "Calendar");
        //}
        [HttpPost]
        public JsonResult EditCLD(CalendarValidation _DO)
        {
            var Date = (DateTime)_DO.Date;
            var dd = Date.ToString("dd");
            var mm = Date.ToString("MM");
            var yyyy = Date.ToString("yyyy");
            var IDCalendar = dd + mm + yyyy;
            try
            {
                string path = Server.MapPath("~/Images/");
                //string path ="~/Images/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string FileName = _DO.ImageFile != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";
                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";

                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.Image = "~/Images/" + FileName;
                }
                var a = db.Calender_update(_DO.IDPlan, Convert.ToInt32(IDCalendar), _DO.LongName, _DO.ShortName, _DO.PhongBanID, _DO.Date, _DO.Image, _DO.Note);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
                return Json(false);
            }
            //return View();
            return Json(true);
        }
        [HttpPost]
        public JsonResult Delete(int? id)
        {

            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return Json(false);
            }
            try
            {
                db.Calender_delete(id);
                return Json(true);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
                return Json(false);
            }

        }


        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_KHB.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_KHB.xlsx");
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

                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            for (int i = 3; i < dt.Rows.Count; i++)
                            {

                                string ShortName = dt.Rows[i][1].ToString();
                                if (ShortName != "")
                                {
                                    string LongName = dt.Rows[i][2].ToString();
                                    string PhongBan = dt.Rows[i][3].ToString();
                                    var PB = db.PhongBans.Where(x => x.TenPhongBan == PhongBan).FirstOrDefault();
                                    string Date = dt.Rows[i][4].ToString();
                                    var Day = DateTime.ParseExact(Date, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    var dd = Day.ToString("dd");
                                    var mm = Day.ToString("MM");
                                    var yyyy = Day.ToString("yyyy");
                                    var IDCalendar = dd + mm + yyyy;
                                    string Note = dt.Rows[i][5].ToString();

                                    var a = db.Calender_insert(Convert.ToInt32(IDCalendar), LongName, ShortName, Convert.ToInt32(PB.IDPhongBan), Day, null, Note, 0);
                                    dtc++;
                                }

                            }

                            string msg = "";
                            if (dtc != 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu";
                            }
                            else { msg = "File import không có dữ liệu"; }

                            TempData["msgSuccess"] = "<script>alert('Import thành công: " + msg + " dòng dữ liệu');</script>";

                        }
                        catch (Exception ex)
                        {
                            TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import: " + ex.Message + "');</script>";
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

            return RedirectToAction("Index", "Calendar");
        }

        private List<CalendarValidation> GetCalender(DateTime? begind, DateTime? endd)
        {
            var res = (from a in db.Calenders
                       join pb in db.PhongBans on a.PhongBanID equals pb.IDPhongBan
                       select new CalendarValidation
                       {
                           IDPlan = (int)a.IDPlan,
                           IDCalendar = (int)a.IDCalendar,
                           LongName = a.LongName,
                           ShortName = a.ShortName,
                           PhongBanID = (int)a.PhongBanID,
                           TenPhongBan = pb.TenPhongBan,
                           Date = (DateTime)a.Date,
                           Image = a.Image,
                           Note = a.Note,
                           Status = (int)a.Status,
                       }).ToList();

            return res;
        }

        public ActionResult ExportToExcel(DateTime? begind, DateTime? endd)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EX).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_KHB.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_KHB_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("KHB");

                List<CalendarValidation> Data = GetCalender(begind, endd);
                if (Data.Count > 0)
                {
                    int row = 3, rowlast = 1, stt = 0;
                    foreach (var item in Data)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        Worksheet.Cell("B" + row).Value = item.ShortName;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.LongName;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.TenPhongBan;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.Date;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;
                        Worksheet.Cell("E" + row).Style.DateFormat.Format = "dd-MM-yyyy";

                        Worksheet.Cell("F" + row).Value = item.Note;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        if (item.Status == 0)
                        {
                            Worksheet.Cell("G" + row).Value = "Chưa hoàn thành";
                            Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;
                        }
                        else if (item.Status == 1)
                        {
                            Worksheet.Cell("G" + row).Value = "Đã hoàn thành";
                            Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;
                        }

                        row = rowlast - 1;
                    }
                    Worksheet.Range("A4:G" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A4:G" + (row)).Style.Font.SetFontSize(10);
                    //Worksheet.Range("A2:I" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Range("A2:I" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Danh sách lịch bay.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Không có dữ liệu danh sách lịch bay');</script>";
                    return RedirectToAction("Index", "Calendar");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
                return RedirectToAction("Index", "Calendar");
            }

        }

        public ActionResult Confirm(int? id, int? idtt)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                if (idtt == 1)
                {
                    db.Calender_Confirm(id, 1);
                }
                else
                {
                    db.Calender_Confirm(id, 0);
                }
                TempData["msgSuccess"] = "<script>alert('Xác nhận bay thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Calendar");
        }
        private bool checkCalendar(int IDCalendar, string ShortName)
        {
            var ct = db.Calenders.Where(x => x.IDCalendar == IDCalendar && x.ShortName == ShortName).FirstOrDefault();
            if (ct == null)
                return true;
            else
                return false;
        }
        [HttpPost]
        public JsonResult IsAlreadyCLD(string IDCalendar, string ShortName)
        {
            var slip = IDCalendar.Split('-');
            IDCalendar = slip[2] + slip[1] + slip[0];
            var id = Convert.ToInt32(IDCalendar);
            return Json(checkCalendar(id, ShortName));
        }
    }
}