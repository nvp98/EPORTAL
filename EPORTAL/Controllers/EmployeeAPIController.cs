using EPORTAL.Models;
using EPORTAL.ModelsView360;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Controllers
{
    public class EmployeeAPIController : Controller
    {
        // GET: EmployeeAPI
        EPORTALEntities _db = new EPORTALEntities();
        public ActionResult Index()
        {

            return View();
        }
        List<Employees_API.data> GetAPI()
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
                var list = JsonConvert.DeserializeObject<List<Employees_API.data>>(json);
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
            var model = _db.Vitris.Where(x => x.TenViTri == TenViTri).FirstOrDefault();
            if (model == null)
                return 0;
            return model.IDViTri;
        }
        public static string convertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public ActionResult Sync()
        {
            string MaNV, sMaNV;
            int IDViTri, IDPhongBan;
            int dtc = 0;
            string msg = "";
            if (User.Identity.IsAuthenticated)
            {

                List<NhanVien> LNV = _db.NhanViens.ToList();
                if (MyAuthentication.Username == "HPDQ12806" || MyAuthentication.Username == "HPDQ18461")
                {
                    List<Employees_API.data> listNV = GetAPI();
                    if (listNV.Count > 0)
                    {
                        foreach (var item in listNV)
                        {
                            if (item.manv != null)
                            {
                                MaNV = item.manv;
                                sMaNV = MaNV.Substring(0, 4);

                                var rsnv = LNV.Where(x => x.MaNV == MaNV).FirstOrDefault();
                                if (rsnv == null)
                                {
                                    if (sMaNV == "HPDQ" && MaNV.Length==9)
                                    {
                                        ObjectParameter IDPhongBanout = new ObjectParameter("IDPhongBan", typeof(int));
                                        ObjectParameter IDViTriout = new ObjectParameter("IDViTri", typeof(int));
                                        IDPhongBan = GetIDPhongBan(item.phongban);
                                        if (IDPhongBan == 0)
                                        {
                                            _db.PhongBan_insert(IDPhongBanout, item.phongban);
                                            IDPhongBan = Convert.ToInt32(IDPhongBanout.Value);
                                        }
                                        IDViTri = GetIDViTri(item.vitri);
                                        if (IDViTri == 0)
                                        {
                                            _db.Vitri_insert(IDViTriout, item.vitri);
                                            IDViTri = Convert.ToInt32(IDViTriout.Value);
                                        }
                                        _db.Nhanvien_insert_API(MaNV, item.hoten, convertToUnSign(item.hoten), item.diachi, item.sodienthoai, DateTime.ParseExact(item.ngayvaolam, "dd/MM/yyyy", CultureInfo.InvariantCulture), IDPhongBan, item.tinhtranglamviec, IDViTri);
                                        _db.NhanVienCCCD_Update(MaNV, item.cmnd);
                                    }
                                }
                                else
                                {
                                    if (sMaNV == "HPDQ" && MaNV.Length == 9)
                                    {
                                        ObjectParameter IDPhongBanout = new ObjectParameter("IDPhongBan", typeof(int));
                                        ObjectParameter IDViTriout = new ObjectParameter("IDViTri", typeof(int));
                                        IDPhongBan = GetIDPhongBan(item.phongban);
                                        if (IDPhongBan == 0)
                                        {
                                            _db.PhongBan_insert(IDPhongBanout, item.phongban);
                                            IDPhongBan = Convert.ToInt32(IDPhongBanout.Value);
                                        }
                                        IDViTri = GetIDViTri(item.vitri);
                                        if (IDViTri == 0)
                                        {
                                            _db.Vitri_insert(IDViTriout, item.vitri);
                                            IDViTri = Convert.ToInt32(IDViTriout.Value);
                                        }
                                        if (rsnv.IDPhongBan != IDPhongBan || rsnv.IDViTri != IDViTri || rsnv.IDTinhTrangLV!=item.tinhtranglamviec)
                                        {
                                            _db.Nhanvien_update_API(MaNV, item.diachi, item.sodienthoai, IDPhongBan, item.tinhtranglamviec, IDViTri);
                                            dtc++;
                                        }
                                        if(item.tinhtranglamviec==1)
                                        {
                                            var model = _db.AuthorizationContractors.Where(x => x.IDNhanVien == rsnv.ID).FirstOrDefault();
                                            if (model == null)
                                            {
                                                _db.AuthorizationContractors_insert(rsnv.ID, 4, null);
                                            }
                                            var mkd=_db.KD_KyDuyet.Where(x=>x.IDNhanVien==rsnv.ID).FirstOrDefault();
                                            if(mkd == null)
                                            {
                                                _db.KD_KyDuyet_insert(rsnv.ID, "",4);
                                            }    
                                        }
                                        _db.NhanVienCCCD_Update(MaNV, item.cmnd);

                                    }
                                }
                            }

                        }
                        if (dtc != 0)
                        {
                            msg = "Cập nhật thông tin được " + dtc + " nhân viên";
                        }
                        TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";
                        return RedirectToAction("Index", "Account");
                    }
                }
            }
            else
            {
                return RedirectToAction("", "Login");
            }
            return View();
        }
    }
}