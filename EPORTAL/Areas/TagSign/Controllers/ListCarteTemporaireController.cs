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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Interop;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ListCarteTemporaireController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListCarteTemporaire";
        // GET: TagSign/ListCarteTemporaire
        public ActionResult Index(int? page, int? search, DateTime? begind, DateTime? endd)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            List<NT_CarteTemporaireValidation> data = new List<NT_CarteTemporaireValidation>();
            if (search == null && begind == null && endd == null)
            {
                data = (from a in db_nt.NT_CarteTemporaire.Where(x => x.NguoiTaoID == Models.MyAuthentication.ID)
                        select new NT_CarteTemporaireValidation
                        {
                            IDTTT = (int)a.IDTTT,
                            NoiDung = a.NoiDung,
                            NguoiTaoID = (int?)a.NguoiTaoID ?? default,
                            NgayTao = (DateTime?)a.NgayTao ?? default,
                            TinhTrang = (int?)a.TinhTrang ?? default,
                            NTID = (int?)a.NTID ?? default,
                            PhongBanID = (int?)a.PhongBanID ?? default,
                            HangMuc = a.HangMuc,
                            ThoiHan = (DateTime)a.ThoiHan,
                            kyTruoc = a.KyTruoc,
                            GhiChu = a.GhiChu,
                            isNT = a.isNT,
                            FileHosoDK = a.FileHosoDK,
                        }).ToList();
            }
            else
            {
                data = db_nt.NT_CarteTemporaire_search(begind, endd, search).Where(x => x.NguoiTaoID == Models.MyAuthentication.ID).Select(x => new NT_CarteTemporaireValidation()
                {
                    IDTTT = (int)x.IDTTT,
                    NoiDung = x.NoiDung,
                    NguoiTaoID = (int?)x.NguoiTaoID ?? default,
                    NgayTao = (DateTime?)x.NgayTao ?? default,
                    TinhTrang = (int?)x.TinhTrang ?? default,
                    NTID = (int?)x.NTID ?? default,
                    PhongBanID = (int?)x.PhongBanID ?? default,
                    HangMuc = x.HangMuc,
                    ThoiHan = (DateTime)x.ThoiHan,
                    kyTruoc = x.KyTruoc,
                    GhiChu = x.GhiChu,
                    isNT = x.isNT,
                    FileHosoDK = x.FileHosoDK,
                }).ToList();
            }          
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName");
            return View(data.OrderByDescending(x => x.NgayTao).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Delete(int? id)
        {
           
            try
            {
                var delete_hande=db_nt.NT_Handle.Where(x=>x.IDTTT==id).ToList();
                if(delete_hande.Count > 0)
                {
                    db_nt.NT_handle_Delete_byIDTTT(id);
                }
                var delete_detail = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDTTT == id).ToList();
                if(delete_detail.Count > 0)
                {
                    db_nt.NT_DetailCarteTemporaire_delete_ByIDTTT(id);
                }
                string CarteTemporaire = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == id).Select(x => x.FileHosoDK).SingleOrDefault();
                if (CarteTemporaire != "")
                {
                    string path = Server.MapPath("~/UploadedFiles/UploadedFiles/");
                    string filePath = path + CarteTemporaire;
                    System.IO.File.Delete(filePath);
                }
                db_nt.NT_CarteTemporaire_delete(id);
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListCarteTemporaire");
        }
        public FileResult DownloadHoSo(int id)
        {
            var cartemp = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == id).SingleOrDefault();
            string path = Server.MapPath("~/UploadedFiles/UploadedFiles/");
            string filepath = path + cartemp.FileHosoDK;
            string contentType = MimeMapping.GetMimeMapping(filepath);
            string fileName = cartemp.FileHosoDK.ToString().Substring(cartemp.FileHosoDK.Split('_')[0].ToString().Length + 1);
            return File(filepath, contentType, fileName);
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_Don dang ky cap the tam NT.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_Don dang ky cap the tam NT.xlsx");
        }
        public ActionResult Add()
        {
           
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName");

            List<NT_Contract> hd = new List<NT_Contract>();
            ViewBag.IDHD = new SelectList(hd, "IDContract", "Somecontracts");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            List<LoaiPT> pTs = db_nt.LoaiPTs.ToList();
            ViewBag.IDPT = new SelectList(pTs, "LoaiPTID", "TenPT");

            var IDPhongBan = db.PhongBans.Where(x => x.IDPhongBan == Models.MyAuthentication.IDPhongban).FirstOrDefault();
            if (IDPhongBan != null)
            {
                List<PhongBan> pb = db.PhongBans.ToList();
                ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", IDPhongBan.IDPhongBan);
            }
            else
            {
                List<PhongBan> pb = db.PhongBans.ToList();
                ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan");
            }

           

            return View();
        }
        public string upLoadfile(HttpPostedFileBase file)
        {

            string filePath = string.Empty;
            string path = Server.MapPath("~/UploadedFiles/UploadedFiles/");
            string unixTimestamp = Convert.ToString((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            filePath = path + unixTimestamp + "_" + Path.GetFileName(file.FileName);

            file.SaveAs(filePath);
            return unixTimestamp + "_" + Path.GetFileName(file.FileName);
        }
        [HttpPost]
        public ActionResult Add(NT_DetailCarteTemporaireValidation _DO, FormCollection collection)
        {
         
            int TTTID = 0;
            int TTThe = 0;
            int dtc = 1;
            HttpPostedFileBase file_1 = Request.Files["FileHosoDK"];
            string filePath_1 = string.Empty;
            try
            {
                var ListCT = new List<NT_DetailCarteTemporaireValidation>();
                DateTime DayUpload = DateTime.Now;
                foreach (var key in collection.AllKeys)
                {
                    if (key != "__RequestVerificationToken")
                    {
                        if (key == "HangMuc")
                        {
                            DateTime ThoiHan = DateTime.ParseExact(collection["ThoiHan"], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            DateTime CheckDay = DateTime.ParseExact(DayUpload.ToString("yyyy-MM-dd"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            if (ThoiHan < CheckDay)
                            {
                                TempData["msgError"] = "<script>alert('Vui lòng chọn lại thời hạn thẻ');</script>";
                                return RedirectToAction("Add", "ListCarteTemporaire");
                            }
                            if (collection["XacNhan_1"] == "Xác Nhận")
                            {
                                if (collection["NoiDung"] == "")
                                {
                                    TempData["msgError"] = "<script>alert('Vui lòng nhập nội dung');</script>";
                                    return RedirectToAction("Add", "ListCarteTemporaire");
                                }
                                if (collection["HangMuc"] == "")
                                {
                                    TempData["msgError"] = "<script>alert('Vui lòng nhập hạng mục');</script>";
                                    return RedirectToAction("Add", "ListCarteTemporaire");
                                }
                                if ((file_1 != null) && (file_1.ContentLength > 0) && !string.IsNullOrEmpty(file_1.FileName))
                                {
                                    filePath_1 = upLoadfile(file_1);
                                }
                                System.Data.Entity.Core.Objects.ObjectParameter returnIDTTT = new System.Data.Entity.Core.Objects.ObjectParameter("IDTTT", typeof(int));
                                db_nt.NT_CarteTemporaire_insert(collection["NoiDung"], Models.MyAuthentication.ID, DayUpload, 0, Convert.ToInt32(collection["IDNT"]), Convert.ToInt32(collection["IDPhongBan"]), collection["HangMuc"], ThoiHan, 0, 0, filePath_1, returnIDTTT);
                                int IDTTT = Convert.ToInt32(returnIDTTT.Value);
                                TTTID = IDTTT;
                                TTThe = 1;
                            }

                        }
                        if (key == "Imoprt")
                        {
                            if (collection["NoiDung"] == "")
                            {
                                TempData["msgError"] = "<script>alert('Vui lòng nhập nội dung');</script>";
                                return RedirectToAction("Add", "ListCarteTemporaire");
                            }
                            if (collection["HangMuc"] == "")
                            {
                                TempData["msgError"] = "<script>alert('Vui lòng nhập hạng mục');</script>";
                                return RedirectToAction("Add", "ListCarteTemporaire");
                            }
                            DateTime ThoiHan = DateTime.ParseExact(collection["ThoiHan"], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            System.Data.Entity.Core.Objects.ObjectParameter returnIDTTT = new System.Data.Entity.Core.Objects.ObjectParameter("IDTTT", typeof(int));
                            if ((file_1 != null) && (file_1.ContentLength > 0) && !string.IsNullOrEmpty(file_1.FileName))
                            {
                                filePath_1 = upLoadfile(file_1);
                            }
                            db_nt.NT_CarteTemporaire_insert(collection["NoiDung"], Models.MyAuthentication.ID, DayUpload, 0, Convert.ToInt32(collection["IDNT"]), Convert.ToInt32(collection["IDPhongBan"]), collection["HangMuc"], ThoiHan, 0, 0, filePath_1, returnIDTTT);
                            int IDTTT = Convert.ToInt32(returnIDTTT.Value);
                            TTTID = IDTTT;

                        }
                        if (key.Split('_')[0] == "Ghichu")
                        {
                           

                                ListCT.Add(new NT_DetailCarteTemporaireValidation()
                                {
                                    HoVaTen = collection["HoVaTen_" + key.Split('_')[1]],
                                    CCCD = collection["CCCD_" + key.Split('_')[1]],
                                    NgaySinh = (DateTime)((collection["NgaySinh_" + key.Split('_')[1]] == "") ? default : DateTime.ParseExact(collection["NgaySinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None)),
                                    //MucDich = Convert.ToInt32(collection["IDMD_" + key.Split('_')[1]]),
                                    ChucVu = collection["ChucVu_" + key.Split('_')[1]],
                                    LoaiPTID = Convert.ToInt32(collection["LoaiPTID_" + key.Split('_')[1]]),
                                    Sdt = collection["sdt_" + key.Split('_')[1]],
                                    BienSo = collection["BienSo_" + key.Split('_')[1]],
                                    Cong = Convert.ToInt32(collection["IDGate_" + key.Split('_')[1]]),
                                    SoPhieuDKVTTS = collection["SoPhieuDKVTTS_" + key.Split('_')[1]],
                                    VTTSChuaDK = collection["VTTheoPVCC_" + key.Split('_')[1]],
                                    VTTheoPVCC = collection["ChucVu_" + key.Split('_')[1]],
                                    SoLanRaVao = collection["SoLanRaVao_" + key.Split('_')[1]],
                                    GhiChu = collection["Ghichu_" + key.Split('_')[1]]
                                });                         
                        }
                    }

                }
                string msg = "";
                if (collection.Count == 7)
                {
                    string filePath = string.Empty;
                    HttpPostedFileBase file = Request.Files["FileExcel"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string path = Server.MapPath("~/UploadedFiles/DKHocATEX/");
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
                        if (dt.Rows.Count > 6)
                        {
                            try
                            {
                                for (int i = 6; i < dt.Rows.Count; i++)
                                {
                                    string HoVaTen = dt.Rows[i][1].ToString().Trim();

                                    if (HoVaTen != "")
                                    {
                                        string CCCD = dt.Rows[i][2].ToString().Trim();
                                        var CheckCCCD = db_nt.NT_NhanVienVP.Where(x => x.CCCD == CCCD).FirstOrDefault();
                                        if (CCCD.Length < 9 || CCCD.Length > 14 || CheckCCCD != null)
                                        {
                                            if (TTTID != 0)
                                            {
                                                db_nt.NT_DetailCarteTemporaire_delete(TTTID);
                                                //db_nt.NT_CarteTemporaire_delete(TTTID);
                                                TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin và import lại từ dòng : " + dtc + "');</script>";
                                            }
                                            if (CheckCCCD != null)
                                            {
                                                TempData["msgError"] = "<script>alert('Nhân viên nằm trong danh sách vi phạm. Tại dòng : " + dtc + "');</script>";
                                               
                                            }
                                            return RedirectToAction("Index", "Detail_ListCarteTemporaire", new { id = TTTID });
                                        }
                                        string NgaySinh = dt.Rows[i][3].ToString().Trim();
                                        if (NgaySinh == "")
                                        {
                                            TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin và import lại từ dòng : " + dtc + "');</script>";
                                            return RedirectToAction("Index", "Detail_ListCarteTemporaire", new { id = TTTID });
                                        }
                                        DateTime BirthDayConvert = DateTime.ParseExact(NgaySinh, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        
                                        string chucvu = dt.Rows[i][4].ToString().Trim();
                                        string Sdt = dt.Rows[i][5].ToString().Trim();
                                        string LoaiPT = dt.Rows[i][6].ToString().Trim();
                                        int idpt = db_nt.LoaiPTs.Where(x => x.TenPT == LoaiPT).Select(x => x.LoaiPTID).FirstOrDefault();
                                        string BienSo = dt.Rows[i][7].ToString().Trim();
                                        string Cong = dt.Rows[i][8].ToString().Trim();
                                        string SoPhieuDKVTTS = dt.Rows[i][9].ToString().Trim();
                                        string VTTSChuaDK = dt.Rows[i][10].ToString().Trim();
                                        string VTTheoPVCC = dt.Rows[i][11].ToString().Trim();
                                        string SoLanRaVao = dt.Rows[i][12].ToString().Trim();
                                        if (Cong == "")
                                        {
                                            TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin và import lại từ dòng : " + dtc + "');</script>";
                                            return RedirectToAction("Index", "Detail_ListCarteTemporaire", new { id = TTTID });
                                        }
                                        var IDCong = db.NT_Gate.Where(x => x.Gate == Cong).FirstOrDefault();
                                        string GhiChu = dt.Rows[i][13].ToString().Trim();
                                        var insert = db_nt.NT_DetailCarteTemporaire_Create
                                          (HoVaTen,
                                           CCCD,
                                           BirthDayConvert,
                                           Sdt,
                                           BienSo,
                                           Convert.ToInt32(IDCong.IDGate),
                                           GhiChu,
                                           TTTID,
                                           chucvu,
                                           Convert.ToInt32(idpt),
                                           SoPhieuDKVTTS,
                                           VTTSChuaDK,
                                           VTTheoPVCC,
                                           SoLanRaVao);
                                    }
                                    dtc++;
                                }
                                

                            }
                            catch (Exception ex)
                            {
                                TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin và import lại từ dòng : " + dtc + "');</script>";
                                return RedirectToAction("Index", "Detail_ListCarteTemporaire", new { id = TTTID });
                            }

                        }
                        else
                        {
                            TempData["msgError"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                        }

                    }
                    else
                    {
                        TempData["msgError"] = "<script>alert('Vui lòng chọn File Excel');</script>";
                        return RedirectToAction("Index", "Detail_ListCarteTemporaire", new { id = TTTID });
                    }
                    return RedirectToAction("Index", "Detail_ListCarteTemporaire", new { id = TTTID });
                }
                else
                {
                   
                    foreach (var item in ListCT)
                    {
                        var CheckCCCD = db_nt.NT_NhanVienVP.Where(x => x.CCCD == item.CCCD).FirstOrDefault();
                        if (CheckCCCD == null)
                        {
                            if (item.HoVaTen == "" || item.CCCD == "" || item.NgaySinh == (DateTime)default || item.Sdt == "" || item.BienSo == "" || Convert.ToInt32(item.Cong) == 0 || item.ChucVu == "" || Convert.ToInt32(item.LoaiPTID) == 0 || item.SoPhieuDKVTTS == "" || item.VTTSChuaDK == "" || item.VTTheoPVCC == "" || item.SoLanRaVao == "")
                            {
                                msg += dtc + ", ";
                            }
                            var insert = db_nt.NT_DetailCarteTemporaire_Create
                                               (item.HoVaTen,
                                             item.CCCD,
                                             item.NgaySinh,
                                             item.Sdt,
                                             item.BienSo,
                                             Convert.ToInt32(item.Cong),
                                             item.GhiChu,
                                             TTTID,
                                             item.ChucVu,
                                             Convert.ToInt32(item.LoaiPTID),
                                             item.SoPhieuDKVTTS,
                                             item.VTTSChuaDK,
                                             item.VTTheoPVCC,
                                             item.SoLanRaVao);
                            dtc++;
                        }
                    }
                    if (msg != "")
                    {
                        TempData["msgError"] = "<script>alert('Vui lòng nhập đủ thông tin dòng: " + msg + "');</script>";
                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
                    }
                }
            }
            
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('đã cõ lỗi xãy ra: " + e.Message + "');</script>";
            }

            if (TTThe == 1)
            {
                var IDCT = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDTTT == TTTID).ToList();
                foreach (var item in IDCT)
                {
                    db_nt.NT_Handle_insert(TTTID, item.IDCTTTT);
                }
            }
            return RedirectToAction("Index", "ListCarteTemporaire");
        }
        public ActionResult CheckInformation(int id)
        {
            

            var IDCVKN = Models.MyAuthentication.IDPhongban.ToString();
            //KTV ký nháy
           var Carte = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == id).SingleOrDefault();
            //var test = db.AuthorizationContractors.Where(x => x.NhanVien.IDPhongBan == Carte.PhongBanID).ToList();

            /*var KTV = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 2 && x.NhanVien.IDPhongBan == Carte.PhongBanID)
                       join a in db.NhanViens on au.IDNhanVien equals a.ID
                       select new CheckInforUser
                       {
                           IDNhanVien = (int)au.IDNhanVien,
                           HoTen = a.HoTen + " : " + a.MaNV,
                       }).ToList();*/
            //TP BP/NM
            var TPBP = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && (x.NhanVien.IDPhongBan == Carte.PhongBanID || x.IDCVKN.Contains(Carte.PhongBanID.ToString())))
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            //VP1 C
            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 5)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();
            VP1C.Add(new CheckInforUser() { IDNhanVien=0,HoTen="Nhóm VP1C"});
            //ViewBag.KTVList = new SelectList(KTV, "IDNhanVien", "HoTen");
            ViewBag.TPBPList = new SelectList(TPBP, "IDNhanVien", "HoTen");
            ViewBag.VP1CList = new SelectList(VP1C, "IDNhanVien", "HoTen",0);
            ViewData["id"] = id;
            return PartialView();
        }

        [HttpPost]
        public ActionResult CheckInformation(NT_CarteTempo carteTemp, int id)
        {
            try
            {
                var idttt = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == id).SingleOrDefault();
                if (idttt != null)
                {
                    if (carteTemp.TPBP != null && carteTemp.VP1C != null)
                    {
                        //var Check = db.KD_TrinhKy.Where(x => x.IDDS == id).FirstOrDefault();                     
                        if (carteTemp.TPBP != null)
                        {
                            db_nt.sp_NTCarteTemp_Create(id, 1, null, carteTemp.TPBP, null, carteTemp.GhiChu);
                        }
                        if (carteTemp.VP1C != null)
                        {
                            db_nt.sp_NTCarteTemp_Create(id, 2, null, carteTemp.VP1C, null, carteTemp.GhiChu);
                        }
                        db_nt.NT_CarteTemporaire_Update_XL(idttt.IDTTT, 1);
                        TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";

                        //db_dk.DK_CardExtend_UpdateTK(id, 1);
                    }
                    else if (carteTemp.VP1C == 0)
                    {
                        TempData["msgError"] = "<script>alert('Vui lòng chọn VP1C');</script>";
                    }

                }
                else
                {
                    TempData["msgError"] = "<script>alert('Trình ký thất bại);</script>";
                }
            }

            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Trình ký thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListCarteTemporaire");
        }
        public ActionResult Cancel(int id)
        {
            var a = db_nt.NT_CarteTemp.Where(x => x.TTTID == id).ToList();
            if (a.Count > 0)
            {
                db_nt.sp_NTCarteTemp_Delete(id);
                db_nt.NT_CarteTemporaire_Update_XL(id, 0);
                TempData["msgSuccess"] = "<script>alert('Hủy trình ký thành công');</script>";
            }
            else
            {
                TempData["msgError"] = "<script>alert('Hủy trình ký thất bại');</script>";
            }
            return RedirectToAction("index", "ListCarteTemporaire");

        }
        public ActionResult Show(int? id, int? page)
        {

            var res = (from c in db_nt.NT_CarteTemp.ToList()
                       join b in db_nt.NT_CarteTemporaire.ToList() on c.TTTID equals b.IDTTT

                       where c.TTTID == id
                       select new HandleModel()
                       {
                           IDCXL = c.CapDuyet,
                           IDTTT = c.TTTID,
                           isNT = b.isNT,
                           GhiChu = c.GhiChu,
                           IDNguoiXuLy = c.NhanVienID,
                           NgayXL = (DateTime?)c.NgayDuyet ?? default,
                           TinhTrang = c.TinhTrangID
                       }
                    ).ToList();

            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);
            return View(res.OrderBy(x => x.IDCXL).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ShowAll(int? page,int? search, DateTime? begind, DateTime? endd)
        {
            List<NT_CarteTemporaireValidation> data = new List<NT_CarteTemporaireValidation>();
            if(search==null  && begind==null && endd==null)
            {
                data = db_nt.NT_CarteTemporaire.Select(x => new NT_CarteTemporaireValidation()
                {
                    IDTTT = (int)x.IDTTT,
                    NoiDung = x.NoiDung,
                    NguoiTaoID = (int?)x.NguoiTaoID ?? default,
                    NgayTao = (DateTime?)x.NgayTao ?? default,
                    TinhTrang = (int?)x.TinhTrang ?? default,
                    NTID = (int?)x.NTID ?? default,
                    PhongBanID = (int?)x.PhongBanID ?? default,
                    HangMuc = x.HangMuc,
                    ThoiHan = (DateTime)x.ThoiHan,
                    kyTruoc = x.KyTruoc,
                    GhiChu = x.GhiChu,
                    isNT = x.isNT,
                    FileHosoDK = x.FileHosoDK,
                }).ToList();
            }
            else
            {
                data = db_nt.NT_CarteTemporaire_search(begind, endd, search).Select(x => new NT_CarteTemporaireValidation()
                {
                    IDTTT = (int)x.IDTTT,
                    NoiDung = x.NoiDung,
                    NguoiTaoID = (int?)x.NguoiTaoID ?? default,
                    NgayTao = (DateTime?)x.NgayTao ?? default,
                    TinhTrang = (int?)x.TinhTrang ?? default,
                    NTID = (int?)x.NTID ?? default,
                    PhongBanID = (int?)x.PhongBanID ?? default,
                    HangMuc = x.HangMuc,
                    ThoiHan = (DateTime)x.ThoiHan,
                    kyTruoc = x.KyTruoc,
                    GhiChu = x.GhiChu,
                    isNT = x.isNT,
                    FileHosoDK = x.FileHosoDK,
                }).ToList();
            }
            
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName");
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(x => x.NgayTao).ToList().ToPagedList(pageNumber, pageSize));
        }
    }
}