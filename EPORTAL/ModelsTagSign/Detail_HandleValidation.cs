using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class Detail_HandleValidation
    {
        public int IDXL { get; set; }
        public int IDTTT { get; set; }
        public int IDCTTTT { get; set; }
        public string HoVaTen { get; set; }
        public string CCCD { get; set; }
        public string ChucVu { get; set; }
        
        public DateTime NgaySinh { get; set; }
     //   public int MucDich { get; set; }
        public string Sdt { get; set; }
        public string BienSo { get; set; }
        public int Cong { get; set; }
        public string GhiChu { get; set; }
        public int NguoiXuLy { get; set; }
        public DateTime ThoiGianXuLy { get; set; }
        public int? IDThe { get; set; }
        public string MaThe { get; set; }
        public string TenCong { get; set; }
        public string SoPhieuDKVTTS { get; set; }
        public string VTTSChuaDK { get; set; }
        public string VTTheoPVCC { get; set; }
        public string SoLanRaVao { get; set; }
        public string TenPhuongTien { get; set; }
    }
}