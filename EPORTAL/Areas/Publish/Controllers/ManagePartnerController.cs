using EPORTAL.ModelsDanhBaDoiTac;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Publish.Controllers
{
    public class ManagePartnerController : Controller
    {
        // GET: Publish/ManagePartner
        DanhBaDoiTacEntities db_context = new DanhBaDoiTacEntities();
        // GET: ManagePartner
        public ActionResult Index(int? page, int? id)
        {
            List<DB_LinhVucHoatDong> lvhd = db_context.DB_LinhVucHoatDong.ToList();
            ViewBag.LinhVucHĐ = new SelectList(lvhd, "IDLinhVucHĐ", "TenLinhVucHĐ");

            List<DB_LoaiDoiTac> ldt = db_context.DB_LoaiDoiTac.ToList();
            ViewBag.LoaiIDDT = new SelectList(ldt, "IDLoaiDT", "TenLoaiDT");
            var res = (from d in db_context.DB_TTDoiTac
                       join f in db_context.DB_DiaChi on d.City equals f.IDDiaChi
                       select new ManagePartnerValidation
                       {
                           IDDoiTac = d.IDDoiTac,
                           IDBP = d.BPID,
                           LinhVucHĐ = d.LinhVucHĐ,
                           //TenLVHĐ = e.TenLinhVucHĐ,
                           LoaiIDDT = d.LoaiIDDT,
                           //TenLoaiDT = l.TenLoaiDT,
                           FullName = d.FullName,
                           ShortName = d.ShortName,
                           TaxCode = d.Taxcode,
                           Address = d.Address,
                           City = (int)d.City,
                           CityName = f.DiaChi,
                           Regions = (int)d.Regions,
                           RegionsName = f.Mien,
                           Customer = d.Customer,
                           ImageLogo = d.ImageLogo,
                           Email = d.Email,
                           Vender = d.Vender

                       }).ToList();
            if (id != null)
            {
                var Idds = db_context.DB_TTDoiTac.Where(x => x.IDDoiTac == id).SingleOrDefault();
                ManagePartnerValidation iddoitac = new ManagePartnerValidation()
                {
                    IDDoiTac = (int)id
                };
                ViewData["id"] = iddoitac;
            }
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }



        public ActionResult Create(int? id)
        {
            List<DB_DiaChi> dc = db_context.DB_DiaChi.ToList();
            ViewBag.DCList = new SelectList(dc, "IDDiaChi", "DiaChi");

            List<DB_DiaChi> m = db_context.DB_DiaChi.ToList();
            ViewBag.MList = new SelectList(m, "IDDiaChi", "Mien");

            List<DB_LinhVucHoatDong> lvhd = db_context.DB_LinhVucHoatDong.ToList();
            ViewBag.LVHDList = new SelectList(lvhd, "IDLinhVucHĐ", "TenLinhVucHĐ");
            List<DB_LoaiDoiTac> ldt = db_context.DB_LoaiDoiTac.ToList();
            ViewBag.LoaiIDDT = new SelectList(ldt, "IDLoaiDT", "TenLoaiDT");
            
            return PartialView();

        }

        [HttpPost]
        public ActionResult Create(ManagePartnerValidation _DO)
        {

            try
            {
                string path = Server.MapPath("~/UploadedFiles/Logo/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.ImageFile != null ? _DO.IDBP : "";

                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.ImageLogo = "/UploadedFiles/Logo/" + FileName;
                }





                //Check trùng mã Đối tác
                if (IsDTAvailable(_DO.IDBP) == false)
                {
                    db_context.DB_TTDoiTac_insert(_DO.IDBP, _DO.FullName, _DO.ShortName, _DO.TaxCode, _DO.Address, _DO.City, _DO.Regions,
                        _DO.ImageLogo, _DO.Customer, _DO.Vender, _DO.Email, _DO.LinhVucHĐ, _DO.LoaiIDDT);
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Mã Đối tác đã tồn tại');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ManagePartner");

        }


        public ActionResult ImportExcel()
        {
            return PartialView();
        }
        [HttpPost]

        public ActionResult ImportExcel(ManagePartnerValidation _DO)
        {

            string filePath = string.Empty;
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["FileUpload"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string path = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    filePath = path + Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm") + "-" + file.FileName);

                    file.SaveAs(filePath);
                    Stream stream = file.InputStream;
                    // We return the interface, so that  
                    IExcelDataReader reader = null;
                    if (file.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (file.FileName.EndsWith(".xlsx"))
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
                    int dtc = 0, dtrung = 0;
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {

                            for (int i = 5; i < dt.Rows.Count; i++)
                            {

                                if (dt.Rows[i] != null)
                                {

                                    var IDBP = dt.Rows[i][0].ToString();
                                    var FullName = dt.Rows[i][1].ToString();
                                    var ShortName = dt.Rows[i][2].ToString();
                                    var TaxCode = dt.Rows[i][3].ToString();
                                    var Address = dt.Rows[i][4].ToString();
                                    int City = Convert.ToInt32(dt.Rows[i][5]);
                                    int Regions = Convert.ToInt32(dt.Rows[i][6]);
                                    var ImageLogo = dt.Rows[i][7].ToString();
                                    var Customer = dt.Rows[i][8].ToString();
                                    var Vender = dt.Rows[i][9].ToString();
                                    var Email = dt.Rows[i][10].ToString();
                                    var LinhVucHĐ = dt.Rows[i][11].ToString();
                                    var LoaiDTID = dt.Rows[i][12].ToString();


                                    //var NVID = db_context.LoaiDoiTacs.Where(x => x.IDLoaiDT == LoaiDTID).Select(g => g.IDLoaiDT).FirstOrDefault();

                                    if (IsDTAvailable(IDBP) == false)
                                    {

                                        InsertDSDoiTac(IDBP, FullName, ShortName, TaxCode, Address, City, Regions, ImageLogo, Customer, Vender, Email, LinhVucHĐ, LoaiDTID);
                                        dtc++;

                                    }
                                    else
                                    {
                                        dtrung++;
                                    }
                                }

                            }
                            string msg = "";
                            if (dtc != 0 && dtrung != 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu, " + "Có " + dtrung + " dòng trùng Mã NV cập nhập nội dung";
                            }
                            else if (dtc != 0 && dtrung == 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu";
                            }
                            else if (dtc == 0 && dtrung != 0)
                            {
                                msg = "Có " + dtrung + " dòng trùng Mã NV cập nhập nội dung";
                            }
                            else { msg = "File import không có dữ liệu"; }


                            TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";

                        }
                        catch (Exception e)
                        {
                            TempData["msgError"] = "<script>alert('File import nội dung không đúng. Vui lòng kiểm tra lại nội dung');</script>";
                        }
                    }
                    else
                    {
                        TempData["msgError"] = "<script>alert('File import không đúng định dạng. Vui lòng kiểm tra biểu mẫu file import');</script>";
                    }

                }
                else
                {
                    TempData["msgError"] = "<script>alert('Vui lòng nhập file Import');</script>";
                }
            }
            else
            {
                TempData["msgError"] = "<script>alert('Vui lòng nhập file Import');</script>";
            }

            return RedirectToAction("Index", "ManagePartner");
        }



        public void InsertDSDoiTac(string IDBP, string FullName, string ShortName, string TaxCode, string Address, int City, int Regions, string ImageLogo, string Customer, string Vender, string Email, string LinhVucHĐ, string LoaiDTID)
        {
            db_context.DB_TTDoiTac_insert(IDBP, FullName, ShortName, TaxCode, Address, City, Regions, ImageLogo, Customer, Vender, Email, LinhVucHĐ, LoaiDTID);
        }


        public ActionResult Edit(int id)
        {
            var res = (from c in db_context.DB_TTDoiTac
                       select new ManagePartnerValidation
                       {
                           IDDoiTac = c.IDDoiTac,
                           IDBP = c.BPID,
                           FullName = c.FullName,
                           ShortName = c.ShortName,
                           TaxCode = c.Taxcode,
                           Address = c.Address,
                           City = (int)c.City,
                           Regions = (int)c.Regions,
                           ImageLogo = c.ImageLogo,
                           Customer = c.Customer,
                           Vender = c.Vender,
                           Email = c.Email,
                           LinhVucHĐ = c.LinhVucHĐ,
                           LoaiIDDT = c.LoaiIDDT
                       }).ToList();
            ManagePartnerValidation DO = new ManagePartnerValidation();

            if (res.Count > 0)
            {
                foreach (var co in res)
                {

                    DO.IDDoiTac = co.IDDoiTac;
                    DO.IDBP = co.IDBP;
                    DO.LinhVucHĐ = co.LinhVucHĐ;
                    DO.LoaiIDDT = co.LoaiIDDT;
                    DO.FullName = co.FullName;
                    DO.ShortName = co.ShortName;
                    DO.TaxCode = co.TaxCode;
                    DO.Address = co.Address;
                    DO.City = co.City;
                    DO.Regions = co.Regions;
                    DO.ImageLogo = co.ImageLogo;
                    DO.Customer = co.Customer;
                    DO.Vender = co.Vender;
                    DO.Email = co.Email;

                }

                List<DB_LoaiDoiTac> dm = db_context.DB_LoaiDoiTac.ToList();
                ViewBag.DMDTList = new SelectList(dm, "IDLoaiDT", "TenLoaiDT");
                List<DB_DiaChi> dc = db_context.DB_DiaChi.ToList();
                ViewBag.DCList = new SelectList(dc, "IDDiaChi", "DiaChi1");
                List<DB_LinhVucHoatDong> lvhd = db_context.DB_LinhVucHoatDong.ToList();
                ViewBag.LVHDList = new SelectList(lvhd, "IDLinhVucHĐ", "LinhVucHĐ");


            }
            else
            {
                return HttpNotFound();
            }


            return PartialView(DO);
        }

        [HttpPost]
        public ActionResult Edit(ManagePartnerValidation _DO)
        {
            try
            {

                string path = Server.MapPath("~/UploadedFiles/Logo/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.ImageFile != null ? _DO.IDBP : "";

                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                //Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.ImageLogo = "/UploadedFiles/Logo/" + FileName;
                }



                db_context.DB_TTDoiTac_update(_DO.IDDoiTac, _DO.IDBP, _DO.FullName, _DO.ShortName, _DO.TaxCode, _DO.Address, _DO.City, _DO.Regions,
                        _DO.ImageLogo, _DO.Customer, _DO.Vender, _DO.Email, _DO.LinhVucHĐ, _DO.LoaiIDDT);

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + e.Message + " ');</script>";
            }

            return RedirectToAction("Index", "ManagePartner");
        }


        public ActionResult Delete(int id)
        {
            try
            {
                db_context.DB_TTDoiTac_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ManagePartner");
        }


        //Func check trùng mã Đối tác
        public bool IsDTAvailable(string MaDoiTac)
        {
            var IsCheck = (from dt in db_context.DB_TTDoiTac
                           where (dt.BPID.ToLower() == MaDoiTac)
                           select new { dt.BPID }).FirstOrDefault();
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