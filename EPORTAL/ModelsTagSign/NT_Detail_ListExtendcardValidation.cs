using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class NT_Detail_ListExtendcardValidation
    {
        public int IDCTGH { get; set; }
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public DateTime NgaySinh { get; set; }
        public string DiaChi { get; set; }
        public int ChucVu { get; set; }
        public string NamePosition { get; set; }
        public string Sdt { get; set; }
        public string DoiThe { get; set; }
        public string GiaHanThe { get; set; }
        public string BoSungDt { get; set; }
        public DateTime ThoiHanThe { get; set; }
        public DateTime DaySchool { get; set; }
        public int IDCA { get; set; }
        public string TenCA { get; set; }
        public string IDKV { get; set; }
        public int NhomNT { get; set; }
        public string NameContractorGroup { get; set; }
        public string CongID { get; set; }
        public string GhiChu { get; set; }
        public int IDGH { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public HttpPostedFileBase FileExcel { get; set; }
        public string Namefile { get; set; }

        public string[] SelectedKV { get; set; }
        public string[] Selected { get; set; }
        public int InOutID { get; set; }
        public int ResultID { get; set; }
        public bool IsChecked { get; set; }
    }
}