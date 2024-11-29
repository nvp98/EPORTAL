using EPORTAL.Models;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class InOutAndResultController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "InOutAndResult";
        // GET: TagSign/InOutAndResult
        public ActionResult Index(int? page, DateTime? begind, DateTime? endd, int? IDTT)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            
            DateTime DayNow = DateTime.Now.Date;

            List<NT_Confirm> xn = db.NT_Confirm.ToList();
            //if (IDTT == null) IDTT = 0;
            ViewBag.TTList = new SelectList(xn, "IDTT", "TenTT", IDTT);

            var res = (from a in db.NT_DetailCardRegistrationInfor
                      join c in db. NT_CardRegistrationInfor on a.IDDS equals c.IDDS
                      join n in db.NT_ContractorGroup on a.IDGroup equals n.IDGroup
                      join cv in db.NT_Position on a.Position equals cv.IDCV
                      where(begind == null && endd == null ? (a.NT_CardRegistrationInfor.DaySchool >= DayNow && a.NT_CardRegistrationInfor.DaySchool <= DayNow && a.NT_CardRegistrationInfor.IDTTTK == 2) :
                           (a.NT_CardRegistrationInfor.DaySchool >= begind && a.NT_CardRegistrationInfor.DaySchool <= endd && a.NT_CardRegistrationInfor.IDTTTK == 2))
                      select new NT_DetailCardRegistrationInforValidation
                      {
                          IDCT = (int)a.IDCT,
                          FullName = a.FullName,
                          CCCD = a.CCCD,
                          BirthDay = (DateTime)a.BirthDay,
                          PermanentResidence = a.PermanentResidence,
                          Position = (int)a.Position,
                          NamePosition = cv.TenCV,
                          Phone = a.Phone,
                          StorageCard = a.StorageCard,
                          AccessCard = a.AccessCard,
                          CreateDate = (DateTime)a.CreateDate,
                          IDGroup = (int)a.IDGroup,
                          NameContractorGroup = n.NameContractorGroup,
                          IDGate = a.IDGate,
                          Note = a.Note,
                          IDDS = (int?)a.IDDS ?? default,
                          InOutID = (int?)a.InOutID ?? default,
                          ResultID = (int?)a.ResultID ?? default,
                          DaySchool = (DateTime)c.DaySchool
                      }).ToList();
            if (IDTT != null)
            {
                res = res.Where(x=>x.InOutID == IDTT).ToList();
            }

            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCT).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult InOut(NT_DetailCardRegistrationInforValidation _DO, FormCollection collection)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("CONFIRM"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            try
            {
                string Begind = collection["begind"];
                string Endd = collection["endd"];
                foreach (var key in collection.AllKeys)
                {
                    if(key != "__RequestVerificationToken" && key != "begind" && key != "endd" && key != "select-all")
                    {
                        string[] arrListStr = collection[key].Split(',');
                        if(arrListStr.Length >= 2)
                        {
                            foreach (var value in arrListStr)
                            {
                                if(value == "true")
                                {
                                    var IDCT = db.NT_DetailCardRegistrationInfor.Where(x=>x.CCCD == key).FirstOrDefault();
                                    if(IDCT.ResultID == 0)
                                    {
                                        db.NT_DetailCardRegistrationInfor_InOut(Convert.ToInt32(IDCT.IDCT), 1);
                                    }    
                                }
                               
                            }
                        } 
                        else
                        {
                            var IDCT = db.NT_DetailCardRegistrationInfor.Where(x => x.CCCD == key).FirstOrDefault();
                            db.NT_DetailCardRegistrationInfor_InOut(Convert.ToInt32(IDCT.IDCT), 0);
                        }    

                    } 
                } 
               TempData["msgError"] = "<script>alert('Xác nhận vào cổng thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi xác nhận vào cổng: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "InOutAndResult");
        }

        public ActionResult Result (int? page, DateTime? begind, DateTime? endd, int? IDTT)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var Day = DateTime.Now.ToString("dd-MM-yyyy");
            DateTime DayNow = DateTime.Parse(Day);

            List<NT_Confirm> xn = db.NT_Confirm.ToList();
            //if (IDTT == null) IDTT = 0;
            ViewBag.TTList = new SelectList(xn, "IDTT", "TenTT", IDTT);

            var res = (from a in db.NT_DetailCardRegistrationInfor
                      join c in db.NT_CardRegistrationInfor on a.IDDS equals c.IDDS
                      join n in db.NT_ContractorGroup on a.IDGroup equals n.IDGroup
                      join cv in db.NT_Position on a.Position equals cv.IDCV
                      where (begind == null && endd == null ? (a.NT_CardRegistrationInfor.DaySchool >= DayNow && a.NT_CardRegistrationInfor.DaySchool <= DayNow && a.NT_CardRegistrationInfor.IDTTTK == 2 && a.InOutID == 1) :
                           (a.NT_CardRegistrationInfor.DaySchool >= begind && a.NT_CardRegistrationInfor.DaySchool <= endd && a.NT_CardRegistrationInfor.IDTTTK == 2 && a.InOutID == 1 ))
                      select new NT_DetailCardRegistrationInforValidation
                      {
                          IDCT = (int)a.IDCT,
                          FullName = a.FullName,
                          CCCD = a.CCCD,
                          BirthDay = (DateTime)a.BirthDay,
                          PermanentResidence = a.PermanentResidence,
                          Position = (int)a.Position,
                          NamePosition = cv.TenCV,
                          Phone = a.Phone,
                          StorageCard = a.StorageCard,
                          AccessCard = a.AccessCard,
                          CreateDate = (DateTime)a.CreateDate,
                          IDGroup = (int)a.IDGroup,
                          NameContractorGroup = n.NameContractorGroup,
                          IDGate = a.IDGate,
                          Note = a.Note,
                          IDDS = (int?)a.IDDS ?? default,
                          InOutID = (int?)a.InOutID ?? default,
                          ResultID = (int?)a.ResultID ?? default,
                      }).ToList();
            if (IDTT != null)
            {
                res = res.Where(x => x.ResultID == IDTT).ToList();
            }

            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCT).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult ResultUpdate(NT_DetailCardRegistrationInforValidation _DO, FormCollection collection)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("CONFIRM_RESULT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                string Begind = collection["begind"];
                string Endd = collection["endd"];
                foreach (var key in collection.AllKeys)
                {
                    if (key != "__RequestVerificationToken" && key != "begind" && key != "endd" && key != "select-all")
                    {
                        string[] arrListStr = collection[key].Split(',');
                        if (arrListStr.Length >= 2)
                        {
                            foreach (var value in arrListStr)
                            {
                                if (value == "true")
                                {
                                    var IDCT = db.NT_DetailCardRegistrationInfor.Where(x => x.CCCD == key).FirstOrDefault();
                                    db.NT_DetailCardRegistrationInfor_Result(Convert.ToInt32(IDCT.IDCT), 1);
                                }

                            }
                        }
                        else
                        {
                            var IDCT = db.NT_DetailCardRegistrationInfor.Where(x => x.CCCD == key).FirstOrDefault();
                            db.NT_DetailCardRegistrationInfor_Result(Convert.ToInt32(IDCT.IDCT), 0);
                        }

                    }
                }
                TempData["msgError"] = "<script>alert('Xác nhận kết quả học an toàn thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi xác nhận kết quả học an toàn học an toàn: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Result", "InOutAndResult");
        }
    }
}