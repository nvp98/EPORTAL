using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsView360.DTOs
{
    public class KhaiBaoTongHopViewModel
    {
        public KhaiBaoNhanVienDto NhanVien { get; set; }
        public KhaiBaoVoChongDto VoChong { get; set; }
        public List<KhaiBaoConCaiDto> DanhSachConCai { get; set; }
    }

    public class KhaiBaoNhanVienDto
    {
        public int KhaiBaoID { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public short? LoaiKhaiBao { get; set; }
        public bool DaXacNhan { get; set; }
        public DateTime? NgayKhaiBao { get; set; }
    }

    public class KhaiBaoVoChongDto
    {
        public int VoChongID { get; set; }
        public string MaNhanVien { get; set; }
        public string TenVoChong { get; set; }
        public int? NamSinhVoChong { get; set; }
    }

    public class KhaiBaoConCaiDto
    {
        public int ConCaiID { get; set; }
        public string MaNhanVien { get; set; }
        public string TenCon { get; set; }
        public int? NamSinhCon { get; set; }
        public string GiayKhaiSinh { get; set; }
    }
}