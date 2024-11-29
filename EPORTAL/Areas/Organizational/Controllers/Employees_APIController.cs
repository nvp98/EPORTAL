using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ExcelDataReader;
using Newtonsoft.Json;
using PagedList;
using SoDoToChuc.Models;
using EPORTAL.ModelsOrganizational;
using EPORTAL.Models;

namespace EPORTAL.Areas.Organizational.Controllers
{
    public  class Employees_APIController : Controller
    {
        // GET: Employee
        SoDoToChucEntities _db = new SoDoToChucEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "Employees_API";
        public ActionResult Index(int? page,int? IDPhongBan,string IDMaVT,string search)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.VIEW_ALL).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;

            List<PhongBan> pb = _db.PhongBans.ToList();
            //if (IDPhongBan == null) IDPhongBan = 0;
            ViewBag.PBList = new SelectList(pb, "IDPhongBan", "TenPhongBan", IDPhongBan);
            List<MaViTri> mvt = _db.MaViTris.ToList();
            ViewBag.MaVT = new SelectList(mvt, "TenMaViTri", "TenMaViTri", IDMaVT);
            IList<SelectListItem> ttlv = new List<SelectListItem>
            {
                new SelectListItem{Text="Còn làm việc",Value ="1"},
                new SelectListItem{Text ="Đã nghỉ việc",Value ="0"}
            };
            //List<NhanVienAPI> nv = _db.NhanVienAPIs.Where(x => x.IDTinhTrangLV == 1).ToList();
            //var ListSearch = _db.NhanVienAPI_select(search).ToList();
            //ViewBag.MaNV = new SelectList(nv, "manv", "manv", MaNV);
            var res = (from a in _db.NhanVienAPI_select(search)
                       join b in _db.PhongBans on a.IDPhongBan equals b.IDPhongBan into ulist
                       from b in ulist.DefaultIfEmpty()
                       join c in _db.ViTriLViecs on a.IDViTri equals c.IDViTri into uist
                       from c in uist.DefaultIfEmpty()
                       join d in _db.PhanXuongs on a.IDPhanXuong equals d.IDPhanXuong into ust
                       from d in ust.DefaultIfEmpty()
                       join e in _db.ToLVs on a.IDToLV equals e.IDTo into us1t
                       from e in us1t.DefaultIfEmpty()
                       join f in _db.Cas on a.IDCa equals f.IDCa into us2t
                       from f in us2t.DefaultIfEmpty()
                       join g in _db.NhomLVs on a.IDNhomLV equals g.IDNhom into us3t
                       from g in us3t.DefaultIfEmpty()
                       select new DataNhanVienAPI
                       {
                           ID =a.ID,
                           manv =a.MaNV,
                           hoten =a.HoTen,
                           ngaysinh =a.NgaySinh.ToString(),
                           diachi =a.DiaChi,
                           ngayvaolam =a.NgayVaoLam.ToString(),
                           phongban =b.TenPhongBan,
                           tinhtranglamviec =(int)a.IDTinhTrangLV,
                           sodienthoai =a.DienThoai,
                           idphongban =(int)a.IDPhongBan,
                           vitri =c.TenViTri,
                           cmnd =a.CMND,
                           //tenphanxuong = d.TenPhanXuong,
                           tenphanxuong = (a.IDPhanXuong ==null||a.IDPhanXuong ==0)?String.Empty:d.TenPhanXuong,
                           //tenkip = f.TenCa,
                           tenkip =(a.IDCa ==null||a.IDCa ==0)?String.Empty:f.TenCa,
                           //tento = e.TenTo,
                           tento =(a.IDToLV ==null||a.IDToLV ==0)?String.Empty:e.TenTo,
                           //tennhom = g.TenNhom,
                           tennhom =(a.IDNhomLV ==null||a.IDNhomLV ==0)?String.Empty:g.TenNhom,
                           Mavitri =a.MaViTri,
                           TT_BGD = (int?)a.TT_BGD ?? default,
                           ImagePath = a.ImagePath,
                       });

