
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
    
public partial class NT_DetailExtendcard
{

    public int IDCTGH { get; set; }

    public string HoTen { get; set; }

    public string CCDC { get; set; }

    public Nullable<System.DateTime> NgaySinh { get; set; }

    public string DiaChi { get; set; }

    public Nullable<int> ChucVu { get; set; }

    public string Sdt { get; set; }

    public string DoiThe { get; set; }

    public string GiaHanThe { get; set; }

    public string BoSungDt { get; set; }

    public Nullable<System.DateTime> ThoiHanThe { get; set; }

    public Nullable<int> IDCA { get; set; }

    public string IDKV { get; set; }

    public Nullable<int> NhomNT { get; set; }

    public string CongID { get; set; }

    public string GhiChu { get; set; }

    public Nullable<int> IDGH { get; set; }



    public virtual NT_Extendcard NT_Extendcard { get; set; }

}

}