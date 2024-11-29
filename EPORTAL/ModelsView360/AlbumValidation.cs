using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsView360
{
    public class AlbumValidation
    {
        public int IDAlbum { get; set; }
        public string TenAlbum { get; set; }
        public string Images { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
    }
}