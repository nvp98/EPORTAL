using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Partner.Controllers
{
    public class CardTempController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        EPORTAL_NTEntities dbNT = new EPORTAL_NTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "CardTemp";
        // GET: Partner/CardTemp
        public ActionResult Index(int? page, string search)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;
            var lsGate = db.NT_Gate.ToList();
            var lsCard = dbNT.NT_CardTemp.ToList();
            var res = from a in dbNT.NT_CardTemp
                      select new CardTempValidation
                      {
                          IDThe =a.IDThe,
                          MaThe =a.MaThe,
                          TenThe =a.TenThe,
                          EmployeeID = a.EmployeeID,
                          UserID =a.UserID
                      };
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NT_Gate> dt = db.NT_Gate.ToList();
            ViewBag.IDCong = new SelectList(dt, "IDGate", "Gate");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(CardTempValidation _DO, FormCollection collection)
        {

            try
            {

                if (Request != null)
                {
                    if (!String.IsNullOrEmpty(_DO.MaThe))
                    {
                        var aa = dbNT.NT_CardTemp.Where(x => x.MaThe == _DO.MaThe).ToList();
                        if (aa.Count == 0) dbNT.NT_CardTemp_insert(_DO.MaThe, _DO.TenThe,_DO.UserID,_DO.EmployeeID, null);
                        else if(aa.Count >0 ) dbNT.NT_CardTemp_update(aa.First().IDThe,_DO.MaThe, _DO.TenThe, _DO.UserID, _DO.EmployeeID, null);
                    }
                    var ListVT = new List<CardTempValidation>();
                    foreach (var key in collection.AllKeys)
                    {
                        if (key.Split('_')[0] == "MaThe" && key.Split('_').Count()>1)
                        {
                            ListVT.Add(new CardTempValidation() { MaThe = collection[key], TenThe = collection["TenThe_" + key.Split('_')[1]],EmployeeID = collection["EmployeeID_" + key.Split('_')[1]],UserID = collection["UserID_" + key.Split('_')[1]] });
                        }
                    }
                    foreach (var item in ListVT)
                    {
                        var card = dbNT.NT_CardTemp.Where(x => x.MaThe == item.MaThe).ToList();
                        //int idvt = GetIDNhom(item.TenViTri.Trim());
                        if (!String.IsNullOrEmpty(item.MaThe) && card.Count ==0)
                        {
                            var aa = dbNT.NT_CardTemp_insert(item.MaThe, item.TenThe,item.UserID,item.EmployeeID, null);
                        }
                        else if(!String.IsNullOrEmpty(item.MaThe) && card.Count >0)
                        {
                            var aa = dbNT.NT_CardTemp_update(card.First().IDThe,item.MaThe, item.TenThe, item.UserID, item.EmployeeID, null);
                        }
                    }
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";

                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "CardTemp");
        }

        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in dbNT.NT_CardTemp.Where(x => x.IDThe == id)
                       select new CardTempValidation
                       {
                           IDThe = a.IDThe,
                           MaThe = a.MaThe,
                           TenThe = a.TenThe,
                           //IDCong =a.IDCong,
                           //TenCong =b.Gate,
                           EmployeeID = a.EmployeeID,
                           UserID = a.UserID
                       }).ToList();
            CardTempValidation DO = new CardTempValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDThe = a.IDThe;
                    DO.MaThe = a.MaThe;
                    DO.TenThe = a.TenThe;
                    DO.EmployeeID = a.EmployeeID;
                    DO.UserID = a.UserID;
                }
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(CardTempValidation _DO)
        {

            try
            {
                var a = dbNT.NT_CardTemp_update(_DO.IDThe,_DO.MaThe,_DO.TenThe,_DO.UserID,_DO.EmployeeID,null);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "CardTemp");
        }
        public ActionResult Delete(int? id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.DELETE).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                dbNT.NT_CardTemp_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Violate");
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM.DanhSachTheTamNT.xlsx";
            return File(path, "application/vnd.ms-excel", "BM.DanhSachTheTamNT.xlsx");
        }
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
        public ActionResult ImportExcels()
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
                    int dtcu = 0;
                    //int IDGate = 0;

                    if (dt.Rows.Count > 0)
                    {
                        try
                        {

                            NT_Partner ct = new NT_Partner();
                            for (int i = 1; i < dt.Rows.Count; i++)
                            {
                                DataRow dr = dt.Rows[i];
                                //IDGate = IDGateCheck(dr[5].ToString());
                                ////int IDProject = Convert.ToInt32(_DO.Selected[a]);
                                var car = dr[2].ToString();
                                var card = dbNT.NT_CardTemp.Where(x=>x.TenThe == car).ToList();
                                if ( card.Count == 0)
                                {
                                    dbNT.NT_CardTemp_insert(dr[4].ToString(), dr[2].ToString(), dr[1].ToString(), dr[3].ToString(), null);
                                    dtc++;
                                }
                                else if( card.Count != 0)
                                {
                                    dbNT.NT_CardTemp_update(card.First().IDThe,dr[4].ToString(), dr[2].ToString(), dr[1].ToString(), dr[3].ToString(), null);
                                    dtcu++;
                                }
                            }

                            string msg = "";
                            if (dtc != 0)
                            {
                                msg = "Import được " + dtc + " dòng dữ liệu";
                            }
                            else if(dtcu != 0)
                            {
                                msg = "Cập nhật được được " + dtcu + " dòng dữ liệu";
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

            return RedirectToAction("Index", "CardTemp");
        }
        public int IDGateCheck(string Gate)
        {
            var Regsbb = (from u in db.NT_Gate
                          where u.Gate.ToLower() == Gate.ToLower()
                          select new { u.IDGate }).FirstOrDefault();
            if (Regsbb != null)
                return Regsbb.IDGate;
            return 0;
        }
        public int? ToNullableInt(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }

    }
}