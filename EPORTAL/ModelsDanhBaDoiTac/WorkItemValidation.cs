using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class WorkItemValidation
    {

        public int IDHangMuc { get; set; }

        public string MaHM { get; set; }

        public string TenHangMuc { get; set; }

        public DateTime NgayKyHĐ { get; set; }

        public int LinhVucHM_ID { get; set; }

        public string TenLVHM { get; set; }

        public int DoiTacID { get; set; }

        public string TenDT { get; set; }

    }
}