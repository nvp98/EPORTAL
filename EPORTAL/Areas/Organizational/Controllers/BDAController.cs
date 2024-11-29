using SoDoToChuc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsOrganizational;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public class BDAController : Controller
    {
        // GET: BDA
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        public ActionResult Index(int? id)
        {
            if (id != null)
            {
                var re = db_context.PhongBans.Where(x => x.IDPhongBan == id).SingleOrDefault();

                KPVSXValidation rec = new KPVSXValidation()
                {
                    IDPhongBan = (int)id,
                    TenPhongBan = re.TenPhongBan
                };
                Session["nav"] = "BDA";
                ViewData["id"] = rec;
                Session["idpb"] = id;

                var tongns = db_context.NhanVienAPIs.Where(x => x.IDPhongBan == id && x.IDTinhTrangLV == 1).Count();
                var nshc = db_context.NhanVienAPIs.Where(x => x.IDPhongBan == id && x.IDTinhTrangLV == 1 && x.IDCa == 1).Count();
                var ns1c = tongns - nshc;
                var tolv = db_context.ToLVs.Where(x => x.PhongBanID == id).Count();

                getNS dataNS = new getNS()
                {
                    TongNS = tongns,
                    nshc = nshc,
                    ns1ca = ns1c,
                    tongtolv = tolv,
                };

                ViewBag.DataBDA = dataNS;

            }

            //var res = (from a in db_context.PhanXuongs.Where(x => x.PhongBanID == id)
            //           join b in db_context.NhanViens.Where(x => x.IDPhongBan == id) on a.PhongBanID equals b.IDPhongBan
            //           select new NMNLValidation()
            //           {
            //               IDPhanXuong = a.IDPhanXuong,
            //               TenPhanXuong = a.TenPhanXuong,
            //               PhongBanID = (int)a.PhongBanID,
            //               HoTen = b.HoTen
            //           });
            return View();
        }
    }
}