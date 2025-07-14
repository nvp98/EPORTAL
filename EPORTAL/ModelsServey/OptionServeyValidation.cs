using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsServey
{
    public class OptionServeyValidation
    {
        public int IDOT { get; set; }
        public string ContentOT { get; set; }
        public Nullable<int> IDSV { get; set; }
        public string ContentSV { get; set; }
        public string FilePath { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public Nullable<int> OrderBy { get; set; }
        public Nullable<int> MaOT { get; set; }
        public string TenNhom { get; set; }
    }
}