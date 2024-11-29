using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsServey
{
    public class PartTogetherValidation
    {
        public int ID { get; set; }
        public Nullable<int> IDNV { get; set; }
        public string PhongBan { get; set; }
        public string HoTen { get; set; }
        public string MaNV { get; set; }
        public string HoTenNV { get; set; }
        public string TenNV { get; set; }
        public Nullable<int> IDESV { get; set; }
        public string Re { get; set; }
        public Nullable<bool> isCom { get; set; }
        public string Com { get; set; }
        public string NamSinh { get; set; }
        public string QuanHe { get; set; }
        public string Note { get; set; }
        public string DienThoai { get; set; }
        public int? GioiTinh { get; set; }
        public string GioiTinhStr { get; set; }
        public string CuLy { get; set; }
        public string MauAo { get; set; }
        public string Size { get; set; }
        public string DiaChi { get; set; }

        public int? IDNguoiThan { get; set; }
        public int? IDSV { get; set; }

        public int? IDCuLy { get; set; }
        public int? IDMau { get; set; }

        public int? IDSize { get; set; }
        public int? CungCTy { get; set; }
        public List<String> ListSelect { get; set; }
    }
}