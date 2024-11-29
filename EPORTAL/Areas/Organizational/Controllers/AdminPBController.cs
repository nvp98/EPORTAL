using ExcelDataReader;
using PagedList;
using SoDoToChuc.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsOrganizational;
using EPORTAL.Models;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public class AdminPBController : Controller
    {
        // GET: AdminPB
        SoDoToChucEntities db_context = new SoDoToChucEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "AdminPB";
        public ActionResult Index(int? page, int? NhomID,int? IDPhongBan)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<Nhom> nhom = db_context.Nhoms.ToList();
            if (NhomID == null) NhomID = 0;
            ViewBag.NhomList = new SelectList(nhom, "IDNhom", "TenNhom", NhomID);

            List<PhongBan> pb = db_context.PhongBans.ToList();
            //if (IDPhongBan == null) IDPhongBan = 0;
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan", IDPhongBan);

            var res = (from a in db_context.PhongBans
                       join b in db_context.Nhoms on a.NhomID equals b.IDNhom into ulist
                       from b in ulist.DefaultIfEmpty()
                       select new AdminPB()
                       {
                           IDPhongBan = a.IDPhongBan,
                           TenPhongBan = a.TenPhongBan,
                           NhomID = (int?)a.NhomID??0,
                           TenNhom =b.TenNhom,
                           ImagePath = a.ImagePath,
                           url = a.url,
                           TPDB =(int?)a.TPDB??0,
                           QDTPTDB =(int?)a.QDTPTDB??0,
                           PTDB =(int?)a.PTDB??0,
                           TPKipDB =(int?)a.TPKipDB??0,
                           KTVHCDB =(int?)a.KTVHCDB??0,
                           KTVCaDB =(int?)a.KTVCaDB??0,
                           TTTPHCDB =(int?)a.TTTPHCDB??0,
                           TTTPCaDB =(int?)a.TTTPCaDB??0,
                           NVHCDB =(int?)a.NVHCDB??0,
                           NVCaDB =(int?)a.NVCaDB??0
                       });
            if (NhomID != 0)
               res= res.Where(x => x.NhomID == NhomID);
            if (IDPhongBan != null)
                res = res.Where(x => x.IDPhongBan == IDPhongBan);
            if (page == null) page = 1;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public int? ToNullableInt(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }
        public int? ToNullableZero(int? s)
        {
            if(s != 0)
            {
                return s;
            }
            else
                return null;
        }
        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<Nhom> nhom = db_context.Nhoms.ToList();
            ViewBag.NhomList = new SelectList(nhom, "IDNhom", "TenNhom");

            return PartialView();
        }
       
        [HttpPost]
        public ActionResult Create(AdminPB _DO)
        {
            
            try
            {
                string path = Server.MapPath("~/Images/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //Use Namespace called :  System.IO  
                string FileName = _DO.ImageFile != null ? "PB"+_DO.IDPhongBan : "";

                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.ImagePath = "~/Images/" + FileName;
                }
                var a = db_context.PhongBan_insert(_DO.TenPhongBan, _DO.NhomID, _DO.ImagePath, ToNullableInt(_DO.TPDB.ToString()), ToNullableInt(_DO.QDTPTDB.ToString()), ToNullableInt(_DO.PTDB.ToString()), ToNullableInt(_DO.TPKipDB.ToString()), ToNullableInt(_DO.KTVHCDB.ToString()), ToNullableInt(_DO.KTVCaDB.ToString()), ToNullableInt(_DO.TTTPHCDB.ToString()), ToNullableInt(_DO.TTTPCaDB.ToString()), ToNullableInt(_DO.NVHCDB.ToString()), ToNullableInt(_DO.NVCaDB.ToString()));
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminPB");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db_context.PhongBans.Where(x => x.IDPhongBan == id)
                       select new AdminPB
                       {
                           IDPhongBan = (int)a.IDPhongBan,
                           TenPhongBan =a.TenPhongBan,
                           NhomID =(int?)a.NhomID??0,
                           ImagePath = a.ImagePath,
                           url =a.url,
                           TPDB = (int?)a.TPDB ?? 0,
                           QDTPTDB = (int?)a.QDTPTDB ?? 0,
                           PTDB = (int?)a.PTDB ?? 0,
                           TPKipDB = (int?)a.TPKipDB ?? 0,
                           KTVHCDB = (int?)a.KTVHCDB ?? 0,
                           KTVCaDB = (int?)a.KTVCaDB ?? 0,
                           TTTPHCDB =(int?)a.TTTPHCDB??0,
                           TTTPCaDB =(int?)a.TTTPCaDB??0,
                           NVHCDB = (int?)a.NVHCDB ?? 0,
                           NVCaDB = (int?)a.NVCaDB ?? 0
                       }).ToList();
            AdminPB DO = new AdminPB();
            if (res.Count > 0)
            {
                foreach(var a in res)
                {
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.TenPhongBan = a.TenPhongBan;
                    DO.NhomID = a.NhomID;
                    DO.TenNhom = a.TenNhom;
                    DO.ImagePath = a.ImagePath;
                    DO.url = a.url;
                    DO.TPDB = a.TPDB;
                    DO.QDTPTDB = a.QDTPTDB;
                    DO.PTDB = a.PTDB;
                    DO.TPKipDB = a.TPKipDB;
                    DO.KTVHCDB = a.KTVHCDB;
                    DO.KTVCaDB = a.KTVCaDB;
                    DO.TTTPHCDB = a.TTTPHCDB;
                    DO.TTTPCaDB = a.TTTPCaDB;
                    DO.NVHCDB = a.NVHCDB;
                    DO.NVCaDB = a.NVCaDB;
                }
                List<Nhom> nhom = db_context.Nhoms.ToList();
                ViewBag.NhomID = new SelectList(nhom, "IDNhom", "TenNhom",DO.NhomID);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(AdminPB _DO)
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
                string FileName = _DO.ImageFile != null ? "PB" + _DO.IDPhongBan : "";
                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.ImagePath = "~/Images/" + FileName;
                }
                //if(_DO.NhomID == 0)
                //{
                //    _DO.NhomID = null;
                //}
                var a = db_context.PhongBan_update(_DO.IDPhongBan,_DO.TenPhongBan, ToNullableZero(_DO.NhomID), _DO.ImagePath, ToNullableInt(_DO.TPDB.ToString()), ToNullableInt(_DO.QDTPTDB.ToString()), ToNullableInt(_DO.PTDB.ToString()), ToNullableInt(_DO.TPKipDB.ToString()), ToNullableInt(_DO.KTVHCDB.ToString()), ToNullableInt(_DO.KTVCaDB.ToString()), ToNullableInt(_DO.TTTPHCDB.ToString()), ToNullableInt(_DO.TTTPCaDB.ToString()), ToNullableInt(_DO.NVHCDB.ToString()), ToNullableInt(_DO.NVCaDB.ToString()));
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "AdminPB", new { @NhomID = _DO.NhomID });
        }
        public ActionResult Delete(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db_context.PhongBan_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "AdminPB");
        }
        public int GetIDPhongBan(string TenPB)
        {
            var model = db_context.PhongBans.Where(x => x.TenPhongBan == TenPB).SingleOrDefault();
            if (model == null)
                return 0;
            return model.IDPhongBan;
        }
        //public int? ToNullableInt(string s)
        //{
        //    int i;
        //    if (int.TryParse(s, out i)) return i;
        //    return null;
        //}
        public ActionResult ImportExcel()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            return PartialView();
        }
        [HttpPost]
        public ActionResult ImportExcel(AdminPB _DO)
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
                    //DataTable dtbp = result.Tables[1];

                    reader.Close();
                    int dtc = 0, dtrung = 0;
                    int IDPB;
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {

                            for (int i = 4; i < dt.Rows.Count; i++)
                            {

                                if (dt.Rows[i] != null)
                                {

                                    //var ID = dt.Rows[i][2].ToString();
                                    //var tpdb = dt.Rows[i][3].ToString();
                                    //var qdtptdb = dt.Rows[i][4].ToString();
                                    IDPB = GetIDPhongBan(dt.Rows[i][1].ToString());
                                    //int? aa = ToNullableInt(dt.Rows[i][2].ToString());
                                    //int? aaa = ToNullableInt(dt.Rows[i][3].ToString());
                                    if (IDPB != 0)
                                    {
                                        var a = db_context.PhongBan_update_Excel(IDPB, dt.Rows[i][1].ToString(), ToNullableInt(dt.Rows[i][2].ToString()), ToNullableInt(dt.Rows[i][3].ToString()), ToNullableInt(dt.Rows[i][4].ToString()), ToNullableInt(dt.Rows[i][8].ToString()), ToNullableInt(dt.Rows[i][5].ToString()), ToNullableInt(dt.Rows[i][9].ToString()), ToNullableInt(dt.Rows[i][6].ToString()), ToNullableInt(dt.Rows[i][10].ToString()), ToNullableInt(dt.Rows[i][7].ToString()), ToNullableInt(dt.Rows[i][11].ToString()));
                                        dtc++;
                                    }
                                    if(IDPB ==0)
                                    {
                                        var a = db_context.PhongBan_insert_Excel(dt.Rows[i][1].ToString(), ToNullableInt(dt.Rows[i][2].ToString()), ToNullableInt(dt.Rows[i][3].ToString()), ToNullableInt(dt.Rows[i][4].ToString()), ToNullableInt(dt.Rows[i][8].ToString()), ToNullableInt(dt.Rows[i][5].ToString()), ToNullableInt(dt.Rows[i][9].ToString()), ToNullableInt(dt.Rows[i][6].ToString()), ToNullableInt(dt.Rows[i][10].ToString()), ToNullableInt(dt.Rows[i][7].ToString()), ToNullableInt(dt.Rows[i][11].ToString()));
                                        dtc++;
                                    }    


                                    //else
                                    //{
                                    //    dtrung++;
                                    //}
                                }

                            }
                            string msg = "";
                            if (dtc != 0 && dtrung != 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu, " + "Có " + dtrung + " dòng trùng dữ liệu";
                            }
                            else if (dtc != 0 && dtrung == 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu";
                            }
                            else if (dtc == 0 && dtrung != 0)
                            {
                                msg = "Có " + dtrung + " dòng trùng dữ liệu cập nhập nội dung";
                            }
                            else { msg = "File import không có dữ liệu"; }


                            TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";

                        }
                        catch (Exception ex)
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

            return RedirectToAction("Index", "AdminPB");
        }
        public JsonResult GetNhomList(int id)
        {
            List<PXList> PXList = (from b in db_context.NhomLVs.Where(x => x.IDPhongBan == id)
                                   select new PXList()
                                   {
                                       IDPX = b.IDNhom,
                                       TenPX = b.TenNhom
                                   }).ToList();

            return Json(PXList, JsonRequestBehavior.AllowGet);
        }
    }
}