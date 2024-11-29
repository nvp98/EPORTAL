using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class A_AddUserQuyen
    {
        public int IDDS { get; set; }
        public string MaNV { get; set; }
        public string TenNV { get; set; }
        public string NVDG { get; set; }
        public Nullable<int> IDHT { get; set; }
        public string TenHT { get; set; }
        public string[] Selected { get; set; }
        public string[] SelectedKN { get; set; }
    }
}