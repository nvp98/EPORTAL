using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class InfoPartnerController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: InfoPartner
        public ActionResult Index(int? page, int id)
        {
            var res = (from d in db_context.DB_TTDoiTac

                       select new ManagePartnerValidation
                       {
                           IDDoiTac = d.IDDoiTac,
                           IDBP = d.BPID,
                           LinhVucHĐ = d.LinhVucHĐ,
                           // LoaiIDDT = (string)l.IDLoaiDT,
                           //TenLVHĐ = e.TenLinhVucHĐ,
                           //TenLoaiDT = l.TenLoaiDT,
                           FullName = d.FullName,
                           ShortName = d.ShortName,
                           TaxCode = d.Taxcode,
                           Address = d.Address,
                           City = (int)d.City,
                           Regions = (int)d.Regions,
                           Customer = d.Customer,
                           ImageLogo = d.ImageLogo,
                           Vender = d.Vender,

                       }).Where(x => x.IDDoiTac == id).ToList();
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
    }
}