using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
//using EPORTAL.ModelsOrganizational;
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

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class Detail_ListCarteTemporaireNTController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Detail_ListCarteTemporaireNT";
        // GET: TagSign/Detail_ListCarteTemporaire
        public ActionResult Index(int? id, int? page)
        {
            var IDTTT = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == id).FirstOrDefault();

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName", IDTTT.NTID);

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            List<PhongBan> pb = db.PhongBans.ToList();
            ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", IDTTT.PhongBanID);
            List<LoaiPT> pTs = db_nt.LoaiPTs.ToList();
            ViewBag.IDPT = new SelectList(pTs, "LoaiPTID", "TenPT");
            /*List<NT_Purpose> md = db_nt.NT_Purpose.ToList();
            ViewBag.IDMD = new SelectList(md, "IDMD", "TenMD");*/

            ViewBag.ThoiHan = IDTTT.ThoiHan.ToString();

            var res = (from a in db_nt.NT_DetailCarteTemporaire.Where(x => x.IDTTT == id)
                       select new NT_DetailCarteTemporaireValidation
                       {
                           IDCTTTT = (int)a.IDCTTTT,
                           HoVaTen = a.HoVaTen,
                           CCCD = a.CCCD,
                           NgaySinh = (DateTime)a.NgaySinh,
                           //MucDich = (int)a.MucDich,
                           ChucVu = a.ChucVu,
                           LoaiPTID = a.LoaiPTID,
                           Sdt = a.Sdt,
                           BienSo = a.BienSo,
                           Cong = (int)a.Cong,
                           GhiChu = a.GhiChu,
                           IDTTT = (int)a.IDTTT,
                           SoPhieuDKVTTS=a.SoPhieuDKVTTS,
                           VTTSChuaDK=a.VTTSChuaDK,
                           VTTheoPVCC = a.VTTheoPVCC,
                           SoLanRaVao=a.SoLanRaVao
                       }).ToList();
            if (id != null)
            {
                NT_CarteTemporaireValidation idttt = new NT_CarteTemporaireValidation()
                {
                    IDTTT = (int)id
                };
                ViewData["id"] = idttt;
            }
            /*if(IDTTT.TinhTrang ==3 || IDTTT.TinhTrang == 4)
            {
                return RedirectToAction("Index", "Detail_ViewTemporarycardNT",new { id =id});
            }*/
            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCTTTT).ToList().ToPagedList(pageNumber, pageSize));
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
        public ActionResult EditChild(int id, int idttt)
        {
            try
            {
                var a = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDCTTTT == id).Select(x => new NT_DetailCarteTemporaireValidation()
                {
                    Cong = (int)x.Cong,
                    IDCTTTT = x.IDCTTTT,
                    BienSo = x.BienSo,
                    LoaiPTID = (int)x.LoaiPTID,
                    CCCD = x.CCCD,
                    Sdt = x.Sdt,
                    ChucVu = x.ChucVu,
                    GhiChu = x.GhiChu,
                    IDTTT = (int)x.IDTTT,
                    HoVaTen = x.HoVaTen,
                    NgaySinh = (DateTime)x.NgaySinh,
                    SoPhieuDKVTTS = x.SoPhieuDKVTTS,
                    VTTSChuaDK = x.VTTSChuaDK,
                    VTTheoPVCC = x.VTTheoPVCC,
                    SoLanRaVao = x.SoLanRaVao

                }).SingleOrDefault();
                if (a != null)
                {
                    var loaipt = db_nt.LoaiPTs.ToList();
                    ViewBag.loaipt = new SelectList(loaipt, "LoaiPTID", "TenPT", a.LoaiPTID);
                    List<NT_Gate> cong = db.NT_Gate.ToList();
                    ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate", a.Cong);
                    ViewData["id"] = idttt;
                    return PartialView(a);
                }
                else TempData["msgError"] = "<script>alert('Cập nhật dữ liệu thất bại');</script>";

            }
            catch (Exception ex)
            {
                TempData["msgError"] = "<script>alert('Cập nhật dữ liệu thất bại: " + ex.Message + "');</script>";
            }
            return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = idttt });
        }
        [HttpPost]
        public ActionResult EditChild(NT_DetailCarteTemporaireValidation request, int id)
        {
            if (request.HoVaTen != null && request.CCCD != null && request.LoaiPTID != null &&
                request.ChucVu != null && request.NgaySinh != null && request.Sdt != null && request.BienSo != null &&
                request.Cong != 0 && request.SoPhieuDKVTTS != null && request.VTTSChuaDK != null && request.VTTheoPVCC !=null & request.SoLanRaVao!=null)
            {
                var a = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDCTTTT == request.IDCTTTT).SingleOrDefault();
                if (a != null)
                {
                    db_nt.NT_DetailCarteTemporaire_update1(id, request.HoVaTen, request.CCCD, request.NgaySinh, request.LoaiPTID, request.ChucVu, request.Sdt, request.BienSo, request.Cong, request.GhiChu,request.SoPhieuDKVTTS,request.VTTSChuaDK,request.VTTheoPVCC,request.SoLanRaVao);
                    TempData["msgSuccess"] = "<script>alert('Cập nhật dữ liệu thành công');</script>";
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Cập nhật dữ liệu thất bại');</script>";
                }
            }
            else
            {
                TempData["msgError"] = "<script>alert('Thông tin nhập vào không được để trống');</script>";
            }
            return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = request.IDTTT });
        }
        public ActionResult Edit(NT_DetailCarteTemporaireValidation _DO, FormCollection collection)
        {
            int TTTID = 0;
            int TTThe = 0;
            int dtc = 1;
            HttpPostedFileBase file_1 = Request.Files["FileHosoDK"];
            string filePath_1 = string.Empty;
            if (collection.Count > 2)
            {
                try
                {
                    var ListCT = new List<NT_DetailCarteTemporaireValidation>();
                    DateTime DayUpload = DateTime.Now;
                    //var id = db.AuthorizationContractors.Where(x => x.IDNhanVien == Models.MyAuthentication.ID).SingleOrDefault();
                    //var IDNV = db.NhanViens.Where(x => x.ID == id.IDNhanVien).FirstOrDefault();

                    //Upload file PDF ĐK học ATLĐ

                    foreach (var key in collection.AllKeys)
                    {
                        if (key != "__RequestVerificationToken")
                        {

                            if (key == "HangMuc")
                            {
                                //int idttt = Convert.ToInt32(collection["Gui"]);

                                string TT = collection["Gui"];
                                string TinhTrang = TT.Substring(0, 1);
                                TTThe = Convert.ToInt32(TinhTrang);
                                string ID = TT.Substring(2);
                                int idttt = Convert.ToInt32(ID);
                                var IDTTT = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == idttt).FirstOrDefault();
                                TTTID = Convert.ToInt32(ID);
                              
                                DateTime ThoiHan = DateTime.ParseExact(collection["ThoiHan"], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                if (TinhTrang == "0")
                                {
                                    if (collection["NoiDung"] == "")
                                    {
                                        TempData["msgError"] = "<script>alert('Vui lòng nhập nội dung');</script>";
                                        return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = idttt });
                                    }
                                    if (collection["HangMuc"] == "")
                                    {
                                        TempData["msgError"] = "<script>alert('Vui lòng nhập hạng mục');</script>";
                                        return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = idttt });
                                    }
                                    if ((file_1 != null) && (file_1.ContentLength > 0) && !string.IsNullOrEmpty(file_1.FileName))
                                    {
                                        filePath_1 = upLoadfile(file_1);
                                        string path = Server.MapPath("~/UploadedFiles/UploadedFiles/");
                                        if (IDTTT.FileHosoDK != "")
                                        {
                                            string filePath = path + IDTTT.FileHosoDK.ToString();
                                            System.IO.File.Delete(filePath);
                                        }
                                    }
                                    else
                                    {
                                        filePath_1=IDTTT.FileHosoDK.ToString();
                                    }
                                    db_nt.NT_CarteTemporaire_update(idttt, collection["NoiDung"], Models.MyAuthentication.ID, DayUpload, Convert.ToInt32(TinhTrang), Convert.ToInt32(collection["IDNT"]), Convert.ToInt32(collection["IDPhongBan"]), collection["HangMuc"], ThoiHan,0,filePath_1);
                                }
                                else
                                {
                                    if (collection["NoiDung"] == "")
                                    {
                                        TempData["msgError"] = "<script>alert('Vui lòng nhập nội dung');</script>";
                                        return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = idttt });
                                    }
                                    if (collection["HangMuc"] == "")
                                    {
                                        TempData["msgError"] = "<script>alert('Vui lòng nhập hạng mục');</script>";
                                        return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = idttt });
                                    }
                                    if ((file_1 != null) && (file_1.ContentLength > 0) && !string.IsNullOrEmpty(file_1.FileName))
                                    {
                                        filePath_1 = upLoadfile(file_1);
                                        string path = Server.MapPath("~/UploadedFiles/UploadedFiles/");
                                        string filePath = path + IDTTT.FileHosoDK.ToString();
                                        System.IO.File.Delete(filePath);
                                    }
                                    else
                                    {
                                        filePath_1 = IDTTT.FileHosoDK.ToString();
                                    }
                                    db_nt.NT_CarteTemporaire_update(idttt, collection["NoiDung"], Models.MyAuthentication.ID, DayUpload, Convert.ToInt32(TinhTrang), Convert.ToInt32(collection["IDNT"]), Convert.ToInt32(collection["IDPhongBan"]), collection["HangMuc"], ThoiHan, 0, filePath_1);
                                }
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
                    var IDCT = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDTTT == TTTID).Count();
                   
                    string msg = "";
                    
                    if (collection.Count == 8)
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
                                                    //  db_nt.NT_CarteTemporaire_delete(TTTID);
                                                    TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin và import lại từ dòng : " + dtc + "');</script>";
                                                }
                                                if (CheckCCCD != null)
                                                {
                                                    TempData["msgError"] = "<script>alert('Nhân viên nằm trong danh sách vi phạm. Tại dòng : " + dtc + "');</script>";
                                                }
                                                    return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = TTTID });
                                            }                                              
                                            string NgaySinh = dt.Rows[i][3].ToString().Trim();
                                            if (NgaySinh == "")
                                            {
                                                TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin và import lại từ dòng : " + dtc + "'');</script>";
                                                return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = TTTID });
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
                                                return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = TTTID });
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
                                        var IDCT1 = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDTTT == TTTID).ToList();
                                        //var handel=db_nt.NT_Handle.Where(x=>x.IDTTT == TTTID).ToList();

                                        foreach (var item in IDCT1)
                                        {
                                            var a = db_nt.NT_Handle.Where(x => x.IDCTTTT == item.IDCTTTT).FirstOrDefault();
                                            if (a == null)
                                            {
                                                db_nt.NT_Handle_insert(TTTID, item.IDCTTTT);
                                            }
                                        }
                                    dtc++;
                                    }

                                }
                                catch (Exception ex)
                                {
                                   
                                    TempData["msgError"] = "<script>alert('Vui lòng kiểm tra thông tin và import lại từ dòng : " + dtc + "');</script>";
                                }
                                return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = TTTID });

                            }
                            else
                            {
                                TempData["msgError"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                            }
                        }
                        else if (collection["check"] == "1" && file.ContentLength == 0)
                        {
                            TempData["msgError"] = "<script>alert('Vui lòng chọn File Excel');</script>";
                            return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = TTTID });
                        }
                    }
                    else
                    {
                       
                        foreach (var item in ListCT)
                        {
                            var CheckCCCD = db_nt.NT_NhanVienVP.Where(x => x.CCCD == item.CCCD).FirstOrDefault();
                            if (CheckCCCD == null)
                            {
                                if (item.HoVaTen == "" || item.CCCD == "" || item.NgaySinh == (DateTime)default || item.Sdt == "" || item.BienSo == "" || Convert.ToInt32(item.Cong) == 0 || item.ChucVu == "" || Convert.ToInt32(item.LoaiPTID) == 0 || item.SoPhieuDKVTTS=="" ||item.VTTSChuaDK=="" || item.VTTheoPVCC=="" || item.SoLanRaVao=="")
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
                            if (msg != "")
                            {
                                TempData["msgError"] = "<script>alert('Vui lòng nhập đủ thông tin dòng: " + msg + "');</script>";
                                return RedirectToAction("Index", "ListCarteTemporaireNT");
                            }
                            else
                            {
                                TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
                                
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (TTTID != 0)
                    {
                        db_nt.NT_DetailCarteTemporaire_delete(TTTID);
                        //db_nt.NT_CarteTemporaire_delete(TTTID);
                    }
                    TempData["msgError"] = "<script>alert('Vui lòng nhập đầy đủ thông tin');</script>";
                    return RedirectToAction("Index", "ListCarteTemporaireNT");
                }
            }
            else
            {
                TempData["msgError"] = "<script>alert('Vui lòng kiểm tra lại dòng : " + dtc + "');</script>";
                return RedirectToAction("Index", "ListCarteTemporaireNT");
            }
                
            try
            {
                var IDCT2 = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDTTT == TTTID).ToList();
                //var handel=db_nt.NT_Handle.Where(x=>x.IDTTT == TTTID).ToList();

                foreach (var item in IDCT2)
                {   
                    var a=db_nt.NT_Handle.Where(x=>x.IDCTTTT == item.IDCTTTT).FirstOrDefault();
                    if(a == null) { 
                    db_nt.NT_Handle_insert(TTTID, item.IDCTTTT);
                    }
                }
                
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Đã có lõi xãy ra:'" + e.Message + ");</script>";
            }
            return RedirectToAction("Index", "ListCarteTemporaireNT");
        }
        public ActionResult Delete(int? id, int? idttt)
        {
            try
            {
                var delete_hande = db_nt.NT_Handle.Where(x => x.IDCTTTT == id).ToList();
                if (delete_hande.Count > 0)
                {
                    db_nt.NT_handle_Delete_byIDCTTT(id);
                }
                db_nt.NT_DetailCarteTemporaire_delete(id);
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Detail_ListCarteTemporaireNT", new { id = idttt });
        }
    }
}