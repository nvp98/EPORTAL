using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class AdminPhanXuong
    {
        public int PhanXuongID { get; set; }
        public string TenPhanXuong { get; set; }
        public int PhongBanID { get; set; }
        public string TenPhongBan { get; set; }
        public int PTDB { get; set; }
        public int TPDB { get; set; }
        public int KTVDB { get; set; }
        public int NVDB { get; set; }
        public int PTTT { get; set; }
        public int TPTT { get; set; }
        public int KTVTT { get; set; }
        public int NVTT { get; set; }
        public int TPKip { get; set; }
        public int TTTP { get; set; }
    }
}