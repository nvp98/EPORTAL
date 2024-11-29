using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class GeneralNotifyController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: Notify_Partner
        public ActionResult Index(int? page)
        {

            var res = (from t in db_context.DB_ThongBao
                       join a in db_context.DB_TinhTrangThongBao on t.TinhTrangTB equals a.IDTinhTrangTB
                       select new NotifyValidation
                       {
                           IDTBao = t.IDTBao,
                           MaTBao = t.MaTBao,
                           NoiDungTBao = t.NoiDungTBao,
                           FileDinhKem = t.FileDinhKem,
                           Ngay = (DateTime)t.Ngay,
                           TinhTrangTB = (int)t.TinhTrangTB
                           //LinhVuc = d.LinhVucHĐ
                       }).OrderByDescending(x => x.Ngay).Where(y => y.TinhTrangTB == 1).ToList();
            if (page == null) page = 1;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }


    }
}