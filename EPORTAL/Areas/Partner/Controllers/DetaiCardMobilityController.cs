//using EPORTAL.Models;
//using EPORTAL.ModelsPartner;
//using EPORTAL.ModelsView360;
//using PagedList;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace EPORTAL.Areas.Partner.Controllers
//{
//    public class DetaiCardMobilityController : Controller
//    {
//        EPORTALEntities db = new EPORTALEntities();
//        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
//        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
//        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
//        EPORTAL_UNISEntities dbUN = new EPORTAL_UNISEntities();
//        String controll = "DetaiCardMobility";
//        string connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ToString();
//        // GET: Partner/DetaiCardMobility
//        public ActionResult Index(int? page, DateTime? begind, DateTime? endd, string MaNhaThau, string MaNhanVien)
//        {
//            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
//            ViewBag.QUYENCN = ListQuyen;
//            if (!ListQuyen.Contains("VIEW_ALL"))
//            {
//                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
//                return RedirectToAction("Logout", "Login", new { area = "" });
//            }

//            var NhaThau = (from nt in db.NT_Partner.Where(x => x.CodeUnis != "")
//                           select new NT_PartnerValidation
//                           {
//                               ID = (int)nt.ID,
//                               CodeUnis = nt.CodeUnis,
//                               FullName = nt.CodeUnis + " : " + nt.ShortName
//                           }).ToList();
//            ViewBag.NTList = new SelectList(NhaThau, "CodeUnis", "FullName", MaNhaThau);

//            var NhanVien = (from nt in db_nt.NT_CardMobility
//                            join nv in db_nt.NT_NhanVienNT on nt.IDNVNT equals nv.IDNVNT
//                            select new CardMobilityValidation
//                            {
//                                Sothe = nt.Sothe,
//                                HoTen = nv.HoTen + " : " + nt.Sothe,
//                            }).ToList();
//            ViewBag.NVList = new SelectList(NhanVien, "SoThe", "HoTen", MaNhanVien);


//            string begindcover = "";
//            string enddcover = "";
//            if (begind == null && endd == null)
//            {
//                begindcover = DateTime.Now.Date.ToString("yyyyMMdd");
//                enddcover = DateTime.Now.Date.ToString("yyyyMMdd");
//            }
//            else
//            {
//                string cover = begind.ToString();
//                begindcover = DateTime.Parse(cover).ToString("yyyyMMdd");
//                string cover1 = endd.ToString();
//                enddcover = DateTime.Parse(cover1).ToString("yyyyMMdd");
//            }
//            List<NT_CardTempENTRY> listNT = new List<NT_CardTempENTRY>();
//            listNT = (from a in dbUN.LogUNIS_Query(begindcover, enddcover).Where(x => x.isNT == 3)
//                      select new NT_CardTempENTRY
//                      {
//                          Mathe = a.MaThe,
//                          MaNT = a.MaNT,
//                          Hoten = a.HoTen,
//                          TenNT = a.TenNT,
//                          Thoigianquyet = a.Ngay.ToString() + a.Gio.ToString(),
//                          Tenmayquyet = a.MQT,
//                          CCCD = a.CCCD
//                      }).ToList();

//            if (MaNhanVien == null) MaNhanVien = "";
//            if (MaNhanVien != "") listNT = listNT.Where(x => x.Mathe == MaNhanVien).ToList();
//            if (MaNhaThau == null) MaNhaThau = "";
//            if (MaNhaThau != "") listNT = listNT.Where(x => x.MaNT == MaNhaThau).ToList();


//            string begindcoverNew = DateTime.Parse(DateTime.Now.AddDays(-1).ToString()).ToString("yyyyMMdd");
//            string enddcoverNew = DateTime.Parse(DateTime.Now.ToString()).ToString("yyyyMMdd");
//            var aa = dbUN.LogUNIS_Query(enddcoverNew, enddcoverNew).OrderByDescending(x => x.Gio).ToList();
//            if (aa.Count == 0) aa = dbUN.LogUNIS_Query(begindcoverNew, begindcoverNew).OrderByDescending(x => x.Gio).ToList();
//            ViewBag.LastTime = aa.Count() != 0 ? aa.First().Ngay.ToString() + aa.First().Gio.ToString() : "";
//            ViewBag.SUM = listNT.Count();


//            if (page == null) page = 1;
//            int pageSize = 150;
//            int pageNumber = (page ?? 1);
//            return View(listNT.OrderByDescending(x => x.Thoigianquyet).ToList().ToPagedList(pageNumber, pageSize));
//        }
//    }
//}