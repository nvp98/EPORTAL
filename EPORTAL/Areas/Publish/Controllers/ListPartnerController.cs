using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class ListPartnerController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: PartnerInformation
        public ActionResult Index(int? page)
        {
            var res = (from d in db_context.DB_TTDoiTac
                       join f in db_context.DB_DiaChi on d.City equals f.IDDiaChi
                       select new ManagePartnerValidation
                       {
                           IDDoiTac = d.IDDoiTac,
                           IDBP = d.BPID,
                           LinhVucHĐ = d.LinhVucHĐ,

                           LoaiIDDT = d.LoaiIDDT,

                           FullName = d.FullName,
                           ShortName = d.ShortName,
                           TaxCode = d.Taxcode,
                           Address = d.Address,
                           City = (int)d.City,
                           Regions = (int)d.Regions,
                           Customer = d.Customer,

                           Vender = d.Vender,
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
    }
}