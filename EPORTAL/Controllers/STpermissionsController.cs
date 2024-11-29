using DocumentFormat.OpenXml.Office2010.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsServey;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Controllers
{
    public class STpermissionsController : Controller
    {
        // GET: STpermissions
        PhanQuyenHTEntities db = new PhanQuyenHTEntities();
        EPORTALEntities dbE = new EPORTALEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "STpermissions";
        public ActionResult Index(int? page)
        {
            if (IDQuyenHT != 1)
            {
                    return RedirectToAction("Index", "ListProject", new { area = "View360" });
            }

            var res = db.A_QuyenHT.ToList();

            if (page == null) page = 1;
            int pageSize = res.Count() != 0 ? res.Count() : 50;
            int pageNumber = 1;
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            if (IDQuyenHT != 1)
            {
                return RedirectToAction("Index", "ListProject", new { area = "View360" });
            }
            //db.Configuration.ProxyCreationEnabled = false;
            return PartialView();
        }
        public int GetIDGropQuyen(string TenQuyen)
        {
            var model = db.A_QuyenHT.Where(x => x.TenQuyenHT == TenQuyen).SingleOrDefault();
            if (model == null)
                return 0;
            return model.IDQuyenHT;
        }

        [HttpPost]
        public ActionResult Create(A_GroupQuyenDetailsValidation _DO)
        {
            try
            {
                if (_DO.TenQuyenHT != null && GetIDGropQuyen(_DO.TenQuyenHT.Trim()) == 0)
                {
                    var aa = db.A_QuyenHT_insert(_DO.TenQuyenHT);
                }

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "STpermissions");
        }
        public ActionResult Edit(int id)
        {
            if (IDQuyenHT != 1)
            {
                return RedirectToAction("Index", "ListProject", new { area = "View360" });
            }
            var res = (from a in db.A_QuyenHT.Where(x => x.IDQuyenHT == id)
                       select new A_GroupQuyenDetailsValidation
                       {
                           IDQuyenHT = a.IDQuyenHT,
                           TenQuyenHT = a.TenQuyenHT,
                       }).ToList();
            A_GroupQuyenDetailsValidation DO = new A_GroupQuyenDetailsValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDQuyenHT = a.IDQuyenHT;
                    DO.TenQuyenHT = a.TenQuyenHT;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(A_GroupQuyenDetailsValidation _DO)
        {

            try
            {
                var a = db.A_QuyenHT_update(_DO.IDQuyenHT, _DO.TenQuyenHT);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "STpermissions");
        }
        public ActionResult DetailedAuthorization(int id)
        {
            if (IDQuyenHT != 1)
            {
                return RedirectToAction("Index", "ListProject", new { area = "View360" });
            }
            var res = (from a in db.A_QuyenHT.Where(x => x.IDQuyenHT == id)
                       select new A_GroupQuyenDetailsValidation
                       {
                           IDQuyenHT = a.IDQuyenHT,
                           TenQuyenHT = a.TenQuyenHT,
                       }).ToList();

            var resCon = (from a in db.A_ListController.Where(x => x.isActive == 1)
                          select new A_ListControllerValidation
                          {
                              IDController = a.IDController,
                              Controller = a.Controller,
                              Mota = a.Mota,
                              isActive = a.isActive,
                              DSQuyenCN = a.DSQuyenCN,
                          }).OrderBy(x => x.Mota).ToList();
            if (resCon.Count > 0)
            {
                foreach (var con in resCon)
                {
                    var lsCheck = new List<ItemCheck>();
                    var dsquyen = con.DSQuyenCN?.Split(',');

                    db.Configuration.ProxyCreationEnabled = false;
                    List<A_QuyenCN> dt = db.A_QuyenCN.ToList();
                    foreach (var item in dt)
                    {
                        if (dsquyen == null)
                        {
                            //var a = new ItemCheck { Name = item.TenQuyenCN, IDCN = item.ID, isChecked = false };
                            //lsCheck.Add(a);
                        }
                        else
                        {
                            int pos = Array.IndexOf(dsquyen, item.IDQuyenCN.ToString());
                            if (pos > -1)
                            {
                                var IDQ = res[0].IDQuyenHT;
                                var check = db.A_QuyenCT.Where(x => x.IDQuyenHT == IDQ && x.IDQuyenCN == item.IDQuyenCN && x.IDController == con.IDController).FirstOrDefault();
                                if (check?.isActive == 1)
                                {
                                    var a = new ItemCheck { Name = item.TenQuyenCN, IDCN = item.IDQuyenCN, isChecked = true };
                                    lsCheck.Add(a);
                                }
                                else
                                {
                                    var a = new ItemCheck { Name = item.TenQuyenCN, IDCN = item.IDQuyenCN, isChecked = false };
                                    lsCheck.Add(a);
                                }

                            }
                            //else
                            //{
                            //    var a = new ItemCheck { Name = item.TenQuyenCN, IDCN = item.ID, isChecked = false };
                            //    lsCheck.Add(a);
                            //}
                        }
                    }
                    con.LSChecked = lsCheck.ToList();
                }
                res[0].ListController = resCon;
            }
            List<A_QuyenHT> dta = db.A_QuyenHT.ToList();
            ViewBag.SelectQuyen = new SelectList(dta, "IDQuyenHT", "TenQuyenHT", id);

            A_GroupQuyenDetailsValidation DO = new A_GroupQuyenDetailsValidation();
            if (res.Count > 0)
            {
                foreach (var c in res)
                {
                    DO.IDQuyenHT = c.IDQuyenHT;
                    DO.TenQuyenHT = c.TenQuyenHT;
                    DO.ListController = c.ListController;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return View(DO);
        }
        [HttpPost]
        public ActionResult DetailedAuthorization(A_GroupQuyenDetailsValidation _DO)
        {
            try
            {
                if (_DO.ListController.Count > 0)
                {
                    foreach (var item in _DO.ListController)
                    {
                        if (item.LSChecked != null)
                        {
                            foreach (var chec in item.LSChecked)
                            {
                                int? isCh = chec.isChecked ? 1 : 0;
                                var idQ = GetIDQuyenDetail(_DO.IDQuyenHT, item.IDController, chec.IDCN);
                                if (isCh == 1)
                                {
                                    if (idQ == 0)
                                    {
                                        db.A_QuyenCT_insert(_DO.IDQuyenHT, item.IDController, chec.IDCN, isCh);
                                    }
                                } else
                                {
                                    if (idQ != 0)
                                    {
                                        db.A_QuyenCT_deleteCN(_DO.IDQuyenHT, item.IDController, chec.IDCN);
                                    }
                                    
                                }
                            }
                        }
                    }
                }
                //db.PhongBan_update_KNL(_DO.IDPhongBan, _DO.TenPhongBan, _DO.MaPB);

                TempData["msgSuccess"] = "<script>alert('Cập nhập thành công');</script>";
            }
            catch (Exception e)
            {

                TempData["msgSuccess"] = "<script>alert('Cập nhập thất bại " + e.Message + " ');</script>";
            }

            return RedirectToAction("DetailedAuthorization", "STpermissions", new { id = _DO.IDQuyenHT });
        }


        public ActionResult ListUser(int id)
        {
            if (IDQuyenHT != 1)
            {
                return RedirectToAction("Index", "ListProject", new { area = "View360" });
            }
            var res = (from a in dbE.NhanViens.Where(x => x.IDQuyenHT == id)
                       select new EmployeeValidation
                       {
                           ID = a.ID,
                           HoTen = a.MaNV + "-" + a.HoTen,
                           PhongBan = a.PhongBan.TenPhongBan,
                           TenViTri = a.Vitri.TenViTri,
                           IDPhongBan = id
                       }).ToList();

            List<A_QuyenHT> dta = db.A_QuyenHT.ToList();
            ViewBag.SelectQuyen = new SelectList(dta, "IDQuyenHT", "TenQuyenHT", id);

            EmployeeValidation DO = new EmployeeValidation();
            if (res.Count > 0)
            {
                foreach (var c in res)
                {
                    DO.ID = c.ID;
                    DO.HoTen = c.HoTen;
                    DO.PhongBan = c.PhongBan;
                    DO.TenViTri = c.TenViTri;
                    DO.IDPhongBan = c.IDPhongBan;
                }
            }
            //else
            //{
            //    return;
            //}
            int pageSize = res.Count() != 0 ? res.Count() : 50;
            int pageNumber = 1;
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult DeleteQuyen(int id, int? IDQuyen)
        {
            try
            {
                var aa = dbE.NhanViens.Where(x => x.ID == id).FirstOrDefault();
                if (aa != null)
                {
                    db.A_Nhanvien_update_QuyenHT(aa.MaNV, 0);
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }
            return RedirectToAction("ListUser", "STpermissions", new { id = IDQuyen });
        }

        public ActionResult AdminPermision(int? page)
        {
            if (IDQuyenHT != 1)
            {
                return RedirectToAction("Index", "ListProject", new { area = "View360" });
            }
            var res = (from a in db.A_ListController
                       select new A_ListControllerValidation
                       {
                           IDController = a.IDController,
                           Controller = a.Controller,
                           Mota = a.Mota,
                           isActive = a.isActive,
                           DSQuyenCN = a.DSQuyenCN,
                       }).OrderBy(x => x.Mota).ToList();
            foreach (var item in res)
            {
                var words = new List<string>();
                if (!String.IsNullOrEmpty(item.DSQuyenCN))
                {
                    var dsquyen = item.DSQuyenCN?.Split(',');
                    List<A_QuyenCN> dt = db.A_QuyenCN.ToList();
                    foreach (var aa in dt)
                    {
                        int pos = Array.IndexOf(dsquyen, aa.IDQuyenCN.ToString());
                        if (pos > -1)
                        {
                            words.Add(aa.MaQuyenCN);
                        }
                    }
                }
                item.DSTenQuyen = string.Join(",", words);
            }

            if (page == null) page = 1;
            int pageSize = res.Count() != 0 ? res.Count() : 50;
            int pageNumber = 1;
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult AddUserQuyen(int? id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<A_QuyenHT> dta = db.A_QuyenHT.Where(x => x.IDQuyenHT == id).ToList();
            ViewBag.IDHT = new SelectList(dta, "IDQuyenHT", "TenQuyenHT", id);

            var aa = dbE.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();

            var nv3 = aa.Select(x => new EmployeeValidation { MaNV = x.MaNV, HoTen = x.MaNV + " - " + x.HoTen }).ToList();
            ViewBag.Selected = new SelectList(nv3, "MaNV", "HoTen");

            List<PhongBan> dt = dbE.PhongBans.ToList();
            ViewBag.IDPB = new SelectList(dt, "IDPhongBan", "TenPhongBan");

            return PartialView();
        }
        //public ActionResult ListCN()
        //{
        //    var model = db.A_QuyenCN.ToList();
        //    return PartialView(model);
        //}
        public ActionResult CreateCN()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult CreateCN(A_QuyenCN _DO)
        {
            try
            {
                if(_DO.MaQuyenCN != null)
                {
                    var check = db.A_QuyenCN.Where(x=>x.MaQuyenCN == _DO.MaQuyenCN).FirstOrDefault();
                    if(check == null)
                    {
                        var aa = db.A_QuyenCN_insert(_DO.MaQuyenCN, _DO.TenQuyenCN);
                    }
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm chức năng thất bại');</script>";
            }
            return RedirectToAction("AdminPermision", "STpermissions");
        }

        [HttpPost]
        public ActionResult AddUserQuyen(A_AddUserQuyen _DO)
        {
            try
            {
                if (_DO.Selected != null)
                {
                    for (int i = 0; i < _DO.Selected.Length; i++)
                    {
                        //db.DSachDG_insert(_DO.Selected[i], manv, _DO.IDKNL);
                        if (_DO.Selected[i] != null && _DO.IDHT != null)
                        {
                            //var mavt = db.ViTriKNLs.Where(x => x.IDVT == _DO.IDKNL).FirstOrDefault()?.MaViTri;
                            //db.Nhanvien_update_ViTri(_DO.Selected[i], mavt);
                            db.A_Nhanvien_update_QuyenHT(_DO.Selected[i], _DO.IDHT);
                        }
                    }
                }

                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi cập nhật: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("ListUser", "STpermissions", new { id = _DO.IDHT });
        }

        public JsonResult CheckNV(int? IDPB)
        {
            var ListNV = new List<EmployeeValidation>();
            var aa = dbE.NhanViens.Where(x => x.IDPhongBan == IDPB && x.IDTinhTrangLV == 1).ToList();
            if (aa.Count > 0)
            {
                foreach (var item in aa)
                {
                    ListNV.Add(new EmployeeValidation { MaNV = item.MaNV, HoTen = item.MaNV + " - " + item.HoTen });
                }

            }
            return Json(ListNV, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CheckNVID(int? IDPB)
        {
            var ListNV = new List<EmployeeValidation>();
            var aa = dbE.NhanViens.Where(x => x.IDPhongBan == IDPB && x.IDTinhTrangLV == 1).ToList();
            if (aa.Count > 0)
            {
                foreach (var item in aa)
                {
                    ListNV.Add(new EmployeeValidation { ID = item.ID, HoTen = item.MaNV + " - " + item.HoTen });
                }

            }
            return Json(ListNV, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditController(int id)
        {
            if (IDQuyenHT != 1)
            {
                return RedirectToAction("Index", "ListProject", new { area = "View360" });
            }
            var res = (from a in db.A_ListController.Where(x => x.IDController == id)
                       select new A_ListControllerValidation
                       {
                           IDController = a.IDController,
                           Controller = a.Controller,
                           Mota = a.Mota,
                           isActive = a.isActive,
                           isCheck = a.isActive == 1 ? true : false,
                           DSQuyenCN = a.DSQuyenCN,
                       }).OrderBy(x=>x.Mota).ToList();
            A_ListControllerValidation DO = new A_ListControllerValidation();

            if (res.Count > 0)
            {
                var lsCheck = new List<ItemCheck>();
                var dsquyen = res[0].DSQuyenCN?.Split(',');

                db.Configuration.ProxyCreationEnabled = false;
                List<A_QuyenCN> dt = db.A_QuyenCN.ToList();
                foreach (var item in dt)
                {
                    if (dsquyen == null)
                    {
                        var a = new ItemCheck { Name = item.TenQuyenCN, IDCN = item.IDQuyenCN, isChecked = false };
                        lsCheck.Add(a);
                    }
                    else
                    {
                        int pos = Array.IndexOf(dsquyen, item.IDQuyenCN.ToString());
                        if (pos > -1)
                        {
                            var a = new ItemCheck { Name = item.TenQuyenCN, IDCN = item.IDQuyenCN, isChecked = true };
                            lsCheck.Add(a);
                        }
                        else
                        {
                            var a = new ItemCheck { Name = item.TenQuyenCN, IDCN = item.IDQuyenCN, isChecked = false };
                            lsCheck.Add(a);
                        }
                    }

                }
                res[0].LSChecked = lsCheck.ToList();
                //ViewBag.Selected = new SelectList(dt, "ID", "TenQuyenCN");
                //ViewBag.Selec = new SelectList(dt, "ID", "TenQuyenCN");

                foreach (var c in res)
                {
                    DO.IDController = c.IDController;
                    DO.Mota = c.Mota;
                    DO.Controller = c.Controller;
                    DO.isActive = c.isActive;
                    DO.DSQuyenCN = c.DSQuyenCN;
                    DO.LSChecked = c.LSChecked;
                    DO.isCheck = c.isCheck;
                }
            }
            else
            {
                return HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult EditController(A_ListControllerValidation _DO)
        {
            try
            {
                var words = new List<string>();
                var lsCN = db.A_QuyenCT.Where(x => x.IDController == _DO.IDController).ToList();
                foreach (var item in _DO.LSChecked)
                {
                    if (item.isChecked) { words.Add(item.IDCN.ToString()); }
                    else
                    {
                        var a = lsCN.Where(x => x.IDQuyenCN == item.IDCN).Count();
                        if (a > 0) db.A_QuyenCT_isActive(_DO.IDController, item.IDCN, 0);
                    }
                }
                _DO.DSQuyenCN = string.Join(",", words);
                _DO.isActive = _DO.isCheck ? 1 : 0;
                var update = db.A_ListController_update(_DO.IDController, _DO.Controller, _DO.Mota, _DO.isActive, _DO.DSQuyenCN);
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }
            return RedirectToAction("AdminPermision", "STpermissions");
        }
        public int GetIDController(string TenController)
        {
            var model = db.A_ListController.Where(x => x.Controller == TenController).SingleOrDefault();
            if (model == null)
                return 0;
            return model.IDController;
        }

        public int GetIDQuyenDetail(int? IDQuyen, int? IDController, int? IDQuyenCN)
        {
            var model = db.A_QuyenCT.Where(x => x.IDQuyenHT == IDQuyen && x.IDController == IDController && x.IDQuyenCN == IDQuyenCN).SingleOrDefault();
            if (model == null)
                return 0;
            return model.IDQuyenCT;
        }
        public ActionResult Sync()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();

                var controllerTypes = from t in asm.GetExportedTypes()
                                      where typeof(IController).IsAssignableFrom(t)
                                      select t;
                List<String> listName = new List<string>();

                foreach (var item in controllerTypes)
                {
                    int index = item.Name.IndexOf("Controller");
                    if (index != -1)
                    {
                        var name = item.Name.Remove(index);
                        listName.Add(name);
                        int IDC = GetIDController(name);
                        if (IDC == 0)
                        {
                            db.A_ListController_insert(name, null, 1,null);
                        }
                    }
                }
                var listC = db.A_ListController.ToList();
                foreach (var a in listC)
                {
                    bool alreadyExist = listName.Contains(a.Controller);
                    if (alreadyExist == false)
                    {
                        //db.A_QuyenCT_deleteController(a.IDController);
                        //db.A_ListController_delete(a.IDController);
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Đồng bộ dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi đồng bộ dữ liệu: " + e.Message + "');</script>";
            }
            return RedirectToAction("AdminPermision", "STpermissions");
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_AddQuyenHT.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_AddQuyenHT.xlsx");
        }
        public ActionResult ImportExcel(int id)
        {
            A_AddUserQuyen quyenHT = new A_AddUserQuyen();
            quyenHT.IDHT = id;
            return PartialView(quyenHT);
        }
        [HttpPost]
        public ActionResult ImportExcel(A_AddUserQuyen _DO)
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
                    filePath = path + Path.GetFileName(file.FileName);

                    file.SaveAs(filePath);
                    Stream stream = file.InputStream;

                    IExcelDataReader reader = null;
                    if (file.FileName.ToLower().EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (file.FileName.ToLower().EndsWith(".xlsx"))
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
                    int dtc = 0;

                    if (dt.Rows.Count > 0)
                    {
                        try
                        {

                            for (int i = 1; i < dt.Rows.Count; i++)
                            {
                                var MaNV = dt.Rows[i][0].ToString();
                                var HoTen = dt.Rows[i][1].ToString();
                                var check = dbE.NhanViens.Where(x=>x.MaNV.Equals(MaNV) && x.HoTen.Equals(HoTen)).FirstOrDefault();
                                if (check != null)
                                {
                                    db.A_Nhanvien_update_QuyenHT(dt.Rows[i][0].ToString(), _DO.IDHT);
                                    dtc++;
                                } else
                                {
                                    TempData["msgSuccess"] = "<script>alert('Lỗi Mã NV và Tên NV không tồn tại. Dòng  " + (i + 1) + "');</script>";
                                    return RedirectToAction("ListUser", "STpermissions", new { id = _DO.IDHT });
                                }
                                

                            }

                            string msg = "";
                            if (dtc != 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu";
                            }
                            else { msg = "File import không có dữ liệu"; }

                            TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";

                        }
                        catch (Exception ex)
                        {
                            TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import: " + ex.Message + "');</script>";
                        }

                    }
                    else
                    {
                        TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                    }

                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
                }
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Vui lòng nhập file Import');</script>";
            }

            return RedirectToAction("ListUser", "STpermissions", new { id = _DO.IDHT });
        }
        public JsonResult CheckLSNV(string lsnv)
        {
            var ListNV = new List<EmployeeServeyValidation>();
            if (!String.IsNullOrEmpty(lsnv))
            {
                //Regex.Replace(_DO.NVDG, @"[^0-9a-zA-Z]+", " ");
                string tx = Regex.Replace(lsnv, @"[^0-9a-zA-Z]+", " ");
                string[] NVS = tx.Split(new char[] { ' ' });

                foreach (var item in NVS)
                {
                    var aa = dbE.NhanViens.Where(x => x.MaNV == item).ToList();
                    if (aa.Count > 0)
                    {
                        ListNV.Add(new EmployeeServeyValidation { IDNV = aa.FirstOrDefault().ID, HoTen = aa.FirstOrDefault().MaNV + " - " + aa.FirstOrDefault().HoTen });
                    }
                }
            }
            return Json(ListNV, JsonRequestBehavior.AllowGet);
        }
    }
}