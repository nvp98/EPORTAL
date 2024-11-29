using EPORTAL.ModelsDanhBaDoiTac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class LoginDBDTController : Controller
    {
        // GET: Publish/Login
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        public ActionResult Index()
        {
            return View();

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(LoginValidation model)
        {
            string mk = Common.Encryptor.MD5Hash(model.MatKhau);
            DB_NhanVien user = db_context.DB_NhanVien.SingleOrDefault(x => x.MaNV == model.TaiKhoan && x.MatKhau == mk && model.isDB == 0);
            DB_TTDoiTac user2 = db_context.DB_TTDoiTac.SingleOrDefault(x => x.BPID == model.TaiKhoan && x.MatKhau == mk && model.isDT == 0);
            //string result = "fail";
            if (user != null && model.isDB == 0)
            {
                string Cookie = string.Format("{0};{1};{2};{3}", user.ID, user.MaNV, user.HoTen, user.IDQuyen);
                FormsAuthentication.SetAuthCookie(Cookie, false);

                return RedirectToAction("Index", "HomePage", new { area = "Publish" });


            }
            else if (user2 != null && model.isDT == 0)
            {
                string Cookie = string.Format("{0};{1};{2};{3}", user2.IDDoiTac, user2.BPID, user2.ShortName, user2.IDQuyen);
                FormsAuthentication.SetAuthCookie(Cookie, false);
                return RedirectToAction("Index", "HomePage", new { area = "Publish" });
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult LoginUser(LoginValidation model)
        {
            string mk = Common.Encryptor.MD5Hash(model.MatKhau);
            DB_NhanVien user = db_context.DB_NhanVien.SingleOrDefault(x => x.MaNV == model.TaiKhoan && x.MatKhau == mk && model.isDB == 0);
            DB_TTDoiTac user2 = db_context.DB_TTDoiTac.SingleOrDefault(x => x.BPID == model.TaiKhoan && x.MatKhau == mk && model.isDT == 0);
            //string result = "fail";
            if (user != null && model.isDB == 0)
            {
                string Cookie = string.Format("{0};{1};{2};{3}", user.ID, user.MaNV, user.HoTen, user.IDQuyen);
                FormsAuthentication.SetAuthCookie(Cookie, false);
                return RedirectToAction("Index", "HomePage", new { area = "Publish" });
            }
            else if (user2 != null && model.isDT == 0)
            {
                string Cookie = string.Format("{0};{1};{2};{3}", user2.IDDoiTac, user2.BPID, user2.ShortName, user2.IDQuyen);
                FormsAuthentication.SetAuthCookie(Cookie, false);
                return RedirectToAction("Index", "HomePage", new { area = "Publish" });
            }
            else
            {
                TempData["msg"] = "<script>alert('Sai tên đăng nhập hoặc mật khẩu');window.location.href = '/Login'</script>";
                //return View(TempData);
                return RedirectToAction("Index", "HomeDBDT");
            }

        }


        public ActionResult Logout()
        {

            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "HomeDBDT");

        }
        /* public ActionResult ChangePassword()
         {
             return View();
         }
         [HttpPost]

         public ActionResult ChangePassword(LoginValidation model)
         {
             /*if (Session["S_TK"] != null)
             {
                 string mk = Encryptor.MD5Hash(model.MatKhauCu);
                 if(User.Identity.IsAuthenticated)
                 {
                     if(MyAuthentication.IDQuyen ==1 || MyAuthentication.IDQuyen==2)
                     {
                         NhanVien Suser = (NhanVien)Session["S_TK"];
                         NhanVien user = db_context.NhanViens.SingleOrDefault(x => x.MaNV == Suser.MaNV && x.MatKhau == mk && x.isDB == 0);
                         if (user != null && model.isDB == 0)
                         {
                             //user.MatKhau = model.MatKhau;
                             mk = Encryptor.MD5Hash(model.MatKhau);
                             user.MatKhau = mk;
                             db_context.SaveChanges();
                             Session.Clear();
                             Session.Abandon();
                             //TempData["msg"] = "<script>alert('Cập nhập thành công')</script>";
                             ViewBag.Message = "<script>alert('Thay đổi mật khẩu thành công');window.location.href = '/Login</script>";
                             //Page.ClientScript.RegisterClientScriptBlock(GetType(), "alerta", "alert('Save records with success')", true);
                             //return RedirectToAction("Index", "Login");
                             return View();
                         }
                         else
                         {
                             ViewBag.Message = "<script>alert('Mật khẩu cũ không đúng, vui lòng nhập lại')</script>";
                             //TempData["msg"] = "<script>alert('Mật khẩu cũ không đúng, vui lòng nhập lại')</script>";
                             return View();
                         }
                     }
                     else if(MyAuthentication2.IDQuyen == 3)
                     {
                         TTDoiTac Suser2 = (TTDoiTac)Session["S_TK"];
                         TTDoiTac user2 = db_context.TTDoiTacs.SingleOrDefault(x => x.MaDoiTac == Suser2.MaDoiTac && x.MatKhau == mk & x.isDT == 0);
                         if (user2 != null && model.isDT == 0)
                         {
                             mk = Encryptor.MD5Hash(model.MatKhau);
                             user2.MatKhau = mk;
                             db_context.SaveChanges();
                             Session.Clear();
                             Session.Abandon();
                             //TempData["msg"] = "<script>alert('Cập nhập thành công')</script>";
                             ViewBag.Message = "<script>alert('Thay đổi mật khẩu thành công');window.location.href = '/Login</script>";
                             //Page.ClientScript.RegisterClientScriptBlock(GetType(), "alerta", "alert('Save records with success')", true);
                             //return RedirectToAction("Index", "Login");
                             return View();
                         }
                         else
                         {
                             ViewBag.Message = "<script>alert('Mật khẩu cũ không đúng, vui lòng nhập lại')</script>";
                             //TempData["msg"] = "<script>alert('Mật khẩu cũ không đúng, vui lòng nhập lại')</script>";
                             return View();
                         }
                     }

                 }


             else
             {
                 ViewBag.Message = "<script>alert('Lỗi thay đổi mật khẩu')</script>";
                 //TempData["msg"] = "<script>alert('Lỗi thay đổi mật khẩu')</script>";
                 return View();
             }*/


    }
}