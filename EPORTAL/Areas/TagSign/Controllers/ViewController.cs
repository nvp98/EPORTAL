using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ViewController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "View";
        // GET: TagSign/View
        public ActionResult Index(int? page, DateTime? begind, DateTime? endd)
        {
            DateTime Now = DateTime.Now;
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db_nt.NT_CarteTemporaire.Where(x => x.TinhTrang == 3 || x.TinhTrang == 5)
                       select new NT_CarteTemporaireValidation
                       {
                           IDTTT = (int)a.IDTTT,
                           NoiDung = a.NoiDung,
                           NguoiTaoID = (int)a.NguoiTaoID,
                           NgayTao = (DateTime?)a.NgayTao ?? default,
                           TinhTrang = (int?)a.TinhTrang ?? default,
                           NTID = (int)a.NTID,
                           PhongBanID = (int)a.PhongBanID,
                           HangMuc = a.HangMuc,
                           ThoiHan = (DateTime)a.ThoiHan,
                           GhiChu = a.GhiChu,
                           isNT =a.isNT
                       }).ToList();

            if (begind != null && endd != null)
            {
                res = res.OrderByDescending(x => x.NgayTao).Where(x => x.NgayTao >= begind && x.NgayTao <= endd).ToList();
            }
            else
            {
                res = res.OrderByDescending(x => x.NgayTao).Where(x => x.NgayTao >= Now && x.NgayTao <= Now).ToList();
            }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);

            return View(res.OrderByDescending(x => x.NgayTao).ToList().ToPagedList(pageNumber, pageSize));
        }
    }
}