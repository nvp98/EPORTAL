﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsView360
{
    public class AuthorizationUSERValidation
    {
        public int ID { get; set; }
        public int ProjectID { get; set; }
        public int NhanVienID { get; set; }
        public DateTime Createdate { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string[] Selected { get; set; }
        public bool IsCheck { get; set; }
    }
}