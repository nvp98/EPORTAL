using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPartner
{
    public class HistoryCardMobilityValidation
    {
        public int IDHCM { get; set; }
        public int IDNVNT { get; set; }
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public int IDNT { get; set; }
        public string ShortName { get; set; }
        public DateTime Tungay { get; set; }
        public DateTime Denngay { get; set; }
        public string SoThe { get; set; }
        public string GhiChu { get; set; }
    }
}