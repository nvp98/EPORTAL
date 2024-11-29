using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class NT_CarteTemporaireValidation
    {
        public int IDTTT { get; set; }
        public string NoiDung { get; set; }
        public int NguoiTaoID { get; set; }
        public int NguoiDuyet { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayDuyet { get; set; }
        public int TinhTrang { get; set; }
        public string TenTT { get; set; }
        public int NTID { get; set; }
        public string TenNT { get; set; }
        public int PhongBanID { get; set; }
        public string TenPhongBan { get; set; }
        public string HangMuc { get; set; }
        public DateTime ThoiHan { get; set; }
        public string GhiChu { get; set; }
        public int? isNT { get; set; }
        public int? capduyet { get; set; }
        public int? kyTruoc { get; set; }
        public string FileHosoDK { set; get; }
    }
    public class NT_ListCarteTemporaireValidation
    {
        public int IDTTT { get; set; }
        public string NoiDung { get; set; }
        public int NguoiTaoID { get; set; }
        public string HoTenNT { get; set; }
        public DateTime NgayTao { get; set; }
        public int TinhTrang { get; set; }
        public int NTID { get; set; }
        public string TenNT { get; set; }
        public int PhongBanID { get; set; }
        public string TenPhongBan { get; set; }
        public string HangMuc { get; set; }
        public DateTime ThoiHan { get; set; }
        public string GhiChu { get; set; }
        public int? isNT { get; set; }
    }
}