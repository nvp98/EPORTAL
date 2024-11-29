using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using EPORTAL.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class Detail_HandleController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Detail_Handle";
        // GET: TagSign/Detail_Handle
        public ActionResult Index(int? id, int? page)
        {
            var b = EPORTAL.Models.MyAuthentication.IDPhongban;
            List<NT_CardTemp> hd = db_nt.NT_CardTemp.ToList();
            db_nt.NT_Handle.Where(h => h.IDTTT == id).ToList().ForEach(x =>
            {
                NT_CardTemp nT_CardTemp = db_nt.NT_CardTemp.Where(c => c.IDThe == x.IDThe).FirstOrDefault();
                if (nT_CardTemp != null)
                {
                    hd.Remove(nT_CardTemp);
                }
            });
            ViewBag.IDThe = new SelectList(hd, "IDThe", "TenThe");

            var res = (from a in db_nt.NT_Handle.Where(x => x.IDTTT == id)
                      join ct in db_nt.NT_DetailCarteTemporaire on a.IDCTTTT equals ct.IDCTTTT
                       join pt in db_nt.LoaiPTs on ct.LoaiPTID equals pt.LoaiPTID
                       join th in db_nt.NT_CardTemp on a.IDThe equals th.IDThe into ulist1
                      from th in ulist1.DefaultIfEmpty()
                     select new Detail_HandleValidation
                      {
                          IDXL = (int)a.IDXL,
                          IDTTT =(int) a.IDTTT,
                          HoVaTen = ct.HoVaTen,
                          CCCD = ct.CCCD,
                          NgaySinh = (DateTime)ct.NgaySinh,
                          //MucDich = (int)ct.MucDich,
                          Sdt = ct.Sdt,
                          ChucVu=ct.ChucVu,
                          BienSo = ct.BienSo,
                          Cong = (int?)ct.Cong??default,
                          GhiChu = ct.GhiChu,
                          IDCTTTT = (int)a.IDCTTTT,
                          ThoiGianXuLy = (DateTime?)a.ThoiGianXuLy??default,
                          NguoiXuLy = (int?)a.NguoiXuLy??default,
                          IDThe = (int?)a.IDThe??default,
                          MaThe = th.MaThe,
                         SoPhieuDKVTTS = ct.SoPhieuDKVTTS,
                         VTTSChuaDK = ct.VTTSChuaDK,
                         VTTheoPVCC = ct.VTTheoPVCC,
                         SoLanRaVao = ct.SoLanRaVao,
                         TenPhuongTien=pt.TenPT
                     }).ToList();
            if (id != null)
            {
                NT_CarteTemporaireValidation idttt = new NT_CarteTemporaireValidation()
                {
                    IDTTT = (int)id
                };
                ViewData["id"] = idttt;
            }

            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCTTTT).ToList());
        }
        public ActionResult Cancel(int? id)
        {
            var res = (from a in db_nt.NT_CarteTemporaire.Where(x=>x.IDTTT == id)
                       select new NT_CarteTemporaireValidation
                       {
                           IDTTT = (int)a.IDTTT,
                           NoiDung = a.NoiDung,
                           NguoiTaoID = (int?)a.NguoiTaoID??default,
                           NgayTao = (DateTime?)a.NgayTao ?? default,
                           TinhTrang = (int?)a.TinhTrang??default,
                           NTID = (int?)a.NTID?? default,
                           PhongBanID = (int?)a.PhongBanID??default,
                           HangMuc = a.HangMuc,
                           ThoiHan = (DateTime)a.ThoiHan,
                           kyTruoc=a.KyTruoc,
                           isNT=a.isNT,
                           FileHosoDK = a.FileHosoDK,
                       }).ToList();
            NT_CarteTemporaireValidation DO = new NT_CarteTemporaireValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDTTT = (int)a.IDTTT;
                    DO.NoiDung = a.NoiDung;
                    DO.NgayTao = (DateTime?)a.NgayTao ?? default;
                    DO.TinhTrang = (int)a.TinhTrang;
                    DO.NTID = (int)a.NTID;
                    DO.PhongBanID = (int)a.PhongBanID;
                    DO.HangMuc = a.HangMuc;
                    DO.ThoiHan = (DateTime)a.ThoiHan;
                    DO.NguoiTaoID = (int)a.NguoiTaoID;
                    DO.kyTruoc=(int)a.kyTruoc;
                    DO.isNT=(int)a.isNT;
                    DO.FileHosoDK=a.FileHosoDK;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Cancel(NT_CarteTemporaireValidation _DO)
        {

            if (_DO.GhiChu != null)
            {

                try
                {
                    if (_DO.isNT == 0)
                    {
                        var check = db_nt.NT_CarteTemp.Where(x => x.TTTID == _DO.IDTTT && x.TinhTrangID == null && x.CapDuyet == 1).ToList();
                        if (check.Count > 0)
                        {
                            db_nt.NT_CarteTemporaire_update(_DO.IDTTT, _DO.NoiDung, _DO.NguoiTaoID, _DO.NgayTao, 4, _DO.NTID, _DO.PhongBanID, _DO.HangMuc, _DO.ThoiHan, 1,_DO.FileHosoDK);
                        }
                        else
                        {
                            db_nt.NT_CarteTemporaire_update(_DO.IDTTT, _DO.NoiDung, _DO.NguoiTaoID, _DO.NgayTao, 4, _DO.NTID, _DO.PhongBanID, _DO.HangMuc, _DO.ThoiHan, 0, _DO.FileHosoDK);
                        }
                    }
                    else
                    {
                        var check = db_nt.NT_CarteTemp.Where(x => ((x.TTTID == _DO.IDTTT && x.TinhTrangID == null && x.CapDuyet == 1) || (x.TTTID == _DO.IDTTT && x.TinhTrangID == null && x.CapDuyet == 2))).ToList();
                        if (check.Count > 0)
                        {
                            db_nt.NT_CarteTemporaire_update(_DO.IDTTT, _DO.NoiDung, _DO.NguoiTaoID, _DO.NgayTao, 4, _DO.NTID, _DO.PhongBanID, _DO.HangMuc, _DO.ThoiHan, 1, _DO.FileHosoDK);
                        }
                        else
                        {
                            db_nt.NT_CarteTemporaire_update(_DO.IDTTT, _DO.NoiDung, _DO.NguoiTaoID, _DO.NgayTao, 4, _DO.NTID, _DO.PhongBanID, _DO.HangMuc, _DO.ThoiHan, 0, _DO.FileHosoDK);
                        }
                    }


                    var CTTTT = db_nt.NT_Handle.Where(x => x.IDTTT == _DO.IDTTT).ToList();
                    var CarteTemp = db_nt.NT_CarteTemp.Where(x => x.TTTID == _DO.IDTTT && x.NhanVienID == Models.MyAuthentication.ID).FirstOrDefault();
                    var nt_CarteTempo = db_nt.NT_CarteTemp.Where(x => x.TTTID == _DO.IDTTT).ToList();
                    NT_CarteTempo item = new NT_CarteTempo();
                    var capduyet = db.AuthorizationContractors.Where(x => x.IDNhanVien == Models.MyAuthentication.ID).Select(x => x.IDLKD).SingleOrDefault();
                    DateTime DayUpload = DateTime.Now;
                    if (capduyet == 5)
                    {
                        var check_isNT = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == _DO.IDTTT).SingleOrDefault();
                        if (check_isNT != null)
                        {
                            if (check_isNT.isNT == 0)
                            {
                                item = nt_CarteTempo.Where(x => x.CapDuyet == 2).Select(x => new NT_CarteTempo() { IDDTT = x.IDDTT, CapDuyet = x.CapDuyet, GhiChu = x.GhiChu }).FirstOrDefault();

                            }
                            else
                            {
                                item = nt_CarteTempo.Where(x => x.CapDuyet == 3).Select(x => new NT_CarteTempo() { IDDTT = x.IDDTT, CapDuyet = x.CapDuyet, GhiChu = x.GhiChu }).FirstOrDefault();

                            }
                            db_nt.sp_NTCarteTemp_Update(item.IDDTT, item.CapDuyet, 0, Models.MyAuthentication.ID, DayUpload, _DO.GhiChu);
                        }
                    }
                    else
                    {
                        item = nt_CarteTempo.Where(x => x.NhanVienID == Models.MyAuthentication.ID).Select(x => new NT_CarteTempo() { IDDTT = x.IDDTT, CapDuyet = x.CapDuyet, GhiChu = x.GhiChu }).FirstOrDefault();
                        db_nt.sp_CarteTemp_Update1(item.IDDTT, 0, DayUpload);
                    }                                    
                    foreach (var a in CTTTT)
                    {
                        db_nt.NT_Handle_update(a.IDXL, Models.MyAuthentication.ID, DayUpload, null);
                    }
                    TempData["msgSuccess"] = "<script>alert('Hủy thành công');</script>";
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
                    return RedirectToAction("Index", "Detail_Handle", new { id = _DO.IDTTT });
                }
            }
            else
            {
                TempData["msgError"] = "<script>alert('Lý do không được để trống');</script>";
                return RedirectToAction("Index", "Detail_Handle", new { id = _DO.IDTTT });
            }
            return RedirectToAction("DSDuyet", "List_Signature", new { loai = 1 });
        }
        public ActionResult Edit(List<Detail_HandleValidation> ListDetail, FormCollection collection)
        {
            int IDTTT = 0;
            int IDTT = 0;
            var capduyet = db.AuthorizationContractors.Where(x => x.IDNhanVien == Models.MyAuthentication.ID).Select(x => x.IDLKD).SingleOrDefault();
            try
            {
                var ListCT = new List<Detail_HandleValidation>();
                DateTime DayUpload = DateTime.Now;
                var id = db.AuthorizationContractors.Where(x => x.IDNhanVien == Models.MyAuthentication.ID).SingleOrDefault();
                var IDNV = db.NhanViens.Where(x => x.ID == id.IDNhanVien).FirstOrDefault();
             
                //Upload file PDF ĐK học ATLĐ
                foreach (var key in collection.AllKeys)
                {
                   
                    if (key != "__RequestVerificationToken")
                    {
                        if (key == "Gui")
                        {
                            string TT = collection["Gui"];
                            string TinhTrang = TT.Substring(0, 1);
                            IDTT = Convert.ToInt32(TinhTrang);
                            int TTThe = Convert.ToInt32(TinhTrang);
                            string ID = TT.Substring(2);
                            IDTTT = Convert.ToInt32(ID);
                          
                        }
                        else
                        {
                            string Arr = key.ToString();
                            string newstring = Arr.Substring(6);
                            string The = collection["IDThe_" + newstring];
                            if (The != "null")
                            {
                                int th = Convert.ToInt32(The);
                                var a = db_nt.NT_Handle.Where(x => x.IDTTT == IDTTT && x.IDThe == th).FirstOrDefault();
                                if (a != null)
                                {
                                    if (a.IDXL != Convert.ToInt32(newstring))
                                    {
                                        TempData["msgError"] = "<script>alert('Vui lòng nhập mã thẻ Không được trùng');</script>";
                                        return RedirectToAction("Index", "Detail_Handle", new { id = IDTTT });
                                    }
                                }
                            }
                            int IDCT = Convert.ToInt32(newstring);
                            var Check = db_nt.NT_Handle.Where(x => x.IDXL == IDCT).FirstOrDefault();
                            var NVNT = db_nt.NT_DetailCarteTemporaire.Where(x => x.IDCTTTT == Check.IDCTTTT).FirstOrDefault();
                            if (Check.IDThe.ToString() != The && Models.MyAuthentication.ID == Check.NguoiXuLy || Check.IDThe.ToString() != The && Check.NguoiXuLy == null)
                            {
                                if (key == "IDThe_" + newstring && The != "null" && IDTT == 2)

                                {
                                    db_nt.NT_Handle_update(Convert.ToInt32(newstring), Models.MyAuthentication.ID, DayUpload, Convert.ToInt32(The));
                                   
                                }
                                else if(key == "IDThe_" + newstring && The != "null" && IDTT == 3)
                                {
                                    if(Check.ThoiGianXuLy == null)
                                    {
                                        db_nt.NT_Handle_update(Convert.ToInt32(newstring), Models.MyAuthentication.ID, DayUpload, Convert.ToInt32(The));
                                    }    
                                   else
                                    {
                                        db_nt.NT_Handle_update(Convert.ToInt32(newstring), Models.MyAuthentication.ID, Check.ThoiGianXuLy, Convert.ToInt32(The));
                                    }    
                                }
                                else if (key == "IDThe_" + newstring && The != "null" && IDTT == 0)
                                {
                                    db_nt.NT_Handle_update(Convert.ToInt32(newstring), Models.MyAuthentication.ID, DayUpload, Convert.ToInt32(The));
                                    TempData["msgError"] = "<script>alert('Cập nhật thành công');</script>";
                                }
                            }   
                            else if (key == "IDThe_" + newstring && Models.MyAuthentication.ID != Check.NguoiXuLy && Check.IDThe != Convert.ToInt32(The))
                            {
                               

                                db_nt.NT_CarteTemporaire_Update_XL(Check.IDTTT, 2);
                                TempData["msgError"] = "<script>alert('Có lỗi khi cập nhật');</script>";

                                return RedirectToAction("Index", "Detail_Handle", new { id = Check.IDTTT });
                            }

                        }

                    }

                }
                var UpDate = db_nt.NT_Handle.Where(x => x.IDTTT == IDTTT && x.IDThe == null).ToList();
                /*if (IDTT == 2)
                {
                    TempData["msgSuccess"] = "<script>alert('Lưu thành công');</script>";
                    return RedirectToAction("Index", "Handle");
                }
                else if (IDTT == 3)
                {
                    TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
                }*/

                if (IDTT != 0)
                {
                    if ((IDTT == 3 && UpDate.Count == 0) || (IDTT == 2 && UpDate.Count == 0))
                    {
                        var nt_CarteTempo = db_nt.NT_CarteTemp.Where(x => x.TTTID == IDTTT ).ToList();
                        NT_CarteTempo item=new NT_CarteTempo();
                        if (capduyet == 5 )
                        {
                            var check_isNT = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == IDTTT).SingleOrDefault();
                            if (check_isNT != null ) {
                                if (check_isNT.isNT == 0)
                                {
                                    item = nt_CarteTempo.Where(x => x.CapDuyet == 2).Select(x=>new NT_CarteTempo() {IDDTT=x.IDDTT,CapDuyet=x.CapDuyet,GhiChu=x.GhiChu }).FirstOrDefault();
                                  
                                }
                                else
                                {
                                    item = nt_CarteTempo.Where(x => x.CapDuyet == 3).Select(x => new NT_CarteTempo() { IDDTT = x.IDDTT, CapDuyet = x.CapDuyet, GhiChu = x.GhiChu }).FirstOrDefault();
                                   
                                }
                                db_nt.sp_NTCarteTemp_Update(item.IDDTT, item.CapDuyet, 1, Models.MyAuthentication.ID, DateTime.Now, item.GhiChu);
                            }
                        }
                        else
                        {
                            item = nt_CarteTempo.Where(x => x.NhanVienID == Models.MyAuthentication.ID).Select(x => new NT_CarteTempo() { IDDTT = x.IDDTT, CapDuyet = x.CapDuyet, GhiChu = x.GhiChu }).FirstOrDefault();
                            db_nt.sp_CarteTemp_Update1(item.IDDTT, 1, DayUpload);
                        }
                            var check = db_nt.NT_CarteTemp.Where(x => x.TTTID == IDTTT);
                            var check1 = check.Where(x => x.TinhTrangID == null & x.CapDuyet == 1).SingleOrDefault();
                            var check2 = check.Where(x => x.TinhTrangID == null && x.CapDuyet == 2).SingleOrDefault();
                            var a = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == IDTTT).SingleOrDefault();
                        
                     
                        if(a.isNT==1) 
                        {       
                            if (check1 != null || check2 != null)
                            {
                                db_nt.NT_CarteTemporaire_update(a.IDTTT,a.NoiDung,a.NguoiTaoID,a.NgayTao,2,a.NTID,a.PhongBanID,a.HangMuc,a.ThoiHan,1,a.FileHosoDK);                  
                            }
                            else
                            {
                                db_nt.NT_CarteTemporaire_Update_XL(Convert.ToInt32(IDTTT), 3);
                            }
                        }
                        else if (a.isNT == 0)
                        {
                            if (check1 != null)
                            {
                                db_nt.NT_CarteTemporaire_update(a.IDTTT, a.NoiDung, a.NguoiTaoID, a.NgayTao, 2, a.NTID, a.PhongBanID, a.HangMuc, a.ThoiHan, 1,a.FileHosoDK);
                            }
                            else
                            {
                                db_nt.NT_CarteTemporaire_Update_XL(Convert.ToInt32(IDTTT), 3);
                            }
                        }
                                                                   
                        TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
                    }
                    else if (IDTT == 3 && UpDate.Count != 0)
                    {
                        TempData["msgError"] = "<script>alert('Vui lòng cập nhật hết danh sách');</script>";
                        return RedirectToAction("Index", "Detail_Handle", new { id = IDTTT });
                    }
                    else if (IDTT == 2)
                    {
                        db_nt.NT_CarteTemporaire_Update_XL(Convert.ToInt32(IDTTT), IDTT);
                        TempData["msgSuccess"] = "<script>alert('Lưu thành công');</script>";
                        return RedirectToAction("Index", "Detail_Handle", new { id = IDTTT });
                    }
                }
                else
                {
                    db_nt.NT_CarteTemporaire_Update_XL(Convert.ToInt32(IDTTT), 5);
                }    

    
               
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi xác nhận');</script>";
            }
            return RedirectToAction("Index", "Detail_Handle", new { id = IDTTT });
        }
    }
}