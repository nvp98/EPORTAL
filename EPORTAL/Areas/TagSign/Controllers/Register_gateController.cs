using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using EPORTAL.Models;
using ExcelDataReader;
using System.Data;
using System.IO;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class Register_gateController : Controller
    {
        // GET: TagSign/Register_gate
        EPORTALEntities db = new EPORTALEntities();

        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Register_gate";
        // GET: TagSign/Gate
        public string convertToUnSign2(string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }
        public ActionResult Index(int? page, string search = "")
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            String searchCovert = convertToUnSign2(search);
            var res = db.sp_RegisterGate_Search(searchCovert).ToList();

            var data = (from a in db.sp_RegisterGate_Search(searchCovert).ToList()
                        join b in db.NT_Gate.ToList() on a.IDCong equals b.IDGate into cong
                        from c in cong.DefaultIfEmpty()
                        join d in db.NhanViens.ToList() on a.MaNV equals d.MaNV
                        select new Register_gates()
                        {
                            IDRE = a.IDRE,
                            IDTrangThai = a.TrangThai,
                            TrangThai = (a.TrangThai == 0) ? "Khóa" : "Mở",
                            NgayDK = (DateTime)(a.NgayDK ?? default),
                            TenNV = d.HoTen,
                            Gate = (a.IDCong == 1) ? "Tất cả" : c.Gate,
                            IDCong = a.IDCong,
                            MaNV = d.MaNV,
                            NgayHetHan = (DateTime)(a.NgayHetHan ?? default)
                        }).ToList();
            ViewBag.Quyen = 1;
            var checkADD = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (checkADD == 0) { ViewBag.Quyen = 0; }
            page = page ?? 1;
            int pageSize = 50;
            int pageNumber = page ?? 1;
            return View(data.OrderByDescending(x => x.IDRE).ToList().ToPagedList(pageNumber, pageSize));
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

            var nhanVien = db.NhanViens.Select(x => new NhanVienModel() { MaNV = x.MaNV, TenNV = x.HoTen }).ToList();
            ViewBag.nhanVien = new SelectList(nhanVien, "MaNV", "TenNV");
            var Cong = (from a in db.NT_Gate.ToList()
                        where a.TinhTrang == 1
                        select new GateValidation() { IDGate = a.IDGate, Gate = a.Gate }).ToList();
            ViewBag.Cong = new SelectList(Cong, "IDGate", "Gate");
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(Register_gates model)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (model.IDCong!=null && model.MaNV != null)
            {
                var check = db.Register_Gate.Where(x => x.IDCong == model.IDCong && x.MaNV == model.MaNV).ToList();
                if (!check.Any())
                {
                    DateTime dateUp = DateTime.Now;
                    if (model.NgayHetHan != null)
                    {
                        if (DateTime.Compare(model.NgayHetHan, dateUp) > 0)
                        {
                            TempData["msgError"] = "<script>alert('Ngày hết hạn phải lớn hơn ngày hiên tại');</script>";
                            return RedirectToAction("Index", "Register_gate");
                        }
                    }
                    try
                    {
                        db.sp_ReGate_Insert(model.IDCong, model.MaNV, 1, dateUp, model.NgayHetHan);
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                    catch (Exception e)
                    {
                        TempData["msgError"] = "<script>alert('Có lỗi khi thêm mới: " + e.Message + "');</script>";

                    }
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Nhân viên này đã được đăng ký ra vào cổng này trước đó');</script>";
                }
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Thông tin không được để trống');</script>";
            }
           

            return RedirectToAction("Index", "Register_gate");
        }
        [HttpPost]
        public ActionResult upLoadfile(HttpPostedFileBase file)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            int dtc = 1;
            int count = 0;
            int temp;
            string dongEx = "";
            string filePath = string.Empty;
            file = Request.Files["FileExcel"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string path = Server.MapPath("~/UploadedFiles/DKHocATEX/");
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
                    return RedirectToAction("Index", "Register_gate");
                }
                DataSet result = reader.AsDataSet();
                DataTable dt = result.Tables[0];
                temp = dt.Rows.Count;
                reader.Close();

                if (dt.Rows.Count > 6)
                {

                    try
                    {
                        for (int i = 6; i < dt.Rows.Count; i++)
                        {
                            string tenCong = dt.Rows[i][1].ToString().Trim();
                            int? IDCong = null;
                            if (String.Compare(tenCong, "Tất cả", true) == 0)
                            {
                                IDCong = 1;
                            }
                            else
                            {
                                var Cong = db.NT_Gate.Where(x => x.Gate == tenCong).SingleOrDefault();
                                if (Cong == null)
                                {
                                    if (dongEx != "")
                                    {
                                        dongEx = dongEx.Trim(',');
                                        TempData["msgExist"] = "<script>alert('Thông tin dòng " + dongEx + " đã được đăng ký trước đó');</script>";
                                    }
                                    TempData["msgError"] = "<script>alert('kiểm tra lại thông tin dòng: " +dtc + ",hãy cập nhập và import lại từ dòng  " + dtc + "');</script>";
                                    return RedirectToAction("Index", "Register_gate");
                                }
                                else
                                {
                                    IDCong = Cong.IDGate;

                                }
                            }
                            string maNV = dt.Rows[i][2].ToString().Trim();

                            var NV = db.NhanViens.Where(x => x.MaNV == maNV).SingleOrDefault();
                            if (maNV == null)
                            {
                                if (dongEx != "")
                                {
                                    dongEx = dongEx.Trim(',');
                                    TempData["msgExist"] = "<script>alert('Thông tin dòng " + dongEx + " đã được đăng ký trước đó');</script>";
                                }
                                TempData["msgError"] = "<script>alert('kiểm tra lại thông tin dòng: " + dtc + ",hãy cập nhập và import lại từ dòng " + dtc + "');</script>";
                                return RedirectToAction("Index", "Register_gate");
                            }

                            string TenNV = dt.Rows[i][3].ToString().Trim();

                            if (String.Compare(TenNV, NV.HoTen.ToString(), true) != 0)
                            {
                                if (dongEx != "")
                                {
                                    dongEx = dongEx.Trim(',');
                                    TempData["msgExist"] = "<script>alert('Thông tin dòng " + dongEx + " đã được đăng ký trước đó');</script>";
                                }
                                TempData["msgError"] = "<script>alert('kiểm tra lại thông tin dòng: " + dtc + ",hãy cập nhập và import lại từ dòng " + dtc + "');</script>";
                                return RedirectToAction("Index", "Register_gate");
                            }
                            var a = db.Register_Gate.Where(x => x.IDCong == IDCong && x.MaNV == maNV).ToList();
                            if (a.Any())
                            {
                                dongEx += dtc +",";                              
                                dtc++;
                                count++;
                                continue;
                            }
                            string ngayDkEx = dt.Rows[i][4].ToString().Trim();
                            DateTime? ngayDK = null;
                            if (ngayDkEx == "")
                            {
                                ngayDK = DateTime.Now;
                            }
                            else
                            {
                                ngayDK = DateTime.ParseExact(ngayDkEx, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }
                            string ngayHHEx = dt.Rows[i][5].ToString().Trim();
                            DateTime? ngayHH = null;
                            if (ngayHHEx != "")
                            {
                                ngayHH = DateTime.ParseExact(ngayHHEx, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }

                            db.sp_ReGate_Insert(IDCong, maNV, 1, ngayDK, ngayHH);
                            dtc++;
                        }                                                    
                    }
                    catch (Exception ex)
                    {
                        if (dongEx != "")
                        {
                            dongEx = dongEx.Trim(',');
                            TempData["msgExist"] = "<script>alert('Thông tin dòng " + dongEx + " đã được đăng ký trước đó');</script>";
                        }
                        TempData["msgError"] = "<script>alert('Vui lòng kiểm tra lại dòng : " + dtc + ",hãy cập nhập và import lại từ dòng: " + dtc + "');</script>";
                        return RedirectToAction("Index", "Register_gate");
                    }
                }
                else
                {
                    TempData["msgError"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                    return RedirectToAction("Index", "Register_gate");
                }

            }
            else
            {
                TempData["msgError"] = "<script>alert('Vui lòng chọn File Excel');</script>";
                return RedirectToAction("Index", "Register_gate");
            }
            if (dongEx != "")
            {
                dongEx = dongEx.Trim(',');
                TempData["msgExist"] = "<script>alert('Thông tin dòng " + dongEx + " đã được đăng ký trước đó');</script>";
            }
            if ((temp - count) >= 7)
            {
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            return RedirectToAction("Index", "Register_gate");
        }
        public ActionResult Delete(int id)
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
                var re_ter = db.Register_Gate.Where(x => x.IDRE == id).SingleOrDefault();
                if (re_ter == null)
                {
                    TempData["msgError"] = "<script>alert('Xóa Thất Bại');</script>";
                }
                else
                {
                    db.sp_ReGate_Delete(id);
                    TempData["msgSuccess"] = "<script>alert('Xóa đăng ký ra vào cổng thành công');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi Xóa: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Register_gate");

        }
        public ActionResult DownloadBM()
        {
            string path = "/App_Data/BM_dang ky cham cong van tay.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_dang ky cham cong van tay.xlsx");
        }

        public ActionResult Edit(int id)
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
                var re_ter = (from a in db.Register_Gate.ToList()
                              join b in db.NhanViens.ToList() on a.MaNV equals b.MaNV

                              join c in db.NT_Gate.ToList() on a.IDCong equals c.IDGate
                              where a.IDRE == id
                              select new Register_gates()
                              {
                                  IDRE = a.IDRE,
                                  IDCong = c.IDGate,
                                  MaNV = b.MaNV,
                                  TenNV = b.HoTen,
                                  Gate = c.Gate,
                                  IDTrangThai = a.TrangThai,
                                  NgayDK = (DateTime)(a.NgayDK ?? default),
                                  NgayHetHan = (DateTime)(a.NgayHetHan ?? default),

                              }).SingleOrDefault();
                if (re_ter != null)
                {
                    var nhanVien = db.NhanViens.Select(x => new NhanVienModel() { MaNV = x.MaNV, TenNV = x.HoTen });
                    ViewBag.nhanVien = new SelectList(nhanVien, "MaNV", "TenNV", re_ter.MaNV);
                    var Cong = (from a in db.NT_Gate.ToList()
                                where a.TinhTrang == 1
                                select new GateValidation() { IDGate = a.IDGate, Gate = a.Gate }).ToList();
                    ViewBag.Cong = new SelectList(Cong, "IDGate", "Gate", re_ter.IDCong);
                    ViewBag.ngayHH = re_ter.NgayHetHan.ToString("yyyy-MM-dd");

                    return PartialView(re_ter);
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Đã có lỗi xãy ra: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "Register_gate");
        }
        [HttpPost]
        public ActionResult Edit(Register_gates model)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (model.IDCong != null && model.MaNV != null)
            {
                DateTime dateUp = DateTime.Now;

                try
                {
                    var re_ter = db.Register_Gate.Where(x => x.IDRE == model.IDRE).SingleOrDefault();

                    if (re_ter == null)
                    {
                        TempData["msgError"] = "<script>alert('Sửa Thất Bại');</script>";
                    }
                    else
                    {
                        if (re_ter.NgayDK != null && re_ter.NgayHetHan != null && re_ter.NgayDK.ToString() != "1/1/0001 12:00:00 AM" && re_ter.NgayHetHan.ToString() != "1/1/0001 12:00:00 AM")
                        {
                            if (DateTime.Compare(model.NgayHetHan, dateUp) > 0)
                            {
                                db.sp_ReGate_Update(model.IDRE, model.IDCong, model.MaNV, model.IDTrangThai, model.NgayDK, model.NgayHetHan);
                                TempData["msgSuccess"] = "<script>alert('Sửa đăng ký ra vào cổng thành công');</script>";

                            }
                            else
                            {
                                TempData["msgError"] = "<script>alert('Ngày hết hạn phải lớn hơn ngày hiên tại');</script>";
                            }

                        }
                        else
                        {
                            db.sp_ReGate_Update(model.IDRE, model.IDCong, model.MaNV, model.IDTrangThai, model.NgayDK, model.NgayHetHan);
                            TempData["msgSuccess"] = "<script>alert('Sửa đăng ký ra vào cổng thành công');</script>";
                        }
                    }
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('Đã có lỗi xãy ra khi sửa: " + e.Message + "');</script>";
                }

            }
            else
            {
                TempData["msgError"] = "<script>alert('Các thông tin nhập vào không được bỏ trống');</script>";
            }

            return RedirectToAction("Index", "Register_gate");
        }
    }
}