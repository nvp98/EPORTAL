using EPORTAL.Models;
using EPORTAL.ModelsServey;
using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace EPORTAL.Areas.Servey.Controllers
{
    public class UserServeyController : Controller
    {
        // GET: Servey/UserServey
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_SERVEYEntities dbSV = new EPORTAL_SERVEYEntities();
        public ActionResult Index()
        {
            var lisSV = dbSV.EmployeeServeys.Where(x=>x.IDNV == MyAuthentication.ID).ToList();
            var sv = dbSV.ListServeys.ToList();
            var res = (from a in sv.Where(x=>x.StartTime <= DateTime.Now && x.EndTime >= DateTime.Now && x.StatusSV ==true)
                       join b in lisSV on a.IDSV equals b.IDSV
                       select new UserServeyValidation
                      {
                          IDSV = (int)a.IDSV,
                          ContentSV = a.ContentSV,
                          StartTime = (DateTime)a.StartTime,
                          EndTime = (DateTime)a.EndTime,
                          StatusSV = (Boolean)a.StatusSV,
                          Exp = (TimeSpan)((DateTime)a.EndTime - DateTime.Now.AddDays(-2)),
                          CountSV = dbSV.EmployeeServeys.Where(x=>x.IDSV ==a.IDSV && x.OTID != null ).Count(),
                      }).ToList();

            return View(res.OrderByDescending(x => x.StartTime).ToList());
        }
    }
}