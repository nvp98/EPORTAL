using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsServey;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using Org.BouncyCastle.Crypto;
using PagedList;

namespace EPORTAL.Areas.Servey.Controllers
{
    public class ListSeveyController : Controller
    {
        // GET: Servey/ListSevey
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_SERVEYEntities dbSV = new EPORTAL_SERVEYEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListSevey";
        public ActionResult Index(int? page, string search)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = from a in dbSV.ListServeys
                      select new ListServeyValidation
                      {
                         IDSV =a.IDSV,
                         ContentSV =a.ContentSV,
                         StartTime =(DateTime)a.StartTime,
                         EndTime =(DateTime)a.EndTime,
                         StatusSV =(Boolean)a.StatusSV
                      };
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            var checkEdit = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            var checkDel = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (checkADD == 0) { ViewBag.QuyenAdd = 0; }
            if (checkEdit == 0) { ViewBag.QuyenEdit = 0; }

            int pageSize = res.Count() != 0 ? res.Count() : 50;
            int pageNumber = 1;
            return View(res.OrderByDescending(x => x.StartTime).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
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
        public ActionResult Create(ListServeyValidation _DO)
        {

            try
            {
                if(_DO.ContentSV != "")
                {
                    var a = dbSV.ListServey_insert(_DO.ContentSV,_DO.StartTime,_DO.EndTime,true);
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Vui lòng nhập nội dung khảo sát ');</script>";
                }
                
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ListSevey");
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in dbSV.ListServeys.Where(x => x.IDSV == id)
                       select new ListServeyValidation
                       {
                           IDSV =a.IDSV,
                           ContentSV =a.ContentSV,
                           StartTime =(DateTime)a.StartTime,
                           EndTime =(DateTime)a.EndTime,
                           StatusSV =(Boolean)a.StatusSV
                       }).ToList();
            ListServeyValidation DO = new ListServeyValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDSV = a.IDSV;
                    DO.ContentSV = a.ContentSV;
                    DO.StartTime = a.StartTime;
                    DO.EndTime = a.EndTime;
                    DO.StatusSV = a.StatusSV;
                }


                //List<PhongBan> pb = db.PhongBans.ToList();
                //ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDPhongBan);
                ViewBag.StartTime = DO.StartTime.ToString("yyyy-MM-dd");
                ViewBag.EndTime = DO.EndTime.ToString("yyyy-MM-dd");
                //List<ProjectsGroup> pg = db.ProjectsGroups.ToList();
                //ViewBag.PGList = new SelectList(pg, "IDGroup", "GroupName", DO.IDGroup);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(ListServeyValidation _DO)
        {

            try
            {

                var a = dbSV.ListServey_update(_DO.IDSV,_DO.ContentSV,_DO.StartTime,_DO.EndTime,_DO.StatusSV);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "ListSevey");
        }
        public ActionResult Delete(int? id)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                dbSV.ListServey_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListSevey");
        }

        public ActionResult DeleteUser(int? id,int? IDSV)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                dbSV.EmployeeServey_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("UserServey", "ListSevey",new { id =IDSV});
        }

        public ActionResult ResetServey(int? id, int? IDSV)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {

                var a = dbSV.EmployeeServey_updateXN(id, false);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("UserServey", "ListSevey", new { id = IDSV });
        }

