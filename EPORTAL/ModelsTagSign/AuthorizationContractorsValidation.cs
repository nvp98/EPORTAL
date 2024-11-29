using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class AuthorizationContractorsValidation
    {
        public int ID { get; set; }
        public int IDNhanVien { get; set; }
        public string MaNV { get; set; }
        public string HoVaTen { get; set; }
        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public int IDQuyen { get; set; }
        public string Name { get; set; }
        public int IDLKD { get; set; }
        public string Chukynhay { get; set; }
        public HttpPostedFileBase ImgChukynhay { get; set; }
        public string Chukychinh { get; set; }
        public HttpPostedFileBase ImgChukychinh { get; set; }
        public string[] Selected { get; set; }
        public string IDCVKN { get; set; }

    }
}