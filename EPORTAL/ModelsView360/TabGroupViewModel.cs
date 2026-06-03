namespace EPORTAL.ModelsView360
{
    // View model cho tab navigation o ListProject/Index va ListVirtual/Index.
    // Truoc kia view tu new EPORTALEntities() + loop query per group => N+1.
    // Gio controller pre-fetch tat ca tab info trong 2 query roi pass qua ViewBag.
    public class TabGroupViewModel
    {
        public int IDGroup { get; set; }
        public string GroupName { get; set; }
        public bool Active { get; set; }
    }
}
