using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class Employees_API
    {
        public string result { get; set; }
        public string content { get; set; }
        public data[] Employees { get; set; }
        public class data
        {
            public string manv { get; set; }
            public string hoten { get; set; }
            public string cmnd { get; set; }
            public string ngaysinh { get; set; }
            public string diachi { get; set; }
            public string sodienthoai { get; set; }
            public string email { get; set; }
            public string ngayvaolam { get; set; }
            public int tinhtranglamviec { get; set; }
            public string ngaynghiviec { get; set; }
            public string phongban { get; set; }
            public string vitri { get; set; }
        }

    }
    public class DataImportExcel
    {
        public int IDPhanXuong { get; set; }
        public int IDTo { get; set; }
        public int IDNhomLV { get; set; }
        public int IDCa { get; set; }
        public int CapBac { get; set; }

    }
    public class DataNhanVienAPI
    {
        public int ID { get; set; }
        public string manv { get; set; }
        public string hoten { get; set; }
        public string cmnd { get; set; }
        public string ngaysinh { get; set; }
        public string diachi { get; set; }
        public string sodienthoai { get; set; }
        public string email { get; set; }
        public string ngayvaolam { get; set; }
        public int tinhtranglamviec { get; set; }
        public string ngaynghiviec { get; set; }
        public string phongban { get; set; }
        public int idphongban { get; set; }
        public string vitri { get; set; }
        public int IDphanxuong { get; set; }
        public string tenphanxuong { get; set; }
        public int IDto { get; set; }
        public string tento { get; set; }
        public int IDkip { get; set; }
        public string tenkip { get; set; }
        public int IDnhomlv { get; set; }
        public string tennhom { get; set; }
        public string Mavitri { get; set; }
        public string ImagePath { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public int TT_BGD { get; set; }

    }
    public class NhanVienAPIUpdate
    {
        public int IDNV { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public int IDPhongBan { get; set; }
        public string MaViTri { get; set; }
        public string ImagePath { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public int TT_BGD { get; set; }

    }
}