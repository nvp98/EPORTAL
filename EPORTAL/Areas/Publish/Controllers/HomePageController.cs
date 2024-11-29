using EPORTAL.ModelsDanhBaDoiTac;
using EPORTAL.ModelsPublish;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class HomePageController : Controller
    {
            DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
            EPORTAL_DOCUMENTEntities db_doc = new EPORTAL_DOCUMENTEntities();
            public ActionResult Index(int? page)
            {
                var res = (from ct in db_doc.VB_DetailDocument
                           select new Detail_DocumentValidation
                           {
                               IDCTVB = ct.IDCTVB,
                               IDVB = (int)ct.IDVB,
                               SoVB = ct.SoVB,
                               NoiDungVB = ct.NoiDungVB,
                               NgayVB = (DateTime)ct.NgayVB,
                               NgayBĐ = (DateTime?)ct.NgayBĐ ?? default,
                               NgayKT = (DateTime?)ct.NgayKT ?? default,
                               TenFile = ct.TenFile,
                               Status = (int?)ct.Status ?? default,
                           }).OrderBy(x => x.NgayVB).ToList();

                if (page == null) page = 1;
                int pageSize = 12;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }

            public ActionResult IntroduceHPDQ()
            {
                return View();
            }
            public ActionResult Instruct(int? page)
            {
                var res = (from d in db_doc.VB_Document.Where(x => x.IDLoai == 1)
                           select new DocumentValidation
                           {
                               IDVB = d.IDVB,
                               TenVB = d.TenVB,
                               NgayCapNhat = (DateTime)d.NgayCapNhat,
                               NhanVienID = (int)d.NhanVienID,
                               IDPage = (int)d.IDPage,
                               IDLoai = (int)d.IDLoai

                           }).OrderBy(x => x.IDPage).ToList();
                if (page == null) page = 1;
                int pageSize = 12;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }
            public ActionResult Video()
            {
                return View();
            }
            public ActionResult ISO(int? page)
            {
                var res = (from d in db_doc.VB_Document.Where(x => x.IDLoai == 2)
                           select new DocumentValidation
                           {
                               IDVB = d.IDVB,
                               TenVB = d.TenVB,
                               NgayCapNhat = (DateTime)d.NgayCapNhat,
                               NhanVienID = (int)d.NhanVienID,
                               IDPage = (int)d.IDPage,
                               IDLoai = (int)d.IDLoai

                           }).OrderBy(x => x.IDPage).ToList();
                if (page == null) page = 1;
                int pageSize = 3;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }
            public ActionResult News(int? id, int? page)
            {

                var res = (from l in db_context.DB_TinTuc
                           select new ManageNewsValidation
                           {
                               IDTinTuc = l.IDTintuc,

                               TenDeTai = l.TenDeTai,
                               TomTatTinTuc = l.TomTatTinTuc,
                               NoiDungTinTuc = l.NoiDungTinTuc,
                               ImageTinTuc = l.ImageTinTuc,
                               NgayDangTin = (DateTime)l.NgayDangTin,
                               NguoiDangTin = l.NguoiDangTin
                           }).OrderByDescending(x => x.IDTinTuc).ToList();
                if (page == null) page = 1;
                int pageSize = 12;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));

            }
            public ActionResult DetailedNews(int? id, int? page)
            {

                var model = db_context.DB_TinTuc.Where(x => x.IDTintuc == id).FirstOrDefault();
                var res = (from l in db_context.DB_TinTuc
                           select new ManageNewsValidation
                           {
                               IDTinTuc = l.IDTintuc,

                               TenDeTai = l.TenDeTai,
                               TomTatTinTuc = l.TomTatTinTuc,
                               NoiDungTinTuc = l.NoiDungTinTuc,
                               ImageTinTuc = l.ImageTinTuc,
                               NgayDangTin = (DateTime)l.NgayDangTin,
                               NguoiDangTin = l.NguoiDangTin
                           }).OrderByDescending(x => x.IDTinTuc).Where(x => x.IDTinTuc == id).ToList();
                if (page == null) page = 1;
                int pageSize = 3;
                int pageNumber = (page ?? 1);
                return View(res.ToList().ToPagedList(pageNumber, pageSize));
            }
            public ActionResult About()
            {
                ViewBag.Message = "Your application description page.";

                return View();
            }

            public ActionResult Contact()
            {
                ViewBag.Message = "Your contact page.";

                return View();
            }

            public List<DanhBaDoiTacEntities> listFoundArticles(string titl)
            {
                object[] parameter = {
                new SqlParameter("@NoiDungTinTuc", titl)
            };
                var list = db_context.Database.SqlQuery<DanhBaDoiTacEntities>("@NoiDungTinTuc", parameter).ToList();
                list.Reverse();
                return list;
            }
        }
    }