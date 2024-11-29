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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class ListPartnerController : Controller
    {
        // GET: ListPartner/ListPartner

        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListPartner";
        //APIPartner
        string authInfo = "b.cntt" + ":" + "HPDQ@1234";
        string baseAddress = "http://bienban.hoaphatdungquat.vn/";

        public ActionResult Index(int? page, string search)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;

            var res = from a in db.NT_Partner_select(search)
                      select new NT_PartnerValidation
                      {
                          ID = (int)a.ID,
                          BPID = (int)a.BPID,
                          FullName = a.FullName,
                          Taxcode = a.Taxcode,
                          Address = a.Address,
                          Customer = a.Customer,
                          Vendor = a.Vendor,
                          Email = a.Email,
                          ShortName = a.ShortName,
                          CodeUnis = a.CodeUnis
                      };
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.BPID).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
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
        public ActionResult Create(NT_PartnerValidation _DO)
        {

            try
            {

                if (Request != null)
                {
                    //Check trùng mã MaNV
                    if (!checkBPID(_DO.BPID) == false)
                    {
                        
                        // API
                        using (var client = new HttpClient())
                        {
                            //string authInfo = "andy" + ":" + "password";
                            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.BaseAddress = new Uri(baseAddress);
                            //HTTP POST
                            HttpResponseMessage result =  client.PostAsJsonAsync("api/PartnerAPI", _DO).Result;
                            result.EnsureSuccessStatusCode();
                            
                            
                        }
                        //

                        var a = db.NT_Partner_insert(_DO.BPID, _DO.FullName, _DO.Taxcode, _DO.Address, _DO.Customer, _DO.Vendor, _DO.Email, _DO.ShortName, _DO.CodeUnis);
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Mã Nhân Viên đã tồn tại');</script>";
                    }

                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ListPartner");
        }

        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_Partner.Where(x => x.ID == id)
                       select new NT_PartnerValidation
                       {
                           BPID = (int)a.BPID,
                           FullName = a.FullName,
                           Taxcode = a.Taxcode,
                           Address = a.Address,
                           Customer = a.Customer,
                           Vendor = a.Vendor,
                           Email = a.Email,
                           ShortName = a.ShortName,
                           CodeUnis = a.CodeUnis
                       }).ToList();
            NT_PartnerValidation DO = new NT_PartnerValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.BPID = (int)a.BPID;
                    DO.FullName = a.FullName;
                    DO.Taxcode = a.Taxcode;
                    DO.Address = a.Address;
                    DO.Customer = a.Customer;
                    DO.Vendor = a.Vendor;
                    DO.Email = a.Email;
                    DO.ShortName = a.ShortName;
                    DO.CodeUnis = a.CodeUnis;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(NT_PartnerValidation _DO)
        {

            try
            {

                if (Request != null)
                {
                    //
                    var a = db.NT_Partner_update(_DO.ID, _DO.BPID, _DO.FullName, _DO.Taxcode, _DO.Address, _DO.Customer, _DO.Vendor, _DO.Email, _DO.ShortName, _DO.CodeUnis);
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ListPartner");
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
                db.NT_Partner_delete(id);
                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListPartner");
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
                            for (int i = 1; i < dt.Rows.Count; i++)
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
                                else
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

                                    var Additional = db.NT_Partner.Where(x => x.BPID == ct.BPID).FirstOrDefault();
                                    var a = db.NT_Partner_Additional(Convert.ToInt32(Additional.ID), ct.ShortName, ct.CodeUnis);
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

            return RedirectToAction("Index", "ListPartner");
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
        [HttpPost]
        public JsonResult IsAlreadyBP(int BPID)
        {
            return Json(checkBPID(BPID));
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

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNT.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNT_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DSNT");

                IList<NT_Partner> listNT_Partner = db.NT_Partner.ToList();
                if (listNT_Partner.Count > 0)
                {
                    int row = 1, rowlast = 1, stt = 0;
                    foreach (var item in listNT_Partner)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = item.BPID;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.Customer;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.Vendor;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.FullName;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.ShortName;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;



                        Worksheet.Cell("F" + row).Value = item.CodeUnis;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("G" + row).Value = item.Taxcode;
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("H" + row).Value = item.Address;
                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("I" + row).Value = item.Email;
                        Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;




                        row = rowlast - 1;
                    }
                    Worksheet.Range("A2:I" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:I" + (row)).Style.Font.SetFontSize(10);
                    //Worksheet.Range("A2:I" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Range("A2:I" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "DSNT.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ListPartner'</script>";
                    return RedirectToAction("Index", "ListPartner");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
                return RedirectToAction("Index", "ListPartner");
            }

        }
        public string GetIp()
        {
            string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
        
    }
}