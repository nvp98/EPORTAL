using EPORTAL.ModelsDanhBaDoiTac;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class ManageNotifyController : Controller
    {
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: Notify_Partner
        public ActionResult Index(int? page)
        {
            var res = (from t in db_context.DB_ThongBao
                       join a in db_context.DB_TinhTrangThongBao on t.TinhTrangTB equals a.IDTinhTrangTB
                       select new NotifyValidation
                       {
                           IDTBao = t.IDTBao,
                           MaTBao = t.MaTBao,
                           NoiDungTBao = t.NoiDungTBao,
                           FileDinhKem = t.FileDinhKem,
                           Ngay = (DateTime)t.Ngay,
                           TinhTrangTB = (int)a.IDTinhTrangTB,
                           TinhTrangThongBao = a.TinhTrangThongBao
                           //LinhVuc = d.LinhVucHĐ
                       }).OrderByDescending(x => x.Ngay).ToList();
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }



        public ActionResult Create()
        {
            List<DB_TinhTrangThongBao> tttb = db_context.DB_TinhTrangThongBao.ToList();
            ViewBag.TTTBList = new SelectList(tttb, "IDTinhTrangTB", "TinhTrangThongBao");
            return PartialView();

        }

        [HttpPost]
        public ActionResult Create(NotifyValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/UploadedFiles/Notify/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.MaTBao : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";

                ////Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.FileDinhKem = "/UploadedFiles/Notify/" + FileName;
                }

                //Check trùng mã Thông Báo
                if (IsTBAvailable(_DO.MaTBao) == false)
                {
                    db_context.DB_ThongBao_insert(_DO.MaTBao, _DO.NoiDungTBao, _DO.FileDinhKem, _DO.Ngay, _DO.TinhTrangTB);
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Mã Thông Báo đã tồn tại');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ManageNotify");

        }


        public ActionResult Edit(int id)
        {

            var res = (from c in db_context.DB_ThongBao
                       join d in db_context.DB_TinhTrangThongBao on c.TinhTrangTB equals d.IDTinhTrangTB
                       select new NotifyValidation
                       {
                           IDTBao = c.IDTBao,
                           MaTBao = c.MaTBao,
                           NoiDungTBao = c.NoiDungTBao,
                           FileDinhKem = c.FileDinhKem,
                           Ngay = (DateTime)c.Ngay,
                           TinhTrangTB = (int)d.IDTinhTrangTB


                       }).ToList();
            NotifyValidation DO = new NotifyValidation();

            if (res.Count > 0)
            {
                foreach (var co in res)
                {
                    DO.IDTBao = co.IDTBao;
                    DO.MaTBao = co.MaTBao;
                    DO.NoiDungTBao = co.NoiDungTBao;
                    DO.FileDinhKem = co.FileDinhKem;
                    DO.Ngay = co.Ngay;
                    DO.TinhTrangTB = co.TinhTrangTB;
                }
                List<DB_TinhTrangThongBao> tttb = db_context.DB_TinhTrangThongBao.ToList();
                ViewBag.TTTBList = new SelectList(tttb, "IDTinhTrangTB", "TinhTrangThongBao1");
                ViewBag.Ngay = DO.Ngay.ToString("yyyy-MM-dd");
            }
            else
            {
                return HttpNotFound();
            }

            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Edit(NotifyValidation _DO)
        {
            try
            {

                string path = Server.MapPath("~/UploadedFiles/Notify/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.NotiFile != null ? _DO.MaTBao : "";

                //To Get File Extension  
                string FileExtension = _DO.NotiFile != null ? Path.GetExtension(_DO.NotiFile.FileName) : "";


                //Add Current Date To Attached File Name  
                if (_DO.NotiFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.NotiFile.SaveAs(path + FileName);
                    _DO.FileDinhKem = "/UploadedFiles/Notify/" + FileName;
                }

                db_context.DB_ThongBao_update(_DO.IDTBao, _DO.MaTBao, _DO.NoiDungTBao, _DO.FileDinhKem, _DO.Ngay, _DO.TinhTrangTB);

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + e.Message + " ');</script>";
            }

            return RedirectToAction("Index", "ManageNotify");
        }



        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_ThongBao_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManageNotify");
        }



        public ActionResult NotifyTo(int? page, int? id)
        {
            var model = db_context.DB_ThongBao.Where(x => x.IDTBao == id).FirstOrDefault();
            //ViewBag.NDTBao = model.NoiDungTBao;

            var rs = (from a in db_context.DB_ThongBao_DoiTac.Where(a => a.TBID == id)
                      join b in db_context.DB_TTDoiTac on a.DoiTacID equals b.IDDoiTac
                      select new Notify_PartnerValidation
                      {
                          IDTBDT = a.ID_TBDT,

                          IDDoiTac = b.IDDoiTac,
                          IDBP = b.BPID,
                          ShortName = b.ShortName,
                      }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(rs.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult AddPermissionall(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_ThongBao> tb = db_context.DB_ThongBao.Where(x => x.IDTBao == id).ToList();
            ViewBag.TBList = new SelectList(tb, "IDTBao", "NoiDungTBao");

            List<DB_TTDoiTac> dt = db_context.DB_TTDoiTac.ToList();
            ViewBag.DTList = new SelectList(dt, "IDDoiTac", "TenQT");

            return PartialView();
        }
        [HttpPost]
        public ActionResult AddPermissionall(Notify_PartnerValidation _DO)
        {
            try
            {
                //var MaNV = db_context.NhanViens.Where(x => x.ID == _DO.NVID).Select(g => g.MaNV).FirstOrDefault();
                if (IsTBDTAvailable(_DO.TBaoID, _DO.IDDoiTac) == false)
                {
                    var a = db_context.DB_TTDoiTac.ToList();
                    foreach (var aa in a)
                    {
                        db_context.DB_ThongBaoDoiTac_insert(_DO.TBaoID, aa.IDDoiTac);
                    }


                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Đối tác đã tồn tại');</script>";
                }

            }

            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm tác: " + e.Message + "');</script>";
            }
            return RedirectToAction("NotifyTo", "ManageNotify", new { id = _DO.TBaoID });

        }
        public ActionResult AddPermission(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            List<DB_ThongBao> tb = db_context.DB_ThongBao.Where(x => x.IDTBao == id).ToList();
            ViewBag.TBList = new SelectList(tb, "IDTBao", "NoiDungTBao");

            List<DB_TTDoiTac> dt = db_context.DB_TTDoiTac.ToList();
            ViewBag.DTList = new SelectList(dt, "IDDoiTac", "BPID");

            return PartialView();
        }
        [HttpPost]
        public ActionResult AddPermission(Notify_PartnerValidation _DO)
        {
            try
            {
                //var MaNV = db_context.NhanViens.Where(x => x.ID == _DO.NVID).Select(g => g.MaNV).FirstOrDefault();
                if (IsTBDTAvailable(_DO.TBaoID, _DO.IDDoiTac) == false)
                {

                    db_context.DB_ThongBaoDoiTac_insert(_DO.TBaoID, _DO.IDDoiTac);
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Đối tác đã tồn tại');</script>";
                }

            }

            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm tác: " + e.Message + "');</script>";
            }
            return RedirectToAction("NotifyTo", "ManageNotify", new { id = _DO.TBaoID });

        }
        public ActionResult DeletePart(int id)
        {
            db_context.Configuration.ProxyCreationEnabled = false;
            var idt = db_context.DB_ThongBao_DoiTac.Where(x => x.ID_TBDT == id).Select(x => x.TBID).FirstOrDefault();
            try
            {
                db_context.DB_ThongBaoDoiTac_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("NotifyTo", "ManageNotify", new { id = idt });
        }

        public void InsertDSDT(int? TBaoID, int? DoiTacID)
        {
            db_context.DB_ThongBaoDoiTac_insert(TBaoID, DoiTacID);
        }



        //Func check trùng mã Thông Báo.
        public bool IsTBAvailable(string MaTBao)
        {
            var IsCheck = (from t in db_context.DB_ThongBao
                           where (t.MaTBao.ToLower() == MaTBao)
                           select new { t.MaTBao }).FirstOrDefault();
            bool status;
            if (IsCheck != null)
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return status;
        }

        // Check Doi Tac có tồn tại.
        public bool IsDTAvailable(int DTacID)
        {
            var IsCheck = (from td in db_context.DB_ThongBao_DoiTac
                           join t in db_context.DB_TTDoiTac
                           on td.DoiTacID equals t.IDDoiTac
                           where (t.IDDoiTac == DTacID)
                           select new { t.ShortName }).FirstOrDefault();
            bool status;
            if (IsCheck != null)
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return status;
        }


        // Check Thong Bao Doi Tac.
        public bool IsTBDTAvailable(int TBaoID, int DTacID)
        {
            var IsCheck = (from h in db_context.DB_ThongBao_DoiTac
                           join l in db_context.DB_ThongBao
                           on h.TBID equals l.IDTBao
                           join n in db_context.DB_TTDoiTac
                           on h.DoiTacID equals n.IDDoiTac
                           where (l.IDTBao == TBaoID && n.IDDoiTac == DTacID)
                           select new { l.MaTBao, n.ShortName }).FirstOrDefault();
            bool status;
            if (IsCheck != null)
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return status;
        }


    }
}