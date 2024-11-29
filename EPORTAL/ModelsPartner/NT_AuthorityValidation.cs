using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPartner
{
    public class NT_AuthorityValidation
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string CCCD { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public DateTime? Begindate { get; set; }
        public DateTime? Enddate { get; set; }
        public string File_Scan { get; set; }
        public HttpPostedFileBase File { get; set; }
        public Nullable<int> BPID { get; set; }
    }
}