using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class NMNLValidation
    {
        public int IDPhanXuong { get; set; }

        public string TenPhanXuong { get; set; }

        public int PhongBanID { get; set; }
        public string HoTen { get; set; }
    }
    public class getBGDV
    {
        public int PhongBanID { get; set; }
        public string ImagePath { get; set; }
    }
    public class getQD
    {
        public int IDPhanXuong { get; set; }

        public string TenPhanXuong { get; set; }

        public int PhongBanID { get; set; }
        public string HoTen { get; set; }
    }
    public class getNS
    {
        public int TongNS { get; set; }
        public int nshc { get; set; }
        public int ns1ca { get; set; }
        public int tongtolv { get; set; }
    }
    public class dstolv
    {
        public string TenTo { get; set; }
        public int sldb { get; set; }
        public int sltt { get; set; }
    }
}