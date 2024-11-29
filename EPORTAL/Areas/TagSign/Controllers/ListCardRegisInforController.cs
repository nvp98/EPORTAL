using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Wordprocessing;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using EPORTAL.Models;
using Font = iTextSharp.text.Font;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsOrganizational;
using System.Web.UI.WebControls;
using iTextSharp.text.pdf.codec;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ListCardRegisInforController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        EPORTAL_UNISEntities dbUN = new EPORTAL_UNISEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListCardRegisInfor";
        // GET: TagSign/ListCardRegisInfor
        public ActionResult Index(int? page, string search, DateTime? begind, DateTime? endd, int? IDList)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            List<NT_UserTemp> us = db_nt.NT_UserTemp.ToList();
            ViewBag.IDList = new SelectList(us, "ID", "UserName");
            var res = (from a in db_dk.DK_CardRegistrationInfor
                       select new DK_ListCardRegisInforNTValidation
                       {
                           IDDKT = (int)a.IDDKT,
                           NoiDung = a.NoiDung,
                           NTID = (int)a.NTID,
                           HDID = (int?)a.HDID ?? default,
                           PhongBanID = (int?)a.PhongBanID ?? default,
                           NgayDangKy = (DateTime)a.NgayDangKy,
                           NhanVienID = (int?)a.NhanVienID?? default,
                           TinhTrangID = (int?)a.TinhTrangID ?? default,
                           FileUpload = a.FileUpload,
                           GhiChu = a.GhiChu,
                           LoaiNT = (int)a.LoaiNT
                       }).ToList();
            if (begind != null && endd != null && IDList != null)
            {
                res = res.OrderByDescending(x => x.NgayDangKy).Where(x => x.NgayDangKy >= begind && x.NgayDangKy <= endd && x.NhanVienID == IDList).ToList();
            }
            else if (begind != null && endd != null)
            {
                res = res.OrderByDescending(x => x.NgayDangKy).Where(x => x.NgayDangKy >= begind && x.NgayDangKy <= endd).ToList();
            }
            else if(IDList != null)
            {
                res = res.OrderByDescending(x => x.NgayDangKy).Where(x => x.NhanVienID == IDList).ToList();
            }    

            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            return View(res.OrderByDescending(x => x.NgayDangKy).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Edit(int id)
        {
            var res = (from a in db_dk.DK_CardRegistrationInfor.Where(x=>x.IDDKT == id)
                       select new DK_ListCardRegisInforNTValidation
                       {
                           IDDKT = (int)a.IDDKT,
                           NoiDung = a.NoiDung,
                           NTID = (int)a.NTID,
                           HDID = (int?)a.HDID ?? default,
                           PhongBanID = (int?)a.PhongBanID ?? default,
                           NgayDangKy = (DateTime)a.NgayDangKy,
                           NhanVienID = (int?)a.NhanVienID ?? default,
                           TinhTrangID = (int?)a.TinhTrangID ?? default,
                           FileUpload = a.FileUpload,
                           FileComplete = a.FileComplete,
                           GhiChu = a.GhiChu,
                           LoaiNT = (int)a.LoaiNT
                       }).ToList();
            DK_ListCardRegisInforNTValidation DO = new DK_ListCardRegisInforNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDDKT = (int)a.IDDKT;
                    DO.NoiDung = a.NoiDung;
                    DO.NTID = (int)a.NTID;
                    DO.HDID = (int?)a.HDID ?? default;
                    DO.PhongBanID = (int?)a.PhongBanID ?? default;
                    DO.NgayDangKy = (DateTime)a.NgayDangKy;
                    DO.NhanVienID = (int?)a.NhanVienID ?? default;
                    DO.TinhTrangID = (int?)a.TinhTrangID ?? default;
                    DO.FileUpload = a.FileUpload;
                    DO.FileComplete = a.FileComplete;
                    DO.GhiChu = a.GhiChu;
                    DO.LoaiNT = (int)a.LoaiNT;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(DK_ListCardRegisInforNTValidation _DO)
        {
            try
            {
                  var Check = db_dk.DK_CardRegistrationInfor.Where(x=>x.IDDKT == _DO.IDDKT).FirstOrDefault();
                    //Upload file PDF ĐK học ATLĐ
                    if(_DO.FilePDF != null && _DO.FileANH == null)
                    {
                        string path = Server.MapPath("~/PDFHocAT/");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        //Use Namespace called :  System.IO  
                        string FileName = _DO.FilePDF != null ? "PDF" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                        //To Get File Extension  
                        string FileExtension = _DO.FilePDF != null ? Path.GetExtension(_DO.FilePDF.FileName) : "";


                        ////Add Current Date To Attached File Name  
                        if (_DO.FilePDF != null)
                        {
                            FileName = FileName.Trim() + FileExtension;
                            _DO.FilePDF.SaveAs(path + FileName);
                            _DO.FileUpload = "~/PDFHocAT/" + FileName;
                        }
                        else
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                            return RedirectToAction("Index", "ListCardRegisInfor");

                        }

                          var a = db_dk.DK_CardRegistrationInfor_Up(Check.IDDKT,_DO.NhanVienID, _DO.FileUpload, Check.FileComplete);
                    }
                    else if(_DO.FilePDF == null && _DO.FileANH != null)
                    {
                        string paths = Server.MapPath("~/PDFHocAT/");
                        if (!Directory.Exists(paths))
                        {
                            Directory.CreateDirectory(paths);
                        }
                        //Use Namespace called :  System.IO  
                        string FileNames = _DO.FileANH != null ? "Folder" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                        //To Get File Extension  
                        string FileExtensions = _DO.FileANH != null ? Path.GetExtension(_DO.FileANH.FileName) : "";


                        ////Add Current Date To Attached File Name  
                        if (_DO.FileANH != null)
                        {
                            FileNames = FileNames.Trim() + FileExtensions;
                            _DO.FileANH.SaveAs(paths + FileNames);
                            _DO.FileComplete = "~/PDFHocAT/" + FileNames;
                        }
                        else
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Folder Ảnh');</script>";
                            return RedirectToAction("Index", "ListCardRegisInfor");

                        }
                          var a = db_dk.DK_CardRegistrationInfor_Up(Check.IDDKT,_DO.NhanVienID, Check.FileUpload, _DO.FileComplete);
                    }
                    else if(_DO.FilePDF != null && _DO.FileANH != null)
                    {
                        string path = Server.MapPath("~/PDFHocAT/");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        //Use Namespace called :  System.IO  
                        string FileName = _DO.FilePDF != null ? "PDF" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                        //To Get File Extension  
                        string FileExtension = _DO.FilePDF != null ? Path.GetExtension(_DO.FilePDF.FileName) : "";


                        ////Add Current Date To Attached File Name  
                        if (_DO.FilePDF != null)
                        {
                            FileName = FileName.Trim() + FileExtension;
                            _DO.FilePDF.SaveAs(path + FileName);
                            _DO.FileUpload = "~/PDFHocAT/" + FileName;
                        }
                        else
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                            return RedirectToAction("Index", "ListCardRegisInfor");

                        }


                         string paths = Server.MapPath("~/PDFHocAT/");
                        if (!Directory.Exists(paths))
                        {
                            Directory.CreateDirectory(paths);
                        }
                        //Use Namespace called :  System.IO  
                        string FileNames = _DO.FileANH != null ? "Folder" + DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                        //To Get File Extension  
                        string FileExtensions = _DO.FileANH != null ? Path.GetExtension(_DO.FileANH.FileName) : "";


                        ////Add Current Date To Attached File Name  
                        if (_DO.FileANH != null)
                        {
                            FileNames = FileNames.Trim() + FileExtensions;
                            _DO.FileANH.SaveAs(paths + FileNames);
                            _DO.FileComplete = "~/PDFHocAT/" + FileNames;
                        }
                        else
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Folder Ảnh');</script>";
                            return RedirectToAction("Index", "ListCardRegisInfor");

                        }
                         var a = db_dk.DK_CardRegistrationInfor_Up(Check.IDDKT,_DO.NhanVienID, _DO.FileUpload, _DO.FileComplete);
                    }
                    else if (_DO.FilePDF == null && _DO.FileANH == null)
                    {
                        var a = db_dk.DK_CardRegistrationInfor_Up(Check.IDDKT, _DO.NhanVienID, Check.FileUpload, Check.FileComplete);
                    }


                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ListCardRegisInfor");
        }

        public ActionResult Indes (int? page, string search, DateTime? begind, DateTime? endd, int? IDList)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NT_UserTemp> us = db_nt.NT_UserTemp.ToList();
            ViewBag.IDList = new SelectList(us, "ID", "UserName");
            var res = (from a in db_dk.DK_CardExtend
                       select new DK_ListExtendcardNTValidation
                       {
                           IDGHT = (int)a.IDGHT,
                           NoiDung = a.NoiDung,
                           NTID = (int)a.NTID,
                           HDID = (int?)a.HDID ?? default,
                           PhongBanID = (int?)a.PhongBanID ?? default,
                           NgayDangKy = (DateTime)a.NgayDangKy,
                           NhanVienID = (int?)a.NhanVienID ?? default,
                           TinhTrangID = (int?)a.TinhTrangID ?? default,
                           GhiChu = a.GhiChu,
                           LoaiNT = (int)a.LoaiNT
                       }).ToList();

            if (begind != null && endd != null && IDList != null)
            {
                res = res.OrderByDescending(x => x.NgayDangKy).Where(x => x.NgayDangKy >= begind && x.NgayDangKy <= endd && x.NhanVienID == IDList).ToList();
            }
            else if (begind != null && endd != null)
            {
                res = res.OrderByDescending(x => x.NgayDangKy).Where(x => x.NgayDangKy >= begind && x.NgayDangKy <= endd).ToList();
            }
            else if (IDList != null)
            {
                res = res.OrderByDescending(x => x.NgayDangKy).Where(x => x.NhanVienID == IDList).ToList();
            }
            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            return View(res.OrderByDescending(x => x.NgayDangKy).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Edits(int id)
        {
            var res = (from a in db_dk.DK_CardExtend.Where(x => x.IDGHT == id)
                       select new DK_ListExtendcardNTValidation
                       {
                           IDGHT = (int)a.IDGHT,
                           NoiDung = a.NoiDung,
                           NTID = (int)a.NTID,
                           HDID = (int?)a.HDID ?? default,
                           PhongBanID = (int?)a.PhongBanID ?? default,
                           NgayDangKy = (DateTime)a.NgayDangKy,
                           NhanVienID = (int?)a.NhanVienID ?? default,
                           TinhTrangID = (int?)a.TinhTrangID ?? default,
                           FileComplete = a.FileComplete,
                           GhiChu = a.GhiChu,
                           LoaiNT = (int)a.LoaiNT
                       }).ToList();
            DK_ListExtendcardNTValidation DO = new DK_ListExtendcardNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDGHT = (int)a.IDGHT;
                    DO.NoiDung = a.NoiDung;
                    DO.NTID = (int)a.NTID;
                    DO.HDID = (int?)a.HDID ?? default;
                    DO.PhongBanID = (int?)a.PhongBanID ?? default;
                    DO.NgayDangKy = (DateTime)a.NgayDangKy;
                    DO.NhanVienID = (int?)a.NhanVienID ?? default;
                    DO.TinhTrangID = (int?)a.TinhTrangID ?? default;
                    DO.FileComplete = a.FileComplete;
                    DO.GhiChu = a.GhiChu;
                    DO.LoaiNT = (int)a.LoaiNT;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edits(DK_ListExtendcardNTValidation _DO)
        {


            try
            {
                string path = Server.MapPath("~/PDFHocAT/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.FileAnh != null ? DateTime.Now.ToString("yyyyMMddHHmmss") : "";

                //To Get File Extension  
                string FileExtension = _DO.FileAnh != null ? Path.GetExtension(_DO.FileAnh.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.FileAnh != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.FileAnh.SaveAs(path + FileName);
                    _DO.FileComplete = "~/PDFHocAT/" + FileName;
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                    return RedirectToAction("Index", "ListCardRegisInforNT");

                }
                var Check = db_dk.DK_CardExtend.Where(x => x.IDGHT == _DO.IDGHT).FirstOrDefault();
                if (Check != null && _DO.FileComplete != null)
                {
                    var a = db_dk.DK_CardExtend_Up(Check.IDGHT, _DO.NhanVienID, _DO.FileComplete);
                }
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Indes", "ListCardRegisInfor");
        }
    }
}