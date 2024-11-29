using EPORTAL.Models;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyAuthentication = EPORTAL.Models.MyAuthentication;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ContractController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Contract";
        // GET: TagSign/Contract
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

            var res = from a in db.NT_Contract_select(search)
                      join nt in db.NT_Partner on a.IDNT equals nt.ID
                      join bp in db.PhongBans on a.IDBP equals bp.IDPhongBan into ulist1
                      from bp in ulist1.DefaultIfEmpty()
                      select new ContractValidation
                      {
                          IDContract = (int)a.IDContract,
                          Somecontracts = a.Somecontracts,
                          Contract = a.Contract,
                          BeginDate = (DateTime?)a.BeginDate??default,
                          EndDate = (DateTime?)a.EndDate??default,
                          FileScan=a.FileScan,
                          IDNT = (int)a.IDNT,
                          Fullname = nt.FullName,
                          IDBP =(int?)a.IDBP??default,
                          TPBP =(int?)a.TPBP??default,
                          TTXL = (int?)a.TTXL ?? default,
                      };
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderBy(x => x.IDContract).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Indes(int? page, string search)
        {
            //var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            //ViewBag.QUYENCN = ListQuyen;
            //if (!ListQuyen.Contains("VIEW_ALL"))
            //{
            //    TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
            //    return RedirectToAction("Logout", "Login", new { area = "" });
            //}
            if (search == null) search = "";
            ViewBag.search = search;

            var res = from a in db.NT_Contract_select(search).Where(x=>x.TTXL == 2)
                      join nt in db.NT_Partner on a.IDNT equals nt.ID
                      join bp in db.PhongBans on a.IDBP equals bp.IDPhongBan into ulist1
                      from bp in ulist1.DefaultIfEmpty()
                      select new ContractValidation
                      {
                          IDContract = (int)a.IDContract,
                          Somecontracts = a.Somecontracts,
                          Contract = a.Contract,
                          BeginDate = (DateTime?)a.BeginDate ?? default,
                          EndDate = (DateTime?)a.EndDate ?? default,
                          FileScan = a.FileScan,
                          IDNT = (int)a.IDNT,
                          Fullname = nt.FullName,
                          IDBP = (int?)a.IDBP ?? default,
                          TPBP = (int?)a.TPBP ?? default,
                          TTXL = (int?)a.TTXL ?? default,
                      };
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderBy(x => x.IDContract).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");

            List<PhongBan> bp = db.PhongBans.ToList();
            ViewBag.BPList = new SelectList(bp, "IDPhongBan", "TenPhongBan");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(ContractValidation _DO)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                var Check = db.NT_Contract.Where(x => x.Somecontracts == _DO.Somecontracts).FirstOrDefault();
                if(Check == null)
                {
                    //Upload file pdf
                    if (_DO.FileUpload != null)
                    {
                        string path = Server.MapPath("~/UploadedFiles/FileContract/");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        string FileName = _DO.FileUpload.FileName != null ? _DO.Somecontracts + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                        string FileExtension = _DO.FileUpload != null ? Path.GetExtension(_DO.FileUpload.FileName) : "";

                        if (_DO.FileUpload != null)
                        {
                            FileName = FileName.Trim() + FileExtension;
                            _DO.FileUpload.SaveAs(path + FileName);
                            _DO.FileScan = "~/UploadedFiles/FileContract/" + FileName;
                        }

                    }
                    var a = db.NT_Contract_insert(_DO.Somecontracts, _DO.Contract, _DO.BeginDate, _DO.EndDate, _DO.FileScan, _DO.IDNT, _DO.IDBP);
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                }    
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Hợp đồng đã tồn tại');</script>";
                }    

                
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Contract");
        }
        public ActionResult Delete(int? id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("DELETE"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.NT_Contract_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Contract");
        }
        public ActionResult Edit(int? id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_Contract.Where(x=>x.IDContract == id)
                      select new ContractValidation
                      {
                          IDContract = (int)a.IDContract,
                          Somecontracts = a.Somecontracts,
                          Contract = a.Contract,
                          BeginDate = (DateTime?)a.BeginDate??default,
                          EndDate = (DateTime?)a.EndDate ??default,
                          FileScan = a.FileScan,
                          IDNT = (int?)a.IDNT?? default,
                          IDBP = (int?)a.IDBP ?? default
                      }).ToList();
            ContractValidation DO = new ContractValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDContract = (int)a.IDContract;
                    DO.Somecontracts = a.Somecontracts;
                    DO.Contract = a.Contract;
                    DO.BeginDate = (DateTime?)a.BeginDate??default;
                    DO.EndDate = (DateTime?)a.EndDate??default;
                    DO.FileScan = a.FileScan;
                    DO.IDNT = (int?)a.IDNT ?? default;
                    DO.IDBP = (int?)a.IDBP ?? default;
                }
                ViewBag.BeginDate = DO.BeginDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = DO.EndDate.ToString("yyyy-MM-dd");

                List<NT_Partner> nt = db.NT_Partner.ToList();
                ViewBag.IDNT = new SelectList(nt, "ID", "FullName", DO.IDNT);


                List<PhongBan> pb = db.PhongBans.ToList();
                ViewBag.IDBP = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDBP);
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(ContractValidation _DO)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                var Check_ = db.NT_Contract.Where(x=>x.IDContract == _DO.IDContract).FirstOrDefault();
             
                //Upload file pdf
                if (_DO.FileUpload != null)
                {
                    string path = Server.MapPath("~/UploadedFiles/FileContract/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = _DO.FileUpload.FileName != null ? _DO.Somecontracts + DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    string FileExtension = _DO.FileUpload != null ? Path.GetExtension(_DO.FileUpload.FileName) : "";

                    if (_DO.FileUpload != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.FileUpload.SaveAs(path + FileName);
                        _DO.FileScan = "~/UploadedFiles/FileContract/" + FileName;
                    }
                    var a = db.NT_Contract_update(_DO.IDContract, _DO.Somecontracts, _DO.Contract, _DO.BeginDate, _DO.EndDate, _DO.FileScan, _DO.IDNT, _DO.IDBP);
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
                }
                else
                {
                    var a = db.NT_Contract_update(_DO.IDContract, _DO.Somecontracts, _DO.Contract, _DO.BeginDate, _DO.EndDate, Check_.FileScan, _DO.IDNT, _DO.IDBP);
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
                }    
              
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Contract");
        }
        public ActionResult CheckInformation(int id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("CHECKINFOR"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var IDCVKN = MyAuthentication.IDPhongban.ToString();
            var TPBP = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == MyAuthentication.IDPhongban || x.IDCVKN.Contains(IDCVKN))
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            ViewBag.TPBPList = new SelectList(TPBP, "IDNhanVien", "HoTen");
            return PartialView();
        }
        [HttpPost]
        public ActionResult CheckInformation(ContractValidation _DO, int? id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("CHECKINFOR"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            try
            {
                db.NT_Contract_updateTK(id, _DO.TPBP, 1);
                TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi trình ký: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Contract");
        }
        public ActionResult Cancel(int? id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("CHECKINFOR"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                db.NT_Contract_updateTK(id, null, 0);

                TempData["msgSuccess"] = "<script>alert('Hủy trình ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Hủy trình ký thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Contract");
        }
        public ActionResult UpdateDay(int? id)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in db.NT_Contract.Where(x => x.IDContract == id)
                       select new ContractValidation
                       {
                           IDContract = (int)a.IDContract,
                           Somecontracts = a.Somecontracts,
                           Contract = a.Contract,
                           BeginDate = (DateTime?)a.BeginDate ?? default,
                           EndDate = (DateTime?)a.EndDate ?? default,
                           FileScan = a.FileScan,
                           IDNT = (int?)a.IDNT ?? default,
                           IDBP = (int?)a.IDBP ?? default
                       }).ToList();
            ContractValidation DO = new ContractValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDContract = (int)a.IDContract;
                    DO.Somecontracts = a.Somecontracts;
                    DO.Contract = a.Contract;
                    DO.BeginDate = (DateTime?)a.BeginDate ?? default;
                    DO.EndDate = (DateTime?)a.EndDate ?? default;
                    DO.FileScan = a.FileScan;
                    DO.IDNT = (int?)a.IDNT ?? default;
                    DO.IDBP = (int?)a.IDBP ?? default;
                }
                ViewBag.BeginDate = DO.BeginDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = DO.EndDate.ToString("yyyy-MM-dd");

                List<NT_Partner> nt = db.NT_Partner.ToList();
                ViewBag.IDNT = new SelectList(nt, "ID", "FullName", DO.IDNT);


                List<PhongBan> pb = db.PhongBans.ToList();
                ViewBag.IDBP = new SelectList(pb, "IDPhongBan", "TenPhongBan", DO.IDBP);
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult UpdateDay(ContractValidation _DO)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            try
            {
                var IDHD = db.NT_Contract.Where(x=>x.IDContract == _DO.IDContract).FirstOrDefault();
                if(IDHD.BeginDate!= _DO.BeginDate && IDHD.EndDate!= _DO.EndDate)
                {
                    var a = db.NT_Contract_update(IDHD.IDContract, IDHD.Somecontracts, IDHD.Contract, _DO.BeginDate, _DO.EndDate, IDHD.FileScan, IDHD.IDNT, IDHD.IDBP);
                }
                db.NT_Contract_updateTK(_DO.IDContract, IDHD.TPBP, 1);
                TempData["msgSuccess"] = "<script>alert('Gia hạn thẻ thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi gia hạn thẻ: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Contract");
        }
    }
}