        public ActionResult OptionServey(int? id)
        {
            var res = (from a in dbSV.OptionServeys.Where(x => x.IDSV == id)
                       join b in dbSV.ListServeys on a.IDSV equals b.IDSV
                       let nhom = dbSV.GroupKhaoSats.FirstOrDefault(x => x.ID == a.MaOT)
                       select new OptionServeyValidation
                       {
                           IDOT =a.IDOT,
                           IDSV = a.IDSV,
                           ContentSV = b.ContentSV,
                           ContentOT =a.ContentOT,
                           FilePath =a.FilePath,
                           OrderBy = a.OrderBy,
                           MaOT =a.MaOT,
                           TenNhom = nhom != null? nhom.TenNhom : "",
                       }).ToList();
            ViewBag.IDSV = id;
            ViewBag.ContentSV = dbSV.ListServeys.Where(x=>x.IDSV ==id).FirstOrDefault().ContentSV;
            return View(res.OrderBy(x => x.OrderBy).ToList());
        }
        public ActionResult CreateOption(int? id)
        {
            ViewBag.IDSV = id;
            var res = new OptionServeyValidation();
            res.IDSV = id;
            return PartialView(res);
        }
        [HttpPost]
        public ActionResult CreateOption(OptionServeyValidation _DO, FormCollection collection)
        {

            try
            {
                //var IDSV = collection["IDSV"];
                if (_DO.ContentSV != "") { var aa = dbSV.OptionServey_create(_DO.ContentOT, _DO.IDSV, null, _DO.OrderBy); }
                var ListVT = new List<OptionServeyValidation>();
                foreach (var key in collection.AllKeys)
                {
                    if (key.Split('_')[0] == "ContentOTT")
                    {
                        ListVT.Add(new OptionServeyValidation() { ContentOT = collection[key], OrderBy = int.Parse(collection["OrderByT_" + key.Split('_')[1]]) });
                    }
                }
                foreach (var item in ListVT)
                {
                        var k = dbSV.OptionServey_create(item.ContentOT, _DO.IDSV,null,item.OrderBy);
                }


                //var a = dbSV.ListServey_update(_DO.IDSV, _DO.ContentSV, _DO.StartTime, _DO.EndTime, _DO.StatusSV);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("OptionServey", "ListSevey",new { id =_DO.IDSV});
        }

        public ActionResult CreateGroup(int? id)
        {
            ViewBag.IDSV = id;
            var res = new GroupKhaoSatView();
            res.IDSV = id;
            var listGroup = dbSV.GroupKhaoSats.Where(x => x.IDSV == id).ToList();
            ViewBag.ListGroup = listGroup;
            return PartialView(res);
        }
        [HttpPost]
        public ActionResult CreateGroup(GroupKhaoSatView _DO, FormCollection collection)
        {

            try
            {
                if (_DO.TenNhom != "") {
                    var newGroup = new GroupKhaoSat()
                    {
                        IDSV = _DO.IDSV,
                        TenNhom = _DO.TenNhom,
                        isChon = _DO.isChon,
                        isShowRe = _DO.isShowRe,
                    };
                    dbSV.GroupKhaoSats.Add(newGroup);
                    dbSV.SaveChanges();
                    newGroup.MaNhom = newGroup.ID;
                    dbSV.SaveChanges();
                }
                //var ListVT = new List<OptionServeyValidation>();
                //foreach (var key in collection.AllKeys)
                //{
                //    if (key.Split('_')[0] == "ContentOTT")
                //    {
                //        ListVT.Add(new OptionServeyValidation() { ContentOT = collection[key], OrderBy = int.Parse(collection["OrderByT_" + key.Split('_')[1]]) });
                //    }
                //}
                //foreach (var item in ListVT)
                //{
                //    var k = dbSV.OptionServey_create(item.ContentOT, _DO.IDSV, null, item.OrderBy);
                //}


                //var a = dbSV.ListServey_update(_DO.IDSV, _DO.ContentSV, _DO.StartTime, _DO.EndTime, _DO.StatusSV);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("OptionServey", "ListSevey", new { id = _DO.IDSV });
        }


        public ActionResult EditOption(int id)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            var res = (from a in dbSV.OptionServeys.Where(x => x.IDOT == id)
                       select new OptionServeyValidation
                       {
                           IDOT =a.IDOT,
                           IDSV = a.IDSV,
                           ContentOT = a.ContentOT,
                           FilePath =a.FilePath,
                           OrderBy = a.OrderBy,
                           MaOT = a.MaOT,
                       }).ToList();
            OptionServeyValidation DO = new OptionServeyValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDOT = a.IDOT;
                    DO.IDSV = a.IDSV;
                    DO.ContentSV = a.ContentSV;
                    DO.ContentOT = a.ContentOT;
                    DO.FilePath = a.FilePath;
                    DO.OrderBy = a.OrderBy;
                    DO.MaOT = a.MaOT;
                }
                //List<PhongBan> pb = db.PhongBans.ToList();
                //ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDPhongBan);
                //List<ProjectsGroup> pg = db.ProjectsGroups.ToList();
                //ViewBag.PGList = new SelectList(pg, "IDGroup", "GroupName", DO.IDGroup);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult EditOption(OptionServeyValidation _DO)
        {

            try
            {
                if (_DO.OrderBy == null) _DO.OrderBy = 0;
                if (_DO.FileUpload != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/FileServey/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Use Namespace called :  System.IO  
                    string FileName = _DO.FileUpload != null ? "File_" + _DO.IDOT : "";

                    //To Get File Extension  
                    string FileExtension = _DO.FileUpload != null ? Path.GetExtension(_DO.FileUpload.FileName) : "";
                    ////Add Current Date To Attached File Name  
                    if (FileExtension != ".pdf")
                    {
                        TempData["msgError"] = "<script>alert('Vui lòng chọn đúng định dạng file PDF');</script>";
                        //return View();
                    }
                    else
                    {
                        if (_DO.FileUpload != null)
                        {
                            FileName = FileName.Trim() + FileExtension;
                            _DO.FileUpload.SaveAs(path + FileName);
                            _DO.FilePath = "~/UploadedFiles/FileServey/" + FileName;
                        }
                        int a = dbSV.OptionServey_update(_DO.IDOT,_DO.ContentOT,_DO.IDSV,_DO.FilePath,_DO.OrderBy);
                        var option = dbSV.OptionServeys.FirstOrDefault(x=>x.IDOT == _DO.IDOT);
                        option.MaOT = _DO.MaOT;
                        dbSV.SaveChanges();
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                }
                else
                {
                    var a = dbSV.OptionServey_update(_DO.IDOT, _DO.ContentOT, _DO.IDSV, _DO.FilePath, _DO.OrderBy);
                    var option = dbSV.OptionServeys.FirstOrDefault(x => x.IDOT == _DO.IDOT);
                    option.MaOT = _DO.MaOT;
                    dbSV.SaveChanges();
                }
                //var a = dbSV.ListServey_update(_DO.IDSV, _DO.ContentSV, _DO.StartTime, _DO.EndTime, _DO.StatusSV);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("OptionServey", "ListSevey", new { id = _DO.IDSV });
        }
        public ActionResult DeleteOption(int? id,int? IDSV)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                dbSV.OptionServey_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("OptionServey", "ListSevey",new { id = IDSV});
        }
        public ActionResult UserServey(int? id,int? page)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var IDs = db.NhanViens.ToList();
            var dsSV = dbSV.EmployeeServeys.ToList();
            var selectSV = dbSV.OptionServey_selectOT(id).ToList();

            var res = (from a in dsSV.Where(x=>x.IDSV ==id)
                       join b in IDs on a.IDNV equals b.ID
                       join c in selectSV on a.OTID equals c.IDOT into ulk
                       from c in ulk.DefaultIfEmpty()
                       select new EmployeeServeyValidation
                       {
                           ID =a.ID,
                           IDSV = a.IDSV,
                           HoTen =b.HoTen,
                           MaNV =b.MaNV,
                           OTID =a.OTID,
                           ContentOT = c?.ContentOT??"",
                           XNSV = a.XNSV,
                           TenPhongBan = b.PhongBan.TenPhongBan
                       }).ToList();
            ViewBag.IDSV = id;
            ViewBag.ContentSV = dbSV.ListServeys.Where(x => x.IDSV == id).FirstOrDefault().ContentSV;
            if (page == null) page = 1;
            int pageSize = res.Count() != 0 ? res.Count() : 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderBy(x => x.TenPhongBan).ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult CreateUser(int? id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var aa = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();

            var nv3 = aa.Select(x => new EmployeeServeyValidation { IDNV = x.ID, HoTen = x.MaNV + " - " + x.HoTen }).ToList();
            ViewBag.Selected = new SelectList(nv3, "IDNV", "HoTen");

            List<PhongBan> dt = db.PhongBans.ToList();
            ViewBag.IDPB = new SelectList(dt, "IDPhongBan", "TenPhongBan");

            ViewBag.IDSV = id;
            var res = new EmployeeServeyValidation();
            res.IDSV = id;
            return PartialView(res);
        }
        [HttpPost]
        public ActionResult CreateUser(EmployeeServeyValidation _DO)
        {

            try
            {
                if (!String.IsNullOrEmpty(_DO.LIDSNV))
                {
                    //Regex.Replace(_DO.NVDG, @"[^0-9a-zA-Z]+", " ");
                    string tx = Regex.Replace(_DO.LIDSNV, @"[^0-9a-zA-Z]+", " ");
                    string[] NVS = tx.Split(new char[] { ' ' });
                    foreach (var item in NVS)
                    {
                        var aa = db.NhanViens.Where(x => x.MaNV == item).ToList();
                        if (aa.Count() > 0)
                        {
                            //var lsSV = dbSV.EmployeeServeys.ToList();
                            //var a = lsSV.Where(x => x.IDNV == aa.FirstOrDefault().ID && x.IDSV == _DO.IDSV).ToList();
                            var a = dbSV.EmployeeServey_selectNV(aa.FirstOrDefault().ID, _DO.IDSV).ToList();
                            if(a.Count ==0) dbSV.EmployeeServey_insert(aa.FirstOrDefault().ID, _DO.IDSV,0);
                        }
                    }
                }
                if (_DO.Selected != null)
                {
                    for (int i = 0; i < _DO.Selected.Length; i++)
                    {
                        if (_DO.Selected[i] != null && _DO.IDSV != null)
                        {
                            //var mavt = db.ViTriKNLs.Where(x => x.IDVT == _DO.IDKNL).FirstOrDefault()?.MaViTri;
                            var a = dbSV.EmployeeServey_selectNV(int.Parse(_DO.Selected[i]), _DO.IDSV).ToList();
                            if(a.Count ==0 ) dbSV.EmployeeServey_insert(int.Parse(_DO.Selected[i]), _DO.IDSV,0);
                        }
                    }
                }
                //var a = dbSV.ListServey_update(_DO.IDSV, _DO.ContentSV, _DO.StartTime, _DO.EndTime, _DO.StatusSV);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("UserServey", "ListSevey", new { id = _DO.IDSV });
        }


        public ActionResult EditUserSevey(int id)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            var IDs = db.NhanViens.ToList();
            var dsSV = dbSV.EmployeeServeys.Where(x => x.ID == id).ToList();
            var res = (from a in dsSV
                       join b in IDs on a.IDNV equals b.ID
                       select new EmployeeServeyValidation
                       {
                           ID =a.ID,
                           IDSV = a.IDSV,
                           StatusSV = (Boolean?)a?.XNSV??false,
                           HoTen = b.MaNV +"-" +b.HoTen,
                       }).ToList();
            EmployeeServeyValidation DO = new EmployeeServeyValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.IDSV = a.IDSV;
                    DO.HoTen = a.HoTen;
                    DO.StatusSV = a.StatusSV;
                }
                //List<PhongBan> pb = db.PhongBans.ToList();
                //ViewBag.IDPhongBan = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDPhongBan);
                //List<ProjectsGroup> pg = db.ProjectsGroups.ToList();
                //ViewBag.PGList = new SelectList(pg, "IDGroup", "GroupName", DO.IDGroup);

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult EditUserSevey(EmployeeServeyValidation _DO)
        {

            try
            {
                 var a = dbSV.EmployeeServey_updateXN(_DO.ID, _DO.StatusSV);
             
                 TempData["msgSuccess"] = "<script>alert('Mở khảo sát thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("UserServey", "ListSevey", new { id = _DO.IDSV });
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
                    var aa = db.NhanViens.Where(x => x.MaNV == item).ToList();
                    if (aa.Count > 0)
                    {
                        ListNV.Add(new EmployeeServeyValidation { IDNV = aa.FirstOrDefault().ID, HoTen = aa.FirstOrDefault().MaNV + " - " + aa.FirstOrDefault().HoTen });
                    }
                }
            }
            return Json(ListNV, JsonRequestBehavior.AllowGet);
        }

        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_DSNV.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_DSNV.xlsx");
        }
        public ActionResult ImportExcel(int? id)
        {
            //List<Project> nt = db.Projects.ToList();
            //ViewBag.ProjectID = new SelectList(nt, "ID", "Title");
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            var pro = dbSV.ListServeys.Where(x=>x.IDSV ==id).ToList();
            ViewBag.IDSV = new SelectList(pro, "IDSV", "ContentSV");

            return PartialView();
        }
        [HttpPost]
        public ActionResult ImportExcel(EmployeeServeyValidation _DO)
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
                    int IDNV = 0;

                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            if (_DO.IDSV != null)
                            {
                                EmployeeServeyValidation aus = new EmployeeServeyValidation();
                                for (int i = 3; i < dt.Rows.Count; i++)
                                {
                                    DataRow dr = dt.Rows[i];
                                    IDNV = IDNhanVien(dr[0].ToString());
                                    //int IDProject = Convert.ToInt32(_DO.Selected[a]);
                                    var nv = dbSV.EmployeeServey_selectNV(IDNV,_DO.IDSV).ToList();
                                    if (IDNV > 0 && nv.Count ==0)
                                    {
                                        dbSV.EmployeeServey_insert(IDNV, _DO.IDSV,0);
                                        dtc++;
                                    }
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

            return RedirectToAction("UserServey", "ListSevey", new { id = _DO.IDSV });
        }
        public int IDNhanVien(string MaNV)
        {
            var Regsbb = (from u in db.NhanViens
                          where u.MaNV.ToLower() == MaNV.ToLower()
                          select new { u.ID }).FirstOrDefault();
            if (Regsbb != null)
                return Regsbb.ID;
            return 0;
        }

        public ActionResult ExportToExcel(int? id)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EX).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSSERVEY.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_DSSERVEY_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DSNV");
                List<ProjectsGroup> listpg = db.ProjectsGroups.ToList();
                //IList<Project> list_Projects = db.Projects.ToList();
                var IDs = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
                var dsSV = dbSV.EmployeeServeys.ToList();
                var selectSV = dbSV.OptionServey_selectOT(id).ToList();