            if (!String.IsNullOrEmpty(IDPhongBan.ToString()))
                res = res.Where(x => x.idphongban == IDPhongBan);
            //if (!String.IsNullOrEmpty(MaNV))
            //{
            //    res = res.Where(x => x.manv == MaNV);
            //}
            if (!String.IsNullOrEmpty(IDMaVT))
                res = res.Where(x => x.Mavitri == IDMaVT);
            if (page == null) page = 1;
            int pageSize = 100;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public int? ToNullableIntTTBGD(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }
        public ActionResult Edit(int id)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.EDIT).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            var res = (from a in _db.NhanVienAPIs.Where(x => x.ID == id)
                       select new NhanVienAPIUpdate
                       {
                           IDNV = a.ID,
                           MaNV = a.MaNV,
                           HoTen = a.HoTen,
                           ImagePath = a.ImagePath,
                           IDPhongBan =(int)a.IDPhongBan,
                           MaViTri =a.MaViTri,
                           TT_BGD = (int?)a.TT_BGD ?? default
                       }).ToList();
            NhanVienAPIUpdate DO = new NhanVienAPIUpdate();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.HoTen = a.HoTen;
                    DO.MaNV = a.MaNV;
                    DO.IDNV = a.IDNV;
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.MaViTri = a.MaViTri;
                    DO.ImagePath = a.ImagePath;
                    DO.TT_BGD = a.TT_BGD;
                }

            }
            else
            {
                HttpNotFound();
            }
            return PartialView(DO);

        }
        [HttpPost]
        public ActionResult Edit(NhanVienAPIUpdate _DO)
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
                string FileName = _DO.ImageFile != null ? _DO.MaNV : "";
                //To Get File Extension  
                string FileExtension = _DO.ImageFile != null ? Path.GetExtension(_DO.ImageFile.FileName) : "";


                ////Add Current Date To Attached File Name  
                if (_DO.ImageFile != null)
                {
                    FileName = FileName.Trim() + FileExtension;
                    _DO.ImageFile.SaveAs(path + FileName);
                    _DO.ImagePath = "~/Images/" + FileName;
                }
                
                //if (!string.IsNullOrEmpty(_DO.TT_BGD.ToString()) && _DO.TT_BGD != 0)
                //{
                //    var b = _db.NhanVien_update_TT(_DO.MaNV, _DO.ImagePath, ToNullableIntTTBGD(_DO.TT_BGD.ToString()));
                //}
                var b = _db.NhanVien_update_TT(_DO.MaNV, _DO.ImagePath, ToNullableIntTTBGD(_DO.TT_BGD.ToString()));
                //var a = _db.NhanVienPVSX_update(_DO.IDNV, _DO.MaNV, _DO.HoTen, _DO.IDPhongBan, _DO.ImagePath, _DO.TT_BGD);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi chỉnh sửa: " + e.Message + "');</script>";
            }
            //return View();
            return RedirectToAction("Index", "Employees_API", new { @IDPhongBan = _DO.IDPhongBan, @IDMaVT =_DO.MaViTri });
        }
        List<SoDoToChuc.Models.Employees_API.data> GetAPI()
        {
            string link = ConfigurationManager.AppSettings["LinkAPI"];
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                var json = webClient.DownloadString(link);
                //Getdata(json);
                //Json khong dung chuan
                //=>Nen cat chuoi, cat chuoi the nay cu chuoi qua
                json = json.Remove(0, 36);
                json = json.Replace("}]}", "}]");
                var list = JsonConvert.DeserializeObject<List<SoDoToChuc.Models.Employees_API.data>>(json);
                return list.ToList();
            }

        }
        public string Getdata(string Content)
        {
            string pattern = "[^data$]]";
            Regex Title = new Regex(pattern);
            Match m = Title.Match(Content);
            if (m.Success)
                return m.ToString();
            return "";
        }
        public int GetIDPhongBan(string TenPB)
        {
            var model = _db.PhongBans.Where(x => x.TenPhongBan == TenPB).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDPhongBan;
        }
        public int GetIDViTri(string TenViTri)
        {
            var model = _db.ViTriLViecs.Where(x => x.TenViTri == TenViTri).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDViTri;
        }

        public int GetIDPhanXuong(string TenPX,int PBID)
        {
            var model = _db.PhanXuongs.Where(x => x.TenPhanXuong == TenPX && x.PhongBanID ==PBID).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDPhanXuong;
        }
        public int GetIDToLV(string TenTo, int PBID)
        {
            var model = _db.ToLVs.Where(x => x.TenTo == TenTo && x.PhongBanID == PBID).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDTo;
        }
        public int GetIDToLVPX(string TenTo, int PBID, int? PXID)
        {
            var model = _db.ToLVs.Where(x => x.TenTo == TenTo && x.PhongBanID == PBID && x.PhanXuongID == PXID).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDTo;
        }
        public int GetIDLoai(string TenLoai)
        {
            var model = _db.LoaiKHs.Where(x => x.TenLoaiKH == TenLoai ).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDLoai;
        }
        public int GetIDMaViTri(string mavitri)
        {
            var model = _db.MaViTris.Where(x => x.TenMaViTri == mavitri).FirstOrDefault();
            if (model == null)
                return 0;
            return (int)model.IDLoai;
        }
        public int GetNhomLV(string TenNhom, int PBID)
        {
            var model = _db.NhomLVs.Where(x => x.TenNhom == TenNhom && x.IDPhongBan == PBID).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDNhom;
        }
        public int GetCaKip(string TenCa)
        {
            var model = _db.Cas.Where(x => x.TenCa == TenCa ).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDCa;
        }
        public int? ToNullableInt(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }
        public int? ToNullzero(string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return 0;
        }

        public string convertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public ActionResult Sync()
        {
            string MaNV, sMaNV;
            int IDViTri, IDPhongBan;
            int dtc = 0, itc = 0;
            string msg = "";
            if (EPORTAL.Models.MyAuthentication.Username != null)
            {
                var UserMaNV = EPORTAL.Models.MyAuthentication.Username;
                int? IDquyen = EPORTAL.Models.MyAuthentication.IDQuyenHT;
                List<NhanVienAPI> LNV = _db.NhanVienAPIs.ToList();
                if (UserMaNV =="HPDQ12806"|| IDquyen ==1 )
                {
                    List<SoDoToChuc.Models.Employees_API.data> listNV = GetAPI();
                    if (listNV.Count > 0)
                    {
                        foreach (var item in listNV)
                        {
                            if (item.manv != null)
                            {
                                MaNV = item.manv;
                                sMaNV = MaNV.Substring(0, 4);

                                var rsnv = LNV.Where(x => x.MaNV == MaNV).SingleOrDefault();
                                if (rsnv == null)
                                {
                                    if (sMaNV == "HPDQ")
                                    {
                                        ObjectParameter IDPhongBanout = new ObjectParameter("IDPhongBan", typeof(int));
                                        ObjectParameter IDViTriout = new ObjectParameter("IDViTri", typeof(int));
                                        IDPhongBan = GetIDPhongBan(item.phongban);
                                        if (IDPhongBan == 0)
                                        {
                                            _db.PhongBan_insert_API(item.phongban);
                                            IDPhongBan = GetIDPhongBan(item.phongban);
                                        }
                                        IDViTri = GetIDViTri(item.vitri);
                                        if (IDViTri == 0)
                                        {
                                            _db.ViTri_insert_API(item.vitri);
                                            IDViTri = GetIDViTri(item.vitri);
                                        }
                                        _db.NhanVien_insert_API(MaNV, item.hoten, DateTime.ParseExact(item.ngaysinh, "dd/MM/yyyy", CultureInfo.InvariantCulture), item.diachi, item.sodienthoai, DateTime.ParseExact(item.ngayvaolam, "dd/MM/yyyy", CultureInfo.InvariantCulture), IDPhongBan, item.tinhtranglamviec, IDViTri, item.cmnd);
                                        itc++;
                                    }
                                }
                                else
                                {
                                    if (sMaNV == "HPDQ")
                                    {
                                        ObjectParameter IDPhongBanout = new ObjectParameter("IDPhongBan", typeof(int));
                                        ObjectParameter IDViTriout = new ObjectParameter("IDViTri", typeof(int));
                                        IDPhongBan = GetIDPhongBan(item.phongban);
                                        if (IDPhongBan == 0)
                                        {
                                            _db.PhongBan_insert_API(item.phongban);
                                            IDPhongBan = GetIDPhongBan(item.phongban);
                                        }
                                        IDViTri = GetIDViTri(item.vitri);
                                        if (IDViTri == 0)
                                        {
                                            _db.ViTri_insert_API(item.vitri);
                                            IDViTri = GetIDViTri(item.vitri);
                                        }
                                        if (rsnv.IDViTri !=IDViTri || rsnv.IDPhongBan != IDPhongBan ||rsnv.IDTinhTrangLV != item.tinhtranglamviec||rsnv.DiaChi !=item.diachi )
                                        {
                                            _db.NhanVien_update_API(MaNV, item.hoten, DateTime.ParseExact(item.ngaysinh, "dd/MM/yyyy", CultureInfo.InvariantCulture), item.diachi, item.sodienthoai, DateTime.ParseExact(item.ngayvaolam, "dd/MM/yyyy", CultureInfo.InvariantCulture), IDPhongBan, item.tinhtranglamviec, IDViTri, item.cmnd);
                                            dtc++;
                                        }

                                    }
                                }
                            }

                        }
                        if (dtc != 0)
                        {
                            msg = "Cập nhật thông tin được " + dtc + " nhân viên";
                        }
                        if (itc != 0)
                        {
                            msg = "Update thông tin được " + itc + " nhân viên";
                        }
                        TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";
                        //return RedirectToAction("Index", "Employees_API");
                    }
                }
            }
            else
            {
                return RedirectToAction("", "Login");
            }
            return RedirectToAction("Index", "Employees_API");

        }
        public ActionResult ImportExcel()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult ImportExcel(SoDoToChuc.Models.Employees_API _DO)
        {
            var check = dbP.A_CheckQuyen(IDQuyenHT, controll, A_Constants.ADD).First();
            if (check == 0)
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            List<NhanVienAPI> LNV = _db.NhanVienAPIs.ToList();
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
                    DataTable dtbp = result.Tables[1];
                    //DataTable dtto = result.Tables[2];
                    //DataTable dtnhom = result.Tables[3];
                    //DataTable dtmavitri = result.Tables[4];
                   
                    reader.Close();
                    int dtc = 0, dtrung = 0,nvsai =0;
                    int IDPB,IDPX,IDTo;

                    //Import Phân Xưởng
                    if (dtbp.Rows.Count > 0)
                    {
                        try
                        {
                            string tenbp="", tenpx="";
                            int idbp=0, idpx=0,slnvpx=0,slttpx=0;
                            for (int i = 4; i < dtbp.Rows.Count; i++)
                            {
                                if (dtbp.Rows[i] != null)
                                {
                                    //Insert phòng ban 
                                    if(!string.IsNullOrEmpty(dtbp.Rows[i][1].ToString().Trim()) )
                                    {
                                        tenbp = dtbp.Rows[i][1].ToString().Trim();
                                        idbp = GetIDPhongBan(tenbp);
                                        if (idbp == 0)
                                        {
                                            _db.PhongBan_insert_API(tenbp);
                                            idbp = GetIDPhongBan(tenbp);
                                        }
                                        else if(idbp!=0&& !string.IsNullOrEmpty(dtbp.Rows[i][0].ToString()))
                                        {
                                            tenpx = "";
                                            var a = _db.PhongBan_update_Excel(idbp, tenbp, ToNullableInt(dtbp.Rows[i][6].ToString()), ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullableInt(dtbp.Rows[i][12].ToString()), ToNullableInt(dtbp.Rows[i][9].ToString()), ToNullableInt(dtbp.Rows[i][13].ToString()), ToNullableInt(dtbp.Rows[i][10].ToString()), ToNullableInt(dtbp.Rows[i][14].ToString()), ToNullableInt(dtbp.Rows[i][11].ToString()), ToNullableInt(dtbp.Rows[i][15].ToString()));
                                        }
                                    }
                                    //else if(!string.IsNullOrEmpty(dtbp.Rows[i][1].ToString()) && dtbp.Rows[i][1].ToString() == tenbp && !string.IsNullOrEmpty(dtbp.Rows[i][0].ToString())) 
                                    //{
                                    //    //update bộ phận
                                    //    if(idbp != 0)
                                    //    {
                                    //        var a = _db.PhongBan_update_Excel(idbp, tenbp, ToNullableInt(dtbp.Rows[i][5].ToString()), ToNullableInt(dtbp.Rows[i][6].ToString()), ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][11].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullableInt(dtbp.Rows[i][12].ToString()), ToNullableInt(dtbp.Rows[i][9].ToString()), ToNullableInt(dtbp.Rows[i][13].ToString()), ToNullableInt(dtbp.Rows[i][10].ToString()), ToNullableInt(dtbp.Rows[i][14].ToString()));
                                    //    }
                                    //    dtc++;
                                    //}
                                    //insert phân xưởng
                                    if (!string.IsNullOrEmpty(dtbp.Rows[i][2].ToString().Trim()) && tenpx != dtbp.Rows[i][2].ToString().Trim()) //có phân xưởng
                                    {
                                        //insert data phan xuong
                                        tenpx = dtbp.Rows[i][2].ToString().Trim();
                                        idpx = GetIDPhanXuong(tenpx, idbp);
                                       
                                        int? slktv = ToNullzero(dtbp.Rows[i][9].ToString()) + ToNullzero(dtbp.Rows[i][13].ToString());
                                        if (idpx == 0)
                                        {
                                            var a = _db.PhanXuong_insert_Excel(tenpx, idbp, ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullableInt(dtbp.Rows[i][12].ToString()), slktv, ToNullableInt(dtbp.Rows[i][15].ToString()), ToNullableInt(dtbp.Rows[i][11].ToString()), ToNullableInt(dtbp.Rows[i][17].ToString()));
                                            idpx = GetIDPhanXuong(tenpx, idbp);
                                            dtc++;
                                        }
                                        else
                                        {
                                            var a = _db.PhanXuong_update_Excel(idpx,tenpx, idbp, ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullableInt(dtbp.Rows[i][12].ToString()), slktv, ToNullableInt(dtbp.Rows[i][15].ToString()), ToNullableInt(dtbp.Rows[i][11].ToString()), ToNullableInt(dtbp.Rows[i][17].ToString()));
                                            dtc++;                                        
                                        }
                                        //else if (idpx != 0)
                                        //{
                                        //    //var a = _db.PhanXuong_update_Excel(IDPX,dtpx.Rows[i][1].ToString(), GetPBID, ToNullableInt(dtpx.Rows[i][3].ToString()), ToNullableInt(dtpx.Rows[i][4].ToString()), ToNullableInt(dtpx.Rows[i][5].ToString()), ToNullableInt(dtpx.Rows[i][6].ToString()), ToNullableInt(dtpx.Rows[i][7].ToString()));
                                        //    dtc++;
                                        //}
                                        //else
                                        //{
                                        //    dtrung++;
                                        //}
                                    }
                                    //else if (!string.IsNullOrEmpty(dtbp.Rows[i][2].ToString()) && dtbp.Rows[i][2].ToString() == "Không") // không có phân xưởng
                                    //{
                                    //    tenpx = "Không";
                                    //}
                                    //insert tổ làm việc
                                    if(!string.IsNullOrEmpty(dtbp.Rows[i][3].ToString().Trim()))
                                    {
                                        var tento = dtbp.Rows[i][3].ToString().Trim();
                                        int GetIDTo = GetIDToLV(tento, idbp);
                                        int IDNhomPT = GetNhomLV(dtbp.Rows[i][5].ToString(), idbp);
                                        
                                        int? vl =  ToNullzero(dtbp.Rows[i][14].ToString()) +  ToNullzero(dtbp.Rows[i][15].ToString()) + ToNullzero(dtbp.Rows[i][10].ToString())+ ToNullzero(dtbp.Rows[i][11].ToString());
                                        if(!string.IsNullOrEmpty(dtbp.Rows[i][2].ToString().Trim()))//tổ thuộc px
                                        {
                                            GetIDTo = GetIDToLVPX(tento, idbp, idpx);
                                            if (GetIDTo == 0)
                                            {
                                                var a = _db.ToLV_insert(tento, idpx, idbp, vl, null);
                                                dtc++;
                                            }
                                            else if (GetIDTo != 0)
                                            {
                                                var a = _db.ToLV_update(tento, idpx, idbp, vl, null, GetIDTo);
                                                dtc++;
                                            }
                                        }
                                        else    //tổ không thuộc px
                                        {
                                            if(IDNhomPT != 0)//tổ thuộc nhóm
                                            {
                                                if (GetIDTo == 0)
                                                {
                                                    var a = _db.ToLV_insert(tento, null, idbp, vl,IDNhomPT);
                                                    dtc++;
                                                }
                                                else if (GetIDTo != 0)
                                                {
                                                    var a = _db.ToLV_update(tento, null, idbp, vl, IDNhomPT, GetIDTo);
                                                    dtc++;
                                                }
                                            }
                                            else if(IDNhomPT == 0)//tổ không thuộc nhóm,px
                                            {
                                                if (GetIDTo == 0)
                                                {
                                                    var a = _db.ToLV_insert(tento, null, idbp, vl, null);
                                                    dtc++;
                                                }
                                                else if (GetIDTo != 0)
                                                {
                                                    var a = _db.ToLV_update(tento, null, idbp, vl, null, GetIDTo);
                                                    dtc++;
                                                }
                                            }
                                            
                                        }
                                        
                                    }
                                    //else if (tenpx !="Không" && !string.IsNullOrEmpty(dtbp.Rows[i][4].ToString().Trim()))
                                    //{
                                    //    var tento = dtbp.Rows[i][4].ToString();
                                    //    int GetIDTo = GetIDToLV(tento, idbp);
                                    //    int? vl = ToNullzero(dtbp.Rows[i][13].ToString()) + ToNullzero(dtbp.Rows[i][14].ToString()) + ToNullzero(dtbp.Rows[i][9].ToString()) + ToNullzero(dtbp.Rows[i][10].ToString());
                                    //    if (GetIDTo == 0)
                                    //    {
                                    //        var a = _db.ToLV_insert(tento, idpx, idbp, vl, null);
                                    //        dtc++;
                                    //    }
                                    //    else if (GetIDTo != 0)
                                    //    {
                                    //        var a = _db.ToLV_update(tento, idpx, idbp, vl, null, GetIDTo);
                                    //        dtc++;
                                    //    }
                                    //}
                                    //insert nhóm làm việc
                                    if(!string.IsNullOrEmpty(dtbp.Rows[i][4].ToString()))
                                    {
                                        
                                        int nhomID = GetNhomLV(dtbp.Rows[i][4].ToString(), idbp);
                                        int nhomIDPT = GetNhomLV(dtbp.Rows[i][5].ToString(), idbp);
                                        //int? slktvn= ToNullzero(dtbp.Rows[i][8].ToString()) + ToNullzero(dtbp.Rows[i][9].ToString());
                                        int? sltttp = ToNullzero(dtbp.Rows[i][10].ToString()) + ToNullzero(dtbp.Rows[i][14].ToString());
                                        if(nhomIDPT !=0)
                                        {
                                            if (nhomID == 0)
                                            {
                                                var a = _db.Nhom_insert_Excel(dtbp.Rows[i][4].ToString(), idbp, ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullzero(dtbp.Rows[i][12].ToString()), ToNullzero(dtbp.Rows[i][9].ToString()), ToNullzero(dtbp.Rows[i][11].ToString()), ToNullzero(dtbp.Rows[i][13].ToString()), ToNullzero(dtbp.Rows[i][15].ToString()), sltttp,nhomIDPT);
                                                dtc++;
                                            }
                                            else if (nhomID != 0)
                                            {
                                                var a = _db.Nhom_update_Excel(nhomID, dtbp.Rows[i][4].ToString(), idbp, ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullzero(dtbp.Rows[i][12].ToString()), ToNullzero(dtbp.Rows[i][9].ToString()), ToNullzero(dtbp.Rows[i][11].ToString()), ToNullzero(dtbp.Rows[i][13].ToString()), ToNullzero(dtbp.Rows[i][15].ToString()), sltttp,nhomIDPT);
                                                dtc++;
                                            }
                                        }
                                        else
                                        {
                                            if (nhomID == 0)
                                            {
                                                var a = _db.Nhom_insert_Excel(dtbp.Rows[i][4].ToString(), idbp, ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullzero(dtbp.Rows[i][12].ToString()), ToNullzero(dtbp.Rows[i][9].ToString()), ToNullzero(dtbp.Rows[i][11].ToString()), ToNullzero(dtbp.Rows[i][13].ToString()), ToNullzero(dtbp.Rows[i][15].ToString()), sltttp, null);
                                                dtc++;
                                            }
                                            else if (nhomID != 0)
                                            {
                                                var a = _db.Nhom_update_Excel(nhomID, dtbp.Rows[i][4].ToString(), idbp, ToNullableInt(dtbp.Rows[i][7].ToString()), ToNullableInt(dtbp.Rows[i][8].ToString()), ToNullzero(dtbp.Rows[i][12].ToString()), ToNullzero(dtbp.Rows[i][9].ToString()), ToNullzero(dtbp.Rows[i][11].ToString()), ToNullzero(dtbp.Rows[i][13].ToString()), ToNullzero(dtbp.Rows[i][15].ToString()), sltttp, null);
                                                dtc++;
                                            }
                                        }
                                        
                                    }


                                    //int GetPBID = GetIDPhongBan(dtpx.Rows[i][2].ToString());
                                    //IDPX = GetIDPhanXuong(dtpx.Rows[i][1].ToString(),GetPBID);

                                    //if (IDPX ==0  )
                                    //{
                                    //    //var a = _db.PhanXuong_insert_Excel(dtpx.Rows[i][1].ToString(), GetPBID, ToNullableInt(dtpx.Rows[i][3].ToString()), ToNullableInt(dtpx.Rows[i][4].ToString()), ToNullableInt(dtpx.Rows[i][5].ToString()), ToNullableInt(dtpx.Rows[i][6].ToString()), ToNullableInt(dtpx.Rows[i][7].ToString()));
                                    //    dtc++;
                                    //}
                                    //else if(IDPX !=0)
                                    //{
                                    //    //var a = _db.PhanXuong_update_Excel(IDPX,dtpx.Rows[i][1].ToString(), GetPBID, ToNullableInt(dtpx.Rows[i][3].ToString()), ToNullableInt(dtpx.Rows[i][4].ToString()), ToNullableInt(dtpx.Rows[i][5].ToString()), ToNullableInt(dtpx.Rows[i][6].ToString()), ToNullableInt(dtpx.Rows[i][7].ToString()));
                                    //    dtc++;
                                    //}    
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

                    //Import Nhân viên
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {

                            for (int i = 1; i < dt.Rows.Count; i++)
                            {

                                if (dt.Rows[i] != null)
                                {
                                    int GetPBID = GetIDPhongBan(dt.Rows[i][7].ToString());
                                    
                                    int? GetIDPX = GetIDPhanXuong(dt.Rows[i][6].ToString(), GetPBID);
                                    if (GetIDPX == 0) GetIDPX = null;

                                    int? GetIDTo = GetIDPX ==null?GetIDToLV(dt.Rows[i][4].ToString(), GetPBID):GetIDToLVPX(dt.Rows[i][4].ToString(), GetPBID,GetIDPX);
                                    if (GetIDTo == 0) GetIDTo = null;
                                    int GetIDCa = GetCaKip(dt.Rows[i][5].ToString());

                                    int? GetIDNhomLV = GetNhomLV(dt.Rows[i][9].ToString(), GetPBID);
                                    if (GetIDNhomLV == 0) GetIDNhomLV = null;

                                    int GetIDLoais = GetIDMaViTri(dt.Rows[i][8].ToString());

                                    var rsnv = LNV.Where(x => x.MaNV == dt.Rows[i][1].ToString()).SingleOrDefault();
                                    if ( rsnv !=null)
                                    {
                                        if(GetPBID != 0)
                                        {
                                            var a = _db.NhanVien_update_Excel(dt.Rows[i][1].ToString(),  GetIDTo, GetIDCa, GetIDPX, dt.Rows[i][8].ToString(),GetIDNhomLV,GetIDLoais );
                                            dtc++;
                                        }
                                    }
                                    else
                                    {
                                        nvsai++;
                                    }    
                                    //else if (IDPX != 0)
                                    //{
                                    //    var a = _db.PhanXuong_update_Excel(IDPX, dtpx.Rows[i][1].ToString(), GetPBID, ToNullableInt(dtpx.Rows[i][3].ToString()), ToNullableInt(dtpx.Rows[i][4].ToString()), ToNullableInt(dtpx.Rows[i][5].ToString()), ToNullableInt(dtpx.Rows[i][6].ToString()), ToNullableInt(dtpx.Rows[i][7].ToString()));
                                    //    dtc++;
                                    //}
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

            return RedirectToAction("Index", "Employees_API");
        }
        //public ActionResult DownloadExcel()
        //{
        //     return PartialView();
        //}
        //[HttpPost]
        public void DownloadExcel()
        {
            //var stream = new MemoryStream();
            string filePath = Server.MapPath("~/App_Data/SDTC_BieuMau.xlsx");
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                // Clear Rsponse reference  
                Response.Clear();
                // Add header by specifying file name  
                Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);
                // Add header for content length  
                Response.AddHeader("Content-Length", file.Length.ToString());
                // Specify content type  
                Response.ContentType = "text/plain";
                // Clearing flush  
                Response.Flush();
                // Transimiting file  
                Response.TransmitFile(file.FullName);
                Response.End();
            }
            //return File(stream, "application/octet-stream", excelName);
        }
    }
}