using EPORTAL.ModelsDanhBaDoiTac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class StatisticPageController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: StatisticPage
        public ActionResult Index(int? page)
        {
            return View();

        }
        public ActionResult GetData()
        {
            int MB = db_context.DB_DiaChi.Where(x => x.Mien == "Miền Bắc").Count();
            int MT = db_context.DB_DiaChi.Where(x => x.Mien == "Miền Trung").Count();
            int MN = db_context.DB_DiaChi.Where(x => x.Mien == "Miền Nam").Count();
            int other = db_context.DB_DiaChi.Where(x => x.Mien == "Orther").Count();
            Demo demo = new Demo();
            demo.Northern = MB;
            demo.Central = MT;
            demo.South = MN;
            demo.Orther = other;
            return Json(demo, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetData1()
        {
            var res = db_context.DB_TTDoiTac.Include("LinhVucHoatDong")
                .GroupBy(p => p.LinhVucHĐ)
                .Select(g => new { name = g.Key, count = g.Count() });
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        private class Demo
        {
            public int Northern { get; set; }
            public int Central { get; set; }
            public int South { get; set; }
            public int Orther { get; set; }
        }
    }
}