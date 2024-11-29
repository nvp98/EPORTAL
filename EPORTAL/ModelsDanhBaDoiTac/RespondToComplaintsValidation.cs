using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class RespondToComplaintsValidation
    {
        public int IDPhanHoi { get; set; }

        public string MaPhanHoi { get; set; }

        public string TieuDePH { get; set; }
        public string TieuDeKN { get; set; }
        public string NoiDungPH { get; set; }

        public DateTime NgayPH { get; set; }

        public string IDNguoiPH { get; set; }

        public string TenNguoiPH { get; set; }

        public int IDKhieuNai { get; set; }

        public string FileDinhKem { get; set; }
        public HttpPostedFileBase NotiFile { get; set; }
    }
}