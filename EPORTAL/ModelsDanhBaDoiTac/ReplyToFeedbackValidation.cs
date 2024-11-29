using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class ReplyToFeedbackValidation
    {
        public int IDLTLPH { get; set; }
        public string MaTLPH { get; set; }
        public string NoiDungTLPH { get; set; }

        public string NoiDungTLPHCT { get; set; }

        public DateTime NgayTLPH { get; set; }
        public string NguoiTLPH { get; set; }

        public int PhanHoiID { get; set; }

        public string MaPhanHoi { get; set; }
        public int ID { get; set; }

        public int IDDoiTac { get; set; }

        public HttpPostedFileBase NotiFile { get; set; }
    }
}