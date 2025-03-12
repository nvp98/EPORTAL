using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class Approve_RegisterPeopleController : Controller
    {
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "FollowTheSiger";
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        // GET: TagSign/Approve_RegisterPeople
        public ActionResult Index_(int? page, int? NTID, DateTime? begind, DateTime? endd,int? id)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var nd = db.AuthorizationContractors.Where(x => x.IDNhanVien == MyAuthentication.ID).SingleOrDefault();
            var idnv = db.NhanViens.Where(x => x.ID == nd.IDNhanVien).FirstOrDefault();
            var data = (from kd in db_dk.SignOff_Flow.Where(x => x.TinhTrangID != 0 && x.NhanVienID == idnv.ID)
                              join ca in db_dk.RegisterPeoples on kd.DKTN_ID equals ca.ID_DKTN
                              select new Follow_RegisterPeopleValidation
                              {
                                  ID_TK_TN = (int)kd.ID_TK_TN,
                                  DKTN_ID = (int)kd.DKTN_ID,
                                  NoiDung = ca.NoiDung,
                                  TrinhKy_ID = (int)ca.TrinhKy_ID,
                                  LoaiNT_ID = (int)ca.LoaiNT_ID,
                                  NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                  NT_ID = (int?)ca.NhaThau_ID ?? default,
                                  HopDong = ca.HopDong,
                                  File_CCAT = ca.File_CCAT,
                                  CapDuyet = (int)kd.CapDuyet,
                                  TinhTrangID = (int)kd.TinhTrangID,
                                  NhanVienID = (int)kd.NhanVienID,
                                  NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                  GhiChu = kd.GhiChu
                         }).ToList();

            if (begind == null && endd == null)
            {
                data = data.Where(x => x.NgayTrinh >= startDay && x.NgayTrinh <= endDay).ToList();
            }
            else
            {
                data = data.Where(x => x.NgayTrinh >= begind && x.NgayTrinh <= endd).ToList();
            }
            if (id != 0)
            {
                data = data.Where(x => x.TrinhKy_ID == id).ToList();
            }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(x => x.NgayDuyet).ToList().ToPagedList(pageNumber, pageSize));
        }    
        public ActionResult Index(int? page, int? NTID, DateTime? begind, DateTime? endd, int? id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }


            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            List<Follow_RegisterPeopleValidation> data = new List<Follow_RegisterPeopleValidation>();
            var nd = db.AuthorizationContractors.Where(x => x.IDNhanVien == MyAuthentication.ID).SingleOrDefault();
            var idnv = db.NhanViens.Where(x => x.ID == nd.IDNhanVien).FirstOrDefault();


            var list = db_dk.SignOff_Flow.Where(x => x.NhanVienID == idnv.ID && x.TinhTrangID == 0).ToList();
            foreach (var item in list)
            {
                if (item.CapDuyet == 1)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.TinhTrangID == 0 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x => x.TinhTrang_ID == 1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID??default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
                else if (item.CapDuyet == 2)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.CapDuyet == 1 && x.TinhTrangID == 1 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x=>x.TinhTrang_ID==1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID ?? default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
                else if (item.CapDuyet == 3)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.CapDuyet == 2 && x.TinhTrangID == 1 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x => x.TinhTrang_ID == 1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID ?? default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
                else if (item.CapDuyet == 4)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.CapDuyet == 3 && x.TinhTrangID == 1 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x => x.TinhTrang_ID == 1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID ?? default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
                else if (item.CapDuyet == 5)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.CapDuyet == 4 && x.TinhTrangID == 1 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x => x.TinhTrang_ID == 1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID ?? default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
                else if (item.CapDuyet == 6)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.CapDuyet == 5 && x.TinhTrangID == 1 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x => x.TinhTrang_ID == 1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID ?? default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
                else if (item.CapDuyet == 7)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.CapDuyet == 6 && x.TinhTrangID == 1 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x => x.TinhTrang_ID == 1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID ?? default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
                else if (item.CapDuyet == 8)
                {
                    var check_list = (from kd in db_dk.SignOff_Flow.Where(x => x.CapDuyet == 7 && x.TinhTrangID == 1 && x.NhanVienID != null && x.DKTN_ID == item.DKTN_ID)
                                      join ca in db_dk.RegisterPeoples.Where(x => x.TinhTrang_ID == 1) on kd.DKTN_ID equals ca.ID_DKTN
                                      select new Follow_RegisterPeopleValidation
                                      {
                                          ID_TK_TN = (int)kd.ID_TK_TN,
                                          DKTN_ID = (int)kd.DKTN_ID,
                                          NoiDung = ca.NoiDung,
                                          TrinhKy_ID = (int?)ca.TrinhKy_ID ?? default,
                                          LoaiNT_ID = (int)ca.LoaiNT_ID,
                                          NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                                          NT_ID = (int?)ca.NhaThau_ID ?? default,
                                          HopDong = ca.HopDong,
                                          File_CCAT = ca.File_CCAT,
                                          CapDuyet = (int)kd.CapDuyet,
                                          TinhTrangID = (int)kd.TinhTrangID,
                                          NhanVienID = (int)kd.NhanVienID,
                                          NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                                          GhiChu = kd.GhiChu
                                      }).FirstOrDefault();
                    if (check_list != null)
                    {
                        data.Add(check_list);
                    }
                }
            }

            if (begind == null && endd == null)
            {
                data = data.Where(x => x.NgayTrinh >= startDay && x.NgayTrinh <= endDay && x.TrinhKy_ID == id).ToList();
            }
            else
            {
                data = data.Where(x => x.NgayTrinh >= begind && x.NgayTrinh <= endd && x.TrinhKy_ID == id).ToList();
            }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(x => x.NgayDuyet).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Show(int? page, int? DKTN_ID)
        {
            var res = (from kd in db_dk.SignOff_Flow.Where(x => x.DKTN_ID == DKTN_ID)
                       join ca in db_dk.RegisterPeoples on kd.DKTN_ID equals ca.ID_DKTN
                       select new Follow_RegisterPeopleValidation
                       {
                           ID_TK_TN = (int)kd.ID_TK_TN,
                           DKTN_ID = (int)kd.DKTN_ID,
                           NoiDung = ca.NoiDung,
                           NgayTrinh = (DateTime?)ca.NgayTrinhKy ?? default,
                           NT_ID = (int?)ca.NhaThau_ID ?? default,
                           HopDong = ca.HopDong,
                           File_CCAT = ca.File_CCAT,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.NgayDuyet).ToList().ToPagedList(pageNumber, pageSize));
        }


        public ActionResult Approve(int? idnv, int? id)
        {

            var _DO = db_dk.SignOff_Flow.Where(x => x.NhanVienID == idnv && x.DKTN_ID == id).FirstOrDefault();
            var check = db_dk.SignOff_Flow.Where(x => x.CapDuyet < _DO.CapDuyet && x.TinhTrangID != 1 && x.DKTN_ID == id).Any();
            if (!check)
            {
                var res = (from kd in db_dk.SignOff_Flow.Where(x => x.ID_TK_TN == _DO.ID_TK_TN)
                           select new Follow_RegisterPeopleValidation
                           {
                               ID_TK_TN = (int)kd.ID_TK_TN,
                               DKTN_ID = (int)kd.DKTN_ID,
                               CapDuyet = (int)kd.CapDuyet,
                               TinhTrangID = (int)kd.TinhTrangID,
                               NhanVienID = (int)kd.NhanVienID,
                               NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                               GhiChu = kd.GhiChu
                           }).ToList();
                Follow_RegisterPeopleValidation DO = new Follow_RegisterPeopleValidation();
                if (res.Count > 0)
                {
                    foreach (var kd in res)
                    {
                        DO.ID_TK_TN = (int)kd.ID_TK_TN;
                        DO.DKTN_ID = (int)kd.DKTN_ID;
                        DO.CapDuyet = (int)kd.CapDuyet;
                        DO.TinhTrangID = (int)kd.TinhTrangID;
                        DO.NhanVienID = (int)kd.NhanVienID;
                        DO.NgayDuyet = (DateTime?)kd.NgayDuyet ?? default;
                        DO.GhiChu = kd.GhiChu;
                    }
                }
                else
                {
                    HttpNotFound();
                }
                return PartialView(DO);
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Cấp duyệt trước đó đã hủy hoặc chưa duyệt');</script>";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult Approve(Follow_RegisterPeopleValidation _DO)
        {
            var ID_DKT = db_dk.RegisterPeoples.Where(x=>x.ID_DKTN ==  _DO.DKTN_ID).FirstOrDefault();
            try
            {
                var a = db_dk.SignOff_Flow.Where(x => x.DKTN_ID == _DO.DKTN_ID && x.CapDuyet < _DO.CapDuyet && x.TinhTrangID == 2).Any();
                if (!a)
                {
                    db_dk.SignOff_Flow_Update(_DO.ID_TK_TN, DateTime.Now, 1, _DO.GhiChu);


                    var DO = db_dk.SignOff_Flow.Where(x => x.DKTN_ID == _DO.DKTN_ID && x.TinhTrangID == 0).FirstOrDefault();
                    if (DO == null)
                    {
                        db_dk.RegisterPeople_UpdateFlow(_DO.DKTN_ID, 3);
                    }
                    TempData["msgSuccess"] = "<script>alert('Phê duyệt thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Phiếu này đã được cấp duyệt trước đó hủy');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Phê duyệt thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Approve_RegisterPeople", new {id = ID_DKT.TrinhKy_ID});
        }

        public ActionResult CancelApprove(int? idnv, int? id)
        {
            var _DO = db_dk.SignOff_Flow.Where(x => x.NhanVienID == idnv && x.DKTN_ID == id).FirstOrDefault();
            var check = db_dk.SignOff_Flow.Where(x => x.CapDuyet < _DO.CapDuyet && x.TinhTrangID != 1 && x.DKTN_ID == id).Any();
            if (!check)
            {
                var res = (from kd in db_dk.SignOff_Flow.Where(x => x.ID_TK_TN == _DO.ID_TK_TN)
                           select new Follow_RegisterPeopleValidation
                           {
                               ID_TK_TN = (int)kd.ID_TK_TN,
                               DKTN_ID = (int)kd.DKTN_ID,
                               CapDuyet = (int)kd.CapDuyet,
                               TinhTrangID = (int)kd.TinhTrangID,
                               NhanVienID = (int)kd.NhanVienID,
                               NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                               GhiChu = kd.GhiChu
                           }).ToList();
                Follow_RegisterPeopleValidation DO = new Follow_RegisterPeopleValidation();
                if (res.Count > 0)
                {
                    foreach (var kd in res)
                    {
                        DO.ID_TK_TN = (int)kd.ID_TK_TN;
                        DO.DKTN_ID = (int)kd.DKTN_ID;
                        DO.CapDuyet = (int)kd.CapDuyet;
                        DO.TinhTrangID = (int)kd.TinhTrangID;
                        DO.NhanVienID = (int)kd.NhanVienID;
                        DO.NgayDuyet = (DateTime?)kd.NgayDuyet ?? default;
                        DO.GhiChu = kd.GhiChu;
                    }
                }
                else
                {
                    HttpNotFound();
                }
                return PartialView(DO);
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Cấp duyệt trước đó đã hủy hoặc chưa duyệt');</script>";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult CancelApprove(Follow_RegisterPeopleValidation _DO)
        {
            try
            {
                db_dk.SignOff_Flow_Update(_DO.ID_TK_TN, DateTime.Now, 2, _DO.GhiChu);

                db_dk.RegisterPeople_UpdateFlow(_DO.DKTN_ID, 2);

                TempData["msgSuccess"] = "<script>alert('Hủy duyệt thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Hủy phê duyệt thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Approve");
        }
    }
}