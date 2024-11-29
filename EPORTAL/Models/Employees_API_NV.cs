using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class Employees_API_NV
    {
        public string result { get; set; }
        public string content { get; set; }
        public data[] Employees { get; set; }
        public class data
        {
            public string maNhanVien { get; set; }
            public string tenNhanVien { get; set; }
            public string maSoThe { get; set; }
            public int idNhaThau { get; set; }
            public string maNhaThau { get; set; }
            public string tenNhaThau { get; set; }
            public string ngayCapThe { get; set; }
            public string tenTrangThai { get; set; }
            public string hanSuDung { get; set; }

        }
    }
}