using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class OverViewTDValidation
    {
        public int ID { get; set; }

        public string TieuDe { get; set; }

        public string NoiDung { get; set; }

        public string HinhAnh { get; set; }

        public string Note1 { get; set; }

        public string Note2 { get; set; }

        public HttpPostedFileBase NameFile { get; set; }
    }
}