using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using Oracle.ManagedDataAccess.Client;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.EnterpriseServices.CompensatingResourceManager;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class ContractorStaffController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ContractorStaff";
        // GET: Partner/ContractorStaff
        string connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ToString();
        public ActionResult Index(int? page, string search, int? IDNT)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var NhaThau = (from nt in db.NT_Partner
                            select new NT_PartnerValidation
                            {
                                ID = (int)nt.ID,
                                FullName = nt.CodeUnis + " : " + nt.FullName
                            }).ToList();
            if (IDNT == null) IDNT = 0;
            ViewBag.BPList = new SelectList(NhaThau, "ID", "FullName", IDNT);

            DateTime DayNow = DateTime.Now;
            if (search == null) search = "";
            ViewBag.search = search;

            var res = (from nv in db_nt.NT_NhanVienNT_Select(search)
                       select new ContractorStaffValidation
                       {
                           IDNVNT = nv.IDNVNT,
                           IDNT = (int)nv.IDNT,
                           HoTen = nv.HoTen,
                           CCCD = nv.CCCD,
                           CMND = nv.CMND,
                           DiaChi = nv.DiaChi,
                           Sdt = (int?)nv.Sdt ?? default,
                           SoThe = nv.SoThe,
                           NgayCap = (DateTime?)nv.NgayCap ?? default,
                           HanSuDung = (DateTime?)nv.HanSuDung ?? default,
                           ChucVuID = (int?)nv.ChucVuID ?? default,
                           TTLV = (int)nv.TTLV
                       }).ToList();
            if(IDNT != 0)
            {
                res = res.Where(x=>x.IDNT == IDNT).ToList();
            }
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.Where(x => x.SoThe != "").OrderBy(x => x.HanSuDung).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            List<NT_TTHD> hd = db_nt.NT_TTHD.ToList();
            ViewBag.IDHD = new SelectList(hd, "IDHD", "TenHD");

            List<NT_Partner> dt = db.NT_Partner.ToList();
            ViewBag.ID = new SelectList(dt, "ID", "FullName");

            List<NT_Position> cv = db.NT_Position.ToList();
            ViewBag.IDCV = new SelectList(cv, "IDCV", "ChuVuTQ");
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(ContractorStaffValidation _DO)
        {
            try
            {
                var Check = db_nt.NT_NhanVienNT.Where(x=>x.CCCD == _DO.CCCD).FirstOrDefault();
                if(Check == null)
                {
                    db_nt.NT_NhanVienNT_insert(_DO.HoTen, _DO.CCCD,_DO.CMND,null, null, _DO.SoThe, _DO.ID, _DO.NgayCap, _DO.HanSuDung,_DO.ChucVuID, _DO.IDHD);
                    TempData["msgError"] = "<script>alert('Thêm mới thành công');</script>";
                }    
                else
                {
                    TempData["msgError"] = "<script>alert('Nhân viên đã tồn tại');</script>";
                    return RedirectToAction("Index", "ContractorStaff");
                }    
             
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ContractorStaff");
        }


        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_DSNVNT.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_DSNVNT.xlsx");
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
            string TenNV = "";
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
                    int NhaThauID = 0;
                    int stt = 0;    
                  
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            EPORTALEntities db = new EPORTALEntities();

                            for (int i = 5; i < dt.Rows.Count; i++)
                            {
                                stt = i;
                                string CCCD = dt.Rows[i][2].ToString().Trim();
                                if(CCCD != "")
                                {
                                    HoTenNT = CCCD.ToString();
                                }    
                          
                                string CMND = dt.Rows[i][3].ToString();
                                if (CMND != "")
                                {
                                    HoTenNT = CMND.ToString();
                                }

                                var Ckeck = db_nt.NT_NhanVienNT.Where(x => x.CCCD == CCCD && x.CMND == CMND).FirstOrDefault();
                                var CheckVP = db_nt.NT_NhanVienVP.Where(x => x.CCCD == CCCD && x.TinhTrang == 0 ||  x.CMND == CMND && x.TinhTrang == 0).FirstOrDefault();
                                if (CheckVP != null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại vi phạm nhân viên có CCCD/CMND: " + HoTenNT + " tại dòng: " + i +"');</script>";
                                    return RedirectToAction("Index", "ContractorStaff");
                                }
                                if (Ckeck == null && CheckVP == null)
                                {
                                    string HoTen = dt.Rows[i][1].ToString();
                                    TenNV = HoTenNT.Trim();
                                  
                                    string DiaChi = dt.Rows[i][4].ToString();
                                    string Sdt = dt.Rows[i][5].ToString();
                                    string SoThe = dt.Rows[i][6].ToString();
                                    string NhaThau = dt.Rows[i][7].ToString();
                                    string NgayCap = dt.Rows[i][8].ToString();
                                    string HanSuDung = dt.Rows[i][9].ToString();
                                    string ChucVu = dt.Rows[i][10].ToString().Trim();
                                    var IDCV = db.NT_Position.Where(x=>x.TenCV == ChucVu).FirstOrDefault();
                                    if(IDCV == null)
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại chức vụ nhân viên có CCCD/CMND: " + HoTenNT + " tại dòng: " + i + "');</script>";
                                        return RedirectToAction("Index", "ContractorStaff");
                                    }    
                                    string TTHD = dt.Rows[i][11].ToString();
                                    var IDHD = db_nt.NT_TTHD.Where(x => x.TenHD == TTHD).FirstOrDefault();
                                    var IDNT = db.NT_Partner.Where(x => x.CodeUnis == NhaThau).FirstOrDefault();
                                    if (IDNT != null)
                                    {
                                        NhaThauID = IDNT.ID;
                                    }

                                    if (NgayCap == "" && HanSuDung != "")
                                    {
                                        DateTime HanSuDungConvert = DateTime.ParseExact(HanSuDung, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                        System.Data.Entity.Core.Objects.ObjectParameter returnIDNVNT = new System.Data.Entity.Core.Objects.ObjectParameter("IDNVNT", typeof(int));
                                        db_nt.NT_NhanVienNT_insertall(HoTen, CCCD, CMND,DiaChi, null, SoThe, NhaThauID, null, HanSuDungConvert, IDCV.IDCV, IDHD.IDHD, returnIDNVNT);
                                        int IDNVNT = Convert.ToInt32(returnIDNVNT.Value);
                                        var Insert = db_nt.NT_HoatDongNV_insert(IDNVNT, NhaThauID, null, HanSuDungConvert, null, null, null);

                                        //var a = db_nt.NT_NhanVienNT_insert(HoTen, CCCD, DiaChi, null, SoThe, IDNhaThau, null, HanSuDungConvert, IDHD.IDHD);
                                    }
                                    else if (HanSuDung == "" && NgayCap != "")
                                    {
                                        DateTime NgayCapConvert = DateTime.ParseExact(NgayCap, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        System.Data.Entity.Core.Objects.ObjectParameter returnIDNVNT = new System.Data.Entity.Core.Objects.ObjectParameter("IDNVNT", typeof(int));
                                        db_nt.NT_NhanVienNT_insertall(HoTen, CCCD, CMND, DiaChi, null, SoThe, NhaThauID, NgayCapConvert, null, IDCV.IDCV, IDHD.IDHD, returnIDNVNT);
                                        int IDNVNT = Convert.ToInt32(returnIDNVNT.Value);
                                        var Insert = db_nt.NT_HoatDongNV_insert(IDNVNT, NhaThauID, NgayCapConvert, null, null, null, null);

                                        //var a = db_nt.NT_NhanVienNT_insert(HoTen, CCCD, DiaChi, null, SoThe, IDNhaThau, NgayCapConvert, null, IDHD.IDHD);
                                    }
                                    else if (HanSuDung == "" && NgayCap == "")
                                    {
                                        System.Data.Entity.Core.Objects.ObjectParameter returnIDNVNT = new System.Data.Entity.Core.Objects.ObjectParameter("IDNVNT", typeof(int));
                                        db_nt.NT_NhanVienNT_insertall(HoTen, CCCD, CMND, DiaChi, null, SoThe, NhaThauID, null, null, IDCV.IDCV,IDHD.IDHD, returnIDNVNT);
                                        int IDNVNT = Convert.ToInt32(returnIDNVNT.Value);
                                        var Insert = db_nt.NT_HoatDongNV_insert(IDNVNT, NhaThauID, null, null, null, null, null);
                                        //var a = db_nt.NT_NhanVienNT_insert(HoTen, CCCD, DiaChi, null, SoThe, IDNhaThau, null, null, IDHD.IDHD);
                                    }
                                    else if (HanSuDung != "" && NgayCap != "")
                                    {
                                        DateTime NgayCapConvert = DateTime.ParseExact(NgayCap, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        DateTime HanSuDungConvert = DateTime.ParseExact(HanSuDung, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        System.Data.Entity.Core.Objects.ObjectParameter returnIDNVNT = new System.Data.Entity.Core.Objects.ObjectParameter("IDNVNT", typeof(int));
                                        db_nt.NT_NhanVienNT_insertall(HoTen, CCCD, CMND, null, null, SoThe, NhaThauID, NgayCapConvert, HanSuDungConvert, IDCV.IDCV, IDHD.IDHD, returnIDNVNT);
                                        int IDNVNT = Convert.ToInt32(returnIDNVNT.Value);
                                        var Insert = db_nt.NT_HoatDongNV_insert(IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, null);
                                    }
                                    imp++;
                                }
                                else if(Ckeck != null && CheckVP == null)
                                {
                                    string HoTen = dt.Rows[i][1].ToString();
                                    TenNV = HoTenNT.Trim();
                                 
                                    string DiaChi = dt.Rows[i][4].ToString();
                                    string Sdt = dt.Rows[i][5].ToString();
                                    string SoThe = dt.Rows[i][6].ToString();
                                    string NhaThau = dt.Rows[i][7].ToString();
                                    string NgayCap = dt.Rows[i][8].ToString();
                                    string HanSuDung = dt.Rows[i][9].ToString();
                                    string ChucVu = dt.Rows[i][10].ToString().Trim();
                                    var IDCV = db.NT_Position.Where(x => x.TenCV == ChucVu).FirstOrDefault();
                                    if (IDCV == null)
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại chức vụ nhân viên có CCCD/CMND: " + HoTenNT + " dòng: "+ i +"');</script>";
                                        return RedirectToAction("Index", "ContractorStaff");
                                    }
                                    string TTHD = dt.Rows[i][11].ToString();
                                    var IDHD = db_nt.NT_TTHD.Where(x => x.TenHD == TTHD).FirstOrDefault();
                                    var IDNT = db.NT_Partner.Where(x => x.CodeUnis == NhaThau).FirstOrDefault();
                                    if (IDNT != null)
                                    {
                                        NhaThauID = IDNT.ID;
                                    }

                                    if (NgayCap == "" && HanSuDung != "")
                                    {
                                        DateTime HanSuDungConvert = DateTime.ParseExact(HanSuDung, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        var a = db_nt.NT_NhanVienNT_update(Ckeck.IDNVNT, HoTen, CCCD, CMND, DiaChi, null, SoThe, NhaThauID, null, HanSuDungConvert, IDCV.IDCV, IDHD.IDHD);
                                    }
                                    else if (HanSuDung == "" && NgayCap != "")
                                    {
                                        DateTime NgayCapConvert = DateTime.ParseExact(NgayCap, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        var a = db_nt.NT_NhanVienNT_update(Ckeck.IDNVNT, HoTen, CCCD, CMND, DiaChi, null, SoThe, NhaThauID, NgayCapConvert, null, IDCV.IDCV, IDHD.IDHD);
                                    }
                                    else if (HanSuDung == "" && NgayCap == "")
                                    {

                                        var a = db_nt.NT_NhanVienNT_update(Ckeck.IDNVNT, HoTen, CCCD, CMND, DiaChi, null, SoThe, NhaThauID, null, null, IDCV.IDCV, IDHD.IDHD);
                                    }
                                    else if (HanSuDung != "" && NgayCap != "")
                                    {
                                        DateTime NgayCapConvert = DateTime.ParseExact(NgayCap, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        DateTime HanSuDungConvert = DateTime.ParseExact(HanSuDung, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        var a = db_nt.NT_NhanVienNT_update(Ckeck.IDNVNT, HoTen, CCCD, CMND, DiaChi, null, SoThe, NhaThauID, NgayCapConvert, HanSuDungConvert, IDCV.IDCV,IDHD.IDHD);

                                        if (IDNT != null)
                                        {
                                            if (Ckeck.IDNT != IDNT.ID && Ckeck.NgayCap != NgayCapConvert && Ckeck.HanSuDung != HanSuDungConvert)
                                            {
                                                var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, null);
                                            }
                                            else if (Ckeck.IDNT != IDNT.ID)
                                            {
                                                string Ghichu = "Chuyển nhà thầu";
                                                var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);
                                            }
                                            else if (Ckeck.NgayCap != NgayCapConvert && Ckeck.HanSuDung != HanSuDungConvert)
                                            {
                                                string Ghichu = "Gia hạn thẻ";
                                                var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);

                                            }
                                            else if (Ckeck.NgayCap == NgayCapConvert && Ckeck.HanSuDung != HanSuDungConvert)
                                            {
                                                string Ghichu = "Gia hạn thẻ";
                                                var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);

                                            }
                                            else if (Ckeck.TTLV != IDHD.IDHD)
                                            {
                                                if (IDHD.IDHD == 1)
                                                {
                                                    string Ghichu = "Mở thẻ";
                                                    var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);
                                                }
                                                else if (IDHD.IDHD == 2)
                                                {
                                                    string Ghichu = "Khóa thẻ";
                                                    var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);
                                                }
                                                else if (IDHD.IDHD == 3)
                                                {
                                                    string Ghichu = "Tạm khóa thẻ";
                                                    var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);
                                                }
                                                else if (IDHD.IDHD == 4)
                                                {
                                                    string Ghichu = "Hết hạn";
                                                    var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, NhaThauID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);
                                                }
                            
                                                //var Insert = db_nt.NT_HoatDongNV_insert(Ckeck.IDNVNT, IDNT.ID, NgayCapConvert, HanSuDungConvert, null, null, Ghichu);

                                            }
                                        }    

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
                            TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại nhân viên có CCCD/CMND: " + HoTenNT + " dòng: " + stt + "');</script>";
                            //return RedirectToAction("Index", "ContractorStaff");
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

            //TempData["msgSuccess"] = "<script>alert( Vui lòng kiểm nhân viên : " + HoTenNT + "');</script>";

            return RedirectToAction("Index", "ContractorStaff");
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
                db_nt.NT_NhanVienNT_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ContractorStaff");
        }

        public ActionResult Lock(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from nv in db_nt.NT_NhanVienNT.Where(x=>x.IDNVNT == id)
                       select new ContractorStaffValidation
                       {
                           IDNVNT = nv.IDNVNT,
                           IDNT = (int)nv.IDNT,
                           HoTen = nv.HoTen,
                           CCCD = nv.CCCD,
                           DiaChi = nv.DiaChi,
                           Sdt = (int?)nv.Sdt ?? default,
                           SoThe = nv.SoThe,
                           NgayCap = (DateTime?)nv.NgayCap ?? default,
                           HanSuDung = (DateTime?)nv.HanSuDung ?? default,
                           TTLV = (int)nv.TTLV

                       }).ToList();
            ContractorStaffValidation DO = new ContractorStaffValidation();
            if (res.Count > 0)
            {
                foreach (var nv in res)
                {
                    DO.IDNVNT = nv.IDNVNT;
                    DO.IDNT = (int)nv.IDNT;
                    DO.HoTen = nv.HoTen;
                    DO.CCCD = nv.CCCD;
                    DO.DiaChi = nv.DiaChi;
                    DO.Sdt = (int?)nv.Sdt ?? default;
                    DO.SoThe = nv.SoThe;
                    DO.NgayCap = (DateTime?)nv.NgayCap ?? default;
                    DO.HanSuDung = (DateTime?)nv.HanSuDung ?? default;
                    DO.TTLV = (int)nv.TTLV;
                }
                List<NT_TTHD> hd = db_nt.NT_TTHD.ToList();
                ViewBag.TTLV = new SelectList(hd, "IDHD", "TenHD", DO.TTLV);

                ViewBag.NgayCap = DO.NgayCap.ToString("yyyy-MM-dd");
                ViewBag.HanSuDung = DO.HanSuDung.ToString("yyyy-MM-dd");
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
     
        }
        [HttpPost]
        public ActionResult Lock(ContractorStaffValidation _DO)
        {
            try
            {  
                var Check = db_nt.NT_NhanVienNT.Where(x=>x.IDNVNT == _DO.IDNVNT).FirstOrDefault();
                db_nt.NT_NhanVienNT_lock(_DO.IDNVNT, _DO.TTLV);
                var TinhTrang = db_nt.NT_TTHD.Where(x=>x.IDHD == _DO.TTLV).FirstOrDefault();
                var Insert = db_nt.NT_HoatDongNV_insert(Check.IDNVNT, Check.IDNT, _DO.NgayCap, _DO.HanSuDung, null, null, TinhTrang.TenHD);
                TempData["msgSuccess"] = "<script>alert('Cập nhật tình trạng thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ContractorStaff");
        }
        public ActionResult UpdateDay(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from nv in db_nt.NT_NhanVienNT.Where(x => x.IDNVNT == id)
                       select new ContractorStaffValidation
                       {
                           IDNVNT = nv.IDNVNT,
                           IDNT = (int)nv.IDNT,
                           HoTen = nv.HoTen,
                           CCCD = nv.CCCD,
                           DiaChi = nv.DiaChi,
                           Sdt = (int?)nv.Sdt ?? default,
                           SoThe = nv.SoThe,
                           NgayCap = (DateTime?)nv.NgayCap ?? default,
                           HanSuDung = (DateTime?)nv.HanSuDung ?? default,
                           TTLV = (int)nv.TTLV

                       }).ToList();
            ContractorStaffValidation DO = new ContractorStaffValidation();
            if (res.Count > 0)
            {
                foreach (var nv in res)
                {
                    DO.IDNVNT = nv.IDNVNT;
                    DO.IDNT = (int)nv.IDNT;
                    DO.HoTen = nv.HoTen;
                    DO.CCCD = nv.CCCD;
                    DO.DiaChi = nv.DiaChi;
                    DO.Sdt = (int?)nv.Sdt ?? default;
                    DO.SoThe = nv.SoThe;
                    DO.NgayCap = (DateTime?)nv.NgayCap ?? default;
                    DO.HanSuDung = (DateTime?)nv.HanSuDung ?? default;
                    DO.TTLV = (int)nv.TTLV;
                }
            }
            else
            {
                HttpNotFound();
            }
            List<NT_Partner> bp = db.NT_Partner.ToList();
            int index = 0;
            foreach (var n in bp)
            {
                bp[index].FullName = n.BPID.ToString() + " - " + n.FullName;
                index++;
            }
            ViewBag.NgayCap = DO.NgayCap.ToString("yyyy-MM-dd");
            ViewBag.HanSuDung = DO.HanSuDung.ToString("yyyy-MM-dd");

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.ContractorsID = new SelectList(nt, "ID", "FullName", DO.IDNT);

            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult UpdateDay(ContractorStaffValidation _DO)
        {
            var DayNow = DateTime.Now.ToString("dd-MM-yyyy");
            string GiaHan = "Gia hạn thẻ ngày : ";
            try
            {
                var Check = db_nt.NT_NhanVienNT.Where(x => x.IDNVNT == _DO.IDNVNT).FirstOrDefault();
                db_nt.NT_NhanVienNT_updateday(_DO.IDNVNT, _DO.NgayCap, _DO.HanSuDung);


                TempData["msgError"] = "<script>alert('Gia hạn thẻ thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ContractorStaff");
        }
        public ActionResult ViewNVNT(int? page, DateTime? begind, DateTime? endd, int? MaNhaThau, int? MaNhanVien)
        {
            //List<NT_Partner> nt = db.NT_Partner.Where(x => x.CodeUnis != "").ToList();

            //ViewBag.NTList = new SelectList(nt, "CodeUnis", "FullName", MaNhaThau);
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var NhaThau = (from nt in db.NT_Partner.Where(x => x.CodeUnis != "")
                           select new NT_PartnerValidation
                           {
                               ID = (int)nt.ID,
                               CodeUnis = nt.CodeUnis,
                               FullName = nt.CodeUnis + " : " + nt.ShortName
                           }).ToList();
            if (MaNhaThau == null) MaNhaThau = 0;
            ViewBag.NTList = new SelectList(NhaThau, "CodeUnis", "FullName");

            var NhanVien = (from nt in db_nt.NT_NhanVienNT
                            select new ContractorStaffValidation
                            {
                                IDNT = (int)nt.IDNVNT,
                                SoThe = nt.SoThe,
                                HoTen = nt.HoTen + " : " + nt.SoThe
                            }).ToList();
            if (MaNhanVien == null) MaNhanVien = 0;
            ViewBag.NVList = new SelectList(NhanVien, "SoThe", "HoTen");


            string begindcover = "";
            string enddcover = "";
            if (begind == null && endd == null)
            {
                begindcover = DateTime.Now.Date.ToString("yyyyMMdd");
                enddcover = DateTime.Now.Date.ToString("yyyyMMdd");
            }
            else
            {
                string cover = begind.ToString();
                begindcover = DateTime.Parse(cover).ToString("yyyyMMdd");
                string cover1 = endd.ToString();
                enddcover = DateTime.Parse(cover1).ToString("yyyyMMdd");
            }


            string sql = "SELECT te.C_Date,te.C_Time,te.L_TID,te.L_UID,te.C_Name,te.C_Unique,te.C_Post,te.C_Card, tt.C_Name as C_Name1, tu.C_REGDATE , tu.L_OPTDATELIMIT, tu.C_DATELIMIT" +
                        " FROM tEnter te, TTERMINAL tt, TUSER tu " +
                        " where te.L_TID=tt.L_ID AND tu.l_ID=te.L_UID AND te.C_Date >= '" + begindcover + "' AND te.C_Date <= '" + enddcover + "' ";

            OracleConnection con = new OracleConnection(connStr);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = sql;
            cmd.Connection = con;
            con.Open();
            OracleDataReader dr = cmd.ExecuteReader();
            List<NhanvienNT> listNT = new List<NhanvienNT>();
            if (dr.HasRows)
            {
                NhanvienNT NhanvienNT = null;
                while (dr.Read())
                {
                    NhanvienNT = new NhanvienNT();
                    NhanvienNT.Mathe = dr["C_Card"].ToString();
                    NhanvienNT.Hoten = dr["C_Name"].ToString();
                    NhanvienNT.MaNT = dr["C_Post"].ToString();
                    NhanvienNT.Thoigianquyet = dr["C_Date"].ToString() + dr["C_Time"].ToString();
                    NhanvienNT.Tenmayquyet = dr["C_Name1"].ToString();
                    //NhanvienNT.Ngaydangky = dr["C_REGDATE"].ToString();
                    //NhanvienNT.NgayHetHan = dr["C_DATELIMIT"].ToString();
                    listNT.Add(NhanvienNT);
                }
            }

            else
            {
                Response.Write("No Data In DataBase");
            }
            con.Close();
            var itemIds = db_nt.NT_NhanVienNT.Select(x => x.SoThe).ToArray();
            if (MaNhaThau == 0 && MaNhanVien == 0)
            {
                listNT = listNT.Where(x => itemIds.Contains(x.Mathe)).ToList();
            }
            else if (MaNhaThau != 0 && MaNhanVien == 0)
            {
                var MaNT = MaNhaThau.ToString();
                listNT = listNT.Where(x => itemIds.Contains(x.Mathe) && x.MaNT == MaNT).ToList();
            }
            else if (MaNhaThau == 0 && MaNhanVien != 0)
            {
                var NVNT = MaNhanVien.ToString();

                listNT = listNT.Where(x => itemIds.Contains(x.Mathe) && x.Mathe == NVNT).ToList();
            }
            else if (MaNhaThau != 0 && MaNhanVien != 0)
            {
                var NVNT = MaNhanVien.ToString();
                var MaNT = MaNhaThau.ToString();
                listNT = listNT.Where(x => itemIds.Contains(x.Mathe) && x.Mathe == NVNT && x.MaNT == MaNT).ToList();
            }
            if (page == null) page = 1;
            int pageSize = 150;
            int pageNumber = (page ?? 1);
            return View(listNT.OrderByDescending(x => x.Thoigianquyet).ToList().ToPagedList(pageNumber, pageSize));

        }
        private List<NhanvienNT> ConvertnhanvienNT(DataTable dt)
        {
            List<NhanvienNT> NVNT = new List<NhanvienNT>();
            NhanvienNT NhanvienNT = null;
            foreach (DataRow row in dt.Rows)
            {
                NhanvienNT = new NhanvienNT();
                NhanvienNT.Mathe = row["C_Card"].ToString();
                NhanvienNT.Hoten = row["C_Name"].ToString();
                NhanvienNT.MaNT = row["C_Post"].ToString();
                NhanvienNT.Thoigianquyet = row["C_Date"].ToString() + row["C_Time"].ToString();
                NhanvienNT.Tenmayquyet = row["C_Name1"].ToString();
                NhanvienNT.Ngaydangky = row["C_REGDATE"].ToString();
                NhanvienNT.NgayHetHan = row["C_DATELIMIT"].ToString();
                NVNT.Add(NhanvienNT);
            }
            return NVNT;
        }
        public ActionResult GetUnis()
        {
            //List<string> listnt = new List<string>();
            //string now = "";
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            string sql = "SELECT tu.L_ID, tu.C_RegDate, tu.C_DateLimit, tu.L_Type ,iu.L_UID, iu.C_CardNum" +
                       " FROM tUser tu, iUserCard iu " +
                       " where tu.L_ID = iu.L_UID";
            OracleConnection con = new OracleConnection(connStr);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = sql;
            cmd.Connection = con;
            con.Open();
            OracleDataReader dr = cmd.ExecuteReader();
            List<ThoiHanTheNT> TNT = new List<ThoiHanTheNT>();
            ThoiHanTheNT TheNT = null;
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    TheNT = new ThoiHanTheNT();
                    TheNT.Sothe = dr["C_CardNum"].ToString();
                    TheNT.Ngaydangky = dr["C_RegDate"].ToString();
                    TheNT.Ngayhethan = dr["C_DateLimit"].ToString();
                    TheNT.L_Type = Convert.ToInt32(dr["L_Type"]);
                    TNT.Add(TheNT);
                }
            }
            else
            {
                Response.Write("No Data In DataBase");
            }
            con.Close();
            con.Dispose();

            int number = 0;
            var itemIds = db_nt.NT_NhanVienNT.Select(x => x.SoThe).ToArray();

            //DataTable tabs = new DataTable();
            //OracleDataAdapter readers = new OracleDataAdapter(sql, connStr);
            //readers.Fill(tabs);
            //List<ThoiHanTheNT> listTNT = new List<ThoiHanTheNT>();
            //listTNT = ConvertUnis(tabs);
            TNT = TNT.Where(x => itemIds.Contains(x.Sothe)).ToList();

            foreach (var item in TNT)
            {
                var IDNT = db_nt.NT_NhanVienNT.Where(x => x.SoThe == item.Sothe).FirstOrDefault();
                if (IDNT != null)
                {
                    if (item.Ngayhethan != "")
                    {
                        var yyyy = item.Ngaydangky.Substring(0, 4);
                        var MM = item.Ngaydangky.Substring(4, 2);
                        var dd = item.Ngaydangky.Substring(6, 2);
                        var NgayDK = dd + "-" + MM + "-" + yyyy;
                        DateTime Ngaydangky = DateTime.ParseExact(NgayDK, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        var yyyy1 = item.Ngayhethan.Substring(0, 4);
                        var MM1 = item.Ngayhethan.Substring(4, 2);
                        var dd1 = item.Ngayhethan.Substring(6, 2);
                        var NgayHT = dd + "-" + MM + "-" + yyyy;
                        DateTime Ngayhethan = DateTime.ParseExact(NgayHT, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        db_nt.NT_NhanVienNT_updateday(IDNT.IDNVNT, Ngaydangky, Ngayhethan);
                    }
                    else
                    {
                        var yyyy = item.Ngaydangky.Substring(0, 4);
                        var MM = item.Ngaydangky.Substring(4, 2);
                        var dd = item.Ngaydangky.Substring(6, 2);
                        var NgayDK = dd + "-" + MM + "-" + yyyy;
                        DateTime Ngaydangky = DateTime.ParseExact(NgayDK, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        db_nt.NT_NhanVienNT_updateday(IDNT.IDNVNT, Ngaydangky, null);
                    }
                    number++;
                }

            }

            TempData["msgError"] = "<script>alert('Update được : " + number + " dòng dữ liệu');</script>";

            return RedirectToAction("Index", "ContractorStaff", new { area = "Partner" });

        }
        private List<ThoiHanTheNT> ConvertUnis(DataTable dt)
        {
            List<ThoiHanTheNT> TNT = new List<ThoiHanTheNT>();
            ThoiHanTheNT TheNT = null;
            foreach (DataRow row in dt.Rows)
            {
                TheNT = new ThoiHanTheNT();
                TheNT.Sothe = row["C_CardNum"].ToString();
                TheNT.Ngaydangky = row["C_RegDate"].ToString();
                TheNT.Ngayhethan = row["C_DateLimit"].ToString();
                TheNT.L_Type = Convert.ToInt32(row["L_Type"]);
                TNT.Add(TheNT);
            }
            return TNT;
        }
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();
            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        public ActionResult ExportToExcel()
        {
            try
            {

                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNVNT.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSNVNT_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("NV");
                var List = db_nt.NT_NhanVienNT.ToList();
                if (List.Count > 0)
                {
                    int row = 6, rowlast = 1, stt = 0;
                    foreach (var item in List)
                    {

                        row++; stt++;
                        rowlast = row + 1;

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.HoTen;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("C" + row).Value = "'"+item.CCCD.ToString();
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.DiaChi;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;


                        Worksheet.Cell("E" + row).Value = item.Sdt;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;



                        Worksheet.Cell("F" + row).Value = "'" + item.SoThe;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        var NhaThau = db.NT_Partner.Where(x => x.ID == item.IDNT).FirstOrDefault();
                        Worksheet.Cell("G" + row).Value = NhaThau.FullName;
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("H" + row).Value = item.NgayCap;
                        Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("H" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("I" + row).Value = item.HanSuDung;
                        Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("I" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                        var TTHD = db_nt.NT_TTHD.Where(x => x.IDHD == item.TTLV).FirstOrDefault();
                        Worksheet.Cell("J" + row).Value = TTHD.TenHD;
                        Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;
                        row = rowlast - 1;
                    }
                    Worksheet.Range("A7:J" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:J" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A7:J" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:J" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "Danh sách NVNT.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/ListPartner'</script>";
                    return RedirectToAction("Index", "ContractorStaff");
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
                return RedirectToAction("Index", "ContractorStaff");
            }

        }

        public ActionResult History()
        {
            int dtc = 0;
            try
            {
                var IDNVNT = db_nt.NT_NhanVienNT.ToList();
                foreach (var nt in IDNVNT)
                {
                    var IDHD = db_nt.NT_HoatDongNV.Where(x => x.IDNVNT == nt.IDNVNT).Count();
                    if (IDHD == 0)
                    {
                        var Insert = db_nt.NT_HoatDongNV_insert(nt.IDNVNT, nt.IDNT, nt.NgayCap, nt.HanSuDung, null, null, null);
                        dtc++;
                    }

                }
                TempData["msgSuccess"] = "<script>alert('Cập nhật được : " + dtc + " nhân viên ');</script>";

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/ListPartner'</script>";
              
            }
            return RedirectToAction("Index", "ContractorStaff");
        }
    }
}