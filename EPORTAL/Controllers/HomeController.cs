using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsView360;
using System.Collections;

namespace EPORTAL.Controllers
{
    public class HomeController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        public ActionResult Index()
        {
            return View();
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
        //public ActionResult GetDataNVNT()
        //{
        //    // Nhân viên nhà thầu làm việc
        //    List<int> Nhanvienlv = new List<int>();
        //    List<int> Countlv = new List<int>();
        //    Stack TenNhaThaulv = new Stack();
        //    var IDNTLV = db_nt.NT_NhanVienNT.Where(x => x.TTLV == 1).ToList();


        //    foreach (var idnt in IDNTLV)
        //    {
        //        Nhanvienlv.Add((int)idnt.IDNT);
        //    }
        //    List<int> DisNTLV = Nhanvienlv.Distinct().ToList();

        //    foreach (var name in DisNTLV)
        //    {
        //        var Name = db.NT_Partner.Where(x => x.ID == name).FirstOrDefault();
        //        TenNhaThaulv.Push(Name.ShortName);

        //        var Counts = db_nt.NT_NhanVienNT.Where(x => x.IDNT == name).Count();
        //        Countlv.Add(Counts);
        //    }

        //    // Nhân viên nhà thầu nghỉ việc
        //    List<int> Nhanviennv = new List<int>();
        //    List<int> Countnv = new List<int>();
        //    Stack TenNhaThaunv = new Stack();
        //    var IDNTNV = db_nt.NT_NhanVienNT.Where(x => x.TTLV == 3).ToList();
        //    foreach (var idnt in IDNTNV)
        //    {
        //        Nhanviennv.Add((int)idnt.IDNT);
        //    }
        //    List<int> DisNTNV = Nhanviennv.Distinct().ToList();

        //    foreach (var name in DisNTNV)
        //    {
        //        var Name = db.NT_Partner.Where(x => x.ID == name).FirstOrDefault();
        //        TenNhaThaunv.Push(Name.ShortName);

        //        var Counts = db_nt.NT_NhanVienNT.Where(x => x.IDNT == name).Count();
        //        Countnv.Add(Counts);
        //    }
        //    //var Test = db_nt.NT_NhanVienNT.Where(x => x.IDNT == 3440).Count();
        //    return Json(new
        //    {
        //        TenNhaThaulv = TenNhaThaulv,
        //        Countlv = Countlv,
        //        TenNhaThaunv = TenNhaThaunv,
        //        Countnv = Countnv
        //    }, JsonRequestBehavior.AllowGet);
        //}
        //public ActionResult GetDataATLD()
        //{
        //    DateTime DayUpload = DateTime.Now;
        //    // Tháng 1
        //    DateTime Thang1 = DateTime.Now.AddMonths(-5);
        //    var Start1 = new DateTime(Thang1.Year, Thang1.Month, 1);
        //    var End1 = Start1.AddMonths(1).AddDays(-1);
        //    var Chuaxuly1 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK != 2 && x.DayUpload >= Start1 && x.DayUpload <= End1).Count();
        //    var Daxuly1 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK == 2 && x.DayUpload >= Start1 && x.DayUpload <= End1).Count();
        //    // Tháng 2
        //    DateTime Thang2 = DateTime.Now.AddMonths(-4);
        //    var Start2 = new DateTime(Thang2.Year, Thang2.Month, 1);
        //    var End2 = Start2.AddMonths(1).AddDays(-1);
        //    var Chuaxuly2 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK != 2 && x.DayUpload >= Start2 && x.DayUpload <= End2).Count();
        //    var Daxuly2 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK == 2 && x.DayUpload >= Start2 && x.DayUpload <= End2).Count();
        //    // Tháng 3
        //    DateTime Thang3 = DateTime.Now.AddMonths(-3);
        //    var Start3 = new DateTime(Thang3.Year, Thang3.Month, 1);
        //    var End3 = Start3.AddMonths(1).AddDays(-1);
        //    var Chuaxuly3 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK != 2 && x.DayUpload >= Start3 && x.DayUpload <= End3).Count();
        //    var Daxuly3 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK == 2 && x.DayUpload >= Start3 && x.DayUpload <= End3).Count();
        //    // Tháng 4
        //    DateTime Thang4 = DateTime.Now.AddMonths(-2);
        //    var Start4 = new DateTime(Thang4.Year, Thang4.Month, 1);
        //    var End4 = Start4.AddMonths(1).AddDays(-1);
        //    var Chuaxuly4 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK != 2 && x.DayUpload >= Start4 && x.DayUpload <= End4).Count();
        //    var Daxuly4 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK == 2 && x.DayUpload >= Start4 && x.DayUpload <= End4).Count();
        //    // Tháng 5
        //    DateTime Thang5 = DateTime.Now.AddMonths(-1);
        //    var Start5 = new DateTime(Thang5.Year, Thang5.Month, 1);
        //    var End5 = Start5.AddMonths(1).AddDays(-1);
        //    var Chuaxuly5 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK != 2 && x.DayUpload >= Start5 && x.DayUpload <= End5).Count();
        //    var Daxuly5 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK == 2 && x.DayUpload >= Start5 && x.DayUpload <= End5).Count();
        //    // Tháng 6
        //    DateTime Thang6 = DateTime.Now;
        //    var Start6 = new DateTime(Thang6.Year, Thang6.Month, 1);
        //    var End6 = Start6.AddMonths(1).AddDays(-1);
        //    var Chuaxuly6 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK != 2 && x.DayUpload >= Start6 && x.DayUpload <= End6).Count();
        //    var Daxuly6 = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK == 2 && x.DayUpload >= Start6 && x.DayUpload <= End6).Count();

        //    var Daxuly = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK == 2).Count();
        //    var Chuaxuly = db.NT_CardRegistrationInfor.Where(x => x.IDTTTK != 2).Count();

        //    return Json(new
        //    {
        //        Daxuly = Daxuly,
        //        Chuaxuly = Chuaxuly,
        //        Daxuly1 = Daxuly1,
        //        Chuaxuly1 = Chuaxuly1,
        //        Daxuly2 = Daxuly2,
        //        Chuaxuly2 = Chuaxuly2,
        //        Daxuly3 = Daxuly3,
        //        Chuaxuly3 = Chuaxuly3,
        //        Daxuly4 = Daxuly4,
        //        Chuaxuly4 = Chuaxuly4,
        //        Daxuly5 = Daxuly5,
        //        Chuaxuly5 = Chuaxuly5,
        //        Daxuly6 = Daxuly6,
        //        Chuaxuly6 = Chuaxuly6
        //    }, JsonRequestBehavior.AllowGet);
        //}
    }
}