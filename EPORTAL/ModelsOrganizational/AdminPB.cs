using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class AdminPB
    {
        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public int NhomID { get; set; }
        public string TenNhom { get; set; }
        public string ImagePath { get; set; }
        public string url { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public int TPDB { get; set; }
        public int QDTPTDB { get; set; }
        public int PTDB { get; set; }
        public int TPKipDB { get; set; }
        public int KTVHCDB { get; set; }
        public int KTVCaDB { get; set; }
        public int TTTPHCDB { get; set; }
        public int TTTPCaDB { get; set; }
        public int NVHCDB { get; set; }
        public int NVCaDB { get; set; }
    }
}