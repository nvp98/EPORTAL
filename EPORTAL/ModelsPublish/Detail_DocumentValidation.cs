using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPublish
{
    public class Detail_DocumentValidation
    {
        public int IDCTVB { get; set; }
        public int IDVB { get; set; }
        public string SoVB { get; set; }
        public string NoiDungVB { get; set; }
        public DateTime NgayVB { get; set; }
        public DateTime NgayBĐ { get; set; }
        public DateTime NgayKT { get; set; }

        public string TenFile { get; set; }
        public int Status { get; set; }
    }
}