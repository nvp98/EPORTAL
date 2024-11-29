using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPartner
{
    public class ViolateValidation
    {
        public int IDNVVP { get; set; }
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public string CMND { get; set; }
        public DateTime NgaySinh { get; set; }
        public string LyDoCam { get; set; }
        public string GhiChu { get; set; }
        public DateTime CreateDay { get; set; }
        public int? CreateIDNV { get; set; }
        public string NVCreate { get; set; }
        public string TinhTrang { get; set; }
        public int? IDTinhTrang { get; set; }
    }

}