                var DsLyDo = dbSV.CTKhaoSats.Where(x=>x.GhiChu != null && x.IDSV ==id).ToList();


                var LSPart = dbSV.PartTogethers.ToList();
                List<PartTogetherValidation> Ls = (from a in LSPart
                                                   join b in IDs on a.IDNV equals b.ID into ulk
                                                   from b in ulk.DefaultIfEmpty()
                                                   select new PartTogetherValidation
                                                   {
                                                       ID = a.ID,
                                                       HoTen = a.HoTen,
                                                       IDESV = a.IDESV,
                                                       IDNV = a.IDNV,
                                                       MaNV = a.IDNV != null ? b.MaNV : "",
                                                       HoTenNV = a.IDNV != null ? b.HoTen : "",
                                                       //NamSinh = (DateTime?)a.NamSinh ?? default,
                                                       Re = a.Re,
                                                       isCom = a.isCom,
                                                       TenNV = a.IDNV != null ? b.MaNV + "-" + b.HoTen : "",
                                                       PhongBan = a.IDNV != null ? b.PhongBan.TenPhongBan : "",
                                                       Note = a.Note
                                                   }).ToList();

                var res = (from a in dsSV.Where(x => x.IDSV == id)
                           join b in IDs on a.IDNV equals b.ID
                           join c in selectSV on a.OTID equals c.IDOT into ulk
                           from c in ulk.DefaultIfEmpty()
                           select new ExportEmployeeServeyValidation
                           {
                               ID = a.ID,
                               IDSV = a.IDSV,
                               IDNV = (int)a.IDNV,
                               HoTen = b.HoTen,
                               MaNV = b.MaNV,
                               OTID = a.OTID,
                               ContentOT = c?.ContentOT ?? "",
                               XNSV = a.XNSV,
                               TenPhongBan = b.PhongBan.TenPhongBan,
                               LSPart = Ls.Where(x => x.IDESV == a.IDNV).ToList(),
                               MenuOT = a.MenuOT,
                               LyDo = DsLyDo.Where(x=>x.IDNV == a.IDNV).FirstOrDefault()?.GhiChu,
                           }).ToList();


