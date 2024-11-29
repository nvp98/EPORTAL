//using EPORTAL.ModelsPartner;
//using EPORTAL.ModelsView360;
//using PagedList;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace EPORTAL.Areas.Partner.Controllers
//{
//    public class HistoryCardMobilityController : Controller
//    {
//        // GET: Partner/HistoryCardMobility
//        public ActionResult Index(int? page, int? idnv)
//        {
//            EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
//            EPORTALEntities db = new EPORTALEntities();

//            var res = (from hd in db_nt.NT_HistoryCardMobility.Where(x => x.IDNVNT == idnv)
//                       join nv in db_nt.NT_NhanVienNT on hd.IDNVNT equals nv.IDNVNT
//                       select new HistoryCardMobilityValidation
//                       {
//                           IDHCM = hd.IDHCM,
//                           IDNVNT = (int?)hd.IDNVNT ?? default,
//                           HoTen = nv.HoTen,
//                           CCCD = nv.CCCD,
//                           IDNT = (int?)hd.IDNT ?? default,
//                           Tungay = (DateTime?)hd.Tungay ?? default,
//                           Denngay = (DateTime?)hd.Denngay ?? default,
//                           GhiChu = hd.GhiChu

//                       }).ToList();

//            if (page == null) page = 1;
//            int pageSize = 50;
//            int pageNumber = (page ?? 1);
//            return View(res.ToList().ToPagedList(pageNumber, pageSize));
//        }
//    }
//}