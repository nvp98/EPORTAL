using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPartner
{
    public class HistoryValidation
    {
        public int IDHD { get; set; }
        public int IDNVNT { get; set; }
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public int IDNT { get; set; }
        public string ShortName { get; set; }
        public string IDKV { get; set; }
        public int IDCA { get; set; }
        public string TenCA { get; set; }
        public DateTime NgayCap { get; set; }
        public DateTime HanSuDung { get; set; }
        public string GhiChu { get; set; }
    }
}