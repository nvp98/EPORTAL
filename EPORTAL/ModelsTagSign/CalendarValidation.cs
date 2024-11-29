using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsTagSign
{
    public class CalendarValidation
    {
        public int IDCalendar { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public Nullable<int> PhongBanID { get; set; }
        public string TenPhongBan { get; set; }
        public DateTime Date { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Image { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public string Note { get; set; }
        public int IDPlan { get; set; }
        public int Status { get; set; }
    }
}