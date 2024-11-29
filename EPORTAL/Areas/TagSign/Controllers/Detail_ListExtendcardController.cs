using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using ExcelDataReader;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPORTAL.ModelsPartner;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class Detail_ListExtendcardController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_NTEntities db_nt = new EPORTAL_NTEntities();
        // GET: TagSign/Detail_ListExtendcard
        public ActionResult Index(int? id, int? page)
        {
            List<NT_Contract> hd = db.NT_Contract.ToList();
            ViewBag.HDList = new SelectList(hd, "IDContract", "Somecontracts");

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName");

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Workplace> kv = db.NT_Workplace.ToList();
            ViewBag.IDKV = new SelectList(kv, "IDKV", "TenKV");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            var res = from a in db.NT_DetailExtendcard.Where(x => x.IDGH == id)
                      join n in db.NT_ContractorGroup on a.NhomNT equals n.IDGroup into ulist1
                      from n in ulist1.DefaultIfEmpty()
                      join cv in db.NT_Position on a.ChucVu equals cv.IDCV
                      join ca in db.NT_Workingtime on a.IDCA equals ca.IDCA
                      select new NT_Detail_ListExtendcardValidation
                      {
                          IDCTGH = (int)a.IDCTGH,
                          HoTen = a.HoTen,
                          CCCD = a.CCDC,
                          NgaySinh = (DateTime)a.NgaySinh,
                          DiaChi = a.DiaChi,
                          ChucVu = (int)a.ChucVu,
                          NamePosition = cv.TenCV,
                          Sdt = a.Sdt,
                          DoiThe = a.DoiThe,
                          GiaHanThe = a.GiaHanThe,
                          BoSungDt = a.BoSungDt,
                          ThoiHanThe = (DateTime)a.ThoiHanThe,
                          IDCA = (int)a.IDCA,
                          IDKV = a.IDKV,
                          NhomNT = (int?)a.NhomNT ?? default,
                          NameContractorGroup = n.NameContractorGroup,
                          CongID = a.CongID,
                          GhiChu = a.GhiChu,
                          IDGH = (int?)a.IDGH ?? default

                      };
            if (id != null)
            {
                var Idds = db.NT_Extendcard.Where(x => x.IDGH == id).SingleOrDefault();
                NT_ExtendcardValidation idgh = new NT_ExtendcardValidation()
                {
                    IDGH = (int)id
                };
                ViewData["id"] = idgh;
            }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);

            return View(res.OrderBy(x => x.IDCTGH).ToList().ToPagedList(pageNumber, pageSize));

        }
        public ActionResult Form(int? id)
        {
            var TD = db.NT_Extendcard.Where(x => x.IDGH == id).SingleOrDefault();

            List<NT_Partner> nt = db.NT_Partner.ToList();
            ViewBag.NTList = new SelectList(nt, "ID", "FullName", Convert.ToInt32(TD.NTID));

            List<NT_Contract> hd = db.NT_Contract.ToList();
            ViewBag.HDList = new SelectList(hd, "IDContract", "Somecontracts", Convert.ToInt32(TD.HDID));

            List<NT_ContractorGroup> nhom = db.NT_ContractorGroup.ToList();
            ViewBag.IDGroup = new SelectList(nhom, "IDGroup", "NameContractorGroup");

            List<NT_Gate> cong = db.NT_Gate.ToList();
            ViewBag.IDGate = new SelectList(cong, "IDGate", "Gate");

            List<NT_Position> cv = db.NT_Position.ToList();
            ViewBag.IDCV = new SelectList(cv, "IDCV", "TenCV");

            List<NT_Workingtime> ca = db.NT_Workingtime.ToList();
            ViewBag.IDCA = new SelectList(ca, "IDCA", "TenCA");

            List<NT_Workplace> kv = db.NT_Workplace.ToList();
            ViewBag.IDKV = new SelectList(kv, "IDKV", "TenKV");

            if (id != null)
            {
                var Idds = db.NT_Extendcard.Where(x => x.IDGH == id).SingleOrDefault();
                NT_ExtendcardValidation idgh = new NT_ExtendcardValidation()
                {
                    IDGH = (int)id
                };
                ViewData["id"] = idgh;
            }
            var res = (from a in db.NT_DetailExtendcard.Where(x => x.IDGH == id)
                       select new NT_Detail_ListExtendcardValidation
                       {
                           IDCTGH = (int)a.IDCTGH,
                           HoTen = a.HoTen,
                           CCCD = a.CCDC,
                           NgaySinh = (DateTime)a.NgaySinh,
                           DiaChi = a.DiaChi,
                           ChucVu = (int)a.ChucVu,
                           Sdt = a.Sdt,
                           DoiThe = a.DoiThe,
                           GiaHanThe = a.GiaHanThe,
                           BoSungDt = a.BoSungDt,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           IDCA = (int)a.IDCA,
                           IDKV = a.IDKV,
                           NhomNT = (int?)a.NhomNT ?? default,
                           CongID = a.CongID,
                           GhiChu = a.GhiChu,
                           IDGH = (int?)a.IDGH ?? default

                       }).ToList();
            NT_Detail_ListExtendcardValidation DO = new NT_Detail_ListExtendcardValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDCTGH = (int)a.IDCTGH;
                    DO.HoTen = a.HoTen;
                    DO.CCCD = a.CCCD;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.DiaChi = a.DiaChi;
                    DO.ChucVu = (int)a.ChucVu;
                    DO.Sdt = a.Sdt;
                    DO.DoiThe = a.DoiThe;
                    DO.GiaHanThe = a.GiaHanThe;
                    DO.BoSungDt = a.BoSungDt;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.IDCA = (int)a.IDCA;
                    DO.IDKV = a.IDKV;
                    DO.NhomNT = (int?)a.NhomNT ?? default;
                    DO.CongID = a.CongID;
                    DO.GhiChu = a.GhiChu;
                    DO.IDGH = (int?)a.IDGH ?? default;
                }
                ViewBag.BirthDay = DO.NgaySinh.ToString("yyyy-MM-dd");

                ViewBag.CreateDate = DO.ThoiHanThe.ToString("yyyy-MM-dd");

                List<NT_ContractorGroup> n = db.NT_ContractorGroup.ToList();
                ViewBag.IDGroup = new SelectList(n, "IDGroup", "NameContractorGroup", DO.NhomNT);

                List<NT_Gate> c = db.NT_Gate.ToList();
                ViewBag.IDGate = new SelectList(c, "IDGate", "Gate", DO.CongID);

                List<NT_Workplace> nkv = db.NT_Workplace.ToList();
                ViewBag.IDKV = new SelectList(nkv, "IDKV", "TenKV", DO.IDKV);
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Form(NT_Detail_ListExtendcardValidation _DO, FormCollection collection)
        {
            try
            {
                int IDContract = 0;
                int GHID = 0;
                int dtc = 1;
                var ListDS = new List<NT_Detail_ListExtendcardValidation>();
                //Upload file 
                if (_DO.FileUpload != null)
                {
                    string path = Server.MapPath("~/FileContract/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = _DO.FileUpload.FileName != null ? DateTime.Now.ToString("yyyyMMddHHmm") : "";

                    string FileExtension = _DO.FileUpload != null ? Path.GetExtension(_DO.FileUpload.FileName) : "";

                    if (_DO.FileUpload != null)
                    {
                        FileName = FileName.Trim() + FileExtension;
                        _DO.FileUpload.SaveAs(path + FileName);
                        _DO.Namefile = "~/FileContract/" + FileName;
                    }

                }
                foreach (var key in collection.AllKeys)
                {
                    if (key != "__RequestVerificationToken")
                    {
                        string IDNT = collection["NTList"];
                        string IDHD = collection["HDList"];
                        if (key == "HDList" && _DO.FileUpload == null)
                        {
                            var FileUpdate = db.NT_Extendcard.Where(x => x.IDGH == _DO.IDGH).FirstOrDefault();
                            db.NT_Extendcard_Detail(_DO.IDGH, null, Convert.ToInt32(collection["NTList"]), Convert.ToInt32(collection["HDList"]));
                            IDContract = Convert.ToInt32(collection["HDList"]);
                            GHID = _DO.IDGH;
                        }
                        else if (key == "HDList" && _DO.FileUpload != null)
                        {
                            db.NT_Extendcard_Detail(_DO.IDGH, null, Convert.ToInt32(collection["NTList"]), Convert.ToInt32(collection["HDList"]));
                            IDContract = Convert.ToInt32(collection["HDList"]);
                            GHID = _DO.IDGH;
                        }

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
                                //Sdt = collection["sdt_" + key.Split('_')[1]],
                                //PhoneInformation = collection["imel_" + key.Split('_')[1]],
                                DoiThe = collection["luutru_" + key.Split('_')[1]],
                                GiaHanThe = collection["luutru_" + key.Split('_')[1]],
                                BoSungDt = collection["luutru_" + key.Split('_')[1]],
                                //AccessCard = collection["ravao_" + key.Split('_')[1]],
                                IDCA = Convert.ToInt32(collection["IDCA_" + key.Split('_')[1]]),
                                IDKV = collection["IDKV_" + key.Split('_')[1]],
                                ThoiHanThe = DateTime.ParseExact(collection["thoihan_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                NhomNT = Convert.ToInt32(collection["IDGroup_" + key.Split('_')[1]]),
                                CongID = collection["IDGate_" + key.Split('_')[1]],
                                GhiChu = collection["ghichu_" + key.Split('_')[1]]
                            });
                        }
                        else
                        {
                            ListDS.Add(new NT_Detail_ListExtendcardValidation()
                            {
                                HoTen = collection["fullname_" + key.Split('_')[1]],
                                CCCD = collection["cccd_" + key.Split('_')[1]],
                                NgaySinh = DateTime.ParseExact(collection["ngaysinh_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                DiaChi = collection["hokhau_" + key.Split('_')[1]],
                                ChucVu = Convert.ToInt32(collection["chucvu_" + key.Split('_')[1]]),
                                //Sdt = collection["sdt_" + key.Split('_')[1]],
                                //PhoneInformation = collection["imel_" + key.Split('_')[1]],
                                DoiThe = collection["luutru_" + key.Split('_')[1]],
                                GiaHanThe = collection["luutru_" + key.Split('_')[1]],
                                BoSungDt = collection["luutru_" + key.Split('_')[1]],
                                //AccessCard = collection["ravao_" + key.Split('_')[1]],
                                IDCA = Convert.ToInt32(collection["IDCA_" + key.Split('_')[1]]),
                                IDKV = collection["IDKV_" + key.Split('_')[1]],
                                ThoiHanThe = DateTime.ParseExact(collection["thoihan_" + key.Split('_')[1]], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None),
                                NhomNT = Convert.ToInt32(collection["IDGroup_" + key.Split('_')[1]]),
                                CongID = collection["IDGate_" + key.Split('_')[1]],
                                GhiChu = collection["ghichu_" + key.Split('_')[1]]
                            });

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

                        return RedirectToAction("Form", "Detail_ListExtendcard", new { id = GHID });
                    }
                }

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            return RedirectToAction("Index", "ListExtendcard");
        }

        public ActionResult Delete(int? id)
        {
            var IDGH = db.NT_DetailExtendcard.Where(x => x.IDCTGH == id).FirstOrDefault();
            try
            {
                db.NT_DetailExtendcard_delete(id);
            }
            catch (Exception e)
            {
                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thất bại: " + e.Message + "');</script>";
            }
            return RedirectToAction("Form", "Detail_ListExtendcard", new { id = Convert.ToInt32(IDGH.IDGH) });

        }
        public ActionResult Edit(int? id)
        {
            var res = (from a in db.NT_DetailExtendcard.Where(x => x.IDCTGH == id)
                       select new NT_Detail_ListExtendcardValidation
                       {
                           IDCTGH = (int)a.IDCTGH,
                           HoTen = a.HoTen,
                           CCCD = a.CCDC,
                           NgaySinh = (DateTime)a.NgaySinh,
                           DiaChi = a.DiaChi,
                           ChucVu = (int)a.ChucVu,
                           Sdt = a.Sdt,
                           DoiThe = a.DoiThe,
                           GiaHanThe = a.GiaHanThe,
                           BoSungDt = a.BoSungDt,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           IDCA = (int)a.IDCA,
                           IDKV = a.IDKV,
                           NhomNT = (int?)a.NhomNT ?? default,
                           CongID = a.CongID,
                           GhiChu = a.GhiChu,
                           IDGH = (int?)a.IDGH ?? default

                       }).ToList();
            NT_Detail_ListExtendcardValidation DO = new NT_Detail_ListExtendcardValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDCTGH = (int)a.IDCTGH;
                    DO.HoTen = a.HoTen;
                    DO.CCCD = a.CCCD;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.DiaChi = a.DiaChi;
                    DO.ChucVu = (int)a.ChucVu;
                    DO.Sdt = a.Sdt;
                    DO.DoiThe = a.DoiThe;
                    DO.GiaHanThe = a.GiaHanThe;
                    DO.BoSungDt = a.BoSungDt;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.IDCA = (int)a.IDCA;
                    DO.IDKV = a.IDKV;
                    DO.NhomNT = (int?)a.NhomNT ?? default;
                    DO.CongID = a.CongID;
                    DO.GhiChu = a.GhiChu;
                    DO.IDGH = (int?)a.IDGH ?? default;
                }
                ViewBag.BirthDay = DO.NgaySinh.ToString("yyyy-MM-dd");

                ViewBag.CreateDate = DO.ThoiHanThe.ToString("yyyy-MM-dd");

                List<NT_ContractorGroup> n = db.NT_ContractorGroup.ToList();
                ViewBag.NhomNT = new SelectList(n, "IDGroup", "NameContractorGroup");

                List<NT_Gate> c = db.NT_Gate.ToList();
                ViewBag.Selected = new SelectList(c, "IDGate", "Gate");

                List<NT_Workplace> nkv = db.NT_Workplace.ToList();
                ViewBag.SelectedKV = new SelectList(nkv, "IDKV", "TenKV");

                List<NT_Workingtime> ca = db.NT_Workingtime.ToList();
                ViewBag.IDCA = new SelectList(ca, "IDCA", "TenCA");

                List<NT_Position> cv = db.NT_Position.ToList();
                ViewBag.IDCV = new SelectList(cv, "IDCV", "TenCV");
            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);
        }
        [HttpPost]
        public ActionResult Edit(NT_Detail_ListExtendcardValidation _DO)
        {
            var IDGH = db.NT_DetailExtendcard.Where(x => x.IDCTGH == _DO.IDCTGH).FirstOrDefault();
            var ID = db.NT_Extendcard.Where(x => x.IDGH == IDGH.IDGH).FirstOrDefault();
            var IDHD = db.NT_Contract.Where(x => x.IDContract == ID.HDID).FirstOrDefault();
            try
            {
                string NameKhuVuc = "";
                foreach (var name in _DO.SelectedKV)
                {
                    if (NameKhuVuc == "")
                    {
                        NameKhuVuc = String.Concat(name);
                    }
                    else
                    {
                        NameKhuVuc = String.Concat(NameKhuVuc + "," + name);
                    }
                }
                string Name = "";
                foreach (var name in _DO.Selected)
                {
                    if (Name == "")
                    {
                        Name = String.Concat(name);
                    }
                    else
                    {
                        Name = String.Concat(Name + "," + name);
                    }
                }
                //if (IDHD.BeginDate < _DO.ThoiHanThe)
                //{
                //    TempData["msgError"] = "<script>alert('Kiểm tra lại thời hạn thẻ');</script>";
                //}
                //else
                {
                    var a = db.NT_DetailExtendcard_update
                     (_DO.IDCTGH,
                     _DO.HoTen,
                     _DO.CCCD,
                     _DO.NgaySinh,
                     _DO.DiaChi,
                     _DO.ChucVu,
                     _DO.Sdt,
                     _DO.DoiThe,
                     _DO.GiaHanThe,
                     _DO.BoSungDt,
                     _DO.ThoiHanThe,
                     _DO.IDCA,
                     NameKhuVuc,
                     _DO.NhomNT,
                         Name,
                     _DO.GhiChu,
                      Convert.ToInt32(IDGH.IDGH));

                    TempData["msgError"] = "<script>alert('Chỉnh sửa thành công');</script>";
                }

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Form", "Detail_ListExtendcard", new { id = Convert.ToInt32(IDGH.IDGH) });
        }
    }
}