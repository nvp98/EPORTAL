using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class NotifyValidation
    {
        public int IDTBao { get; set; }

        public string MaTBao { get; set; }

        public string NoiDungTBao { get; set; }

        public int DoiTacID { get; set; }

        public string TenDoiTac { get; set; }

        public string FileDinhKem { get; set; }

        public HttpPostedFileBase NotiFile { get; set; }

        public DateTime Ngay { get; set; }
        
        public int TinhTrangTB { get; set; }
        public string TinhTrangThongBao { get; set; }

    }
}