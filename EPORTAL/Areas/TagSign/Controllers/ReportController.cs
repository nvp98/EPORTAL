using EPORTAL.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsView360;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ReportController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Report";
        // GET: TagSign/Report
        public ActionResult Index()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return View();
        }
        public ActionResult GetData()
        {
    
            List<int> phongban = new List<int>();
            //List<int> Count = new List<int>();
            Stack TenBP = new Stack();
            //Stack SLNVUser = new Stack();
            List<int> CountUsers = new List<int>();
            List<int> CountVideos = new List<int>();
            //int SL = 0;
            //var Check = db.AuthorizationUSERs.ToList();
            var listPB = db.PhongBans.ToList();
            //foreach(var pb in listPB)
            //{
            //    phongban.Add((int)pb.IDPhongBan);
            //}
            //List<int> DisBP = phongban.Distinct().ToList();
            foreach (var checkpb in listPB)
            {
                //var BP = db.PhongBans.Where(x => x.IDPhongBan == checkpb).FirstOrDefault();
                
                var countNVUser = db.AuthorizationUSERs.Where(x=> x.NhanVien.IDPhongBan == checkpb.IDPhongBan).ToList();
                var countNVVideo = db.AuthorizationVideos.Where(x => x.NhanVien.IDPhongBan == checkpb.IDPhongBan).ToList();
                
                List<int> NhanVienU = new List<int>();
                //NhanVienU.Clear();

                foreach (var checknvU in countNVUser)
                {
                    NhanVienU.Add((int)checknvU.NhanVienID);
                }
                List<int> Disnvu = NhanVienU.Distinct().ToList();

                List<int> NhanVienV = new List<int>();

                foreach (var checknvV in countNVVideo)
                {
                    NhanVienV.Add((int)checknvV.NhanVienID);
                }
                List<int> DisnvV = NhanVienV.Distinct().ToList();
                if(Disnvu.Count() == 0 && Disnvu.Count() == 0)
                {

                } else
                {
                    TenBP.Push(checkpb.TenPhongBan);
                    CountUsers.Add(Disnvu.Count());
                    CountVideos.Add(DisnvV.Count());
                }

                
                //if (SL != 0)
                //{
                //    Count.Add(SL);
                //    SLNV.Push(SL+"_"+checkpb);
                //    SL = 0;
                //}
            }

            //List<int> phongban2 = new List<int>();
            //List<int> Count2 = new List<int>();
            //Stack TenBP2 = new Stack();
            //Stack SLNV2 = new Stack();
            //int SL2 = 0;
            //var Check2 = db.AuthorizationVideos.ToList();
            ////foreach (var pb2 in Check2)
            ////{
            ////    phongban2.Add((int)pb2.NhanVien.IDPhongBan);
            ////}
            ////List<int> DisBP2 = phongban2.Distinct().ToList();
            //foreach (var checkpb2 in phongban)
            //{
            //    var BP2 = db.PhongBans.Where(x => x.IDPhongBan == checkpb2).FirstOrDefault();
            //    TenBP2.Push(BP2.TenPhongBan);
            //    List<int> NhanVien2 = new List<int>();
            //    NhanVien2.Clear();

            //    foreach (var checknv2 in Check2)
            //    {
            //        if (checkpb2 == checknv2.NhanVien.IDPhongBan)
            //        {
            //            NhanVien2.Add((int)checknv2.NhanVienID);
            //        }
            //        List<int> DisNV2 = NhanVien2.Distinct().ToList();
            //        SL2 = DisNV2.Count();
            //    }
            //    if (SL2 != 0)
            //    {
            //        Count2.Add(SL2);
            //        SLNV2.Push(SL2 + "_" + checkpb2);
            //        SL2 = 0;
            //    }
            //}
            return Json(new
            {
                CountBP = TenBP,
                CountNV = CountUsers,
                CountBP2 = TenBP,
                CountNV2 = CountVideos
            },JsonRequestBehavior.AllowGet);

        }
    }
}