using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class NotifyController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: Notify_Partner
        public ActionResult Index(int? page)
        {
            if (MyAuthentication.IDQuyen == 1 || MyAuthentication.IDQuyen == 2)
            {
                var res = (from t in db_context.DB_ThongBao
                           select new NotifyValidation
                           {
                               IDTBao = t.IDTBao,
                               MaTBao = t.MaTBao,
                               NoiDungTBao = t.NoiDungTBao,
                               FileDinhKem = t.FileDinhKem,
                               Ngay = (DateTime)t.Ngay,
                               //LinhVuc = d.LinhVucHĐ
                           }).OrderByDescending(x => x.Ngay).ToList();
                if (page == null) page = 1;
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }
            else if (MyAuthentication2.IDQuyen == 3)
            {
                var res = (from t in db_context.DB_ThongBao
                           join d in db_context.DB_ThongBao_DoiTac on t.IDTBao equals d.TBID
                           join e in db_context.DB_TTDoiTac.Where(x => x.IDDoiTac == MyAuthentication2.IDDoiTac) on d.DoiTacID equals e.IDDoiTac
                           select new NotifyValidation
                           {
                               IDTBao = t.IDTBao,
                               MaTBao = t.MaTBao,
                               NoiDungTBao = t.NoiDungTBao,
                               FileDinhKem = t.FileDinhKem,
                               Ngay = (DateTime)t.Ngay,
                               //LinhVuc = d.LinhVucHĐ
                           }).OrderByDescending(x => x.Ngay).ToList();
                if (page == null) page = 1;
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return View();
            }
        }


    }
}