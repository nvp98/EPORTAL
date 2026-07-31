using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsServey;
using EPORTAL.ModelsView360;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Servey.Controllers
{
    public class ServeyIDController : Controller
    {
        private const int SingingSurveyId = 13;
        private const int SingingMaxSlots = 2;
        private const string SingingRelationType = "Singing";
        private const int PickleballSurveyId = 12;
        private const int PickleballMaxSlots = 2;

        // GET: Servey/ServeyID
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_SERVEYEntities dbSV = new EPORTAL_SERVEYEntities();
        public ActionResult Index(int? IDSV)
        {
            if (IDSV == PickleballSurveyId)
            {
                return IndexPickeball(IDSV);
            }

            if (IDSV == SingingSurveyId)
            {
                return SingingIndex(IDSV.Value);
            }

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

        // ─── PICKLEBALL VIEW (IDSV = 12) ────────────────────────────────────────

        public ActionResult IndexPickeball(int? IDSV)
        {
            if (IDSV != PickleballSurveyId)
            {
                return HttpNotFound();
            }

            var currentResult = IndexDongDoi(IDSV.Value) as ViewResult;
            if (currentResult == null)
            {
                return RedirectToAction("IndexDongDoi", new { IDSV });
            }

            return View("IndexPickeball", currentResult.Model);
        }

        public JsonResult GetPickleballPartners(int IDSV, int IDGroup)
        {
            if (IDSV != PickleballSurveyId)
            {
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }

            var IDNV = MyAuthentication.ID;
            var currentUser = db.NhanViens.FirstOrDefault(x => x.ID == IDNV);
            var surveyGroup = dbSV.GroupKhaoSats.FirstOrDefault(x => x.ID == IDGroup && x.IDSV == IDSV);
            if (currentUser == null || surveyGroup == null)
            {
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }

            string loaiDoi = GetLoaiDoi(surveyGroup.TenNhom ?? "");
            var relations = dbSV.CTDKNguoiThans.Where(x => x.IDSV == IDSV && x.isCom == 1).ToList();
            var excludedIds = new HashSet<int>(relations.Where(x => x.IDNguoiThan.HasValue).Select(x => x.IDNguoiThan.Value));
            foreach (var registrantId in relations.Where(x => x.IDNV.HasValue).Select(x => x.IDNV.Value))
            {
                excludedIds.Add(registrantId);
            }
            excludedIds.Add(IDNV);

            var candidates = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList()
                .Where(x => !excludedIds.Contains(x.ID));
            if (loaiDoi == "DoiNam")
            {
                candidates = candidates.Where(x => x.IsGioiTinh == 0);
            }
            else if (loaiDoi == "DoiNu")
            {
                candidates = candidates.Where(x => x.IsGioiTinh == 1);
            }
            else if (loaiDoi == "HonHop" || loaiDoi == "HonHopTrinhCao")
            {
                int? oppositeGender = currentUser.IsGioiTinh == 0 ? (int?)1 : 0;
                candidates = candidates.Where(x => x.IsGioiTinh == oppositeGender);
            }
            else if (loaiDoi == "HonHopNam")
            {
                candidates = candidates.Where(x => x.IsGioiTinh == 0);
            }

            var departments = db.PhongBans.ToList();
            var result = candidates.OrderBy(x => x.MaNV).ThenBy(x => x.HoTen)
                .Select(x => new
                {
                    x.ID,
                    x.MaNV,
                    x.HoTen,
                    PhongBan = departments.Where(p => p.IDPhongBan == x.IDPhongBan)
                        .Select(p => p.TenPhongBan).FirstOrDefault() ?? ""
                }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPickleball(FormCollection collection)
        {
            int IDSV;
            if (!int.TryParse(collection["IDSV"], out IDSV) || IDSV != PickleballSurveyId)
            {
                return HttpNotFound();
            }

            var IDNV = MyAuthentication.ID;
            try
            {
                using (var transaction = dbSV.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var currentUser = db.NhanViens.FirstOrDefault(x => x.ID == IDNV && x.IDTinhTrangLV == 1);
                    var isAssigned = dbSV.EmployeeServeys.Any(x => x.IDNV == IDNV && x.IDSV == IDSV);
                    if (currentUser == null || !isAssigned)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Bạn không thuộc danh sách đăng ký chương trình này.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    var groups = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).ToList();
                    var options = dbSV.OptionServeys.Where(x => x.IDSV == IDSV).ToList();
                    var newItems = new List<Tuple<GroupKhaoSat, OptionServey>>();

                    foreach (var group in groups)
                    {
                        var selectedValue = collection["answer_" + group.ID];
                        int selectedOptionId;
                        if (string.IsNullOrWhiteSpace(selectedValue)
                            || !int.TryParse(selectedValue, out selectedOptionId))
                        {
                            continue;
                        }

                        var loaiDoi = GetLoaiDoi(group.TenNhom ?? "");
                        var selectedOption = options.FirstOrDefault(x => x.IDOT == selectedOptionId
                            && x.MaOT == group.MaNhom && x.isShow == 1);
                        if (selectedOption == null
                            || !IsPickleballGroupAllowedForGender(loaiDoi, currentUser.IsGioiTinh))
                        {
                            transaction.Rollback();
                            TempData["msgError"] = "<script>alert('Nội dung Pickleball không hợp lệ.');</script>";
                            return RedirectToAction("Index", new { IDSV });
                        }

                        newItems.Add(Tuple.Create(group, selectedOption));
                    }

                    if (!newItems.Any())
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Vui lòng chọn ít nhất một nội dung.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    var existingGroupIds = dbSV.CTKhaoSats
                        .Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.IDGroup.HasValue)
                        .Select(x => x.IDGroup.Value)
                        .Distinct()
                        .ToList();
                    if (newItems.Any(x => existingGroupIds.Contains(x.Item1.ID)))
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Nội dung đã được đăng ký trước đó.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    // Dữ liệu cũ: nếu người dùng từng được người khác chọn làm đồng đội,
                    // mỗi quan hệ vẫn được tính là một lần tham gia.
                    var registeredByOthers = dbSV.CTDKNguoiThans.Where(x => x.IDSV == IDSV
                        && x.IDNguoiThan == IDNV && x.isCom == 1).ToList();
                    var registeredByOthersGroupIds = registeredByOthers
                        .Select(relation => groups.FirstOrDefault(group =>
                            string.Equals(group.TenNhom, relation.GhiChu, StringComparison.OrdinalIgnoreCase))
                            ?? groups.FirstOrDefault(group => IsSamePickleballType(GetLoaiDoi(group.TenNhom ?? ""), relation.QuanHe)))
                        .Where(group => group != null)
                        .Select(group => group.ID)
                        .ToList();
                    if (newItems.Any(x => registeredByOthersGroupIds.Contains(x.Item1.ID)))
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Bạn đã được đăng ký trong nội dung này.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }
                    if (existingGroupIds.Count + registeredByOthers.Count + newItems.Count > PickleballMaxSlots)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Bạn chỉ được đăng ký tối đa " + PickleballMaxSlots + " nội dung.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    dbSV.EmployeeServey_updateOT(IDNV, IDSV, 0);
                    foreach (var item in newItems)
                    {
                        dbSV.CTKhaoSat_insert(IDSV, item.Item2.IDOT, IDNV, item.Item1.ID);
                    }

                    transaction.Commit();
                }

                TempData["msgSuccess"] = "<script>alert('Đăng ký thành công.');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Đăng ký không thành công, vui lòng thử lại.');</script>";
            }

            return RedirectToAction("Index", new { IDSV });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePickleballPartner(int IDSV, int PairID, int PartnerID)
        {
            if (IDSV != PickleballSurveyId)
            {
                return HttpNotFound();
            }

            var IDNV = MyAuthentication.ID;
            try
            {
                using (var transaction = dbSV.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var pair = dbSV.CTDKNguoiThans.FirstOrDefault(x => x.ID == PairID
                        && x.IDNV == IDNV && x.IDSV == IDSV && x.isCom == 1);
                    var currentUser = db.NhanViens.FirstOrDefault(x => x.ID == IDNV && x.IDTinhTrangLV == 1);
                    if (pair == null || currentUser == null || PartnerID == IDNV)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Thông tin chỉnh sửa không hợp lệ.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    var groups = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).ToList();
                    var group = groups.FirstOrDefault(x => string.Equals(x.TenNhom, pair.GhiChu, StringComparison.OrdinalIgnoreCase))
                        ?? groups.FirstOrDefault(x => IsSamePickleballType(GetLoaiDoi(x.TenNhom ?? ""), pair.QuanHe));
                    if (group == null)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Không tìm thấy nội dung Pickleball cần chỉnh sửa.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    var partner = db.NhanViens.FirstOrDefault(x => x.ID == PartnerID && x.IDTinhTrangLV == 1);
                    var partnerAlreadyUsed = dbSV.CTDKNguoiThans.Any(x => x.IDSV == IDSV
                        && x.isCom == 1 && x.ID != pair.ID
                        && (x.IDNguoiThan == PartnerID || x.IDNV == PartnerID));
                    var loaiDoi = GetLoaiDoi(group.TenNhom ?? "");
                    var validGender = partner != null;
                    if (loaiDoi == "DoiNam" || loaiDoi == "HonHopNam")
                    {
                        validGender = validGender && currentUser.IsGioiTinh == 0 && partner.IsGioiTinh == 0;
                    }
                    else if (loaiDoi == "DoiNu")
                    {
                        validGender = validGender && currentUser.IsGioiTinh == 1 && partner.IsGioiTinh == 1;
                    }
                    else if (loaiDoi == "HonHop" || loaiDoi == "HonHopTrinhCao")
                    {
                        validGender = validGender && currentUser.IsGioiTinh != partner.IsGioiTinh;
                    }

                    if (partner == null || partnerAlreadyUsed || !validGender)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Đồng đội không còn khả dụng hoặc không đúng giới tính.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    pair.IDNguoiThan = PartnerID;
                    dbSV.SaveChanges();
                    transaction.Commit();
                }

                TempData["msgSuccess"] = "<script>alert('Đã cập nhật đồng đội.');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Không thể cập nhật đồng đội, vui lòng thử lại.');</script>";
            }

            return RedirectToAction("Index", new { IDSV });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePickleballItem(int IDSV, int id, bool hasPartner)
        {
            if (IDSV != PickleballSurveyId)
            {
                return HttpNotFound();
            }

            if (hasPartner)
            {
                DeleteDongDoiPair(id, IDSV);
            }
            else
            {
                DeleteDongDoiSolo(id, IDSV);
            }
            return RedirectToAction("Index", new { IDSV });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllPickleball(int IDSV)
        {
            if (IDSV != PickleballSurveyId)
            {
                return HttpNotFound();
            }

            DeleteDongDoiDK(IDSV);
            return RedirectToAction("Index", new { IDSV });
        }

        // ─── SINGING REGISTRATION (IDSV = 13) ───────────────────────────────────

        private List<SingingOptionViewModel> GetSingingOptions(int IDSV)
        {
            var groups = dbSV.GroupKhaoSats
                .Where(x => x.IDSV == IDSV && x.MaNhom != null)
                .ToList();
            var options = dbSV.OptionServeys
                .Where(x => x.IDSV == IDSV && x.MaOT != null)
                .ToList();

            return (from option in options
                    join surveyGroup in groups on option.MaOT equals surveyGroup.MaNhom
                    select new SingingOptionViewModel
                    {
                        IDOT = option.IDOT,
                        IDGroup = surveyGroup.ID,
                        Content = option.ContentOT,
                        OrderBy = option.OrderBy,
                        RequiresPartner = option.isShow == 1
                    })
                    .GroupBy(x => x.IDOT)
                    .Select(x => x.First())
                    .OrderBy(x => x.OrderBy)
                    .ThenBy(x => x.IDOT)
                    .ToList();
        }

        private ActionResult SingingIndex(int IDSV)
        {
            var IDNV = MyAuthentication.ID;
            var survey = dbSV.ListServeys.FirstOrDefault(x => x.IDSV == IDSV);
            if (survey == null)
            {
                return HttpNotFound();
            }

            var incomingPairs = dbSV.CTDKNguoiThans
                .Where(x => x.IDNguoiThan == IDNV && x.IDSV == IDSV && x.isCom == 1)
                .OrderBy(x => x.ID)
                .ToList();
            var employeeSurvey = dbSV.EmployeeServeys
                .FirstOrDefault(x => x.IDNV == IDNV && x.IDSV == IDSV);
            if (employeeSurvey == null && !incomingPairs.Any())
            {
                TempData["msgError"] = "<script>alert('Bạn không thuộc danh sách được đăng ký chương trình này.');</script>";
                return RedirectToAction("Index", "UserServey", new { area = "Servey" });
            }

            var optionDefinitions = GetSingingOptions(IDSV);
            var registrant = db.NhanViens.FirstOrDefault(x => x.ID == IDNV);
            var registrantDepartment = registrant != null
                ? db.PhongBans.FirstOrDefault(x => x.IDPhongBan == registrant.IDPhongBan)
                : null;
            var optionIds = optionDefinitions.Select(x => x.IDOT).ToList();
            var registrationRows = dbSV.CTKhaoSats
                .Where(x => x.IDNV == IDNV && x.IDSV == IDSV)
                .ToList()
                .Where(x => x.IDOT.HasValue && optionIds.Contains(x.IDOT.Value))
                .GroupBy(x => x.IDOT.Value)
                .Select(x => x.OrderBy(y => y.ID).First())
                .ToList();

            var pairs = dbSV.CTDKNguoiThans
                .Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.isCom == 1)
                .OrderBy(x => x.ID)
                .ToList();
            var partnerIds = pairs
                .Where(x => x.IDNguoiThan.HasValue)
                .Select(x => x.IDNguoiThan.Value)
                .Distinct()
                .ToList();
            var employees = db.NhanViens.Where(x => partnerIds.Contains(x.ID)).ToList();
            var departments = db.PhongBans.ToList();
            var usedPairIds = new HashSet<int>();
            var registrations = new List<SingingRegistrationItemViewModel>();

            foreach (var row in registrationRows)
            {
                var definition = optionDefinitions.First(x => x.IDOT == row.IDOT.Value);
                CTDKNguoiThan pair = null;
                if (definition.RequiresPartner)
                {
                    pair = pairs.FirstOrDefault(x => !usedPairIds.Contains(x.ID)
                        && string.Equals(x.GhiChu, definition.Content, StringComparison.OrdinalIgnoreCase));
                    if (pair == null)
                    {
                        pair = pairs.FirstOrDefault(x => !usedPairIds.Contains(x.ID));
                    }
                    if (pair != null)
                    {
                        usedPairIds.Add(pair.ID);
                    }
                }

                var partner = pair != null && pair.IDNguoiThan.HasValue
                    ? employees.FirstOrDefault(x => x.ID == pair.IDNguoiThan.Value)
                    : null;
                var department = partner != null
                    ? departments.FirstOrDefault(x => x.IDPhongBan == partner.IDPhongBan)
                    : null;

                registrations.Add(new SingingRegistrationItemViewModel
                {
                    CTKhaoSatID = row.ID,
                    IDOT = definition.IDOT,
                    Content = definition.Content,
                    RequiresPartner = definition.RequiresPartner,
                    PairID = pair != null ? (int?)pair.ID : null,
                    CanManage = true,
                    RegistrationOwnerPhone = registrant != null ? registrant.DienThoai : null,
                    PartnerCode = definition.RequiresPartner
                        ? (partner != null ? partner.MaNV : null)
                        : (registrant != null ? registrant.MaNV : null),
                    PartnerName = definition.RequiresPartner
                        ? (partner != null ? partner.HoTen : null)
                        : (registrant != null ? registrant.HoTen : null),
                    PartnerPhone = definition.RequiresPartner && partner != null ? partner.DienThoai : null,
                    PartnerDepartment = definition.RequiresPartner
                        ? (department != null ? department.TenPhongBan : null)
                        : (registrantDepartment != null ? registrantDepartment.TenPhongBan : null)
                });
            }

            // Nếu người dùng được người khác đăng ký hát cùng, vẫn hiển thị lượt đó
            // trên form của họ nhưng không cho phép sửa/xóa hoặc đăng ký thêm Song ca.
            var incomingOwnerIds = incomingPairs
                .Where(x => x.IDNV.HasValue)
                .Select(x => x.IDNV.Value)
                .Distinct()
                .ToList();
            var incomingOwners = db.NhanViens.Where(x => incomingOwnerIds.Contains(x.ID)).ToList();
            foreach (var incomingPair in incomingPairs)
            {
                var owner = incomingPair.IDNV.HasValue
                    ? incomingOwners.FirstOrDefault(x => x.ID == incomingPair.IDNV.Value)
                    : null;
                var ownerDepartment = owner != null
                    ? departments.FirstOrDefault(x => x.IDPhongBan == owner.IDPhongBan)
                    : null;
                var definition = optionDefinitions.FirstOrDefault(x => x.RequiresPartner
                    && string.Equals(x.Content, incomingPair.GhiChu, StringComparison.OrdinalIgnoreCase))
                    ?? optionDefinitions.FirstOrDefault(x => x.RequiresPartner);

                registrations.Add(new SingingRegistrationItemViewModel
                {
                    CTKhaoSatID = 0,
                    IDOT = definition != null ? definition.IDOT : 0,
                    Content = definition != null ? definition.Content : (incomingPair.GhiChu ?? "Song ca"),
                    RequiresPartner = true,
                    PairID = incomingPair.ID,
                    CanManage = false,
                    RegistrationOwnerPhone = owner != null ? owner.DienThoai : null,
                    PartnerCode = owner != null ? owner.MaNV : null,
                    PartnerName = owner != null ? owner.HoTen : null,
                    PartnerPhone = owner != null ? owner.DienThoai : null,
                    PartnerDepartment = ownerDepartment != null ? ownerDepartment.TenPhongBan : null
                });
            }

            var registeredOptionIds = new HashSet<int>(registrations.Select(x => x.IDOT));
            foreach (var option in optionDefinitions)
            {
                option.IsRegistered = registeredOptionIds.Contains(option.IDOT);
            }

            var assignedSurveyIds = dbSV.EmployeeServeys
                .Where(x => x.IDNV == IDNV)
                .Select(x => x.IDSV)
                .ToList();
            var activeSurveyIds = dbSV.ListServeys
                .Where(x => assignedSurveyIds.Contains(x.IDSV)
                    && x.StartTime <= DateTime.Now
                    && x.EndTime >= DateTime.Now
                    && x.StatusSV == true)
                .OrderBy(x => x.StartTime)
                .ThenBy(x => x.IDSV)
                .Select(x => x.IDSV)
                .ToList();
            var currentSurveyIndex = activeSurveyIds.IndexOf(IDSV);
            ViewBag.NextIDSV = currentSurveyIndex >= 0 && currentSurveyIndex < activeSurveyIds.Count - 1
                ? (int?)activeSurveyIds[currentSurveyIndex + 1]
                : null;

            var now = DateTime.Now;
            var model = new SingingRegistrationViewModel
            {
                IDSV = IDSV,
                Title = survey.ContentSV,
                IsActive = survey.StatusSV == true
                    && (!survey.StartTime.HasValue || survey.StartTime.Value <= now)
                    && (!survey.EndTime.HasValue || survey.EndTime.Value >= now),
                RegistrantCode = registrant != null ? registrant.MaNV : null,
                RegistrantName = registrant != null ? registrant.HoTen : null,
                RegistrantPhone = registrant != null ? registrant.DienThoai : null,
                RegistrantDepartment = registrantDepartment != null ? registrantDepartment.TenPhongBan : null,
                MaxSlots = SingingMaxSlots,
                SlotsRemaining = Math.Max(0, SingingMaxSlots - registrationRows.Count - incomingPairs.Count),
                Options = optionDefinitions,
                Registrations = registrations
            };

            return View("IndexTiengHat", model);
        }

        public JsonResult GetSingingPartners(int IDSV, int IDOT)
        {
            if (IDSV != SingingSurveyId)
            {
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }

            var option = GetSingingOptions(IDSV)
                .FirstOrDefault(x => x.IDOT == IDOT && x.RequiresPartner);
            if (option == null)
            {
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }

            var IDNV = MyAuthentication.ID;
            var isAssigned = dbSV.EmployeeServeys.Any(x => x.IDNV == IDNV && x.IDSV == IDSV);
            if (!isAssigned)
            {
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }
            var relations = dbSV.CTDKNguoiThans
                .Where(x => x.IDSV == IDSV && x.isCom == 1)
                .ToList();
            var excludedIds = new HashSet<int>(relations
                .Where(x => x.IDNguoiThan.HasValue)
                .Select(x => x.IDNguoiThan.Value));
            foreach (var registrantId in relations.Where(x => x.IDNV.HasValue).Select(x => x.IDNV.Value))
            {
                excludedIds.Add(registrantId);
            }
            excludedIds.Add(IDNV);

            var employees = db.NhanViens
                .Where(x => x.IDTinhTrangLV == 1)
                .ToList()
                .Where(x => !excludedIds.Contains(x.ID))
                .ToList();
            var departments = db.PhongBans.ToList();
            var result = employees
                .OrderBy(x => x.MaNV)
                .ThenBy(x => x.HoTen)
                .Select(x => new
                {
                    x.ID,
                    x.MaNV,
                    x.HoTen,
                    PhongBan = departments
                        .Where(p => p.IDPhongBan == x.IDPhongBan)
                        .Select(p => p.TenPhongBan)
                        .FirstOrDefault() ?? ""
                })
                .ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmSinging(SingingRegistrationRequest request)
        {
            if (request == null || request.IDSV != SingingSurveyId)
            {
                return HttpNotFound();
            }

            var IDNV = MyAuthentication.ID;
            var selectedIds = (request.SelectedOptionIds ?? new List<int>()).Distinct().ToList();
            if (selectedIds.Count == 0)
            {
                TempData["msgError"] = "<script>alert('Vui lòng chọn ít nhất một nội dung.');</script>";
                return RedirectToAction("Index", new { IDSV = request.IDSV });
            }

            var survey = dbSV.ListServeys.FirstOrDefault(x => x.IDSV == request.IDSV);
            var now = DateTime.Now;
            var isActive = survey != null && survey.StatusSV == true
                && (!survey.StartTime.HasValue || survey.StartTime.Value <= now)
                && (!survey.EndTime.HasValue || survey.EndTime.Value >= now);
            var employeeSurvey = dbSV.EmployeeServeys
                .FirstOrDefault(x => x.IDNV == IDNV && x.IDSV == request.IDSV);
            if (!isActive || employeeSurvey == null)
            {
                TempData["msgError"] = "<script>alert('Chương trình hiện không nhận đăng ký.');</script>";
                return RedirectToAction("Index", new { IDSV = request.IDSV });
            }

            var singingOptions = GetSingingOptions(request.IDSV);
            var singingOptionIds = singingOptions.Select(x => x.IDOT).ToList();
            var validOptions = singingOptions
                .Where(x => selectedIds.Contains(x.IDOT))
                .ToList();
            if (validOptions.Count != selectedIds.Count)
            {
                TempData["msgError"] = "<script>alert('Nội dung đăng ký không hợp lệ.');</script>";
                return RedirectToAction("Index", new { IDSV = request.IDSV });
            }

            try
            {
                using (var transaction = dbSV.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var incomingRegistrationCount = dbSV.CTDKNguoiThans.Count(x => x.IDSV == request.IDSV
                        && x.IDNguoiThan == IDNV && x.isCom == 1);

                    var existingOptionIds = dbSV.CTKhaoSats
                        .Where(x => x.IDNV == IDNV && x.IDSV == request.IDSV && x.IDOT != null)
                        .Select(x => x.IDOT.Value)
                        .Where(x => singingOptionIds.Contains(x))
                        .Distinct()
                        .ToList();
                    var newOptions = validOptions
                        .Where(x => !existingOptionIds.Contains(x.IDOT))
                        .ToList();
                    if (newOptions.Count == 0)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Các nội dung đã được đăng ký trước đó.');</script>";
                        return RedirectToAction("Index", new { IDSV = request.IDSV });
                    }
                    if (incomingRegistrationCount > 0 && newOptions.Any(x => x.RequiresPartner))
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Bạn đã được đăng ký Song ca với nhân viên khác. Bạn chỉ có thể đăng ký thêm Đơn ca.');</script>";
                        return RedirectToAction("Index", new { IDSV = request.IDSV });
                    }
                    if (existingOptionIds.Count + incomingRegistrationCount + newOptions.Count > SingingMaxSlots)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Bạn chỉ được đăng ký tối đa 2 nội dung.');</script>";
                        return RedirectToAction("Index", new { IDSV = request.IDSV });
                    }

                    var partnerOptions = newOptions.Where(x => x.RequiresPartner).ToList();
                    EPORTAL.ModelsView360.NhanVien partner = null;
                    if (partnerOptions.Any())
                    {
                        if (!request.PartnerID.HasValue || request.PartnerID.Value == IDNV)
                        {
                            transaction.Rollback();
                            TempData["msgError"] = "<script>alert('Vui lòng chọn người hát cùng hợp lệ.');</script>";
                            return RedirectToAction("Index", new { IDSV = request.IDSV });
                        }

                        partner = db.NhanViens.FirstOrDefault(x => x.ID == request.PartnerID.Value && x.IDTinhTrangLV == 1);
                        var currentUserAlreadyUsed = dbSV.CTDKNguoiThans.Any(x => x.IDSV == request.IDSV
                            && x.isCom == 1
                            && (x.IDNguoiThan == IDNV || x.IDNV == IDNV));
                        var partnerAlreadyUsed = dbSV.CTDKNguoiThans.Any(x => x.IDSV == request.IDSV
                            && x.isCom == 1
                            && (x.IDNguoiThan == request.PartnerID.Value || x.IDNV == request.PartnerID.Value));
                        if (partner == null || currentUserAlreadyUsed || partnerAlreadyUsed)
                        {
                            transaction.Rollback();
                            TempData["msgError"] = "<script>alert('Người hát cùng đã được đăng ký hoặc không còn khả dụng.');</script>";
                            return RedirectToAction("Index", new { IDSV = request.IDSV });
                        }
                    }

                    dbSV.EmployeeServey_updateOT(IDNV, request.IDSV, 0);
                    foreach (var option in newOptions)
                    {
                        dbSV.CTKhaoSat_insert(request.IDSV, option.IDOT, IDNV, option.IDGroup);
                        if (option.RequiresPartner)
                        {
                            ObjectParameter IDNTOut = new ObjectParameter("ID", typeof(int));
                            dbSV.CTDKNguoiThan_insert(IDNV, partner.ID, null, null, request.IDSV, 1,
                                null, null, null, SingingRelationType, option.Content, IDNTOut);
                        }
                    }

                    transaction.Commit();
                }

                TempData["msgSuccess"] = "<script>alert('Đăng ký thành công.');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Đăng ký không thành công, vui lòng thử lại.');</script>";
            }

            return RedirectToAction("Index", new { IDSV = request.IDSV });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSingingPartner(int IDSV, int CTKhaoSatID, int PairID, int PartnerID)
        {
            if (IDSV != SingingSurveyId)
            {
                return HttpNotFound();
            }

            var IDNV = MyAuthentication.ID;
            try
            {
                using (var transaction = dbSV.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var registration = dbSV.CTKhaoSats.FirstOrDefault(x => x.ID == CTKhaoSatID
                        && x.IDSV == IDSV && x.IDNV == IDNV && x.IDOT.HasValue);
                    if (registration == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    var option = GetSingingOptions(IDSV)
                        .FirstOrDefault(x => x.IDOT == registration.IDOT.Value && x.RequiresPartner);
                    if (option == null || PartnerID == IDNV)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Thông tin cập nhật không hợp lệ.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    var pair = dbSV.CTDKNguoiThans.FirstOrDefault(x => x.ID == PairID
                        && x.IDNV == IDNV && x.IDSV == IDSV && x.isCom == 1);
                    if (pair == null)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Không tìm thấy đăng ký Song ca cần chỉnh sửa.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    var partner = db.NhanViens.FirstOrDefault(x => x.ID == PartnerID && x.IDTinhTrangLV == 1);
                    var partnerAlreadyUsed = dbSV.CTDKNguoiThans.Any(x => x.IDSV == IDSV
                        && x.isCom == 1 && x.ID != pair.ID
                        && (x.IDNguoiThan == PartnerID || x.IDNV == PartnerID));
                    if (partner == null || partnerAlreadyUsed)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Người hát cùng đã được đăng ký hoặc không còn khả dụng.');</script>";
                        return RedirectToAction("Index", new { IDSV });
                    }

                    pair.IDNguoiThan = PartnerID;
                    dbSV.SaveChanges();
                    transaction.Commit();
                }

                TempData["msgSuccess"] = "<script>alert('Đã cập nhật người hát cùng.');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Không thể cập nhật người hát cùng, vui lòng thử lại.');</script>";
            }

            return RedirectToAction("Index", new { IDSV });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSingingItem(int IDSV, int CTKhaoSatID)
        {
            if (IDSV != SingingSurveyId)
            {
                return HttpNotFound();
            }

            var IDNV = MyAuthentication.ID;
            try
            {
                using (var transaction = dbSV.Database.BeginTransaction())
                {
                    var row = dbSV.CTKhaoSats.FirstOrDefault(x => x.ID == CTKhaoSatID
                        && x.IDSV == IDSV && x.IDNV == IDNV);
                    if (row == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    var option = GetSingingOptions(IDSV).FirstOrDefault(x => x.IDOT == row.IDOT);
                    if (option != null && option.RequiresPartner)
                    {
                        var pairs = dbSV.CTDKNguoiThans
                            .Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.isCom == 1)
                            .OrderBy(x => x.ID)
                            .ToList();
                        var pair = pairs.FirstOrDefault(x => string.Equals(x.GhiChu, option.Content, StringComparison.OrdinalIgnoreCase));
                        if (pair == null && pairs.Count == 1)
                        {
                            pair = pairs[0];
                        }
                        if (pair != null)
                        {
                            dbSV.CTDKNguoiThan_delete(pair.ID);
                        }
                    }

                    dbSV.CTKhaoSats.Remove(row);
                    dbSV.SaveChanges();
                    var validOptionIds = GetSingingOptions(IDSV).Select(x => x.IDOT).ToList();
                    var hasRemaining = dbSV.CTKhaoSats.Any(x => x.IDNV == IDNV && x.IDSV == IDSV
                        && x.IDOT.HasValue && validOptionIds.Contains(x.IDOT.Value));
                    if (!hasRemaining)
                    {
                        dbSV.EmployeeServey_updateOT(IDNV, IDSV, null);
                    }
                    transaction.Commit();
                }
                TempData["msgSuccess"] = "<script>alert('Đã xóa nội dung đăng ký.');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Không thể xóa đăng ký, vui lòng thử lại.');</script>";
            }

            return RedirectToAction("Index", new { IDSV });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllSinging(int IDSV)
        {
            if (IDSV != SingingSurveyId)
            {
                return HttpNotFound();
            }

            var IDNV = MyAuthentication.ID;
            try
            {
                using (var transaction = dbSV.Database.BeginTransaction())
                {
                    var pairs = dbSV.CTDKNguoiThans
                        .Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.isCom == 1)
                        .ToList();
                    foreach (var pair in pairs)
                    {
                        dbSV.CTDKNguoiThan_delete(pair.ID);
                    }
                    dbSV.CTKhaoSat_delete(IDSV, IDNV);
                    dbSV.EmployeeServey_updateOT(IDNV, IDSV, null);
                    transaction.Commit();
                }
                TempData["msgSuccess"] = "<script>alert('Đã xóa toàn bộ đăng ký.');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Không thể xóa đăng ký, vui lòng thử lại.');</script>";
            }

            return RedirectToAction("Index", new { IDSV });
        }

        // ─── PICKLEBALL ──────────────────────────────────────────────────────────

        private string GetLoaiDoi(string tenNhom)
        {
            var t = (tenNhom ?? "").ToLower();
            var compact = t.Replace(" ", "").Replace("-", "");
            if (t.Contains("trình cao") || t.Contains("trinh cao"))
            {
                // Tên cũ có chữ "Nam" chỉ dành cho nam; tên mới là nội dung hỗn hợp cho cả hai giới.
                return t.Contains("nam") ? "HonHopNam" : "HonHopTrinhCao";
            }
            if (t.Contains("hỗn hợp") || compact.Contains("namnữ") || compact.Contains("namnu") || t.Contains("mix"))
                return "HonHop";
            if (t.Contains("nữ"))
                return "DoiNu";
            if (t.Contains("nam"))
                return "DoiNam";
            return "All";
        }

        private bool IsPickleballGroupAllowedForGender(string loaiDoi, int? gender)
        {
            if (!gender.HasValue)
            {
                return true;
            }

            return loaiDoi == "HonHop" || loaiDoi == "HonHopTrinhCao"
                || (gender.Value == 0 && loaiDoi == "HonHopNam")
                || (gender.Value == 0 && loaiDoi == "DoiNam")
                || (gender.Value == 1 && loaiDoi == "DoiNu");
        }

        private bool IsSamePickleballType(string firstType, string secondType)
        {
            if (string.Equals(firstType, secondType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Các quan hệ cũ có thể vẫn lưu mã loại HonHopNam trước khi nhóm được đổi tên.
            return (string.Equals(firstType, "HonHopTrinhCao", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(secondType, "HonHopNam", StringComparison.OrdinalIgnoreCase))
                || (string.Equals(firstType, "HonHopNam", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(secondType, "HonHopTrinhCao", StringComparison.OrdinalIgnoreCase));
        }

        public ActionResult IndexDongDoi(int? IDSV)
        {
            if (IDSV == SingingSurveyId)
            {
                return RedirectToAction("Index", new { IDSV = SingingSurveyId });
            }

            var IDNV = MyAuthentication.ID;
            var currentUser = db.NhanViens.FirstOrDefault(x => x.ID == IDNV);
            var groups   = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).OrderBy(x => x.MaNhom).ToList();
            var options  = dbSV.OptionServeys.Where(x => x.IDSV == IDSV).ToList();
            var LSNV     = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
            var pb       = db.PhongBans.ToList();

            var myPairs = dbSV.CTDKNguoiThans
                .Where(x => x.IDNV == IDNV  && x.IDSV == IDSV && x.isCom == 1)
                .ToList();

            var myPairsView = (from a in myPairs
                               join b in LSNV on a.IDNguoiThan equals b.ID into ul
                               from b in ul.DefaultIfEmpty()
                               let matchedGroup = groups.FirstOrDefault(g => string.Equals(g.TenNhom, a.GhiChu, StringComparison.OrdinalIgnoreCase))
                                   ?? groups.FirstOrDefault(g => IsSamePickleballType(GetLoaiDoi(g.TenNhom ?? ""), a.QuanHe))
                               select new PartTogetherValidation
                               {
                                   ID          = a.ID,
                                   HoTen       = b != null ? b.HoTen : "",
                                   MaNV        = b != null ? b.MaNV  : "",
                                   IDNguoiThan = a.IDNguoiThan,
                                   IDSV        = a.IDSV,
                                   IDGroup     = matchedGroup != null ? (int?)matchedGroup.ID : null,
                                   PhongBan    = b != null ? pb.FirstOrDefault(x => x.IDPhongBan == b.IDPhongBan)?.TenPhongBan : "",
                                   QuanHe      = matchedGroup != null ? GetLoaiDoi(matchedGroup.TenNhom ?? "") : a.QuanHe,
                                   Note        = matchedGroup != null ? matchedGroup.TenNhom : a.GhiChu
                               }).ToList();

            var registeredGroupIds = new HashSet<int>(myPairsView
                .Where(x => x.IDGroup.HasValue)
                .Select(x => x.IDGroup.Value));

            // Registrations saved only to CTKhaoSat (the current Pickleball form does not require a partner).
            var soloCtKS = dbSV.CTKhaoSats
                .Where(x => x.IDNV == IDNV && x.IDSV == IDSV && x.IDGroup != null)
                .ToList()
                .Where(x => !registeredGroupIds.Contains(x.IDGroup.Value))
                .ToList();
            var soloView = (from a in soloCtKS
                            join g in groups on a.IDGroup equals g.ID into gg
                            from g in gg.DefaultIfEmpty()
                            let loaiDoiS = GetLoaiDoi(g != null ? g.TenNhom ?? "" : "")
                            where g != null
                            select new PartTogetherValidation
                            {
                                ID          = a.ID,
                                IDGroup     = g.ID,
                                Note        = g.TenNhom ?? "",
                                QuanHe      = loaiDoiS,
                                IDSV        = IDSV,
                                IDNguoiThan = null,
                            }).ToList();
            myPairsView.AddRange(soloView);

            foreach (var soloGroupId in soloView.Where(x => x.IDGroup.HasValue).Select(x => x.IDGroup.Value))
            {
                registeredGroupIds.Add(soloGroupId);
            }

            var groupViews = groups.Select(g =>
            {
                string loaiDoi = GetLoaiDoi(g.TenNhom ?? "");
                return new PickleballGroupView
                {
                    IDGroup      = g.ID,
                    IDSV         = g.IDSV ?? 0,
                    TenNhom      = g.TenNhom,
                    LoaiDoi      = loaiDoi,
                    IsRegistered = registeredGroupIds.Contains(g.ID),
                    ExistingPair = myPairsView.FirstOrDefault(p => p.IDGroup == g.ID),
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

            // Lọc theo giới tính; các nội dung hỗn hợp (kể cả trình cao) hiển thị cho cả nam và nữ.
            int? userGioiTinh = currentUser?.IsGioiTinh;
            if (userGioiTinh.HasValue && IDSV == PickleballSurveyId)
            {
                groupViews = groupViews
                    .Where(g => IsPickleballGroupAllowedForGender(g.LoaiDoi, userGioiTinh))
                    .ToList();
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

            // Dữ liệu Pickleball cũ có thể còn quan hệ đồng đội. Nội dung mà người dùng
            // đã được người khác đăng ký cùng vẫn chiếm một lượt và không được chọn lại.
            foreach (var relation in registeredByOthers)
            {
                var relatedGroup = groups.FirstOrDefault(g => string.Equals(g.TenNhom, relation.GhiChu, StringComparison.OrdinalIgnoreCase))
                    ?? groups.FirstOrDefault(g => IsSamePickleballType(GetLoaiDoi(g.TenNhom ?? ""), relation.QuanHe));
                var relatedGroupView = relatedGroup != null
                    ? groupViews.FirstOrDefault(g => g.IDGroup == relatedGroup.ID)
                    : null;
                if (relatedGroupView != null)
                {
                    relatedGroupView.IsRegistered = true;
                }
            }

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
            ViewBag.MaxSlots            = PickleballMaxSlots;
            ViewBag.SlotsRemaining      = Math.Max(0, PickleballMaxSlots - myPairsView.Count - registeredByOthers.Count);
            ViewBag.MyPairs             = myPairsView;
            ViewBag.RegistrantCode      = currentUser != null ? currentUser.MaNV : null;
            ViewBag.RegistrantName      = currentUser != null ? currentUser.HoTen : null;
            ViewBag.RegistrantPhone     = currentUser != null ? currentUser.DienThoai : null;
            ViewBag.RegistrantDepartment = currentUser != null
                ? pb.Where(x => x.IDPhongBan == currentUser.IDPhongBan).Select(x => x.TenPhongBan).FirstOrDefault()
                : null;
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
            else if (loaiDoi == "HonHop" || loaiDoi == "HonHopTrinhCao") // Đội hỗn hợp: chọn giới tính ngược với người đăng ký
            {
                int? oppositeGender = currentUser?.IsGioiTinh == 0 ? (int?)1 : 0;
                candidates = candidates.Where(x => x.IsGioiTinh == oppositeGender);
            }
            else if (loaiDoi == "HonHopNam")
                candidates = candidates.Where(x => x.IsGioiTinh == 0);

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
                // Thu thập lựa chọn mới từ form
                var newItems = new List<Tuple<GroupKhaoSat, int, int?>>();

                foreach (var group in groups)
                {
                    string loaiDoi = GetLoaiDoi(group.TenNhom ?? "");
                    if (IDSV == PickleballSurveyId
                        && !IsPickleballGroupAllowedForGender(loaiDoi, currentUser != null ? currentUser.IsGioiTinh : null))
                    {
                        continue;
                    }
                    var alreadyRegistered = existingPairs.Any(x =>
                        string.Equals(x.GhiChu, group.TenNhom, StringComparison.OrdinalIgnoreCase)
                        || IsSamePickleballType(GetLoaiDoi(x.GhiChu ?? ""), loaiDoi)
                        || IsSamePickleballType(x.QuanHe, loaiDoi));
                    if (alreadyRegistered) continue;

                    string selectedOT = collection["answer_" + group.ID];
                    if (selectedOT == null) continue;

                    int idot   = int.Parse(selectedOT);
                    var option = dbSV.OptionServeys.FirstOrDefault(x => x.IDOT == idot
                        && x.IDSV == IDSV && x.MaOT == group.MaNhom);
                    if (option == null) continue;

                    int? idPartner = null;
                    var requiresPartner = option.isShow == 1;
                    if (requiresPartner)
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
                    else if ((loaiDoi == "HonHop" || loaiDoi == "HonHopTrinhCao")
                        && partner?.IsGioiTinh == currentUser?.IsGioiTinh)
                        valid = false;
                    else if (loaiDoi == "HonHopNam"
                        && (partner?.IsGioiTinh != 0 || currentUser?.IsGioiTinh != 0))
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
                using (var transaction = dbSV.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var IDNV = MyAuthentication.ID;
                    var pair = dbSV.CTDKNguoiThans.FirstOrDefault(x => x.ID == id
                        && x.IDSV == IDSV && x.IDNV == IDNV && x.isCom == 1);
                    if (pair == null)
                    {
                        transaction.Rollback();
                        TempData["msgError"] = "<script>alert('Không tìm thấy đăng ký cần xóa.');</script>";
                        return RedirectToAction("IndexDongDoi", new { IDSV });
                    }

                    var groups = dbSV.GroupKhaoSats.Where(x => x.IDSV == IDSV).ToList();
                    var group = groups.FirstOrDefault(x => string.Equals(x.TenNhom, pair.GhiChu, StringComparison.OrdinalIgnoreCase))
                        ?? groups.FirstOrDefault(x => IsSamePickleballType(GetLoaiDoi(x.TenNhom ?? ""), pair.QuanHe));

                    var details = dbSV.ChiTietDKNTs.Where(x => x.IDNguoiThan == pair.ID).ToList();
                    if (details.Any())
                    {
                        dbSV.ChiTietDKNTs.RemoveRange(details);
                    }
                    dbSV.CTDKNguoiThans.Remove(pair);

                    if (group != null)
                    {
                        var registrations = dbSV.CTKhaoSats.Where(x => x.IDNV == IDNV
                            && x.IDSV == IDSV && x.IDGroup == group.ID).ToList();
                        if (registrations.Any())
                        {
                            dbSV.CTKhaoSats.RemoveRange(registrations);
                        }
                    }

                    dbSV.SaveChanges();

                    var hasRemainingRegistration = dbSV.CTKhaoSats.Any(x => x.IDNV == IDNV && x.IDSV == IDSV);
                    if (!hasRemainingRegistration)
                    {
                        dbSV.EmployeeServey_updateOT(IDNV, IDSV, null);
                    }

                    transaction.Commit();
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
