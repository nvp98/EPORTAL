using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class TerminalModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string IPAdress { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string Vitri { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public int? IDCong { get; set; }
        public int? IDTinhTrang { get; set; }
        public string TinhTrang { get; set; }
        public string TenCong { get; set; }
    }
}