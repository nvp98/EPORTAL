using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsEquipment;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Equipment.Controllers
{
    public class EquipmentManagementController : Controller
    {
        EquipmentEntities db = new EquipmentEntities();
        EPORTALEntities dbE = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "EquipmentManagement";
        // GET: Equipment/EquipmentManagement
        public ActionResult Index(int? page, string search, DateTime? begind, DateTime? endd, string statusCheck)
        {
            if (search == null) search = "";
            ViewBag.search = search;
            if (statusCheck == null) statusCheck = "";
            var nhanvien = dbE.NhanViens.AsNoTracking().ToList();
            var res = (from a in db.NV_QuanLyThietBi_select(search)
                      let b = nhanvien.FirstOrDefault(x=>x.MaNV == a.MaNV)
                      let c = nhanvien.FirstOrDefault(x => x.MaNV == a.AdminNM)
                      select new NV_QuanLyThietBiValidation
                      {
                          IDQLTB = a.IDQLTB,
                          IDPhongBan = a.IDPhongBan,
                          ServiceTag = a.ServiceTag,
                          IDTB = a.IDTB,
                          //MaNV = a.MaNV == null ? "" : (a.MaNV + "-" + dbE.NhanViens.Where(x => x.MaNV == a.MaNV).Select(x => x.HoTen).Single()),
                          MaNV =b != null? b.MaNV +"-"+ b.HoTen:"" ,
                          Phone = a.Phone,
                          IDSC = a.IDSC,
                          NgayLap = a.NgayLap,
                          Status = a.Status,
                          NgayXL = a.NgayXL,
                          NgayHT = a.NgayHT,
                          NgayNM = a.NgayNM,
                          GhiChu = a.GhiChu,
                          TenThietBi = a.TenThietBi,
                          TenLoiSC = a.TenLoiSC,
                          TenPhongBan = a.TenPhongBan,
                          //AdminNM = a.AdminNM == null ? "" : (a.AdminNM + "-" + dbE.NhanViens.Where(x => x.MaNV == a.AdminNM).Select(x => x.HoTen).Single())
                          AdminNM = c != null ? c.MaNV + "-" + c.HoTen : "",

                      }).OrderByDescending(x=>x.IDQLTB).AsQueryable();
            if (begind != null && endd != null)
            {
                res = res.Where(x => x.NgayLap >= begind && x.NgayLap <= endd);
                ViewBag.begind = begind.Value.ToString("yyyy-MM-dd");
                ViewBag.endd = endd.Value.ToString("yyyy-MM-dd");
            }
            if (statusCheck != "") { res = res.Where(x => x.Status == "0"); ViewBag.statusCheck = statusCheck; }
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            ViewBag.Quyen = 1;
            if (check == 0)
            {
                res = res.Where(x => x.IDPhongBan == EPORTAL.Models.MyAuthentication.IDPhongban);
                ViewBag.Quyen = 0;
            }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);

            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public string TenNV(string MaNV)
        {
            if (MaNV == null) return "";
            var ten = dbE.NhanViens.Where(x => x.MaNV == MaNV).Select(x => x.HoTen).FirstOrDefault();
            if (ten == null) { return MaNV; } else
            {
                return (MaNV + "-" + ten.ToString());
            }
            
        }
        public ActionResult Create()
        {
            List<NV_ThietBi> tb = db.NV_ThietBi.ToList();
            ViewBag.TBList = new SelectList(tb, "IDTB", "TenThietBi");

            List<NV_LoiSuCoTB> lsc = db.NV_LoiSuCoTB.ToList();
            ViewBag.LSCList = new SelectList(lsc, "IDSC", "TenLoiSC");

            var aa = dbE.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();

            var nv3 = aa.Select(x => new EmployeeValidation { MaNV = x.MaNV, HoTen = x.MaNV + " - " + x.HoTen }).ToList();
            ViewBag.Selected = new SelectList(nv3, "MaNV", "HoTen");

            List<PhongBan> pb = dbE.PhongBans.ToList();
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD_AD).First();
            if (check == 0)
            {
                ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan", EPORTAL.Models.MyAuthentication.IDPhongban);
                ViewBag.Quyen = 0;
            }
            else
            {
                ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");
                ViewBag.Quyen = 1;
            }

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(NV_QuanLyThietBiValidation _DO)
        {

            try
            {
                _DO.NgayLap = DateTime.Now.Date;
                _DO.Status = "0";
                var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD_AD).First();
                if (check == 0)
                {
                    _DO.MaNV = EPORTAL.Models.MyAuthentication.Username;
                    _DO.IDPhongBan = EPORTAL.Models.MyAuthentication.IDPhongban;
                }
                var a = db.NV_QuanLyThietBi_insert(_DO.IDPhongBan, _DO.ServiceTag, _DO.IDTB, _DO.MaNV, _DO.Phone, _DO.IDSC, _DO.NgayLap, _DO.Status, _DO.NgayXL, _DO.NgayHT, _DO.NgayNM, _DO.GhiChu);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "EquipmentManagement");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT_AD).First();
            if (check == 0)
            {
                var checkStatus = db.NV_QuanLyThietBi.Where(x => x.IDQLTB == id).FirstOrDefault();
                if (checkStatus != null && checkStatus.Status != "0")
                {
                    TempData["msgError"] = "<script>alert('Yêu cầu đã được xử lý bạn không thể sử dụng chức năng này');</script>";
                    return RedirectToAction("Index", "EquipmentManagement");
                }
                ViewBag.Quyen = 0;
            }
            else { ViewBag.Quyen = 1; };
            var res = (from a in db.NV_QuanLyThietBi.Where(x => x.IDQLTB == id)
                       select new NV_QuanLyThietBiValidation
                       {
                           IDQLTB = a.IDQLTB,
                           IDPhongBan = a.IDPhongBan,
                           ServiceTag = a.ServiceTag,
                           IDTB = a.IDTB,
                           MaNV = a.MaNV,
                           Phone = a.Phone,
                           IDSC = a.IDSC,
                           NgayLap = a.NgayLap,
                           Status = a.Status,
                           NgayXL = a.NgayXL,
                           NgayHT = a.NgayHT,
                           //NgayNM = a.NgayNM,
                           GhiChu = a.GhiChu,
                       }).ToList();
            NV_QuanLyThietBiValidation DO = new NV_QuanLyThietBiValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDQLTB = a.IDQLTB;
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.ServiceTag = a.ServiceTag;
                    DO.IDTB = a.IDTB;
                    DO.MaNV = a.MaNV;
                    DO.Phone = a.Phone;
                    DO.IDSC = a.IDSC;
                    DO.NgayLap = a.NgayLap;
                    DO.Status = a.Status;
                    DO.NgayXL = a.NgayXL;
                    DO.NgayHT = a.NgayHT;
                    //DO.NgayNM = a.NgayNM;
                    DO.GhiChu = a.GhiChu;
                }
            }
            else
            {
                HttpNotFound();
            }
            List<NV_ThietBi> tb = db.NV_ThietBi.ToList();
            ViewBag.TBList = new SelectList(tb, "IDTB", "TenThietBi", DO.IDTB);

            List<NV_LoiSuCoTB> lsc = db.NV_LoiSuCoTB.ToList();
            ViewBag.LSCList = new SelectList(lsc, "IDSC", "TenLoiSC", DO.IDSC);

            List<PhongBan> pb = dbE.PhongBans.ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDPhongBan);

            var aa = dbE.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();

            var nv3 = aa.Select(x => new EmployeeValidation { MaNV = x.MaNV, HoTen = x.MaNV + " - " + x.HoTen }).ToList();
            ViewBag.Selected = new SelectList(nv3, "MaNV", "HoTen", DO.MaNV);

            ViewBag.NgayXL = DO.NgayXL.HasValue == true ? DO.NgayXL.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.NgayHT = DO.NgayHT.HasValue == true ? DO.NgayHT.Value.ToString("yyyy-MM-dd") : "";
            //ViewBag.NgayNM = DO.NgayNM.HasValue == true ? DO.NgayNM.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.NgayLap = DO.NgayLap.HasValue == true ? DO.NgayLap.Value.ToString("yyyy-MM-dd") : "";
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(NV_QuanLyThietBiValidation _DO)
        {

            try
            {
                if (_DO.Status == "0") { _DO.NgayXL = null; _DO.NgayHT = null; }
                var a = db.NV_QuanLyThietBi_update(_DO.IDQLTB, _DO.IDPhongBan, _DO.ServiceTag, _DO.IDTB, _DO.MaNV, _DO.Phone, _DO.IDSC, _DO.NgayLap, _DO.Status, _DO.NgayXL, _DO.NgayHT, _DO.NgayNM, _DO.GhiChu);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "EquipmentManagement");
        }
        public ActionResult Delete(int? id)
        {
            try
            {
                var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
                if (check == 0)
                {
                    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                    return RedirectToAction("Index", "EquipmentManagement");
                }
                var checkStatus = db.NV_QuanLyThietBi.Where(x => x.IDQLTB == id).FirstOrDefault();
                if (checkStatus != null && checkStatus.Status != "0")
                {
                    TempData["msgError"] = "<script>alert('Yêu cầu đã được xử lý bạn không thể sử dụng chức năng này');</script>";
                    return RedirectToAction("Index", "EquipmentManagement");
                }
                db.NV_QuanLyThietBi_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "EquipmentManagement");
        }
        public ActionResult EditStatus(int? id)
        {
            try
            {
                var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT_AD).First();
                if (check == 0)
                {
                    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                    return RedirectToAction("Index", "EquipmentManagement");
                }
                var model = db.NV_QuanLyThietBi.SingleOrDefault(x => x.IDQLTB == id);
                switch (model.Status)
                {
                    case "0":
                        model.Status = "1";
                        model.AdminNM = EPORTAL.Models.MyAuthentication.Username;
                        model.NgayXL = DateTime.Now.Date;
                        db.SaveChanges();
                        break;
                    case "1":
                        model.Status = "2";
                        model.NgayHT = DateTime.Now.Date;
                        db.SaveChanges();
                        break;
                    //case "2":
                    //    model.Status = "3";
                    //    model.NgayNM = DateTime.Now.Date;
                    //    db.SaveChanges();
                    //    break;
                    //case "3":
                    //    // code block
                    //    result = true;
                    //    break;
                    default:
                        TempData["msgSuccess"] = "<script>alert('Thay đổi thất bại!');</script>";
                        break;
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Thay đổi thất bại: " + ex.Message + "');</script>";
            }

            return RedirectToAction("Index", "EquipmentManagement");
        }
        public ActionResult ExportToExcel(string search, DateTime? begind, DateTime? endd, string statusCheck)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EX).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Index", "EquipmentManagement");
            }
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSSCTB.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSSCTB_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DSSCTB");
                if (search == null) search = "";
                ViewBag.search = search;
                if (statusCheck == null) statusCheck = "";
                var res = from a in db.NV_QuanLyThietBi_select(search)
                          select new NV_QuanLyThietBiValidation
                          {
                              IDQLTB = a.IDQLTB,
                              IDPhongBan = a.IDPhongBan,
                              ServiceTag = a.ServiceTag,
                              IDTB = a.IDTB,
                              MaNV = a.MaNV,
                              Phone = a.Phone,
                              IDSC = a.IDSC,
                              NgayLap = a.NgayLap,
                              Status = a.Status,
                              NgayXL = a.NgayXL,
                              NgayHT = a.NgayHT,
                              NgayNM = a.NgayNM,
                              GhiChu = a.GhiChu,
                              TenThietBi = a.TenThietBi,
                              TenLoiSC = a.TenLoiSC,
                              TenPhongBan = a.TenPhongBan

                          };
                if (begind != null && endd != null)
                {
                    res = res.Where(x => x.NgayLap >= begind && x.NgayLap <= endd);
                    ViewBag.begind = begind.Value.ToString("yyyy-MM-dd");
                    ViewBag.endd = endd.Value.ToString("yyyy-MM-dd");
                }
                if (statusCheck != "") { res = res.Where(x => x.Status == "0"); ViewBag.statusCheck = statusCheck; }
                var resList = res.OrderByDescending(x => x.IDQLTB).ToList();
                //if (IDquyen != 1)
                //{
                //    res = res.Where(x => x.IDPhongBan == EPORTAL.Models.MyAuthentication.IDPhongban).ToList();
                //}
                var cxl = "Chờ xử lý";
                var dxl = "Đang xử lý";
                var daxl = "Đã xử lý";
                var gm = "Đã giao máy";
                if (resList.Count > 0)
                {
                    int row = 1, rowlast = 1, stt = 0;
                    foreach (var item in resList)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = item.TenPhongBan;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.ServiceTag;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = item.TenThietBi;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.MaNV;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("E" + row).Value = item.Phone;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("F" + row).Value = item.TenLoiSC;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("G" + row).Value = item.NgayLap.Value.ToString("dd/MM/yyyy");
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        if (item.Status == "0")
                        {
                            Worksheet.Cell("H" + row).Value = cxl;
                        }
                        else if (item.Status == "1")
                        {
                            Worksheet.Cell("H" + row).Value = dxl;
                        }
                        else if (item.Status == "2")
                        {
                            Worksheet.Cell("H" + row).Value = gm;
                            //} else if (item.Status == "3")
                            //{
                            //    Worksheet.Cell("H" + row).Value = gm;
                        }
                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("I" + row).Value = item.NgayHT.HasValue == true ? item.NgayXL.Value.ToString("dd/MM/yyyy") : "";
                        Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("I" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                        //Worksheet.Cell("I" + row).Value = item.NgayXL.HasValue == true ? item.NgayXL.Value.ToString("dd/MM/yyyy") : "";
                        //Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        //Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Worksheet.Cell("I" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        //Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                        //Worksheet.Cell("J" + row).Value = item.NgayHT.HasValue == true ? item.NgayHT.Value.ToString("dd/MM/yyyy") : "";
                        //Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        //Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Worksheet.Cell("J" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        //Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;

                        //Worksheet.Cell("K" + row).Value = item.NgayNM.HasValue == true ? item.NgayNM.Value.ToString("dd/MM/yyyy") : "";
                        //Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        //Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Worksheet.Cell("K" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        //Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;

                        //Worksheet.Cell("L" + row).Value = item.GhiChu;
                        //Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        //Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("J" + row).Value = item.GhiChu;
                        Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;


                        row = rowlast - 1;
                    }
                    Worksheet.Range("A2:J" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:J" + (row)).Style.Font.SetFontSize(10);
                    //Worksheet.Range("A2:I" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Range("A2:I" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "DSSCTB.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/EquipmentManagement'</script>";
                    return RedirectToAction("Index", "EquipmentManagement");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/EquipmentManagement'</script>";
                return RedirectToAction("Index", "EquipmentManagement");
            }

        }
    }
}