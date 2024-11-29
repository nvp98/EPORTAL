using SoDoToChuc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using EPORTAL.ModelsOrganizational;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public class AdminNhanVienPVSXController : Controller
    {
        // GET: AdminNhanVienPVSX
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        public ActionResult Index(int? page, int? IDPhongBan )
        {
            List<PhongBan> pb1 = db_context.PhongBans.ToList();
            if (IDPhongBan == null) IDPhongBan = 0;
            ViewBag.PBList = new SelectList(pb1, "IDPhongBan", "TenPhongBan", IDPhongBan);


            var res = (from a in db_context.NhanVienPVSXes
                       join pb in db_context.PhongBans on a.IDPhongBan equals pb.IDPhongBan
                       join tt in db_context.NhanVienAPIs on a.MaNV equals tt.MaNV
                       select new AdminNhanVienPVSX
                       {
                           IDNV = a.IDNV,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           TenPhongBan = pb.TenPhongBan,
                           IDPhongBan =(int)a.IDPhongBan,
                           ImagePath =a.ImagePath, 
                           TenViTri = tt.ViTriLViec.TenViTri,
                           TT_BGD =(int?)tt.TT_BGD??default
                       });
            if (IDPhongBan != 0)
                res = res.Where(x => x.IDPhongBan == IDPhongBan);

            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {

            List<NhomLV> nhom = db_context.NhomLVs.ToList();
            ViewBag.NhomList = new SelectList(nhom, "IDNhom", "TenNhom");

            List<PhongBan> pb = db_context.PhongBans.ToList();
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan");

            List<ChucVu> cv = db_context.ChucVus.ToList();
            ViewBag.CVList = new SelectList(cv, "IDChucVu", "TenChucVu");

            List<ViTriLViec> vt = db_context.ViTriLViecs.ToList();
            ViewBag.VTList = new SelectList(vt, "IDViTri", "TenViTri");

            List<CapBac> cb = db_context.CapBacs.ToList();
            ViewBag.CBList = new SelectList(cb, "TT", "TenCapBac");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(AdminNhanVienPVSX _DO)
        {

            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.ImageFile != null ? _DO.MaNV : "";

                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.ImagePath = "~/Images/" + FileName;
                }
                if (!string.IsNullOrEmpty(_DO.TT_BGD.ToString()) && _DO.TT_BGD != 0)
                {
                    var b = db_context.NhanVien_update_TT(_DO.MaNV, _DO.ImagePath, _DO.TT_BGD);
                }
                var a = db_context.NhanVienPVSX_insert(_DO.MaNV, _DO.HoTen, _DO.IDPhongBan,_DO.IDViTri,_DO.ImagePath,_DO.TT_BGD);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminNhanVienPVSX");
        }
        public int? ToNullableInt(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }
        public ActionResult Edit(int id)
        {
            var res = (from a in db_context.NhanVienPVSXes.Where(x => x.IDNV == id)
                       join b in db_context.NhanVienAPIs on a.MaNV equals b.MaNV
                       select new AdminNhanVienPVSX
                       {
                           IDNV = a.IDNV,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           IDPhongBan = (int)a.IDPhongBan,
                           IDViTri = (int?)a.IDViTri??0,
                           ImagePath =a.ImagePath,
                           TT_BGD =(int?)b.TT_BGD??default
                       }).ToList();
            AdminNhanVienPVSX DO = new AdminNhanVienPVSX();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.HoTen = a.HoTen;
                    DO.DiaChi = a.DiaChi;
                    DO.MaNV = a.MaNV;
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.IDViTri = a.IDViTri;
                    DO.IDNV = a.IDNV;
                    DO.ImagePath = a.ImagePath;
                    DO.TT_BGD = a.TT_BGD;
                }
                //List<NhomLV> px = db_context.NhomLVs.Where(x=>x.IDPhongBan ==DO.IDPhongBan).ToList();
                //ViewBag.IDNhom = new SelectList(px, "IDNhom", "TenNhom", DO.IDNhom);

                //List<ChucVu> cv = db_context.ChucVus.ToList();
                //ViewBag.ChucVuID = new SelectList(cv, "IDChucVu", "TenChucVu", DO.ChucVuID);

                List<PhongBan> pb = db_context.PhongBans.ToList();
                ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDPhongBan);

                //List<CapBac> cb = db_context.CapBacs.ToList();
                //ViewBag.IDCapBac = new SelectList(cb, "TT", "TenCapBac",DO.CapBac);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AdminNhanVienPVSX _DO)
        {

            try
            {
                string path = Server.MapPath("~/Images/");
                //string path ="~/Images/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                //string FileName = _DO.ImageFile !=null?Path.GetFileNameWithoutExtension(_DO.ImageFile.FileName):"";
                string FileName = _DO.ImageFile != null ? _DO.MaNV : "";
                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName):"";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile !=null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.ImagePath = "~/Images/" + FileName;
                }
                if(!string.IsNullOrEmpty(_DO.TT_BGD.ToString()) && _DO.TT_BGD !=0)
                {
                    var b = db_context.NhanVien_update_TT(_DO.MaNV, _DO.ImagePath, _DO.TT_BGD);
                }    
                var a = db_context.NhanVienPVSX_update(_DO.IDNV,_DO.MaNV, _DO.HoTen, _DO.IDPhongBan,_DO.ImagePath,_DO.TT_BGD);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminNhanVienPVSX",new { @IDPhongBan =_DO.IDPhongBan });
        }
        public ActionResult Delete(int id)
        {
            try
            {
                db_context.NhanVienPVSX_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AdminNhanVienPVSX");
        }
        public JsonResult GetNhomList(int id)
        {
            List<NhomList> NList = (from b in db_context.NhomLVs.Where(x => x.IDPhongBan == id)
                                   select new NhomList()
                                   {
                                       IDNhom = b.IDNhom,
                                       TenNhom = b.TenNhom
                                   }).ToList();

            return Json(NList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetNV(string MaNV)
        {
            List<GetNVAPI> NV = (from b in db_context.NhanVienAPIs.Where(x => x.MaNV == MaNV)
                                 join c in db_context.PhongBans on b.IDPhongBan equals c.IDPhongBan
                                 select new GetNVAPI()
                                 {
                                     MaNV = b.MaNV,
                                     HoTen = b.HoTen,
                                     DiaChi = b.DiaChi,
                                     DienThoai = b.DienThoai,
                                     TenPhongBan = c.TenPhongBan,
                                     IDPhongBan = (int)b.IDPhongBan,
                                     IDViTri = (int)b.IDViTri,
                                     TenViTri = b.ViTriLViec.TenViTri
                                 }).ToList();

            return Json(NV, JsonRequestBehavior.AllowGet);
        }
    }
}