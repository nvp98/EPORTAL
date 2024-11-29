using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class Register_gates
    {
        public int IDRE { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public int? IDCong { get; set; }

        public int? IDNV { get; set; }
        public int? IDTrangThai { get; set; }

        public DateTime NgayDK { get; set; }
        public DateTime NgayHetHan { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string MaNV { get; set; }
        public string TenNV { get; set; }
        public string TrangThai { get; set; }

        public string Gate { get; set; }
    }
}