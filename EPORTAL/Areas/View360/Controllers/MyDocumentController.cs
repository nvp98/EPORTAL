using EPORTAL.Common;
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
        // KHONG cache HTML output - dua vao Session cache SP `L_ThuVienFile_selectbyUser` (per-user inherent).
        // id = IDNhom dang chon (tab) - null = tat ca nhom.
        public ActionResult Index(int? page, string search, int? id)
        {
            if (search == null) search = "";
            ViewBag.search = search;

            // Cache SP `L_ThuVienFile_selectbyUser` + L_NhomThuVienFile per user trong Session 120s
            var sessKey = "v360_doc_list_" + MyAuthentication.ID + "_" + (search ?? "");
            var sess = System.Web.HttpContext.Current?.Session;
            var cached = sess != null ? sess[sessKey] as Tuple<DateTime, List<L_ThuVienFileValidation>, List<L_NhomThuVienFile>> : null;
            List<L_ThuVienFileValidation> docList;
            List<L_NhomThuVienFile> listNhom;
            if (cached != null && (DateTime.UtcNow - cached.Item1).TotalSeconds < 120)
            {
                docList = cached.Item2;
                listNhom = cached.Item3;
            }
            else
            {
                docList = db.L_ThuVienFile_selectbyUser(search, MyAuthentication.ID)
                    .Select(a => new L_ThuVienFileValidation
                    {
                        ID = a.ID,
                        TenTaiLieu = a.TenTaiLieu,
                        FileName = a.FileName,
                        GhiChu = a.GhiChu,
                        Createdate = a.Createdate,
                        TenNhomTV = a.TenNhomTV,
                        IDNhom = a.IDNhom ?? default,
                    })
                    .OrderByDescending(x => x.ID)
                    .ToList();
                listNhom = db.L_NhomThuVienFile.ToList();
                if (sess != null) sess[sessKey] = Tuple.Create(DateTime.UtcNow, docList, listNhom);
            }
            ViewBag.listNhom = listNhom;

            // Tabs theo nhom (chi nhom co tai lieu user xem duoc) - giong ListVirtual/ListProject
            var groupIdsWithDocs = new HashSet<int>(docList.Select(d => d.IDNhom));
            ViewBag.TabGroups = listNhom
                .Where(n => groupIdsWithDocs.Contains(n.IDNhom))
                .Select(n => new TabGroupViewModel
                {
                    IDGroup = n.IDNhom,
                    GroupName = n.TenNhomTV,
                    Active = id.HasValue && id.Value == n.IDNhom
                })
                .ToList();
            ViewBag.CurrentId = id;

            // Filter theo tab nhom dang chon
            if (id.HasValue)
                docList = docList.Where(d => d.IDNhom == id.Value).ToList();

            if (page == null) page = 1;
            int pageSize = 30;
            int pageNumber = (page ?? 1);
            return View(docList.ToPagedList(pageNumber, pageSize));
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
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcel(AuthorizationUSERValidation _DO)
        {
            HttpPostedFileBase excelFile = Request != null ? Request.Files["FileUpload"] : null;
            var excelError = FileUploadValidator.ValidateExcel(excelFile);
            if (excelError != null)
            {
                TempData["msgError"] = "<script>alert('" + excelError + "');</script>";
                return RedirectToAction("Index", "MyDocument");
            }

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
                    var safeName = FileUploadValidator.SafeFileName(file.FileName);
                    filePath = Path.Combine(path, safeName);

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