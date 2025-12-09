using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsResidency
{
    public class ResidencyViewModel
    {
        public int EmployeeId { get; set; }
        public string MaNhanVien { get; set; }
        public string EmployeeName { get; set; }
        public string TenViTri { get; set; }
        public string TenPhongBan { get; set; }

        public string PhonePersonal { get; set; }
        public string RelativeName { get; set; }
        public string RelativePhone { get; set; }

        public string FatherName { get; set; }
        public int? FatherYob { get; set; }
        public string MotherName { get; set; }
        public int? MotherYob { get; set; }
        public string SpouseName { get; set; }
        public int? SpouseYob { get; set; }

        public List<ChildModel> Children { get; set; }

        public List<Province> Provinces { get; set; } = new List<Province>();
        public List<Commune> Communes { get; set; } = new List<Commune>();

        public string SelectedProvinceId { get; set; }
        public string SelectedCommuneId { get; set; }

        public string PermanentProvinceCode { get; set; }
        public string PermanentProvinceName { get; set; }
        public string PermanentCommuneCode { get; set; }
        public string PermanentCommuneName { get; set; }
        public string PermanentAddress01 { get; set; }
        public string PermanentAddress02 { get; set; }

        public string CurrentProvinceCode { get; set; }
        public string CurrentProvinceName { get; set; }
        public string CurrentCommuneCode { get; set; }
        public string CurrentCommuneName { get; set; }
        public string CurrentAddress01 { get; set; }
        public string CurrentAddress02 { get; set; }

        public string ChangeOption { get; set; }

        public bool IsChange { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    public class ChildModel
    {
        public string HoTen { get; set; }
        public int? NamSinh { get; set; }
    }

    public class Province
    {
        public string code { get; set; }
        public string name { get; set; }
        public string englishName { get; set; }
        public string administrativeLevel { get; set; }
        public string decree { get; set; }
    }

    public class Commune
    {
        public string code { get; set; }
        public string name { get; set; }
        public string englishName { get; set; }
        public string administrativeLevel { get; set; }
        public string provinceCode { get; set; }
        public string provinceName { get; set; }
        public string decree { get; set; }
    }

    public class ProvinceResponse
    {
        public string requestId { get; set; }
        public List<Province> provinces { get; set; }
    }

    public class CommuneResponse
    {
        public string requestId { get; set; }
        public List<Commune> communes { get; set; }
    }
}