using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class AdminNhanVienPVSX
    {
        public int IDNV { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string NgaySinh { get; set; }
        public string DienThoai { get; set; }
        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public int IDViTri { get; set; }
        public string TenViTri { get; set; }
        public int ChucVuID { get; set; }
        public string ChucVu { get; set; }
        public int IDNhom { get; set; }
        public string TenNhom { get; set; }
        public int CapBac { get; set; }
        public string TenCapBac { get; set; }
        public string ImagePath { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public int TT_BGD { get; set; }
    }
    public class NhomList
    {
        public int IDNhom { get; set; }
        public string TenNhom { get; set; }
    }
    public class GetNVAPI
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public int IDViTri { get; set; }
        public string TenViTri { get; set; }
        public string CMND { get; set; }
    }
}