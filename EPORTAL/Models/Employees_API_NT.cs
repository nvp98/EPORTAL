using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class Employees_API_NT
    {
        public string result { get; set; }
        public string content { get; set; }
        public data[] Employees { get; set; }
        public class data
        {
            public string maNhaThau { get; set; }
            public string tenVietTat { get; set; }
            public string tenNhaThau { get; set; }
            public string maSAP { get; set; }
        }
    }
}