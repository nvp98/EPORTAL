using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPartner
{
    public class NT_PartnerValidation
    {
        public int ID { get; set; }
        public int BPID { get; set; }
        public string FullName { get; set; }
        public string Taxcode { get; set; }
        public string Address { get; set; }
        public string Customer { get; set; }
        public string Vendor { get; set; }
        public string Email { get; set; }
        public string ShortName { get; set; }
        public string CodeUnis { get; set; }
    }
}