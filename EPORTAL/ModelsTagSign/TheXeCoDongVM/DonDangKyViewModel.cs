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

        public string TenNhanVienNT { get; set; }
        public string TenBPQL { get; set; }
        public string TenNhaThau { get; set; }
        public string HopDong { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public string FileHoSoXe { get; set; }
        public string TenTinhTrang { get; set; }
        public int? TinhTrang_ID { get; set; }

        public int PerUserStatusCode { get; set; }
        public string PerUserStatusText { get; set; }
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
       
    }
    public class StoreResult
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public string MaDon { get; set; }
    }
    public enum TinhTrangDonDangKy
    {
        ChoXuLy = 1,
        DaXuLy = 2,
        HoanThanh = 3,
        KhongDatYeuCau = 4,
        Nhap = 5
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
        public string UserNameLogin { get; set; }
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
    public class XeCoDongModel
    {
        public int ID { get; set; }
        public int IDNT { get; set; }
        public int? ID_NVNT { get; set; }
        public string TenNhaThau { get; set; }
        public string HoVaTen { get; set; }
        public string CMND_CCCD { get; set; }
        public string TenNgan { get; set; }
        public int? LoaiPhuongTien_ID { get; set; }
        public string LoaiPhuongTien { get; set; }
        public string BienSoXe { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public int? TTHD { get; set; }
        public int? User_Edit { get; set; }

        public string HoVaTenUser_Edit { get; set; }
    }
    public class TrinhKyModel
    {
        public int TrinhKy_ID { get; set; }
        public string Ma_Don { get; set; }
        public int? CapDuyet { get; set; }
        public int? NguoiDuyet_ID { get; set; }
        public DateTime? NgayDuyet { get; set; }
        public int? TinhTrang_ID { get; set; }
        public string GhiChu { get; set; }

        public string TenNguoiDuyet { get; set; }
        public string ChuKyNguoiDuyet { get; set; }
    }
    public class DonDangKyPDFViewModel
    {
        public List<CDNT_DonDangKyDetail> ChiTietDon { get; set; }
        public List<TrinhKyModel> TrinhKy { get; set; }
    }
    public class StepInfo
    {
        public string Ma_Don { get; set; }
        public int? CapDuyet { get; set; }
        public int? TinhTrang_ID { get; set; }
    }
    public class CapCountResult
    {
        public int CapDuyet { get; set; }
        public int SoLuong { get; set; }
    }
}

public enum TrangThaiHoatDong
{
    HoatDong = 1,
    HetHan = 2,
    DaKhoa = 3,
}
