using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class AuthorizationTVValidation
    {
        public int ID { get; set; }
        public int IDThuVien { get; set; }
        public int NhanVienID { get; set; }
        public DateTime Createdate { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string[] Selected { get; set; }
        public bool IsCheck { get; set; }
    }
}