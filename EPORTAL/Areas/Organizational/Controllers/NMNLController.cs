using SoDoToChuc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using EPORTAL.ModelsOrganizational;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public class NMNLController : Controller
    {
        // GET: NMNL
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        public ActionResult Index(int? id)
        {
            if (id != null)
            {
                var re = db_context.PhongBans.Where(x => x.IDPhongBan == id).SingleOrDefault();
                          
                getBGDV rec = new getBGDV()
                {
                    PhongBanID = (int)id,
                    ImagePath =re.ImagePath
                };
                Session["nav"] = "KSX";
                ViewData["id"] = rec;
                Session["idpb"] = id;

                var tongns = db_context.NhanVienAPIs.Where(x => x.IDPhongBan == id && x.IDTinhTrangLV == 1).Count();
                var nshc = db_context.NhanVienAPIs.Where(x => x.IDPhongBan == id && x.IDTinhTrangLV == 1 && x.IDCa == 1).Count();
                var ns1c = tongns -nshc;
                var tolv = db_context.ToLVs.Where(x => x.PhongBanID == id).Count();

                getNS dataNS = new getNS()
                {
                    TongNS = tongns,
                    nshc = nshc,
                    ns1ca = ns1c,
                    tongtolv = tolv,
                };

                ViewBag.DataNS = dataNS;

            }

            //var res = (from a in db_context.PhanXuongs.Where(x => x.PhongBanID == id)
            //           join b in db_context.NhanViens.Where(x=>x.IDPhongBan == id ) on a.PhongBanID equals b.IDPhongBan
            //           select new NMNLValidation()
            //           {
            //               IDPhanXuong = a.IDPhanXuong,
            //               TenPhanXuong = a.TenPhanXuong,
            //               PhongBanID = (int)a.PhongBanID,
            //               HoTen =b.HoTen
            //           });


            //var result = (from b in db_context.NhanViens.Where(x => x.IDPhongBan == id && x.IDViTri == 15)
            //              select new getBGDV()
            //              {
            //                  PhongBanID = (int)b.IDPhongBan,
            //                  HoTen = b.HoTen
            //              });
            //var tup = new Tuple<NMNLValidation, getBGDV>(res, result);
            return View();
        }
        //public ActionResult GetBGD(int? id)
        //{
        //    var query = db_context.NhanViens.Where(x => x.IDPhongBan == id )
        //        .GroupBy(p =>p.HoTen)
        //        .Select(x => new { name = x.Key }).ToList();
        //    return Json(query, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetDataToDB(int? idpx,int? idpb)
        {
            var dstopx = db_context.ToLVs.Where(x => x.PhanXuongID == idpx && x.PhongBanID == idpb).ToList();
            var dsnvpx = db_context.NhanVienAPIs.Where(x => x.IDPhanXuong == idpx && x.IDPhongBan == idpb && x.IDTinhTrangLV == 1).ToList();
            List<dstolv> dsto = new List<dstolv>(); 
            foreach(var item in dstopx)
            {
                var nstt = dsnvpx.Where(x => x.IDToLV == item.IDTo).Count();
                dstolv aa = new dstolv()
                {
                    TenTo = item.TenTo,
                    sldb = (int?)item.SLDB ?? 0,
                    sltt = (int?)nstt ?? 0
                };
                dsto.Add(aa);
            }
            //var query = db_context.ToLVs.Where(x => x.PhanXuongID == idpx && x.PhongBanID == idpb)
            //    .GroupBy(p => p.TenTo)
            //    .Select(x => new { name = x.Key, sldb = x.Sum(a => a.SLDB), sltt = x.Sum(a => a.SLTT) }).ToList();
            return Json( dsto , JsonRequestBehavior.AllowGet);
        }
        public ActionResult getDataNM(int? idpb)
        {
            var tongns = db_context.NhanVienAPIs.Where(x => x.IDPhongBan == idpb && x.IDTinhTrangLV == 1).Count();
            var nshc =db_context.NhanVienAPIs.Where(x => x.IDPhongBan == idpb && x.IDTinhTrangLV == 1&& x.IDCa ==1).Count();
            var ns1c = tongns - nshc;
            var tolv =db_context.ToLVs.Where(x => x.PhongBanID ==idpb).Count();
            getNS query = new getNS()
            {
                TongNS = tongns,
                nshc = nshc,
                ns1ca = ns1c,
                tongtolv = tolv,
            };
            return Json(query, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDataPXDB(int? idpx, int? idpb)
        {
           //get sl nhan vien
            int tptt = 0, qdtt = 0, pttt = 0, tpkip = 0, ktvtt = 0, tttp = 0, nvtt = 0,nvhc = 0;
            var tongnspx = db_context.NhanVienAPIs.Where(x => x.IDTinhTrangLV == 1 && x.IDPhanXuong ==idpx);
            tptt = tongnspx.Where(x => x.IDLoai == 2 || x.IDLoai == 14).Count();
            qdtt = tongnspx.Where(x => x.IDLoai == 3 || x.IDLoai == 11 || x.IDLoai == 12).Count();
            pttt = tongnspx.Where(x => x.IDLoai == 4 || x.IDLoai == 13).Count();
            tpkip = tongnspx.Where(x => x.IDLoai == 9 || x.IDLoai == 10).Count();
            ktvtt = tongnspx.Where(x => x.IDLoai == 5).Count();
            tttp = tongnspx.Where(x => x.IDLoai == 7 || x.IDLoai == 8).Count();
            nvtt = tongnspx.Where(x => x.IDLoai == 6 && x.IDCa != 1).Count();
            nvhc = tongnspx.Where(x => x.IDLoai == 6 && x.IDCa == 1).Count();

            var query = db_context.PhanXuongs.Where(x => x.IDPhanXuong == idpx && x.PhongBanID == idpb)
                //.GroupBy(p => p.)
                .Select(x => new { ktvdb = x.KTVDB, tpdb=x.TPDB,nvdb=x.NVDB,ptdb=x.PTDB,tpkdb = x.TPKip, nvhcdb =x.NVHCDB, ktvtt=ktvtt,tptt=qdtt,pttt=pttt,nvtt=nvtt,nvhctt=nvhc,tpktt=tpkip }).ToList();
            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataNhomLV(int? idnhom, int? idpb)
        {
            //get sl nhan vien
            int tptt = 0, tptqdtt = 0, pttt = 0, tpkip = 0, ktvtt = 0, tttp = 0, nvtt = 0,nvhc =0;
            var tongnsnhom = db_context.NhanVienAPIs.Where(x => x.IDTinhTrangLV == 1 && x.IDNhomLV == idnhom);
            //var bpvpc = db_context.PhongBans.Where(x => x.NhomID == 1 && x.IDPhongBan ==idpb).ToList();
            //tptt = tongnsnhom.Where(x => x.IDLoai == 2 || x.IDLoai == 14).Count();
            tptqdtt = tongnsnhom.Where(x => x.IDLoai == 3 || x.IDLoai == 11 || x.IDLoai == 12).Count();
            pttt = tongnsnhom.Where(x => x.IDLoai == 4 || x.IDLoai == 13).Count();
            tpkip = tongnsnhom.Where(x => x.IDLoai == 9 || x.IDLoai == 10).Count();
            ktvtt = tongnsnhom.Where(x => x.IDLoai == 5).Count();
            tttp = tongnsnhom.Where(x => x.IDLoai == 7 || x.IDLoai == 8).Count();
            nvtt = tongnsnhom.Where(x => x.IDLoai == 6 && x.IDCa !=1).Count(); 
            nvhc = tongnsnhom.Where(x => x.IDLoai == 6 && x.IDCa ==1).Count();
            var query = db_context.NhomLVs.Where(x => x.IDNhom == idnhom && x.IDPhongBan == idpb)
                //.GroupBy(p => p.)
                .Select(x => new { tptdb = x.TPTDB, ptdb = x.PTDB, tpkdb = x.TPKipDB, ktvdb = x.KTVDB,tttpdb=x.TTTPDB, nvdb = x.NVCa,nvhcdb =x.NVDB,tpttt=tptqdtt, pttt = pttt, ktvtt = ktvtt, nvtt = nvtt,nvhctt =nvhc, tpktt = tpkip }).ToList();
            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PXDetail(int? id)
        {
            return PartialView();
        }
    }


}