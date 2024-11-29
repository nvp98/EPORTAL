using EPORTAL.Models;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    public class ListProjectController : Controller
    {
        // GET: View360/ListProject
        EPORTALEntities db = new EPORTALEntities();
        public ActionResult Index(int? page, string search, int? id)
        {
            if (search == null) search = "";
            ViewBag.search = search;
   
            var res = (from a in db.Project_select_USER(search, MyAuthentication.ID).Where(x=>x.IDGroup == id)
                      select new ProjectValidation
                      {
                          ID = a.ID,
                          Title = a.Title,
                          Images = a.Images,
                          URL = a.URL,
                          Date = (DateTime)a.Date,
                          Note = a.Note,
                          FilePDF = a.FilePDF,
                          IDPhongBan = (int)a.IDPhongBan,
                          //TenPhongBan = a.TenPhongBan
                      });

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.Date).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Details(int id)
        {
            var res = (from a in db.Projects.Where(x => x.ID == id)
                       select new ProjectValidation
                       {
                           ID = a.ID,
                           URL = a.URL,
                           Date = (DateTime?)a.Date ?? DateTime.Now,
                           Note = a.Note,
                           IDPhongBan = (int)a.IDPhongBan,
                           FilePDF = a.FilePDF,
                           Images = a.Images,
                           Title = a.Title
                       }).ToList();
            ProjectValidation DO = new ProjectValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.URL = a.URL;
                    DO.Date = a.Date;
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.FilePDF = a.FilePDF;
                    DO.Images = a.Images;
                    DO.Title = a.Title;
                    DO.Note = a.Note;
                }
            }
            else
            {
                HttpNotFound();
            }
            return View(DO);
        }
    }
}