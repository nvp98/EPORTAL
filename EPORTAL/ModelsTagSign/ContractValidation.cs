using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class ContractValidation
    {
        public int IDContract { get; set; }
        public string Somecontracts { get; set; }
        public string Contract { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FileScan { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public Nullable<int> IDNT { get; set; }
        public string Fullname { get; set; }
        public Nullable<int> IDBP { get; set; }
        public string TenBP { get; set; }
        public int TPBP { get; set; }
        public int TTXL { get; set; }
    }
}