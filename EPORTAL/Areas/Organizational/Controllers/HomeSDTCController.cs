using SoDoToChuc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsOrganizational;
using EPORTAL.Models;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public class HomeSDTCController : Controller
    {
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "HomeSDTC";
        public ActionResult Index()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            Session["nav"] = "";
            Session["idpb"] = "";

            var tongns = db_context.NhanVienAPIs.Where(x =>  x.IDTinhTrangLV == 1).Count();
            var nshc = db_context.NhanVienAPIs.Where(x =>  x.IDTinhTrangLV == 1 && x.IDCa == 1).Count();
            var ns1c = tongns -nshc;
            //var tolv = db_context.ToLVs.Where(x => x.PhongBanID == id).Count();

            getNS dataNS = new getNS()
            {
                TongNS = tongns,
                nshc = nshc,
                ns1ca = ns1c,
                //tongtolv = tolv,
            };

            ViewBag.DataNS = dataNS;

            //ds BGĐ
            var bgd = (from a in db_context.NhanVienAPIs.Where(x => x.IDPhongBan == 1 && x.IDTinhTrangLV == 1)
                       select new dataBGD
                       {
                           HoTen =a.HoTen,
                           TenViTri =a.ViTriLViec.TenViTri,
                           ImagePath = a.ImagePath,
                           TT_BGD =(int?)a.TT_BGD??0
                       }).ToList().OrderBy(x=>x.TT_BGD);
            ViewBag.dataBGD = bgd;
            //thong tin phong ban
            return View();
        }
        public JsonResult getDataBP(int idpb )
        {
            var ns = db_context.NhanVienAPIs.Where(x => x.IDPhongBan == idpb && x.IDTinhTrangLV == 1);
            var tenpb = db_context.PhongBans.Where(x => x.IDPhongBan == idpb).Select(x=>x.TenPhongBan).SingleOrDefault();

            List <dataBGD> dstpbp = (from a in db_context.NhanVienAPIs.Where(x => x.IDPhongBan == idpb && x.IDTinhTrangLV == 1 && (x.MaViTri == "TBP" || x.MaViTri == "PBP")).OrderBy(x => x.TT_BGD)
                     
                       select new dataBGD
                       {
                           HoTen = a.HoTen,
                           TenViTri = a.ViTriLViec.TenViTri,
                           ImagePath = a.ImagePath,
                           TT_BGD = (int?)a.TT_BGD ?? 0
                       }).OrderBy(x=>x.TT_BGD).ToList();
            var dsn = db_context.NhomLVs.Where(x => x.IDPhongBan == idpb && (x.IDNhomPT == null ||x.IDNhomPT ==0));
            
            //số lượng QĐ, TPT
            int slqdpb = 0, sltptpb = 0;
            slqdpb = ns.Where(x => x.IDLoai == 3 || x.IDLoai == 11).Count();
            sltptpb = ns.Where( x => x.IDLoai == 12 || x.IDLoai == 13).Count();
            //foreach (var it in aqd)
            //{
            //    var aa = ns.Where(x => x.MaViTri == it.TenMaViTri).Count();
            //    slqdpb = slqdpb + aa;
            //}

            //danh sách nhóm
            List<dsnhom> dsnlv = new List<dsnhom>();
            List<dataBGD> listdatapt = new List<dataBGD>();
            List<dataBGD> listdatatpt = new List<dataBGD>();
            List<nhomchitiet> listchitiets = new List<nhomchitiet>();
            List<nhomchitiet> listtos = new List<nhomchitiet>();

            foreach (var item in dsn)
            {
                int slpt = 0,slktv = 0,slnv=0, sltpkip = 0;

                var dsnvnhom = db_context.NhanVienAPIs.Where(x => x.IDNhomLV == item.IDNhom && x.IDTinhTrangLV ==1);
                slpt = dsnvnhom.Where(x => x.IDLoai == 4).Count();
                slktv = dsnvnhom.Where(x => x.IDLoai == 5).Count();
                sltpkip = dsnvnhom.Where(x => x.IDLoai == 9 || x.IDLoai == 10).Count();

                //var tptc = dsnvnhom.Where(x => x.IDLoai == 12||)?.Count() ?? 0;
                listdatapt = (from a in dsnvnhom.Where(x=>x.IDLoai ==4 ||x.IDLoai ==13)
                              
                              select new dataBGD
                              {
                                  HoTen = a.HoTen,
                                  TenViTri = a.ViTriLViec.TenViTri,
                                  ImagePath = a.ImagePath, 
                              }).ToList();
                listdatatpt= (from a in dsnvnhom.Where(x =>  x.IDLoai == 12 ||x.IDLoai ==14 )
                            
                              select new dataBGD
                              {
                                  HoTen = a.HoTen,
                                  TenViTri = a.ViTriLViec.TenViTri,
                                  ImagePath = a.ImagePath,
                              }).ToList();
                //var dsnhomchitiet = db_context.NhanVienAPIs.Where(x => x.IDNhomLV == item.IDNhom && x.IDTinhTrangLV == 1);
                // List danh sách nhóm chi tiết
                listchitiets = (from a in db_context.NhomLVs.Where(x => x.IDNhomPT == item.IDNhom)
                               select new nhomchitiet
                               {
                                   TenNhom =a.TenNhom,
                                   slktv = ns.Where(x => x.IDNhomLV == a.IDNhom && x.IDLoai == 5).Count(),
                                   slnv = ns.Where(x => x.IDNhomLV == a.IDNhom && x.IDLoai ==6).Count(),
                                   tongns = ns.Where(x => x.IDNhomLV == a.IDNhom  ).Count(),
                                   sltttp = ns.Where(x => x.IDNhomLV == a.IDNhom &&(x.IDLoai == 8|| x.IDLoai == 7)).Count(),
                                   slpt = ns.Where(x => x.IDNhomLV == a.IDNhom && (x.IDLoai == 4 || x.IDLoai == 13)).Count(),
                                   listpt = (from b in ns.Where(x => x.IDNhomLV == a.IDNhom && (x.IDLoai == 4 || x.IDLoai == 13))
                                            
                                             select new dataBGD
                                             {
                                                 HoTen = b.HoTen,
                                                 TenViTri = b.ViTriLViec.TenViTri,
                                                 ImagePath = b.ImagePath,
                                             }).ToList(),
                               }).ToList();
                // List danh sách tổ thuộc mảng công việc
                listtos = (from a in db_context.ToLVs.Where(x => x.IDNhomPT == item.IDNhom)
                           select new nhomchitiet
                           {
                               TenNhom =a.TenTo,
                               slktv = ns.Where(x => x.IDToLV == a.IDTo  && x.IDLoai == 5).Count(),
                               slnv = ns.Where(x => x.IDToLV == a.IDTo  && x.IDLoai == 6).Count(),
                               tongns = ns.Where(x => x.IDToLV == a.IDTo).Count(),
                               sltttp = ns.Where(x => x.IDToLV == a.IDTo && (x.IDLoai == 8 || x.IDLoai == 7)).Count(),
                               slpt = ns.Where(x => x.IDToLV == a.IDTo && (x.IDLoai == 4 || x.IDLoai == 13)).Count(),
                               listpt = (from b in ns.Where(x => x.IDToLV == a.IDTo && (x.IDLoai == 4 || x.IDLoai == 13))
                                        
                                         select new dataBGD
                                         {
                                             HoTen = b.HoTen,
                                             TenViTri = b.ViTriLViec.TenViTri,
                                             ImagePath = b.ImagePath,
                                         }).ToList(),
                           }
                           ).ToList();
                slnv = dsnvnhom.Count() - slktv - slpt;
                dsnhom anhom = new dsnhom()
                {
                    TenNhom = item.TenNhom,
                    slpt = slpt,
                    slktv = slktv,
                    slnv = slnv,
                    sltpk =sltpkip,
                    listpt = listdatapt,
                    IDnhom = item.IDNhom,
                    TPT = listdatatpt.Count(),
                    listtpt = listdatatpt,
                    tongns = slpt + slktv + slnv + listdatatpt.Count(),
                    listchitiet =listchitiets,
                    listto =listtos
                };
                dsnlv.Add(anhom);
            }

            //khối sản xuất
            //var check = db_context.PhongBans.Where(x => x.IDPhongBan == idpb && x.NhomID == 2).Count();
            List<dspx> dspxlv = new List<dspx>();
            List<dataBGD> listdataqd = new List<dataBGD>();
            var dspx = db_context.PhanXuongs.Where(x => x.PhongBanID == idpb);
            if (dspx.Count() > 0)
            {
                foreach (var item in dspx)
                {
                    var atpk = db_context.MaViTris.Where(x => x.IDLoai == 9 || x.IDLoai == 10).ToList();
                    int slqd = 0, slpt = 0, sltpk = 0, slktv = 0,sltt =0, slnv = 0;
                    var dsnvpx = db_context.NhanVienAPIs.Where(x => x.IDPhanXuong == item.IDPhanXuong && x.IDTinhTrangLV == 1);
                    slqd = dsnvpx.Where(x => x.IDLoai == 3 || x.IDLoai == 11).Count();
                    sltpk = dsnvpx.Where(x => x.IDLoai == 9 || x.IDLoai == 10).Count();
                    slpt = dsnvpx.Where(x => x.IDLoai == 4).Count();
                    slktv = dsnvpx.Where(x => x.IDLoai == 5).Count();
                    sltt = dsnvpx.Where(x => x.IDLoai == 7 || x.IDLoai == 8).Count();
                    listdataqd = (from a in dsnvpx.Where(x => x.IDLoai == 3||x.IDLoai ==11)
                                  
                                  select new dataBGD
                                  {
                                      HoTen = a.HoTen,
                                      TenViTri = a.ViTriLViec.TenViTri,
                                      ImagePath = a.ImagePath,
                                  }).ToList();

                    slnv = dsnvpx.Count() - slqd - sltpk - slpt - slktv;
                    dspx apx = new dspx()
                    {
                        TenPhanXuong = item.TenPhanXuong,
                        slqd = slqd,
                        slpt = slpt,
                        sltpk = sltpk,
                        slktv = slktv,
                        sltttp =sltt,
                        slnv = slnv,
                        listqd = listdataqd,
                        tongns = dsnvpx.Count(),
                    };
                    dspxlv.Add(apx);
                }
            }
            dataBP getDataBPLV = new dataBP()
            {
                tenpb = tenpb,
                tongns = ns.Count(),
                listtp = dstpbp,
                listnhom = dsnlv,
                listpx = dspxlv,
                slqdpb =slqdpb,
                sltptpb =sltptpb,
            };


            return Json(getDataBPLV,JsonRequestBehavior.AllowGet);
        }
        public int GetIDLoai(string TenLoai)
        {
            var model = db_context.LoaiKHs.Where(x => x.TenLoaiKH == TenLoai).SingleOrDefault();
            if (model == null)
                return 0;
            return model.IDLoai;
        }
        public ActionResult GetDataNS()
        {
            
            //var aqd = db_context.MaViTris.Where(x => x.IDLoai == 3 || x.IDLoai == 11|| x.IDLoai == 12 || x.IDLoai == 13).ToList();
            //var a = db_context.MaViTris.Where(x => x.IDLoai == 4 || x.IDLoai == 9 || x.IDLoai == 10).ToList();
            //var lsktv = db_context.MaViTris.Where(x => x.IDLoai == 5).ToList();
            //var lsnv = db_context.MaViTris.Where(x => x.IDLoai == 6 || x.IDLoai == 7 || x.IDLoai == 8).ToList();

            int tptt=0,qdtt=0,pttt = 0,tpkip =0,ktvtt =0,tttp=0,nvtt=0,pbgpmb=0,nvhctt=0;
            var bpvpc = db_context.PhongBans.Where(x => x.NhomID == 1).ToList();
            var tongnscty = db_context.NhanVienAPIs.Where(x => x.IDTinhTrangLV == 1);
            tptt =tongnscty.Where(x => x.IDLoai == 2 ).Count();
            qdtt = tongnscty.Where(x => x.IDLoai == 3 || x.IDLoai == 11 || x.IDLoai == 12).Count();
            pttt = tongnscty.Where(x => x.IDLoai == 4 || x.IDLoai == 13 ).Count();
            tpkip =tongnscty.Where(x => x.IDLoai == 9 || x.IDLoai == 10).Count();
            ktvtt = tongnscty.Where(x => x.IDLoai == 5).Count();
            tttp =tongnscty.Where(x => x.IDLoai == 7||x.IDLoai==8).Count();
            nvtt =tongnscty.Where(x => x.IDLoai == 6 && x.IDCa !=1).Count();
            pbgpmb = tongnscty.Where(x => x.IDLoai == 14).Count();
            nvhctt =tongnscty.Where(x => x.IDLoai == 6 && x.IDCa ==1).Count();
            //foreach(var it in bpvpc)
            //{
            //    var n =tongnscty.Where(x => x.IDPhongBan == it.IDPhongBan && x.IDLoai == 6).Count();
            //    nvhctt = nvhctt + n;
            //}
            //nvtt = nvtt - nvhctt;

            TotalList query = new TotalList()
            {
                TPTotal = (int)tptt,
                QDTotal = qdtt +pbgpmb,
                PTTotal = pttt,
                TPKipTotal =tpkip,
                KTVTotal = ktvtt,
                TTTPTotal =tttp,
                NVTotal = nvtt,
                NVVPC =nvhctt,
                PBGPMB =pbgpmb
            };
             
            return Json(query, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDataNSDB()
        {
            //int tptotal = 0, qdtotal = 0, pttotal = 0, tpkiptotal = 0, ktvtotal = 0, tttptotal = 0, nvtotal = 0;
            var tptotal = db_context.PhongBans.Select(x => x.TPDB).DefaultIfEmpty(0).Sum();
            var qdtotal = db_context.PhongBans.Select(x => x.QDTPTDB).DefaultIfEmpty(0).Sum();
            var pttotal = db_context.PhongBans.Select(x => x.PTDB).DefaultIfEmpty(0).Sum();
            var tpkiptotal = db_context.PhongBans.Select(x=>x.TPKipDB).DefaultIfEmpty(0).Sum();
            var ktvtotal = db_context.PhongBans.Select(x => x.KTVHCDB).DefaultIfEmpty(0).Sum() + db_context.PhongBans.Select(x => x.KTVCaDB).DefaultIfEmpty(0).Sum();
            var tttptotal = db_context.PhongBans.Select(x => x.TTTPHCDB).DefaultIfEmpty(0).Sum() + db_context.PhongBans.Select(x => x.TTTPCaDB).DefaultIfEmpty(0).Sum();
            var nvtotal = db_context.PhongBans.Select(x=>x.NVHCDB).DefaultIfEmpty(0).Sum()+ db_context.PhongBans.Select(x => x.NVCaDB).DefaultIfEmpty(0).Sum();
            var nvvpc = db_context.PhongBans.Where(x => x.NhomID == 1).Select(x => x.NVHCDB).DefaultIfEmpty(0).Sum();
            nvtotal = nvtotal - nvvpc;

            TotalList query = new TotalList()
            {
                TPTotal = (int)tptotal,
                QDTotal = (int)qdtotal,
                PTTotal = (int)pttotal,
                TPKipTotal =(int)tpkiptotal,
                KTVTotal = (int)ktvtotal,
                TTTPTotal =(int)tttptotal,
                NVTotal = (int)nvtotal,
                NVVPC =(int)nvvpc
            };

            return Json(query, JsonRequestBehavior.AllowGet);
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
    }
}