//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPORTAL.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class A_QuyenHT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public A_QuyenHT()
        {
            this.A_QuyenCT = new HashSet<A_QuyenCT>();
        }
    
        public int IDQuyenHT { get; set; }
        public string TenQuyenHT { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<A_QuyenCT> A_QuyenCT { get; set; }
    }
}