
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace EPORTAL.ModelsView360
{

using System;
    using System.Collections.Generic;
    
public partial class KD_LoaiKyDuyet
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public KD_LoaiKyDuyet()
    {

        this.KD_KyDuyet = new HashSet<KD_KyDuyet>();

    }


    public int IDLKD { get; set; }

    public string TenLoaiKyDuyet { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<KD_KyDuyet> KD_KyDuyet { get; set; }

}

}