using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class ManagePartnerValidation
    {
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

        public string CityName { get; set; }
        public int Regions { get; set; }

        public string RegionsName { get; set; }

        public string Customer { get; set; }

        public string Vender { get; set; }

        public string Email { get; set; }

        public string ImageLogo { get; set; }


        public string TenQuyen { get; set; }

        public int TLSLNT { get; set; }

        public int TLSLNCC { get; set; }
        public int TLSLKH { get; set; }

        public string TenLinhVucHĐ { get; set; }
        public int LoaiiDDT { get; set; }
        public int IDLinhVucHĐ { get; set; }


        public HttpPostedFileBase ImageFile { get; set; }


    }
}