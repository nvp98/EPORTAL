﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPORTAL.ModelsEquipment
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class EquipmentEntities : DbContext
    {
        public EquipmentEntities()
            : base("name=EquipmentEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<NV_LoiSuCoTB> NV_LoiSuCoTB { get; set; }
        public virtual DbSet<NV_QuanLyThietBi> NV_QuanLyThietBi { get; set; }
        public virtual DbSet<NV_ThietBi> NV_ThietBi { get; set; }
    
        public virtual int NV_LoiSuCoTB_delete(Nullable<int> iDSC)
        {
            var iDSCParameter = iDSC.HasValue ?
                new ObjectParameter("IDSC", iDSC) :
                new ObjectParameter("IDSC", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_LoiSuCoTB_delete", iDSCParameter);
        }
    
        public virtual int NV_LoiSuCoTB_insert(string tenLoiSC)
        {
            var tenLoiSCParameter = tenLoiSC != null ?
                new ObjectParameter("TenLoiSC", tenLoiSC) :
                new ObjectParameter("TenLoiSC", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_LoiSuCoTB_insert", tenLoiSCParameter);
        }
    
        public virtual int NV_LoiSuCoTB_update(Nullable<int> iDSC, string tenLoiSC)
        {
            var iDSCParameter = iDSC.HasValue ?
                new ObjectParameter("IDSC", iDSC) :
                new ObjectParameter("IDSC", typeof(int));
    
            var tenLoiSCParameter = tenLoiSC != null ?
                new ObjectParameter("TenLoiSC", tenLoiSC) :
                new ObjectParameter("TenLoiSC", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_LoiSuCoTB_update", iDSCParameter, tenLoiSCParameter);
        }
    
        public virtual int NV_QuanLyThietBi_delete(Nullable<int> iDQLTB)
        {
            var iDQLTBParameter = iDQLTB.HasValue ?
                new ObjectParameter("IDQLTB", iDQLTB) :
                new ObjectParameter("IDQLTB", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_QuanLyThietBi_delete", iDQLTBParameter);
        }
    
        public virtual int NV_QuanLyThietBi_insert(Nullable<int> iDPhongBan, string serviceTag, Nullable<int> iDTB, string maNV, string phone, Nullable<int> iDSC, Nullable<System.DateTime> ngayLap, string status, Nullable<System.DateTime> ngayXL, Nullable<System.DateTime> ngayHT, Nullable<System.DateTime> ngayNM, string ghiChu)
        {
            var iDPhongBanParameter = iDPhongBan.HasValue ?
                new ObjectParameter("IDPhongBan", iDPhongBan) :
                new ObjectParameter("IDPhongBan", typeof(int));
    
            var serviceTagParameter = serviceTag != null ?
                new ObjectParameter("ServiceTag", serviceTag) :
                new ObjectParameter("ServiceTag", typeof(string));
    
            var iDTBParameter = iDTB.HasValue ?
                new ObjectParameter("IDTB", iDTB) :
                new ObjectParameter("IDTB", typeof(int));
    
            var maNVParameter = maNV != null ?
                new ObjectParameter("MaNV", maNV) :
                new ObjectParameter("MaNV", typeof(string));
    
            var phoneParameter = phone != null ?
                new ObjectParameter("Phone", phone) :
                new ObjectParameter("Phone", typeof(string));
    
            var iDSCParameter = iDSC.HasValue ?
                new ObjectParameter("IDSC", iDSC) :
                new ObjectParameter("IDSC", typeof(int));
    
            var ngayLapParameter = ngayLap.HasValue ?
                new ObjectParameter("NgayLap", ngayLap) :
                new ObjectParameter("NgayLap", typeof(System.DateTime));
    
            var statusParameter = status != null ?
                new ObjectParameter("Status", status) :
                new ObjectParameter("Status", typeof(string));
    
            var ngayXLParameter = ngayXL.HasValue ?
                new ObjectParameter("NgayXL", ngayXL) :
                new ObjectParameter("NgayXL", typeof(System.DateTime));
    
            var ngayHTParameter = ngayHT.HasValue ?
                new ObjectParameter("NgayHT", ngayHT) :
                new ObjectParameter("NgayHT", typeof(System.DateTime));
    
            var ngayNMParameter = ngayNM.HasValue ?
                new ObjectParameter("NgayNM", ngayNM) :
                new ObjectParameter("NgayNM", typeof(System.DateTime));
    
            var ghiChuParameter = ghiChu != null ?
                new ObjectParameter("GhiChu", ghiChu) :
                new ObjectParameter("GhiChu", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_QuanLyThietBi_insert", iDPhongBanParameter, serviceTagParameter, iDTBParameter, maNVParameter, phoneParameter, iDSCParameter, ngayLapParameter, statusParameter, ngayXLParameter, ngayHTParameter, ngayNMParameter, ghiChuParameter);
        }
    
        public virtual ObjectResult<NV_QuanLyThietBi_searchByID_Result> NV_QuanLyThietBi_searchByID(Nullable<int> iDQLTB)
        {
            var iDQLTBParameter = iDQLTB.HasValue ?
                new ObjectParameter("IDQLTB", iDQLTB) :
                new ObjectParameter("IDQLTB", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<NV_QuanLyThietBi_searchByID_Result>("NV_QuanLyThietBi_searchByID", iDQLTBParameter);
        }
    
        public virtual ObjectResult<NV_QuanLyThietBi_select_Result> NV_QuanLyThietBi_select(string search)
        {
            var searchParameter = search != null ?
                new ObjectParameter("search", search) :
                new ObjectParameter("search", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<NV_QuanLyThietBi_select_Result>("NV_QuanLyThietBi_select", searchParameter);
        }
    
        public virtual int NV_QuanLyThietBi_update(Nullable<int> iDQLTB, Nullable<int> iDPhongBan, string serviceTag, Nullable<int> iDTB, string maNV, string phone, Nullable<int> iDSC, Nullable<System.DateTime> ngayLap, string status, Nullable<System.DateTime> ngayXL, Nullable<System.DateTime> ngayHT, Nullable<System.DateTime> ngayNM, string ghiChu)
        {
            var iDQLTBParameter = iDQLTB.HasValue ?
                new ObjectParameter("IDQLTB", iDQLTB) :
                new ObjectParameter("IDQLTB", typeof(int));
    
            var iDPhongBanParameter = iDPhongBan.HasValue ?
                new ObjectParameter("IDPhongBan", iDPhongBan) :
                new ObjectParameter("IDPhongBan", typeof(int));
    
            var serviceTagParameter = serviceTag != null ?
                new ObjectParameter("ServiceTag", serviceTag) :
                new ObjectParameter("ServiceTag", typeof(string));
    
            var iDTBParameter = iDTB.HasValue ?
                new ObjectParameter("IDTB", iDTB) :
                new ObjectParameter("IDTB", typeof(int));
    
            var maNVParameter = maNV != null ?
                new ObjectParameter("MaNV", maNV) :
                new ObjectParameter("MaNV", typeof(string));
    
            var phoneParameter = phone != null ?
                new ObjectParameter("Phone", phone) :
                new ObjectParameter("Phone", typeof(string));
    
            var iDSCParameter = iDSC.HasValue ?
                new ObjectParameter("IDSC", iDSC) :
                new ObjectParameter("IDSC", typeof(int));
    
            var ngayLapParameter = ngayLap.HasValue ?
                new ObjectParameter("NgayLap", ngayLap) :
                new ObjectParameter("NgayLap", typeof(System.DateTime));
    
            var statusParameter = status != null ?
                new ObjectParameter("Status", status) :
                new ObjectParameter("Status", typeof(string));
    
            var ngayXLParameter = ngayXL.HasValue ?
                new ObjectParameter("NgayXL", ngayXL) :
                new ObjectParameter("NgayXL", typeof(System.DateTime));
    
            var ngayHTParameter = ngayHT.HasValue ?
                new ObjectParameter("NgayHT", ngayHT) :
                new ObjectParameter("NgayHT", typeof(System.DateTime));
    
            var ngayNMParameter = ngayNM.HasValue ?
                new ObjectParameter("NgayNM", ngayNM) :
                new ObjectParameter("NgayNM", typeof(System.DateTime));
    
            var ghiChuParameter = ghiChu != null ?
                new ObjectParameter("GhiChu", ghiChu) :
                new ObjectParameter("GhiChu", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_QuanLyThietBi_update", iDQLTBParameter, iDPhongBanParameter, serviceTagParameter, iDTBParameter, maNVParameter, phoneParameter, iDSCParameter, ngayLapParameter, statusParameter, ngayXLParameter, ngayHTParameter, ngayNMParameter, ghiChuParameter);
        }
    
        public virtual int NV_ThietBi_delete(Nullable<int> iDTB)
        {
            var iDTBParameter = iDTB.HasValue ?
                new ObjectParameter("IDTB", iDTB) :
                new ObjectParameter("IDTB", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_ThietBi_delete", iDTBParameter);
        }
    
        public virtual int NV_ThietBi_insert(string tenThietBi)
        {
            var tenThietBiParameter = tenThietBi != null ?
                new ObjectParameter("TenThietBi", tenThietBi) :
                new ObjectParameter("TenThietBi", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_ThietBi_insert", tenThietBiParameter);
        }
    
        public virtual int NV_ThietBi_update(Nullable<int> iDTB, string tenThietBi)
        {
            var iDTBParameter = iDTB.HasValue ?
                new ObjectParameter("IDTB", iDTB) :
                new ObjectParameter("IDTB", typeof(int));
    
            var tenThietBiParameter = tenThietBi != null ?
                new ObjectParameter("TenThietBi", tenThietBi) :
                new ObjectParameter("TenThietBi", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NV_ThietBi_update", iDTBParameter, tenThietBiParameter);
        }
    }
}
