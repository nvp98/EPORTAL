using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class Detail_ViewTemporarycardController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Detail_ViewTemporarycard";
        // GET: TagSign/Detail_ViewTemporarycard
        public ActionResult Index(int? id, int? page)
        {
            List<NT_CardTemp> hd = db_nt.NT_CardTemp.ToList();
            ViewBag.IDThe = new SelectList(hd, "IDThe", "MaThe");

            var res = (from a in db_nt.NT_Handle.Where(x => x.IDTTT == id)
                       join ct in db_nt.NT_DetailCarteTemporaire on a.IDCTTTT equals ct.IDCTTTT
                       join th in db_nt.NT_CardTemp on a.IDThe equals th.IDThe into ulist1
                       from th in ulist1.DefaultIfEmpty()
                       select new Detail_HandleValidation
                       {
                           IDXL = (int)a.IDXL,
                           IDTTT = (int)a.IDTTT,
                           HoVaTen = ct.HoVaTen,
                           CCCD = ct.CCCD,
                           NgaySinh = (DateTime)ct.NgaySinh,
                           //MucDich = (int)ct.MucDich,
                           Sdt = ct.Sdt,
                           BienSo = ct.BienSo,
                           Cong = (int)ct.Cong,
                           GhiChu = ct.GhiChu,
                           IDCTTTT = (int)a.IDCTTTT,
                           ThoiGianXuLy = (DateTime?)a.ThoiGianXuLy ?? default,
                           NguoiXuLy = (int?)a.NguoiXuLy ?? default,
                           IDThe = (int?)a.IDThe ?? default,
                           MaThe = th.MaThe
                       }).ToList();
            if (id != null)
            {
                NT_CarteTemporaireValidation idttt = new NT_CarteTemporaireValidation()
                {
                    IDTTT = (int)id
                };
                ViewData["id"] = idttt;
            }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCTTTT).ToList());
        }
    }
}