using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace EPORTAL.Areas.Management.Controllers
{
    public class VehicleManagementController : Controller
    {
        EPORTAL_NTEntities db = new EPORTAL_NTEntities();
        EPORTALEntities db1 = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "VehicleManagement";
        // GET: VehicleManagement
        public ActionResult Index(string search, int? page, int? IDPhongBan, int? ID_NT)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            if (search == null) search = "";
            ViewBag.search = search;

            List<PhongBan> pb = db1.PhongBans.ToList();
            if (IDPhongBan == null) IDPhongBan = 0;
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan", IDPhongBan);

            List<NT_Partner> n = db1.NT_Partner.ToList();
            if (ID_NT == null) ID_NT = 0;
            ViewBag.NTList = new SelectList(n, "ID", "FullName", ID_NT);

            var res = (from ce in db.NT_ContactEquipment_Select(search) 
                       join  mdd in db.NT_MaDinhDanh on ce.IDMDD equals mdd.IDDD into ps_jointable
                       from mdd in ps_jointable.DefaultIfEmpty()
                       join nt in db1.NT_Partner on ce.IDContact equals nt.ID
                       join bp in db1.PhongBans on ce.IDDepartment equals bp.IDPhongBan 
                       select new VehicleManagementValidation
                       {                           
                           IDTBNT = (int)ce.IDTBNT,
                           NameEquipment = (ce.NameEquipment),
                           IndentifierContact = ce.IndentifierContact,
                           BienSoXe = ce.BienSoXe ??default,
                           IDContact = (int?)nt.ID ??default,
                           TenNhaThau = nt.FullName,
                           Operator = ce.Operator,
                           IDDepartment = (int)bp.IDPhongBan,
                           TenPhongBan = bp.TenPhongBan,
                           Cavet = (int)ce.Cavet,
                           NgayHetHanDK = (DateTime?)ce.NgayHetHanDK ??default,
                           NgayHetHanKD = (DateTime?)ce.NgayHetHanKD ??default,
                           NgayHetHanBHX = (DateTime?)ce.NgayHetHanBHX ??default,
                           CCHV = (int)ce.CCHV,
                           NgayHetHanTAT = (DateTime?)ce.NgayHetHanTAT ??default,
                           NgayHetHanVCHHChayNo = (DateTime?)ce.NgayHetHanVCHHChayNo ?? default,
                           NgayHetHanCNKD = (DateTime?)ce.NgayHetHanCNKD ?? default,
                           ChungChiPCCCCNCH = ce.ChungChiPCCCCNCH ??default,
                           ChungChiVCHHNHCN = ce.ChungChiVCHHNHCN ?? default,
                           StartDate = (DateTime)ce.StartDate,
                           IDKV = ce.KhuVucThiCong,
                           NguoiNhap = ce.NguoiNhap,
                           TinhTrang = (int)ce.TinhTrang,
                           GhiChu = ce.GhiChu,
                           FileUpload = ce.FileUpload,
                           IDMDD = (int?)mdd.IDDD ??default,
                           MaDinhDanh = mdd.MaDinhDanh ,
                           SoKhung = ce.SoKhung ??default,
                           ThoiHanTheTu = (DateTime?)ce.ThoiHanTheTu ?? default,
                           ThoiHanTheRVTX = (DateTime?)ce.ThoiHanTheRVTX ?? default,
                           CapMoi = ce.CapMoi,
                           GiaHan = ce.GiaHan,
                           MatThe = ce.MatThe,
                           CapNhatThongTin = ce.CapNhatThongTin,
                           BaoLanh = ce.BaoLanh,
                           NgayChinhSua = (DateTime?)ce.NgayChinhSua ?? default,
                           FileTheTu = ce.FileTheTu

                       }).ToList();
            if(IDPhongBan != 0)
            {
                res= res.Where(x=>x.IDDepartment == IDPhongBan).ToList();
            }    
            if(ID_NT != 0)
            {
                res = res.Where(x => x.IDContact == ID_NT).ToList();
            }

            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Delete(int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            try
            {
                db.NT_ContactEquipment_delete(id);
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "VehicleManagement");
        }
        public ActionResult Edit(int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var res = (from ce in db.NT_ContactEquipment.Where(x => x.IDTBNT == id)
                       select new VehicleManagementValidation
                       {
                           IDTBNT = (int)ce.IDTBNT,
                           NameEquipment = (ce.NameEquipment),
                           IndentifierContact = ce.IndentifierContact,
                           BienSoXe = ce.BienSoXe,
                           IDContact = (int)ce.IDContact,
                           Operator = ce.Operator,
                           IDDepartment = (int)ce.IDDepartment,
                           Cavet = (int)ce.Cavet,
                           NgayHetHanDK = (DateTime)ce.NgayHetHanDK,
                           NgayHetHanKD = (DateTime)ce.NgayHetHanKD,
                           NgayHetHanBHX = (DateTime)ce.NgayHetHanBHX,
                           CCHV = (int)ce.CCHV,
                           NgayHetHanTAT = (DateTime)ce.NgayHetHanTAT,
                           NgayHetHanVCHHChayNo = (DateTime)ce.NgayHetHanVCHHChayNo,
                           NgayHetHanCNKD = (DateTime)ce.NgayHetHanCNKD,
                           ChungChiPCCCCNCH = ce.ChungChiPCCCCNCH,
                           ChungChiVCHHNHCN = ce.ChungChiVCHHNHCN,
                           StartDate = (DateTime)ce.StartDate,
                           IDKV = ce.KhuVucThiCong,
                           NguoiNhap = ce.NguoiNhap,
                           TinhTrang = (int)ce.TinhTrang,
                           GhiChu = ce.GhiChu,
                           FileUpload = ce.FileUpload,
                           IDMDD = (int)ce.IDMDD,
                           SoKhung = ce.SoKhung,
                           ThoiHanTheTu = (DateTime)ce.ThoiHanTheTu,
                           ThoiHanTheRVTX = (DateTime)ce.ThoiHanTheRVTX, 
                           CapMoi = ce.CapMoi,
                           GiaHan = ce.GiaHan,
                           MatThe =ce.MatThe,
                           CapNhatThongTin =ce.CapNhatThongTin,
                           BaoLanh = ce.BaoLanh
                       }).ToList();
            VehicleManagementValidation DO = new VehicleManagementValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDTBNT = (int)a.IDTBNT;
                    DO.NameEquipment = (a.NameEquipment);
                    DO.IndentifierContact = a.IndentifierContact;
                    DO.BienSoXe = a.BienSoXe;
                    DO.IDContact = (int)a.IDContact;
                    DO.Operator = a.Operator;
                    DO.IDDepartment = (int)a.IDDepartment;
                    DO.Cavet = (int)a.Cavet;
                    DO.NgayHetHanDK = (DateTime)a.NgayHetHanDK;
                    DO.NgayHetHanKD = (DateTime)a.NgayHetHanKD;
                    DO.NgayHetHanBHX = (DateTime)a.NgayHetHanBHX;
                    DO.CCHV = (int)a.CCHV;
                    DO.NgayHetHanTAT = (DateTime)a.NgayHetHanTAT;
                    DO.NgayHetHanVCHHChayNo = (DateTime)a.NgayHetHanVCHHChayNo;
                    DO.NgayHetHanCNKD = (DateTime)a.NgayHetHanCNKD;
                    DO.ChungChiPCCCCNCH = a.ChungChiPCCCCNCH;
                    DO.ChungChiVCHHNHCN = a.ChungChiVCHHNHCN;
                    DO.StartDate = (DateTime)a.StartDate;
                    DO.IDKV = a.IDKV;
                    DO.NguoiNhap = a.NguoiNhap;
                    DO.TinhTrang = (int)a.TinhTrang;
                    DO.GhiChu = a.GhiChu;
                    DO.FileUpload = a.FileUpload;
                    DO.IDMDD = (int)a.IDMDD;
                    DO.SoKhung = a.SoKhung;
                    DO.ThoiHanTheTu = (DateTime)a.ThoiHanTheTu;
                    DO.ThoiHanTheRVTX = (DateTime)a.ThoiHanTheRVTX;
                    DO.CapMoi = a.CapMoi;
                    DO.GiaHan = a.GiaHan;
                    DO.MatThe = a.MatThe;
                    DO.CapNhatThongTin = a.CapNhatThongTin;
                    DO.BaoLanh = a.BaoLanh;
                }
                List<PhongBan> pb = db1.PhongBans.ToList();
                ViewBag.IDPB = new SelectList(pb, "IDPhongBan", "TenPhongBan");
                List<NT_Partner> nt = db1.NT_Partner.ToList();
                ViewBag.NTList = new SelectList(nt, "ID", "FullName");
                List<NT_MaDinhDanh> mdd = db.NT_MaDinhDanh.ToList();
                ViewBag.MDDList = new SelectList(mdd, "IDDD", "TenMaDinhDanh");

                ViewBag.NgayHetHanDK = DO.NgayHetHanDK.ToString("yyyy-MM-dd");
                ViewBag.NgayHetHanKD = DO.NgayHetHanKD.ToString("yyyy-MM-dd");
                ViewBag.NgayHetHanBHX = DO.NgayHetHanBHX.ToString("yyyy-MM-dd");
                ViewBag.NgayHetHanTAT = DO.NgayHetHanTAT.ToString("yyyy-MM-dd");
                ViewBag.NgayHetHanVCHHChayNo = DO.NgayHetHanVCHHChayNo.ToString("yyyy-MM-dd");
                ViewBag.NgayHetHanCNKD = DO.NgayHetHanCNKD.ToString("yyyy-MM-dd");
                ViewBag.ThoiHanTheTu = DO.ThoiHanTheTu.ToString("yyyy-MM-dd");
                ViewBag.ThoiHanTheRVTX = DO.ThoiHanTheRVTX.ToString("yyyy-MM-dd");
                ViewBag.StartDate = DO.StartDate.ToString("yyyy-MM-dd");
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
        }


        [HttpPost]
        public ActionResult Edit(VehicleManagementValidation _DO)
        {
            try
            {
                var Check_ID = db.NT_ContactEquipment.Where(x=>x.IDTBNT == _DO.IDTBNT).FirstOrDefault();
                if (_DO.NameFile != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/FileContractEquiment/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = _DO.NameFile.FileName != null ? _DO.NameEquipment + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                    if (_DO.NameFile != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.NameFile.SaveAs(path + FileName);
                        _DO.FileUpload = "~/UploadedFiles/FileContract/" + FileName;
                    }
                    db.NT_ContactEquipment_update(_DO.IDTBNT, _DO.NameEquipment, _DO.IndentifierContact, _DO.BienSoXe, _DO.IDContact, _DO.Operator,
                                         _DO.IDDepartment, _DO.Cavet, _DO.NgayHetHanDK, _DO.NgayHetHanKD, _DO.NgayHetHanBHX, _DO.CCHV, _DO.NgayHetHanTAT,
                                         _DO.NgayHetHanVCHHChayNo, _DO.NgayHetHanCNKD, _DO.ChungChiPCCCCNCH, _DO.ChungChiVCHHNHCN,
                                         _DO.StartDate, _DO.IDKV, Models.MyAuthentication.TenNV, _DO.TinhTrang, _DO.GhiChu, _DO.FileUpload, _DO.IDMDD, _DO.SoKhung,
                                         _DO.ThoiHanTheTu, _DO.ThoiHanTheRVTX, _DO.CapMoi, _DO.GiaHan, _DO.MatThe, _DO.CapNhatThongTin, _DO.BaoLanh, DateTime.Now, Check_ID.FileTheTu);

                }
                else
                {
                    db.NT_ContactEquipment_update(_DO.IDTBNT, _DO.NameEquipment, _DO.IndentifierContact, _DO.BienSoXe, _DO.IDContact, _DO.Operator,
                                       _DO.IDDepartment, _DO.Cavet, _DO.NgayHetHanDK, _DO.NgayHetHanKD, _DO.NgayHetHanBHX, _DO.CCHV, _DO.NgayHetHanTAT,
                                       _DO.NgayHetHanVCHHChayNo, _DO.NgayHetHanCNKD, _DO.ChungChiPCCCCNCH, _DO.ChungChiVCHHNHCN,
                                       _DO.StartDate, _DO.IDKV, Models.MyAuthentication.TenNV, _DO.TinhTrang, _DO.GhiChu, Check_ID.FileUpload, _DO.IDMDD, _DO.SoKhung,
                                       _DO.ThoiHanTheTu, _DO.ThoiHanTheRVTX, _DO.CapMoi, _DO.GiaHan, _DO.MatThe, _DO.CapNhatThongTin, _DO.BaoLanh, DateTime.Now, Check_ID.FileTheTu);
                }    
           
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "VehicleManagement");
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM.QLPT.xlsx";
            return File(path, "application/vnd.ms-excel", "BM.DanhSachTheTamNT_Import.xlsx");
        }
        public ActionResult ImportExcel()
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
        public ActionResult ImportExcel(VehicleManagementValidation _DO)
        {
            string filePath = string.Empty;
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["FileUpload"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string path = Server.MapPath("~/UploadedFiles/FileContractEquiment");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    filePath = path + Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm") + "-" + file.FileName);

                    file.SaveAs(filePath);
                    Stream stream = file.InputStream;

                    IExcelDataReader reader = null;
                    if (file.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (file.FileName.EndsWith(".xlsx"))
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
                    int dtc = 0;
                    var sIndentifierContact = "";
                    try
                    {
                        for (int i = 3; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i] != null)
                            {
                                var NameEquipment = dt.Rows[i][1].ToString().Trim();
                                var IndentifierContact = dt.Rows[i][2].ToString().Trim();
                                var BienSoXe = dt.Rows[i][3].ToString().Trim();
                                var Contact = dt.Rows[i][4].ToString().Trim();
                                var IDNT = db1.NT_Partner.Where(x => x.FullName == Contact).FirstOrDefault();
                                var Operator = dt.Rows[i][5].ToString().Trim();
                                var Department = dt.Rows[i][6].ToString().Trim();
                                var IDPB = db1.PhongBans.Where(x => x.TenPhongBan == Department).FirstOrDefault();
                                var Cavet = dt.Rows[i][7].ToString().Trim();
                                var IDCavet = "";
                                if (Cavet.StartsWith("Có"))
                                {
                                    Cavet = "0";
                                }
                                else if (Cavet.StartsWith("Không"))
                                {
                                    Cavet = "1";
                                }
                                var SoKhung = dt.Rows[i][8].ToString().Trim();
                                var ThoiHanTheTu = dt.Rows[i][9].ToString().Trim();
                                var ThoiHanTheRVTX = dt.Rows[i][10].ToString().Trim();
                                var NgayHetHanDK = dt.Rows[i][11].ToString().Trim();
                                var NgayHetHanKD = dt.Rows[i][12].ToString().Trim();
                                var NgayHetHanBHX = dt.Rows[i][13].ToString().Trim();
                                var CCHV = dt.Rows[i][14].ToString().Trim();
                                var IDCCHV = "";
                                if (CCHV.StartsWith("Có"))
                                {
                                    CCHV = "0";
                                }
                                else if (CCHV.StartsWith("Không"))
                                {
                                    CCHV = "1";
                                }
                                var NgayHetHanTAT = dt.Rows[i][15].ToString().Trim();
                                var NgayHetHanVCHHChayNo = dt.Rows[i][16].ToString().Trim();
                                var NgayHetHanCNKD = dt.Rows[i][17].ToString().Trim();
                                var ChungChiPCCCCNCH = dt.Rows[i][18].ToString().Trim();
                                var ChungChiVCHHNHCN = dt.Rows[i][19].ToString().Trim();
                                var StartDate = dt.Rows[i][20].ToString().Trim();
                                var IDKV = dt.Rows[i][21].ToString().Trim();
                                var NguoiNhap = dt.Rows[i][22].ToString().Trim();
                                var TinhTrang = dt.Rows[i][23].ToString().Trim();
                                var IDTinhTrang = "";
                                if (TinhTrang.StartsWith("Đang hoạt động"))
                                {
                                    TinhTrang = "0";
                                }
                                else if (TinhTrang.StartsWith("Không còn hoạt động"))
                                {
                                    TinhTrang = "1";
                                }
                                var IDMDD = dt.Rows[i][2].ToString().Trim();

                                if (IndentifierContact.StartsWith("NT-"))
                                {
                                    IDMDD = "1";

                                    sIndentifierContact = IndentifierContact.Remove(0, 3);
                                }
                                else if (IndentifierContact.StartsWith("NT.DQ2-"))
                                {
                                    IDMDD = "2";
                                    sIndentifierContact = IndentifierContact.Remove(0, 7);
                                }
                                else if (IndentifierContact.StartsWith("HP-"))
                                {
                                    IDMDD = "3";
                                    sIndentifierContact = IndentifierContact.Remove(0, 3);
                                }
                                else if (IndentifierContact.StartsWith("HPSX-"))
                                {
                                    IDMDD = "4";
                                    sIndentifierContact = IndentifierContact.Remove(0, 5);
                                }
                                if (NameEquipment != null || NameEquipment != "")
                                {
                                    if (ThoiHanTheTu == null || ThoiHanTheTu == "")
                                    {
                                        ThoiHanTheTu = "01-01-0001";
                                    }
                                    if (NameEquipment != "" && ThoiHanTheRVTX == null || ThoiHanTheRVTX == "")
                                    {
                                        ThoiHanTheRVTX = "01-01-0001";
                                    }
                                    if (NgayHetHanDK == null || NgayHetHanDK == "")
                                    {
                                        NgayHetHanDK = "01-01-0001";

                                    }
                                    if (NgayHetHanKD == null || NgayHetHanKD == "")
                                    {
                                        NgayHetHanKD = "01-01-0001";

                                    }
                                    if (NgayHetHanBHX == null || NgayHetHanBHX == "")
                                    {
                                        NgayHetHanBHX = "01-01-0001";

                                    }
                                    if (NgayHetHanTAT == null || NgayHetHanTAT == "")
                                    {
                                        NgayHetHanTAT = "01-01-0001";

                                    }
                                    if (NgayHetHanVCHHChayNo == null || NgayHetHanVCHHChayNo == "")
                                    {
                                        NgayHetHanVCHHChayNo = "01-01-0001";

                                    }
                                    if (NgayHetHanCNKD == null || NgayHetHanCNKD == "")
                                    {
                                        NgayHetHanCNKD = "01-01-0001";

                                    }
                                }
                                if (BienSoXe == "" || BienSoXe == null)
                                {
                                    BienSoXe = "Không có";
                                }
                                if (SoKhung == "" || SoKhung == null)
                                {
                                    SoKhung = "Không có";
                                }
                                var CapMoi = dt.Rows[i][24].ToString().Trim();
                                var GiaHan = dt.Rows[i][25].ToString().Trim();
                                var MatThe = dt.Rows[i][26].ToString().Trim();
                                var CapNhatThongTin = dt.Rows[i][27].ToString().Trim();
                                var BaoLanh = dt.Rows[i][28].ToString().Trim();
                                var GhiChu = dt.Rows[i][29].ToString().Trim();
                                //InsertDSPhuongTien(NameEquipment, sIndentifierContact, BienSoXe, Convert.ToInt32(IDNT.ID), Operator,
                                //        Convert.ToInt32(IDPB.IDPhongBan), Convert.ToInt32(Cavet), Convert.ToDateTime(NgayHetHanDK), Convert.ToDateTime(NgayHetHanKD),
                                //        Convert.ToDateTime(NgayHetHanBHX), Convert.ToInt32(CCHV), Convert.ToDateTime(NgayHetHanTAT), Convert.ToDateTime(NgayHetHanVCHHChayNo),
                                //        Convert.ToDateTime(NgayHetHanCNKD), ChungChiPCCCCNCH, ChungChiVCHHNHCN,
                                //        Convert.ToDateTime(StartDate), IDKV, NguoiNhap, Convert.ToInt32(TinhTrang), GhiChu, "", Convert.ToInt32(IDMDD), SoKhung,
                                //        Convert.ToDateTime(ThoiHanTheTu), Convert.ToDateTime(ThoiHanTheRVTX));

                                db.NT_ContactEquipment_insert(NameEquipment, sIndentifierContact, BienSoXe, Convert.ToInt32(IDNT.ID), Operator,
                                           Convert.ToInt32(IDPB.IDPhongBan), Convert.ToInt32(Cavet), Convert.ToDateTime(NgayHetHanDK), Convert.ToDateTime(NgayHetHanKD), Convert.ToDateTime(NgayHetHanBHX), Convert.ToInt32(CCHV), Convert.ToDateTime(NgayHetHanTAT),
                                           Convert.ToDateTime(NgayHetHanVCHHChayNo), Convert.ToDateTime(NgayHetHanCNKD), ChungChiPCCCCNCH, ChungChiVCHHNHCN,
                                          Convert.ToDateTime(StartDate), IDKV, NguoiNhap, Convert.ToInt32(TinhTrang), GhiChu, "", Convert.ToInt32(IDMDD), SoKhung,
                                            Convert.ToDateTime(ThoiHanTheTu), Convert.ToDateTime(ThoiHanTheRVTX), CapMoi, GiaHan, MatThe, CapNhatThongTin, BaoLanh, DateTime.Now, "");
                                dtc++;
                            }

                        }
                        string msg = "";
                        if (dtc != 0)
                        {
                            msg = "Import được " + dtc + " dòng dữ liệu";
                        }
                        else { msg = "File import không có dữ liệu"; }


                        TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";
                    }
                    catch (Exception ex)
                    {
                        string msg = "";
                        if (dtc != 0)
                        {
                            msg = "Import được " + dtc + " dòng dữ liệu";
                        }
                        else { msg = "File import không có dữ liệu"; }


                        TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";
                    }
                }
                else
                {
                    TempData["msgError"] = "<script>alert('File import không đúng định dạng. Vui lòng kiểm tra biểu mẫu file import');</script>";
                }

            }
            else
            {
                TempData["msgError"] = "<script>alert('Vui lòng nhập file Import');</script>";
            }

            return RedirectToAction("Index", "VehicleManagement");
        }

        public ActionResult ExportToExcel()
        {
            string Name = "";
            int number = 0;
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM.DanhSachTheTamNT.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM.DanhSachTheTamNT_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("TB");
                var List = db.NT_ContactEquipment.ToList();
                if (List.Count > 0)
                {
                    int row = 3,stt = 0;
                  
                    foreach (var item in List)
                    {

                        row++; stt++; number++;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.NameEquipment;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        var MDD = db.NT_MaDinhDanh.Where(x => x.IDDD == item.IDMDD).FirstOrDefault();
                        if(MDD != null)
                        {
                            Worksheet.Cell("C" + row).Value = MDD.MaDinhDanh + "-" + @item.IndentifierContact;
                            Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;
                        }
     

                        Worksheet.Cell("D" + row).Value = item.BienSoXe;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                        var IDNT = db1.NT_Partner.Where(p => p.ID == item.IDContact).FirstOrDefault();
                        if(IDNT != null)
                        {
                            Worksheet.Cell("E" + row).Value = IDNT.FullName;
                            Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;
                        }    
                 
                        Worksheet.Cell("F" + row).Value = item.Operator;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        var IDPhongBan = db1.PhongBans.Where(p => p.IDPhongBan == item.IDDepartment).FirstOrDefault();
                        if(IDPhongBan != null)
                        {
                            Worksheet.Cell("G" + row).Value = IDPhongBan.TenPhongBan;
                            Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;
                        }    


                        if(item.Cavet == 1)
                        {
                            Worksheet.Cell("H" + row).Value = "Có";
                            Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;
                        } 
                        else
                        {
                            Worksheet.Cell("H" + row).Value = "Không";
                            Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        }    
  

                        Worksheet.Cell("I" + row).Value = item.SoKhung;
                        Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                        if(item.ThoiHanTheTu != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("J" + row).Value = item.ThoiHanTheTu;
                            Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;
                        }

                        if (item.ThoiHanTheRVTX != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("K" + row).Value = item.ThoiHanTheRVTX;
                            Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;

                        }
                        if (item.NgayHetHanDK != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("L" + row).Value = item.NgayHetHanDK;
                            Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;
                        }

                        if (item.NgayHetHanKD != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("M" + row).Value = item.NgayHetHanKD;
                            Worksheet.Cell("M" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("M" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("M" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.NgayHetHanBHX != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("N" + row).Value = item.NgayHetHanBHX;
                            Worksheet.Cell("N" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("N" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("N" + row).Style.Alignment.WrapText = true;
                        }


                        

                        if(item.CCHV == 0)
                        {
                            Worksheet.Cell("O" + row).Value = "Không";
                            Worksheet.Cell("O" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell("O" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("O" + row).Style.Alignment.WrapText = true;
                        }
                        else
                        {
                            Worksheet.Cell("O" + row).Value = "Có";
                            Worksheet.Cell("O" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell("O" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("O" + row).Style.Alignment.WrapText = true;
                        }

                        if (item.NgayHetHanTAT != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("P" + row).Value = item.NgayHetHanTAT;
                            Worksheet.Cell("P" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("P" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("P" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.NgayHetHanVCHHChayNo != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("Q" + row).Value = item.NgayHetHanVCHHChayNo;
                            Worksheet.Cell("Q" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("Q" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("Q" + row).Style.Alignment.WrapText = true;
                        }

                        if (item.NgayHetHanCNKD != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("R" + row).Value = item.NgayHetHanCNKD;
                            Worksheet.Cell("R" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("R" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("R" + row).Style.Alignment.WrapText = true;
                        }


                     

                        Worksheet.Cell("S" + row).Value = item.ChungChiPCCCCNCH;
                        Worksheet.Cell("S" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("S" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("S" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("T" + row).Value = item.ChungChiVCHHNHCN;
                        Worksheet.Cell("T" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("T" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("T" + row).Style.Alignment.WrapText = true;

                        if (item.StartDate != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("U" + row).Value = item.StartDate;
                            Worksheet.Cell("U" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("U" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("U" + row).Style.Alignment.WrapText = true;

                        }

                        Worksheet.Cell("V" + row).Value = item.KhuVucThiCong;
                        Worksheet.Cell("V" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("V" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("V" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("W" + row).Value = item.NguoiNhap;
                        Worksheet.Cell("W" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("W" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("W" + row).Style.Alignment.WrapText = true;


                        if(item.TinhTrang == 0)
                        {
                            Worksheet.Cell("X" + row).Value = "Đang hoạt động";
                            Worksheet.Cell("X" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("X" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("X" + row).Style.Alignment.WrapText = true;
                        }
                        else
                        {

                            Worksheet.Cell("X" + row).Value = "Không hoạt động";
                            Worksheet.Cell("X" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("X" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("X" + row).Style.Alignment.WrapText = true;
                        }

                        if(item.CapMoi == "1" || item.CapMoi == "X" || item.CapMoi == "x")
                        {
                            Worksheet.Cell("Y" + row).Value = "X";
                            Worksheet.Cell("Y" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("Y" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("Y" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.GiaHan == "1" || item.GiaHan == "X" || item.GiaHan == "x")
                        {
                            Worksheet.Cell("Z" + row).Value = "X";
                            Worksheet.Cell("Z" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("Z" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("Z" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.MatThe == "1" || item.MatThe == "X" || item.MatThe == "x")
                        {
                            Worksheet.Cell("AA" + row).Value = "X";
                            Worksheet.Cell("AA" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("AA" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("AA" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.CapNhatThongTin == "1" || item.CapNhatThongTin == "X" || item.CapNhatThongTin == "x")
                        {
                            Worksheet.Cell("AB" + row).Value = "X";
                            Worksheet.Cell("AB" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("AB" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("AB" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.BaoLanh == "1" || item.BaoLanh == "X" || item.BaoLanh == "x")
                        {
                            Worksheet.Cell("AC" + row).Value = "X";
                            Worksheet.Cell("AC" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("AC" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("AC" + row).Style.Alignment.WrapText = true;
                        }
                        if (item.NgayChinhSua != DateTime.Parse("0001/01/01"))
                        {
                            Worksheet.Cell("AD" + row).Value = item.NgayChinhSua;
                            Worksheet.Cell("AD" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell("AD" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell("AD" + row).Style.Alignment.WrapText = true;
                        }    


                        Worksheet.Cell("AF" + row).Value = item.GhiChu;
                        Worksheet.Cell("AF" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("AF" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("AF" + row).Style.Alignment.WrapText = true;
                    }
                    Worksheet.Range("A4:AF" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A4:AF" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A4:AF" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A4:AF" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "BM.DanhSachTheTamNT.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Có lỗi khi xuất dữ liệu');</script>";
                    return RedirectToAction("Index", "VehicleManagement");
                }

            }
            catch (Exception ex)
            {
                TempData["msgError"] = "<script>alert('Có lỗi ở phương tiện: " + Name + " tại dòng  : '" + number + "');</script>";
                return RedirectToAction("Index", "VehicleManagement");
            }

        }
        public ActionResult Create_New()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<PhongBan> pb = db1.PhongBans.ToList();
            ViewBag.IDPB = new SelectList(pb, "IDPhongBan", "TenPhongBan");
            List<NT_Partner> nt = db1.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");
            List<NT_MaDinhDanh> mdd = db.NT_MaDinhDanh.ToList();
            ViewBag.MDDList = new SelectList(mdd, "IDDD", "TenMaDinhDanh");
            return PartialView();
        }

        [HttpPost]
        public ActionResult Create_New(VehicleManagementValidation _DO)
        {
            try
            {
                if (_DO.NameFile != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/FileContractEquiment/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = _DO.NameFile != null ? _DO.NameEquipment + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                    if (_DO.NameFile != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.NameFile.SaveAs(path + FileName);
                        _DO.FileUpload = "~/UploadedFiles/FileContract/" + FileName;
                    }

                }

                db.NT_ContactEquipment_insert(_DO.NameEquipment, _DO.IndentifierContact, _DO.BienSoXe, _DO.IDContact, _DO.Operator,
                                              _DO.IDDepartment, _DO.Cavet, _DO.NgayHetHanDK, _DO.NgayHetHanKD, _DO.NgayHetHanBHX, _DO.CCHV, _DO.NgayHetHanTAT,
                                              _DO.NgayHetHanVCHHChayNo, _DO.NgayHetHanCNKD, _DO.ChungChiPCCCCNCH, _DO.ChungChiVCHHNHCN,
                                              _DO.StartDate, _DO.IDKV, Models.MyAuthentication.TenNV, _DO.TinhTrang, _DO.GhiChu, _DO.FileUpload, _DO.IDMDD, _DO.SoKhung,
                                              _DO.ThoiHanTheTu, _DO.ThoiHanTheRVTX,_DO.CapMoi, _DO.GiaHan, _DO.MatThe, _DO.CapNhatThongTin, _DO.BaoLanh, DateTime.Now, _DO.FileTheTu );
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới! " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "VehicleManagement");
        }
        public ActionResult Update_File(int? id, int? idl)
        {
            VehicleManagementValidation DO = new VehicleManagementValidation();
            DO.IDTBNT = (int)id;
            DO.IDLoai = (int)idl;
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Update_File(VehicleManagementValidation _DO)
        {
            if(_DO.IDLoai == 1)
            {
                var Check = db.NT_ContactEquipment.Where(x => x.IDTBNT == _DO.IDTBNT).FirstOrDefault();
                if(Check != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/FileContractEquiment/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = _DO.NameFile != null ? _DO.NameEquipment + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                    if (_DO.NameFile != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.NameFile.SaveAs(path + FileName);
                        _DO.FileUpload = "~/UploadedFiles/FileContract/" + FileName;
                    }
                    db.NT_ContactEquipment_update_file(_DO.IDTBNT, _DO.FileUpload, Check.FileTheTu);
                }


            }   
            else
            {
                var Check = db.NT_ContactEquipment.Where(x=>x.IDTBNT== _DO.IDTBNT).FirstOrDefault();
                if (Check != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/FileContractEquiment/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = _DO.NameFile != null ? _DO.NameEquipment + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    string FileExtension = _DO.NameFile != null ? Path.GetExtension(_DO.NameFile.FileName) : "";

                    if (_DO.NameFile != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.NameFile.SaveAs(path + FileName);
                        _DO.FileUpload = "~/UploadedFiles/FileContract/" + FileName;
                    }
                    db.NT_ContactEquipment_update_file(_DO.IDTBNT, Check.FileUpload, _DO.FileUpload);

                }

            }    
            return RedirectToAction("Index", "VehicleManagement");
        }

    }
}
