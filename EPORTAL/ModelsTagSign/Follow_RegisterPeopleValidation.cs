using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class Follow_RegisterPeopleValidation
    {
        public int ID_TK_TN { get; set; }
        public int DKTN_ID { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayTrinh { get; set; }
        public int NT_ID { get; set; }
        public string HopDong { get; set; }
        public string File_CCAT { get; set; }
        public string File_IMG { get; set; }
        public int CapDuyet { get; set; }
        public int LuongXuLy { get; set; }
        public int TinhTrangID { get; set; }
        public int NhanVienID { get; set; }
        public DateTime? NgayDuyet { get; set; }
        public string GhiChu { get; set; }

        public int TrinhKy_ID { get; set; }
        public int LoaiNT_ID { get; set; }
        
    }
}