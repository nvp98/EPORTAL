using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class NT_DetailCardRegistrationInforValidation
    {
        public int IDCT { get; set; }
        public string FullName { get; set; }
        public string CCCD { get; set; }
        public DateTime BirthDay { get; set; }
        public string PermanentResidence { get; set; }
        public int Position { get; set; }
        public string NamePosition { get; set; }
        public string Phone { get; set; }
        public string StorageCard { get; set; }
        public string AccessCard { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DaySchool { get; set; }
        public int IDGroup { get; set; }
        public string NameContractorGroup { get; set; }
        public int IDCA { get; set; }
        public string TenCA { get; set; }
        public string IDKV { get; set; }
        public string TenKV { get; set; }
        public string IDGate { get; set; }
        public string Note { get; set; }
        public int IDDS { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public HttpPostedFileBase FileExcel { get; set; }
        public string Namefile { get; set; }
        public string[] Selected { get; set; }
        public string[] SelectedKV { get; set; }
        public int InOutID { get; set; }
        public int ResultID { get; set; }
        public bool IsChecked { get; set; }
    }
}