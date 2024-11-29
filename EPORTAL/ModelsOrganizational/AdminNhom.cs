using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class AdminNhom
    {
        public int IDNhom { get; set; }
        public string TenNhom { get; set; }
        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public int IDNhomPT { get; set; }
        public string TenNhomPT { get; set; }
        public int SLKTV { get; set; }
        public int isTPT { get; set; }
        public int PTDB { get; set; }
        public int KTVDB { get; set; }
        public int NVDB { get; set; }
        public int KTVCa { get; set; }
        public int NVCa { get; set; }
        public int TPTDB { get; set; }
        public int TPKip { get; set; }
        public int TTTPDB { get; set; }
    }
    public class listIDTPT
    {
        public string name { get; set; }
        public int isTPT { get; set; }
    }
    public class ListPT
    {
        public int IDNhom { get; set; }
        public string TenNhom { get; set; }
    }
}