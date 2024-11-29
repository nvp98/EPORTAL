using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class GateValidation
    {
        public int IDGate { get; set; }
        public string Gate { get; set; }
        public int? IDTinhTrang { get; set; }
        public string TinhTrang { get; set; }
    }
}