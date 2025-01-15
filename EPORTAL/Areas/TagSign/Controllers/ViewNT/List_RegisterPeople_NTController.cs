using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using iTextSharp.text.pdf;
using iTextSharp.text;
using PagedList;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa.Options;
using DocumentFormat.OpenXml.Presentation;
using Font = iTextSharp.text.Font;
using System.Globalization;
using DocumentFormat.OpenXml.Wordprocessing;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsServey;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Web.Services.Description;
using ClosedXML.Excel;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class List_RegisterPeople_NTController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        EPORTAL_UNISEntities dbUN = new EPORTAL_UNISEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "List_RegisterPeople_NT";

        // GET: TagSign/List_RegisterPeople_NT
        public ActionResult Index(int? page, DateTime? begind, DateTime? endd)
        {
            var res = (from a in db_dk.RegisterPeoples.Where(x => x.NhanVienNT_ID == Models.MyAuthentication.ID)
                       select new List_RegisterPeopleValidation
                       {
                           ID_DKTN = (int)a.ID_DKTN,
                           NoiDung = a.NoiDung,
                           BPQL_ID = (int?)a.BPQL_ID ?? default,
                           NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default,
                           NhaThau_ID = (int)a.NhaThau_ID,
                           HopDong = a.HopDong,
                           NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                           File_CCAT = a.File_CCAT,
                           File_IMG = a.File_IMG,
                           TrinhKy_ID = (int?)a.TrinhKy_ID ?? default,
                           TinhTrang_ID = (int?)a.TinhTrang_ID ?? default,
                           LoaiNT_ID = (int?)a.LoaiNT_ID ?? default
                       }).ToList();
            //var res = db_dk.RegisterPeoples.ToList();
            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.NgayTrinhKy).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Index_HPDQ(int? page, DateTime? begind, DateTime? endd, string search)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);
            var res = (from a in db_dk.RegisterPeoples.Where(x=>x.TinhTrang_ID != 0)
                       select new List_RegisterPeopleValidation
                       {
                           ID_DKTN = (int)a.ID_DKTN,
                           NoiDung = a.NoiDung,
                           BPQL_ID = (int?)a.BPQL_ID ?? default,
                           NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default,
                           NhaThau_ID = (int)a.NhaThau_ID,
                           HopDong = a.HopDong,
                           NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                           File_CCAT = a.File_CCAT,
                           File_IMG = a.File_IMG,
                           TrinhKy_ID = (int?)a.TrinhKy_ID ?? default,
                           TinhTrang_ID = (int?)a.TinhTrang_ID ?? default,
                           LoaiNT_ID = (int?)a.LoaiNT_ID ?? default
                       }).ToList();

            if (begind == null && endd == null)
            {
                res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay).ToList();
            }
            else
            {
                res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd).ToList();
            }

            if (search != "" && search != null)
            {
                var ID_NT = db.NT_Partner.Where(x=>x.FullName.Contains(search)).FirstOrDefault();   
                res = res.Where(x => x.NhaThau_ID == ID_NT.ID).ToList();
            }    
                if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.NgayTrinhKy).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Show(int? page, int? DKTN_ID)
        {
            var res = (from kd in db_dk.SignOff_Flow.Where(x => x.DKTN_ID == DKTN_ID)
                       join ca in db_dk.RegisterPeoples on kd.DKTN_ID equals ca.ID_DKTN
                       select new Follow_RegisterPeopleValidation
                       {
                           ID_TK_TN = (int)kd.ID_TK_TN,
                           DKTN_ID = (int)kd.DKTN_ID,
                           NoiDung = ca.NoiDung,
                           NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                           NT_ID = (int?)ca.NhaThau_ID ?? default,
                           HopDong = ca.HopDong,
                           File_CCAT = ca.File_CCAT,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.NgayDuyet).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Show_HPDQ(int? page, int? DKTN_ID)
        {
            var res = (from kd in db_dk.SignOff_Flow.Where(x => x.DKTN_ID == DKTN_ID)
                       join ca in db_dk.RegisterPeoples on kd.DKTN_ID equals ca.ID_DKTN
                       select new Follow_RegisterPeopleValidation
                       {
                           ID_TK_TN = (int)kd.ID_TK_TN,
                           DKTN_ID = (int)kd.DKTN_ID,
                           NoiDung = ca.NoiDung,
                           NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                           NT_ID = (int?)ca.NhaThau_ID ?? default,
                           HopDong = ca.HopDong,
                           File_CCAT = ca.File_CCAT,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.NgayDuyet).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Edit_HPDQ(int? id)
        {
            var ID_DKTN = db_dk.SignOff_Flow.Where(x=>x.ID_TK_TN == id).FirstOrDefault();
            db_dk.SignOff_Flow_Update(id, null, 0, null);

            db_dk.RegisterPeople_UpdateFlow(ID_DKTN.DKTN_ID, 1);

            return RedirectToAction("Show_HPDQ", "List_RegisterPeople_NT", new {id = ID_DKTN.DKTN_ID });
        }

        public ActionResult CancelApprove( int? id, int? idl)
        {
            var res = (from kd in db_dk.SignOff_Flow.Where(x => x.ID_TK_TN == id)
                       select new Follow_RegisterPeopleValidation
                       {
                           ID_TK_TN = (int)kd.ID_TK_TN,
                           DKTN_ID = (int)kd.DKTN_ID,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();
            Follow_RegisterPeopleValidation DO = new Follow_RegisterPeopleValidation();
            if (res.Count > 0)
            {
                foreach (var kd in res)
                {
                    DO.ID_TK_TN = (int)kd.ID_TK_TN;
                    DO.DKTN_ID = (int)kd.DKTN_ID;
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
        public ActionResult CancelApprove(Follow_RegisterPeopleValidation _DO, int? idl)
        {
            try
            {
                db_dk.SignOff_Flow_Update(_DO.ID_TK_TN, _DO.NgayDuyet, 1, _DO.GhiChu);

                TempData["msgSuccess"] = "<script>alert('Hủy duyệt thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Hủy phê duyệt thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Show_HPDQ", "List_RegisterPeople_NT", new { id = _DO.DKTN_ID });
        }


        public ActionResult Delete(int? id)
        {
            try
            {
                var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                foreach (var item in ID_CT)
                {
                    db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                }
                db_dk.RegisterPeople_Delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "List_RegisterPeople_NT");
        }

        public FileResult DownloadExcel()
        {
            string path = "BM_Đăng ký thẻ người.xlsx";
            string filePath = Server.MapPath("~/App_Data/" + path);
            if (!System.IO.File.Exists(filePath))
            {
                return null; // Xử lý lỗi nếu file không tồn tại
            }
            var kvlv = db.NT_Workplace.ToList();
            var cong = db.NT_Gate.ToList();
            var cv = db.NT_Position.ToList();

            // Tạo workbook từ file gốc
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet =  workbook.Worksheet("DL");
                for (var i = 0; i < kvlv.Count; i++)
                {
                    worksheet.Cell(i+2, 3).Value = kvlv[i].TenKV;
                }
                for (var i = 0; i < cong.Count; i++)
                {
                    worksheet.Cell(i+2, 8).Value = cong[i].Gate;
                }
                for (var i = 0; i < cv.Count; i++)
                {
                    worksheet.Cell(i+2, 5).Value = cv[i].TenCV;
                }
                // Lưu lại file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", path);
                }
            }
        }
        public ActionResult Create()
        {
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName");

            List<SignerType> kv = db_dk.SignerTypes.ToList();
            ViewBag.IDLTK = new SelectList(kv, "ID_LTK", "TenLoai");

            List<ModelsView360.PhongBan> pb = db.PhongBans.Where(x=>x.status==1).ToList();
            ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            return PartialView();
        }

        [HttpPost]
        public ActionResult InsertEdit(HttpPostedFileBase file, int id)
        {
            int status = 1, dtc = 0;

            string LoaiTK = "", msg = "";
            string HoVaTen = "";
            string filePath = string.Empty;
            string path = Server.MapPath("~/PDFHocAT/");
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string pathex = Server.MapPath("~/UploadedFiles/DKHocATEX/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(pathex);
                }
                filePath = pathex + Path.GetFileName(file.FileName);

                file.SaveAs(filePath);
                Stream stream = file.InputStream;

                IExcelDataReader reader = null;
                if (file.FileName.ToLower().EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (file.FileName.ToLower().EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else
                {
                    msg = "Vui lòng chọn đúng định dạng file Excel";
                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = msg });
                }
                DataSet result = reader.AsDataSet();
                DataTable dt = result.Tables[0];
                reader.Close();
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        for (int i = 4; i < dt.Rows.Count; i++)
                        {
                            dtc++;
                            string HoTen = dt.Rows[i][1].ToString().Trim();
                            if (HoTen != "")
                            {
                                HoVaTen = HoTen;
                                string BirthDay = dt.Rows[i][2].ToString().Trim();

                                try
                                {
                                    if (BirthDay == "")
                                    {
                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                        return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Kiểm tra lại ngày sinh. Nhân viên : {HoVaTen} " });
                                    }
                                    DateTime NgaySinh1 = DateTime.ParseExact(BirthDay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                }
                                catch
                                {

                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Kiểm tra lại định dạng ngày của Ngày sinh. Nhân viên : {HoVaTen} " });
                                }
                                DateTime NgaySinh = DateTime.ParseExact(BirthDay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


                                string CCCD = dt.Rows[i][3].ToString().Trim();
                                string HoKhau = dt.Rows[i][4].ToString().Trim();

                                string ChucVu = dt.Rows[i][5].ToString().Trim();
                                var IDCV = db.NT_Position.Where(x => x.TenCV == ChucVu).FirstOrDefault();
                                if (IDCV == null)
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng kiểm tra lại chức vụ. Nhân viên : {HoVaTen} " });

                                }
                                string SoDienThoai = dt.Rows[i][6].ToString().Trim();
                                string Ten_NTP = dt.Rows[i][7].ToString().Trim();
                                string HoTen_QuanLy = dt.Rows[i][8].ToString().Trim();
                                string SoDienThoai_QuanLy = dt.Rows[i][9].ToString().Trim();
                                string CapMoi = dt.Rows[i][10].ToString().Trim();
                                if (CapMoi != "")
                                {
                                    var CheckCCCD = db_nt.NT_NhanVienVP.Where(x => x.CCCD.Contains(CCCD) && x.TinhTrang == 0).FirstOrDefault();
                                    var CheckNVNT = db_nt.NT_NhanVienNT.Where(x => x.CCCD.Contains(CCCD) && x.TTLV == 1 || x.CCCD == CCCD && x.TTLV == 1).FirstOrDefault();
                                    if (CheckNVNT != null)
                                    {
                                        if (CheckNVNT.TTLV == 1)
                                        {

                                            var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                            foreach (var item in ID_CT)
                                            {
                                                db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                            }
                                            return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Nhân viên chưa cắt thẻ Nhà thầu cũ. Nhân viên : {HoVaTen} " });
                                        }
                                    }
                                    if (CheckCCCD != null)
                                    {

                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                        return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Nhân viên nằm trong danh sách vi phạm. Nhân viên : {HoVaTen} " });

                                    }
                                }
                                string GiaHan = dt.Rows[i][11].ToString().Trim();
                                string BoSungCong = dt.Rows[i][12].ToString().Trim();
                                string CapLai = dt.Rows[i][13].ToString().Trim();
                                string ChuyenDoiNT = dt.Rows[i][14].ToString().Trim();

                                if (CapMoi == "" && GiaHan == "" && BoSungCong == "" && CapLai == "" && ChuyenDoiNT == "")
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"NVui lòng tích chọn loại đăng ký thẻ. Nhân viên : {HoVaTen} " });


                                }

                                if (LoaiTK == "1" && GiaHan != "" || LoaiTK == "1" && BoSungCong != "" || LoaiTK == "1" && CapLai != "" || LoaiTK == "1" && ChuyenDoiNT != "")
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : {HoVaTen} " });

                                }

                                if (LoaiTK == "2" && CapMoi != "" || LoaiTK == "2" && BoSungCong != "" || LoaiTK == "2" && CapLai != "" || LoaiTK == "2" && ChuyenDoiNT != "")
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : {HoVaTen} " });
                                }

                                if (LoaiTK == "3" && CapMoi != "" || LoaiTK == "3" && GiaHan != "" || LoaiTK == "3" && CapLai != "" || LoaiTK == "3" && ChuyenDoiNT != "")
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : {HoVaTen} " });
                                }
                                if (LoaiTK == "4" && CapMoi != "" || LoaiTK == "4" && GiaHan != "" || LoaiTK == "4" && BoSungCong != "" || LoaiTK == "4" && ChuyenDoiNT != "")
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : {HoVaTen} " });
                                }

                                if (LoaiTK == "5" && CapMoi != "" || LoaiTK == "5" && GiaHan != "" || LoaiTK == "5" && CapLai != "" || LoaiTK == "5" && BoSungCong != "")
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : {HoVaTen} " });
                                }



                                string THThe = dt.Rows[i][16].ToString().Trim();
                                try
                                {
                                    if (THThe == "")
                                    {

                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                        return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng kiểm tra thời hạn thẻ. Nhân viên : {HoVaTen} " });
                                    }
                                    DateTime ThoiHanThe1 = DateTime.ParseExact(THThe, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                }
                                catch
                                {

                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng kiểm tra định dạng ngày thời hạn thẻ. Nhân viên : {HoVaTen} " });
                                }
                                DateTime ThoiHanThe = DateTime.ParseExact(THThe, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                string KhuVuc = dt.Rows[i][17].ToString().Trim();
                                var NameKV = new List<string>();
                                string[] arrList = KhuVuc.Trim().Split(',');
                                foreach (var str in arrList)
                                {
                                    var IDKV = db.NT_Workplace.Where(x => x.TenKV == str.Trim()).FirstOrDefault();

                                    if (IDKV == null && id != 0)
                                    {

                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                        return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $"Vui lòng kiểm tra lại khu vực làm việc. Nhân viên : {HoVaTen} " });
                                    }
                                    else
                                    {
                                        NameKV.Add(IDKV.IDKV.ToString());
                                    }
                                }
                                var ListKV = string.Join(",", NameKV);

                                string NameGate = dt.Rows[i][18].ToString().Trim();
                                var Name = new List<string>();
                                string[] arrListStr = NameGate.Split(',');
                                foreach (var str in arrListStr)
                                {
                                    var IDGate = db.NT_Gate.Where(x => x.Gate == str.Trim()).FirstOrDefault();

                                    if (IDGate == null && id != 0)
                                    {

                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                        return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $" Vui lòng kiểm tra lại cổng ra vào. Nhân viên : {HoVaTen} " });

                                    }
                                    else
                                    {
                                        Name.Add(IDGate.IDGate.ToString());
                                    }
                                }
                                var ListGate = string.Join(",", Name);


                                string NameGroup = "";


                                string GhiChu = dt.Rows[i][19].ToString().Trim();

                                int DienThoaiDiDong = (dt.Rows[i][15].ToString() == "") ? 0 : 1;
                                var insert = db_dk.Detail_RegisterPeople_Insert
                                (id,
                                HoTen,
                                NgaySinh,
                                CCCD,
                                HoKhau,
                                (IDCV == null) ? 0 : IDCV.IDCV,
                                SoDienThoai,
                                Ten_NTP,
                                HoTen_QuanLy,
                                SoDienThoai_QuanLy,
                                CapMoi,
                                GiaHan,
                                BoSungCong,
                                CapLai,
                                ChuyenDoiNT,
                                ThoiHanThe,
                                ListKV,
                                ListGate,

                                null,
                                GhiChu,
                                DienThoaiDiDong,
                                "");
                                dtc++;
                            }



                        }
                        if (status == 0)
                        {
                            msg = (status == 0) ? "Có những dòng dữ liệu có thể không hợp lệ, vui long kiểm tra kỹ trước khi xác nhận" : "thêm thành công";
                        }
                        return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $" Thêm thành công " });
                    }
                    catch (Exception ex)
                    {

                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                        foreach (var item in ID_CT)
                        {
                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                        }
                        return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = $" Kiểm tra lại thông tin. Nhân viên : {HoVaTen} " });
                    }

                }
                else
                {
                    if (id != 0)
                    {
                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                        foreach (var item in ID_CT)
                        {
                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                        }
                       // db_dk.RegisterPeople_Delete(id);
                    }
                    return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = "File import không đúng định dạng. Vui lòng tải biểu mẫu file import" });
                }

            }
            else
            {
                if (id != 0)
                {
                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id).ToList();
                    foreach (var item in ID_CT)
                    {
                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                    }
                   // db_dk.RegisterPeople_Delete(id);
                }
                // TempData["msgSuccess"] = "<script>alert('Vui lòng nhập dữ liệu');</script>";
                return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = "Vui lòng nhập dữ liệu" });
            }
            return Json(new { success = "success", data = Url.Action("Edit", new { id = id }), message = msg });
        }
        [HttpPost]
        public ActionResult Create(List_Detail_RegisterPeopleValidation _DO, FormCollection collection)
        {
             int ID_DKTN = 0;
            string NoiDung = "";
            string HopDong = "";
            string LoaiNT = "";
            string NhaThauID = "";
            string BP = "";
            string LoaiTK = "";
            string HoVaTen = "";
            int dtc = 0,status=1;


            string path = Server.MapPath("~/PDFHocAT/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            //Use Namespace called :  System.IO  
            string FileName = _DO.FilePDF != null ? "PDF" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

            //To Get File Extension  
            string FileExtension = _DO.FilePDF != null ? Path.GetExtension(_DO.FilePDF.FileName) : "";


            ////Add Current Date To Attached File Name  
            if (_DO.FilePDF != null)
            {
                FileName = FileName.Trim() + FileExtension;
                _DO.FilePDF.SaveAs(path + FileName);
                _DO.FileUpload = "~/PDFHocAT/" + FileName;
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                return RedirectToAction("Index", "List_RegisterPeople_NT");

            }

            string paths = Server.MapPath("~/PDFHocAT/");
            if (!Directory.Exists(paths))
            {
                Directory.CreateDirectory(paths);
            }
            //Use Namespace called :  System.IO  
            string FileNames = _DO.FileANH != null ? "Folder" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

            //To Get File Extension  
            string FileExtensions = _DO.FileANH != null ? Path.GetExtension(_DO.FileANH.FileName) : "";


            ////Add Current Date To Attached File Name  
            if (_DO.FileANH != null)
            {
                FileNames = FileNames.Trim() + FileExtensions;
                _DO.FileANH.SaveAs(paths + FileNames);
                _DO.FileUploadImg = "~/PDFHocAT/" + FileNames;
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Folder Ảnh');</script>";
                return RedirectToAction("Index", "List_RegisterPeople_NT");

            }


            foreach (var key in collection.AllKeys)
            {
                if (key != "__RequestVerificationToken")
                {
                    NoiDung = collection["NoiDung"];
                    HopDong = collection["HopDong"];
                    LoaiNT = collection["NT"];
                    NhaThauID = collection["IDNT"];
                    BP = collection["IDPhongBan"];
                    LoaiTK = collection["IDLTK"];
                    if (NoiDung == null)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng điền nội dung trích yếu');</script>";
                        return RedirectToAction("Index", "List_RegisterPeople_NT");
                    }
                    if (LoaiNT == null)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn loại nhân viên nhà thầu');</script>";
                        return RedirectToAction("Index", "List_RegisterPeople_NT");
                    }
                    if (NhaThauID == "")
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Nhà thầu');</script>";
                        return RedirectToAction("Index", "List_RegisterPeople_NT");
                    }
                    if (BP == "")
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn BP quản lý');</script>";
                        return RedirectToAction("Index", "List_RegisterPeople_NT");
                    }
                    if(HopDong == "")
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng nhập số hợp đồng');</script>";
                        return RedirectToAction("Index", "List_RegisterPeople_NT");
                    }    
                    if (key == "Imoprt")
                    {

                    }
                }

            }
            System.Data.Entity.Core.Objects.ObjectParameter returnID_DKTN = new System.Data.Entity.Core.Objects.ObjectParameter("ID_DKTN", typeof(int));
            db_dk.RegisterPeople_Insert(NoiDung, Convert.ToInt32(BP), Models.MyAuthentication.ID, Convert.ToInt32(NhaThauID), HopDong, DateTime.Now, _DO.FileUpload, _DO.FileUploadImg, Convert.ToInt32(LoaiTK), 0, Convert.ToInt32(LoaiNT), returnID_DKTN);
            ID_DKTN = Convert.ToInt32(returnID_DKTN.Value);


            // insert danh sách nhân viên đăng ký thẻ người
            string filePath = string.Empty;
            HttpPostedFileBase file = Request.Files["FileExcel"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string pathex = Server.MapPath("~/UploadedFiles/DKHocATEX/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(pathex);
                }
                filePath = pathex + Path.GetFileName(file.FileName);

                file.SaveAs(filePath);
                Stream stream = file.InputStream;

                IExcelDataReader reader = null;
                if (file.FileName.ToLower().EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (file.FileName.ToLower().EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Vui lòng chọn đúng định dạng file Excel');</script>";
                    return View();
                }
                DataSet result = reader.AsDataSet();
                DataTable dt = result.Tables[0];
                reader.Close();
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        for (int i = 4; i < dt.Rows.Count; i++)
                        {
                            dtc++;
                            string HoTen = dt.Rows[i][1].ToString().Trim();
                            if (HoTen != "")
                            {
                                HoVaTen = HoTen;
                                string BirthDay = dt.Rows[i][2].ToString().Trim();

                                try
                                {
                                    if (BirthDay == "")
                                    {
                                        status = 0;
                                        if (ID_DKTN != 0)
                                        {
                                            var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                            foreach (var item in ID_CT)
                                            {
                                                db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                            }
                                           /* db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                        }
                                        TempData["msgSuccess"] = "<script>alert('Kiểm tra lại ngày sinh. Nhân viên : " + HoVaTen + "');</script>";

                                        return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                    }
                                    DateTime NgaySinh1 = DateTime.ParseExact(BirthDay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                }
                                catch
                                {
                                    if (ID_DKTN != 0)
                                    {
                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                       /* db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    }
                                    TempData["msgSuccess"] = "<script>alert('Kiểm tra lại định dạng ngày của Ngày sinh. Nhân viên : " + HoVaTen + "');</script>";

                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }
                                DateTime NgaySinh = DateTime.ParseExact(BirthDay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


                                string CCCD = dt.Rows[i][3].ToString().Trim();
                                string HoKhau = dt.Rows[i][4].ToString().Trim();

                                string ChucVu = dt.Rows[i][5].ToString().Trim();
                                var IDCV = db.NT_Position.Where(x => x.TenCV == ChucVu).FirstOrDefault();
                                if (IDCV == null)
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                  /*  db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại chức vụ. Nhân viên : " + HoVaTen + "');</script>";
                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }
                                string SoDienThoai = dt.Rows[i][6].ToString().Trim();
                                string Ten_NTP = dt.Rows[i][7].ToString().Trim();
                                string HoTen_QuanLy = dt.Rows[i][8].ToString().Trim();
                                string SoDienThoai_QuanLy = dt.Rows[i][9].ToString().Trim();
                                string CapMoi = dt.Rows[i][10].ToString().Trim();
                                if (CapMoi != "")
                                {
                                    var CheckCCCD = db_nt.NT_NhanVienVP.Where(x => x.CCCD.Contains(CCCD) && x.TinhTrang == 0).FirstOrDefault();
                                    var CheckNVNT = db_nt.NT_NhanVienNT.Where(x => x.CCCD.Contains(CCCD) && x.TTLV == 1 || x.CCCD == CCCD && x.TTLV == 1).FirstOrDefault();
                                    if (CheckNVNT != null)
                                    {
                                        if (CheckNVNT.TTLV == 1)
                                        {
                                            var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                            foreach (var item in ID_CT)
                                            {
                                                db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                            }
                                           /* db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                            status = 0;
                                            TempData["msgSuccess"] = "<script>alert('Nhân viên chưa cắt thẻ Nhà thầu cũ. Vui lòng kiểm tra Nhân viên : " + HoVaTen + "');</script>";
                                            return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                        }
                                    }
                                    if (CheckCCCD != null)
                                    {
                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                       /* db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                        status = 0;
                                        TempData["msgSuccess"] = "<script>alert('Nhân viên nằm trong danh sách vi phạm. Vui lòng kiểm tra Nhân viên : " + HoVaTen + "');</script>";
                                        return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                    }
                                }
                                string GiaHan = dt.Rows[i][11].ToString().Trim();
                                string BoSungCong = dt.Rows[i][12].ToString().Trim();
                                string CapLai = dt.Rows[i][13].ToString().Trim();
                                string ChuyenDoiNT = dt.Rows[i][14].ToString().Trim();

                                if(CapMoi == "" && GiaHan == "" && BoSungCong == "" && CapLai == "" && ChuyenDoiNT=="")
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                   /* db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert( Vui lòng tích chọn loại đăng ký thẻ. Nhân viên : " + HoVaTen + "');</script>";
                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });

                                }    

                                if(LoaiTK == "1" && GiaHan != "" || LoaiTK == "1" && BoSungCong != "" || LoaiTK == "1" && CapLai != "" || LoaiTK == "1" && ChuyenDoiNT != "")
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    /*db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : " + HoVaTen + "');</script>";

                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }

                                if (LoaiTK == "2" && CapMoi != "" || LoaiTK == "2" && BoSungCong != "" || LoaiTK == "2" && CapLai != "" || LoaiTK == "2" && ChuyenDoiNT != "")
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                   /* db_dk.RegisterPeople_Delete(ID_DKTN);*/

                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : " + HoVaTen + "');</script>";

                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }

                                if (LoaiTK == "3" && CapMoi != "" || LoaiTK == "3" && GiaHan != "" || LoaiTK == "3" && CapLai != "" || LoaiTK == "3" && ChuyenDoiNT != "")
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    /*db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : " + HoVaTen + "');</script>";

                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }
                                if (LoaiTK == "4" && CapMoi != "" || LoaiTK == "4" && GiaHan != "" || LoaiTK == "4" && BoSungCong != "" || LoaiTK == "4" && ChuyenDoiNT != "")
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    /*db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng tích chọn đúng loại đăng ký thẻ. Nhân viên : " + HoVaTen + "');</script>";

                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }

                                if (LoaiTK == "5" && CapMoi != "" || LoaiTK == "5" && GiaHan != "" || LoaiTK == "5" && CapLai != "" || LoaiTK == "5" && BoSungCong != "")
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                   /* db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng tích chọn đúng loại đăng ký thẻ.Nhân viên : " + HoVaTen + "');</script>";

                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }



                                string THThe = dt.Rows[i][16].ToString().Trim();
                                try
                                {
                                    if (THThe == "")
                                    {
                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                     /*   db_dk.RegisterPeople_Delete(ID_DKTN);*/

                                        status = 0;
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra thời hạn thẻ. Nhân viên : " + HoVaTen + "');</script>";
                                        return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                    }
                                    DateTime ThoiHanThe1 = DateTime.ParseExact(THThe, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                }
                                catch
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                  /*  db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                    status = 0;
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra định dạng ngày thời hạn thẻ. Nhân viên : " + HoVaTen + "');</script>";
                                    return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                }
                                DateTime ThoiHanThe = DateTime.ParseExact(THThe, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                string KhuVuc = dt.Rows[i][17].ToString().Trim();
                                var NameKV = new List<string>();
                                string[] arrList = KhuVuc.Trim().Split(',');
                                foreach (var str in arrList)
                                {
                                    var IDKV = db.NT_Workplace.Where(x => x.TenKV == str.Trim()).FirstOrDefault();

                                    if (IDKV == null && ID_DKTN != 0)
                                    {
                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                     /*   db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                        status = 0;
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại khu vực làm việc. Nhân viên : " + HoVaTen + "');</script>";

                                        return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                    }
                                    else
                                    {
                                        NameKV.Add(IDKV.IDKV.ToString());
                                    }
                                }
                                var ListKV = string.Join(",", NameKV);

                                string NameGate = dt.Rows[i][18].ToString().Trim();
                                var Name = new List<string>();
                                 string[] arrListStr = NameGate.Split(',');
                                foreach (var str in arrListStr)
                                {
                                    var IDGate = db.NT_Gate.Where(x => x.Gate == str.Trim()).FirstOrDefault();

                                    if (IDGate == null && ID_DKTN != 0)
                                    {
                                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                        foreach (var item in ID_CT)
                                        {
                                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                        }
                                      /*  db_dk.RegisterPeople_Delete(ID_DKTN);*/
                                        status = 0;
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại cổng ra vào.  Nhân viên : " + HoVaTen + "');</script>";
                                        return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                                    }
                                    else
                                    {
                                        Name.Add(IDGate.IDGate.ToString());
                                    }
                                }
                                var ListGate = string.Join(",", Name);


                                string NameGroup =/* dt.Rows[i][18].ToString().Trim()*/ "";
                               /* var IDGroup = db.NT_ContractorGroup.Where(x => x.NameContractorGroup == NameGroup).FirstOrDefault();
                                if (IDGroup == null)
                                {
                                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                                    foreach (var item in ID_CT)
                                    {
                                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                                    }
                                    db_dk.RegisterPeople_Delete(ID_DKTN);

                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại nhóm Nhà thầu.  Nhân viên : " + HoVaTen + "');</script>";
                                    return RedirectToAction("Index", "List_RegisterPeople_NT");
                                }*/

                                string GhiChu = dt.Rows[i][19].ToString().Trim();
                                //var a = dt.Rows[i][15].ToString();
                                int DienThoaiDiDong = (dt.Rows[i][15].ToString() == "") ? 0:1 ;
                                var insert = db_dk.Detail_RegisterPeople_Insert
                                (ID_DKTN,
                                HoTen,
                                NgaySinh,
                                CCCD,
                                HoKhau,
                                (IDCV==null)?0: IDCV.IDCV,
                                SoDienThoai,
                                Ten_NTP,
                                HoTen_QuanLy,
                                SoDienThoai_QuanLy,
                                CapMoi,
                                GiaHan,
                                BoSungCong,
                                CapLai,
                                ChuyenDoiNT,
                                ThoiHanThe,
                                ListKV,
                                ListGate,
                                /*Convert.ToInt32(IDGroup.IDGroup)*/
                                null,
                                GhiChu,
                                DienThoaiDiDong,
                                "");
                                dtc++;
                            }



                        }
                        if (status == 0)
                        {
                            TempData["msgSuccess"] = "<script>alert('Có những dòng dữ liệu có thể không hợp lệ, vui long kiểm tra kỹ trước khi xác nhận');</script>";
                        }
                        return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });

                    }
                    catch (Exception ex)
                    {
                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                        foreach (var item in ID_CT)
                        {
                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                        }
                        /*db_dk.RegisterPeople_Delete(ID_DKTN);*/
                        status = 0;
                        TempData["msgSuccess"] = "<script>alert('Kiểm tra lại thông tin.  Nhân viên : " + HoVaTen + "');</script>";
                        return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                    }

                }
                else
                {
                    if (ID_DKTN != 0)
                    {
                        var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                        foreach (var item in ID_CT)
                        {
                            db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                        }
                      /*  db_dk.RegisterPeople_Delete(ID_DKTN);*/
                    }
                    TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                }

            }
            else
            {
                if (ID_DKTN != 0)
                {
                    var ID_CT = db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == ID_DKTN).ToList();
                    foreach (var item in ID_CT)
                    {
                        db_dk.Detail_RegisterPeople_Delete(item.ID_CT_DKTN);
                    }
                  /*  db_dk.RegisterPeople_Delete(ID_DKTN);*/
                }
                TempData["msgSuccess"] = "<script>alert('Vui lòng nhập dữ liệu');</script>";
            }
            return RedirectToAction("Index", "List_RegisterPeople_NT");

        }
        public ActionResult Edit(int? id)
        {
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");

            List<ModelsView360.PhongBan> pb = db.PhongBans.Where(x => x.status == 1).ToList();
            ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            List<SignerType> tk = db_dk.SignerTypes.ToList();
            ViewBag.IDLTK = new SelectList(tk, "ID_LTK", "TenLoai");

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
            var res = (from a in db_dk.RegisterPeoples.Where(x => x.ID_DKTN == id)
                       select new List_RegisterPeopleValidation
                       {
                           ID_DKTN = (int)a.ID_DKTN,
                           NoiDung = a.NoiDung,
                           BPQL_ID = (int?)a.BPQL_ID ?? default,
                           NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default,
                           NhaThau_ID = (int)a.NhaThau_ID,
                           HopDong = a.HopDong,
                           NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                           File_CCAT = a.File_CCAT,
                           File_IMG = a.File_IMG,
                           TrinhKy_ID = (int?)a.TrinhKy_ID ?? default,
                           TinhTrang_ID = (int?)a.TinhTrang_ID ?? default,
                           LoaiNT_ID = (int?)a.LoaiNT_ID ?? default
                       }).ToList();
            List_RegisterPeopleValidation DO = new List_RegisterPeopleValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_DKTN = (int)a.ID_DKTN;
                    DO.NoiDung = a.NoiDung;
                    DO.BPQL_ID = (int?)a.BPQL_ID ?? default;
                    DO.NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default;
                    DO.NhaThau_ID = (int)a.NhaThau_ID;
                    DO.HopDong = a.HopDong;
                    DO.NgayTrinhKy = (DateTime)a.NgayTrinhKy;
                    DO.File_CCAT = a.File_CCAT;
                    DO.File_IMG = a.File_IMG;
                    DO.TrinhKy_ID = (int?)a.TrinhKy_ID ?? default;
                    DO.TinhTrang_ID = (int?)a.TinhTrang_ID ?? default;
                    DO.LoaiNT_ID = (int?)a.LoaiNT_ID ?? default;
                }
            }
            else
            {
                HttpNotFound();
            }
            return View(DO);

        }
        [HttpPost]
        public ActionResult Edit(List_Detail_RegisterPeopleValidation _DO, FormCollection collection)
        {
            int DKTN_ID = 0;    
            if (collection.Count > 2)
            {
                try
                {
                    var ListDS = new List<List_Detail_RegisterPeopleValidation>();
                    DateTime DayUpload = DateTime.Now;
                    string ID_DKTN = collection["XacNhan"];
                    DKTN_ID = Convert.ToInt32(ID_DKTN);
                    var DO = db_dk.RegisterPeoples.Where(x => x.ID_DKTN == DKTN_ID).FirstOrDefault();
                    foreach (var key in collection.AllKeys)
                    {
                        if (key != "__RequestVerificationToken" && key.Split('_')[0] == "GhiChu" && key != "XacNhan")
                        {
                            var GhiChu = collection["GhiChu_" + key.Split('_')[1]];
                            ListDS.Add(new List_Detail_RegisterPeopleValidation()
                            {
                                HoVaTen = collection["HoTen_" + key.Split('_')[1]],
                                NgaySinh = DateTime.ParseExact(collection["NgaySinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                CCCD = collection["CCCD_" + key.Split('_')[1]],
                                HoKhau = collection["HoKhau_" + key.Split('_')[1]],
                                CV_ID = Convert.ToInt32(collection["ChucVu_" + key.Split('_')[1]]),
                                SoDienThoai = collection["SoDienThoai_" + key.Split('_')[1]],
                                Ten_NTP = collection["Ten_NTP_" + key.Split('_')[1]],
                                HoTen_QuanLy = collection["HoTen_QuanLy_" + key.Split('_')[1]],
                                SoDienThoai_QuanLy = collection["SoDienThoai_QuanLy_" + key.Split('_')[1]],
                                CapMoi = collection["CapMoi_" + key.Split('_')[1]],
                                GiaHan = collection["GiaHan_" + key.Split('_')[1]],
                                BoSungCong = collection["BoSungCong_" + key.Split('_')[1]],
                                CapLai = collection["CapLai_" + key.Split('_')[1]],
                                ChuyenNT = collection["ChuyenNT_" + key.Split('_')[1]],
                                ThoiHanThe = DateTime.ParseExact(collection["ThoiHanThe_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                KhuVucLamViec = collection["KhuVucLamViec_" + key.Split('_')[1]],
                               // NhomNT = Convert.ToInt32(collection["NhomNT_" + key.Split('_')[1]]),
                                DienThoaiDiDong=(collection["DienThoaiDiDong_" + key.Split('_')[1]]==null)?0: Convert.ToInt32(collection["DienThoaiDiDong_" + key.Split('_')[1]]),
                                CongLamViec = collection["Cong_" + key.Split('_')[1]],
                                GhiChu = collection["GhiChu_" + key.Split('_')[1]]
                            });
                        }

                    }
                    foreach (var item in ListDS)
                    {
                        if (item.CapMoi != null)
                        {
                            var CheckVP = db_nt.NT_NhanVienVP.Where(x => x.CCCD == item.CCCD && x.TinhTrang == 0).FirstOrDefault();
                            var CheckNVNT = db_nt.NT_NhanVienNT.Where(x => x.CCCD.Contains(item.CCCD) || x.CCCD == item.CCCD && x.TTLV == 1).FirstOrDefault();
                            if (item.HoVaTen != "" && item.CCCD != "" && CheckVP != null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Nhân viên nằm trong danh sách vi phạm.  Nhân viên : " + item.HoVaTen + "');</script>";
                                return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });
                            }
                            if (item.HoVaTen != "" && item.CCCD != "" && CheckNVNT != null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Nhân viên chưa cắt thẻ nhà thầu cũ. Nhân viên : " + item.HoVaTen + "');</script>";
                                return RedirectToAction("Edit", "List_RegisterPeople_NT", new { id = ID_DKTN });

                            }




                        }
                        string LoaiCap = "X";
                        if (item.CapMoi != null)
                        {
                            var insert = db_dk.Detail_RegisterPeople_Insert
                                (DKTN_ID,
                                  item.HoVaTen,
                                   item.NgaySinh,
                                  item.CCCD,
                                  item.HoKhau,
                                  item.CV_ID,
                                  item.SoDienThoai,
                                  item.Ten_NTP,
                                  item.HoTen_QuanLy,
                                  item.SoDienThoai_QuanLy,
                                  LoaiCap,
                                  null,
                                  null,
                                  null,
                                  null,
                                  item.ThoiHanThe,
                                  item.KhuVucLamViec,
                                  item.CongLamViec,
                                  /* item.NhomNT,*/
                                  null,
                                 item.GhiChu,
                                 item.DienThoaiDiDong,
                                 "");
                        }
                        else if (item.GiaHan != null)
                        {
                            var insert = db_dk.Detail_RegisterPeople_Insert
                               (DKTN_ID,
                                 item.HoVaTen,
                                  item.NgaySinh,
                                 item.CCCD,
                                 item.HoKhau,
                                 item.CV_ID,
                                 item.SoDienThoai,
                                 item.Ten_NTP,
                                 item.HoTen_QuanLy,
                                 item.SoDienThoai_QuanLy,
                                 null,
                                 LoaiCap,
                                 null,
                                 null,
                                 null,
                                 item.ThoiHanThe,
                                 item.KhuVucLamViec,
                                 item.CongLamViec,
                                 /*item.NhomNT,*/
                                 null,
                                 item.GhiChu,
                                 item.DienThoaiDiDong,
                                 "");
                        }
                        else if (item.BoSungCong != null)
                        {
                            var insert = db_dk.Detail_RegisterPeople_Insert
                               (DKTN_ID,
                                 item.HoVaTen,
                                  item.NgaySinh,
                                 item.CCCD,
                                 item.HoKhau,
                                 item.CV_ID,
                                 item.SoDienThoai,
                                 item.Ten_NTP,
                                 item.HoTen_QuanLy,
                                 item.SoDienThoai_QuanLy,
                                 null,
                                 null,
                                 LoaiCap,
                                 null,
                                 null,
                                 item.ThoiHanThe,
                                 item.KhuVucLamViec,
                                 item.CongLamViec,
                                 /* item.NhomNT,*/
                                 null,
                                    item.GhiChu,
                                 item.DienThoaiDiDong,
                                 "");
                        }
                        else if (item.CapLai != null)
                        {
                            var insert = db_dk.Detail_RegisterPeople_Insert
                               (DKTN_ID,
                                 item.HoVaTen,
                                  item.NgaySinh,
                                 item.CCCD,
                                 item.HoKhau,
                                 item.CV_ID,
                                 item.SoDienThoai,
                                 item.Ten_NTP,
                                 item.HoTen_QuanLy,
                                 item.SoDienThoai_QuanLy,
                                 null,
                                 null,
                                 null,
                                 LoaiCap,
                                 null,
                                 item.ThoiHanThe,
                                 item.KhuVucLamViec,
                                 item.CongLamViec,
                                 /* item.NhomNT,*/
                                 null,
                                  item.GhiChu,
                                 item.DienThoaiDiDong,
                                 "");
                        }
                        else if (item.ChuyenNT != null)
                        {
                            var insert = db_dk.Detail_RegisterPeople_Insert
                               (DKTN_ID,
                                 item.HoVaTen,
                                  item.NgaySinh,
                                 item.CCCD,
                                 item.HoKhau,
                                 item.CV_ID,
                                 item.SoDienThoai,
                                 item.Ten_NTP,
                                 item.HoTen_QuanLy,
                                 item.SoDienThoai_QuanLy,
                                 null,
                                 null,
                                 null,
                                 null,
                                 LoaiCap,
                                 item.ThoiHanThe,
                                 item.KhuVucLamViec,
                                 item.CongLamViec,
                                  /*item.NhomNT,*/
                                  null,
                                  item.GhiChu,
                                 item.DienThoaiDiDong,
                                 "");
                        }

                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công dòng');</script>";

                    }
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Vui lòng nhập đầy đủ thông tin');</script>";
                }
            }
            return RedirectToAction("Index", "List_RegisterPeople_NT");
        }

        public ActionResult Update(int? id)
        {

            var res = (from a in db_dk.RegisterPeoples.Where(x => x.ID_DKTN == id)
                       select new List_RegisterPeopleValidation
                       {
                           ID_DKTN = (int)a.ID_DKTN,
                           NoiDung = a.NoiDung,
                           BPQL_ID = (int?)a.BPQL_ID ?? default,
                           NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default,
                           NhaThau_ID = (int)a.NhaThau_ID,
                           HopDong = a.HopDong,
                           NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                           File_CCAT = a.File_CCAT,
                           File_IMG = a.File_IMG,
                           TrinhKy_ID = (int?)a.TrinhKy_ID ?? default,
                           TinhTrang_ID = (int?)a.TinhTrang_ID ?? default,
                           LoaiNT_ID = (int?)a.LoaiNT_ID ?? default
                       }).ToList();
            List_RegisterPeopleValidation DO = new List_RegisterPeopleValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_DKTN = (int)a.ID_DKTN;
                    DO.NoiDung = a.NoiDung;
                    DO.BPQL_ID = (int?)a.BPQL_ID ?? default;
                    DO.NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default;
                    DO.NhaThau_ID = (int)a.NhaThau_ID;
                    DO.HopDong = a.HopDong;
                    DO.NgayTrinhKy = (DateTime)a.NgayTrinhKy;
                    DO.File_CCAT = a.File_CCAT;
                    DO.File_IMG = a.File_IMG;
                    DO.TrinhKy_ID = (int?)a.TrinhKy_ID ?? default;
                    DO.TinhTrang_ID = (int?)a.TinhTrang_ID ?? default;
                    DO.LoaiNT_ID = (int?)a.LoaiNT_ID ?? default;
                }
                ViewBag.NgayTrinhKy = DO.NgayTrinhKy.ToString("yyyy-MM-dd");


                List<NT_Partner> nt = db.NT_Partner.ToList();
                ViewBag.ID = new SelectList(nt, "ID", "FullName", DO.NhaThau_ID);

                List<SignerType> kv = db_dk.SignerTypes.ToList();
                ViewBag.ID_LTK = new SelectList(kv, "ID_LTK", "TenLoai", DO.TrinhKy_ID);

                List<ModelsView360.PhongBan> pb = db.PhongBans.Where(x=>x.status==1).ToList();
                ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.BPQL_ID);
            }


            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Update(List_RegisterPeopleValidation _DO)
        {
            try
            {
                var Check_ = db_dk.RegisterPeoples.Where(x => x.ID_DKTN == _DO.ID_DKTN).FirstOrDefault();

                if (Check_.File_CCAT != _DO.File_CCAT && Check_.File_IMG == _DO.File_IMG)
                {
                    // Cập nhật File PDF
                    string path = Server.MapPath("~/PDFHocAT/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Use Namespace called :  System.IO  
                    string FileName = _DO.FilePDF != null ? "PDF" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                    //To Get File Extension  
                    string FileExtension = _DO.FilePDF != null ? Path.GetExtension(_DO.FilePDF.FileName) : "";


                    ////Add Current Date To Attached File Name  
                    if (_DO.FilePDF != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.FilePDF.SaveAs(path + FileName);
                        _DO.FileUpload = "~/PDFHocAT/" + FileName;
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                        return RedirectToAction("Index", "ListCardRegisInforNT");

                    }

                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, _DO.NgayTrinhKy, _DO.FileUpload, _DO.File_IMG, _DO.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }
                else if (Check_.File_CCAT == _DO.File_CCAT && Check_.File_IMG != _DO.File_IMG)
                {
                    // Cập nhật Folder ảnh
                    string paths = Server.MapPath("~/PDFHocAT/");
                    if (!Directory.Exists(paths))
                    {
                        Directory.CreateDirectory(paths);
                    }
                    //Use Namespace called :  System.IO  
                    string FileNames = _DO.FileANH != null ? "Folder" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                    //To Get File Extension  
                    string FileExtensions = _DO.FileANH != null ? Path.GetExtension(_DO.FileANH.FileName) : "";


                    ////Add Current Date To Attached File Name  
                    if (_DO.FileANH != null)
                    {
                        FileNames = FileNames.Trim() + FileExtensions;
                        _DO.FileANH.SaveAs(paths + FileNames);
                        _DO.FileUploadImg = "~/PDFHocAT/" + FileNames;
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Folder Ảnh');</script>";
                        return RedirectToAction("Index", "ListCardRegisInforNT");

                    }
                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, _DO.NgayTrinhKy, _DO.File_CCAT, _DO.FileUploadImg, _DO.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }
                else if (Check_.File_CCAT != _DO.File_CCAT && Check_.File_IMG != _DO.File_IMG)
                {
                    // Cập nhật File PDF
                    string path = Server.MapPath("~/PDFHocAT/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Use Namespace called :  System.IO  
                    string FileName = _DO.FilePDF != null ? "PDF" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                    //To Get File Extension  
                    string FileExtension = _DO.FilePDF != null ? Path.GetExtension(_DO.FilePDF.FileName) : "";


                    ////Add Current Date To Attached File Name  
                    if (_DO.FilePDF != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.FilePDF.SaveAs(path + FileName);
                        _DO.FileUpload = "~/PDFHocAT/" + FileName;
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                        return RedirectToAction("Index", "ListCardRegisInforNT");

                    }

                    // Cập nhật Folder ảnh
                    string paths = Server.MapPath("~/PDFHocAT/");
                    if (!Directory.Exists(paths))
                    {
                        Directory.CreateDirectory(paths);
                    }
                    //Use Namespace called :  System.IO  
                    string FileNames = _DO.FileANH != null ? "Folder" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                    //To Get File Extension  
                    string FileExtensions = _DO.FileANH != null ? Path.GetExtension(_DO.FileANH.FileName) : "";


                    ////Add Current Date To Attached File Name  
                    if (_DO.FileANH != null)
                    {
                        FileNames = FileNames.Trim() + FileExtensions;
                        _DO.FileANH.SaveAs(paths + FileNames);
                        _DO.FileUploadImg = "~/PDFHocAT/" + FileNames;
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Folder Ảnh');</script>";
                        return RedirectToAction("Index", "ListCardRegisInforNT");

                    }
                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, _DO.NgayTrinhKy, _DO.FileUpload, _DO.FileUploadImg, _DO.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }
                else if (Check_.File_CCAT == _DO.File_CCAT && Check_.File_IMG == _DO.File_IMG)
                {
                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, _DO.NgayTrinhKy, _DO.File_CCAT, _DO.File_IMG, _DO.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }


            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Vui lòng nhập đầy đủ thông tin');</script>";
            }
            return RedirectToAction("Edit", "List_RegisterPeople_NT");
        }

        public ActionResult Update_Test(int? id)
        {

            var res = (from a in db_dk.RegisterPeoples.Where(x => x.ID_DKTN == id)
                       select new List_RegisterPeopleValidation
                       {
                           ID_DKTN = (int)a.ID_DKTN,
                           NoiDung = a.NoiDung,
                           BPQL_ID = (int?)a.BPQL_ID ?? default,
                           NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default,
                           NhaThau_ID = (int)a.NhaThau_ID,
                           HopDong = a.HopDong,
                           NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                           File_CCAT = a.File_CCAT,
                           File_IMG = a.File_IMG,
                           TrinhKy_ID = (int?)a.TrinhKy_ID ?? default,
                           TinhTrang_ID = (int?)a.TinhTrang_ID ?? default,
                           LoaiNT_ID = (int?)a.LoaiNT_ID ?? default
                       }).ToList();
            List_RegisterPeopleValidation DO = new List_RegisterPeopleValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_DKTN = (int)a.ID_DKTN;
                    DO.NoiDung = a.NoiDung;
                    DO.BPQL_ID = (int?)a.BPQL_ID ?? default;
                    DO.NhanVienNT_ID = (int?)a.NhanVienNT_ID ?? default;
                    DO.NhaThau_ID = (int)a.NhaThau_ID;
                    DO.HopDong = a.HopDong;
                    DO.NgayTrinhKy = (DateTime)a.NgayTrinhKy;
                    DO.File_CCAT = a.File_CCAT;
                    DO.File_IMG = a.File_IMG;
                    DO.TrinhKy_ID = (int?)a.TrinhKy_ID ?? default;
                    DO.TinhTrang_ID = (int?)a.TinhTrang_ID ?? default;
                    DO.LoaiNT_ID = (int?)a.LoaiNT_ID ?? default;
                }
                ViewBag.NgayTrinhKy = DO.NgayTrinhKy.ToString("yyyy-MM-dd");


                List<NT_Partner> nt = db.NT_Partner.ToList();
                ViewBag.ID = new SelectList(nt, "ID", "FullName", DO.NhaThau_ID);

                List<SignerType> kv = db_dk.SignerTypes.ToList();
                ViewBag.ID_LTK = new SelectList(kv, "ID_LTK", "TenLoai", DO.TrinhKy_ID);

                List<ModelsView360.PhongBan> pb = db.PhongBans.Where(x => x.status == 1).ToList();
                ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.BPQL_ID);
            }


            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Update_Test(List_RegisterPeopleValidation _DO)
        {
            try
            {
                var Check_ = db_dk.RegisterPeoples.Where(x => x.ID_DKTN == _DO.ID_DKTN).FirstOrDefault();

                // Cập nhật File PDF
                string path = Server.MapPath("~/PDFHocAT/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.FilePDF != null ? "PDF" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                //To Get File Extension  
                string FileExtension = _DO.FilePDF != null ? Path.GetExtension(_DO.FilePDF.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.FilePDF != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.FilePDF.SaveAs(path + FileName);
                    _DO.FileUpload = "~/PDFHocAT/" + FileName;
                }

                // Cập nhật Folder ảnh
                string paths = Server.MapPath("~/PDFHocAT/");
                if (!Directory.Exists(paths))
                {
                    Directory.CreateDirectory(paths);
                }
                //Use Namespace called :  System.IO  
                string FileNames = _DO.FileANH != null ? "Folder" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                //To Get File Extension  
                string FileExtensions = _DO.FileANH != null ? Path.GetExtension(_DO.FileANH.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.FileANH != null)
                {
                    FileNames = FileNames.Trim() + FileExtensions;
                    _DO.FileANH.SaveAs(paths + FileNames);
                    _DO.FileUploadImg = "~/PDFHocAT/" + FileNames;
                }
                if (_DO.FilePDF != null && _DO.FileANH != null)
                {

                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, Check_.NgayTrinhKy, _DO.FileUpload, _DO.FileUploadImg, Check_.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }
                else if (_DO.FilePDF != null && _DO.FileANH == null)
                {
                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, Check_.NgayTrinhKy, _DO.FileUpload, Check_.File_IMG, Check_.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }
                else if (_DO.FilePDF == null && _DO.FileANH != null)
                {
                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, Check_.NgayTrinhKy, Check_.File_CCAT, _DO.FileUploadImg, Check_.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }
                else if (_DO.FilePDF == null && _DO.FileANH == null)
                {
                    db_dk.RegisterPeople_Update(_DO.ID_DKTN, _DO.NoiDung, _DO.BPQL_ID, _DO.NhaThau_ID, _DO.HopDong, Check_.NgayTrinhKy, Check_.File_CCAT, Check_.File_IMG, Check_.TrinhKy_ID, _DO.LoaiNT_ID);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Vui lòng nhập đầy đủ thông tin');</script>";
            }
            return RedirectToAction("Index", "List_RegisterPeople_NT");
        }

        public ActionResult CheckInformation(int? id)
        {

            var ID = db_dk.RegisterPeoples.Where(x => x.ID_DKTN == id).FirstOrDefault();

            var KTV_QL = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 2 && x.NhanVien.IDPhongBan == ID.BPQL_ID || x.IDLKD == 2 && x.IDCVKN.Contains(ID.BPQL_ID.ToString()))
                          join a in db.NhanViens on au.IDNhanVien equals a.ID
                          select new CheckInforUser
                          {
                              IDNhanVien = (int)au.IDNhanVien,
                              HoTen = a.HoTen + " : " + a.MaNV,
                          }).ToList();
            ViewBag.KTV_QL_List = new SelectList(KTV_QL, "IDNhanVien", "HoTen");

            var TP_QL = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == ID.BPQL_ID || x.IDLKD == 1 && x.IDCVKN.Contains(ID.BPQL_ID.ToString()))
                         join a in db.NhanViens on au.IDNhanVien equals a.ID
                         select new CheckInforUser
                         {
                             IDNhanVien = (int)au.IDNhanVien,
                             HoTen = a.HoTen + " : " + a.MaNV,
                         }).ToList();
            ViewBag.TP_QL_List = new SelectList(TP_QL, "IDNhanVien", "HoTen");


            var TP_HCDN = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == 7 || x.IDLKD == 1 && x.NhanVien.IDPhongBan == 55)
                           join a in db.NhanViens on au.IDNhanVien equals a.ID
                           select new CheckInforUser
                           {
                               IDNhanVien = (int)au.IDNhanVien,
                               HoTen = a.HoTen + " : " + a.MaNV,
                           }).ToList();
            ViewBag.TP_HCDN = new SelectList(TP_HCDN, "IDNhanVien", "HoTen");


            var KTV_LQ_CD = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 2 && x.NhanVien.IDPhongBan == 31)
                             join a in db.NhanViens on au.IDNhanVien equals a.ID
                             select new CheckInforUser
                             {
                                 IDNhanVien = (int)au.IDNhanVien,
                                 HoTen = a.HoTen + " : " + a.MaNV,
                             }).ToList();
            ViewBag.KTV_LQ_CD_List = new SelectList(KTV_LQ_CD, "IDNhanVien", "HoTen");

            var TP_QL_CD = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == 31)
                            join a in db.NhanViens on au.IDNhanVien equals a.ID
                            select new CheckInforUser
                            {
                                IDNhanVien = (int)au.IDNhanVien,
                                HoTen = a.HoTen + " : " + a.MaNV,
                            }).ToList();
            ViewBag.TP_QL_CD_List = new SelectList(TP_QL_CD, "IDNhanVien", "HoTen");

            var KTV_LQ_CTH = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 2 && x.NhanVien.IDPhongBan == 51)
                              join a in db.NhanViens on au.IDNhanVien equals a.ID
                              select new CheckInforUser
                              {
                                  IDNhanVien = (int)au.IDNhanVien,
                                  HoTen = a.HoTen + " : " + a.MaNV,
                              }).ToList();
            ViewBag.KTV_LQ_CTH_List = new SelectList(KTV_LQ_CTH, "IDNhanVien", "HoTen");

            var TP_QL_CTH = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == 51)
                             join a in db.NhanViens on au.IDNhanVien equals a.ID
                             select new CheckInforUser
                             {
                                 IDNhanVien = (int)au.IDNhanVien,
                                 HoTen = a.HoTen + " : " + a.MaNV,
                             }).ToList();
            ViewBag.TP_QL_CTH_List = new SelectList(TP_QL_CTH, "IDNhanVien", "HoTen");

            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 3)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();
            ViewBag.VP1C_List = new SelectList(VP1C, "IDNhanVien", "HoTen");


            return PartialView();
        }
        [HttpPost]
        public ActionResult CheckInformation(List_RegisterPeopleValidation _DO, FormCollection collection, int? id)
        {
            try
            {
                string Date = DateTime.Now.ToString("yyyyMMddHHmmss");
                string format = "yyyyMMddHHmmss";
                DateTime? DateNow = DateTime.ParseExact(Date, format, CultureInfo.InvariantCulture);
                List<int> List_NhanVien = new List<int>();
                List<List_RegisterPeopleValidation> List = new List<List_RegisterPeopleValidation>();
                foreach (var key in collection.AllKeys)
                {
                    if (key != "__RequestVerificationToken")
                    {
                        if (key == "CBNV1" && collection["CBNV1"] != "")
                        {
                            string ID_NV1 = collection["CBNV1"];
                            List_NhanVien.Add(Convert.ToInt32(ID_NV1));
                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 1

                            });
                        }
                        if (key == "CBNV2" && collection["CBNV2"] != "")
                        {
                            string ID_NV1 = collection["CBNV2"];
                            List_NhanVien.Add(Convert.ToInt32(ID_NV1));
                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 2

                            });
                        }
                        if (key == "CBNV3" && collection["CBNV3"] != "")
                        {
                            string ID_NV1 = collection["CBNV3"];
                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 3

                            });
                        }
                        if (key == "CBNV4" && collection["CBNV4"] != "")
                        {
                            string ID_NV1 = collection["CBNV4"];
                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 4

                            });
                        }
                        if (key == "CBNV5" && collection["CBNV5"] != "")
                        {
                            string ID_NV1 = collection["CBNV5"];
                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 5

                            });
                        }
                        if (key == "CBNV6" && collection["CBNV6"] != "")
                        {
                            string ID_NV1 = collection["CBNV6"];
                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 6

                            });
                        }
                        if (key == "CBNV7" && collection["CBNV7"] != "")
                        {
                            string ID_NV1 = collection["CBNV7"];
                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 7

                            });
                        }
                        if (key == "CBNV8" && collection["CBNV8"] != "")
                        {
                            string ID_NV1 = collection["CBNV8"];

                            List.Add(new List_RegisterPeopleValidation()
                            {
                                ID_NV = Convert.ToInt32(ID_NV1),
                                LuongXuLY = 8

                            });
                        }
                    }
                }
                int CapDuyet = 1;
                foreach (var item in List)
                {
                    db_dk.SignOff_Flow_Insert(id, CapDuyet, item.LuongXuLY, item.ID_NV, null, 0, null);
                    CapDuyet++;

                }
                db_dk.RegisterPeople_UpdateFlow(id, 1);

                TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
            }

            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Trình ký thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "List_RegisterPeople_NT");
        }
        public ActionResult Cancel(int? id)
        {
            try
            {
                db_dk.SignOff_Flow_Delete(id);
                db_dk.RegisterPeople_UpdateFlow(id, 0);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "List_RegisterPeople_NT");
        }

        public ActionResult PrintTest_View(int? id, int? page)
        {
            var res = (from a in db_dk.Detail_RegisterPeople.Where(x => x.DKTN_ID == id)
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
                          // NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           DienThoaiDiDong=a.DienThoaiThongMinh??0,
                           DKTN_ID = (int)a.DKTN_ID,
                           Price = a.Price
                       }).ToList();
            if (id != null)
            {
                ViewData["ID_DKTN"] = id;
            }
            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.ID_CT_DKTN).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult PrintTest_Print(int? id, int? page)
        {
            var res = (from a in db_dk.Detail_RegisterPeople.
                       Where(x => x.DKTN_ID == id && x.Price == null || x.DKTN_ID == id && x.Price == "")
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
                          // NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           DKTN_ID = (int)a.DKTN_ID,
                           DienThoaiDiDong=a.DienThoaiThongMinh??0,
                           Price = a.Price
                       }).ToList();
            if (id != null)
            {
                ViewData["ID_DKTN"] = id;
            }
            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.ID_CT_DKTN).ToList().ToPagedList(pageNumber, pageSize));
        }
        public void tempFile_Pdf(int? id, String path)
        {
            string html = Server.MapPath("~/Views/Shared/footer.html");
            //string html1 = Server.MapPath("~/Views/Shared/header.html");
            string footer = string.Format(
                        "--footer-html \"{0}\" " +
                        "--footer-spacing \"5\" " +
                        "--footer-font-size \"5\" " +
                        "--footer-line --encoding utf-8", html);
            //string footer = "--footer-right \"Page: [page] of [toPage]\" " + "--footer-center \"Tài liệu này thuộc sở hữu của Hòa Phát Dung Quất. Việc phát tán, sử dụng trái phép bị nghiêm cấm\" --footer-line --footer-font-size \"9\" --footer-spacing 5 --footer-font-name \"Arial Unicode MS\" --encoding \"utf-8\"";
            var actionPDF = new ActionAsPdf("PrintTest_Print", new { id = id })
            {
                PageSize = Size.A4,
                PageMargins = new Margins(13, 5, 10, 5),
                PageWidth = 500,
                PageHeight = 300,
                CustomSwitches = footer,
                MinimumFontSize = 20,
            };
            var applicationPDFData = actionPDF.BuildPdf(this.ControllerContext);
            System.IO.File.WriteAllBytes(path, applicationPDFData);

        }
        public ActionResult ExportTo_Pdfsss(int? id)
        {
            var root = Server.MapPath("~/UploadedFiles/PDFDangKyThe/");
            var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            string destinationpath = string.Empty;

            tempFile_Pdf(id, path);
            var IDDK = db_dk.RegisterPeoples.Where(x => x.ID_DKTN == id).FirstOrDefault();
            var IDNT = db.NT_Partner.Where(x => x.ID == IDDK.NhaThau_ID).FirstOrDefault();
            destinationpath = IDNT.FullName + ".pdf";
            string originalFile = path;

            PdfReader reader = new PdfReader(originalFile);

            Font font = new Font(Font.FontFamily.HELVETICA, 16, Font.NORMAL, BaseColor.LIGHT_GRAY);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var pdfStamper = new PdfStamper(reader, memoryStream, '\0'))
                {
                    int pageCount = reader.NumberOfPages;
                    PdfLayer layer = new PdfLayer("WatermarkLayer", pdfStamper.Writer);
                    string layerwarkmarktxt = "";// define text for 
                    for (int i = 1; i <= pageCount; i++)
                    {
                        Rectangle rect = reader.GetPageSize(i);
                        PdfContentByte cb = pdfStamper.GetUnderContent(i);
                        cb.BeginLayer(layer);
                        PdfGState gState = new PdfGState();
                        gState.FillOpacity = 0.1f; // define opacity level
                        cb.SetGState(gState);
                        cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 10);
                        List<string> watermarkList = new List<string>();
                        float singleWaterMarkWidth = cb.GetEffectiveStringWidth(layerwarkmarktxt, false);
                        float fontHeight = 10;
                        float currentWaterMarkWidth = 0;
                        while (currentWaterMarkWidth + singleWaterMarkWidth < rect.Width)
                        {
                            watermarkList.Add(layerwarkmarktxt);
                            currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        }
                        float currentYPos = rect.Height;
                        while (currentYPos > 0)
                        {
                            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), 200, 200, 30);
                            currentYPos -= fontHeight;
                        }
                        cb.EndLayer();
                    }
                }
                reader.Close();
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return File(memoryStream.ToArray(), "application/pdf", destinationpath);
            }

        }
    }
}