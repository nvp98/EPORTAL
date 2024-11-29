using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class PictureValidation
    {
        public int ID { get; set; }

        public string TenHinhAnh { get; set; }

        public string Image { get; set; }

        public HttpPostedFileBase NameFile { get; set; }
    }

}