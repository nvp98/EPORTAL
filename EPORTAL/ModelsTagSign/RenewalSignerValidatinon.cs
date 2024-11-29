using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class RenewalSignerValidatinon
    {
        public int IDTKGH { get; set; }

        public int IDGH { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public int ContractorsID { get; set; }
        public int ContractID { get; set; }
        public string Somecontracts { get; set; }
        public int PhongBanID { get; set; }
        public int IDKTV { get; set; }
        public int TKKTV { get; set; }
        public DateTime? NgayKyKTV { get; set; }
        public int IDBPQL { get; set; }

        public int TKBPQL { get; set; }
        public DateTime? NgayKyBPQL { get; set; }
        public int IDVP1C { get; set; }

        public int TKVP1C { get; set; }
        public DateTime? NgayKyVP1C { get; set; }
        public int idkd { get; set; }

        public string content { get; set; }

        public string fullname { get; set; }


        public int ttktv { get; set; }

        public int ttbpql { get; set; }

        public int ttvp1c { get; set; }
        public string tenttktv { get; set; }
        public string tenttbpql { get; set; }
        public string tenttvp1c { get; set; }

        public string maktv { get; set; }
        public string mabpql { get; set; }
        public string mavp1c { get; set; }
        public string tenktv { get; set; }
        public string tenbpql { get; set; }
        public string tenvp1c { get; set; }

        public string GhiChuKTV { get; set; }
        public string GhiChuBPQL { get; set; }
        public string GhiChuVP1C { get; set; }
    }
}