using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class EmployeBenefitValidation
    {
        public int ID { get; set; }

        public string TieuDe { get; set; }

        public string NoiDung { get; set; }

        public string HinhAnh { get; set; }

        public HttpPostedFileBase NameFile { get; set; }
    }
}