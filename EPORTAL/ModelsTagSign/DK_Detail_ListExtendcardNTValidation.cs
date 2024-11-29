using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class DK_Detail_ListExtendcardNTValidation
    {
        public int IDCTGH { get; set; }
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public DateTime NgaySinh { get; set; }
        public string HoKhau { get; set; }
        public int ChucVu { get; set; }
        public string SoDienThoai { get; set; }
        public string CapLai { get; set; }
        public string GiaHan { get; set; }
        public string ThongTinLuuTru { get; set; }
        public string DTTM { get; set; }
        public string RaVaoCang { get; set; }
        public DateTime ThoiHanThe { get; set; }
        public string KhuVucLamViec { get; set; }
        public string Cong { get; set; }
        public int NhomNT { get; set; }
        public string GhiChu { get; set; }
        public int IDGHT { get; set; }
        public string FileUpload { get; set; }
        public HttpPostedFileBase FilePDF { get; set; }
        public HttpPostedFileBase FileAnh { get; set; }
        public HttpPostedFileBase FileExcel { get; set; }
        public string Namefile { get; set; }
        public string[] Selected { get; set; }
        public string[] SelectedKV { get; set; }
        public bool IsChecked { get; set; }

        public string NoiDung { get; set; }
        public int NTID { get; set; }
        public int HDID { get; set; }
        public int PhongBanID { get; set; }
        public DateTime NgayDangKy { get; set; }
        public int NhanVienID { get; set; }
        public int TinhTrangID { get; set; }
        public HttpPostedFileBase File { get; set; }
        public DateTime? Begind { get; set; }
        public DateTime? Endd { get; set; }
        public int LoaiNT { get; set; }
        public string FileComplete { get; set; }
    }
}