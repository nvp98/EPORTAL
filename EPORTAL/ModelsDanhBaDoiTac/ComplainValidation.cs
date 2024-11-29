using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class ComplainValidation
    {
        public int IDKhieuNai { get; set; }

        public string MaKhieuNai { get; set; }

        public string TieuDeKN { get; set; }
        public string NoiDungKN { get; set; }

        public string FileDinhKem { get; set; }

        public string NguoiKN { get; set; }

       
        public DateTime NgayKN { get; set; }

        public int IDDoiTac { get; set; }

        public string IDBP { get; set; }

        public string LinhVucHĐ { get; set; }

        public string Email { get; set; }
        public string LoaiDTID { get; set; }

        public string TenLoaiDT { get; set; }

       

        public string ShortName { get; set; }

        public string FullName { get; set; }

        

        public string TaxCode { get; set; }

        public string Address { get; set; }

        public int City { get; set; }

        public int Regions { get; set; }

        public string ImageLogo { get; set; }

        public string Customer { get; set; }

        public string Vendor { get; set; }

        public string QuanLy { get; set; }

        public string TinhTrang { get; set; }

        public string TenQuyen { get; set; }
        public int SLPH { get; set; }

        public int IDTTKN { get; set; }

        public string TenTTKN { get; set; }



        public HttpPostedFileBase NotiFile { get; set; }
    }
}