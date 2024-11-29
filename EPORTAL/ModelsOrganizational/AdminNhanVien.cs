using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class AdminNhanVien
    {
        public int IDNV { get; set; }
        public string MaNV { get; set; }
        public string TenNV { get; set; }
        public string Email { get; set; }
        public DateTime NgaySinh { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }

        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public int? PhanXuongID { get; set; }
        public string TenPhanXuong { get; set; }
        public int ChucVuID { get; set; }
        public string ChucVu { get; set; }
        public int CapBac { get; set; }
        public string TenCapBac { get; set; }
        public string ImagePath { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }

    }
}