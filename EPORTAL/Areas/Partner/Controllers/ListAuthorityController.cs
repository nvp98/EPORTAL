using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
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

namespace EPORTAL.Areas.Partner.Controllers
{
    public class ListAuthorityController : Controller
    {
        // GET: ListAuthority/NT_Authority

        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListAuthority";
        public ActionResult Index(int? page, string search, int? IDNT)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;

            List<NT_Partner> nt = db.NT_Partner.ToList();
            if (IDNT == null) IDNT = 0;
            ViewBag.NTList = new SelectList(nt, "BPID", "FullName", IDNT);

            var res = (from a in db.NT_Authority_select(search)
                      select new NT_AuthorityValidation
                      {
                          ID = (int)a.ID,
                          BPID = a.BPID,
                          FullName = a.FullName,
                          CCCD = a.CCCD,
                          Email = a.Email,
                          Phone = a.Phone,
                          Position = a.Position,
                          Createdate = (DateTime)a.Createdate,
                          Begindate = a.Begindate,
                          Enddate = a.Enddate,
                          File_Scan = a.File_Scan,
                      }).ToList();
            if(IDNT!=0)
            {
                res = res.Where(x=>x.BPID == IDNT).ToList();
            }
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.ID).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NT_Partner> bp = db.NT_Partner.ToList();
            int index = 0;
            foreach (var n in bp)
            {
                bp[index].FullName = n.BPID.ToString() + " - " + n.FullName;
                index++;
            }
            ViewBag.BPList = new SelectList(bp, "BPID", "FullName");
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(NT_AuthorityValidation _DO)
        {

            try
            {

                string path = Server.MapPath("~/UploadedFiles/Authority/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string Name = _DO.FullName.ToUpper();
                //Use Namespace called :  System.IO  
                string FileName = _DO.File != null ? _DO.CCCD + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                //To Get File Extension  
                string FileExtension = _DO.File != null ? Path.GetExtension(_DO.File.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.File != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.File.SaveAs(path + FileName);
                    _DO.File_Scan = "~/UploadedFiles/Authority/" + FileName;
                }

                var a = db.NT_Authority_insert(_DO.FullName, _DO.CCCD, _DO.Email, _DO.Phone, _DO.Position, DateTime.Now, _DO.Begindate, _DO.Enddate, _DO.File_Scan, _DO.BPID);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ListAuthority");
        }

        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_Authority.Where(x => x.ID == id)
                       select new NT_AuthorityValidation
                       {
                           ID = (int)a.ID,
                           BPID = (int)a.BPID,
                           FullName = a.FullName,
                           CCCD = a.CCCD,
                           Email = a.Email,
                           Phone = a.Phone,
                           Position = a.Position,
                           Createdate = (DateTime)a.Createdate,
                           Begindate = a.Begindate,
                           Enddate = a.Enddate,
                           File_Scan = a.File_Scan,
                       }).ToList();
            NT_AuthorityValidation DO = new NT_AuthorityValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = (int)a.ID;
                    DO.BPID = (int)a.BPID;
                    DO.FullName = a.FullName;
                    DO.CCCD = a.CCCD;
                    DO.Email = a.Email;
                    DO.Phone = a.Phone;
                    DO.Position = a.Position;
                    DO.Email = a.Email;
                    DO.Createdate = a.Createdate;
                    DO.Begindate = a.Begindate;
                    DO.Enddate = a.Enddate;
                    DO.File_Scan = a.File_Scan;
                }
            }
            else
            {
                HttpNotFound();
            }
            List<NT_Partner> bp = db.NT_Partner.ToList();
            int index = 0;
            foreach (var n in bp)
            {
                bp[index].FullName = n.BPID.ToString() + " - " + n.FullName;
                index++;
            }
            ViewBag.BPList = new SelectList(bp, "BPID", "FullName");
            ViewBag.Begindate = DO.Begindate.HasValue == true ? DO.Begindate.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.Enddate = DO.Enddate.HasValue == true ? DO.Enddate.Value.ToString("yyyy-MM-dd") : "";
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(NT_AuthorityValidation _DO)
        {

            try
            {

                if (Request != null)
                {
                    //Upload file pdf
                    string filePath = string.Empty;
                    string path = Server.MapPath("~/UploadedFiles/Authority/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string Name = _DO.FullName.ToUpper();
                    //Use Namespace called :  System.IO  
                    string FileName = _DO.File != null ? _DO.CCCD + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    //To Get File Extension  
                    string FileExtension = _DO.File != null ? Path.GetExtension(_DO.File.FileName) : "";


                    ////Add Current Date To Attached File Name  
                    if (_DO.File != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.File.SaveAs(path + FileName);
                        _DO.File_Scan = "~/UploadedFiles/Authority/" + FileName;
                    }
                    var a = db.NT_Authority_update(_DO.FullName, _DO.CCCD, _DO.Email, _DO.Phone, _DO.Position, DateTime.Now, _DO.Begindate, _DO.Enddate, _DO.File_Scan, _DO.BPID, _DO.ID);
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ListAuthority");
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
                db.NT_Authority_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListAuthority");
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_DSNT.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_DSNT.xlsx");
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

                            NT_Partner ct = new NT_Partner();
                            for (int i = 2; i < dt.Rows.Count; i++)
                            {
                                if (checkBPID(Convert.ToInt32(dt.Rows[i][0])) == true)
                                {
                                    ct = new NT_Partner();
                                    ct.BPID = Convert.ToInt32(dt.Rows[i][0]);
                                    ct.Customer = dt.Rows[i][1].ToString();
                                    ct.Vendor = dt.Rows[i][2].ToString();
                                    ct.FullName = dt.Rows[i][3].ToString();
                                    ct.ShortName = dt.Rows[i][4].ToString();
                                    ct.CodeUnis = dt.Rows[i][5].ToString();
                                    ct.Taxcode = dt.Rows[i][6].ToString();
                                    ct.Address = dt.Rows[i][7].ToString();
                                    ct.Email = dt.Rows[i][8].ToString();
                                    var a = db.NT_Partner_insert(ct.BPID, ct.FullName, ct.Taxcode, ct.Address, ct.Customer, ct.Vendor, ct.Email, ct.ShortName, ct.CodeUnis);
                                    //db.NT_Partners.Add(ct);
                                    //db.SaveChanges();
                                    ct = null;
                                    dtc++;

                                }
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

            return RedirectToAction("Index", "ListAuthority");
        }
        private bool checkBPID(int bPID)
        {
            var ct = (from u in db.NT_Partner.Where(x => x.BPID == bPID)
                      select new { u.BPID }).FirstOrDefault();
            if (ct == null)
                return true;
            else
                return false;
        }
        public ActionResult ExportToExcel()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EX).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_Authority.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_Authority_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("Authority");

                IList<NT_Authority> listAuthority = db.NT_Authority.ToList();
                if (listAuthority.Count > 0)
                {
                    int row = 1, rowlast = 1, stt = 0;
                    foreach (var item in listAuthority)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = item.FullName;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.Position;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.CCCD;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.Email;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.Phone;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        var nt = db.NT_Partner.Where(x => x.BPID == item.BPID).FirstOrDefault();

                        Worksheet.Cell("F" + row).Value = nt.FullName;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("G" + row).Value = item.Begindate.Value.ToString("dd-MM-yyyy");
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("H" + row).Value = item.Enddate.Value.ToString("dd-MM-yyyy");
                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        row = rowlast - 1;
                    }
                    Worksheet.Range("A2:H" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:H" + (row)).Style.Font.SetFontSize(10);
                    //Worksheet.Range("A2:I" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Range("A2:I" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Authority.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ListAuthority'</script>";
                    return RedirectToAction("Index", "ListAuthority");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListAuthority'</script>";
                return RedirectToAction("Index", "ListAuthority");
            }

        }
    }
}