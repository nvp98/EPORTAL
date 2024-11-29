using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class NT_CarteTempo
    {
        public int IDDTT { get; set; }
        public int? TTTID { get; set; }
        public int? CapDuyet { get; set; }
        public int? KTV { get; set; }
        public int? TPBP { get; set; }
        public int? VP1C { get; set; }
        public int? TinhTrangID { get; set; }
        public int? NhanVienID { get; set; }
        public int? isNT { get; set; }
        
        public DateTime? NgayDuyet { get; set; }
        public string GhiChu { get; set; }

    }
}