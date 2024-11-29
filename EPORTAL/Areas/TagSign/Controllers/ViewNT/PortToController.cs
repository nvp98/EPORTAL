using EPORTAL.Models;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class PortToController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        // GET: TagSign/PortTo
        public ActionResult Index(int? page)
        {
          
            var res = from a in db.NT_Gate
                      select new GateValidation
                      {
                          IDGate = (int)a.IDGate,
                          Gate = a.Gate
                      };
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDGate).ToList().ToPagedList(pageNumber, pageSize));
        }
    }
}