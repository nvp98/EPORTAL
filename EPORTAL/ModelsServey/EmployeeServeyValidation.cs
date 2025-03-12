using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsServey
{
    public class EmployeeServeyValidation
    {
        public int ID { get; set; }
        public int IDNV { get; set; }
        public string LIDSNV { get; set; }
        public string MaNV { get; set; }
        public string TenPhongBan { get; set; }
        public string HoTen { get; set; }
        public Nullable<int> IDSV { get; set; }
        public string ContentSV { get; set; }
        public string[] Selected { get; set; }
        public Nullable<int> OTID { get; set; }
        public string ContentOT { get; set; }
        public bool? XNSV { get; set; }
        public bool StatusSV { get; set; }
    }
    public class ExportEmployeeServeyValidation
    {
        public int ID { get; set; }
        public int IDNV { get; set; }
        public string LIDSNV { get; set; }
        public string MaNV { get; set; }
        public string TenPhongBan { get; set; }
        public string HoTen { get; set; }
        public Nullable<int> IDSV { get; set; }
        public string ContentSV { get; set; }
        public string[] Selected { get; set; }
        public Nullable<int> OTID { get; set; }
        public string ContentOT { get; set; }
        public string LyDo { get; set; }
        public bool? XNSV { get; set; }
        public bool StatusSV { get; set; }
        public List<PartTogetherValidation> LSPart{ get; set; }
        public Nullable<int> MenuOT { get; set; }
        public List<String> ListSelect { get; set; }
        public List<String> ListSelectKhac { get; set; }
    }
}