using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPORTAL.Models
{
    public class LoginValidation
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Nhập MãNV")]
        [RegularExpression(@"[A-Za-z0-9]*$", ErrorMessage = "Tài khoản chứa kí tự đặc biệt")]
        [MaxLength(15, ErrorMessage = "Vượt quá số kí tự 15")]
        public string MaNV { get; set; }
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
        public int IDQuyenHT { get; set; }
        public int IDPhongBan { get; set; }
        public int NT_Permission { get; set; }
        public string Chukynhay { get; set; }
        public HttpPostedFileBase ImgChukynhay { get; set; }
        public string Chukychinh { get; set; }
        public HttpPostedFileBase ImgChukychinh { get; set; }
        public string GroupQuyen { get; set; }
        public List<ItemCheck> LSChecked { get; set; }
    }
}