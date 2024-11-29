using EPORTAL.Common;
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
    public class AccountNTController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_NTEntities dbNT = new EPORTAL_NTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "AccountNT";
        // GET: Partner/AccountNT
        public ActionResult Index(int? page, string search)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var lsNT = db.NT_Partner.ToList();
            var lsUserNT = dbNT.NT_UserTemp.ToList();
            if (search == null) search = "";
            ViewBag.search = search;
            var res = (from a in dbNT.NT_UserTemp_Select(search)
                      join b in lsNT on a.IDNT equals b.ID
                      select new AccountNTValidation
                      {
                          ID =a.ID,
                          IDNT =a.IDNT,
                          UserName =a.UserName,
                          MatKhau =a.MatKhau,
                          TenNhaThau =b.FullName,
                          TinhTrang =a.TinhTrang
                      }).ToList();

            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }

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

            var NhaThau = (from nt in db.NT_Partner
                           select new NT_PartnerValidation
                           {
                               ID = (int)nt.ID,
                               FullName = nt.BPID + " : " + nt.FullName
                           }).ToList();
            ViewBag.IDNT = new SelectList(NhaThau, "ID", "FullName");

            var us = dbNT.NT_UserTemp.Select(x=>x.UserName).ToList();
            ViewBag.IDUser =us;

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(AccountNTValidation _DO)
        {

            try
            {

                if (Request != null)
                {
                    if (!String.IsNullOrEmpty(_DO.UserName) && !String.IsNullOrEmpty(_DO.MatKhau))
                    {
                        var mk = Encryptor.MD5Hash(_DO.MatKhau);
                        dbNT.NT_UserTemp_insert(_DO.UserName,mk,_DO.IDNT,1);
                    }
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AccountNT");
        }
        public ActionResult Resetpass(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }


            var lsNT = db.NT_Partner.ToList();
            var lsUserNT = dbNT.NT_UserTemp.ToList();
            var res = (from a in lsUserNT.Where(x => x.ID == id)
                       join b in lsNT on a.IDNT equals b.ID
                       select new AccountNTValidation
                       {
                           ID = a.ID,
                           UserName = a.UserName,
                       }).ToList();
            AccountNTValidation DO = new AccountNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.UserName = a.UserName;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Resetpass(AccountNTValidation _DO)
        {
            try
            {
                dbNT.NT_UserTemp_updatePass(_DO.ID, Encryptor.MD5Hash(_DO.MatKhau));
                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AccountNT");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var lsNT = db.NT_Partner.ToList();
            var lsUserNT = dbNT.NT_UserTemp.ToList();
            var res = (from a in lsUserNT.Where(x=>x.ID ==id)
                      join b in lsNT on a.IDNT equals b.ID
                      select new AccountNTValidation
                      {
                          ID = a.ID,
                          IDNT = a.IDNT,
                          UserName = a.UserName,
                          MatKhau = a.MatKhau,
                          TenNhaThau = b.FullName,
                          TinhTrang = a.TinhTrang,
                          StatusSV = a.TinhTrang ==1?true:false
                      }).ToList();
            AccountNTValidation DO = new AccountNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.MatKhau = a.MatKhau;
                    DO.TenNhaThau = a.TenNhaThau;
                    DO.TinhTrang = a.TinhTrang;
                    DO.UserName = a.UserName;
                    DO.IDNT = a.IDNT;
                    DO.StatusSV = a.StatusSV;
                }
            }
            else
            {
                HttpNotFound();
            }
            List<NT_Partner> dt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(dt, "ID", "FullName",DO.IDNT);
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AccountNTValidation _DO)
        {

            try
            {
                _DO.TinhTrang = _DO.StatusSV == true ? 1 : 0;
                var a = dbNT.NT_UserTemp_update(_DO.ID,_DO.UserName, _DO.MatKhau, _DO.IDNT, _DO.TinhTrang);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AccountNT");
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
                dbNT.NT_UserTemp_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AccountNT");
        }

        public ActionResult Signature(int? id)
        
        {
            var lsNT = db.NT_Partner.ToList();
            var lsUserNT = dbNT.NT_UserTemp.ToList();
            var res = (from a in lsUserNT.Where(x => x.ID == id)
                       join b in lsNT on a.IDNT equals b.ID
                       select new AccountNTValidation
                       {
                           ID = a.ID,
                           IDNT = a.IDNT,
                           UserName = a.UserName,
                           MatKhau = a.MatKhau,
                           TenNhaThau = b.FullName,
                           TinhTrang = a.TinhTrang,
                           ChuKy = a.ChuKy,
                           StatusSV = a.TinhTrang == 1 ? true : false
                       }).ToList();
            AccountNTValidation DO = new AccountNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.MatKhau = a.MatKhau;
                    DO.TenNhaThau = a.TenNhaThau;
                    DO.TinhTrang = a.TinhTrang;
                    DO.UserName = a.UserName;
                    DO.IDNT = a.IDNT;
                    DO.ChuKy = a.ChuKy;
                    DO.StatusSV = a.StatusSV;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Signature(AccountNTValidation _DO)
        {
            try
            {
                if(_DO.ImgChuky != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/Signature/");
                    //string path ="~/Images/";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var IDNV = dbNT.NT_UserTemp.Where(x => x.ID == _DO.ID).FirstOrDefault();
                    string FileName = _DO.ImgChuky != null ? IDNV.UserName + DateTime.Now.ToString("yyyyMMddHHmm") + "_" + _DO.ImgChuky : "";
                    //To Get File Extension  
                    string FileExtension = _DO.ImgChuky != null ? Path.GetExtension(_DO.ImgChuky.FileName) : "";

                    if (_DO.ImgChuky != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.ImgChuky.SaveAs(path + FileName);
                        _DO.ChuKy = "~/UploadedFiles/Signature/" + FileName;
                    }
                    var Update = db.NhanViens.Where(x => x.ID == _DO.ID).FirstOrDefault();
                    var Update_CK = dbNT.NT_UserTemp_Img(_DO.ID, _DO.ChuKy);
                }

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại:" + e.Message + "');</script>";

            }

            return RedirectToAction("Index", "ListCarteTemporaireNT", new { area = "TagSign" });
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(AccountNTValidation _DO)
        {
            if (User.Identity.IsAuthenticated)
            {
                string mk = Encryptor.MD5Hash(_DO.MatKhauCu);
                var Check = dbNT.NT_UserTemp.Where(x=>x.ID == _DO.ID).FirstOrDefault();
                if(Check.MatKhau != mk)
                {
                    TempData["msgSuccess"] = "<script>alert('Mật khẩu không đúng');</script>";
                    return RedirectToAction("ChangePassword", "AccountNT", new { area = "Partner" });
                } 
                if(_DO.MatKhau != _DO.NhapLaiMatKhau)
                {
                    TempData["msgSuccess"] = "<script>alert('Mật khẩu nhập lại không đúng');</script>";
                    return RedirectToAction("ChangePassword", "AccountNT", new { area = "Partner" });
                }
                string MKM = Encryptor.MD5Hash(_DO.MatKhau);
                var UpPass = dbNT.NT_UserTemp_Pass(_DO.ID, MKM);
                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Lỗi thay đổi mật khẩu');</script>";
            }
            return RedirectToAction("Index", "ListCarteTemporaireNT", new { area = "TagSign" });
        }
        public ActionResult ResetListPass()
        {
            try
            {
                var List = dbNT.NT_UserTemp.ToList();
                foreach (var item in List)
                {   
                    string str = item.UserName.ToString();
                    string[] arrListStr = str.Split('-');
                    if(arrListStr.Length == 2)
                    {
                        var a = dbNT.NT_UserTemp_update(item.ID, item.UserName, item.MatKhau, item.IDNT, 0);
                    }

                    TempData["msgSuccess"] = "<script>alert('Khóa tài khoản thành công');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }


            return RedirectToAction("Index", "AccountNT");
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
                            string MaNT= dt.Rows[i][1].ToString().Trim();
                            int BPID = Convert.ToInt32(MaNT);
                            var ID_NT = db.NT_Partner.Where(x => x.BPID == BPID).FirstOrDefault();
                            if(ID_NT == null)
                            {
                                TempData["msgError"] = "<script>alert('Vui lòng kiểm lại thông tin Business Partner: " + MaNT + "');</script>";

                                return RedirectToAction("Index", "AccountNT");
                            }    
                            string MatKhau = MaNT;
                            var mk = Encryptor.MD5Hash(MatKhau);
                            dbNT.NT_UserTemp_insert(MaNT, mk, ID_NT.ID, 1);
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
            TempData["msgSuccess"] = "<script>alert('Cập nhật tài khoản thành công');</script>";

            return RedirectToAction("Index", "AccountNT");
        }
    }
}