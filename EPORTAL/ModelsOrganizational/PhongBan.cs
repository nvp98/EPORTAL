//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPORTAL.ModelsOrganizational
{
    using System;
    using System.Collections.Generic;
    
    public partial class PhongBan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PhongBan()
        {
            this.NhanVienAPIs = new HashSet<NhanVienAPI>();
        }
    
        public int IDPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public Nullable<int> NhomID { get; set; }
        public string ImagePath { get; set; }
        public string url { get; set; }
        public Nullable<int> TPDB { get; set; }
        public Nullable<int> QDTPTDB { get; set; }
        public Nullable<int> PTDB { get; set; }
        public Nullable<int> TPKipDB { get; set; }
        public Nullable<int> KTVHCDB { get; set; }
        public Nullable<int> KTVCaDB { get; set; }
        public Nullable<int> TTTPHCDB { get; set; }
        public Nullable<int> TTTPCaDB { get; set; }
        public Nullable<int> NVHCDB { get; set; }
        public Nullable<int> NVCaDB { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NhanVienAPI> NhanVienAPIs { get; set; }
        public virtual Nhom Nhom { get; set; }
    }
}