                if (res.Count > 0)
                {
                    int row = 4, rowlast = 4, stt = 0;
                    foreach (var item in res)
                    {

                        row++; stt++;
                        rowlast = row + 1;
                        var xn = item.XNSV == true ? "Đã khảo sát" : "Chưa Xác nhận";
                        var menu = "";
                        if (item.MenuOT == 1) menu = "Ăn chay Rằm(Mùng 1)";
                        else if (item.MenuOT == 2) menu = "Ăn chay trường";

                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = item.MaNV;
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        //var pg = listpg.Where(x => x.IDGroup == item.IDGroup).FirstOrDefault();


                        Worksheet.Cell("C" + row).Value = item.HoTen;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.TenPhongBan;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("D" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("E" + row).Value = item.ContentOT;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("F" + row).Value = item.LyDo;
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        if (item.LSPart.Count > 0)
                        {
                           foreach(var pa in item.LSPart)
                            {
                                if (pa.isCom == true)
                                {

                                    Worksheet.Cell("G" + row).Value = "";
                                    Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("H" + row).Value = "";
                                    Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("I" + row).Value = "";
                                    Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("J" + row).Value = "";
                                    Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("K" + row).Value = pa.MaNV;
                                    Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;


                                    Worksheet.Cell("L" + row).Value = pa.HoTenNV;
                                    Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("M" + row).Value = pa.PhongBan;
                                    Worksheet.Cell("M" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("M" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("M" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("N" + row).Value = pa.Re;
                                    Worksheet.Cell("N" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("N" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("N" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("O" + row).Value = pa.Note;
                                    Worksheet.Cell("O" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("O" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("O" + row).Style.Alignment.WrapText = true;
                                }
                                else
                                {
                                    Worksheet.Cell("G" + row).Value = pa.HoTen;
                                    Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("H" + row).Value = pa.NamSinh;
                                    Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("I" + row).Value = pa.Re;
                                    Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("J" + row).Value = pa.Note;
                                    Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("K" + row).Value = "";
                                    Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;


                                    Worksheet.Cell("L" + row).Value = "";
                                    Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("M" + row).Value = "";
                                    Worksheet.Cell("M" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("M" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("M" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("N" + row).Value = "";
                                    Worksheet.Cell("N" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("N" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("N" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("O" + row).Value = "";
                                    Worksheet.Cell("O" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("O" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("O" + row).Style.Alignment.WrapText = true;
                                }
                                row++;
                            }
                            row = row - 1;
                        }


                        //row = rowlast - 1;
                    }
                    Worksheet.Range("A2:O" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:O" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A2:O" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A2:O" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "BM_DSSERVEY.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/Projects'</script>";
                    return RedirectToAction("UserServey", "ListSevey", new { id = id });
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Projects'</script>";
                return RedirectToAction("UserServey", "ListSevey", new { id = id });
            }

        }

        public ActionResult ExportToExcelDK(int? id)
        {
            //var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EX).First();
            //if (check == 0)
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            try
            {
                string fileNameMau = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_THONGKEDANGKY.xlsx";
                string fileNameMauTemp = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\BM_THONGKEDANGKY_temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNameMau);
                IXLWorksheet Worksheet = Workbook.Worksheet("DSDK");
                //List<ProjectsGroup> listpg = db.ProjectsGroups.ToList();
                //IList<Project> list_Projects = db.Projects.ToList();
                var IDs = db.NhanViens.Where(x => x.IDTinhTrangLV == 1).ToList();
                var dsSV = dbSV.EmployeeServeys.Where(x=>x.IDSV == id).ToList();
                var selectSV = dbSV.OptionServey_selectOT(id).ToList();
                var selectDC =dbSV.DiaDiems.ToList();
                var option = dbSV.OptionServeys.Where(x => x.IDSV == id).ToList();
                var ctKS = dbSV.CTKhaoSats.Where(x=>x.IDSV ==id).ToList();
                var ctNTDK = dbSV.ChiTietDKNTs.Where(x => x.IDSV == id).ToList();

                var LSPart = dbSV.CTDKNguoiThans.Where(x=>x.IDSV ==id).ToList();

                List<int> ListIDgroup = new List<int> { };
                List<string> columnHeaders = new List<string> { };
                List<string> columnOTHeaders = new List<string> { };
                var listND = dbSV.GroupKhaoSats.Where(x => x.IDSV == id).ToList();
                var listOT = dbSV.OptionServeys.Where(x => x.IDSV == id && x.isShow != 0).ToList();
                foreach (var item in listND)
                {
                    var listOTSL = listOT.Where(x => x.MaOT == item.MaNhom).ToList();
                    foreach (var item1 in listOTSL)
                    {
                        columnHeaders.Add(item.TenNhom);
                        columnOTHeaders.Add(item1.ContentOT);
                        ListIDgroup.Add(item1.IDOT); //optionID
                    }
                }

                List<PartTogetherValidation> Ls = (from a in LSPart
                                                   join b in IDs on a.IDNguoiThan equals b.ID into ulk
                                                   from b in ulk.DefaultIfEmpty()
                                                   select new PartTogetherValidation
                                                   {
                                                       ID = a.ID,
                                                       HoTen =  a.HoTen,
                                                       IDNguoiThan = a.IDNguoiThan,
                                                       GioiTinh = a.GioiTinh,
                                                       IDNV = a.IDNV,
                                                       MaNV = a.IDNguoiThan != null && b != null ? b.MaNV : "",
                                                       HoTenNV = a.IDNguoiThan != null && b != null ? b.HoTen : "",
                                                       CungCTy = a.isCom,
                                                       TenNV = a.IDNguoiThan != null && b != null ? b.MaNV + "-" + b.HoTen : "",
                                                       PhongBan = a.IDNguoiThan != null && b != null ? b.PhongBan.TenPhongBan : "",
                                                       DienThoai = a.DienThoai,
                                                       NamSinh = a.NamSinh,
                                                       QuanHe =a.QuanHe,
                                                       GioiTinhStr = a.GioiTinh ==1?"Nam":"Nữ",
                                                       Note =a.GhiChu
                                                   }).ToList();
                foreach (var item in Ls)
                {
                    List<string> columnSelectCom = new List<string> { };
                    foreach (var sl in ListIDgroup)
                    {
                        var ctksNV = ctNTDK.Where(x => x.IDNguoiThan == item.ID && x.IDSV == id && x.IDOT == sl).ToList();
                        if (ctksNV.Count() != 0)
                        {
                            //var ot = option.Where(x => x.IDOT == ctksNV.FirstOrDefault().IDOT).FirstOrDefault().ContentOT;
                            columnSelectCom.Add("1");
                        }
                        else
                        {
                            columnSelectCom.Add("");
                        }
                    }
                    item.ListSelect = columnSelectCom;
                }

                var DsLyDo = dbSV.CTKhaoSats.Where(x => x.IDSV == id).ToList();
                var res = (from a in dsSV.Where(x => x.IDSV == id)
                           join b in IDs on a.IDNV equals b.ID
                           join c in selectDC on a.IDDC equals c.ID into ulk
                           from c in ulk.DefaultIfEmpty()
                           select new ExportEmployeeServeyValidation
                           {
                               ID = a.ID,
                               IDSV = a.IDSV,
                               IDNV = (int)a.IDNV,
                               HoTen = b.HoTen,
                               MaNV = b.MaNV,
                               OTID = a.OTID,
                               ContentOT = c?.TenDC ?? "", //dịa chỉ
                               XNSV = a.XNSV,
                               TenPhongBan = b.PhongBan.TenPhongBan,
                               LSPart = Ls.Where(x => x.IDNV == a.IDNV).ToList(),
                               MenuOT = a.MenuOT,
                               LyDo = DsLyDo.Where(x => x.IDNV == a.IDNV).Count() != 0?DsLyDo.Where(x => x.IDNV == a.IDNV).FirstOrDefault()?.GhiChu:"",
                           }).ToList();
             
                // ketqua Dang ky
                foreach (var item in res)
                {
                    List<string> columnSelect = new List<string> { };
                    List<string> columnSelectKhac = new List<string> { };
                    foreach (var dk in ListIDgroup)
                    {
                        var ctksNV = ctKS.Where(x => x.IDNV == item.IDNV && x.IDSV == item.IDSV && x.IDOT == dk).ToList();
                        if (ctksNV.Count() != 0)
                        {
                            //var ot = option.Where(x => x.IDOT == ctksNV.FirstOrDefault().IDOT).FirstOrDefault().ContentOT;
                            columnSelect.Add("1");
                            if(dk != 48 && dk != 49) // điền điểm đón người thân trùng với người đăng ký
                            {
                                columnSelectKhac.Add("1");
                            }
                            else
                            {
                                columnSelectKhac.Add("");
                            }
                        }
                        else
                        {
                            columnSelect.Add("");
                            columnSelectKhac.Add("");
                        }
                    }
                    item.ListSelect = columnSelect;
                    item.ListSelectKhac = columnSelectKhac;
                }

                if (res.Count > 0)
                {
                    int row = 3, rowlast = 3, stt = 0;
                    int dem = 0;

                    var cell = Worksheet.Cell("P" + 2 ); // Lấy ô bắt đầu
                    var cell2 = Worksheet.Cell("P" + 3);
                    // Chèn dữ liệu từ danh sách vào các ô liên tiếp trong hàng
                    foreach (string data in columnHeaders)
                    {
                        cell.Value = data;
                        cell2.Value = columnOTHeaders[dem];
                        cell = cell.CellRight(); // Di chuyển sang ô bên phải
                        cell2 = cell2.CellRight();
                        dem++;
                    }
                   
                    foreach (var item in res)
                    {

                        row++; stt++;
                        rowlast = row + 1;
                        //var xn = item.XNSV == true ? "Đã khảo sát" : "Chưa Xác nhận";
                        //var menu = "";
                        //if (item.MenuOT == 1) menu = "Ăn chay Rằm(Mùng 1)";
                        //else if (item.MenuOT == 2) menu = "Ăn chay trường";
              
                        Worksheet.Cell("A" + row).Value = stt;
                        Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("B" + row).Value = "Công ty CP Thép Hòa Phát Dung Quất";
                        Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                        //var pg = listpg.Where(x => x.IDGroup == item.IDGroup).FirstOrDefault();


                        Worksheet.Cell("C" + row).Value = item.MaNV;
                        Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("D" + row).Value = item.HoTen;
                        Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Worksheet.Cell("D" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                        Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("E" + row).Value = item.TenPhongBan;
                        Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("F" + row).Value = "CBCNV";
                        Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                        Worksheet.Cell("G" + row).Value = item.OTID != null ?"Đã đăng ký":"Chưa đăng ký";
                        Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                        var selec = Worksheet.Cell("P" + row); // Lấy ô bắt đầu
                        // Chèn dữ liệu từ danh sách vào các ô liên tiếp trong hàng
                        foreach (string data in item.ListSelect)
                        {
                            selec.Value = data;
                            selec = selec.CellRight(); // Di chuyển sang ô bên phải
                        }
                        selec.Value = item.LyDo;

                        if (item.LSPart.Count > 0)
                        {
                            foreach (var pa in item.LSPart)
                            {
                                row++; stt++;
                                rowlast = row + 1;
                                DateTime dateTime;
                                if (DateTime.TryParse(pa.NamSinh, out dateTime))
                                {
                                    pa.NamSinh = dateTime.ToString("dd/MM/yyyy");
                                }

                                if (pa.CungCTy == 1)
                                {
                                   

                                    Worksheet.Cell("A" + row).Value = stt;
                                    Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("B" + row).Value = "Công ty CP Thép Hòa Phát Dung Quất";
                                    Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                                    //var pg = listpg.Where(x => x.IDGroup == item.IDGroup).FirstOrDefault();


                                    Worksheet.Cell("C" + row).Value = item.MaNV;
                                    Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("D" + row).Value = item.HoTen;
                                    Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    //Worksheet.Cell("D" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                                    Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("E" + row).Value = item.TenPhongBan;
                                    Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;


                                    Worksheet.Cell("F" + row).Value = "Người thân";
                                    Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("G" + row).Value = item.OTID != null ? "Đã đăng ký" : "Chưa đăng ký";
                                    Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("H" + row).Value = pa.HoTenNV;
                                    Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("I" + row).Value = pa.MaNV;
                                    Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("J" + row).Value = pa.PhongBan;
                                    Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("K" + row).Value = pa.QuanHe;
                                    Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("L" + row).Value = pa.NamSinh;
                                    Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;
                                    Worksheet.Cell("L" + row).Style.DateFormat.Format = "dd/MM/yyyy";

                                    Worksheet.Cell("M" + row).Value = "";
                                    Worksheet.Cell("M" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("M" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("M" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("N" + row).Value = "";
                                    Worksheet.Cell("N" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("N" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("N" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("O" + row).Value = pa.Note;
                                    Worksheet.Cell("O" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("O" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("O" + row).Style.Alignment.WrapText = true;

                                    var selecCom = Worksheet.Cell("P" + row); // Lấy ô bắt đầu
                                                                           // Chèn dữ liệu từ danh sách vào các ô liên tiếp trong hàng
                                    foreach (string data in item.ListSelectKhac)  // điền chọn điểm đăng ký theo người đăng ký
                                    {
                                        selecCom.Value = data;
                                        selecCom = selecCom.CellRight(); // Di chuyển sang ô bên phải
                                    }

                                }
                                else
                                {
                                    Worksheet.Cell("A" + row).Value = stt;
                                    Worksheet.Cell("A" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("A" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("A" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("B" + row).Value = "Công ty CP Thép Hòa Phát Dung Quất";
                                    Worksheet.Cell("B" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("B" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("B" + row).Style.Alignment.WrapText = true;

                                    //var pg = listpg.Where(x => x.IDGroup == item.IDGroup).FirstOrDefault();


                                    Worksheet.Cell("C" + row).Value = item.MaNV;
                                    Worksheet.Cell("C" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("C" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("C" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("D" + row).Value = item.HoTen;
                                    Worksheet.Cell("D" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    Worksheet.Cell("D" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    //Worksheet.Cell("D" + row).Style.DateFormat.Format = "dd/MM/yyyy";
                                    Worksheet.Cell("D" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("E" + row).Value = item.TenPhongBan;
                                    Worksheet.Cell("E" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("E" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("E" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("F" + row).Value = "Người thân";
                                    Worksheet.Cell("F" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("F" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("F" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("G" + row).Value = item.OTID != null ? "Đã đăng ký" : "Chưa đăng ký";
                                    Worksheet.Cell("G" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("G" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("G" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("H" + row).Value = pa.HoTen;
                                    Worksheet.Cell("H" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("H" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("H" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("I" + row).Value = "";
                                    Worksheet.Cell("I" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("I" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("I" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("J" + row).Value = "";
                                    Worksheet.Cell("J" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("J" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("J" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("K" + row).Value = pa.QuanHe;
                                    Worksheet.Cell("K" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("K" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("K" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("L" + row).Value = pa.NamSinh;
                                    Worksheet.Cell("L" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("L" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("L" + row).Style.Alignment.WrapText = true;
                                    Worksheet.Cell("L" + row).Style.DateFormat.Format = "dd/MM/yyyy";

                                    Worksheet.Cell("M" + row).Value = pa.GioiTinhStr;
                                    Worksheet.Cell("M" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("M" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("M" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("N" + row).Value = "'" + pa.DienThoai;
                                    Worksheet.Cell("N" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("N" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("N" + row).Style.Alignment.WrapText = true;

                                    Worksheet.Cell("O" + row).Value = pa.Note;
                                    Worksheet.Cell("O" + row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    Worksheet.Cell("O" + row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    Worksheet.Cell("O" + row).Style.Alignment.WrapText = true;

                                    var selecCom = Worksheet.Cell("P" + row); // Lấy ô bắt đầu
                                                                              // Chèn dữ liệu từ danh sách vào các ô liên tiếp trong hàng
                                    foreach (string data in item.ListSelectKhac) // chèn đúng select của người đăng ký
                                    {
                                        selecCom.Value = data;
                                        selecCom = selecCom.CellRight(); // Di chuyển sang ô bên phải
                                    }
                                }
                                //row++;
                            }
                            //row = row - 1;
                        }


                        //row = rowlast - 1;
                    }
                    Worksheet.Range("A2:O" + (row)).Style.Font.SetFontName("Arial");
                    Worksheet.Range("A2:O" + (row)).Style.Font.SetFontSize(10);
                    Worksheet.Range("A2:O" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A2:O" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Worksheet.Column("D").AdjustToContents();
                    Workbook.SaveAs(fileNameMauTemp);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameMauTemp);
                    string fileName = "BM_DSSERVEY.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    TempData["msg"] = "<script>alert('Không có dữ liệu');window.location.href = '/Projects'</script>";
                    return RedirectToAction("UserServey", "ListSevey", new { id = id });
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('" + ex + "');window.location.href = '/Projects'</script>";
                return RedirectToAction("UserServey", "ListSevey", new { id = id });
            }

        }


    }
}