﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class TK_CardRegistrationInforValidation
    {
        public int IDTKDKT { get; set; }

        public int DKTID { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayTrinh { get; set; }
        public int CapDuyet { get; set; }
        public int TinhTrangID { get; set; }
        public int NhanVienID { get; set; }
        public DateTime? NgayDuyet { get; set; }
        public string GhiChu { get; set; }
        public int KTV { get; set; }

        public int BPLQ { get; set; }
        public int BPQL { get; set; }
        public int VP1C { get; set; }
    }
}