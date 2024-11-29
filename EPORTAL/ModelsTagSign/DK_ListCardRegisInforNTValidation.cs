using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class DK_ListCardRegisInforNTValidation
    {
        public int IDDKT { get; set; }
        public string NoiDung { get; set; }
        public int NTID { get; set; }
        public int HDID { get; set; }
        public int PhongBanID { get; set; }
        public DateTime NgayDangKy { get; set; }
        public int NhanVienID { get; set; }
        public int TinhTrangID { get; set; }
        public string FileUpload { get; set; }
        public HttpPostedFileBase File { get; set; }
        public DateTime? Begind { get; set; }
        public DateTime? Endd { get; set; }
        public string GhiChu { get; set; }
        public int LoaiNT { get; set; }
        public string FileComplete { get; set; }
        public HttpPostedFileBase FilePDF { get; set; }
        public HttpPostedFileBase FileANH { get; set; }
    }
}