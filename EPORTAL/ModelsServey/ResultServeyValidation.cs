using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsServey
{
    public class ResultServeyValidation
    {
        public int ID { get; set; }
        public Nullable<int> IDES { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public Nullable<int> IDSV { get; set; }
        public string ContentSV { get; set; }
        public Nullable<int> IDOT { get; set; }
        public string ContentOT { get; set; }
        public Nullable<int> StatusRS { get; set; }
    }
}