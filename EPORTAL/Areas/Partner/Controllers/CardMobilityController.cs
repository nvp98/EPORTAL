using EPORTAL.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class CardMobilityController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "CardMobility";
        // GET: Partner/CardMobility
        public ActionResult Index(int? page, string search, int? IDNT)
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

            var NhaThau = (from nt in db.NT_Partner
                           select new NT_PartnerValidation
                           {
                               ID = (int)nt.ID,
                               FullName = nt.CodeUnis + " : " + nt.FullName
                           }).ToList();
            if (IDNT == null) IDNT = 0;
            ViewBag.BPList = new SelectList(NhaThau, "ID", "FullName", IDNT);

            var res = (from nv in db_nt.NT_CardMobility
                       join b in db_nt.NT_NhanVienNT on nv.IDNVNT equals b.IDNVNT
                       join c in db_nt.NT_TTHD on nv.IDTT equals c.IDHD
                       select new CardMobilityValidation
                       {
                           IDCD = nv.IDCD,
                           IDNVNT = (int?)nv.IDNVNT ?? default,
                           HoTen = b.HoTen,
                           CCCD = b.CCCD,
                           IDNT = (int?)b.IDNT ?? default,
                           LoaiPT = nv.LoaiPT,
                           Hangxe = nv.Hangxe,
                           Dongxe = nv.Dongxe,
                           Mausac = nv.Mausac,
                           Bienso = nv.Bienso,
                           Tungay = (DateTime?)nv.Tungay ?? default,
                           Denngay = (DateTime?)nv.Denngay ?? default,
                           Sothe = nv.Sothe,
                           IDTT = (int)nv.IDTT,
                           TenHD = c.TenHD
                       });

            if (IDNT != 0)
            {
                res = res.Where(x => x.IDNT == IDNT);
            }
            if (search != null)
            {
                res = res.Where(x => x.HoTen.Contains(search) || x.CCCD.Contains(search) || x.Sothe.Contains(search));
            }    
                ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var NVNT = (from nt in db_nt.NT_NhanVienNT
                        select new ContractorStaffValidation
                        {
                            IDNVNT = (int)nt.IDNVNT,
                            HoTen = nt.HoTen + " - " + nt.CCCD
                        }).ToList();
            ViewBag.IDNVNT = new SelectList(NVNT, "IDNVNT", "HoTen");

            List<NT_TTHD> hd = db_nt.NT_TTHD.ToList();
            ViewBag.IDHD = new SelectList(hd, "IDHD", "TenHD");

            List<NT_Partner> dt = db.NT_Partner.ToList();
            ViewBag.ID = new SelectList(dt, "ID", "FullName");
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(CardMobilityValidation _DO)
        {
            try
            {
                db_nt.NT_CardMobility_insert(_DO.IDNVNT, _DO.ID, _DO.LoaiPT, _DO.Hangxe, _DO.Dongxe, _DO.Mausac, _DO.Bienso, _DO.Tungay, _DO.Denngay, _DO.Sothe, _DO.IDHD);
                TempData["msgError"] = "<script>alert('Thêm mới thành công');</script>";

                // cập nhật history
                //db_nt.NT_NT_HistoryCardMobility_insert(_DO.IDNVNT, _DO.IDNT, _DO.Tungay, _DO.Denngay, _DO.Sothe, null);

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "CardMobility");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var res = (from nv in db_nt.NT_CardMobility.Where(x => x.IDCD == id)
                       join b in db_nt.NT_NhanVienNT on nv.IDNVNT equals b.IDNVNT
                       join c in db_nt.NT_TTHD on nv.IDTT equals c.IDHD
                       select new CardMobilityValidation
                       {
                           IDCD = nv.IDCD,
                           IDNVNT = (int)nv.IDNVNT,
                           HoTen = b.HoTen,
                           CCCD = b.CCCD,
                           IDNT = (int?)nv.IDNT ?? default,
                           LoaiPT = nv.LoaiPT,
                           Hangxe = nv.Hangxe,
                           Dongxe = nv.Dongxe,
                           Mausac = nv.Mausac,
                           Bienso = nv.Bienso,
                           Tungay = (DateTime?)nv.Tungay ?? default,
                           Denngay = (DateTime?)nv.Denngay ?? default,
                           Sothe = nv.Sothe,
                           IDTT = (int)nv.IDTT,
                           TenHD = c.TenHD
                       }).ToList();
            CardMobilityValidation DO = new CardMobilityValidation();
            if (res.Count > 0)
            {
                foreach (var nv in res)
                {
                    DO.IDCD = nv.IDCD;
                    DO.IDNVNT = (int)nv.IDNVNT;
                    DO.IDNT = (int?)nv.IDNT ?? default;
                    DO.LoaiPT = nv.LoaiPT;
                    DO.Hangxe = nv.Hangxe;
                    DO.Dongxe = nv.Dongxe;
                    DO.Mausac = nv.Mausac;
                    DO.Bienso = nv.Bienso;
                    DO.Tungay = (DateTime?)nv.Tungay ?? default;
                    DO.Denngay = (DateTime?)nv.Denngay ?? default;
                    DO.Sothe = nv.Sothe;
                    DO.IDTT = (int)nv.IDTT;
                }
                var NVNT = (from nt in db_nt.NT_NhanVienNT
                            select new ContractorStaffValidation
                            {
                                IDNVNT = (int)nt.IDNVNT,
                                HoTen = nt.HoTen + " - " + nt.CCCD
                            }).ToList();
                ViewBag.IDNVNT = new SelectList(NVNT, "IDNVNT", "HoTen", DO.IDNVNT);

                List<NT_TTHD> hd = db_nt.NT_TTHD.ToList();
                ViewBag.IDHD = new SelectList(hd, "IDHD", "TenHD", DO.IDTT);

                List<NT_Partner> dt = db.NT_Partner.ToList();
                ViewBag.ID = new SelectList(dt, "ID", "FullName", DO.IDNT);

                ViewBag.Tungay = DO.Tungay.ToString("yyyy-MM-dd");
                ViewBag.Denngay = DO.Denngay.ToString("yyyy-MM-dd");
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(CardMobilityValidation _DO)
        {
            try
            {
                var Check = db_nt.NT_CardMobility.Where(x => x.IDCD == _DO.IDCD).FirstOrDefault();

                db_nt.NT_CardMobility_update(_DO.IDCD, _DO.IDNVNT, _DO.ID, _DO.LoaiPT, _DO.Hangxe, _DO.Dongxe, _DO.Mausac, _DO.Bienso, _DO.Tungay, _DO.Denngay, _DO.Sothe, _DO.IDHD);

                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "CardMobility");
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
                db_nt.NT_CardMobility_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "CardMobility");
        }

        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_DSTCĐ.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_DSTCĐ.xlsx");
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
        public ActionResult ImportExcels()
        {
            string filePath = string.Empty;
            string HoTenNT = "";
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["FileUpload"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string path = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    filePath = path + Path.GetFileName(file.FileName);

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
                    int imp = 0;
                    int up = 0;
                    int IDNhaThau = 0;
                    string TenNV = "";
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            EPORTALEntities db = new EPORTALEntities();

                            for (int i = 6; i < dt.Rows.Count; i++)
                            {
                                string HoTen = dt.Rows[i][1].ToString();
                                TenNV = HoTen;
                                string CCCD = dt.Rows[i][2].ToString().Trim();
                                var Ckeck = db_nt.NT_NhanVienNT.Where(x => x.CCCD.Contains(CCCD)).FirstOrDefault();
                                if (Ckeck == null)
                                {

                                    string Success = "";
                                    Success = "Vui lòng kiển tra nhân viên: " + HoTen + " Tại dòng: " + (i + 1) + "";

                                    TempData["msgSuccess"] = "<script>alert('" + Success + "');</script>";

                                    return RedirectToAction("Index", "CardMobility");
                                }
                                string Sothe = dt.Rows[i][11].ToString();
                                var CheckCD = db_nt.NT_CardMobility.Where(x => x.Sothe == Sothe).FirstOrDefault();
                                var CheckVP = db_nt.NT_NhanVienVP.Where(x => x.CCCD == CCCD && x.TinhTrang == 0).FirstOrDefault();
                                if (CCCD != "" && CheckCD == null && CheckVP == null)
                                {
                                    string NhaThau = dt.Rows[i][3].ToString();
                                    int BPID = Convert.ToInt32(NhaThau);
                                    var IDNT = db.NT_Partner.Where(x => x.BPID == BPID).FirstOrDefault();
                                    if (IDNT != null)
                                    {
                                        IDNhaThau = IDNT.ID;
                                    }
                                    string LoaiPT = dt.Rows[i][4].ToString();
                                    string Hangxe = dt.Rows[i][5].ToString();
                                    string Dongxe = dt.Rows[i][6].ToString();
                                    string Mausac = dt.Rows[i][7].ToString();
                                    string Bienso = dt.Rows[i][8].ToString();
                                    string Tungay = dt.Rows[i][9].ToString();
                                    string Denngay = dt.Rows[i][10].ToString();

                                    string TTHD = dt.Rows[i][12].ToString();
                                    var IDHD = db_nt.NT_TTHD.Where(x => x.TenHD == TTHD).FirstOrDefault();

                                    if (Tungay != "" && Denngay != "")
                                    {
                                        DateTime TungayConvert = DateTime.ParseExact(Tungay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        DateTime DenngayConvert = DateTime.ParseExact(Denngay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                        db_nt.NT_CardMobility_insert(Ckeck.IDNVNT, IDNhaThau, LoaiPT, Hangxe, Dongxe, Mausac, Bienso, TungayConvert, DenngayConvert, Sothe, IDHD.IDHD);
                                    }
                                    else if (Tungay != "")
                                    {
                                     
                                        DateTime DenngayConvert = DateTime.ParseExact(Denngay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                        db_nt.NT_CardMobility_insert(Ckeck.IDNVNT, IDNhaThau, LoaiPT, Hangxe, Dongxe, Mausac, Bienso, null, DenngayConvert, Sothe, IDHD.IDHD);
                                    }
                                    else if (Denngay != "")
                                    {
                                        DateTime TungayConvert = DateTime.ParseExact(Tungay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                        db_nt.NT_CardMobility_insert(Ckeck.IDNVNT, IDNhaThau, LoaiPT, Hangxe, Dongxe, Mausac, Bienso, TungayConvert, null, Sothe, IDHD.IDHD);

                                    }
                                    imp++;
                                }
                                else if (CCCD != "" && CheckCD != null && CheckVP == null)
                                {
                                    string NhaThau = dt.Rows[i][3].ToString();
                                    int BPID = Convert.ToInt32(NhaThau);
                                    var IDNT = db.NT_Partner.Where(x => x.BPID == BPID).FirstOrDefault();
                                    if (IDNT != null)
                                    {
                                        IDNhaThau = IDNT.ID;
                                    }
                                    string LoaiPT = dt.Rows[i][4].ToString();
                                    string Hangxe = dt.Rows[i][5].ToString();
                                    string Dongxe = dt.Rows[i][6].ToString();
                                    string Mausac = dt.Rows[i][7].ToString();
                                    string Bienso = dt.Rows[i][8].ToString();
                                    string Tungay = dt.Rows[i][9].ToString();
                                    string Denngay = dt.Rows[i][10].ToString();

                                    string TTHD = dt.Rows[i][12].ToString();
                                    var IDHD = db_nt.NT_TTHD.Where(x => x.TenHD == TTHD).FirstOrDefault();
                                    if (Tungay != "" && Denngay != "")
                                    {
                                        DateTime TungayConvert = DateTime.ParseExact(Tungay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        DateTime DenngayConvert = DateTime.ParseExact(Denngay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                        db_nt.NT_CardMobility_update(CheckCD.IDCD, Ckeck.IDNVNT, IDNhaThau, LoaiPT, Hangxe, Dongxe, Mausac, Bienso, TungayConvert, DenngayConvert, Sothe, IDHD.IDHD);
                                    }
                                    else if (Tungay != "")
                                    {

                                        DateTime DenngayConvert = DateTime.ParseExact(Denngay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                        db_nt.NT_CardMobility_update(CheckCD.IDCD, Ckeck.IDNVNT, IDNhaThau, LoaiPT, Hangxe, Dongxe, Mausac, Bienso, null, DenngayConvert, Sothe, IDHD.IDHD);
                                    }
                                    else if (Denngay != "")
                                    {
                                        DateTime TungayConvert = DateTime.ParseExact(Tungay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                        db_nt.NT_CardMobility_update(CheckCD.IDCD, Ckeck.IDNVNT, IDNhaThau, LoaiPT, Hangxe, Dongxe, Mausac, Bienso, TungayConvert, null, Sothe, IDHD.IDHD);

                                    }

                                    up++;
                                }
                            }

                            string msg = "";
                            if (imp != 0 || up != 0)
                            {
                                msg = "Import " + imp + " và Update " + up + " dòng dữ liệu";
                            }
                            else { msg = "File import không có dữ liệu"; }

                            TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";

                        }
                        catch (Exception ex)
                        {
                            TempData["msgSuccess"] = "<script>alert( Vui lòng kiểm tra lỗi : " + ex.Message + " tại dòng : " + TenNV + "');</script>";
                        }
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                    }

                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
                }
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
            }

            //TempData["msgSuccess"] = "<script>alert( Vui lòng kiểm tra lỗi : " + HoTenNT + "');</script>";

            return RedirectToAction("Index", "CardMobility");
        }
    }
}