using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsPublish
{
    public class DocumentValidation
    {
        public int IDVB { get; set; }
        public string TenVB { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public int NhanVienID { get; set; }
        public int IDPage { get; set; }
        public int IDLoai { get; set; }

    }
}