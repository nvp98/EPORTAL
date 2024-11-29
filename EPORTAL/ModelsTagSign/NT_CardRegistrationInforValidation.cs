using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class NT_CardRegistrationInforValidation
    {
        public int IDDS { get; set; }
        public string Namefile { get; set; }
        public string Contents { get; set; }
        public DateTime DayUpload { get; set; }
        public string Note { get; set; }
        public int IDTTTK { get; set; }
        public int IDNVTK { get; set; }
        public int ContractorsID { get; set; }
        public string FullName { get; set; }
        public Nullable<int> ContractID { get; set; }
        public string Somecontracts { get; set; }
        public int PhongBanID { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public DateTime? Begind { get; set; }
        public DateTime? Endd { get; set; }
        public string FileComplete { get; set; }
        public DateTime DaySchool { get; set; }
    }
}