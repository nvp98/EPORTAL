using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsView360
{
    public class VideoValidation
    {
        public int IDVideo { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string Images { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public int AlbumID { get; set; }
        public string TenAlbum { get; set; }

    }
}