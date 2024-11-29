using EPORTAL.ModelsDanhBaDoiTac;
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

namespace EPORTAL.Areas.Publish.Controllers
{
    public class EmployeesAPIController : Controller
    {
        // GET: Publish/EmployeesAPI
        DanhBaDoiTacEntities _db = new DanhBaDoiTacEntities();
        public ActionResult Index()
        {

            return View();
        }
        List<EmployeesAPI.data> GetAPI()
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
                var list = JsonConvert.DeserializeObject<List<EmployeesAPI.data>>(json);
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
            var model = _db.DB_PhongBan.Where(x => x.TenPhongBan == TenPB).SingleOrDefault();
            if (model == null)
                return 0;
            return model.IDPhongBan;
        }
        public int GetIDViTri(string TenViTri)
        {
            var model = _db.DB_Vitri.Where(x => x.TenViTri == TenViTri).SingleOrDefault();
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
            //if (Session["S_TK"] != null)
            //{
            //    NhanVien users = (NhanVien)Session["S_TK"];
            List<DB_NhanVien> LNV = _db.DB_NhanVien.ToList();
            //    if (users.MaNV == "HPDQ12806")
            //    {
            List<EmployeesAPI.data> listNV = GetAPI();
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
                                    _db.DB_PhongBan_insert(IDPhongBanout, item.phongban);
                                    IDPhongBan = Convert.ToInt32(IDPhongBanout.Value);
                                }
                                IDViTri = GetIDViTri(item.vitri);
                                if (IDViTri == 0)
                                {
                                    _db.DB_Vitri_insert(IDViTriout, item.vitri);
                                    IDViTri = Convert.ToInt32(IDViTriout.Value);
                                }
                                _db.DB_Nhanvien_insert_API(MaNV, item.hoten, convertToUnSign(item.hoten), item.diachi, item.sodienthoai, DateTime.ParseExact(item.ngayvaolam, "dd/MM/yyyy", CultureInfo.InvariantCulture), IDPhongBan, item.tinhtranglamviec, IDViTri);

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
                                    _db.DB_PhongBan_insert(IDPhongBanout, item.phongban);
                                    IDPhongBan = Convert.ToInt32(IDPhongBanout.Value);
                                }
                                IDViTri = GetIDViTri(item.vitri);
                                if (IDViTri == 0)
                                {
                                    _db.DB_Vitri_insert(IDViTriout, item.vitri);
                                    IDViTri = Convert.ToInt32(IDViTriout.Value);
                                }
                                if (rsnv.IDPhongBan != IDPhongBan || rsnv.IDViTri != IDViTri)
                                {
                                    _db.DB_Nhanvien_update_API(MaNV, item.diachi, item.sodienthoai, IDPhongBan, item.tinhtranglamviec, IDViTri);
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
                TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";
                return RedirectToAction("Index", "EmployeeAPI");
            }
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("", "Login");
            //}
            return View();
        }
    }
}