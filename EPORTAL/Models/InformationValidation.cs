using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class InformationValidation
    {
        public int IIDThongTin { get; set; }

        public int IDNhanVien { get; set; }

        public string MaNV { get; set; }

        public string HoTen { get; set; }

        public string TenPB { get; set; }

        public string TenViTri { get; set; }
        public string CMND { get; set; }

        public string CCCD { get; set; }
        public string FileHinh { get; set; }

        public bool XacNhan { get; set; }
    }
}