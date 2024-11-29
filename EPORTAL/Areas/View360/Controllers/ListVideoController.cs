using EPORTAL.Models;
using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    public class ListVideoController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        // GET: View360/ListVideo
        public ActionResult Index(int? id, string search, int? idalbum)
        {
            if (search == null) search = "";
            ViewBag.search = search;

            var video = from a in db.Video_select(search, MyAuthentication.ID)
                      select new VideoValidation
                      {
                          IDVideo = a.IDVideo,
                          Title = a.Title,
                          Images = a.Images,
                          URL = a.URL,
                          Date = (DateTime)a.Date,
                          Note = a.Note,
                          IDPhongBan = (int)a.IDPhongBan,
                          AlbumID = (int)a.AlbumID
                      };
            if (id != null)
                {
                    var res1 = db.Videos.Where(x => x.IDVideo == id).SingleOrDefault();
                    VideoValidation rec1 = new VideoValidation()
                    {
                        IDVideo = (int)id,
                        Title = res1.Title
                    };
                    ViewData["id"] = rec1;
                }
            if(idalbum != null)
            {
                var al = db.Albums.Where(x => x.IDAlbum == idalbum).SingleOrDefault();
                AlbumValidation album = new AlbumValidation()
                {
                    IDAlbum = (int)idalbum,
                    TenAlbum = al.TenAlbum
                };
                ViewData["idal"] = album;
            }
            return View(video.OrderByDescending(x => x.Date).ToList());
        }
    }
}