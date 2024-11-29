using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class List_SignatureController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        EPORTAL_REGISTEREntities db_re = new EPORTAL_REGISTEREntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "List_Signature";
        // GET: TagSign/List_Signature
        public ActionResult DSDuyet(int? page, int loai, int? search, DateTime? begind, DateTime? endd)
        {
            
            var ID = Models.MyAuthentication.ID;
            var capduyet = db.AuthorizationContractors.Where(x => x.IDNhanVien == ID).Select(x => x.IDLKD).SingleOrDefault();
            List<NT_CarteTemporaireValidation> resCarte = new List<NT_CarteTemporaireValidation>();
            List<NT_CarteTemporaireValidation> data = new List<NT_CarteTemporaireValidation>();
            if (search == null && begind == null && endd == null)
            {
                 resCarte = db_nt.NT_CarteTemporaire.Select(x=>new NT_CarteTemporaireValidation()
                 {
                     IDTTT = (int)x.IDTTT,
                     NoiDung = x.NoiDung,
                     NguoiTaoID = (int?)x.NguoiTaoID ?? default,
                     NgayTao = (DateTime?)x.NgayTao ?? default,
                     TinhTrang = (int?)x.TinhTrang??default,
                     NTID = (int?)x.NTID??default,
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
                resCarte = db_nt.NT_CarteTemporaire_search(begind, endd, search).Select(x => new NT_CarteTemporaireValidation()
                 {
                     IDTTT = (int)x.IDTTT,
                     NoiDung = x.NoiDung,
                     NguoiTaoID = (int?)x.NguoiTaoID ?? default,
                     NgayTao = (DateTime?)x.NgayTao ?? default,
                     TinhTrang = (int)x.TinhTrang,
                     NTID = (int)x.NTID,
                     PhongBanID = (int)x.PhongBanID,
                     HangMuc = x.HangMuc,
                     ThoiHan = (DateTime)x.ThoiHan,
                     kyTruoc = x.KyTruoc,
                     GhiChu = x.GhiChu,
                     isNT = x.isNT,
                     FileHosoDK = x.FileHosoDK,
                 }).ToList();
            }
            if (capduyet != 0)
            {
                if (loai == 0)
                {
                    //var c = db_nt.NT_CarteTemporaire.Where(x =>x.IDTTT==4878 &&  x.TinhTrang == 2 ).ToList();
                    //var c = db_re.TK_CarteTemp.ToList();
                    if (capduyet !=5)
                    {
                        
                        var dsDuyet = (from a in db_nt.NT_CarteTemp.Where(x => x.NhanVienID == ID && x.TinhTrangID == null).ToList()
                                       join b in resCarte.Where(x => x.TinhTrang == 2 || x.TinhTrang == 5 || x.TinhTrang == 1).ToList() on a.TTTID equals b.IDTTT
                                       select new NT_CarteTemporaireValidation()
                                       {
                                           IDTTT = (int)a.TTTID,
                                           NoiDung = b.NoiDung,
                                           NguoiTaoID = (int?)b.NguoiTaoID ?? default,
                                           NgayTao = (DateTime?)b.NgayTao ?? default,
                                           NgayDuyet = (DateTime?)a.NgayDuyet ?? default,
                                           TinhTrang = (int)b.TinhTrang,
                                           NTID = (int)b.NTID,
                                           PhongBanID = (int)b.PhongBanID,
                                           HangMuc = b.HangMuc,
                                           GhiChu = b.GhiChu,
                                           ThoiHan = (DateTime)b.ThoiHan,
                                           capduyet = a.CapDuyet,
                                           isNT = b.isNT,
                                           FileHosoDK = b.FileHosoDK,
                                       }).ToList();
                        if (dsDuyet.Count > 0)
                        {
                            if (capduyet == 1)
                            {
                                foreach (var a in dsDuyet)
                                {
                                    if (a.isNT == 0)
                                    {
                                        data.Add(a);
                                    }
                                    else
                                    {
                                        var check = db_nt.NT_CarteTemp.Where(t => t.CapDuyet == 1 && t.TTTID == a.IDTTT && (t.TinhTrangID == 0 || t.TinhTrangID == null)).FirstOrDefault();
                                        if (check != null)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            data.Add(a);
                                        }
                                    }
                                }                              
                            }
                            else if (capduyet == 2)
                            {
                                data = dsDuyet;
                            }
                            
                        }
                    }
                    else
                    {
                        var dsDuyet= (from a in db_nt.NT_CarteTemp.Where(x => x.TinhTrangID == null).ToList()
                                      join b in resCarte.Where(x => x.TinhTrang == 2 || x.TinhTrang == 5 || x.TinhTrang == 1 ).ToList() on a.TTTID equals b.IDTTT
                                      where (b.isNT==0 && a.CapDuyet==2)||(b.isNT==1 && a.CapDuyet==3)
                                      select new NT_CarteTemporaireValidation()
                                      {
                                          IDTTT = (int)a.TTTID,
                                          NoiDung = b.NoiDung,
                                          NguoiTaoID = (int?)b.NguoiTaoID ?? default,
                                          NgayTao = (DateTime?)b.NgayTao ?? default,
                                          NgayDuyet = (DateTime?)a.NgayDuyet ?? default,
                                          TinhTrang = (int?)b.TinhTrang ?? default,
                                          NTID = (int?)b.NTID ?? default,
                                          PhongBanID = (int?)b.PhongBanID ?? default,
                                          HangMuc = b.HangMuc,
                                          GhiChu = b.GhiChu,
                                          ThoiHan = (DateTime)b.ThoiHan,
                                          capduyet = a.CapDuyet,
                                          isNT = b.isNT,
                                          NguoiDuyet=(int?)a.NhanVienID??default,
                                          FileHosoDK = b.FileHosoDK,
                                      }).ToList();
                        foreach(var item in dsDuyet)
                        {                          
                                if (item.NguoiDuyet == 0)
                                {
                                    data.Add(item);
                                }
                                else
                                {
                                    var check = db_nt.NT_CarteTemp.Where(x => x.NhanVienID == ID && x.TTTID==item.IDTTT).SingleOrDefault();
                                    if(check != null)
                                    {
                                        data.Add(item);
                                    }
                                }                           
                        }
                    }                                     
                }
                    
                
                else
                {
                    var dsDaXuLy = (from a in db_nt.NT_CarteTemp.Where(x => x.NhanVienID == ID && x.TinhTrangID != null).ToList()
                                    join b in resCarte.ToList() on a.TTTID equals b.IDTTT
                                    select new NT_CarteTemporaireValidation()
                                    {
                                        IDTTT = (int)b.IDTTT,
                                        GhiChu = a.GhiChu,
                                        NoiDung = b.NoiDung,
                                        NguoiTaoID = (int?)b.NguoiTaoID ?? default,
                                        NgayTao = (DateTime?)b.NgayTao ?? default,
                                        NgayDuyet = (DateTime?)a.NgayDuyet ?? default,
                                        TinhTrang = (int?)a.TinhTrangID ?? default,
                                        NTID = (int?)b.NTID ?? default,
                                        TenTT = (a.TinhTrangID == 0) ? "Đã hủy" : "Đã Duyệt",
                                        PhongBanID = (int?)b.PhongBanID ?? default,
                                        HangMuc = b.HangMuc,
                                        ThoiHan = (DateTime?)b.ThoiHan ?? default,
                                        capduyet = (int?)a.CapDuyet ?? default,
                                        isNT = b.isNT,
                                        FileHosoDK=b.FileHosoDK,
                                    }).ToList();
                    data = dsDaXuLy;
                }
                if (page == null) page = 1;
                int pageSize = 100;
                int pageNumber = (page ?? 1);
                List<NT_Partner> nt = db.NT_Partner.ToList();
                ViewBag.IDNT = new SelectList(nt, "ID", "FullName");
                ViewBag.loai = loai;
                ViewBag.CapDuyet = capduyet;
                return View(data.OrderByDescending(x => x.NgayTao).ToList().ToPagedList(pageNumber, pageSize));
            }
            return Content("bạn khong co quyenf truy cap");

        }

        public ActionResult DetailInfo(int? id, int? page, int? loai)
        {
            
            int idnv = Models.MyAuthentication.ID;
            var capduyet = db.AuthorizationContractors.Where(x => x.IDNhanVien == idnv).Select(x => x.IDLKD).SingleOrDefault();
           
            var res = (from a in db_nt.NT_DetailCarteTemporaire.Where(x => x.IDTTT == id)
                       join b in db_nt.LoaiPTs on a.LoaiPTID equals b.LoaiPTID
                       select new NT_DetailCarteTemporaireValidation
                       {
                           IDCTTTT = (int)a.IDCTTTT,
                           HoVaTen = a.HoVaTen,
                           CCCD = a.CCCD,
                           NgaySinh = (DateTime)a.NgaySinh,
                           //    MucDich = (int)a.MucDich,
                           Sdt = a.Sdt,
                           ChucVu = a.ChucVu,
                           LoaiPTID = (int?)a.LoaiPTID ?? default,
                           TenPT = b.TenPT,
                           
                           BienSo = a.BienSo,
                           Cong = (int?)a.Cong ?? default,
                           GhiChu = a.GhiChu,
                           IDTTT = (int?)a.IDTTT ?? default,
                           SoPhieuDKVTTS = a.SoPhieuDKVTTS,
                           VTTSChuaDK = a.VTTSChuaDK,
                           VTTheoPVCC = a.VTTheoPVCC,
                           SoLanRaVao = a.SoLanRaVao
                       }).ToList();
            if (id != null)
            {
                NT_CarteTemporaireValidation idttt = new NT_CarteTemporaireValidation()
                {
                    IDTTT = (int)id,
                    
                };
                ViewData["id"] = idttt;
                ViewBag.loai = loai;
                ViewBag.idttt = id;
                ViewBag.capduyet = capduyet;
                
                
            }
            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCTTTT).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Duyet(int? id, int? idttt)
        {
            var a = db_nt.NT_CarteTemp.Where(x => x.IDDTT == id).Select(x => new NT_CarteTempo()
            {
                IDDTT = (int)x.IDDTT,
                CapDuyet = (int?)x.CapDuyet ?? default,
                TTTID = (int?)x.TTTID ?? default,
                NgayDuyet = (DateTime?)x.NgayDuyet ?? default,
                NhanVienID = (int?)x.NhanVienID ?? default,
                TinhTrangID = (int?)x.TinhTrangID ?? default,
                GhiChu = x.GhiChu
            }).SingleOrDefault();
            if (a != null)
            {
                return PartialView(a);
            }
            else
            {
                TempData["msgError"] = "<script>alert('Duyệt thất bại');</script>";
                return RedirectToAction("Index", "List_Signature", new { id = idttt });
            }
        }
        [HttpPost]
        public ActionResult Cancel(NT_CarteTempo nT_Carte, int? id)
        {
            if (nT_Carte.GhiChu != null)
            {
                var carte = db_nt.NT_CarteTemp.Where(x => x.IDDTT == id).SingleOrDefault();
                if (carte != null)
                {
                    db_nt.sp_NTCarteTemp_Update(nT_Carte.IDDTT, nT_Carte.CapDuyet, 0, nT_Carte.NhanVienID, DateTime.Now, nT_Carte.GhiChu);
                    var a = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == nT_Carte.TTTID).SingleOrDefault();
                    if (a != null)
                    {
                        db_nt.NT_CarteTemporaire_update(a.IDTTT, a.NoiDung, a.NguoiTaoID, a.NgayTao, 4, a.NTID, a.PhongBanID, a.HangMuc, a.ThoiHan, a.KyTruoc, a.FileHosoDK);
                        TempData["msgSuccess"] = "<script>alert('Hủy thành công');</script>";
                        return RedirectToAction("DSDuyet", "List_Signature", new { loai = 1 });
                    }
                    TempData["msgError"] = "<script>alert('Hủy thất bại');</script>";
                    return RedirectToAction("DetailInfo", "List_Signature", new { id = nT_Carte.TTTID, loai = 0 });
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Hủy thất bại');</script>";
                    return RedirectToAction("DetailInfo", "List_Signature", new { id = nT_Carte.TTTID, loai = 0 });
                }
            }
            else
            {
                TempData["msgError"] = "<script>alert('Lý do không được để trống');</script>";
                return RedirectToAction("DetailInfo", "List_Signature", new { id = nT_Carte.TTTID, loai = 0 });
            }
        }
        [HttpPost]
        public ActionResult Duyet(int? id)
        {

            var phieu = db_nt.NT_CarteTemp.Where(a => a.TTTID == id && a.NhanVienID == Models.MyAuthentication.ID).Select(x => new NT_CarteTempo()
            {
                IDDTT = x.IDDTT,
                CapDuyet = (int?)x.CapDuyet ?? default,
                TTTID = (int?)x.TTTID ?? default,
                NgayDuyet = (DateTime?)x.NgayDuyet ?? default,
                NhanVienID = (int?)x.NhanVienID ?? default,
                TinhTrangID = (int?)x.TinhTrangID ?? default,
                GhiChu = x.GhiChu,

            }).SingleOrDefault();
            if (phieu != null)
            {
                db_nt.sp_NTCarteTemp_Update(phieu.IDDTT, phieu.CapDuyet, 1, phieu.NhanVienID, DateTime.Now, phieu.GhiChu);
                var check1 = db_nt.NT_CarteTemporaire.Where(x => x.IDTTT == id).SingleOrDefault();
                if (check1.isNT == 1)
                {
                    if (phieu.CapDuyet == 2)
                    {
                        var check = db_nt.NT_CarteTemp.Where(x => x.TTTID == id && x.CapDuyet == 3 && x.TinhTrangID == 1).FirstOrDefault();
                        if (check != null)
                        {
                            db_nt.NT_CarteTemporaire_Update_XL(phieu.TTTID, 3);
                        }
                    }
                    else
                    {
                        db_nt.NT_CarteTemporaire_Update_XL(phieu.TTTID, 2);
                    }
                }
                else
                {
                    var check = db_nt.NT_CarteTemp.Where(x => x.TTTID == id && x.CapDuyet == 2 && x.TinhTrangID == 1).FirstOrDefault();
                    if (check != null)
                    {
                        db_nt.NT_CarteTemporaire_Update_XL(phieu.TTTID, 3);
                    }
                    else
                    {
                        db_nt.NT_CarteTemporaire_Update_XL(phieu.TTTID, 2);
                    }

                }
                //var check = db_nt.NT_CarteTemp.Where(x => x.TTTID == id && x.CapDuyet == 1 && x.TinhTrangID == 1).FirstOrDefault();



                TempData["msgSuccess"] = "<script>alert('Duyệt thành công');</script>";
                return RedirectToAction("DSDuyet", "List_Signature", new { loai = 1 });
            }
            TempData["msgError"] = "<script>alert('Xử lý thất bại');</script>";
            return RedirectToAction("DSDuyet", "List_Signature", new { loai = 0 });

        }

        public ActionResult DSKyTruoc(int? page, int? search, DateTime? begind, DateTime? endd)
        {
            int idnv = Models.MyAuthentication.ID;
            var capduyet = db.AuthorizationContractors.Where(x => x.IDNhanVien == idnv).Select(x => x.IDLKD).SingleOrDefault();
            List<NT_CarteTemporaireValidation> data = new List<NT_CarteTemporaireValidation>();
            if (search == null && begind == null && endd == null)
            {
                data =(from a in db_nt.NT_CarteTemporaire.ToList()
                      join b in db_nt.NT_CarteTemp.ToList() on a.IDTTT equals b.TTTID
                      where b.NhanVienID == idnv && a.KyTruoc == 1
                      select  new NT_CarteTemporaireValidation()
                       {
                        IDTTT = (int)b.TTTID,
                        kyTruoc = a.KyTruoc,
                        NoiDung = a.NoiDung,
                        NguoiTaoID = (int?)a.NguoiTaoID ?? default,
                        NgayTao = (DateTime?)a.NgayTao ?? default,
                        NgayDuyet = (DateTime?)b.NgayDuyet ?? default,
                        TinhTrang = (int?)a.TinhTrang ?? default,
                        NTID = (int?)a.NTID ?? default,
                        PhongBanID = (int?)a.PhongBanID ?? default,
                        HangMuc = a.HangMuc,
                        GhiChu = a.GhiChu,
                        ThoiHan = (DateTime?)a.ThoiHan ?? default,
                        capduyet = b.CapDuyet,
                        isNT = a.isNT,
                        FileHosoDK = a.FileHosoDK,
                          }).ToList();
            }
            else
            {
                data =(from a in db_nt.NT_CarteTemporaire_search(begind, endd, search).ToList()
                       join b in db_nt.NT_CarteTemp.ToList() on a.IDTTT equals b.TTTID
                       where b.NhanVienID == idnv && a.KyTruoc == 1
                       select new NT_CarteTemporaireValidation()
                        {
                           IDTTT = (int)b.TTTID,
                           kyTruoc = a.KyTruoc,
                           NoiDung = a.NoiDung,
                           NguoiTaoID = (int?)a.NguoiTaoID ?? default,
                           NgayTao = (DateTime?)a.NgayTao ?? default,
                           NgayDuyet = (DateTime?)b.NgayDuyet ?? default,
                           TinhTrang = (int?)a.TinhTrang ?? default,
                           NTID = (int?)a.NTID ?? default,
                           PhongBanID = (int)a.PhongBanID,
                           HangMuc = a.HangMuc,
                           GhiChu = a.GhiChu,
                           ThoiHan = (DateTime?)a.ThoiHan ?? default,
                           capduyet = b.CapDuyet,
                           isNT = a.isNT,
                           FileHosoDK = a.FileHosoDK,
                       }).ToList();
            }
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName");
            if (page == null) page = 1;
            int pageSize = 1000;
            int pageNumber = (page ?? 1);
            ViewBag.flag = 1;
            return View(data.OrderByDescending(x => x.NgayTao).ToList().ToPagedList(pageNumber, pageSize));
        }

        /*public ActionResult Show(int? id, int? page)
        {
            
            var a = (from c in db_nt.NT_CarteTemp.ToList()
                     join b in db_nt.NT_CarteTemporaire.ToList() on c.TTTID equals b.IDTTT
                     join nv in db.NhanViens.ToList() on c.NhanVienID equals nv.ID
                     where c.TTTID == id
                     select new HandleModel()
                     {                      
                         IDCXL = c.CapDuyet,
                         IDTTT = c.TTTID,
                         isNT=b.isNT,
                         GhiChu = c.GhiChu,                       
                         IDNguoiXuLy = nv.ID,
                         NgayXL = (DateTime?)c.NgayDuyet ?? default,
                         IDCD = nv.IDViTri,
                         TIDBP = nv.IDPhongBan,
                         TinhTrang = c.TinhTrangID
                     }
                    ).ToList();
            
            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);         
            return View(a.OrderBy(x => x.IDCXL).ToPagedList(pageNumber, pageSize));
        }*/
    }
}