using ClosedXML.Excel;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsPartner;
using EPORTAL.Models;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class Detail_ListCardRegisInforController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        // GET: TagSign/Detail_ListCardRegisInfor
        public ActionResult Index(int? id, int? page)
        {
            List<NT_Contract> hd = db.NT_Contract.ToList();
            ViewBag.HDList = new SelectList(hd, "IDContract", "Somecontracts");

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            var res = from a in db_dk.DK_DetailCardRegistrationInfor.Where(x => x.IDDKT == id)
                      select new DK_Detail_ListCardRegisInforNTValidation
                      {
                          IDCTDK = (int)a.IDCTDK,
                          HoTen = a.HoTen,
                          CCCD = a.CCCD,
                          NgaySinh = (DateTime)a.NgaySinh,
                          HoKhau = a.HoKhau,
                          ChucVu = (int)a.ChucVu,
                          SoDienThoai = a.SoDienThoai,
                          TheLuuTru = a.TheLuuTru,
                          TheRaVaoKLH = a.TheRaVaoKLH,
                          DienThoaiTM = a.DienThoaiTM,
                          RaVaoCang = a.RaVaoCang,
                          ThoiHanThe = (DateTime)a.ThoiHanThe,
                          KhuVucLamViec = a.KhuVucLamViec,
                          Cong = a.Cong,
                          NhomNT = (int)a.NhomNT,
                          GhiChu = a.GhiChu,
                          IDDKT = (int)a.IDDKT
                      };
            if (id != null)
            {
                ViewData["id"] = id;
            }

            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCTDK).ToList().ToPagedList(pageNumber, pageSize));

        }
        public ActionResult Edit(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardRegistrationInfor.Where(x => x.IDCTDK == id)
                       select new DK_Detail_ListCardRegisInforNTValidation
                       {
                           IDCTDK = (int)a.IDCTDK,
                           HoTen = a.HoTen,
                           CCCD = a.CCCD,
                           NgaySinh = (DateTime)a.NgaySinh,
                           HoKhau = a.HoKhau,
                           ChucVu = (int)a.ChucVu,
                           SoDienThoai = a.SoDienThoai,
                           TheLuuTru = a.TheLuuTru,
                           TheRaVaoKLH = a.TheRaVaoKLH,
                           DienThoaiTM = a.DienThoaiTM,
                           RaVaoCang = a.RaVaoCang,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           KhuVucLamViec = a.KhuVucLamViec,
                           Cong = a.Cong,
                           NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           IDDKT = (int)a.IDDKT
                       }).ToList();
            DK_Detail_ListCardRegisInforNTValidation DO = new DK_Detail_ListCardRegisInforNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDCTDK = (int)a.IDCTDK;
                    DO.HoTen = a.HoTen;
                    DO.CCCD = a.CCCD;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.HoKhau = a.HoKhau;
                    DO.ChucVu = (int)a.ChucVu;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.TheLuuTru = a.TheLuuTru;
                    DO.TheRaVaoKLH = a.TheRaVaoKLH;
                    DO.DienThoaiTM = a.DienThoaiTM;
                    DO.RaVaoCang = a.RaVaoCang;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.KhuVucLamViec = a.KhuVucLamViec;
                    DO.Cong = a.Cong;
                    DO.NhomNT = (int)a.NhomNT;
                    DO.GhiChu = a.GhiChu;
                    DO.IDDKT = (int)a.IDDKT;
                }
                ViewBag.NgaySinh = DO.NgaySinh.ToString("yyyy-MM-dd");

                ViewBag.ThoiHanThe = DO.ThoiHanThe.ToString("yyyy-MM-dd");

                List<NT_ContractorGroup> n = db.NT_ContractorGroup.ToList();
                ViewBag.IDGroup = new SelectList(n, "IDGroup", "NameContractorGroup", DO.NhomNT);

                List<NT_Gate> c = db.NT_Gate.ToList();
                ViewBag.Selected = new SelectList(c, "IDGate", "Gate");

                List<NT_Workplace> nkv = db.NT_Workplace.ToList();
                ViewBag.SelectedKV = new SelectList(nkv, "IDKV", "TenKV");

                List<NT_Position> chucvu = db.NT_Position.ToList();
                ViewBag.IDCV = new SelectList(chucvu, "IDCV", "TenCV");
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(DK_Detail_ListCardRegisInforNTValidation _DO, FormCollection collection)
        {
            try
            {
                var DO = db_dk.DK_DetailCardRegistrationInfor.Where(x => x.IDCTDK == _DO.IDCTDK).FirstOrDefault();
                if(_DO.GhiChu == null)
                {
                   var a = db_dk.DK_DetailCardRegistrationInfor_Update
                    (DO.IDCTDK,
                    DO.HoTen,
                    DO.CCCD,
                    DO.NgaySinh,
                    DO.HoKhau,
                    DO.ChucVu,
                    DO.SoDienThoai,
                    DO.TheLuuTru,
                    DO.TheRaVaoKLH,
                    DO.DienThoaiTM,
                    DO.RaVaoCang,
                    DO.ThoiHanThe,
                    DO.KhuVucLamViec,
                    DO.Cong,
                    DO.NhomNT,
                   "",
                    DO.IDDKT);
                }    
                else
                {
                 var a = db_dk.DK_DetailCardRegistrationInfor_Update
                    (DO.IDCTDK,
                    DO.HoTen,
                    DO.CCCD,
                    DO.NgaySinh,
                    DO.HoKhau,
                    DO.ChucVu,
                    DO.SoDienThoai,
                    DO.TheLuuTru,
                    DO.TheRaVaoKLH,
                    DO.DienThoaiTM,
                    DO.RaVaoCang,
                    DO.ThoiHanThe,
                    DO.KhuVucLamViec,
                    DO.Cong,
                    DO.NhomNT,
                    _DO.GhiChu,
                    DO.IDDKT);
                }    

                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thất bại: " + e.Message + "');</script>";
            }

            return RedirectToAction("Index", "Detail_ListCardRegisInfor", new { id = _DO.IDDKT });
        }

        public ActionResult Indes (int? id, int? page)
        {
            List<NT_Contract> hd = db.NT_Contract.ToList();
            ViewBag.HDList = new SelectList(hd, "IDContract", "Somecontracts");

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            var res = from a in db_dk.DK_DetailCardExtend.Where(x => x.IDGHT == id)
                      select new DK_Detail_ListExtendcardNTValidation
                      {
                          IDCTGH = (int)a.IDCTGH,
                          HoTen = a.HoTen,
                          CCCD = a.CCCD,
                          NgaySinh = (DateTime)a.NgaySinh,
                          HoKhau = a.HoKhau,
                          ChucVu = (int)a.ChucVu,
                          SoDienThoai = a.SoDienThoai,
                          CapLai = a.CapLai,
                          GiaHan = a.GiaHan,
                          ThongTinLuuTru = a.ThongTinLuuTru,
                          DTTM = a.DTTM,
                          RaVaoCang = a.RaVaoCang,
                          ThoiHanThe = (DateTime)a.ThoiHanThe,
                          KhuVucLamViec = a.KhuVucLamViec,
                          Cong = a.Cong,
                          NhomNT = (int)a.NhomNT,
                          GhiChu = a.GhiChu,
                          IDGHT = (int)a.IDGHT
                      };
            if (id != null)
            {
                ViewData["id"] = id;
            }

            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCTGH).ToList().ToPagedList(pageNumber, pageSize));

        }
        public ActionResult Edits(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardExtend.Where(x => x.IDCTGH == id)
                       select new DK_Detail_ListExtendcardNTValidation
                       {
                           IDCTGH = (int)a.IDCTGH,
                           HoTen = a.HoTen,
                           CCCD = a.CCCD,
                           NgaySinh = (DateTime)a.NgaySinh,
                           HoKhau = a.HoKhau,
                           ChucVu = (int)a.ChucVu,
                           SoDienThoai = a.SoDienThoai,
                           CapLai = a.CapLai,
                           GiaHan = a.GiaHan,
                           ThongTinLuuTru = a.ThongTinLuuTru,
                           DTTM = a.DTTM,
                           RaVaoCang = a.RaVaoCang,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           KhuVucLamViec = a.KhuVucLamViec,
                           Cong = a.Cong,
                           NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           IDGHT = (int)a.IDGHT
                       }).ToList();
            DK_Detail_ListExtendcardNTValidation DO = new DK_Detail_ListExtendcardNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDCTGH = (int)a.IDCTGH;
                    DO.HoTen = a.HoTen;
                    DO.CCCD = a.CCCD;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.HoKhau = a.HoKhau;
                    DO.ChucVu = (int)a.ChucVu;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.CapLai = a.CapLai;
                    DO.GiaHan = a.GiaHan;
                    DO.ThongTinLuuTru = a.ThongTinLuuTru;
                    DO.DTTM = a.DTTM;
                    DO.RaVaoCang = a.RaVaoCang;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.KhuVucLamViec = a.KhuVucLamViec;
                    DO.Cong = a.Cong;
                    DO.NhomNT = (int)a.NhomNT;
                    DO.GhiChu = a.GhiChu;
                    DO.IDGHT = (int)a.IDGHT;
                }
                ViewBag.NgaySinh = DO.NgaySinh.ToString("yyyy-MM-dd");

                ViewBag.ThoiHanThe = DO.ThoiHanThe.ToString("yyyy-MM-dd");

                List<NT_ContractorGroup> n = db.NT_ContractorGroup.ToList();
                ViewBag.IDGroup = new SelectList(n, "IDGroup", "NameContractorGroup", DO.NhomNT);

                List<NT_Gate> c = db.NT_Gate.ToList();
                ViewBag.Selected = new SelectList(c, "IDGate", "Gate");

                List<NT_Workplace> nkv = db.NT_Workplace.ToList();
                ViewBag.SelectedKV = new SelectList(nkv, "IDKV", "TenKV");

                List<NT_Position> chucvu = db.NT_Position.ToList();
                ViewBag.IDCV = new SelectList(chucvu, "IDCV", "TenCV");
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edits(DK_Detail_ListExtendcardNTValidation _DO, FormCollection collection)
        {
            var DO = db_dk.DK_DetailCardExtend.Where(x => x.IDCTGH == _DO.IDCTGH).FirstOrDefault();
            if(_DO.GhiChu == null) {
               var a = db_dk.DK_DetailCardExtend_Update
                (DO.IDCTGH,
                DO.HoTen,
                DO.CCCD,
                DO.NgaySinh,
                DO.HoKhau,
                DO.ChucVu,
                DO.SoDienThoai,
                DO.CapLai,
                DO.GiaHan,
                DO.ThongTinLuuTru,
                DO.DTTM,
                DO.RaVaoCang,
                DO.ThoiHanThe,
                 DO.KhuVucLamViec,
                 DO.Cong,
                DO.NhomNT,
               "",
                DO.IDGHT);
            }
            else
            {
               var a = db_dk.DK_DetailCardExtend_Update
                (DO.IDCTGH,
                DO.HoTen,
                DO.CCCD,
                DO.NgaySinh,
                DO.HoKhau,
                DO.ChucVu,
                DO.SoDienThoai,
                DO.CapLai,
                DO.GiaHan,
                DO.ThongTinLuuTru,
                DO.DTTM,
                DO.RaVaoCang,
                DO.ThoiHanThe,
                 DO.KhuVucLamViec,
                 DO.Cong,
                DO.NhomNT,
                _DO.GhiChu,
                DO.IDGHT);
            }    


            TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";

            return RedirectToAction("Indes", "Detail_ListCardRegisInfor", new { id = _DO.IDGHT });
        }
    }
}