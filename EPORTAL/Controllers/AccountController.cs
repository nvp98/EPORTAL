using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.Models;
using PagedList;
using EPORTAL.Common;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text.RegularExpressions;
using System.Data;
using ExcelDataReader;
using System.IO;

namespace EPORTAL.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Account";
        public ActionResult Index(int? page, string search)
        {           
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login");
            }
            if (search == null) search = "";
            ViewBag.search = search;

            var res = from a in dbP.Account_select(search)
                      select new EmployeeValidation
                      {
                          ID = a.ID,
                          HoTen = a.HoTen,
                          MaNV = a.MaNV,
                          PhongBan = a.TenPhongBan,
                          TenQuyen = a.TenQuyenHT,
                          Email=a.Email,
                      };

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));

        }

        public ActionResult ResetListPass()
        {
            //var ListQuyen = new HomeController().GetPermisionCN(Idquyen, ControllerName);
            //if (!ListQuyen.Contains(CONSTKEY.R_PASS))
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("", "Home");
            //}
            db.Configuration.ProxyCreationEnabled = false;
            return PartialView();
        }
        public ActionResult DownloadBM()
        {
            string path = "/App_Data/BM_cap_nhat_email.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_cap_nhat_email.xlsx");
        }
        [HttpPost]
        public ActionResult upLoadfile(HttpPostedFileBase file)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            int dtc = 1;
            string filePath = string.Empty;
            file = Request.Files["FileExcel"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string path = Server.MapPath("~/UploadedFiles/DKHocATEX/");
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
                    return RedirectToAction("Index", "Account");
                }
                DataSet result = reader.AsDataSet();
                DataTable dt = result.Tables[0];
                reader.Close();

                if (dt.Rows.Count > 1)
                {

                    try
                    {
                        for (int i = 1; i < dt.Rows.Count; i++)
                        {
                            string MaNVEx = dt.Rows[i][1].ToString().Trim();                                                     
                            var NV = db.NhanViens.Where(x => x.MaNV == MaNVEx).SingleOrDefault();
                            if (MaNVEx != "")
                            {
                                if (NV == null)
                                {                                   
                                    TempData["msgError"] = "<script>alert('Mã nhân dòng: " + dtc + " không tồn tại, hãy kiểm tra và import lại từ dòng " + dtc + "');</script>";
                                    return RedirectToAction("Index", "Account");
                                }
                            }
                            else
                            {
                                TempData["msgError"] = "<script>alert('Mã nhân viên dòng: " + dtc + " không được để trống, hãy cập nhập và import lại từ dòng " + dtc + "');</script>";
                                return RedirectToAction("Index", "Account");
                            }
                            string email = dt.Rows[i][3].ToString().Trim();
                            if (email == "")
                            {
                                TempData["msgError"] = "<script>alert('Email dòng: " + dtc + " không được để trống,hãy cập nhập và import lại từ dòng " + dtc + "');</script>";
                                return RedirectToAction("Index", "Account");
                            }
                            else
                            {
                                var emailCheck=db.NhanViens.Where(x=>x.Email == email).SingleOrDefault();
                                if(emailCheck != null)
                                {
                                    TempData["msgError"] = "<script>alert('Email tại dòng: " + dtc + ", đã tồn tại, hãy cập nhập và import lại từ dòng " + dtc + "');</script>";
                                    return RedirectToAction("Index", "Account");
                                }
                            }
                            db.NhanVienEmail_Update(MaNVEx, email);
                            dtc++;
                        }
                    }
                    catch (Exception ex)
                    {                        
                        TempData["msgError"] = "<script>alert('Vui lòng kiểm tra lại dòng : " + dtc + ",hãy cập nhập và import lại từ dòng: " + dtc + "');</script>";
                        return RedirectToAction("Index", "Account");
                    }
                }
                else
                {
                    TempData["msgError"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                    return RedirectToAction("Index", "Account");
                }

            }
            else
            {
                TempData["msgError"] = "<script>alert('Vui lòng chọn File Excel');</script>";
                return RedirectToAction("Index", "Account");
            }
            TempData["msgSuccess"] = "<script>alert('Cập nhật email thành công');</script>";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult ResetListPass(LoginValidation _DO)
        {
            try
            {
                if (!String.IsNullOrEmpty(_DO.MaNV))
                {
                    //Regex.Replace(_DO.NVDG, @"[^0-9a-zA-Z]+", " ");
                    string tx = Regex.Replace(_DO.MaNV, @"[^0-9a-zA-Z]+", " ");
                    string[] NVS = tx.Split(new char[] { ' ' });
                    foreach (var item in NVS)
                    {
                        var aa = db.NhanViens.Where(x => x.MaNV == item).FirstOrDefault();
                        if (aa != null)
                        {
                            db.TaiKhoan_Update(aa.ID, Encryptor.MD5Hash(_DO.MatKhau));
                        }
                    }
                }
                //db.TaiKhoan_Update(_DO.ID, Encryptor.MD5Hash(_DO.MatKhau));
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Account");
        }

        public ActionResult Resetpass(int id)
        {
           /* var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login");
            }*/
            var res = (from dm in db.TaiKhoan_ID(id)
                       select new LoginValidation
                       {
                           ID = dm.ID,
                           HoTen = dm.HoTen
                       }).ToList();
            LoginValidation DO = new LoginValidation();
            if (res.Count > 0)
            {
                foreach (var dm in res)
                {
                    DO.ID = dm.ID;
                    DO.HoTen = dm.HoTen;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Resetpass(LoginValidation _DO)
        {
            try
            {
                db.TaiKhoan_Update(_DO.ID, Encryptor.MD5Hash(_DO.MatKhau));
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Account");
        }

        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login");
            }
            List<A_QuyenHT> pq = dbP.A_QuyenHT.ToList();
            ViewBag.PList = new SelectList(pq, "IDQuyenHT", "TenQuyenHT");

            List<PhongBan> dn = db.PhongBans.ToList();
            ViewBag.DNList = new SelectList(dn, "IDDN", "TenDN");

            //List<NT_Permission> q = db.NT_Permission.ToList();
            //ViewBag.IDQuyenNT = new SelectList(q, "IDQ", "Name");

            var res = (from a in dbP.A_TaiKhoan_ID(id)
                       select new LoginValidation
                       {
                           ID = a.ID,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           IDQuyenHT = (int)a.IDQuyenHT,
                           GroupQuyen = a.GroupQuyen
                       }).ToList();
            if (res.Count > 0)
            {
                var lsCheck = new List<ItemCheck>();
                var dsquyen = res[0].GroupQuyen?.Split(',');

                db.Configuration.ProxyCreationEnabled = false;
                List<A_QuyenHT> dt = dbP.A_QuyenHT.ToList();
                foreach (var item in dt)
                {
                    if (dsquyen == null)
                    {
                        var a = new ItemCheck { Name = item.TenQuyenHT, IDCN = item.IDQuyenHT, isChecked = false };
                        lsCheck.Add(a);
                    }
                    else
                    {
                        int pos = Array.IndexOf(dsquyen, item.IDQuyenHT.ToString());
                        if (pos > -1)
                        {
                            var a = new ItemCheck { Name = item.TenQuyenHT, IDCN = item.IDQuyenHT, isChecked = true };
                            lsCheck.Add(a);
                        }
                        else
                        {
                            var a = new ItemCheck { Name = item.TenQuyenHT, IDCN = item.IDQuyenHT, isChecked = false };
                            lsCheck.Add(a);
                        }
                    }

                }
                res[0].LSChecked = lsCheck.ToList();
            }

            LoginValidation DO = new LoginValidation();
            if (res.Count > 0)
            {
                foreach (var nv in res)
                {
                    DO.ID = nv.ID;
                    DO.MaNV = nv.MaNV;
                    DO.HoTen = nv.HoTen;
                    DO.IDQuyenHT = nv.IDQuyenHT;
                    DO.GroupQuyen = nv.GroupQuyen;
                    DO.LSChecked = nv.LSChecked;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Edit(LoginValidation _DO)
        {
            try
            {
                if (_DO.IDQuyenHT != 0)
                {
                    dbP.A_TK_Update_Quyen(_DO.ID, _DO.IDQuyenHT);

                }
                var words = new List<string>();
                var lsCN = dbP.A_QuyenHT.ToList();
                foreach (var item in _DO.LSChecked)
                {
                    if (item.isChecked) { words.Add(item.IDCN.ToString()); }
                    //else
                    //{
                    //    var a = lsCN.Where(x => x.IDQuyenHT == item.IDCN).Count();
                    //    if (a > 0) dbP.A_QuyenCT_isActive(_DO.IDController, item.IDCN, 0);
                    //}
                }
                _DO.GroupQuyen = string.Join(",", words);
                var update = dbP.A_TK_Update_GroupQuyen(_DO.ID, _DO.GroupQuyen);
                //var Check_Permission = db.AuthorizationContractors.Where(x => x.IDNhanVien == _DO.ID).FirstOrDefault();

                //if (Check_Permission != null)
                //{
                //    // update quyền QLNT
                //    var IDQ = db.AuthorizationContractors.Where(x => x.ID == Check_Permission.ID).FirstOrDefault();

                //    var a = db.AuthorizationContractors_update(IDQ.ID, _DO.ID, _DO.NT_Permission,4);
                //}
                //else
                //{
                //    // insert quyền QLNT
                //    db.AuthorizationContractors_insert(_DO.ID, _DO.NT_Permission);
                //}    

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại:" + e.Message + "');</script>";
            }

            return RedirectToAction("Index", "Account");
        }
    }
}