using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class ContractorGroupController : Controller
    {
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTALEntities db = new EPORTALEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ContractorGroup";
        // GET: TagSign/ContractorGroup
        public ActionResult Index(int? page)
        {

            var res = from a in db.NT_ContractorGroup
                      select new DataInformationValidation
                      {
                          IDGroup = (int)a.IDGroup,
                          NameContractorGroup = a.NameContractorGroup,
                          Describe = a.Describe
                      };
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDGroup).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Index_HPDQ(int? page, string search)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_ContractorGroup
                      select new DataInformationValidation
                      {
                          IDGroup = (int)a.IDGroup,
                          NameContractorGroup = a.NameContractorGroup,
                          Describe = a.Describe
                      }).ToList();
            if(search != null)
            {
                res = res.Where(x=>x.NameContractorGroup.Contains(search)).ToList();
            }
                           ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; } 
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.IDGroup).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(DataInformationValidation _DO)
        {

            try
            {
                NT_ContractorGroup ContractorGroup = new NT_ContractorGroup();
                ContractorGroup.NameContractorGroup = _DO.NameContractorGroup;
                ContractorGroup.Describe = _DO.Describe;
                db.NT_ContractorGroup.Add(ContractorGroup);
                db.SaveChanges();
                //var a = db.NT_Gate_insert(_DO.Gate, 1);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index_HPDQ", "ContractorGroup");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_ContractorGroup
                      select new DataInformationValidation
                      {
                          IDGroup = (int)a.IDGroup,
                          NameContractorGroup = a.NameContractorGroup,
                          Describe = a.Describe
                      }).ToList();
            DataInformationValidation DO = new DataInformationValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDGroup = (int)a.IDGroup;
                    DO.NameContractorGroup = a.NameContractorGroup;
                    DO.Describe = a.Describe;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);


        }
        [HttpPost]
        public ActionResult Edit(DataInformationValidation _DO)
        {

            try
            {
                var ContractorGroup = db.NT_ContractorGroup.Where(x => x.IDGroup == _DO.IDGroup).FirstOrDefault();
                if (ContractorGroup != null)
                {
                    ContractorGroup.NameContractorGroup = _DO.NameContractorGroup;
                    ContractorGroup.Describe = _DO.Describe;
                }
                db.SaveChanges();
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index_HPDQ", "ContractorGroup");
        }
    }
}