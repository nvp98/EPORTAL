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
    public class TerminalController : Controller
    {
        // GET: TagSign/Terminal
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Terminal";
        // GET: TagSign/Gate
        public ActionResult Index(int? page, int? search)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var result = new List<TerminalModel>();
            if (search != null)
            {
                result = db.sp_Terminal_Search(search).Select(x => new TerminalModel() { Id = x.ID, IDCong = x.IDCong, IDTinhTrang = x.TinhTrang, IPAdress = x.IPAdress, Vitri = x.TenVT }).ToList();
            }
            else
            {
                result = db.Terminals.Select(x => new TerminalModel() { Id = x.ID, IDCong = x.IDCong, IDTinhTrang = x.TinhTrang, IPAdress = x.IPAdress, Vitri = x.TenVT }).ToList();

            }
            var res = from re in result.ToList()
                      join b in db.NT_Gate.ToList() on re.IDCong equals b.IDGate
                      select new TerminalModel()
                      {
                          Id = re.Id,
                          IDCong = b.IDGate,
                          TenCong = b.Gate,
                          Vitri = re.Vitri,
                          IPAdress = re.IPAdress,
                          IDTinhTrang = re.IDTinhTrang,
                          TinhTrang = (re.IDTinhTrang == 0) ? "Đóng" : "Mở"
                      };
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            var cong = db.NT_Gate.Where(x => x.TinhTrang == 1).ToList();
            ViewBag.Cong = new SelectList(cong, "IDGate", "Gate");
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.Id).ToList().ToPagedList(pageNumber, pageSize));

        }
        public ActionResult Create()
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var listCong = from a in db.NT_Gate.ToList() select new GateValidation() { IDGate = a.IDGate, Gate = a.Gate };
            ViewBag.ListCong = new SelectList(listCong, "IDGate", "Gate");
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(TerminalModel request)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                
                db.sp_Terminal_Insert(request.IPAdress, request.Vitri, request.IDCong, 1);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Terminal");
        }

        public ActionResult Edit(int id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var model = db.Terminals.Where(x => x.ID == id).Select(x => new TerminalModel() { Id = x.ID, IDCong = x.IDCong, IPAdress = x.IPAdress, Vitri = x.TenVT }).SingleOrDefault();
            var listCong = from a in db.NT_Gate.ToList() select new GateValidation() { IDGate = a.IDGate, Gate = a.Gate };
            ViewBag.ListCong = new SelectList(listCong, "IDGate", "Gate", model.IDCong);
            return PartialView(model);
        }
        [HttpPost]

        public ActionResult Edit(TerminalModel request)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.sp_Terminal_Update(request.Id, request.IPAdress, request.Vitri, request.IDCong, request.IDTinhTrang);
                TempData["msgSuccess"] = "<script>alert('Sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi sửa: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Terminal");
        }
    }
}