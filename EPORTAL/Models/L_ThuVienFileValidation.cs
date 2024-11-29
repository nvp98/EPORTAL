using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class L_ThuVienFileValidation
    {
        public int ID { get; set; }
        public string TenTaiLieu { get; set; }
        public string FileName { get; set; }
        public string GhiChu { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public string TenNhomTV { get; set; }
        public int IDNhom { get; set; }
    }
}