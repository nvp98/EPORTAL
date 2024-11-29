using EPORTAL.Models;
using EPORTAL.ModelsPartner;
using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Font = iTextSharp.text.Font;
using Rotativa.Options;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class ListExtendcardController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "ListExtendcard";
        // GET: TagSign/ListExtendcard
        public ActionResult Index(int? page, string search, DateTime? begind, DateTime? endd)
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

            var res = (from a in db.NT_Extendcard
                       join b in db.NT_Partner on a.NTID equals b.ID
                       join c in db.NT_Contract on a.HDID equals c.IDContract into ulist1
                       from c in ulist1.DefaultIfEmpty()
                       select new NT_ExtendcardValidation
                       {
                           IDGH = (int)a.IDGH,
                           NoiDung = a.NoiDung,
                           Namefile = a.Namefile,
                           NTID = (int)b.ID,
                           FullName = b.FullName,
                           HDID = (int?)c.IDContract ?? default,
                           Somecontracts = c.Somecontracts ?? default,
                           PhongBanID = (int)a.PhongBanID,
                           NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                           GhiChu = a.GhiChu,
                           IDTTTK = (int)a.IDTTTK,
                           IDNVTK = (int)a.IDNVTK,
                           FileComplete = a.FileComplete

                       }).ToList();
            if (begind != null && endd != null)
            {
                res = res.OrderByDescending(x => x.NgayTrinhKy).Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd).ToList();
            }
            //View theo BP/NM
            if (ListQuyen.Contains("VIEW_BP")) res = res.Where(x => x.PhongBanID == Models.MyAuthentication.IDPhongban).ToList();
            // View theo User
            if (ListQuyen.Contains("VIEW_CN")) res = res.Where(x => x.PhongBanID == Models.MyAuthentication.IDPhongban).ToList();

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);

            return View(res.OrderByDescending(x => x.NgayTrinhKy).ToList().ToPagedList(pageNumber, pageSize));
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
                var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == id).ToList();
                foreach (var delete in Delete)
                {
                    db.NT_DetailExtendcard_delete(delete.IDCTGH);
                }
                db.NT_Extendcard_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListExtendcard");
        }
        public FileResult DownloadExcel()
        {
            string path = "/App_Data/BM_GHT.xlsx";
            return File(path, "application/vnd.ms-excel", "BM_GHT.xlsx");
        }
        public ActionResult Add()
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.IDNT = new SelectList(nt, "ID", "FullName");

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Position> cv = db.NT_Position.ToList();
            ViewBag.IDCV = new SelectList(cv, "IDCV", "TenCV");

            List<NT_Contract> hd = new List<NT_Contract>();
            ViewBag.IDHD = new SelectList(hd, "IDContract", "Somecontracts");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            List<NT_Workingtime> ca = db.NT_Workingtime.ToList();
            ViewBag.IDCA = new SelectList(ca, "IDCA", "TenCA");

            List<NT_Workplace> kv = db.NT_Workplace.ToList();
            ViewBag.IDKV = new SelectList(kv, "IDKV", "TenKV");

            return PartialView();
        }
        [HttpPost]
        public ActionResult Add(NT_Detail_ListExtendcardValidation _DO, FormCollection collection)
        {
            int IDContract = 0;
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("ADD"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            int GHID = 0;
            int GHIDUP = 0;
            int dtc = 1;
            if (collection.Count > 2)
            {
                try
                {
                    string Contents = "Danh sách đăng ký gia hạn và cấp lại thẻ";
                    var ListDS = new List<NT_Detail_ListExtendcardValidation>();
                    DateTime DayUpload = DateTime.Now;
                    var id = db.AuthorizationContractors.Where(x => x.IDNhanVien == Models.MyAuthentication.ID).SingleOrDefault();
                    var IDNV = db.NhanViens.Where(x => x.ID == id.IDNhanVien).FirstOrDefault();

                    //Upload file PDF ĐK học ATLĐ

                    foreach (var key in collection.AllKeys)
                    {
                        if (key != "__RequestVerificationToken")
                        {
                            string IDNT = collection["NTList"];
                            string NT = collection["NT"];
                            if (NT == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn loại nhân viên nhà thầu');</script>";
                                return RedirectToAction("Index", "ListCardRegisInfor");
                            }
                            if (key == "IDHD")
                            {

                                System.Data.Entity.Core.Objects.ObjectParameter returnIDGH = new System.Data.Entity.Core.Objects.ObjectParameter("IDGH", typeof(int));
                                db.NT_Extendcard_insertAll(Contents,NT, Convert.ToInt32(collection["IDNT"]), Convert.ToInt32(collection["IDHD"]), Convert.ToInt32(IDNV.IDPhongBan), DayUpload, _DO.GhiChu,0, Convert.ToInt32(IDNV.ID), returnIDGH);
                                int IDDS = Convert.ToInt32(returnIDGH.Value);
                                GHID = (int)returnIDGH.Value;
                                IDContract = Convert.ToInt32(collection["IDHD"]);
                            }
                            if (key.Split('_')[0] == "ghichu")
                            {
                                string CheckCard = collection["luutru_" + key.Split('_')[1]];
                                if (CheckCard == "1")
                                {
                                    ListDS.Add(new NT_Detail_ListExtendcardValidation()
                                    {
                                        HoTen = collection["fullname_" + key.Split('_')[1]],
                                        CCCD = collection["cccd_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["ngaysinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        DiaChi = collection["hokhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["chucvu_" + key.Split('_')[1]]),
                                        Sdt = collection["sdt_" + key.Split('_')[1]],                                    
                                        DoiThe = collection["luutru_" + key.Split('_')[1]],
                                        //GiaHanThe = collection["ravao_" + key.Split('_')[1]],
                                        //BoSungDt = collection["thoihan_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["thoihan_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        IDCA = Convert.ToInt32(collection["IDCA_" + key.Split('_')[1]]),
                                        IDKV = collection["IDKV_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["IDGroup_" + key.Split('_')[1]]),
                                        CongID = collection["IDGate_" + key.Split('_')[1]],
                                        GhiChu = collection["ghichu_" + key.Split('_')[1]]
                                    });
                                }
                                else if(CheckCard == "2")
                                {
                                 
                                    ListDS.Add(new NT_Detail_ListExtendcardValidation()
                                    {
                                        HoTen = collection["fullname_" + key.Split('_')[1]],
                                        CCCD = collection["cccd_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["ngaysinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        DiaChi = collection["hokhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["chucvu_" + key.Split('_')[1]]),
                                        Sdt = collection["sdt_" + key.Split('_')[1]],
                                        //DoiThe = collection["luutru_" + key.Split('_')[1]],
                                        GiaHanThe = collection["luutru_" + key.Split('_')[1]],
                                        //BoSungDt = collection["thoihan_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["thoihan_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        IDCA = Convert.ToInt32(collection["IDCA_" + key.Split('_')[1]]),
                                        IDKV = collection["IDKV_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["IDGroup_" + key.Split('_')[1]]),
                                        CongID = collection["IDGate_" + key.Split('_')[1]],
                                        GhiChu = collection["ghichu_" + key.Split('_')[1]]
                                    });

                                }
                                else if (CheckCard == "3")
                                {
                                  
                                    ListDS.Add(new NT_Detail_ListExtendcardValidation()
                                    {
                                        HoTen = collection["fullname_" + key.Split('_')[1]],
                                        CCCD = collection["cccd_" + key.Split('_')[1]],
                                        NgaySinh = DateTime.ParseExact(collection["ngaysinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        DiaChi = collection["hokhau_" + key.Split('_')[1]],
                                        ChucVu = Convert.ToInt32(collection["chucvu_" + key.Split('_')[1]]),
                                        Sdt = collection["sdt_" + key.Split('_')[1]],
                                        //DoiThe = collection["luutru_" + key.Split('_')[1]],
                                        //GiaHanThe = collection["ravao_" + key.Split('_')[1]],
                                        BoSungDt = collection["luutru_" + key.Split('_')[1]],
                                        ThoiHanThe = DateTime.ParseExact(collection["thoihan_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                        IDCA = Convert.ToInt32(collection["IDCA_" + key.Split('_')[1]]),
                                        IDKV = collection["IDKV_" + key.Split('_')[1]],
                                        NhomNT = Convert.ToInt32(collection["IDGroup_" + key.Split('_')[1]]),
                                        CongID = collection["IDGate_" + key.Split('_')[1]],
                                        GhiChu = collection["ghichu_" + key.Split('_')[1]]
                                    });

                                }

                            }
                            else if (key.Split('_')[0] == "IDDSList" && collection["IDDSList"] != null)
                            {

                                GHIDUP = Convert.ToInt32(collection["IDDSList"]);

                            }
                        }

                    }

                    if (collection.Count == 5)
                    {
                        string filePath = string.Empty;
                        HttpPostedFileBase file = Request.Files["FileExcel"];
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
                                return View();
                            }
                            DataSet result = reader.AsDataSet();
                            DataTable dt = result.Tables[0];
                            reader.Close();
                            var CheckBirthDay = "";
                            var CheckCreateDate = "";
                            var CheckGroup = "";
                            var CheckIDCA = "";
                            if (dt.Rows.Count > 0)
                            {
                                try
                                {
                                    for (int i = 7; i < dt.Rows.Count; i++)
                                    {
                                        string HoVaTen = dt.Rows[i][1].ToString().Trim();

                                        if (HoVaTen != "")
                                        {
                                            string CCCD = dt.Rows[i][2].ToString().Trim();
                                            var CheckCCCD = db_nt.NT_NhanVienVP.Where(x => x.CCCD == CCCD).FirstOrDefault();
                                            if (CCCD.Length < 9 || CCCD.Length > 14 || CheckCCCD != null)
                                            {
                                                if (GHID != 0)
                                                {
                                                    var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                                    foreach (var d in Delete)
                                                    {
                                                        db.NT_DetailExtendcard_delete(d.IDCTGH);
                                                    }
                                                    db.NT_Extendcard_delete(GHID);
                                                }
                                                if (CheckCCCD != null)
                                                {
                                                    TempData["msgSuccess"] = "<script>alert('Nhân viên nằm trong danh sách vi phạm. Tại dòng : " + dtc + "');</script>";
                                                    return RedirectToAction("Index", "ListExtendcard");
                                                }
                                                else
                                                {
                                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại CCCD. Tại dòng : " + dtc + "');</script>";
                                                    return RedirectToAction("Index", "ListExtendcard");
                                                }
                                            }
                                            string NgaySinh = dt.Rows[i][3].ToString().Trim();
                                            CheckBirthDay = "0";
                                            DateTime BirthDayConvert = DateTime.ParseExact(NgaySinh, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                            CheckBirthDay = BirthDayConvert.ToString();
                                            string DiaChi = dt.Rows[i][4].ToString().Trim();
                                            string ChucVu = dt.Rows[i][5].ToString().Trim();
                                            var IDCV = db.NT_Position.Where(x => x.TenCV == ChucVu).FirstOrDefault();
                                            if (IDCV == null)
                                            {
                                                if (GHID != 0)
                                                {
                                                    var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                                    foreach (var d in Delete)
                                                    {
                                                        db.NT_DetailExtendcard_delete(d.IDCTGH);
                                                    }
                                                    db.NT_Extendcard_delete(GHID);
                                                }

                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại chức vụ. Tại dòng : " + dtc + "');</script>";
                                                return RedirectToAction("Index", "ListExtendcard");
                                            }
                                            string DoiThe = dt.Rows[i][6].ToString().Trim();
                                            string GiaHanThe = dt.Rows[i][7].ToString().Trim();
                                            string BoSungDT = dt.Rows[i][8].ToString().Trim();
                                            if (DoiThe == "" && GiaHanThe == "" && BoSungDT == "")
                                            {
                                                if (GHID != 0)
                                                {
                                                    var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                                    foreach (var d in Delete)
                                                    {
                                                        db.NT_DetailExtendcard_delete(d.IDCTGH);
                                                    }
                                                    db.NT_Extendcard_delete(GHID);
                                                }

                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại thẻ. Tại dòng : " + dtc + "');</script>";
                                                return RedirectToAction("Index", "ListExtendcard");
                                            }
                                            string ThoiHan = dt.Rows[i][9].ToString().Trim();
                                            CheckCreateDate = "0";
                                            var Check = db.NT_Contract.Where(x => x.IDContract == IDContract).FirstOrDefault();
                                            DateTime CreateDateConvert = DateTime.ParseExact(ThoiHan, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                            if (Check.BeginDate < CreateDateConvert)
                                            {
                                                if (GHID != 0)
                                                {
                                                    var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                                    foreach (var d in Delete)
                                                    {
                                                        db.NT_DetailExtendcard_delete(d.IDCTGH);
                                                    }
                                                    db.NT_Extendcard_delete(GHID);
                                                }

                                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra thời hạn thẻ. Tại dòng : " + dtc + "');</script>";
                                                return RedirectToAction("Index", "ListExtendcard");
                                            }
                                            CheckCreateDate = CreateDateConvert.ToString();

                                            string Ca = dt.Rows[i][10].ToString().Trim();
                                            var IDCa = db.NT_Workingtime.Where(x => x.TenCA == Ca).FirstOrDefault();
                                            var check = db.NT_Workingtime.ToList();

                                            if (IDCa == null)
                                            {
                                                CheckIDCA = "0";
                                                CheckIDCA = IDCa.ToString();
                                            }

                                            string NameKhuVuc = dt.Rows[i][11].ToString().Trim();
                                            var NameKV = new List<string>();
                                            string[] arrList = NameKhuVuc.Split(',');
                                            foreach (var str in arrList)
                                            {
                                                var IDKV = db.NT_Workplace.Where(x => x.TenKV == str).FirstOrDefault();

                                                if (IDKV == null && GHID != 0)
                                                {
                                                    var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                                    foreach (var d in Delete)
                                                    {
                                                        db.NT_DetailExtendcard_delete(d.IDCTGH);
                                                    }
                                                    db.NT_Extendcard_delete(GHID);

                                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại khu vực. Tại dòng : " + dtc + "');</script>";
                                                    return RedirectToAction("Index", "ListCardRegisInfor");
                                                }
                                                else
                                                {
                                                    NameKV.Add(IDKV.IDKV.ToString());
                                                }
                                            }
                                            var ListKV = string.Join(",", NameKV);
                                            string NameGate = dt.Rows[i][12].ToString().Trim();
                                            var Name = new List<string>();
                                            string[] arrListStr = NameGate.Split(',');
                                            foreach (var str in arrListStr)
                                            {
                                                var IDGate = db.NT_Gate.Where(x => x.Gate == str).FirstOrDefault();

                                                if (IDGate == null && GHID != 0)
                                                {
                                                    var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                                    foreach (var d in Delete)
                                                    {
                                                        db.NT_DetailExtendcard_delete(d.IDCTGH);
                                                    }
                                                    db.NT_Extendcard_delete(GHID);

                                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại số điện thoại. Tại dòng : " + dtc + "');</script>";
                                                    return RedirectToAction("Index", "ListExtendcard");
                                                }
                                                else
                                                {
                                                    Name.Add(IDGate.IDGate.ToString());
                                                }
                                            }
                                            var ListGate = string.Join(",", Name);
                                            string Note = dt.Rows[i][13].ToString().Trim();
                                            var insert = db.NT_DetailExtendcard_insert
                                            (HoVaTen,
                                            CCCD,
                                            BirthDayConvert,
                                            DiaChi,
                                            IDCV.IDCV,
                                            null,
                                            DoiThe,
                                            GiaHanThe,
                                            BoSungDT,
                                            CreateDateConvert,
                                            IDCa.IDCA,
                                            ListKV,
                                            null,
                                            ListGate,
                                            Note,
                                            GHID);
                                            dtc++;
                                        }

                                    }

                                    return RedirectToAction("Form", "Detail_ListExtendcard", new { id = GHID });
                                }
                                catch (Exception ex)
                                {
                                    if (GHID != 0)
                                    {
                                        var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                        foreach (var d in Delete)
                                        {
                                            db.NT_DetailExtendcard_delete(d.IDCTGH);
                                        }
                                        db.NT_Extendcard_delete(GHID);
                                    }

                                    if (CheckBirthDay == "0")
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại ngày tháng năm sinh. Tại dòng : " + dtc + "');</script>";
                                    }
                                    else if (CheckCreateDate == "0")
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại thời hạn thẻ. Tại dòng : " + dtc + "');</script>";
                                    }
                                    else if (CheckGroup == "0")
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại nhóm NT. Tại dòng : " + dtc + "');</script>";
                                    }
                                    //TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import: " + ex.Message + "');</script>";
                                }

                            }
                            else
                            {
                                if (GHID != 0)
                                {
                                    var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                    foreach (var d in Delete)
                                    {
                                        db.NT_DetailExtendcard_delete(d.IDCTGH);
                                    }
                                    db.NT_Extendcard_delete(GHID);
                                }
                                TempData["msgSuccess"] = "<script>alert('File import không đúng định dạng. Vui lòng tải biểu mẫu file import');</script>";
                            }

                        }
                        else
                        {
                            if (GHID != 0)
                            {
                                var Delete = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).ToList();
                                foreach (var d in Delete)
                                {
                                    db.NT_DetailExtendcard_delete(d.IDCTGH);
                                }
                                db.NT_Extendcard_delete(GHID);
                            }
                            TempData["msgSuccess"] = "<script>alert('Vui lòng nhập dữ liệu');</script>";
                        }

                    }
                    else
                    {
                        foreach (var item in ListDS)
                        {
                            var Check = db.NT_Contract.Where(x => x.IDContract == IDContract).FirstOrDefault();
                            var CheckVP = db_nt.NT_NhanVienVP.Where(x => x.CCCD == item.CCCD).FirstOrDefault();
                            if (item.HoTen != "" && item.CCCD != "" && item.DiaChi != "" && CheckVP == null)
                            {
                                if (GHIDUP == 0)
                                {
                                    var insert = db.NT_DetailExtendcard_insert
                                       (item.HoTen,
                                         item.CCCD,
                                         item.NgaySinh,
                                         item.DiaChi,
                                         item.ChucVu,
                                         item.Sdt,
                                         item.DoiThe,
                                         item.GiaHanThe,
                                         item.BoSungDt,
                                         item.ThoiHanThe,
                                         item.IDCA,
                                         item.IDKV,
                                         item.NhomNT,
                                         item.CongID,
                                         item.GhiChu,
                                         GHID);
                                    dtc++;
                                }
                                else
                                {
                                    var insert = db.NT_DetailExtendcard_insert
                                      (item.HoTen,
                                         item.CCCD,
                                         item.NgaySinh,
                                         item.DiaChi,
                                         item.ChucVu,
                                         item.Sdt,
                                         item.DoiThe,
                                         item.GiaHanThe,
                                         item.BoSungDt,
                                         item.ThoiHanThe,
                                         item.IDCA,
                                         item.IDKV,
                                         item.NhomNT,
                                         item.CongID,
                                         item.GhiChu,
                                        GHIDUP);
                                }

                            }
                            else
                            {
                                var Detai = db.NT_DetailExtendcard.Where(x => x.IDGH == GHID).Count();
                                if (GHID != 0 && Detai == 0)
                                {
                                    db.NT_Extendcard_delete(GHID);
                                }
                                if (Check.BeginDate < item.ThoiHanThe)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra thời hạn thẻ tại dòng số: " + dtc + "');</script>";
                                }
                                else if (item.HoTen != "" || item.CCCD != "" || item.DiaChi != "")
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng nhập đủ thông tin dòng số: " + dtc + "');</script>";
                                }
                                else if (CheckVP != null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Nhân viên nằm trong danh sách vi phạm. Tại dòng: " + dtc + "');</script>";
                                }

                            }

                        }
                    }


                }
                catch (Exception e)
                {
                    if (GHID != 0)
                    {
                        db.NT_Extendcard_delete(GHID);
                    }
                    TempData["msgError"] = "<script>alert('Vui lòng nhập đầy đủ thông tin');</script>";
                }
            }
            else
            {
                foreach (var key in collection.AllKeys)
                {
                    if (key != "__RequestVerificationToken")
                    {
                        if (key == "IDNT")
                        {
                            string IDNT = collection["IDNT"].ToString();
                            if (IDNT == "")
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhà thầu');</script>";
                                return RedirectToAction("Index", "ListExtendcard");
                            }
                            else if (_DO.FileUpload == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng chọn File PDF');</script>";
                                return RedirectToAction("Index", "ListExtendcard");
                            }
                        }

                    }

                }

                //TempData["msgError"] = "<script>alert('Vui lòng nhập thông tin');</script>";
            }

            //return View();
            DateTime Day = DateTime.Now;
            return RedirectToAction("Index", "ListExtendcard");
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

            var IDCVKN = Models.MyAuthentication.IDPhongban.ToString();
            //KTV ký nháy
            var KTV = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 2 && x.NhanVien.IDPhongBan == Models.MyAuthentication.IDPhongban)
                       join a in db.NhanViens on au.IDNhanVien equals a.ID
                       select new CheckInforUser
                       {
                           IDNhanVien = (int)au.IDNhanVien,
                           HoTen = a.HoTen + " : " + a.MaNV,
                       }).ToList();
            //TP BP/NM
            var TPBP = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == Models.MyAuthentication.IDPhongban || x.IDCVKN.Contains(IDCVKN))
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            //TP BP/NM
            var TPHCDN = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 1 && x.NhanVien.IDPhongBan == 7)
                          join a in db.NhanViens on au.IDNhanVien equals a.ID
                          select new CheckInforUser
                          {
                              IDNhanVien = (int)au.IDNhanVien,
                              HoTen = a.HoTen + " : " + a.MaNV,
                          }).ToList();
            //VP1 C
            var VP1C = (from au in db.AuthorizationContractors.Where(x => x.IDLKD == 3)
                        join a in db.NhanViens on au.IDNhanVien equals a.ID
                        select new CheckInforUser
                        {
                            IDNhanVien = (int)au.IDNhanVien,
                            HoTen = a.HoTen + " : " + a.MaNV,
                        }).ToList();

            ViewBag.KTVList = new SelectList(KTV, "IDNhanVien", "HoTen");
            ViewBag.TPBPList = new SelectList(TPBP, "IDNhanVien", "HoTen");
            ViewBag.TPHCDNList = new SelectList(TPHCDN, "IDNhanVien", "HoTen");
            ViewBag.VP1CList = new SelectList(VP1C, "IDNhanVien", "HoTen");
            ViewBag.ID = id;



            return PartialView();
        }
        [HttpPost]
        public ActionResult CheckInformation(RenewalSignerValidatinon _DO, int? id)
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

                if (_DO.IDBPQL != 0 && _DO.IDVP1C != 0)
                {
                    var Check = db.KD_TrinhKyGH.Where(x => x.IDGH == id).FirstOrDefault();
                    if (Check == null)
                    {
                        db.KD_TrinhKyGH_Insert(id, _DO.IDKTV, 0, _DO.IDBPQL, 0, _DO.IDVP1C, 0);
                    }
                    else
                    {
                        db.KD_TrinhKy_UpdateTK(Check.IDTKGH, id, _DO.IDKTV, 0, _DO.IDBPQL, 0, _DO.IDVP1C, 0);
                    }

                    db.NT_Extendcard_updateTK(id, 1);

                    TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";

                }
                else if (_DO.IDBPQL == 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn BPQL');</script>";
                }
                else if (_DO.IDVP1C == 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn VP1C');</script>";
                }

            }

            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Trình ký thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListExtendcard");
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
                var IDKT = db.KD_TrinhKyGH.Where(x => x.IDGH == id).FirstOrDefault();
                if (IDKT != null)
                {
                    db.KD_TrinhKyGH_delete(IDKT.IDTKGH);

                    db. NT_Extendcard_updateTK(id, 0);
                }
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListExtendcard");
        }
    }
}