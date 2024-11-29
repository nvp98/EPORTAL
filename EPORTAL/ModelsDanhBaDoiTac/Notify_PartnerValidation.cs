using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class Notify_PartnerValidation
    {
        public int IDTBDT { get; set; }

        public int IDDoiTac { get; set; }

        public string IDBP { get; set; }

        public string LinhVucHĐ { get; set; }

        public string TenLVHĐ { get; set; }

        public string LoaiIDDT { get; set; }

        public string TenLoaiDT { get; set; }

        public string FullName { get; set; }

        public string ShortName { get; set; }

        public string TaxCode { get; set; }

        public string Address { get; set; }

        public int City { get; set; }


        public int Regions { get; set; }



        public string Customer { get; set; }

        public string Vender { get; set; }

        public string Email { get; set; }


        public int TBaoID { get; set; } 

        public string MaTBao { get; set; }

        public string NoiDungTBao { get; set; }

        public string FileDinhKem { get; set; }

        public DateTime Ngay { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

    }
}