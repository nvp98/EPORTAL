using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPORTAL.ModelsTagSign.TheXeCoDongVM
{
    public class DonDangKyKhoaTheViewModel
    {
        public int ID { get; set; }
        public string MaDon { get; set; }
        public string NoiDung { get; set; }
        public int? ID_NhaThau { get; set; }
        public string TenNhaThau { get; set; }
        public string BoPhanQuanLy { get; set; }
        public int? TinhTrang { get; set; }
        public DateTime? NgayTao { get; set; }
    }
    public class KTNT_ChiTietVM
    {
        public int ID { get; set; }
        public string MaDon { get; set; }

        public string TN_HoTen { get; set; }
        public string TN_CCCD_HoChieu { get; set; }

        public string TX_LoaiXeCoDong { get; set; }
        public string TX_BienKiemSoat { get; set; }

        public string PT_LoaiPhuongTien { get; set; }
        public string PT_BienKiemSoat { get; set; }

        public string GhiChu { get; set; }
        public DateTime? NgayTao { get; set; }
    }
    public class TaoDonDangKyViewModel
    {
        public int ID { get; set; }
        public string NoiDung { get; set; }
        public int? NhaThau_ID { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? VP1C_ID { get; set; }
        public List<KTNT_ChiTietVM> ChiTiet { get; set; }
    }
    public class SPResult
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public string MaDon { get; set; }
    }
}