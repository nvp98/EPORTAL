using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class List_RegisterPeopleValidation
    {
        public int ID_DKTN { get; set; }
        public string NoiDung { get; set; }
        public int BPQL_ID { get; set; }
        public string TenBP_QL { get; set; }
        public int NhanVienNT_ID { get; set; }
        public string Ten_User { get; set; }
        public int NhaThau_ID { get; set; }
        public string TenNhaThau { get; set; }
        public string HopDong { get; set; }
        public DateTime NgayTrinhKy { get; set; }
        public string File_CCAT { get; set; }
        public string File_IMG { get; set; }
        public int TrinhKy_ID { get; set; }
        public string TenLoaiTK { get; set; }
        public int TinhTrang_ID { get; set; }
        public string GhiChu { get; set; }
        public int LoaiNT_ID { get; set; }
        public string FileComplete { get; set; }
        public HttpPostedFileBase File { get; set; }
        public DateTime? Begind { get; set; }
        public DateTime? Endd { get; set; }
        public HttpPostedFileBase FilePDF { get; set; }
        public HttpPostedFileBase FileANH { get; set; }
        public int CBNV1 { get; set; }
        public int CBNV2 { get; set; }
        public int CBNV3 { get; set; }
        public int CBNV4 { get; set; }
        public int CBNV5 { get; set; }
        public int CBNV6 { get; set; }
        public int CBNV7 { get; set; }
        public int CBNV8 { get; set; }
        public string FileUpload { get; set; }
        public string FileUploadImg { get; set; }
        public int ID_NV { get; set; }
        public int LuongXuLY { get; set; }
    }
}