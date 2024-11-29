using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using EPORTAL.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using Org.BouncyCastle.Asn1.Ocsp;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class Detail_List_RegisterPeople_NTController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        // GET: TagSign/Detail_List_RegisterPeople_NT
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

            var res = from a in db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id)
                      select new List_Detail_RegisterPeopleValidation
                      {
                          ID_CT_DKTN = (int)a.ID_CT_DKTN,
                          HoVaTen = a.HoVaTen,
                          NgaySinh = (DateTime)a.NgaySinh,
                          CCCD = a.CCCD,
                          HoKhau = a.HoKhau,
                          CV_ID = (int)a.CV_ID,
                          SoDienThoai = a.SoDienThoai,
                          Ten_NTP = a.Ten_NTP,
                          HoTen_QuanLy = a.HoTen_QuanLy,
                          SoDienThoai_QuanLy = a.SoDienThoai_QuanLy,
                          CapMoi = a.CapMoi,
                          GiaHan = a.GiaHan,
                          BoSungCong = a.BoSungCong,
                          CapLai = a.CapLai,
                          ChuyenNT = a.ChuyenNT,
                          ThoiHanThe = (DateTime)a.ThoiHanThe,
                          KhuVucLamViec = a.KhuVucLamViec,
                          CongLamViec = a.CongLamViec,
                          NhomNT = (int)a.NhomNT,
                          GhiChu = a.GhiChu,
                          DKTN_ID = (int)a.DKTN_ID
                      };
            if (id != null)
            {
                ViewData["id"] = id;
            }

            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.ID_CT_DKTN).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Delete(int? id, int? iddkt)
        {
            try
            {
                db_dk.Detail_RegisterPeople_Delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = iddkt });
        }

        public ActionResult Edit(int? id)
        {
            var res = (from a in db_dk.Detail_RegisterPeople.Where(x => x.ID_CT_DKTN == id)
                       select new List_Detail_RegisterPeopleValidation
                       {
                           ID_CT_DKTN = (int)a.ID_CT_DKTN,
                           HoVaTen = a.HoVaTen,
                           NgaySinh = (DateTime)a.NgaySinh,
                           CCCD = a.CCCD,
                           HoKhau = a.HoKhau,
                           CV_ID = (int)a.CV_ID,
                           SoDienThoai = a.SoDienThoai,
                           Ten_NTP = a.Ten_NTP,
                           HoTen_QuanLy = a.HoTen_QuanLy,
                           SoDienThoai_QuanLy = a.SoDienThoai_QuanLy,
                           CapMoi = a.CapMoi,
                           GiaHan = a.GiaHan,
                           BoSungCong = a.BoSungCong,
                           CapLai = a.CapLai,
                           ChuyenNT = a.ChuyenNT,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           KhuVucLamViec = a.KhuVucLamViec,
                           CongLamViec = a.CongLamViec,
                           NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           DKTN_ID = (int)a.DKTN_ID
                       }).ToList();
            List_Detail_RegisterPeopleValidation DO = new List_Detail_RegisterPeopleValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_CT_DKTN = (int)a.ID_CT_DKTN;
                    DO.HoVaTen = a.HoVaTen;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.CCCD = a.CCCD;
                    DO.HoKhau = a.HoKhau;
                    DO.CV_ID = (int)a.CV_ID;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.Ten_NTP = a.Ten_NTP;
                    DO.HoTen_QuanLy = a.HoTen_QuanLy;
                    DO.SoDienThoai_QuanLy = a.SoDienThoai_QuanLy;
                    DO.CapMoi = a.CapMoi;
                    DO.GiaHan = a.GiaHan;
                    DO.BoSungCong = a.BoSungCong;
                    DO.CapLai = a.CapLai;
                    DO.ChuyenNT = a.ChuyenNT;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.KhuVucLamViec = a.KhuVucLamViec;
                    DO.CongLamViec = a.CongLamViec;
                    DO.NhomNT = (int)a.NhomNT;
                    DO.GhiChu = a.GhiChu;
                    DO.DKTN_ID = (int)a.DKTN_ID;
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
                ViewBag.IDCV = new SelectList(chucvu, "IDCV", "TenCV", DO.CV_ID);
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(List_Detail_RegisterPeopleValidation _DO, FormCollection collection)
        {
            var DO = db_dk.Detail_RegisterPeople.Where(x => x.ID_CT_DKTN == _DO.ID_CT_DKTN).FirstOrDefault();
            if (_DO.SelectedKV != null && _DO.Selected != null)
            {
                string NameKhuVuc = "";
                foreach (var name in _DO.SelectedKV)
                {
                    if (NameKhuVuc == "")
                    {
                        NameKhuVuc = String.Concat(name);
                    }
                    else
                    {
                        NameKhuVuc = String.Concat(NameKhuVuc + "," + name);
                    }
                }
                string Name = "";
                foreach (var name in _DO.Selected)
                {
                    if (Name == "")
                    {
                        Name = String.Concat(name);
                    }
                    else
                    {
                        Name = String.Concat(Name + "," + name);
                    }
                }
                var insert = db_dk.Detail_RegisterPeople_Update
                                    (DO.ID_CT_DKTN,
                                      _DO.HoVaTen,
                                       _DO.NgaySinh,
                                      _DO.CCCD,
                                      _DO.HoKhau,
                                      _DO.CV_ID,
                                      _DO.Ten_NTP,
                                      _DO.HoTen_QuanLy,
                                      _DO.SoDienThoai_QuanLy,
                                      _DO.SoDienThoai,
                                      _DO.CapMoi,
                                      _DO.GiaHan,
                                      _DO.BoSungCong,
                                      _DO.CapLai,
                                      _DO.ChuyenNT,
                                      _DO.ThoiHanThe,
                                        NameKhuVuc,
                                        Name,
                                      _DO.NhomNT,
                                      "");
                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            else if (_DO.SelectedKV != null && _DO.Selected == null)
            {
                string NameKhuVuc = "";
                foreach (var name in _DO.SelectedKV)
                {
                    if (NameKhuVuc == "")
                    {
                        NameKhuVuc = String.Concat(name);
                    }
                    else
                    {
                        NameKhuVuc = String.Concat(NameKhuVuc + "," + name);
                    }
                }
                var insert = db_dk.Detail_RegisterPeople_Update
                                 (_DO.ID_CT_DKTN,
                                   _DO.HoVaTen,
                                    _DO.NgaySinh,
                                   _DO.CCCD,
                                   _DO.HoKhau,
                                   _DO.CV_ID,
                                   _DO.SoDienThoai,
                                   _DO.Ten_NTP,
                                   _DO.HoTen_QuanLy,
                                   _DO.SoDienThoai_QuanLy,
                                   _DO.CapMoi,
                                   _DO.GiaHan,
                                   _DO.BoSungCong,
                                   _DO.CapLai,
                                   _DO.ChuyenNT,
                                   _DO.ThoiHanThe,
                                     NameKhuVuc,
                                    DO.CongLamViec,
                                   _DO.NhomNT,
                                   "");
                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            else if (_DO.SelectedKV == null && _DO.Selected != null)
            {
                string Name = "";
                foreach (var name in _DO.Selected)
                {
                    if (Name == "")
                    {
                        Name = String.Concat(name);
                    }
                    else
                    {
                        Name = String.Concat(Name + "," + name);
                    }
                }
                var insert = db_dk.Detail_RegisterPeople_Update
                        (_DO.ID_CT_DKTN,
                          _DO.HoVaTen,
                           _DO.NgaySinh,
                          _DO.CCCD,
                          _DO.HoKhau,
                          _DO.CV_ID,
                          _DO.SoDienThoai,
                          _DO.Ten_NTP,
                          _DO.HoTen_QuanLy,
                          _DO.SoDienThoai_QuanLy,
                          _DO.CapMoi,
                          _DO.GiaHan,
                          _DO.BoSungCong,
                          _DO.CapLai,
                          _DO.ChuyenNT,
                          _DO.ThoiHanThe,
                            DO.KhuVucLamViec,
                            Name,
                          _DO.NhomNT,
                          "");

                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            else if (_DO.SelectedKV == null && _DO.Selected == null)
            {
                var insert = db_dk.Detail_RegisterPeople_Update
                        (_DO.ID_CT_DKTN,
                          _DO.HoVaTen,
                           _DO.NgaySinh,
                          _DO.CCCD,
                          _DO.HoKhau,
                          _DO.CV_ID,
                          _DO.SoDienThoai,
                          _DO.Ten_NTP,
                          _DO.HoTen_QuanLy,
                          _DO.SoDienThoai_QuanLy,
                          _DO.CapMoi,
                          _DO.GiaHan,
                          _DO.BoSungCong,
                          _DO.CapLai,
                          _DO.ChuyenNT,
                          _DO.ThoiHanThe,
                           DO.KhuVucLamViec,
                           DO.CongLamViec,
                          _DO.NhomNT,
                          "");
                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = DO.DKTN_ID });
        }
        public ActionResult Index_HPDQ(int? id, int? page)
        {
            List<NT_Contract> hd = db.NT_Contract.ToList();
            ViewBag.HDList = new SelectList(hd, "IDContract", "Somecontracts");

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            var res = from a in db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id)
                      select new List_Detail_RegisterPeopleValidation
                      {
                          ID_CT_DKTN = (int)a.ID_CT_DKTN,
                          HoVaTen = a.HoVaTen,
                          NgaySinh = (DateTime)a.NgaySinh,
                          CCCD = a.CCCD,
                          HoKhau = a.HoKhau,
                          CV_ID = (int)a.CV_ID,
                          SoDienThoai = a.SoDienThoai,
                          Ten_NTP = a.Ten_NTP,
                          HoTen_QuanLy = a.HoTen_QuanLy,
                          SoDienThoai_QuanLy = a.SoDienThoai_QuanLy,
                          CapMoi = a.CapMoi,
                          GiaHan = a.GiaHan,
                          BoSungCong = a.BoSungCong,
                          CapLai = a.CapLai,
                          ChuyenNT = a.ChuyenNT,
                          ThoiHanThe = (DateTime)a.ThoiHanThe,
                          KhuVucLamViec = a.KhuVucLamViec,
                          CongLamViec = a.CongLamViec,
                          NhomNT = (int)a.NhomNT,
                          GhiChu = a.GhiChu,
                          DKTN_ID = (int)a.DKTN_ID, 
                          Price = a.Price
                      };
            if (id != null)
            {
                ViewData["id"] = id;
            }

            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.ID_CT_DKTN).ToList().ToPagedList(pageNumber, pageSize));
        }



        public ActionResult Cancel(int? id)
        {
            var res = (from a in db_dk.Detail_RegisterPeople.Where(x => x.ID_CT_DKTN == id)
                       select new List_Detail_RegisterPeopleValidation
                       {
                           ID_CT_DKTN = (int)a.ID_CT_DKTN,
                           HoVaTen = a.HoVaTen,
                           NgaySinh = (DateTime)a.NgaySinh,
                           CCCD = a.CCCD,
                           HoKhau = a.HoKhau,
                           CV_ID = (int)a.CV_ID,
                           SoDienThoai = a.SoDienThoai,
                           Ten_NTP = a.Ten_NTP,
                           HoTen_QuanLy = a.HoTen_QuanLy,
                           SoDienThoai_QuanLy = a.SoDienThoai_QuanLy,
                           CapMoi = a.CapMoi,
                           GiaHan = a.GiaHan,
                           BoSungCong = a.BoSungCong,
                           CapLai = a.CapLai,
                           ChuyenNT = a.ChuyenNT,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           KhuVucLamViec = a.KhuVucLamViec,
                           CongLamViec = a.CongLamViec,
                           NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           DKTN_ID = (int)a.DKTN_ID
                       }).ToList();
            List_Detail_RegisterPeopleValidation DO = new List_Detail_RegisterPeopleValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_CT_DKTN = (int)a.ID_CT_DKTN;
                    DO.HoVaTen = a.HoVaTen;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.CCCD = a.CCCD;
                    DO.HoKhau = a.HoKhau;
                    DO.CV_ID = (int)a.CV_ID;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.Ten_NTP = a.Ten_NTP;
                    DO.HoTen_QuanLy = a.HoTen_QuanLy;
                    DO.SoDienThoai_QuanLy = a.SoDienThoai_QuanLy;
                    DO.CapMoi = a.CapMoi;
                    DO.GiaHan = a.GiaHan;
                    DO.BoSungCong = a.BoSungCong;
                    DO.CapLai = a.CapLai;
                    DO.ChuyenNT = a.ChuyenNT;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.KhuVucLamViec = a.KhuVucLamViec;
                    DO.CongLamViec = a.CongLamViec;
                    DO.NhomNT = (int)a.NhomNT;
                    DO.GhiChu = a.GhiChu;
                    DO.DKTN_ID = (int)a.DKTN_ID;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Cancel(List_Detail_RegisterPeopleValidation _DO)
        {
            db_dk.Detail_RegisterPeople_Price(_DO.ID_CT_DKTN, _DO.GhiChu);
            return RedirectToAction("Index_HPDQ", "Detail_List_RegisterPeople_NT", new { id = _DO.DKTN_ID });
        }
    }
}