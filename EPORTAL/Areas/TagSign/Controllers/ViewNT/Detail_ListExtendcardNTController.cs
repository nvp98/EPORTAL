using EPORTAL.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class Detail_ListExtendcardNTController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_UNISEntities dbUN = new EPORTAL_UNISEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        // GET: TagSign/Detail_ListExtendcardNT
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

        public ActionResult Delete(int? id, int? idght)
        {
            try
            {
                db_dk.DK_DetailCardExtend_Delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }

            return RedirectToAction("Form", "Detail_ListExtendcardNT", new { id = idght });
        }
        public ActionResult Form(int? id)
        {
            var TD = db_dk.DK_CardExtend.Where(x => x.IDGHT == id).SingleOrDefault();

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName", Convert.ToInt32(TD.NTID));

            List<NT_Contract> hd = db.NT_Contract.ToList();
            ViewBag.HDList = new SelectList(hd, "IDContract", "Somecontracts", Convert.ToInt32(TD.HDID));

            List<ModelsView360.PhongBan> pb = GetPhongBans().ToList();
            ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", Convert.ToInt32(TD.PhongBanID));

            List<NT_Position> cv = db.NT_Position.ToList();
            ViewBag.IDCV = new SelectList(cv, "IDCV", "TenCV");

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            List<NT_Workingtime> ca = db.NT_Workingtime.ToList();
            ViewBag.IDCA = new SelectList(ca, "IDCA", "TenCA");

            List<NT_Workplace> kv = db.NT_Workplace.ToList();
            ViewBag.IDKV = new SelectList(kv, "IDKV", "TenKV");

            if (id != null)
            {
                ViewData["id"] = id;
            }
            var res = (from a in db_dk.DK_DetailCardExtend.Where(x => x.IDGHT == id)
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
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }

        private DbSet<ModelsView360.PhongBan> GetPhongBans()
        {
            return db.PhongBans;
        }

        [HttpPost]
        public ActionResult Form(DK_Detail_ListExtendcardNTValidation _DO, FormCollection collection)
        {
            int IDGHT = 0;
            int stt = 0;
            DateTime DayUpload = DateTime.Now;
            var Day_HT = DayUpload.ToString("yyyyMMdd");
            DateTime DayUpload_QK = DateTime.Now.AddMonths(-2);
            var Day_QK = DayUpload_QK.ToString("yyyyMMdd");
            if (collection.Count > 2)
            {
                try
                {
                    var ListDS = new List<DK_Detail_ListExtendcardNTValidation>();
                    string GHTID = collection["XacNhan"];
                    IDGHT = Convert.ToInt32(GHTID);
                    foreach (var key in collection.AllKeys)
                    {
                        if (key != "__RequestVerificationToken")
                        {
                            var DO = db_dk.DK_CardExtend.Where(x => x.IDGHT == IDGHT).FirstOrDefault();
                            db_dk.DK_CardExtend_Update(DO.IDGHT, collection["NoiDung"], Convert.ToInt32(collection["NTList"]), Convert.ToInt32(collection["HDList"]), Convert.ToInt32(collection["IDPhongBan"]), DO.NgayDangKy, DO.GhiChu, Convert.ToInt32(collection["NT"]));
                            if (key.Split('_')[0] == "GhiChu")
                            {
                                string CheckCard = collection["luutru_" + key.Split('_')[1]];
                                if (CheckCard == "1")
                                {
                                    ListDS.Add(new DK_Detail_ListExtendcardNTValidation()
                                    {
                                        HoTen = collection["HoTen_" + key.Split('_')[1]],
                                        CCCD = collection["CCCD_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["NgaySinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        HoKhau = collection["HoKhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["ChucVu_" + key.Split('_')[1]]),
                                        SoDienThoai = collection["SoDienThoai_" + key.Split('_')[1]],
                                        CapLai = collection["luutru_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["ThoiHanThe_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        KhuVucLamViec = collection["KhuVucLamViec_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["NhomNT_" + key.Split('_')[1]]),
                                        Cong = collection["Cong_" + key.Split('_')[1]],
                                        GhiChu = collection["GhiChu_" + key.Split('_')[1]]
                                    });
                                }
                                else if (CheckCard == "2")
                                {
                                    ListDS.Add(new DK_Detail_ListExtendcardNTValidation()
                                    {
                                        HoTen = collection["HoTen_" + key.Split('_')[1]],
                                        CCCD = collection["CCCD_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["NgaySinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        HoKhau = collection["HoKhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["ChucVu_" + key.Split('_')[1]]),
                                        SoDienThoai = collection["SoDienThoai_" + key.Split('_')[1]],
                                        GiaHan = collection["luutru_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["ThoiHanThe_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        KhuVucLamViec = collection["KhuVucLamViec_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["NhomNT_" + key.Split('_')[1]]),
                                        Cong = collection["Cong_" + key.Split('_')[1]],
                                        GhiChu = collection["GhiChu_" + key.Split('_')[1]]
                                    });

                                }
                                else if (CheckCard == "3")
                                {
                                    ListDS.Add(new DK_Detail_ListExtendcardNTValidation()
                                    {
                                        HoTen = collection["HoTen_" + key.Split('_')[1]],
                                        CCCD = collection["CCCD_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["NgaySinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        HoKhau = collection["HoKhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["ChucVu_" + key.Split('_')[1]]),
                                        SoDienThoai = collection["SoDienThoai_" + key.Split('_')[1]],
                                        ThongTinLuuTru = collection["luutru_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["ThoiHanThe_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        KhuVucLamViec = collection["KhuVucLamViec_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["NhomNT_" + key.Split('_')[1]]),
                                        Cong = collection["Cong_" + key.Split('_')[1]],
                                        GhiChu = collection["GhiChu_" + key.Split('_')[1]]
                                    });

                                }
                                else if (CheckCard == "4")
                                {
                                    ListDS.Add(new DK_Detail_ListExtendcardNTValidation()
                                    {
                                        HoTen = collection["HoTen_" + key.Split('_')[1]],
                                        CCCD = collection["CCCD_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["NgaySinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        HoKhau = collection["HoKhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["ChucVu_" + key.Split('_')[1]]),
                                        SoDienThoai = collection["SoDienThoai_" + key.Split('_')[1]],
                                        DTTM = collection["luutru_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["ThoiHanThe_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        KhuVucLamViec = collection["KhuVucLamViec_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["NhomNT_" + key.Split('_')[1]]),
                                        Cong = collection["Cong_" + key.Split('_')[1]],
                                        GhiChu = collection["GhiChu_" + key.Split('_')[1]]
                                    });

                                }
                                else if (CheckCard == "5")
                                {
                                    ListDS.Add(new DK_Detail_ListExtendcardNTValidation()
                                    {
                                        HoTen = collection["HoTen_" + key.Split('_')[1]],
                                        CCCD = collection["CCCD_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["NgaySinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        HoKhau = collection["HoKhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["ChucVu_" + key.Split('_')[1]]),
                                        SoDienThoai = collection["SoDienThoai_" + key.Split('_')[1]],
                                        RaVaoCang = collection["luutru_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["ThoiHanThe_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        KhuVucLamViec = collection["KhuVucLamViec_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["NhomNT_" + key.Split('_')[1]]),
                                        Cong = collection["Cong_" + key.Split('_')[1]],
                                        GhiChu = collection["GhiChu_" + key.Split('_')[1]]
                                    });

                                }

                            }
                        }

                    }

                    foreach (var item in ListDS)
                    {
                        var CheckVP = db_nt.NT_NhanVienVP.Where(x => x.CCCD == item.CCCD && x.TinhTrang == 0).FirstOrDefault();
                        var Check_UN = dbUN.LogUNIS_Check(item.CCCD, Day_HT, Day_QK).Count();
                        if (item.HoTen != "" && item.CCCD != "" && CheckVP == null && IDGHT != 0 && Check_UN > 0)
                        {
                            var insert = db_dk.DK_DetailCardExtend_Insert
                                 (item.HoTen,
                                   item.CCCD,
                                   item.NgaySinh,
                                   item.HoKhau,
                                   item.ChucVu,
                                   item.SoDienThoai,
                                   item.CapLai,
                                   item.GiaHan,
                                   item.ThongTinLuuTru,
                                   item.DTTM,
                                   item.RaVaoCang,
                                   item.ThoiHanThe,
                                   item.KhuVucLamViec,
                                   item.Cong,
                                   item.NhomNT,
                                   "",
                                   IDGHT);
                            stt++;
                            TempData["msgSuccess"] = "<script>alert('Thêm mới thành công " + stt + " dòng');</script>";
                        }
                        else
                        {
                            var Count = db_dk.DK_CardExtend.Where(x => x.IDGHT == IDGHT).Count();
                            if (IDGHT == 0)
                            {
                                if (Count != 0)
                                {
                                    db_dk.DK_DetailCardRegistrationInfor_Delete(IDGHT);
                                }
                                db_dk.DK_CardRegistrationInfor_Delete(IDGHT);

                                TempData["msgSuccess"] = "<script>alert('Vui lòng nhập kiểm tra lại thông tin nhân viên: " + item.HoTen + "');</script>";

                            }

                        }

                    }
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Vui lòng nhập đầy đủ thông tin');</script>";
                }
            }
            return RedirectToAction("Index", "ListExtendcardNT");
        }

        public ActionResult Edit(int? id)
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
        public ActionResult Edit(DK_Detail_ListExtendcardNTValidation _DO, FormCollection collection)
        {
            var DO = db_dk.DK_DetailCardExtend.Where(x=>x.IDCTGH == _DO.IDCTGH).FirstOrDefault();
            if(_DO.KhuVucLamViec != null && _DO.Selected == null)
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

              var a = db_dk.DK_DetailCardExtend_Update
                 (_DO.IDCTGH,
                 _DO.HoTen,
                 _DO.CCCD,
                 _DO.NgaySinh,
                 _DO.HoKhau,
                 _DO.ChucVu,
                 _DO.SoDienThoai,
                 _DO.CapLai,
                 _DO.GiaHan,
                 _DO.ThongTinLuuTru,
                 _DO.DTTM,
                 _DO.RaVaoCang,
                 _DO.ThoiHanThe,
                  NameKhuVuc,
                  DO.Cong,
                 _DO.NhomNT,
                 _DO.GhiChu,
                 _DO.IDGHT);

                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }    
            else if(_DO.KhuVucLamViec == null && _DO.Selected != null)
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
               var a = db_dk.DK_DetailCardExtend_Update
                 (_DO.IDCTGH,
                 _DO.HoTen,
                 _DO.CCCD,
                 _DO.NgaySinh,
                 _DO.HoKhau,
                 _DO.ChucVu,
                 _DO.SoDienThoai,
                 _DO.CapLai,
                 _DO.GiaHan,
                 _DO.ThongTinLuuTru,
                 _DO.DTTM,
                 _DO.RaVaoCang,
                 _DO.ThoiHanThe,
                  DO.KhuVucLamViec,
                 Name,
                 _DO.NhomNT,
                 _DO.GhiChu,
                 _DO.IDGHT);

                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }  
            else if(_DO.KhuVucLamViec != null && _DO.Selected != null)
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
             var a = db_dk.DK_DetailCardExtend_Update
                 (_DO.IDCTGH,
                 _DO.HoTen,
                 _DO.CCCD,
                 _DO.NgaySinh,
                 _DO.HoKhau,
                 _DO.ChucVu,
                 _DO.SoDienThoai,
                 _DO.CapLai,
                 _DO.GiaHan,
                 _DO.ThongTinLuuTru,
                 _DO.DTTM,
                 _DO.RaVaoCang,
                 _DO.ThoiHanThe,
                  NameKhuVuc,
                  Name,
                 _DO.NhomNT,
                 _DO.GhiChu,
                 _DO.IDGHT);

                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            else if (_DO.KhuVucLamViec == null && _DO.Selected == null)
            {
                var a = db_dk.DK_DetailCardExtend_Update
                    (_DO.IDCTGH,
                    _DO.HoTen,
                    _DO.CCCD,
                    _DO.NgaySinh,
                    _DO.HoKhau,
                    _DO.ChucVu,
                    _DO.SoDienThoai,
                    _DO.CapLai,
                    _DO.GiaHan,
                    _DO.ThongTinLuuTru,
                    _DO.DTTM,
                    _DO.RaVaoCang,
                    _DO.ThoiHanThe,
                     DO.KhuVucLamViec,
                     DO.Cong,
                    _DO.NhomNT,
                    _DO.GhiChu,
                    _DO.IDGHT);

                TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            return RedirectToAction("Form", "Detail_ListExtendcardNT", new {id = _DO.IDGHT});
        }
    }
}