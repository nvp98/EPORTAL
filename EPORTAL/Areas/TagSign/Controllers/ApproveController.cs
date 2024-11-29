using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using iTextSharp.text.pdf;
using iTextSharp.text;
using PagedList;
using Rotativa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa.Options;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsOrganizational;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2010.Excel;
using ClosedXML.Excel;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ApproveController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Approve";

        // GET: TagSign/Approve
        public ActionResult Index(int? page, string search, DateTime? begind, DateTime? endd)
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

            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var nd = db.AuthorizationContractors.Where(x => x.IDNhanVien == MyAuthentication.ID).SingleOrDefault();

            var idnv = db.NhanViens.Where(x => x.ID == nd.IDNhanVien).FirstOrDefault();

            var res = (from kd in db_dk.TK_CardRegistrationInfor.Where(x=>x.NhanVienID == idnv.ID && x.TinhTrangID == 0)
                       join ca in db_dk.DK_CardRegistrationInfor on kd.DKTID equals ca.IDDKT
                       select new TK_CardRegistrationInforValidation
                       {
                           IDTKDKT = (int)kd.IDTKDKT,
                           DKTID = (int)kd.DKTID,
                           NoiDung = ca.NoiDung,
                           NgayTrinh = (DateTime?)ca.NgayDangKy??default,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet??default,
                           GhiChu = kd.GhiChu
                       });
            if (begind == null && endd == null)
            {
                res = res.Where(x => x.NgayTrinh >= startDay && x.NgayTrinh <= endDay);
            }
            else
            {
                res = res.Where(x => x.NgayTrinh >= begind && x.NgayTrinh <= endd);
            }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x=>x.NgayTrinh).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Detail(int? id, int? page)
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
                      join cr in db_dk.DK_CardRegistrationInfor on a.IDDKT equals cr.IDDKT
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
                          IDDKT = (int)a.IDDKT,
                          TinhTrangID = (int)cr.TinhTrangID
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
        public ActionResult Approve(int? id, int? idnv)
        {

            var res = (from kd in db_dk.TK_CardRegistrationInfor.Where(x => x.IDTKDKT == id)
                       select new TK_CardRegistrationInforValidation
                       {
                           IDTKDKT = (int)kd.IDTKDKT,
                           DKTID = (int)kd.DKTID,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();
            TK_CardRegistrationInforValidation DO = new TK_CardRegistrationInforValidation();
            if (res.Count > 0)
            {
                foreach (var kd in res)
                {
                    DO.IDTKDKT = (int)kd.IDTKDKT;
                    DO.DKTID = (int)kd.DKTID;
                    DO.CapDuyet = (int)kd.CapDuyet;
                    DO.TinhTrangID = (int)kd.TinhTrangID;
                    DO.NhanVienID = (int)kd.NhanVienID;
                    DO.NgayDuyet = (DateTime?)kd.NgayDuyet ?? default;
                    DO.GhiChu = kd.GhiChu;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Approve(TK_CardRegistrationInforValidation _DO)
        {
            try
            {
                DateTime DayNow = DateTime.Now;
                var IDDKT = db_dk.DK_CardRegistrationInfor.Where(x => x.IDDKT == _DO.DKTID).FirstOrDefault();
                var IDTK = db_dk.TK_CardRegistrationInfor.Where(x => x.DKTID == _DO.DKTID && x.NhanVienID == _DO.NhanVienID).FirstOrDefault();
                if (IDTK != null)
                {
                    var update = db_dk.TK_CardRegistrationInfor_UpdateTK(IDTK.IDTKDKT,1,DayNow,_DO.GhiChu);
                }
                var Update = db_dk.TK_CardRegistrationInfor.Where(x => x.DKTID == _DO.DKTID && x.TinhTrangID == 0 || x.DKTID == _DO.DKTID && x.TinhTrangID == 0 && x.CapDuyet != 1 && x.CapDuyet != 2).FirstOrDefault();
                if (Update == null)
                {
                    db_dk.DK_CardRegistrationInfor_UpdateTK(_DO.DKTID, 2);
                }
                TempData["msgSuccess"] = "<script>alert('Phê duyệt thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Phê duyệt thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Approve");
        }

        public ActionResult CancelApprove( int? id)
        {

            var res = (from kd in db_dk.TK_CardRegistrationInfor.Where(x => x.IDTKDKT == id)
                       select new TK_CardRegistrationInforValidation
                       {
                           IDTKDKT = (int)kd.IDTKDKT,
                           DKTID = (int)kd.DKTID,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();
            TK_CardRegistrationInforValidation DO = new TK_CardRegistrationInforValidation();
            if (res.Count > 0)
            {
                foreach (var kd in res)
                {
                    DO.IDTKDKT = (int)kd.IDTKDKT;
                    DO.DKTID = (int)kd.DKTID;
                    DO.CapDuyet = (int)kd.CapDuyet;
                    DO.TinhTrangID = (int)kd.TinhTrangID;
                    DO.NhanVienID = (int)kd.NhanVienID;
                    DO.NgayDuyet = (DateTime?)kd.NgayDuyet ?? default;
                    DO.GhiChu = kd.GhiChu;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult CancelApprove(TK_CardRegistrationInforValidation _DO)
        {
            try
            {
                DateTime DayNow = DateTime.Now;
                var IDDKT = db_dk.DK_CardRegistrationInfor.Where(x => x.IDDKT == _DO.DKTID).FirstOrDefault();
                var IDTK = db_dk.TK_CardRegistrationInfor.Where(x => x.DKTID == _DO.DKTID && x.NhanVienID == _DO.NhanVienID).FirstOrDefault();
                if (IDTK != null)
                {
                    var update = db_dk.TK_CardRegistrationInfor_UpdateTK(IDTK.IDTKDKT, 2, DayNow,_DO.GhiChu);
                }
                db_dk.DK_CardRegistrationInfor_UpdateTK(_DO.DKTID, 3);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Hủy phê duyệt thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Approve");
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
                if(_DO.GhiChu == null) {
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

            return RedirectToAction("Detail", "Approve", new { id = _DO.IDDKT });
        }

        public ActionResult ExportToExcel(int? id)
        {
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ĐKT.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_ĐKT_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DKT");
                var List = db_dk.DK_DetailCardRegistrationInfor.Where(x=>x.IDDKT == id).ToList();
                if (List.Count > 0)
                {
                    int row = 5, rowlast = 1, stt = 0;
                    foreach (var item in List)
                    {
                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.NgaySinh;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.HoTen;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.CCCD;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("E" + row).Value = item.HoKhau;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        var IDCV = db.NT_Position.Where(x=>x.IDCV == item.ChucVu).FirstOrDefault();
                        if(IDCV != null)
                        {
                            Worksheet.Cell("F" + row).Value = IDCV.TenCV;
                            Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;
                        }

                        Worksheet.Cell("G" + row).Value = item.SoDienThoai;
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        if (item.TheLuuTru != "")
                        {
                            Worksheet.Cell("H" + row).Value = "X";
                            Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.TheRaVaoKLH != "")
                        {
                            Worksheet.Cell("I" + row).Value = "X";
                            Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.DienThoaiTM != "")
                        {
                            Worksheet.Cell("J" + row).Value = "X";
                            Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.RaVaoCang != "")
                        {
                            Worksheet.Cell("K" + row).Value = "X";
                            Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;
                        }

                        Worksheet.Cell("L" + row).Value = item.ThoiHanThe;
                        Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("L" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;

                        string ListKhuVuc = "";
                        string[] arrList = item.KhuVucLamViec.Split(',');
                        foreach (var ar in arrList)
                        {
                            if (ar != "")
                            {
                                int IDKV = Convert.ToInt32(ar);
                                var NameKhuVuc = db.NT_Workplace.Where(x => x.IDKV == IDKV).FirstOrDefault();
                                ListKhuVuc = String.Concat(NameKhuVuc.TenKV + " , " + ListKhuVuc);
                            }

                        }
                        Worksheet.Cell("M" + row).Value = ListKhuVuc;
                        Worksheet.Cell("M" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("M" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("M" + row).Style.Alignment.WrapText = true;

                        string ListCong = "";
                        string[] arrListStr = item.Cong.Split(',');
                        foreach (var ar in arrListStr)
                        {
                            int IDGate = Convert.ToInt32(ar);
                            var Cong = db.NT_Gate.Where(x => x.IDGate == IDGate).FirstOrDefault();
                            ListCong = String.Concat(Cong.Gate + " , " + ListCong);
                        }
                        Worksheet.Cell("N" + row).Value = ListCong;
                        Worksheet.Cell("N" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("N" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("N" + row).Style.Alignment.WrapText = true;

                        var IDNNT = db.NT_ContractorGroup.Where(x => x.IDGroup == item.NhomNT).FirstOrDefault();
                        if (IDNNT != null)
                        {
                            Worksheet.Cell("O" + row).Value = IDNNT.NameContractorGroup;
                            Worksheet.Cell("O" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("O" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("O" + row).Style.Alignment.WrapText = true;
                        }

                        row = rowlast - 1;
                    }
                    Worksheet.Range("A6:O" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A6:O" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A6:O" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A6:O" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Danh sách đăng ký thẻ.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Có lỗi khi xuất dữ liệu');</script>";
                    return RedirectToAction("Index", "Approve");
                }

            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi xuất dữ liệu');</script>";
                return RedirectToAction("Index", "Approve");
            }

        }
        public ActionResult Show(int? page, int? IDDKT)
        {
            var res = (from kd in db_dk.TK_CardRegistrationInfor.Where(x => x.DKTID == IDDKT)
                       join ca in db_dk.DK_CardRegistrationInfor on kd.DKTID equals ca.IDDKT
                       select new TK_CardRegistrationInforValidation
                       {
                           IDTKDKT = (int)kd.IDTKDKT,
                           DKTID = (int)kd.DKTID,
                           NoiDung = ca.NoiDung,
                           NgayTrinh = (DateTime?)ca.NgayDangKy ?? default,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
    }
}