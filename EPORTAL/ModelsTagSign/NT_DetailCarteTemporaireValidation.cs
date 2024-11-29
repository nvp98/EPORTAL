using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class NT_DetailCarteTemporaireValidation
    {
        public int IDCTTTT { get; set; }
        public string HoVaTen { get; set; }
        public string CCCD { get; set; }
        public DateTime NgaySinh { get; set; }
        public string Sdt { get; set; }
        public string BienSo { get; set; }
        public int Cong { get; set; }
        public int? LoaiPTID { get; set; }
        public string TenPT { get; set; }
        public string ChucVu { get; set; }
        public string GhiChu { get; set; }
        public string SoPhieuDKVTTS {  get; set; }
        public string VTTSChuaDK {  get; set; }
        public string VTTheoPVCC {  get; set; }
        public string SoLanRaVao {  get; set; }
        public int IDTTT { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public HttpPostedFileBase FileExcel { get; set; }
        public string Namefile { get; set; }
        public string[] Selected { get; set; }
    }
}