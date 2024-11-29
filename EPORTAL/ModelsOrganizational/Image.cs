using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class Image
    {
        public int IDImage { get; set; }
        public string Name { get; set; }
        public byte[] files { get; set; }
    }
}