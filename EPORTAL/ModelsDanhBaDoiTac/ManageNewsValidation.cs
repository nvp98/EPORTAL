using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class ManageNewsValidation
    {
        public int IDTinTuc { get; set; }

       

        public string TenDeTai { get; set; }

        public string TomTatTinTuc { get; set; }

        public string NoiDungTinTuc { get; set; }

        public string ImageTinTuc { get; set; }

        public DateTime NgayDangTin { get; set; }

        public string NguoiDangTin { get; set; }

        public HttpPostedFileBase NotiFile { get; set; }

    }
}