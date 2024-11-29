using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class A_GroupQuyenDetailsValidation
    {
        public int IDQuyenHT { get; set; }
        public string TenQuyenHT { get; set; }
        public List<A_ListControllerValidation> ListController { get; set; }
    }
}