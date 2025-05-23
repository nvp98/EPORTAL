//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPORTAL.ModelsOrganizational
{
    using System;
    using System.Collections.Generic;
    
    public partial class NhanVienAPI
    {
        public int ID { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public Nullable<System.DateTime> NgayVaoLam { get; set; }
        public Nullable<int> IDPhongBan { get; set; }
        public Nullable<int> IDTinhTrangLV { get; set; }
        public Nullable<int> IDViTri { get; set; }
        public string CMND { get; set; }
        public Nullable<int> IDPhanXuong { get; set; }
        public Nullable<int> IDToLV { get; set; }
        public Nullable<int> IDCa { get; set; }
        public Nullable<int> IDNhomLV { get; set; }
        public string MaViTri { get; set; }
        public Nullable<int> IDLoai { get; set; }
        public Nullable<int> TT_BGD { get; set; }
        public string ImagePath { get; set; }
    
        public virtual PhanXuong PhanXuong { get; set; }
        public virtual PhongBan PhongBan { get; set; }
        public virtual ToLV ToLV { get; set; }
        public virtual ViTri ViTri { get; set; }
        public virtual ViTriLViec ViTriLViec { get; set; }
    }
}
