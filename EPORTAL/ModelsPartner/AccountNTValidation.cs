using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPartner
{
    public class AccountNTValidation
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string MatKhau { get; set; }
        public string NhapLaiMatKhau { get; set; }
        public string MatKhauCu { get; set; }
        public string ChuKy { get; set; }
        public Nullable<int> IDNT { get; set; }
        public string TenNhaThau { get; set; }
        public Nullable<int> TinhTrang { get; set; }
        public bool StatusSV { get; set; }
        public HttpPostedFileBase ImgChuky { get; set; }
    }
}