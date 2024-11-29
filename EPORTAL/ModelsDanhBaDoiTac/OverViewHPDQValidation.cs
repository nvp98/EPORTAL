using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class OverViewHPDQValidation
    {
        public int IDTQ { get; set; }
        public string TieuDe { get; set; }
        public string TomTat { get; set; }
        public string DayDu { get; set; }
        public string Note { get; set; }
        public string Image { get; set; }

        public HttpPostedFileBase NameFile { get; set; }
    }
}