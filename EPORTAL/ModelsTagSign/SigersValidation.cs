using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class SigersValidation
    {
        public int idtk { get; set; }

        public int idds { get; set; }
        public DateTime? day { get; set; }
        public int ContractorsID { get; set; }
        public Nullable<int> ContractID { get; set; }
        public string Somecontracts { get; set; }
        public int PhongBanID { get; set; }

        public int idktv { get; set; }

        public int tkktv { get; set; }
        public DateTime? ngaykyktv { get; set; }
        public int idbpql { get; set; }

        public int tkbpql { get; set; }
        public DateTime? ngaykybpql { get; set; }
        public int idvp1c { get; set; }

        public int tkvp1c { get; set; }
        public DateTime? ngaykyvp1c { get; set; }
        public int idkd { get; set; }

        public string content { get; set; }

        public string fullname { get; set; }

        //public string bpql { get; set; }

        //public string vp1c { get; set; }

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

        public string ghichuktv { get; set; }
        public string ghichutpbp { get; set; }
        public string ghichuvp1c { get; set; }
        public DateTime? dayschool { get; set; }
    }
}