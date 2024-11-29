using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsServey
{
    public class UserServeyValidation
    {
        public int IDSV { get; set; }
        public string ContentSV { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool StatusSV { get; set; }
        public TimeSpan Exp { get; set; }
        public int? CountSV { get; set; }
        public int? CountDKS { get; set; }
     
    }
}