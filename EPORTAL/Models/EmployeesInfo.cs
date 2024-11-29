using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class EmployeesInfo
    {
        public int IDThongTin { get; set; }

        public int IDNhanVien { get; set; }

        public string MaNV { get; set; }

        public string HoTen { get; set; }

        public int IDPhongBan { get; set; }

        public string PhongBan { get; set; }

        public int IDViTri { get; set; }

        public string ViTri { get; set; }

        [Required(ErrorMessage = "Mời nhập CMND")]
        [MaxLength(9, ErrorMessage = "CMND bao gồm 9 ký tự")]
        [MinLength(9, ErrorMessage = "CMND bao gồm 9 ký tự")]
        public string CMND { get; set; }

        [Required(ErrorMessage = "Mời nhập CCCD/Mã định danh")]
        [MaxLength(12, ErrorMessage = "CCCD/ĐDCN bao gồm 12 ký tự")]
        [MinLength(12, ErrorMessage = "CCCD/ĐDCN bao gồm 12 ký tự")]
        public string CCCD { get; set; }

        public string FileHinh { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        public int TongSoNV { get; set; }

        public int SLHoanThanh { get; set; }

        public bool XacNhan { get; set; }

    }
}