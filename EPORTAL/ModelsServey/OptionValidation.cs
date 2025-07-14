using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsServey
{
    public class OptionValidation
    {
        public int IDOT { get; set; }
        public string ContentOT { get; set; }
        public Nullable<int> IDSV { get; set; }
        public string ContentSV { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool StatusSV { get; set; }
        public string FilePath { get; set; }
        public Nullable<int> OrderBy { get; set; }

        public Nullable<int> MaOT { get; set; }
        public Nullable<int> isShow { get; set; }

    }

    public class OptionList
    {
        public int ID { get; set; }
        public int? IDNV { get; set; }
        public string MaNV { get; set; }
        public string TenPhongBan { get; set; }
        public string HoTen { get; set; }
        public Nullable<int> IDSV { get; set; }
        public Nullable<bool> XNSV { get; set; }
        public string ContentSV { get; set; }
        public string GhiChu { get; set; }
        public List<OptionValidation> OptionLS { get; set; }
        public int? Answer { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> MenuOT { get; set; }
        //public bool ChayRam { get; set; }
        //public bool ChayTruong { get; set; }
    }
    public class GroupKhaoSatView
    {
        public int ID { get; set; }
        public int? MaNhom { get; set; }
        public int? IDSV { get; set; }
        public string TenNhom { get; set; }

        public int? IDDC { get; set; }

        public OptionList OptionList { get; set; }
        public bool isCompany { get; set; }
        public bool IsChecked { get; set; }
        public int? isShowRe { get; set; }
        public int? isChon { get; set; }

    }

    public class DangKyView
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
        public DateTime NamSinh { get; set; }
        public string Note { get; set; }
        public string DienThoai { get; set; }
        public int? GioiTinh { get; set; }
        public string GioiTinhStr { get; set; }
        public string CuLy { get; set; }
        public string MauAo { get; set; }
        public string Size { get; set; }
        public string DiaChi { get; set; }

    }

    public class DKNguoiThan
    {
       public bool IsChecked { get; set; }
        public int ID { get; set; }
        public Nullable<int> IDNV { get; set; }
        public Nullable<int> IDNguoiThan { get; set; }
        public string HoTen { get; set; }
        public string DienThoai { get; set; }
        public string NamSinh { get; set; }
        public string QuanHe { get; set; }
        public string GhiChu { get; set; }
        public Nullable<int> IDSV { get; set; }
        public Nullable<int> isCom { get; set; }
        public Nullable<int> GioiTinh { get; set; }
        public Nullable<int> IDDC { get; set; }
        public  List<GroupKhaoSatView> listGroup { get; set; }
      public PartTogetherValidation partTogetherValidation { get; set; }

    }

    public class OptionSelect
    {
        public string option { get; set; }
        public string name { get; set; }
    }

}