using SoDoToChuc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsOrganizational;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public class KPVSXController : Controller
    {
        // GET: KPVSX
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        public ActionResult Index(int? id )
        {
            if (id != null)
            {
                var res =  db_context.PhongBans.Where(x => x.IDPhongBan == id).SingleOrDefault();
                KPVSXValidation rec = new KPVSXValidation()
                {
                    IDPhongBan = (int)id,
                    TenPhongBan = res.TenPhongBan
                };
                ViewData["id"] = rec;
                Session["nav"] = "KPVSX";
                Session["idpb"] = id;
            }

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

            ViewBag.DataNS = dataNS;
            //var res =from a in db_context.NhanVienPVSXes.Where(x =>x.IDPhongBan ==id)
            //         join b in db_context.ChucVus on a.IDChucVu equals b.IDChucVu
            //         select new KPVSXValidation()
            //         {

            //         }
            return View();
        }
        public ActionResult GetNSTTPB(int?idpb)
        {

            //var aqd = db_context.MaViTris.Where(x => x.IDLoai == 3 || x.IDLoai == 11|| x.IDLoai == 12 || x.IDLoai == 13).ToList();
            //var a = db_context.MaViTris.Where(x => x.IDLoai == 4 || x.IDLoai == 9 || x.IDLoai == 10).ToList();
            //var lsktv = db_context.MaViTris.Where(x => x.IDLoai == 5).ToList();
            //var lsnv = db_context.MaViTris.Where(x => x.IDLoai == 6 || x.IDLoai == 7 || x.IDLoai == 8).ToList();

            int tptt = 0, qdtt = 0, pttt = 0, tpkip = 0, ktvtt = 0, tttp = 0, nvhctt = 0,nvcakip =0;
            var tongnscty = db_context.NhanVienAPIs.Where(x => x.IDPhongBan ==idpb && x.IDTinhTrangLV == 1);
            tptt = tongnscty.Where(x => x.IDLoai == 2 ).Count();
            qdtt = tongnscty.Where(x => x.IDLoai == 3 || x.IDLoai == 11 || x.IDLoai == 12||x.IDLoai ==14).Count();
            pttt = tongnscty.Where(x => x.IDLoai == 4 || x.IDLoai == 13).Count();
            tpkip = tongnscty.Where(x => x.IDLoai == 9 || x.IDLoai == 10).Count();
            ktvtt = tongnscty.Where(x => x.IDLoai == 5).Count();
            tttp = tongnscty.Where(x => x.IDLoai == 7 || x.IDLoai == 8).Count();
            nvhctt = tongnscty.Where(x => x.IDLoai == 6 &&x.IDCa ==1).Count();
            nvcakip = tongnscty.Where(x => x.IDLoai == 6 && x.IDCa != 1).Count();

            TotalList query = new TotalList()
            {
                TPTotal = (int)tptt,
                QDTotal = qdtt,
                PTTotal = pttt,
                TPKipTotal = tpkip,
                KTVTotal = ktvtt,
                TTTPTotal = tttp,
                NVVPC = nvhctt,
                NVTotal = nvcakip
                
            };

            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNSDBBP(int?idpb)
        {
            //int tptotal = 0, qdtotal = 0, pttotal = 0, tpkiptotal = 0, ktvtotal = 0, tttptotal = 0, nvtotal = 0;
            var pb = db_context.PhongBans.Where(x => x.IDPhongBan == idpb);
            int tptotal = (int?)pb.Sum(x=>x.TPDB)??0;
            var qdtotal = (int?)pb.Sum(x => x.QDTPTDB) ?? 0;
            var pttotal =  (int?)pb.Sum(x => x.PTDB) ?? 0;
            var tpkiptotal = (int?)pb.Sum(x => x.TPKipDB) ?? 0;
            var ktvtotal = (int?)pb.Sum(x => x.KTVHCDB??0)+ (int?)pb.Sum(x => x.KTVCaDB??0);
            var tttptotal = (int?)pb.Sum(x => x.TTTPHCDB??0)+ (int?)pb.Sum(x => x.TTTPCaDB ?? 0);
            var nvvpc =  (int?)pb.Sum(x => x.NVHCDB ?? 0);
            var nvcakip = (int?)pb.Sum(x => x.NVCaDB ?? 0);

            TotalList query = new TotalList()
            {
                TPTotal = tptotal,
                QDTotal = (int)qdtotal,
                PTTotal = (int)pttotal,
                TPKipTotal = (int)tpkiptotal,
                KTVTotal = (int)ktvtotal,
                TTTPTotal = (int)tttptotal,
                NVVPC = (int)nvvpc,
                NVTotal =(int)nvcakip 
            };

            return Json(query, JsonRequestBehavior.AllowGet);
        }

    }
}