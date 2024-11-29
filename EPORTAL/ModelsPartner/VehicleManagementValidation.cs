using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPartner
{
    public class VehicleManagementValidation
    {
        public int IDTBNT { get; set; }
        public string NameEquipment { get; set; }
        public string IndentifierContact { get; set; }
        public string BienSoXe { get; set; }
        public int IDContact { get; set; }

        public string TenNhaThau { get; set; }
        public string Operator { get; set; }
        public int IDDepartment { get; set; }

        public string TenPhongBan { get; set; }
        public int Cavet { get; set; }
        public DateTime NgayHetHanDK { get; set; }
      
        public DateTime NgayHetHanKD { get; set; }

        public DateTime NgayHetHanBHX { get; set; }

        public int CCHV { get; set; }
        public DateTime NgayHetHanTAT { get; set; }

        public DateTime NgayHetHanVCHHChayNo { get; set; }

        public DateTime NgayHetHanCNKD { get; set; }

        public string ChungChiPCCCCNCH { get; set; }
        public string ChungChiVCHHNHCN { get; set; }
        public DateTime StartDate { get; set; }
        public string IDKV { get; set; }
        public string NguoiNhap { get; set; }
        public int TinhTrang { get; set; }
        public string GhiChu { get; set; }

        public string FileUpload { get; set; }
        public HttpPostedFileBase NameFile { get; set; }
        
        public int IDMDD { get; set; }

        public string MaDinhDanh { get; set; }

        public string SoKhung { get; set; }

        public DateTime ThoiHanTheTu { get; set; }

        public DateTime ThoiHanTheRVTX { get; set; }

        public int IDLoai { get; set; }

        public string CapMoi { get; set; }
        public string GiaHan { get; set; }
        public string MatThe { get; set; }
        public string CapNhatThongTin { get; set; }
        public string BaoLanh { get; set; }
        public DateTime NgayChinhSua { get; set; }
        public string FileTheTu { get; set; }

    }
}