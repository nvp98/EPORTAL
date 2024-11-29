using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class List_Detail_RegisterPeopleValidation
    {
        public int ID_CT_DKTN { get; set; }
        public int DKTN_ID { get; set; }
        public string HoVaTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string CCCD { get; set; }
        public string HoKhau { get; set; }
        public int CV_ID { get; set; }
        public string SoDienThoai { get; set; }

        public string Ten_NTP { get; set; }
        public string HoTen_QuanLy { get; set; }
        public string SoDienThoai_QuanLy { get; set; }


        public string CapMoi { get; set; }
        public string GiaHan { get; set; }
        public string BoSungCong { get; set; }
        public string CapLai { get; set; }
        public string ChuyenNT { get; set; }
        public DateTime ThoiHanThe { get; set; }
        public string KhuVucLamViec { get; set; }
        public string CongLamViec { get; set; }
        public int NhomNT { get; set; }
        public string GhiChu { get; set; }
        public string Price { get; set; }

        public string FileUpload { get; set; }
        public string FileUploadImg { get; set; }
        public HttpPostedFileBase FilePDF { get; set; }
        public HttpPostedFileBase FileANH { get; set; }
        public HttpPostedFileBase FileExcel { get; set; }
        public string Namefile { get; set; }
        public string[] Selected { get; set; }
        public string[] SelectedKV { get; set; }
        public bool IsChecked { get; set; }

        //ListCardRegisInforNT
        public string NoiDung { get; set; }
        public int NTID { get; set; }
        public int HDID { get; set; }
        public int PhongBanID { get; set; }
        public DateTime NgayDangKy { get; set; }
        public int NhanVienID { get; set; }
        public int TinhTrangID { get; set; }
        public HttpPostedFileBase File { get; set; }
        public DateTime? Begind { get; set; }
        public DateTime? Endd { get; set; }
        public int LoaiNT { get; set; }
        public int IDCV { get; set; }

        public string FileComplete { get; set; }
    }
}