
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
    
public partial class Video
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Video()
    {

        this.AuthorizationVideos = new HashSet<AuthorizationVideo>();

    }


    public int IDVideo { get; set; }

    public string Title { get; set; }

    public string URL { get; set; }

    public string Images { get; set; }

    public Nullable<System.DateTime> Date { get; set; }

    public string Note { get; set; }

    public Nullable<int> IDPhongBan { get; set; }

    public Nullable<int> AlbumID { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<AuthorizationVideo> AuthorizationVideos { get; set; }

}

}
