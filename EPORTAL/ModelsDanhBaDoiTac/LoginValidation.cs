using EPORTAL.ModelsDanhBaDoiTac;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class LoginValidation
    {
        public int ID { get; set; }
        public int IDDoiTac { get; set; }
        [Required(ErrorMessage = "Nhập Tên Đăng nhập")]
        [RegularExpression(@"[A-Za-z0-9]*$", ErrorMessage = "Tài khoản chứa kí tự đặc biệt")]
        [MaxLength(15, ErrorMessage = "Vượt quá số kí tự 15")]
        public string TaiKhoan { get; set; }
       
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Nhập mật khẩu")]
        [MaxLength(50, ErrorMessage = "Vượt quá số kí tự 15")]
      
        
        public string MatKhau { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Xác nhận mật khẩu")]
        [MaxLength(50, ErrorMessage = "Vượt quá số kí tự 50")]
        [Compare("MatKhau", ErrorMessage = "Xác nhận không khớp")]
        public string NhapLaiMatKhau { get; set; }
        public string MatKhauCu { get; set; }
        public string HoTen { get; set; }
        public string ShortName { get; set; }
        public int IDQuyen { get; set; }
        public int IDPhongBan { get; set; }

        public int isDB
        { get;set; }
        public int isDoiTac
        {get; set; }

        public int isDT { get; set; }
        public string MaNV { get; set;}
        public string IDBP { get; set; }

    }
}