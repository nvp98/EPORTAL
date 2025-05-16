using EPORTAL.Common;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsServey;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EPORTAL.Models;
namespace EPORTAL.Controllers
{
    public class LoginController : Controller
    {
        
        // GET: Login
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_SERVEYEntities dbSV = new EPORTAL_SERVEYEntities();
        EPORTAL_NTEntities dbNT = new EPORTAL_NTEntities();
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(LoginValidation u)
        {
            var IDGroup = db.ProjectsGroups.Min(x => x.IDGroup);
            int id = Convert.ToInt32(IDGroup);

            string mk = Common.Encryptor.MD5Hash(u.MatKhau);
            NhanVien user = db.NhanViens.Where(x => x.MaNV == u.MaNV && x.MatKhau == mk && x.IDTinhTrangLV == 1).FirstOrDefault();
            var checkUser = dbNT.NT_UserTemp.Where(x => x.UserName == u.MaNV && x.MatKhau == mk && x.TinhTrang ==1).FirstOrDefault();
            if (user != null)
            {
                string Cookie = string.Format("{0};{1};{2};{3};{4};{5};{6}", user.ID, user.MaNV, user.HoTen, user.IDPhongBan, user.IDQuyen, user.IDQuyenHT, user.GroupQuyen);
                FormsAuthentication.SetAuthCookie(Cookie, false);

                return RedirectToAction("Index", "ListProject", new { area = "View360" , id = id});

            }
            //else if(user == null && checkUser != null)
            //{
            //    //string Cookie = string.Format("{0};{1};{2};{3};{4};{5};{6};{7}", user.ID, user.MaNV, user.HoTen, user.IDPhongBan, user.IDQuyen, user.IDQuyenHT, user.GroupQuyen,"NT");
            //    //FormsAuthentication.SetAuthCookie(Cookie, false);
            //    return RedirectToAction("Index", "ListCarteTemporaireNT", new { area = "TagSign" });
            //}
            else
            {
                return View();
            }


        }
        [HttpPost]
        public ActionResult LoginUser(LoginValidation u, string returnUrl)
        {

            NhanVien user1 = db.NhanViens.Where(x => x.MaNV == u.MaNV && x.CCCD.Substring(x.CCCD.Length - 5,5) == u.MatKhau).FirstOrDefault();


            string mk = Common.Encryptor.MD5Hash(u.MatKhau);
            NhanVien user = db.NhanViens.Where(x => x.MaNV == u.MaNV && x.MatKhau == mk ).FirstOrDefault();
            var checkUser = dbNT.NT_UserTemp.Where(x => x.UserName == u.MaNV && x.MatKhau == mk && x.TinhTrang == 1).FirstOrDefault();
            if (user != null || user1 != null)
            {
                if (user == null) user = user1;
                string Cookie = string.Format("{0};{1};{2};{3};{4};{5};{6}", user.ID, user.MaNV, user.HoTen, user.IDPhongBan, user.IDQuyen, user.IDQuyenHT, user.GroupQuyen);
                FormsAuthentication.SetAuthCookie(Cookie, false);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                var check = dbSV.EmployeeServeys.Where(x => x.IDNV == user.ID).ToList();
                var check_q = db.AuthorizationContractors.Where(x => x.IDNhanVien == user.ID && x.IDLKD == 1 
                                                                   || x.IDNhanVien == user.ID && x.IDLKD == 2 
                                                                   || x.IDNhanVien == user.ID && x.IDLKD == 3
                                                                   || x.IDNhanVien == user.ID && x.IDLKD == 5).FirstOrDefault();
                var check_ck = db.NhanViens.Where(x => x.MaNV == u.MaNV).FirstOrDefault();
                if(check_q != null && check_ck != null)
                {
                    if (check_q.IDLKD == 1 && check_ck.Chukychinh == null || check_q.IDLKD == 2 && check_ck.Chukynhay == null || check_q.IDLKD == 3 && check_ck.Chukynhay == null || check_q.IDLKD == 5 && check_ck.Chukynhay == null)
                    {
                        return RedirectToAction("Signature", "Login", new { id = check_ck.ID });
                    }
                }    
                if (u.MatKhau == "123456a@")
                {
                    return RedirectToAction("ChangePassword", "Login");
                }
                else if (check.Count > 0) // đăng ký
                {
                    return RedirectToAction("Index", "UserServey", new { area = "Servey" });
                }
                else
                {
                    var List_Group = db.Get_IDGroup(user.ID).Min(x => x.IDGroup);
                    int id = Convert.ToInt32(List_Group);
                    return RedirectToAction("Index", "ListProject", new { area = "View360" , id =id});
                }

            }
            else if (user == null && checkUser != null)
            {
                string Cookie = string.Format("{0};{1}", checkUser.ID,checkUser.UserName);
                FormsAuthentication.SetAuthCookie(Cookie, false);
                return RedirectToAction("Index", "List_RegisterPeople_NT", new { area = "TagSign" });
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Sai tên đăng nhập hoặc mật khẩu');window.location.href = '/Login'</script>";
                //return View(TempData);
                return RedirectToAction("", "Login");
            }
        }
        public ActionResult Logout()
        {
            
            
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            if (TempData["msgError"] != null)
            {
                var checkMsg = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                if(checkMsg == TempData["msgError"].ToString())
                {
                    return RedirectToAction("MsgE", "Login");
                }
            }
            return RedirectToAction("", "Login");

        }
        public ActionResult MsgE()
        {
            TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');location.reload()</script>"; 
            return RedirectToAction("Index", "Login");

        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(LoginValidation model)
        {
            if (User.Identity.IsAuthenticated)
            {
                string mk = Encryptor.MD5Hash(model.MatKhauCu);
                NhanVien Suser = db.NhanViens.SingleOrDefault(x => x.ID == Models.MyAuthentication.ID);
                NhanVien user = db.NhanViens.SingleOrDefault(x => x.MaNV == Suser.MaNV && x.MatKhau == mk);
                if (user != null)
                {
                    //user.MatKhau = model.MatKhau;
                    mk = Encryptor.MD5Hash(model.MatKhau);
                    user.MatKhau = mk;
                    db.SaveChanges();
                    Session.Clear();
                    Session.Abandon();
                    //TempData["msg"] = "<script>alert('Cập nhập thành công')</script>";
                    ViewBag.Message = "<script>alert('Thay đổi mật khẩu thành công');window.location.href = '/Login</script>";
                    //Page.ClientScript.RegisterClientScriptBlock(GetType(), "alerta", "alert('Save records with success')", true);
                    //return RedirectToAction("Index", "Login");
                    return RedirectToAction("Index", "ListProject", new { area = "View360" });
                }
                else
                {
                    ViewBag.Message = "<script>alert('Mật khẩu cũ không đúng, vui lòng nhập lại')</script>";
                    //TempData["msg"] = "<script>alert('Mật khẩu cũ không đúng, vui lòng nhập lại')</script>";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "<script>alert('Lỗi thay đổi mật khẩu')</script>";
                //TempData["msg"] = "<script>alert('Lỗi thay đổi mật khẩu')</script>";
                return View();
            }

        }

        public ActionResult Signature(int? id)
        {
            var res = (from a in db.NhanViens.Where(x => x.ID == id)
                       select new LoginValidation
                       {
                           ID = a.ID,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           Chukynhay = a.Chukynhay,
                           Chukychinh = a.Chukychinh
                       }).ToList();
            LoginValidation DO = new LoginValidation();
            if (res.Count > 0)
            {
                foreach (var nv in res)
                {
                    DO.ID = nv.ID;
                    DO.MaNV = nv.MaNV;
                    DO.HoTen = nv.HoTen;
                    DO.Chukynhay = nv.Chukynhay;
                    DO.Chukychinh = nv.Chukychinh;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Signature(LoginValidation _DO)
        {
            try
            {
               if(_DO.ImgChukynhay != null)
               {
                    string path = Server.MapPath("~/UploadedFiles/Signature/");
                    //string path ="~/Images/";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var IDNV = db.NhanViens.Where(x=>x.ID == _DO.ID).FirstOrDefault();
                    string FileName = _DO.ImgChukynhay != null ? IDNV.MaNV + DateTime.Now.ToString("yyyyMMddHHmm") + "_" + "Chukynhay" : "";
                    //To Get File Extension  
                    string FileExtension = _DO.ImgChukynhay != null ? Path.GetExtension(_DO.ImgChukynhay.FileName) : "";

                    if (_DO.ImgChukynhay != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.ImgChukynhay.SaveAs(path + FileName);
                        _DO.Chukynhay = "~/UploadedFiles/Signature/" + FileName;
                    }
                }    
               if(_DO.ImgChukychinh != null)
               {
                    string path = Server.MapPath("~/UploadedFiles/Signature/");
                    //string path ="~/Images/";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var IDNV = db.NhanViens.Where(x => x.ID == _DO.ID).FirstOrDefault();
                    string FileName = _DO.ImgChukychinh != null ? IDNV.MaNV + DateTime.Now.ToString("yyyyMMddHHmm") + "_" + "Chukychinh" : "";
                    //To Get File Extension  
                    string FileExtension = _DO.ImgChukychinh != null ? Path.GetExtension(_DO.ImgChukychinh.FileName) : "";

                    if (_DO.ImgChukychinh != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.ImgChukychinh.SaveAs(path + FileName);
                        _DO.Chukychinh = "~/UploadedFiles/Signature/" + FileName;
                    }
                }
                var Update = db.NhanViens.Where(x=>x.ID == _DO.ID).FirstOrDefault();
                if(_DO.Chukychinh == null)
                {
                    db.TaiKhoan_Img(_DO.ID, _DO.Chukynhay, Update.Chukychinh);
                }  
                else if(_DO.Chukynhay == null)
                {
                    db.TaiKhoan_Img(_DO.ID, Update.Chukynhay, _DO.Chukychinh);
                }  
                else if(_DO.Chukychinh != null && _DO.Chukynhay != null)
                {
                    db.TaiKhoan_Img(_DO.ID, _DO.Chukynhay, _DO.Chukychinh);
                }  
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại:" + e.Message + "');</script>";

            }

              return RedirectToAction("Index", "ListProject", new { area = "View360" });
        }
    }
}