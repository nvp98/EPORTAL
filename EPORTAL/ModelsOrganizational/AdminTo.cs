using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class AdminTo
    {
        public string TenTo { get; set; }
        public int PhanXuongID { get; set; }
        public int PhongBanID { get; set; }
        public int SLDB { get; set; }
        public int SLTT { get; set; }
        public string TenPhanXuong { get; set; }
        public string TenPhongBan { get; set; }
        public int IDTo { get; set; }

    }
    public class PXList
    {
        public int IDPX { get; set; }
        public string TenPX { get; set; }
    }
}