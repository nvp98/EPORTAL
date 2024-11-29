using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class EmployeeValidation
    {
        public int ID { get; set; }

        public string MaNV { get; set; }

        public string HoTen { get; set; }

        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public string PhongBan { get; set; }
        public int IDQuyenHT { get; set; }
        public string TenQuyen { get; set; }
        public string TenQuyenHT { get; set; }
        public int IDViTri { get; set; }
        public string TenViTri { get; set; }
        public bool IsGV { get; set; }
        public string Email { get; set; }
        public int IDTinhTrangLV { get; set; }

    }
}