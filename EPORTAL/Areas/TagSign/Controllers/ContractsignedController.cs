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
    public class ContractsignedController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Contractsigned";
        // GET: TagSign/Contractsigned
        public ActionResult Index(int? page, string search)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;
            var nd = db.AuthorizationContractors.Where(x => x.IDNhanVien == Models.MyAuthentication.ID).SingleOrDefault();

            var res = (from a in db.NT_Contract_select(search).Where(x=>x.TTXL == 1 && x.TPBP == nd.IDNhanVien)
                      join nt in db.NT_Partner on a.IDNT equals nt.ID
                      join bp in db.PhongBans on a.IDBP equals bp.IDPhongBan into ulist1
                      from bp in ulist1.DefaultIfEmpty()
                      select new ContractValidation
                      {
                          IDContract = (int)a.IDContract,
                          Somecontracts = a.Somecontracts,
                          Contract = a.Contract,
                          BeginDate = (DateTime?)a.BeginDate ?? default,
                          EndDate = (DateTime?)a.EndDate ?? default,
                          FileScan = a.FileScan,
                          IDNT = (int)a.IDNT,
                          Fullname = nt.FullName,
                          IDBP = (int?)a.IDBP ?? default,
                          TPBP = (int?)a.TPBP ?? default,
                          TTXL = (int?)a.TTXL ?? default,
                      }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderBy(x => x.IDContract).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Approve(int? id, int? idnv)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("APPROVE"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.NT_Contract_updateTK(id, idnv, 2);
                TempData["msgSuccess"] = "<script>alert('Phê duyệt thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Phê duyệt thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Contractsigned");
        }

        public ActionResult CancelApprove(int? id, int? idnv)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("APPROVE"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.NT_Contract_updateTK(id, idnv, 2);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Contractsigned");
        }
    }
}