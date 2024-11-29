using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsEquipment
{
    public class NV_QuanLyThietBiValidation
    {
        public int IDQLTB { get; set; }
        public Nullable<int> IDPhongBan { get; set; }
        public string ServiceTag { get; set; }
        public Nullable<int> IDTB { get; set; }
        public string MaNV { get; set; }
        public string Phone { get; set; }
        public Nullable<int> IDSC { get; set; }
        public Nullable<System.DateTime> NgayLap { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> NgayXL { get; set; }
        public Nullable<System.DateTime> NgayHT { get; set; }
        public Nullable<System.DateTime> NgayNM { get; set; }
        public string GhiChu { get; set; }
        public string TenThietBi { get; set; }
        public string TenLoiSC { get; set; }
        public string TenPhongBan { get; set; }
        public string AdminNM { get; set; }
    }
}