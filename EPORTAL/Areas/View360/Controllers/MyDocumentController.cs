using EPORTAL.Models;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace EPORTAL.Areas.View360.Controllers
{
    public class MyDocumentController : Controller
    {
        PhanQuyenHTEntities db = new PhanQuyenHTEntities();
        EPORTALEntities dbE = new EPORTALEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "MyDocument";
        List<string> listQuyen = new List<string>();
        public MyDocumentController()
        {
           listQuyen = db.A_CheckListQuyen(IDQuyenHT, controll).ToList();
        }
        // GET: View360/MyDocument
        public ActionResult Index(int? page, string search)
        {
            //listQuyen = db.A_CheckListQuyen(IDQuyenHT, controll).ToList();
            //if (listQuyen.Contains(A_Constants.VIEW_ALL) == false)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            if (search == null) search = "";
            ViewBag.search = search;
            var res = from a in db.L_ThuVienFile_selectbyUser(search, MyAuthentication.ID)
                      select new L_ThuVienFileValidation
                      {
                          ID = a.ID,
                          TenTaiLieu = a.TenTaiLieu,
                          FileName = a.FileName,
                          GhiChu = a.GhiChu,
                          Createdate = a.Createdate,
                          TenNhomTV = a.TenNhomTV,
                          IDNhom = a.IDNhom ?? default,
                      };
            var listNhom = db.L_NhomThuVienFile.ToList();
            ViewBag.listNhom = listNhom;
            if (page == null) page = 1;
            int pageSize = 30;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.ID).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult ImportExcel()
        {
            //List<Project> nt = db.Projects.ToList();
            //ViewBag.ProjectID = new SelectList(nt, "ID", "Title");
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            //List<ProjectView> pro = db.Projects.Select(x => new ProjectView { ID = x.ID, Title = x.Title }).ToList();
            //ViewBag.Selected = new SelectList(pro, "ID", "Title");

            return PartialView();
        }
        [HttpPost]
        public ActionResult ImportExcel(AuthorizationUSERValidation _DO)
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
                    int IDNV = 0;

                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            for (int a = 0; a < _DO.Selected.Length; a++)
                            {
                                AuthorizationUSER aus = new AuthorizationUSER();
                                for (int i = 2; i < dt.Rows.Count; i++)
                                {
                                    //DataRow dr = dt.Rows[i];
                                    //IDNV = IDNhanVien(dr[1].ToString());
                                    //int IDProject = Convert.ToInt32(_DO.Selected[a]);
                                    //var NhanVien = db.AuthorizationUSERs.Where(x => x.ProjectID == IDProject && x.NhanVienID == IDNV).FirstOrDefault();
                                    //if (IDNV > 0 && CheckPer(IDNV, _DO.ProjectID) == 0 && NhanVien == null)
                                    //{
                                    //    aus.NhanVienID = IDNV;
                                    //    aus.ProjectID = Convert.ToInt32(_DO.Selected[a]);
                                    //    aus.Createdate = DateTime.Now;
                                    //    db.AuthorizationUSERs.Add(aus);
                                    //    db.SaveChanges();
                                    //    dtc++;
                                    //}
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

            return RedirectToAction("Index", "MyDocument");
        }
    }
}