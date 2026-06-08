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

        // So luong Video accessible cua user trong album nay - tinh san o controller (Index)
        // de tranh N+1 trong view (truoc kia view tu new EPORTALEntities() trong foreach).
        public int SoLuongVideo { get; set; }
    }
}