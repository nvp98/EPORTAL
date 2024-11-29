using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Rotativa;
using Rotativa.Options;
using DocumentFormat.OpenXml.Presentation;
using Font = iTextSharp.text.Font;
using static iTextSharp.text.Font;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace EPORTAL.Areas.TagSign.Controllers.ViewNT
{
    public class ListExtendcardNTController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListExtendcardNT";
        // GET: TagSign/ListExtendcardNT
        public ActionResult Index(int? page, string search, DateTime? begind, DateTime? endd)
        {
            var res = (from a in db_dk.DK_CardExtend.Where(x => x.NhanVienID == Models.MyAuthentication.ID)
                       select new DK_ListExtendcardNTValidation
                       {
                           IDGHT = (int)a.IDGHT,
                           NoiDung = a.NoiDung,
                           NTID = (int)a.NTID,
                           HDID = (int?)a.HDID ?? default,
                           PhongBanID = (int?)a.PhongBanID ?? default,
                           NgayDangKy = (DateTime)a.NgayDangKy,
                           NhanVienID = (int?)a.NhanVienID ?? default,
                           TinhTrangID = (int?)a.TinhTrangID ?? default,
                           GhiChu = a.GhiChu,
                           LoaiNT = (int)a.LoaiNT
                       }).ToList();
            if (begind != null && endd != null)
            {
                res = res.OrderByDescending(x => x.NgayDangKy).Where(x => x.NgayDangKy >= begind && x.NgayDangKy <= endd).ToList();
            }
            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            return View(res.OrderByDescending(x => x.NgayDangKy).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Delete(int? id)
        {
            try
            {
                db_dk.DK_DetailCardExtend_Delete(id);
                db_dk.DK_CardExtend_Delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListExtendcardNT");
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM de nghi cap lai, gia han, doi thong tin.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_GHT.xlsx");
        }
        public JsonResult GetHD(int? IDNT)
        {
            if (IDNT == null) IDNT = 0;
            List<ContractValidation> px = (from hd in db.NT_Contract.Where(x => x.IDNT == IDNT && x.TTXL == 2)
                                           select new ContractValidation()
                                           {
                                               IDContract = hd.IDContract,
                                               Somecontracts = hd.Somecontracts,
                                               Contract = hd.Contract,
                                               IDNT = hd.IDNT
                                           }).ToList();
            return Json(px, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create()
        {
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName");

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Position> cv = db.NT_Position.ToList();
            ViewBag.IDCV = new SelectList(cv, "IDCV", "TenCV");

            List<NT_Contract> hd = new List<NT_Contract>();
            ViewBag.IDHD = new SelectList(hd, "IDContract", "Somecontracts");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            List<NT_Workingtime> ca = db.NT_Workingtime.ToList();
            ViewBag.IDCA = new SelectList(ca, "IDCA", "TenCA");

            List<NT_Workplace> kv = db.NT_Workplace.ToList();
            ViewBag.IDKV = new SelectList(kv, "IDKV", "TenKV");

            List<PhongBan> pb = db.PhongBans.ToList();
            ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(DK_Detail_ListExtendcardNTValidation _DO, FormCollection collection)
        {
            int IDGHT = 0;
            int IDNhaThau =0;
            int stt = 0;
            int dtc = 0;
            string HoVaTen = "";
            if (collection.Count > 2)
            {
                try
                {
                    var ListDS = new List<DK_Detail_ListExtendcardNTValidation>();
                    DateTime DayUpload = DateTime.Now;

                    //Upload file Img ĐK học ATLĐ

                    string path = Server.MapPath("~/PDFHocAT/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Use Namespace called :  System.IO  
                    string FileName = _DO.FileAnh != null ? DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                    //To Get File Extension  
                    string FileExtension = _DO.FileAnh != null ? Path.GetExtension(_DO.FileAnh.FileName) : "";


                    ////Add Current Date To Attached File Name  
                    if (_DO.FileAnh != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.FileAnh.SaveAs(path + FileName);
                        _DO.FileUpload = "~/PDFHocAT/" + FileName;
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                        return RedirectToAction("Index", "ListCardRegisInforNT");

                    }


                    foreach (var key in collection.AllKeys)
                    {
                        if (key != "__RequestVerificationToken")
                        {
                            string IDNT = collection["NTList"];
                            string LoaiNT = collection["NT"];
                            string NhaThauID = collection["IDNT"];
                            IDNhaThau = Convert.ToInt32(NhaThauID);
                            string BP = collection["IDPhongBan"];
                            if (LoaiNT == null && key == "NT")
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn loại nhân viên nhà thầu');</script>";
                                return RedirectToAction("Index", "ListCardRegisInforNT");
                            }
                            if (NhaThauID == "" && key == "IDNT")
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Nhà thầu');</script>";
                                return RedirectToAction("Index", "ListCardRegisInforNT");
                            }
                            if (BP == "" && key == "IDPhongBan")
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn BP quản lý');</script>";
                                return RedirectToAction("Index", "ListCardRegisInforNT");
                            }
                            if (key == "IDPhongBan")
                            {
                                System.Data.Entity.Core.Objects.ObjectParameter returnIDGHT = new System.Data.Entity.Core.Objects.ObjectParameter("IDGHT", typeof(int));
                                db_dk.DK_CardExtend_Insert(collection["NoiDung"], Convert.ToInt32(collection["IDNT"]), Convert.ToInt32(collection["IDHD"]), Convert.ToInt32(collection["IDPhongBan"]), DayUpload, Models.MyAuthentication.ID, 0, _DO.GhiChu, Convert.ToInt32(LoaiNT), _DO.FileUpload, returnIDGHT);
                                IDGHT = Convert.ToInt32(returnIDGHT.Value);
                            }
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
                        var CheckVP = db_nt.NT_NhanVienVP.Where(x => x.CCCD == item.CCCD).FirstOrDefault();
                        if (item.HoTen != "" && item.CCCD != "" && CheckVP == null && IDGHT != 0)
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
                                   null,
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
                    if (collection.Count > 6)
                    {
                        string filePath = string.Empty;
                        HttpPostedFileBase file = Request.Files["FileExcel"];
                        if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                        {
                            string pathex = Server.MapPath("~/UploadedFiles/DKHocATEX/");
                            if (!Directory.Exists(pathex))
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
                                    for (int i = 5; i < dt.Rows.Count; i++)
                                    {
                                        dtc++;
                                        string HoTen = dt.Rows[i][1].ToString().Trim();
                                        HoVaTen = HoTen;
                                        if (HoTen != "")
                                        {
                                            string BirthDay = dt.Rows[i][2].ToString().Trim();
                                            if (BirthDay == "")
                                            {
                                                if (IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                                }
                                                TempData["msgSuccess"] = "<script>alert('Kiểm tra lại ngày sinh. Nhân viên : " + HoVaTen + "');</script>";
                                                return RedirectToAction("Index", "ListCardRegisInforNT");
                                            }
                                            DateTime NgaySinh = DateTime.ParseExact(BirthDay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


                                            string CCCD = dt.Rows[i][3].ToString().Trim();
                                            var CheckVP = db_nt.NT_NhanVienVP.Where(x => x.CCCD.Contains(CCCD) & x.TinhTrang == 0).FirstOrDefault();
                                           
                                            if (CCCD.Length < 9 || CCCD.Length > 14 || CheckVP != null)
                                            {
                                                if (IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                                }
                                                if (CheckVP != null)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);

                                                    TempData["msgSuccess"] = "<script>alert('Nhân viên nằm trong danh sách vi phạm. Nhân viên : " + HoVaTen + "');</script>";
                                                    return RedirectToAction("Index", "ListExtendcardNT");
                                                }
                                            }

                                            string HoKhau = dt.Rows[i][4].ToString().Trim();
                                            string ChucVu = dt.Rows[i][5].ToString().Trim();
                                            var IDCV = db.NT_Position.Where(x => x.TenCV == ChucVu).FirstOrDefault();
                                            if (IDCV == null)
                                            {
                                                if (IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                                }

                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại chức vụ. Nhân viên : " + HoVaTen + "');</script>";
                                                return RedirectToAction("Index", "ListExtendcardNT");
                                            }
                                            string SoDienThoai = dt.Rows[i][6].ToString().Trim();

                                            if (SoDienThoai.Length != 10 && IDCV.IDCV != 9 && SoDienThoai != "")
                                            {
                                                if (IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                                }

                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại số điện thoại. Nhân viên : " + HoVaTen + "');</script>";
                                                return RedirectToAction("Index", "ListCardRegisInforNT");
                                            }

                                            string CapLai = dt.Rows[i][7].ToString().Trim();
                                            string GiaHan = dt.Rows[i][8].ToString().Trim();
                                            string ThongTinLuuTru = dt.Rows[i][9].ToString().Trim();
                                            string DTTM = dt.Rows[i][10].ToString().Trim();
                                            string RaVaoCang = dt.Rows[i][11].ToString().Trim();
                                            if (CapLai == "" && GiaHan == "" && ThongTinLuuTru == "" && RaVaoCang == "" && DTTM =="")
                                            {
                                                if (IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                                }

                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại thẻ. Nhân viên : " + HoVaTen + "');</script>";
                                                return RedirectToAction("Index", "ListExtendcardNT");
                                            }

                                            string THThe = dt.Rows[i][12].ToString().Trim();
                                            if (THThe == "")
                                            {
                                                if (IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                                }


                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra thời hạn thẻ. Nhân viên : " + HoVaTen + "');</script>";
                                                return RedirectToAction("Index", "ListExtendcardNT");
                                            }
                                            DateTime ThoiHanThe = DateTime.ParseExact(THThe, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                            string KhuVuc = dt.Rows[i][13].ToString().Trim();
                                            var NameKV = new List<string>();
                                            string[] arrList = KhuVuc.Trim().Split(',');
                                            foreach (var str in arrList)
                                            {
                                                var IDKV = db.NT_Workplace.Where(x => x.TenKV == str).FirstOrDefault();

                                                if (IDKV == null && IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);

                                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại khu vực làm việc. Nhân viên : " + HoVaTen + "');</script>";
                                                    return RedirectToAction("Index", "ListExtendcardNT");
                                                }
                                                else
                                                {
                                                    NameKV.Add(IDKV.IDKV.ToString());
                                                }
                                            }
                                            var ListKV = string.Join(",", NameKV);

                                            string NameGate = dt.Rows[i][14].ToString().Trim();
                                            var Name = new List<string>();
                                            string[] arrListStr = NameGate.Split(',');
                                            foreach (var str in arrListStr)
                                            {
                                                var IDGate = db.NT_Gate.Where(x => x.Gate == str).FirstOrDefault();

                                                if (IDGate == null && IDGHT != 0)
                                                {
                                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                    db_dk.DK_CardExtend_Delete(IDGHT);

                                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại cổng ra vào. Nhân viên : " + HoVaTen + "');</script>";
                                                    return RedirectToAction("Index", "ListExtendcardNT");
                                                }
                                                else
                                                {
                                                    Name.Add(IDGate.IDGate.ToString());
                                                }
                                            }
                                            var ListGate = string.Join(",", Name);


                                            string NameGroup = dt.Rows[i][15].ToString().Trim();
                                            var IDGroup = db.NT_ContractorGroup.Where(x => x.NameContractorGroup == NameGroup).FirstOrDefault();
                                            if (IDGroup == null)
                                            {
                                                db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                                db_dk.DK_CardExtend_Delete(IDGHT);

                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại nhóm Nhà thầu. Nhân viên : " + HoVaTen + "');</script>";
                                                return RedirectToAction("Index", "ListExtendcardNT");
                                            }

                                            //string GhiChu = dt.Rows[i][16].ToString().Trim();
                                            var insert = db_dk.DK_DetailCardExtend_Insert
                                            (HoTen,
                                            CCCD,
                                            NgaySinh,
                                            HoKhau,
                                            IDCV.IDCV,
                                            SoDienThoai,
                                            CapLai,
                                            GiaHan,
                                            ThongTinLuuTru,
                                            DTTM,
                                            RaVaoCang,
                                            ThoiHanThe,
                                            ListKV,
                                            ListGate,
                                            Convert.ToInt32(IDGroup.IDGroup),
                                            "",
                                            IDGHT);
                                            dtc++;
                                        }

                                    }
                                    return RedirectToAction("Form", "Detail_ListExtendcardNT", new { id = IDGHT });

                                }
                                catch (Exception ex)
                                {
                                   
                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra thông tin. Nhân viên : " + HoVaTen + "');</script>";
                                    return RedirectToAction("Index", "ListExtendcardNT");
                                }

                            }
                            else
                            {
                                if (IDGHT != 0)
                                {
                                    db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                    db_dk.DK_CardExtend_Delete(IDGHT);
                                }
                                TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                            }

                        }
                        else
                        {
                            if (IDGHT != 0)
                            {
                                db_dk.DK_DetailCardExtend_Delete(IDGHT);
                                db_dk.DK_CardExtend_Delete(IDGHT);
                            }
                            TempData["msgSuccess"] = "<script>alert('Vui lòng nhập dữ liệu');</script>";
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin. Nhân viên : " + HoVaTen + "');</script>";
                }
            }
            return RedirectToAction("Index", "ListExtendcardNT");
        }

        public ActionResult CheckInformation(int id)
        {

            var IDCVKN = Models.MyAuthentication.IDPhongban.ToString();
            var PhongBanID = db_dk.DK_CardExtend.Where(x => x.IDGHT == id).FirstOrDefault();
            //KTV ký nháy
            var KTV = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 2 && x.NhanVien.IDPhongBan == PhongBanID.PhongBanID || x.IDLKD == 2 && x.IDCVKN.Contains(PhongBanID.PhongBanID.ToString()))
                       join a in db.NhanViens on au.IDNhanVien equals a.ID
                       select new CheckInforUser
                       {
                           IDNhanVien = (int)au.IDNhanVien,
                           HoTen = a.HoTen + " : " + a.MaNV,
                       }).ToList();

            //BPLQ ký nháy
            var BPLQ = (from au in db.AuthorizationContractors
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            //TP BP/NM
            var TPBP = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == PhongBanID.PhongBanID || x.IDCVKN.Contains(PhongBanID.PhongBanID.ToString()))
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            //TP BP/NM
            var TPHCDN = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == 7 || x.IDLKD == 1 && x.NhanVien.IDPhongBan == 55)
                          join a in db.NhanViens on au.IDNhanVien equals a.ID
                          select new CheckInforUser
                          {
                              IDNhanVien = (int)au.IDNhanVien,
                              HoTen = a.HoTen + " : " + a.MaNV,
                          }).ToList();
            //VP1 C
            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 3)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            ViewBag.KTVList = new SelectList(KTV, "IDNhanVien", "HoTen");
            ViewBag.TPBPList = new SelectList(TPBP, "IDNhanVien", "HoTen");
            ViewBag.TPHCDNList = new SelectList(TPHCDN, "IDNhanVien", "HoTen");
            ViewBag.VP1CList = new SelectList(VP1C, "IDNhanVien", "HoTen");
            ViewBag.BPLQList = new SelectList(BPLQ, "IDNhanVien", "HoTen");
            ViewBag.ID = id;



            return PartialView();
        }
        [HttpPost]
        public ActionResult CheckInformation(TK_CardExtendValidation _DO, int? id)
        {
            try
            {

                if (_DO.BPQL != 0 && _DO.VP1C != 0)
                {
                    //var Check = db.KD_TrinhKy.Where(x => x.IDDS == id).FirstOrDefault();
                    if (_DO.KTV != 0)
                    {
                        db_dk.TK_CardExtend_Insert(id, 1, 0, _DO.KTV, null, _DO.GhiChu);
                    }
                    if (_DO.BPQL != 0)
                    {
                        db_dk.TK_CardExtend_Insert(id, 2, 0, _DO.BPQL, null, _DO.GhiChu);
                    }
                    if (_DO.BPLQ != 0)
                    {
                        db_dk.TK_CardExtend_Insert(id, 3, 0, _DO.BPLQ, null, _DO.GhiChu);
                    }
                    if (_DO.VP1C != 0)
                    {
                        db_dk.TK_CardExtend_Insert(id, 4, 0, _DO.VP1C, null, _DO.GhiChu);
                    }

                    TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";

                    db_dk.DK_CardExtend_UpdateTK(id, 1);
                }
                else if (_DO.VP1C == 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn VP1C');</script>";
                }

            }

            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Trình ký thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListExtendcardNT");
        }

        public ActionResult Cancel(int? id)
        {
            try
            {
                db_dk.TK_CardExtend_Cancel(id);
                TempData["msgSuccess"] = "<script>alert('Hủy trình thành công');</script>";
                db_dk.DK_CardExtend_UpdateTK(id, 0);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListExtendcardNT");
        }
        public ActionResult TestView(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardExtend.Where(x => x.IDGHT == id && x.GhiChu == "" || x.IDGHT == id && x.GhiChu == null)
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
                           GhiChu = a.GhiChu?? "",
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
                return View(DO);
            }
            else
            {
                return RedirectToAction("Index", "ListExtendcardNT");
            }

        }
        public ActionResult PrintTestView(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardExtend.Where(x => x.IDGHT == id && x.GhiChu == "" || x.IDGHT == id && x.GhiChu == null)
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
                return View(DO);
            }
            else
            {
                return RedirectToAction("Index", "ListExtendcardNT");
            }

        }
        public void tempFilePdf(int? id, String path)
        {
            //var root = Server.MapPath("~/PDF/");
            //var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            //var path = Path.Combine(root, pdfname);
            //path = Path.GetFullPath(path);
            string html = Server.MapPath("~/Views/Shared/footer.html");
            //string html1 = Server.MapPath("~/Views/Shared/header.html");
            string footer = string.Format(
                        "--footer-html \"{0}\" " +
                        "--footer-spacing \"5\" " +
                        "--footer-font-size \"5\" " +
                        "--footer-line --encoding utf-8", html);
            //string footer = "--footer-right \"Page: [page] of [toPage]\" " + "--footer-center \"Tài liệu này thuộc sở hữu của Hòa Phát Dung Quất. Việc phát tán, sử dụng trái phép bị nghiêm cấm\" --footer-line --footer-font-size \"9\" --footer-spacing 5 --footer-font-name \"Arial Unicode MS\" --encoding \"utf-8\"";
            var actionPDF = new ActionAsPdf("PrintTestView", new { id = id })
            {
                //FileName = "cv.pdf",
                ////PageOrientation = Orientation.Portrait,
                PageSize = Size.A4,
                PageMargins = new Margins(13, 5, 10, 5),
                PageWidth = 500,
                PageHeight = 300,
                ////IsLowQuality = true ,
                CustomSwitches = footer,
                MinimumFontSize = 20,
                //SaveOnServerPath = path
            };
            var applicationPDFData = actionPDF.BuildPdf(this.ControllerContext);
            System.IO.File.WriteAllBytes(path, applicationPDFData);

        }
        public ActionResult ExportToPdfsss(int? id)
        {


            var root = Server.MapPath("~/UploadedFiles/PDFDangKyGH/");
            var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            string destinationpath = string.Empty;

            tempFilePdf(id, path);
            var IDGH = db_dk.DK_CardExtend.Where(x => x.IDGHT == id).FirstOrDefault();
            var IDNT = db.NT_Partner.Where(x => x.ID == IDGH.NTID).FirstOrDefault();
            // Destination File path
            destinationpath = IDNT.FullName + ".pdf";
            //Source File Path
            string originalFile = path;



            // Read file from file location
            PdfReader reader = new PdfReader(originalFile);

            //define font size and style
            Font font = new Font(Font.FontFamily.HELVETICA, 16, Font.NORMAL, BaseColor.LIGHT_GRAY);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var pdfStamper = new PdfStamper(reader, memoryStream, '\0'))
                {
                    // Getting total number of pages of the Existing Document
                    int pageCount = reader.NumberOfPages;

                    // Create two New Layer for Watermark
                    PdfLayer layer = new PdfLayer("WatermarkLayer", pdfStamper.Writer);
                    //PdfLayer layer2 = new PdfLayer("WatermarkLayer2", pdfStamper.Writer);

                    // Loop through each Page

                    string layerwarkmarktxt = "";// define text for 
                    //string Layer2warkmarktxt = "Confidential";
                    for (int i = 1; i <= pageCount; i++)
                    {
                        // Getting the Page Size
                        Rectangle rect = reader.GetPageSize(i);
                        // Get the ContentByte object
                        PdfContentByte cb = pdfStamper.GetUnderContent(i);
                        // Tell the cb that the next commands should be "bound" to this new layer
                        // Start Layer
                        cb.BeginLayer(layer);
                        PdfGState gState = new PdfGState();
                        gState.FillOpacity = 0.1f; // define opacity level
                        cb.SetGState(gState);
                        // set font size and style for layer water mark text
                        cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 10);
                        List<string> watermarkList = new List<string>();
                        float singleWaterMarkWidth = cb.GetEffectiveStringWidth(layerwarkmarktxt, false);
                        float fontHeight = 10;
                        //Work out the Watermark for a Single Line on the Page based on the Page Width
                        float currentWaterMarkWidth = 0;
                        while (currentWaterMarkWidth + singleWaterMarkWidth < rect.Width)
                        {
                            watermarkList.Add(layerwarkmarktxt);
                            currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        }
                        //watermarkList.Add(layerwarkmarktxt);
                        //currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        //Fill the Page with Lines of Watermarks
                        float currentYPos = rect.Height;
                        //cb.BeginText();
                        //var font = new Font(Font.FontFamily.HELVETICA, 60, Font.NORMAL, BaseColor.LIGHT_GRAY);
                        //ColumnText.ShowTextAligned(pdfWriter.DirectContent, Element.ALIGN_CENTER, new Phrase("PAID", font), 300, 400, 45);
                        while (currentYPos > 0)
                        {
                            //ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), 550, 400, 45);
                            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), 200, 200, 30);
                            currentYPos -= fontHeight;
                        }
                        cb.EndLayer();
                    }

                }
                reader.Close();
                // Save file to destination location if required
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                //System.IO.File.WriteAllBytes(destinationpath, memoryStream.ToArray());
                return File(memoryStream.ToArray(), "application/pdf", destinationpath);
            }

            // send file to browse to open it from destination location.


        }


        public ActionResult Test_View(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardExtend.Where(x => x.IDGHT == id && x.GhiChu == "" || x.IDGHT == id && x.GhiChu == null)
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
                           GhiChu = a.GhiChu ?? "",
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
                return View(DO);
            }
            else
            {
                return RedirectToAction("Index", "ListExtendcardNT");
            }

        }
        public ActionResult PrintTest_View(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardExtend.Where(x => x.IDGHT == id && x.GhiChu == "" || x.IDGHT == id && x.GhiChu == null)
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
                return View(DO);
            }
            else
            {
                return RedirectToAction("Index", "ListExtendcardNT");
            }

        }
        public void tempFile_Pdf(int? id, String path)
        {
            //var root = Server.MapPath("~/PDF/");
            //var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            //var path = Path.Combine(root, pdfname);
            //path = Path.GetFullPath(path);
            string html = Server.MapPath("~/Views/Shared/footer.html");
            //string html1 = Server.MapPath("~/Views/Shared/header.html");
            string footer = string.Format(
                        "--footer-html \"{0}\" " +
                        "--footer-spacing \"5\" " +
                        "--footer-font-size \"5\" " +
                        "--footer-line --encoding utf-8", html);
            //string footer = "--footer-right \"Page: [page] of [toPage]\" " + "--footer-center \"Tài liệu này thuộc sở hữu của Hòa Phát Dung Quất. Việc phát tán, sử dụng trái phép bị nghiêm cấm\" --footer-line --footer-font-size \"9\" --footer-spacing 5 --footer-font-name \"Arial Unicode MS\" --encoding \"utf-8\"";
            var actionPDF = new ActionAsPdf("PrintTest_View", new { id = id })
            {
                //FileName = "cv.pdf",
                ////PageOrientation = Orientation.Portrait,
                PageSize = Size.A4,
                PageMargins = new Margins(13, 5, 10, 5),
                PageWidth = 500,
                PageHeight = 300,
                ////IsLowQuality = true ,
                CustomSwitches = footer,
                MinimumFontSize = 20,
                //SaveOnServerPath = path
            };
            var applicationPDFData = actionPDF.BuildPdf(this.ControllerContext);
            System.IO.File.WriteAllBytes(path, applicationPDFData);

        }
        public ActionResult ExportTo_Pdfsss(int? id)
        {


            var root = Server.MapPath("~/UploadedFiles/PDFDangKyGH/");
            var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            string destinationpath = string.Empty;

            tempFile_Pdf(id, path);
            var IDGH = db_dk.DK_CardExtend.Where(x => x.IDGHT == id).FirstOrDefault();
            var IDNT = db.NT_Partner.Where(x => x.ID == IDGH.NTID).FirstOrDefault();
            // Destination File path
            destinationpath = IDNT.FullName + ".pdf";
            //Source File Path
            string originalFile = path;



            // Read file from file location
            PdfReader reader = new PdfReader(originalFile);

            //define font size and style
            Font font = new Font(Font.FontFamily.HELVETICA, 16, Font.NORMAL, BaseColor.LIGHT_GRAY);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var pdfStamper = new PdfStamper(reader, memoryStream, '\0'))
                {
                    // Getting total number of pages of the Existing Document
                    int pageCount = reader.NumberOfPages;

                    // Create two New Layer for Watermark
                    PdfLayer layer = new PdfLayer("WatermarkLayer", pdfStamper.Writer);
                    //PdfLayer layer2 = new PdfLayer("WatermarkLayer2", pdfStamper.Writer);

                    // Loop through each Page

                    string layerwarkmarktxt = "";// define text for 
                    //string Layer2warkmarktxt = "Confidential";
                    for (int i = 1; i <= pageCount; i++)
                    {
                        // Getting the Page Size
                        Rectangle rect = reader.GetPageSize(i);
                        // Get the ContentByte object
                        PdfContentByte cb = pdfStamper.GetUnderContent(i);
                        // Tell the cb that the next commands should be "bound" to this new layer
                        // Start Layer
                        cb.BeginLayer(layer);
                        PdfGState gState = new PdfGState();
                        gState.FillOpacity = 0.1f; // define opacity level
                        cb.SetGState(gState);
                        // set font size and style for layer water mark text
                        cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 10);
                        List<string> watermarkList = new List<string>();
                        float singleWaterMarkWidth = cb.GetEffectiveStringWidth(layerwarkmarktxt, false);
                        float fontHeight = 10;
                        //Work out the Watermark for a Single Line on the Page based on the Page Width
                        float currentWaterMarkWidth = 0;
                        while (currentWaterMarkWidth + singleWaterMarkWidth < rect.Width)
                        {
                            watermarkList.Add(layerwarkmarktxt);
                            currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        }
                        //watermarkList.Add(layerwarkmarktxt);
                        //currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        //Fill the Page with Lines of Watermarks
                        float currentYPos = rect.Height;
                        //cb.BeginText();
                        //var font = new Font(Font.FontFamily.HELVETICA, 60, Font.NORMAL, BaseColor.LIGHT_GRAY);
                        //ColumnText.ShowTextAligned(pdfWriter.DirectContent, Element.ALIGN_CENTER, new Phrase("PAID", font), 300, 400, 45);
                        while (currentYPos > 0)
                        {
                            //ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), 550, 400, 45);
                            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), 200, 200, 30);
                            currentYPos -= fontHeight;
                        }
                        cb.EndLayer();
                    }

                }
                reader.Close();
                // Save file to destination location if required
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                //System.IO.File.WriteAllBytes(destinationpath, memoryStream.ToArray());
                return File(memoryStream.ToArray(), "application/pdf", destinationpath);
            }

            // send file to browse to open it from destination location.


        }
    }
}