using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class HistoryBlackListController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "HistoryBlackList";
        // GET: Partner/HistoryBlackList
        public ActionResult Index(int? page, string search)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;
            var hisvp = db_nt.NT_HistoryBlackList_Select(search).ToList();
            var nv = db.NhanViens.Where(x => x.IDTinhTrangLV == 1);
            var res = from a in hisvp
                      join b in nv on a.IDNVUpdate equals b.ID
                      select new HistoryBlackListValidation
                      {
                          NVVPID = a.NVVPID,
                          NgayUpdate =(DateTime?)a.NgayUpdate??default,
                          IDNVUpdate =a.IDNVUpdate,
                          StatusUpdate =a.StatusUpdate,
                          HoTen =a.HoTen,
                          CCCD =a.CCCD,
                          CMND =a.CMND,
                          HoTenNVUPdate =b.MaNV+"-"+ b.HoTen,
                          TinhTrang = a.StatusUpdate ==1?"Mở khóa":"Khóa"
                      };
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDNVVP).ToList().ToPagedList(pageNumber, pageSize));
        }
    }
}