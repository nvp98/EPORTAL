using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsServey;
using EPORTAL.ModelsView360;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Servey.Controllers
{
    public class ServeyIDController : Controller
    {
        // GET: Servey/ServeyID
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_SERVEYEntities dbSV = new EPORTAL_SERVEYEntities();
        public ActionResult Index(int? IDSV)
        {
            DateTime ts = DateTime.Now;
            ts = new DateTime(ts.Year, ts.Month, ts.Day, 0, 0, 0);
            var res = (from a in dbSV.OptionServeys.Where(x => x.IDSV == IDSV)
                       select new OptionValidation
                       {
                           IDOT = a.IDOT,
                           ContentOT = a.ContentOT,
                           FilePath = a.FilePath,
                           OrderBy = a.OrderBy,
                           IDSV = (int)a.IDSV,
                           MaOT = a.MaOT,
                       }).OrderBy(x => x.OrderBy).ToList(); //ds option


            var aa = res.Count > 0 ? res[0].ContentSV : null;

            var employ = dbSV.EmployeeServeys.Where(x => x.IDNV == MyAuthentication.ID && x.IDSV == IDSV).FirstOrDefault();
            var ctks = dbSV.CTKhaoSats.ToList();
            ViewBag.TenDK = dbSV.ListServeys.Where(x => x.IDSV == IDSV).FirstOrDefault().ContentSV;
            ViewBag.TinhTrangDK = employ != null ? employ.OTID : null;
            ViewBag.checkShowDK = ctks.Where(x => x.IDNV == MyAuthentication.ID && x.IDSV == IDSV && x.IDOT == 98).Count() != 0 ? "true" : "false";
            ViewBag.DKKhong = ctks.Where(x => x.IDNV == MyAuthentication.ID && x.IDSV == IDSV && x.IDOT == 99).Count() != 0 ? "true" : "false";
            ViewBag.IDSV = IDSV;
            var group = (from a in dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV)
                         let CtKS = dbSV.CTKhaoSats.Where(x => x.IDSV == IDSV && x.IDNV == MyAuthentication.ID && x.IDGroup == a.ID).FirstOrDefault()
                         select new GroupKhaoSatView
                         {
                             ID = a.ID,
                             IDSV = a.IDSV,
                             MaNhom = a.MaNhom,
                             TenNhom = a.TenNhom,
                             OptionList = new OptionList
                             {
                                 ID = a.ID,
                                 GhiChu = CtKS != null ? CtKS.GhiChu : "",
                                 Answer = (int?)dbSV.CTKhaoSats.Where(x => x.IDSV == IDSV && x.IDNV == MyAuthentication.ID && x.IDGroup == a.ID).FirstOrDefault().IDOT ?? default,
                                 OptionLS = (from ks in dbSV.OptionServeys.Where(x => x.IDSV == IDSV && x.MaOT == a.MaNhom)
                                             select new OptionValidation
                                             {
                                                 IDOT = ks.IDOT,
                                                 ContentOT = ks.ContentOT,
                                                 FilePath = ks.FilePath,
                                                 OrderBy = ks.OrderBy,
                                                 IDSV = (int)a.IDSV,
                                                 MaOT = ks.MaOT,
                                                 isShow = ks.isShow
                                             }).OrderBy(x => x.OrderBy).ToList()
                             },
                         }).OrderBy(x => x.MaNhom).ToList();

            var khongtgia = dbSV.CTKhaoSats.Where(x => x.IDNV == MyAuthentication.ID && x.IDOT == 71).FirstOrDefault();

            var res1 = new List<OptionList>();
            List<OptionSelect> ops = new List<OptionSelect>();
            List<OptionSelect> ops1 = new List<OptionSelect>();
            List<OptionSelect> ops2 = new List<OptionSelect>();
            ops.Add(new OptionSelect { option = "Vợ", name = "Vợ" });
            ops.Add(new OptionSelect { option = "Chồng", name = "Chồng" });
            ops.Add(new OptionSelect { option = "Con", name = "Con" });
            ops.Add(new OptionSelect { option = "Khác", name = "Khác" });
            //ops1.Add(new OptionSelect { option = "Vợ", name = "Vợ" });
            ops1.Add(new OptionSelect { option = "Chồng", name = "Chồng" });
            // ops2.Add(new OptionSelect { option = "1", name = "Vợ" });
            // ops2.Add(new OptionSelect { option = "Chồng", name = "Chồng" });
            //// ops2.Add(new OptionSelect { option = "Con", name = "Con" });
            // //ops2.Add(new OptionSelect { option = "Khác", name = "Khác" });
            // ViewBag.GhiChu = new SelectList(ops2, "NoiDung", "NoiDung");
            ops2.Add(new OptionSelect { option = "1", name = "Nam" });
            ops2.Add(new OptionSelect { option = "2", name = "Nữ" });

            ViewBag.GioiTinh = new SelectList(ops2, "option", "name");
            ViewBag.LSOP = new SelectList(ops, "option", "name");
            ViewBag.LSOP1 = new SelectList(ops1, "option", "name");
            List<OptionSelect> opsLyDo = new List<OptionSelect>();
            opsLyDo.Add(new OptionSelect { option = "CBNV nữ đang nuôi con nhỏ < 12 tháng", name = "CBNV nữ đang nuôi con nhỏ < 12 tháng" });
            opsLyDo.Add(new OptionSelect { option = "CBNV nữ đang mang thai từ 25 tuần trở lên hoặc có bệnh lý thai kỳ", name = "CBNV nữ đang mang thai từ 25 tuần trở lên hoặc có bệnh lý thai kỳ" });
            opsLyDo.Add(new OptionSelect { option = "CBNV đang nằm viện điều trị", name = "CBNV đang nằm viện điều trị" });
            opsLyDo.Add(new OptionSelect { option = "CBNV bị tai nạn đang điều trị ngoại trú như: Gãy tay, chân, chấn thương nặng", name = "CBNV bị tai nạn đang điều trị ngoại trú như: Gãy tay, chân, chấn thương nặng" });
            opsLyDo.Add(new OptionSelect { option = "Lý do khác", name = "Lý do khác" });
            ViewBag.LyDo = new SelectList(opsLyDo, "option", "name");
            ViewBag.LyDoChon = khongtgia?.GhiChu;

            var IDNV = MyAuthentication.ID;
            var pb = db.PhongBans.ToList();
            var LSNV = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
            var dknt = dbSV.CTDKNguoiThans.ToList();
            var option = dbSV.OptionServeys.ToList();
            var ChiTietDKNT = dbSV.ChiTietDKNTs.ToList();
            var Diachi = dbSV.DiaDiems.ToList();

            List<GhiChu> nt = dbSV.GhiChus.ToList();
            ViewBag.GhiChu = new SelectList(nt, "NoiDung", "NoiDung");

            List<OptionServey> opt = dbSV.OptionServeys.Where(x => x.IDSV == IDSV).ToList();
            ViewBag.OptionLS = new SelectList(opt, "MaOT", "ContentOT");
            ViewBag.OptionLSID = opt;
            ViewBag.OptionLSCheck = new SelectList(opt, "IDOT", "MaOT");

            //list header
            List<string> columnHeaders = new List<string> { };
            List<string> columnHeaderRE = new List<string> { };
            List<int> ListIDgroup = new List<int> { };
            List<int> ListIDgroupRe = new List<int> { };
            List<int?> ListMagroup = new List<int?> { };
            var listND = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).ToList();
            foreach (var item in listND)
            {
                columnHeaders.Add(item.TenNhom);
                ListIDgroup.Add(item.ID);
                if (item.isShowRe != 0)
                {
                    columnHeaderRE.Add(item.TenNhom);
                    ListMagroup.Add(item.MaNhom);
                    ListIDgroupRe.Add(item.ID);
                }
            }
            ViewBag.ColumHeader = columnHeaders;
            ViewBag.ColumHeaderRe = columnHeaderRE;
            ViewBag.Magroup = ListMagroup;

            //ds đki nguoi cung cty
            var listCom = (from a in dknt.Where(x => x.IDNV == IDNV && x.isCom == 1 && x.IDSV == IDSV)
                           join b in LSNV on a.IDNguoiThan equals b.ID into ul
                           from b in ul.DefaultIfEmpty()
                           select new PartTogetherValidation
                           {
                               ID = a.ID,
                               HoTen = b.HoTen,
                               MaNV = b.MaNV,
                               IDNguoiThan = a.IDNguoiThan,
                               IDSV = a.IDSV,
                               PhongBan = pb.Where(x => x.IDPhongBan == b.IDPhongBan).FirstOrDefault().TenPhongBan,
                               NamSinh = a.NamSinh,
                               QuanHe = a.QuanHe,
                               Note = a.GhiChu
                               //ListSelect = 
                               //GioiTinhStr = a.GioiTinh ==1 &&  ?"Nam":"Nữ",
                               //TenNhom = a.TenNhom,
                               //CuLy = ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 1).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 1).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                               //MauAo = ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 2).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 2).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                               //Size = ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 3).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 3).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                               //DiaChi = Diachi.Where(x => x.ID == a.IDDC).FirstOrDefault().TenDC,
                           }).ToList();
            foreach (var item in listCom)
            {
                List<string> columnSelectCom = new List<string> { };
                foreach (var sl in ListIDgroupRe)
                {
                    var ctksNV = ChiTietDKNT.Where(x => x.IDNguoiThan == item.ID && x.IDSV == IDSV && x.IDGroup == sl).ToList();
                    if (ctksNV.Count() != 0)
                    {
                        var ot = option.Where(x => x.IDOT == ctksNV.FirstOrDefault().IDOT).FirstOrDefault().ContentOT;
                        columnSelectCom.Add(ot);
                    }
                    else
                    {
                        columnSelectCom.Add("");
                    }
                }
                item.ListSelect = columnSelectCom;
            }

            ViewBag.DSCty = listCom;

            //ds đki người khác cty
            var listKhac = (from a in dknt.Where(x => x.IDNV == IDNV && x.isCom == 0 && x.IDSV == IDSV)
                            select new PartTogetherValidation
                            {
                                ID = a.ID,
                                HoTen = a.HoTen,
                                DienThoai = a.DienThoai,
                                GioiTinhStr = a.GioiTinh == 1 ? "Nam" : "Nữ",
                                IDNguoiThan = a.IDNguoiThan,
                                IDSV = a.IDSV,
                                NamSinh = a.NamSinh,
                                QuanHe = a.QuanHe,
                                Note = a.GhiChu
                                //PhongBan = db.PhongBans.Where(x => x.IDPhongBan == b.IDPhongBan).FirstOrDefault().TenPhongBan,
                                //GioiTinhStr = a.GioiTinh ==1 &&  ?"Nam":"Nữ",
                                //TenNhom = a.TenNhom,
                                //CuLy = dbSV.OptionServeys.Where(x=>x.ID)
                                //CuLy = ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 1).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 1).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                                //MauAo = ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 2).FirstOrDefault() != null? option.Where(x => x.IDSV == IDSV && x.IDOT == ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 2).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                                //Size = ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 3).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ChiTietDKNT.Where(m => m.IDSV == IDSV && m.IDNguoiThan == a.ID && m.IDGroup == 3).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                                //DiaChi = Diachi.Where(x => x.ID == a.IDDC ).FirstOrDefault().TenDC,
                            }).ToList();
            foreach (var item in listKhac)
            {
                List<string> columnSelectCom = new List<string> { };
                foreach (var sl in ListIDgroupRe)
                {
                    var ctksNV = ChiTietDKNT.Where(x => x.IDNguoiThan == item.ID && x.IDSV == IDSV && x.IDGroup == sl).ToList();
                    if (ctksNV.Count() != 0)
                    {
                        var ot = option.Where(x => x.IDOT == ctksNV.FirstOrDefault().IDOT).FirstOrDefault().ContentOT;
                        columnSelectCom.Add(ot);
                    }
                    else
                    {
                        columnSelectCom.Add("");
                    }
                }
                item.ListSelect = columnSelectCom;
            }


            ViewBag.DSKhac = listKhac;



            List<PhongBan> dt = db.PhongBans.ToList();
            ViewBag.IDPB = new SelectList(dt, "IDPhongBan", "TenPhongBan");

            List<DiaDiem> dc = dbSV.DiaDiems.ToList();
            ViewBag.IDDC = new SelectList(dc, "ID", "TenDC", employ.IDDC);



            var ListNV = new List<EmployeeValidation>();

            var kk = dbSV.EmployeeServeys.Where(x => x.IDSV == IDSV).ToList();
            var LS = (from a in kk
                      join b in LSNV on a.IDNV equals b.ID
                      select b).ToList();

            if (LS.Count > 0)
            {
                foreach (var item in LS)
                {
                    ListNV.Add(new EmployeeValidation { ID = item.ID, HoTen = item.MaNV + " - " + item.HoTen });
                }

            }
            //thong tin dk
            var employdk = dbSV.EmployeeServeys.Where(x => x.IDNV == IDNV && x.IDSV == IDSV).ToList();

            var thongtindk = (from a in employdk.Where(x => x.OTID != null)
                              select new PartTogetherValidation
                              {
                                  IDSV = a.IDSV,
                                  //PhongBan = db.PhongBans.Where(x => x.IDPhongBan == b.IDPhongBan).FirstOrDefault().TenPhongBan,
                                  //GioiTinhStr = a.GioiTinh ==1 &&  ?"Nam":"Nữ",
                                  //TenNhom = a.TenNhom,
                                  //CuLy = dbSV.OptionServeys.Where(x=>x.ID)
                                  //CuLy = ctks.Where(m => m.IDSV == IDSV && m.IDNV == a.IDNV && m.IDGroup == 1).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ctks.Where(m => m.IDSV == IDSV && m.IDNV == a.IDNV && m.IDGroup == 1).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                                  //MauAo = ctks.Where(m => m.IDSV == IDSV && m.IDNV == a.IDNV && m.IDGroup == 2).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ctks.Where(m => m.IDSV == IDSV && m.IDNV == a.IDNV && m.IDGroup == 2).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                                  //Size = ctks.Where(m => m.IDSV == IDSV && m.IDNV == a.IDNV && m.IDGroup == 3).FirstOrDefault() != null ? option.Where(x => x.IDSV == IDSV && x.IDOT == ctks.Where(m => m.IDSV == IDSV && m.IDNV == a.IDNV && m.IDGroup == 3).FirstOrDefault().IDOT).FirstOrDefault().ContentOT : "",
                                  //DiaChi = Diachi.Where(x => x.ID == a.IDDC).FirstOrDefault().TenDC,
                              }).FirstOrDefault();
            ViewBag.ThongTinDK = thongtindk;



            List<string> columnSelect = new List<string> { };
            foreach (var item in listND)
            {
                var ctksNV = ctks.Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.IDGroup == item.ID).ToList();
                if (ctksNV.Count() != 0)
                {
                    var ot = option.Where(x => x.IDOT == ctksNV.FirstOrDefault().IDOT).FirstOrDefault().ContentOT;
                    columnSelect.Add(ot);
                    //if (ctksNV.FirstOrDefault().IDOT == 71) columnSelect.Add(ctksNV.FirstOrDefault().GhiChu);
                }
                else
                {
                    columnSelect.Add("");
                }
            }
            var ctksNVend = ctks.Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.IDOT == 71).ToList();
            if (ctksNVend.Count() != 0)
            {
                columnSelect.Add(ctksNVend.FirstOrDefault().GhiChu);
            }
            ViewBag.columnSelect = columnSelect;



            ViewBag.IDNV = new SelectList(ListNV, "ID", "HoTen");

            // Tìm survey tiếp theo trong danh sách đang hoạt động của user
            var allActiveSV2 = dbSV.EmployeeServeys
                .Where(x => x.IDNV == IDNV)
                .Select(x => x.IDSV)
                .ToList();
            var activeSorted2 = dbSV.ListServeys
                .Where(x => allActiveSV2.Contains(x.IDSV)
                         && x.StartTime <= DateTime.Now
                         && x.EndTime   >= DateTime.Now
                         && x.StatusSV  == true)
                .OrderBy(x => x.StartTime).ThenBy(x => x.IDSV)
                .Select(x => x.IDSV)
                .ToList();
            int currentIdx2 = activeSorted2.IndexOf((int)IDSV);
            int? nextIDSV2  = (currentIdx2 >= 0 && currentIdx2 < activeSorted2.Count - 1)
                              ? activeSorted2[currentIdx2 + 1]
                              : (int?)null;
            ViewBag.NextIDSV = nextIDSV2;

            return View(group.ToList());
        }
        [HttpPost]
        public ActionResult Confirm(List<GroupKhaoSatView> ListGR, FormCollection collection)
        {
            //var kq = ListSV.FirstOrDefault();
            var IDSV = ListGR[0].IDSV;
            var IDDC = ListGR[0].IDDC;
            try
            {
                var a = ListGR;

                var gr = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).ToList();
                var grChoose = gr.Where(x => x.isChon != 0).ToList();
                dbSV.CTKhaoSat_delete(ListGR[0].IDSV, MyAuthentication.ID);
                var keysToRemove = new List<string>();
                var keysSelectChoose = new List<string>();
                foreach (var item in gr)
                {
                    var sl = collection["gr.OptionList.Answer[" + item.ID + "]"];
                    if (sl != null)
                    {
                        keysToRemove.Add(sl);
                    }
                    if (item.isChon != 0 && sl != null)
                    {
                        keysSelectChoose.Add(sl);
                    }
                }
                if (keysSelectChoose.Count != 0 || keysSelectChoose.Count >= grChoose.Count() && !keysToRemove.Contains("71") || keysToRemove.Contains("71"))
                { // 25 OTID server lựa chọn 0
                    foreach (var key in gr)
                    {
                        var sl = collection["gr.OptionList.Answer[" + key.ID + "]"];
                        if (sl != null)
                        {
                            dbSV.EmployeeServey_updateOT(MyAuthentication.ID, ListGR[0].IDSV, 0);
                            dbSV.CTKhaoSat_insert(ListGR[0].IDSV, int.Parse(sl), MyAuthentication.ID, key.ID);
                            int IDOT = int.Parse(sl);
                            int idsv = int.Parse(ListGR[0].IDSV.ToString());
                            var ketqua = dbSV.CTKhaoSats.Where(x => x.IDSV == idsv && x.IDOT == IDOT && x.IDNV == MyAuthentication.ID && x.IDGroup == key.ID).FirstOrDefault();
                            if (ketqua.IDOT == 71)
                            {
                                ketqua.GhiChu = collection["LyDo"];
                                dbSV.SaveChanges();
                            }
                        }
                    }
                    // check trường hợp chọn không xóa kết quả
                    var kk = dbSV.CTKhaoSats.Where(x => x.IDOT == 71 && x.IDSV == IDSV && x.IDNV == MyAuthentication.ID).ToList(); // thay đổi ID
                    if (kk.Count != 0)
                    {
                        var recordToDelete = dbSV.CTKhaoSats.Where(x => x.IDOT != 71 && x.IDSV == IDSV && x.IDNV == MyAuthentication.ID).ToList(); // thay đổi ID
                        var recordToDelete2 = dbSV.CTDKNguoiThans.Where(x => x.IDSV == IDSV && x.IDNV == MyAuthentication.ID).ToList();
                        dbSV.CTKhaoSats.RemoveRange(recordToDelete);
                        dbSV.CTDKNguoiThans.RemoveRange(recordToDelete2);
                        dbSV.SaveChanges();
                    }
                    if (IDDC != null)
                    {
                        //dbSV.EmployeeServey_updateOtion(MyAuthentication.ID, IDSV, IDDC,);
                        dbSV.EmployeeServey_updateDC(MyAuthentication.ID, IDSV, IDDC);
                    }

                    // thêm người thân cùng cty
                    var ListVT = new List<DKNguoiThan>();
                    var ListSelc = new List<int>();
                    var ListVTKhac = new List<DKNguoiThan>();
                    var ListSelcKhac = new List<int>();

                    foreach (var key in collection.AllKeys)
                    {
                        if (key.Split('_')[0] == "IDNV")
                        {
                            if (collection[key] != "null")
                            {
                                ListVT.Add(new DKNguoiThan() { IDNguoiThan = int.Parse(collection[key]), QuanHe = collection["reCung_" + key.Split('_')[1]], GhiChu = collection["noteCung_" + key.Split('_')[1]] });
                            }


                        }
                        if (key.Split('_')[0] == "nameKhac")
                        {
                            if (collection[key] != "")
                            {
                                ListVTKhac.Add(new DKNguoiThan() { HoTen = collection[key], QuanHe = collection["reKhac_" + key.Split('_')[1]], DienThoai = collection["sdtKhac_" + key.Split('_')[1]], GioiTinh = int.Parse(collection["gioitinhKhac_" + key.Split('_')[1]]), NamSinh = collection["nsKhac_" + key.Split('_')[1]], GhiChu = collection["noteKhac_" + key.Split('_')[1]] });
                            }

                        }
                        if (key.Split('_')[0] == "slectCung")
                        {
                            if (collection[key] != "null" && collection[key] != null)
                            {
                                ListSelc.Add(int.Parse(collection[key]));
                            }
                        }
                        if (key.Split('_')[0] == "slectKhac")
                        {
                            if (collection[key] != "null" && collection[key] != null)
                            {
                                ListSelcKhac.Add(int.Parse(collection[key]));
                            }
                        }

                    }
                    foreach (var item in ListVT)
                    {
                        ObjectParameter IDNguoiThan = new ObjectParameter("ID", typeof(int));
                        dbSV.CTDKNguoiThan_insert(MyAuthentication.ID, item.IDNguoiThan, null, null, IDSV, 1, null, null, null, item.QuanHe, item.GhiChu, IDNguoiThan);
                        int IDNT = 0;
                        IDNT = Convert.ToInt32(IDNguoiThan.Value);
                        dbSV.ChiTietDKNT_delete(IDNT);
                        if (ListSelc.Count > 0)
                        {
                            foreach (var ot in ListSelc)
                            {
                                var sls = dbSV.OptionServeys.Where(x => x.IDOT == ot).FirstOrDefault();
                                dbSV.ChiTietDKNT_insert(IDSV, ot, IDNT, sls.MaOT);
                            }
                        }

                    }
                    // thêm người thân khác cty

                    foreach (var item in ListVTKhac)
                    {
                        ObjectParameter IDNguoiThan = new ObjectParameter("ID", typeof(int));
                        DateTime dateTime;
                        if (DateTime.TryParse(item.NamSinh, out dateTime))
                        {
                            item.NamSinh = dateTime.ToString("dd/MM/yyyy");
                        }
                        dbSV.CTDKNguoiThan_insert(MyAuthentication.ID, null, item.HoTen, item.DienThoai, IDSV, 0, item.GioiTinh, null, item.NamSinh.ToString(), item.QuanHe, item.GhiChu, IDNguoiThan);
                        int IDNT = 0;
                        IDNT = Convert.ToInt32(IDNguoiThan.Value);
                        dbSV.ChiTietDKNT_delete(IDNT);
                        if (ListSelcKhac.Count > 0)
                        {
                            foreach (var ot in ListSelcKhac)
                            {
                                var sls = dbSV.OptionServeys.Where(x => x.IDOT == ot).FirstOrDefault();
                                dbSV.ChiTietDKNT_insert(IDSV, ot, IDNT, sls.MaOT);
                            }
                        }

                    }


                    TempData["msgSuccess"] = "<script>alert('Đăng ký thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn đầy đủ thông tin đăng ký?');</script>";
                }



            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi gửi: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ServeyID", new { IDSV = IDSV });
        }


        public ActionResult Edit(int? id)
        {
            //var listDB = db.DinhBienVTs.ToList();
            var pb = db.PhongBans.ToList();
            var LSNV = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
            var dknt = dbSV.CTDKNguoiThans.Where(x => x.ID == id).FirstOrDefault();
            var option = dbSV.OptionServeys.ToList();
            var ChiTietDKNT = dbSV.ChiTietDKNTs.Where(x => x.IDNguoiThan == id).ToList();
            var Diachi = dbSV.DiaDiems.ToList();
            var res = (from a in dbSV.CTDKNguoiThans.Where(x => x.ID == id)
                       select new PartTogetherValidation
                       {
                           ID = a.ID,
                           HoTen = a.HoTen,
                           DienThoai = a.DienThoai,
                           GioiTinhStr = a.GioiTinh == 1 ? "Nam" : "Nữ",
                           GioiTinh = a.GioiTinh,
                           IDNguoiThan = a.IDNguoiThan,

                       }).FirstOrDefault();

            var listCuLy = option.Where(x => x.IDSV == dknt.IDSV && x.MaOT == 1).ToList();
            var listMau = option.Where(x => x.IDSV == dknt.IDSV && x.MaOT == 2).ToList();
            var listSize = option.Where(x => x.IDSV == dknt.IDSV && x.MaOT == 3).ToList();
            var idcly = ChiTietDKNT.Where(x => x.IDGroup == 1).FirstOrDefault().IDOT;
            var idMau = ChiTietDKNT.Where(x => x.IDGroup == 2).FirstOrDefault().IDOT;
            var idSize = ChiTietDKNT.Where(x => x.IDGroup == 3).FirstOrDefault().IDOT;
            ViewBag.LSCuLy = new SelectList(listCuLy, "IDOT", "ContentOT", idcly);
            ViewBag.IDMau = new SelectList(listMau, "IDOT", "ContentOT", idMau);
            ViewBag.IDSize = new SelectList(listCuLy, "IDOT", "ContentOT", idSize);
            return PartialView(res);
        }

        public ActionResult XNServey(int? IDNV, int? IDSV)
        {
            try
            {
                var check = dbSV.EmployeeServey_selectNV(IDNV, IDSV).First();
                if (check != null)
                {
                    var a = dbSV.EmployeeServey_updateOtion(check.IDNV, check.IDSV, check.OTID, true, check.MenuOT);
                    TempData["msgSuccess"] = "<script>alert('Hoàn thành khảo sát');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "UserServey");
        }

        public ActionResult CreateRelation(int? IDSV)
        {
            DateTime ts = DateTime.Now;
            ts = new DateTime(ts.Year, ts.Month, ts.Day, 0, 0, 0);
            var res = (from a in dbSV.OptionServeys.Where(x => x.IDSV == IDSV)
                       select new OptionValidation
                       {
                           IDOT = a.IDOT,
                           ContentOT = a.ContentOT,
                           FilePath = a.FilePath,
                           OrderBy = a.OrderBy,
                           IDSV = (int)a.IDSV,
                           MaOT = a.MaOT,
                       }).OrderBy(x => x.OrderBy).ToList(); //ds option


            var aa = res.Count > 0 ? res[0].ContentSV : null;

            var employ = dbSV.EmployeeServeys.ToList();
            var ctks = dbSV.CTKhaoSats.ToList();
            ViewBag.TenDK = dbSV.ListServeys.Where(x => x.IDSV == IDSV).FirstOrDefault().ContentSV;
            var group = (from a in dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV)
                         select new GroupKhaoSatView
                         {
                             ID = a.ID,
                             IDSV = a.IDSV,
                             MaNhom = a.MaNhom,
                             TenNhom = a.TenNhom,
                             IsChecked = true,
                             OptionList = new OptionList
                             {
                                 ID = a.ID,
                                 //IDNV = aaa.IDNV,
                                 Answer = (int?)dbSV.CTKhaoSats.Where(x => x.IDSV == IDSV && x.IDNV == MyAuthentication.ID && x.IDGroup == a.ID).FirstOrDefault().IDOT ?? default,
                                 //ContentSV = aa,

                                 //XNSV = a.XNSV,
                                 //IDSV = IDSV,
                                 //Status = dbSV.ListServeys.Where(x => x.StartTime <= ts && x.EndTime >= ts).ToList().Count() > 0 ? true : false,
                                 OptionLS = (from ks in dbSV.OptionServeys.Where(x => x.IDSV == IDSV && x.MaOT == a.MaNhom)
                                             select new OptionValidation
                                             {
                                                 IDOT = ks.IDOT,
                                                 ContentOT = ks.ContentOT,
                                                 FilePath = ks.FilePath,
                                                 OrderBy = ks.OrderBy,
                                                 IDSV = (int)a.IDSV,
                                                 MaOT = ks.MaOT,
                                                 isShow = ks.isShow
                                             }).OrderBy(x => x.OrderBy).ToList()
                                 //MenuOT = a.MenuOT,
                             }
                         }).OrderBy(x => x.MaNhom).ToList();

            var res1 = new DKNguoiThan();
            res1.listGroup = group;

            List<OptionSelect> ops = new List<OptionSelect>();
            List<OptionSelect> ops2 = new List<OptionSelect>();
            ops.Add(new OptionSelect { option = "1", name = "Nam" });
            ops.Add(new OptionSelect { option = "2", name = "Nữ" });
            //ops.Add(new OptionSelect { option = "Khác", name = "Khác" });
            //ops2.Add(new OptionSelect { option = "Vợ", name = "Vợ" });
            //ops2.Add(new OptionSelect { option = "Chồng", name = "Chồng" });
            //ops2.Add(new OptionSelect { option = "Con", name = "Con" });
            //ops2.Add(new OptionSelect { option = "Khác", name = "Khác" });

            ViewBag.LSOP = new SelectList(ops, "option", "name");
            ViewBag.LSOP2 = new SelectList(ops2, "option", "name");
            List<PhongBan> dt = db.PhongBans.ToList();
            ViewBag.IDPB = new SelectList(dt, "IDPhongBan", "TenPhongBan");
            List<DiaDiem> dc = dbSV.DiaDiems.ToList();
            ViewBag.IDDC = new SelectList(dc, "ID", "TenDC");
            var ListNV = new List<EmployeeValidation>();
            var LSNV = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
            var kk = dbSV.EmployeeServeys.ToList();

            List<GhiChu> nt = dbSV.GhiChus.Where(x => x.IDSV == IDSV).ToList();
            ViewBag.GhiChu = new SelectList(nt, "NoiDung", "NoiDung");

            var LS = (from a in kk
                      join b in LSNV on a.IDNV equals b.ID
                      select b).ToList();

            if (LS.Count > 0)
            {
                foreach (var item in LSNV)
                {
                    ListNV.Add(new EmployeeValidation { ID = item.ID, HoTen = item.MaNV + " - " + item.HoTen });
                }

            }
            ViewBag.IDNV = new SelectList(ListNV, "ID", "HoTen");
            //ViewBag.LSPart = PartTogether(MyAuthentication.ID).ToList();
            return View(res1);
        }

        [HttpPost]
        public ActionResult ConfirmRe(DKNguoiThan ListGR, FormCollection collection)
        {
            //var kq = ListSV.FirstOrDefault();
            var IDSV = 0;
            //var IDDC = ListGR[0].IDDC;
            try
            {
                var a = ListGR;
                ObjectParameter IDNguoiThan = new ObjectParameter("ID", typeof(int));
                int IDNT = 0;

                var gr = dbSV.GroupKhaoSats.Where(x => x.IDSV == ListGR.IDSV).ToList();

                var keysToRemove = new List<string>();
                foreach (var item in gr)
                {
                    var sl = collection["gr.OptionList.Answer[" + item.ID + "]"];
                    if (sl != null)
                    {
                        keysToRemove.Add(sl);
                    }
                }
                if (keysToRemove.Count == gr.Count() || true)
                {
                    DateTime dateTime;
                    if (DateTime.TryParse(ListGR.NamSinh, out dateTime))
                    {
                        ListGR.NamSinh = dateTime.ToString("dd/MM/yyyy");
                    }

                    if (ListGR.IsChecked == true)
                    {
                        dbSV.CTDKNguoiThan_insert(MyAuthentication.ID, ListGR.IDNV, null, null, ListGR.IDSV, 1, ListGR.GioiTinh, ListGR.IDDC, ListGR.NamSinh.ToString(), ListGR.QuanHe, ListGR.GhiChu, IDNguoiThan);
                    }
                    else
                    {
                        if (ListGR.HoTen != "" && ListGR.DienThoai != "")
                        {
                            dbSV.CTDKNguoiThan_insert(MyAuthentication.ID, null, ListGR.HoTen, ListGR.DienThoai, ListGR.IDSV, 0, ListGR.GioiTinh, ListGR.IDDC, ListGR.NamSinh.ToString(), ListGR.QuanHe, ListGR.GhiChu, IDNguoiThan);
                        }
                    }
                    IDNT = Convert.ToInt32(IDNguoiThan.Value);
                    dbSV.ChiTietDKNT_delete(IDNT);
                    foreach (var key in gr)
                    {
                        var sl = collection["gr.OptionList.Answer[" + key.ID + "]"];
                        if (sl != null)
                        {

                            dbSV.ChiTietDKNT_insert(ListGR.IDSV, int.Parse(sl), IDNT, key.ID);
                        }
                    }
                    TempData["msgSuccess"] = "<script>alert('Đăng ký thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn đầy đủ thông tin đăng ký?');</script>";
                }


            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi gửi: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ServeyID", new { IDSV = ListGR.IDSV });
        }



        public JsonResult AddRe(string HoTen, DateTime? NS, int? IDNV, string Re, bool isCom, string note, int? IDSV, int? answer)
        {
            //var a = dbSV.EmployeeServey_selectNV(MyAuthentication.ID, IDSV).First().OTID;
            if (isCom == true)
            {
                if (answer != 4 && HoTen != "") dbSV.PartTogether_insert(null, HoTen, NS, MyAuthentication.ID, Re, false, note);
            }
            else
            {
                var check = dbSV.PartTogethers.Where(x => x.IDNV == IDNV && x.IDESV == MyAuthentication.ID).ToList();
                if (check.Count == 0 && IDNV != null && answer != 4) dbSV.PartTogether_insert(IDNV, null, null, MyAuthentication.ID, Re, true, note);
            }
            //RedirectToAction("Index", "ViTriKNL");
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public List<PartTogetherValidation> PartTogether(int? ESVID)
        {
            var LSPart = dbSV.PartTogethers.Where(x => x.IDESV == ESVID).ToList();
            var LSNV = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
            List<PartTogetherValidation> Ls = (from a in LSPart
                                               join b in LSNV on a.IDNV equals b.ID into ulk
                                               from b in ulk.DefaultIfEmpty()
                                               select new PartTogetherValidation
                                               {
                                                   ID = a.ID,
                                                   HoTen = a.HoTen,
                                                   IDESV = a.IDESV,
                                                   IDNV = a.IDNV,
                                                   MaNV = a.IDNV != null ? b.MaNV : "",
                                                   HoTenNV = a.IDNV != null ? b.HoTen : "",
                                                   //NamSinh =(DateTime?)a.NamSinh??default,
                                                   Re = a.Re,
                                                   isCom = a.isCom,
                                                   TenNV = a.IDNV != null ? b.MaNV + "-" + b.HoTen : "",
                                                   PhongBan = a.IDNV != null ? b.PhongBan.TenPhongBan : "",
                                                   Note = a.Note
                                               }).ToList();
            return Ls;
        }
        public ActionResult Delete(int? id, int? IDSV)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                dbSV.ChiTietDKNT_delete(id);
                dbSV.CTDKNguoiThan_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ServeyID", new { IDSV = IDSV });
        }

        public ActionResult DeleteDK(int? IDSV)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                var listNT = dbSV.CTDKNguoiThans.Where(x => x.IDSV == IDSV && x.IDNV == MyAuthentication.ID).ToList();
                if (listNT != null)
                {
                    foreach (var item in listNT)
                    {
                        dbSV.ChiTietDKNT_delete(item.ID);
                        dbSV.CTDKNguoiThan_delete(item.ID);
                    }
                }
                dbSV.CTKhaoSat_delete(IDSV, MyAuthentication.ID);
                dbSV.EmployeeServey_updateDC(MyAuthentication.ID, IDSV, null);
                dbSV.EmployeeServey_updateOT(MyAuthentication.ID, IDSV, null);
                //dbSV.ChiTietDKNT_delete(id);
                //dbSV.CTDKNguoiThan_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "UserServey");
        }


        public ActionResult DeletePart(int? id, int? IDSV)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                dbSV.PartTogether_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ServeyID", new { IDSV = IDSV });
        }

        // ─── PICKLEBALL ──────────────────────────────────────────────────────────

        private string GetLoaiDoi(string tenNhom)
        {
            var t = (tenNhom ?? "").ToLower();
            if (t.Contains("hỗn hợp") || t.Contains("nam nữ") || t.Contains("nam-nữ") || t.Contains("mix"))
                return "HonHop";
            if (t.Contains("nữ"))
                return "DoiNu";
            if (t.Contains("nam"))
                return "DoiNam";
            return "All";
        }

        public ActionResult IndexDongDoi(int? IDSV)
        {
            var IDNV = MyAuthentication.ID;
            var currentUser = db.NhanViens.FirstOrDefault(x => x.ID == IDNV);
            var groups   = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).OrderBy(x => x.MaNhom).ToList();
            var options  = dbSV.OptionServeys.Where(x => x.IDSV == IDSV).ToList();
            var LSNV     = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
            var pb       = db.PhongBans.ToList();
            var servey = dbSV.ListServeys.FirstOrDefault(x => x.IDSV == IDSV);

            var myPairs = dbSV.CTDKNguoiThans
                .Where(x => x.IDNV == IDNV  && x.IDSV == IDSV && x.isCom == 1)
                .ToList();

            var myPairsView = (from a in myPairs
                               join b in LSNV on a.IDNguoiThan equals b.ID into ul
                               from b in ul.DefaultIfEmpty()
                               select new PartTogetherValidation
                               {
                                   ID          = a.ID,
                                   HoTen       = b != null ? b.HoTen : "",
                                   MaNV        = b != null ? b.MaNV  : "",
                                   IDNguoiThan = a.IDNguoiThan,
                                   IDSV        = a.IDSV,
                                   PhongBan    = b != null ? pb.FirstOrDefault(x => x.IDPhongBan == b.IDPhongBan)?.TenPhongBan : "",
                                   QuanHe      = a.QuanHe,  // LoaiDoi
                                   Note        = a.GhiChu   // TenNhom
                               }).ToList();

            var registeredLoaiDoi = myPairs.Select(x => x.QuanHe).ToList();

            // Solo registrations: isShow=0, saved only to CTKhaoSats (no partner)
            var soloCtKS = dbSV.CTKhaoSats
                .Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.IDGroup != null)
                .ToList();
            var soloView = (from a in soloCtKS
                            join g in groups on a.IDGroup equals g.ID into gg
                            from g in gg.DefaultIfEmpty()
                            let loaiDoiS = GetLoaiDoi(g != null ? g.TenNhom ?? "" : "")
                            where g != null && !registeredLoaiDoi.Contains(loaiDoiS)
                            select new PartTogetherValidation
                            {
                                ID          = a.ID,
                                Note        = g.TenNhom ?? "",
                                QuanHe      = loaiDoiS,
                                IDSV        = IDSV,
                                IDNguoiThan = null,
                            }).ToList();
            myPairsView.AddRange(soloView);

            var allRegisteredLoaiDoi = registeredLoaiDoi
                .Union(soloView.Select(x => x.QuanHe))
                .ToList();

            var groupViews = groups.Select(g =>
            {
                string loaiDoi = GetLoaiDoi(g.TenNhom ?? "");
                return new PickleballGroupView
                {
                    IDGroup      = g.ID,
                    IDSV         = g.IDSV ?? 0,
                    TenNhom      = g.TenNhom,
                    LoaiDoi      = loaiDoi,
                    IsRegistered = allRegisteredLoaiDoi.Contains(loaiDoi),
                    ExistingPair = myPairsView.FirstOrDefault(p => p.QuanHe == loaiDoi),
                    Options      = options.Where(o => o.MaOT == g.MaNhom)
                                         .OrderBy(o => o.OrderBy)
                                         .Select(o => new OptionValidation
                                         {
                                             IDOT      = o.IDOT,
                                             ContentOT = o.ContentOT,
                                             isShow    = o.isShow
                                         }).ToList()
                };
            }).ToList();

            // Ẩn nhóm không phù hợp giới tính: Nam chỉ thấy DoiNam + HonHop, Nữ chỉ thấy DoiNu + HonHop
            int? userGioiTinh = currentUser?.IsGioiTinh;
            if (userGioiTinh.HasValue && servey.ContentSV.Contains("Pickleball"))
            {
                groupViews = groupViews.Where(g =>
                    g.LoaiDoi == "HonHop" ||
                    (userGioiTinh == 0 && g.LoaiDoi == "DoiNam") ||
                    (userGioiTinh == 1 && g.LoaiDoi == "DoiNu")
                ).ToList();
            }

            // Những người khác đã chọn mình làm đồng đội
            var registeredByOthers = dbSV.CTDKNguoiThans
                .Where(x => x.IDNguoiThan == IDNV && x.IDSV == IDSV && x.isCom == 1)
                .ToList();

            var registeredByOthersView = (from a in registeredByOthers
                                          join b in LSNV on a.IDNV equals b.ID into ul
                                          from b in ul.DefaultIfEmpty()
                                          select new PartTogetherValidation
                                          {
                                              ID       = a.ID,
                                              HoTen    = b != null ? b.HoTen : "",
                                              MaNV     = b != null ? b.MaNV  : "",
                                              IDSV     = a.IDSV,
                                              PhongBan = b != null ? pb.FirstOrDefault(x => x.IDPhongBan == b.IDPhongBan)?.TenPhongBan : "",
                                              QuanHe   = a.QuanHe,
                                              Note     = a.GhiChu
                                          }).ToList();

            // Tìm survey tiếp theo trong danh sách đang hoạt động của user
            var allActiveSV = dbSV.EmployeeServeys
                .Where(x => x.IDNV == IDNV)
                .Select(x => x.IDSV)
                .ToList();
            var activeSorted = dbSV.ListServeys
                .Where(x => allActiveSV.Contains(x.IDSV)
                         && x.StartTime <= DateTime.Now
                         && x.EndTime   >= DateTime.Now
                         && x.StatusSV  == true)
                .OrderBy(x => x.StartTime).ThenBy(x => x.IDSV)
                .Select(x => x.IDSV)
                .ToList();
            int currentIdx = activeSorted.IndexOf((int)IDSV);
            int? nextIDSV  = (currentIdx >= 0 && currentIdx < activeSorted.Count - 1)
                             ? activeSorted[currentIdx + 1]
                             : (int?)null;

            ViewBag.IDSV                = IDSV;
            ViewBag.TenDK               = dbSV.ListServeys.FirstOrDefault(x => x.IDSV == IDSV)?.ContentSV;
            ViewBag.SlotsRemaining      = 2 - myPairsView.Count;
            ViewBag.MyPairs             = myPairsView;
            ViewBag.CurrentUserGioiTinh = userGioiTinh;
            ViewBag.RegisteredByOthers  = registeredByOthersView;
            ViewBag.NextIDSV            = nextIDSV;

            return View(groupViews);
        }

        public JsonResult GetDongDoiPartners(int IDSV, int IDGroup)
        {
            var IDNV        = MyAuthentication.ID;
            var currentUser = db.NhanViens.FirstOrDefault(x => x.ID == IDNV);
            var group       = dbSV.GroupKhaoSats.FirstOrDefault(x => x.ID == IDGroup);
            string loaiDoi  = GetLoaiDoi(group?.TenNhom ?? "");

            var takenPartnerIDs = dbSV.CTDKNguoiThans
                .Where(x => x.IDSV == IDSV && x.isCom == 1 && x.IDNguoiThan != null)
                .Select(x => x.IDNguoiThan.Value)
                .ToList();

            var registrantIDs = dbSV.CTDKNguoiThans
                .Where(x => x.IDSV == IDSV && x.isCom == 1 && x.IDNV != null)
                .Select(x => x.IDNV.Value)
                .Distinct()
                .ToList();

            var excludedIDs = takenPartnerIDs.Union(registrantIDs).Distinct().ToList();
            excludedIDs.Add(IDNV);

            // Load về memory trước vì IsGioiTinh chưa được map trong EDMX
            var candidates = db.NhanViens
                .Where(x => x.IDTinhTrangLV == 1)
                .ToList()
                .Where(x => !excludedIDs.Contains(x.ID));

            if (loaiDoi == "DoiNam")
                candidates = candidates.Where(x => x.IsGioiTinh == 0);
            else if (loaiDoi == "DoiNu")
                candidates = candidates.Where(x => x.IsGioiTinh == 1);
            else if(loaiDoi == "HonHop") // HonHop: chọn giới tính ngược với người đăng ký
            {
                int? oppositeGender = currentUser?.IsGioiTinh == 0 ? (int?)1 : 0;
                candidates = candidates.Where(x => x.IsGioiTinh == oppositeGender);
            }

            var result = candidates
                .OrderBy(x => x.HoTen)
                .Select(x => new { x.ID, HoTen = x.MaNV + " - " + x.HoTen })
                .ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmDongDoi(FormCollection collection)
        {
            int IDSV = int.Parse(collection["IDSV"]);
            int IDNV = MyAuthentication.ID;

            try
            {
                var currentUser = db.NhanViens.FirstOrDefault(x => x.ID == IDNV);
                var groups      = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).ToList();

                var existingPairs     = dbSV.CTDKNguoiThans
                    .Where(x => (x.IDNV == IDNV || x.IDNguoiThan == IDNV) && x.IDSV == IDSV && x.isCom == 1).ToList();
                var registeredLoaiDoi = existingPairs.Select(x => x.QuanHe).ToList();

                // Thu thập lựa chọn mới từ form
                var newItems = new List<Tuple<GroupKhaoSat, int, int?>>();

                foreach (var group in groups)
                {
                    string loaiDoi = GetLoaiDoi(group.TenNhom ?? "");
                    if (registeredLoaiDoi.Contains(loaiDoi)) continue; // đã đăng ký nội dung này rồi

                    string selectedOT = collection["answer_" + group.ID];
                    if (selectedOT == null) continue;

                    int idot   = int.Parse(selectedOT);
                    var option = dbSV.OptionServeys.FirstOrDefault(x => x.IDOT == idot);
                    if (option == null) continue;

                    int? idPartner = null;
                    if (option.isShow == 1)
                    {
                        string selectedPartner = collection["partner_" + group.ID];
                        if (string.IsNullOrEmpty(selectedPartner))
                        {
                            TempData["msgError"] = "<script>alert('Vui lòng chọn đồng đội cho nội dung: " + group.TenNhom + "');</script>";
                            return RedirectToAction("IndexDongDoi", new { IDSV });
                        }
                        idPartner = int.Parse(selectedPartner);
                    }
                    newItems.Add(Tuple.Create(group, idot, idPartner));
                }

                if (newItems.Count == 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Không có nội dung mới nào được chọn');</script>";
                    return RedirectToAction("IndexDongDoi", new { IDSV });
                }

                // Kiểm tra tối đa 2 nội dung
                if (existingPairs.Count + newItems.Count > 2)
                {
                    TempData["msgError"] = "<script>alert('Bạn chỉ được đăng ký tối đa 2 nội dung');</script>";
                    return RedirectToAction("IndexDongDoi", new { IDSV });
                }

                // Validate đồng đội
                var takenIDs = dbSV.CTDKNguoiThans
                    .Where(x => x.IDSV == IDSV && x.isCom == 1 && x.IDNguoiThan != null)
                    .Select(x => x.IDNguoiThan.Value).ToList();
                var registrantIDs = dbSV.CTDKNguoiThans
                    .Where(x => x.IDSV == IDSV && x.isCom == 1 && x.IDNV != null)
                    .Select(x => x.IDNV.Value).Distinct().ToList();

                var selectedPartnerIDs = new List<int>();

                foreach (var item in newItems)
                {
                    var group      = item.Item1;
                    int? idPartner = item.Item3;
                    string loaiDoi = GetLoaiDoi(group.TenNhom ?? "");

                    if (!idPartner.HasValue) continue; // không cần đồng đội

                    if (takenIDs.Contains(idPartner.Value) || registrantIDs.Contains(idPartner.Value))
                    {
                        var partnerName = db.NhanViens.FirstOrDefault(x => x.ID == idPartner.Value)?.HoTen;
                        TempData["msgError"] = "<script>alert('" + partnerName + " đã được đăng ký trong giải này, vui lòng chọn người khác');</script>";
                        return RedirectToAction("IndexDongDoi", new { IDSV });
                    }

                    if (selectedPartnerIDs.Contains(idPartner.Value))
                    {
                        TempData["msgError"] = "<script>alert('Không thể chọn cùng một người cho nhiều nội dung');</script>";
                        return RedirectToAction("IndexDongDoi", new { IDSV });
                    }
                    selectedPartnerIDs.Add(idPartner.Value);

                    // Validate giới tính
                    var partner = db.NhanViens.FirstOrDefault(x => x.ID == idPartner.Value);
                    bool valid  = true;

                    if (loaiDoi == "DoiNam" && (partner?.IsGioiTinh != 0 || currentUser?.IsGioiTinh != 0))
                        valid = false;
                    else if (loaiDoi == "DoiNu" && (partner?.IsGioiTinh != 1 || currentUser?.IsGioiTinh != 1))
                        valid = false;
                    else if (loaiDoi == "HonHop" && partner?.IsGioiTinh == currentUser?.IsGioiTinh)
                        valid = false;

                    if (!valid)
                    {
                        TempData["msgError"] = "<script>alert('Đồng đội không đúng giới tính cho nội dung " + group.TenNhom + "');</script>";
                        return RedirectToAction("IndexDongDoi", new { IDSV });
                    }
                }

                // Lưu
                dbSV.EmployeeServey_updateOT(IDNV, IDSV, 0);
                foreach (var item in newItems)
                {
                    var group      = item.Item1;
                    int idot       = item.Item2;
                    int? idPartner = item.Item3;
                    string loaiDoi = GetLoaiDoi(group.TenNhom ?? "");

                    dbSV.CTKhaoSat_insert(IDSV, idot, IDNV, group.ID);
                    if (idPartner.HasValue)
                    {
                        ObjectParameter IDNTOut = new ObjectParameter("ID", typeof(int));
                        dbSV.CTDKNguoiThan_insert(IDNV, idPartner.Value, null, null, IDSV, 1, null, null, null, loaiDoi, group.TenNhom, IDNTOut);
                    }
                }

                TempData["msgSuccess"] = "<script>alert('Đăng ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Lỗi: " + e.Message + "');</script>";
            }

            return RedirectToAction("IndexDongDoi", new { IDSV });
        }

        public ActionResult DeleteDongDoiDK(int IDSV)
        {
            try
            {
                var listNT = dbSV.CTDKNguoiThans
                    .Where(x => x.IDSV == IDSV && x.IDNV == MyAuthentication.ID && x.isCom == 1)
                    .ToList();
                foreach (var item in listNT)
                {
                    dbSV.ChiTietDKNT_delete(item.ID);
                    dbSV.CTDKNguoiThan_delete(item.ID);
                }
                dbSV.CTKhaoSat_delete(IDSV, MyAuthentication.ID);
                dbSV.EmployeeServey_updateOT(MyAuthentication.ID, IDSV, null);
                TempData["msgSuccess"] = "<script>alert('Đã xóa đăng ký. Bạn có thể đăng ký lại.');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Lỗi: " + e.Message + "');</script>";
            }
            return RedirectToAction("IndexDongDoi", new { IDSV });
        }

        public ActionResult DeleteDongDoiSolo(int id, int IDSV)
        {
            try
            {
                var item = dbSV.CTKhaoSats.FirstOrDefault(x => x.ID == id && x.IDNV == MyAuthentication.ID);
                if (item != null)
                {
                    dbSV.CTKhaoSats.Remove(item);
                    dbSV.SaveChanges();
                    var remaining = dbSV.CTKhaoSats
                        .Where(x => x.IDNV == MyAuthentication.ID && x.IDSV == IDSV)
                        .ToList();
                    if (!remaining.Any())
                        dbSV.EmployeeServey_updateOT(MyAuthentication.ID, IDSV, null);
                }
                TempData["msgSuccess"] = "<script>alert('Đã xóa đăng ký');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Lỗi: " + e.Message + "');</script>";
            }
            return RedirectToAction("IndexDongDoi", new { IDSV });
        }

        public ActionResult DeleteDongDoiPair(int id, int IDSV)
        {
            try
            {
                var pair = dbSV.CTDKNguoiThans.FirstOrDefault(x => x.ID == id);
                if (pair != null && pair.IDNV == MyAuthentication.ID)
                {
                    dbSV.CTDKNguoiThan_delete(id);

                    var remaining = dbSV.CTDKNguoiThans
                        .Where(x => x.IDNV == MyAuthentication.ID && x.IDSV == IDSV && x.isCom == 1)
                        .ToList();
                    if (!remaining.Any())
                        dbSV.EmployeeServey_updateOT(MyAuthentication.ID, IDSV, null);
                }
                TempData["msgSuccess"] = "<script>alert('Đã xóa đăng ký');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Lỗi: " + e.Message + "');</script>";
            }
            return RedirectToAction("IndexDongDoi", new { IDSV });
        }

    }
}