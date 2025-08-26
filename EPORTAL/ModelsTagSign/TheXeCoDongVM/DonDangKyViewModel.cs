using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPORTAL.ModelsTagSign.TheXeCoDongVM
{
    public class DonDangKyViewModel
    {
        public int ID { get; set; }
        public string Ma_Don { get; set; }
        public string NoiDung { get; set; }
        // public string TenLoaiNT { get; set; }
        public string TenNhanVienNT { get; set; }
        public string TenBPQL { get; set; }
        public string TenNhaThau { get; set; }
        public string HopDong { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public string FileHoSoXe { get; set; }
        public string TenTinhTrang { get; set; }
        public int? TinhTrang_ID { get; set; }
    }
    public class ChiTietDonVM
    {
        public int ID { get; set; }
        public int? ID_LoaiPhuongTien { get; set; }
        public string BienSoXe { get; set; }
        public bool CapMoi { get; set; }
        public bool CapLai { get; set; }
        public bool GiaHan { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string GhiChu { get; set; }

        public static ChiTietDonVM FromModel(CDNT_ChiTietDon m)
        {
            return new ChiTietDonVM()
            {
                ID = m.ID,
                ID_LoaiPhuongTien = m.ID_LoaiPhuongTien,
                BienSoXe = m.BienSoXe,
                CapMoi = m.CapMoi ?? false,
                CapLai = m.CapLai ?? false,
                GiaHan = m.GiaHan ?? false,
                TuNgay = m.TuNgay,
                DenNgay = m.DenNgay,
                GhiChu = m.GhiChu
            };
        }
        public class StoreResult
        {
            public int Result { get; set; }
            public string Message { get; set; }
            public string MaDon { get; set; }
        }
    }
    public enum TinhTrangDonDangKy
    {
        ChoXuLy = 1,         // Nhà thầu trình ký -> Chờ BPQL xử lý
        DaXuLy = 2,          // BPQL đã duyệt -> chờ CPT xác nhận
        HoanThanh = 3,       // CPT đã xác nhận cấp thẻ
        KhongDatYeuCau = 4   // BPQL từ chối
    }

    public class DonDangKyInsertModel
    {
        public int ID { get; set; }
        public string Ma_Don { get; set; }
        public string NoiDung { get; set; }
        public int? PhongBan_ID { get; set; }
        public int? NhanVienNT_ID { get; set; }
        public int? NhaThau_ID { get; set; }
        public string HopDong { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public string FileHoSoXe { get; set; }
        public int? TrinhKy_ID { get; set; }
        public int? TinhTrang_ID { get; set; }
        public int? LoaiNT_ID { get; set; }
        public string JsonDanhSachXe { get; set; }
    }
    public class CDNT_DonDangKyDetail
    {
        public int ID { get; set; }
        public string Ma_Don { get; set; }
        public string NoiDung { get; set; }
        public int? BPQL_ID { get; set; }
        public string TenPhongBan { get; set; }
        public int? NhanVienNT_ID { get; set; }
        public string HoTen { get; set; }
        public int? NhaThau_ID { get; set; }
        public string FullName { get; set; }
        public string HopDong { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public string FileHoSoXe { get; set; }
        public int? TrangThaiDuyet_ID { get; set; }
        public int? TrinhKy_ID { get; set; }
        public int? TinhTrang_ID { get; set; }
        public string TenTinhTrang { get; set; }
        public int? LoaiNT_ID { get; set; }
        public int ChiTiet_ID { get; set; }
        public int? ID_LoaiPhuongTien { get; set; }
        public string LoaiPhuongTien { get; set; }
        public string BienSoXe { get; set; }
        public bool? CapMoi { get; set; }
        public bool? CapLai { get; set; }
        public bool? GiaHan { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string GhiChu { get; set; }
        public string HoSoTheoXe { get; set; }
    }
    public class DonDangKyModel
    {
        public int ID { get; set; }
        public string Ma_Don { get; set; }
        public string NoiDung { get; set; }
        public int? BPQL_ID { get; set; }
        public int? NhanVienNT_ID { get; set; }
        public int? NhaThau_ID { get; set; }
        public string HopDong { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public string FileHoSoXe { get; set; }
        public int? TrinhKy_ID { get; set; }
        public int? TinhTrang_ID { get; set; }
        public int? LoaiNT_ID { get; set; }
        public List<ChiTietDonVM> DanhSachXe { get; set; }
    }
    public class DinhBienPhuongTienVM
    {
        public int NhaThauID { get; set; }

        public int SoQuanLy { get; set; }
        public int SoCongNhan { get; set; }

        public int XeMay_DaCap { get; set; }
        public int Xe3Gac_DaCap { get; set; }

        public int XeMay_ToiDa { get; set; }
        public int Xe3Gac_ToiDa { get; set; }

        public int XeMay_ConLai { get; set; }
        public int Xe3Gac_ConLai { get; set; }
    }

}
