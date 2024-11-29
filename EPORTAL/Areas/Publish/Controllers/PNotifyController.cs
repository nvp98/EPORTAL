using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class PNotifyController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: ManagePartner
        public ActionResult Index(int id, int? page, string search)
        {
            //var res = (from td in db_context.ThongBao_DoiTac
            //           join t in db_context.ThongBaos on td.TBID equals t.IDTBao
            //           join d in db_context.TTDoiTacs.Where(x => x.IDDoiTac ==id) on td.DoiTacID equals d.IDDoiTac
            //           select new Notify_PartnerValidation
            //           {
            //               IDTBDT = td.ID_TBDT,
            //               MaDoiTac = d.MaDoiTac,
            //               TenQT = d.TenQT,
            //               TBaoID = t.IDTBao,
            //               MaTBao = t.MaTBao,
            //               NoiDungTBao = t.NoiDungTBao,
            //               FileDinhKem = t.FileDinhKem,
            //               Ngay = (DateTime)t.Ngay,
            //           }).OrderByDescending(x => x.Ngay).ToList();
            //if (page == null) page = 1;
            //int pageSize = 20;
            //int pageNumber = (page ?? 1);
            //return View(res.ToList().ToPagedList(pageNumber, pageSize));

            if (search == null) search = "";
            ViewBag.search = search;
            ViewBag.YNList = getYNOptions();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(PNotify(id, search).ToPagedList(pageNumber, pageSize));
        }


        private List<SelectListItem> getYNOptions()
        {
            List<SelectListItem> year = new List<SelectListItem>();
            var res = (from td in db_context.DB_ThongBao
                       select new NotifyValidation
                       {
                           IDTBao = td.IDTBao,
                           MaTBao = td.MaTBao,
                           NoiDungTBao = td.NoiDungTBao,
                           FileDinhKem = td.FileDinhKem,
                           Ngay = (DateTime)td.Ngay,
                       }).ToList();
            foreach (var it in res)
            {
                year.Add(new SelectListItem() { Text = it.Ngay.Year.ToString(), Value = it.MaTBao, Selected = true });
            }
            return year;
        }

        List<Notify_PartnerValidation> PNotify(int id, string search)
        {

            if (search == "" & search == null)
            {
                var res = (from td in db_context.DB_ThongBao_DoiTac
                           join t in db_context.DB_ThongBao on td.TBID equals t.IDTBao
                           join d in db_context.DB_TTDoiTac.Where(x => x.IDDoiTac == id) on td.DoiTacID equals d.IDDoiTac
                           select new Notify_PartnerValidation
                           {
                               IDTBDT = td.ID_TBDT,
                               IDBP = d.BPID,
                               ShortName = d.ShortName,
                               TBaoID = t.IDTBao,
                               MaTBao = t.MaTBao,
                               NoiDungTBao = t.NoiDungTBao,
                               FileDinhKem = t.FileDinhKem,
                               Ngay = (DateTime)t.Ngay,
                           }).OrderByDescending(x => x.Ngay).ToList();
                return res;
            }
            else
            {
                var res = (from td in db_context.DB_ThongBao_DoiTac.Where(x => (x.DB_ThongBao.NoiDungTBao.Contains(search) || x.DB_ThongBao.MaTBao.Contains(search)))
                           join t in db_context.DB_ThongBao on td.TBID equals t.IDTBao
                           join d in db_context.DB_TTDoiTac.Where(x => x.IDDoiTac == id) on td.DoiTacID equals d.IDDoiTac
                           select new Notify_PartnerValidation
                           {
                               IDTBDT = td.ID_TBDT,
                               IDBP = d.BPID,
                               ShortName = d.ShortName,
                               TBaoID = t.IDTBao,
                               MaTBao = t.MaTBao,
                               NoiDungTBao = t.NoiDungTBao,
                               FileDinhKem = t.FileDinhKem,
                               Ngay = (DateTime)t.Ngay,
                           }).OrderByDescending(x => x.Ngay).ToList();
                return res;
            }


        }

    }
}