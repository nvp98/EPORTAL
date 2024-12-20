﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class PhanQuyenHTEntities : DbContext
    {
        public PhanQuyenHTEntities()
            : base("name=PhanQuyenHTEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<A_ListController> A_ListController { get; set; }
        public virtual DbSet<A_QuyenCN> A_QuyenCN { get; set; }
        public virtual DbSet<A_QuyenCT> A_QuyenCT { get; set; }
        public virtual DbSet<A_QuyenHT> A_QuyenHT { get; set; }
        public virtual DbSet<L_AuthorizationTV> L_AuthorizationTV { get; set; }
        public virtual DbSet<L_NhomThuVienFile> L_NhomThuVienFile { get; set; }
        public virtual DbSet<L_ThuVienFile> L_ThuVienFile { get; set; }
    
        public virtual ObjectResult<string> A_CheckListQuyen(Nullable<int> iDQuyenHT, string controller)
        {
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("A_CheckListQuyen", iDQuyenHTParameter, controllerParameter);
        }
    
        public virtual ObjectResult<string> A_CheckListQuyenActive(Nullable<int> iDQuyenHT, string controller)
        {
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("A_CheckListQuyenActive", iDQuyenHTParameter, controllerParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> A_CheckQuyen(Nullable<int> iDQuyenHT, string controller, string maQuyenCN)
        {
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            var maQuyenCNParameter = maQuyenCN != null ?
                new ObjectParameter("MaQuyenCN", maQuyenCN) :
                new ObjectParameter("MaQuyenCN", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("A_CheckQuyen", iDQuyenHTParameter, controllerParameter, maQuyenCNParameter);
        }
    
        public virtual int A_ListController_delete(Nullable<int> iDController)
        {
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_ListController_delete", iDControllerParameter);
        }
    
        public virtual int A_ListController_insert(string controller, string mota, Nullable<int> isActive, string dSQuyenCN)
        {
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            var motaParameter = mota != null ?
                new ObjectParameter("Mota", mota) :
                new ObjectParameter("Mota", typeof(string));
    
            var isActiveParameter = isActive.HasValue ?
                new ObjectParameter("isActive", isActive) :
                new ObjectParameter("isActive", typeof(int));
    
            var dSQuyenCNParameter = dSQuyenCN != null ?
                new ObjectParameter("DSQuyenCN", dSQuyenCN) :
                new ObjectParameter("DSQuyenCN", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_ListController_insert", controllerParameter, motaParameter, isActiveParameter, dSQuyenCNParameter);
        }
    
        public virtual ObjectResult<A_ListController_searchByID_Result> A_ListController_searchByID(Nullable<int> iDController)
        {
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<A_ListController_searchByID_Result>("A_ListController_searchByID", iDControllerParameter);
        }
    
        public virtual ObjectResult<A_ListController_select_Result> A_ListController_select(string search)
        {
            var searchParameter = search != null ?
                new ObjectParameter("search", search) :
                new ObjectParameter("search", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<A_ListController_select_Result>("A_ListController_select", searchParameter);
        }
    
        public virtual int A_ListController_update(Nullable<int> iDController, string controller, string mota, Nullable<int> isActive, string dSQuyenCN)
        {
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            var motaParameter = mota != null ?
                new ObjectParameter("Mota", mota) :
                new ObjectParameter("Mota", typeof(string));
    
            var isActiveParameter = isActive.HasValue ?
                new ObjectParameter("isActive", isActive) :
                new ObjectParameter("isActive", typeof(int));
    
            var dSQuyenCNParameter = dSQuyenCN != null ?
                new ObjectParameter("DSQuyenCN", dSQuyenCN) :
                new ObjectParameter("DSQuyenCN", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_ListController_update", iDControllerParameter, controllerParameter, motaParameter, isActiveParameter, dSQuyenCNParameter);
        }
    
        public virtual int A_Nhanvien_update_QuyenHT(string maNV, Nullable<int> iDQuyenHT)
        {
            var maNVParameter = maNV != null ?
                new ObjectParameter("MaNV", maNV) :
                new ObjectParameter("MaNV", typeof(string));
    
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_Nhanvien_update_QuyenHT", maNVParameter, iDQuyenHTParameter);
        }
    
        public virtual int A_QuyenCN_delete(Nullable<int> iDQuyenCN)
        {
            var iDQuyenCNParameter = iDQuyenCN.HasValue ?
                new ObjectParameter("IDQuyenCN", iDQuyenCN) :
                new ObjectParameter("IDQuyenCN", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCN_delete", iDQuyenCNParameter);
        }
    
        public virtual int A_QuyenCN_insert(string maQuyenCN, string tenQuyenCN)
        {
            var maQuyenCNParameter = maQuyenCN != null ?
                new ObjectParameter("MaQuyenCN", maQuyenCN) :
                new ObjectParameter("MaQuyenCN", typeof(string));
    
            var tenQuyenCNParameter = tenQuyenCN != null ?
                new ObjectParameter("TenQuyenCN", tenQuyenCN) :
                new ObjectParameter("TenQuyenCN", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCN_insert", maQuyenCNParameter, tenQuyenCNParameter);
        }
    
        public virtual int A_QuyenCN_update(Nullable<int> iDQuyenCN, string maQuyenCN, string tenQuyenCN)
        {
            var iDQuyenCNParameter = iDQuyenCN.HasValue ?
                new ObjectParameter("IDQuyenCN", iDQuyenCN) :
                new ObjectParameter("IDQuyenCN", typeof(int));
    
            var maQuyenCNParameter = maQuyenCN != null ?
                new ObjectParameter("MaQuyenCN", maQuyenCN) :
                new ObjectParameter("MaQuyenCN", typeof(string));
    
            var tenQuyenCNParameter = tenQuyenCN != null ?
                new ObjectParameter("TenQuyenCN", tenQuyenCN) :
                new ObjectParameter("TenQuyenCN", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCN_update", iDQuyenCNParameter, maQuyenCNParameter, tenQuyenCNParameter);
        }
    
        public virtual int A_QuyenCT_delete(Nullable<int> iDQuyenCT)
        {
            var iDQuyenCTParameter = iDQuyenCT.HasValue ?
                new ObjectParameter("IDQuyenCT", iDQuyenCT) :
                new ObjectParameter("IDQuyenCT", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCT_delete", iDQuyenCTParameter);
        }
    
        public virtual int A_QuyenCT_deleteCN(Nullable<int> iDQuyenHT, Nullable<int> iDController, Nullable<int> iDQuyenCN)
        {
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            var iDQuyenCNParameter = iDQuyenCN.HasValue ?
                new ObjectParameter("IDQuyenCN", iDQuyenCN) :
                new ObjectParameter("IDQuyenCN", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCT_deleteCN", iDQuyenHTParameter, iDControllerParameter, iDQuyenCNParameter);
        }
    
        public virtual int A_QuyenCT_deleteController(Nullable<int> iDController)
        {
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCT_deleteController", iDControllerParameter);
        }
    
        public virtual int A_QuyenCT_insert(Nullable<int> iDQuyenHT, Nullable<int> iDController, Nullable<int> iDQuyenCN, Nullable<int> isActive)
        {
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            var iDQuyenCNParameter = iDQuyenCN.HasValue ?
                new ObjectParameter("IDQuyenCN", iDQuyenCN) :
                new ObjectParameter("IDQuyenCN", typeof(int));
    
            var isActiveParameter = isActive.HasValue ?
                new ObjectParameter("isActive", isActive) :
                new ObjectParameter("isActive", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCT_insert", iDQuyenHTParameter, iDControllerParameter, iDQuyenCNParameter, isActiveParameter);
        }
    
        public virtual int A_QuyenCT_isActive(Nullable<int> iDController, Nullable<int> iDQuyenCN, Nullable<int> isActive)
        {
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            var iDQuyenCNParameter = iDQuyenCN.HasValue ?
                new ObjectParameter("IDQuyenCN", iDQuyenCN) :
                new ObjectParameter("IDQuyenCN", typeof(int));
    
            var isActiveParameter = isActive.HasValue ?
                new ObjectParameter("isActive", isActive) :
                new ObjectParameter("isActive", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCT_isActive", iDControllerParameter, iDQuyenCNParameter, isActiveParameter);
        }
    
        public virtual ObjectResult<A_QuyenCT_searchByID_Result> A_QuyenCT_searchByID(Nullable<int> iDQuyenCT)
        {
            var iDQuyenCTParameter = iDQuyenCT.HasValue ?
                new ObjectParameter("IDQuyenCT", iDQuyenCT) :
                new ObjectParameter("IDQuyenCT", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<A_QuyenCT_searchByID_Result>("A_QuyenCT_searchByID", iDQuyenCTParameter);
        }
    
        public virtual int A_QuyenCT_update(Nullable<int> iDQuyenCT, Nullable<int> iDQuyenHT, Nullable<int> iDController, Nullable<int> iDQuyenCN, Nullable<int> isActive)
        {
            var iDQuyenCTParameter = iDQuyenCT.HasValue ?
                new ObjectParameter("IDQuyenCT", iDQuyenCT) :
                new ObjectParameter("IDQuyenCT", typeof(int));
    
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            var iDControllerParameter = iDController.HasValue ?
                new ObjectParameter("IDController", iDController) :
                new ObjectParameter("IDController", typeof(int));
    
            var iDQuyenCNParameter = iDQuyenCN.HasValue ?
                new ObjectParameter("IDQuyenCN", iDQuyenCN) :
                new ObjectParameter("IDQuyenCN", typeof(int));
    
            var isActiveParameter = isActive.HasValue ?
                new ObjectParameter("isActive", isActive) :
                new ObjectParameter("isActive", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenCT_update", iDQuyenCTParameter, iDQuyenHTParameter, iDControllerParameter, iDQuyenCNParameter, isActiveParameter);
        }
    
        public virtual int A_QuyenHT_delete(Nullable<int> iDQuyenHT)
        {
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenHT_delete", iDQuyenHTParameter);
        }
    
        public virtual int A_QuyenHT_insert(string tenQuyenHT)
        {
            var tenQuyenHTParameter = tenQuyenHT != null ?
                new ObjectParameter("TenQuyenHT", tenQuyenHT) :
                new ObjectParameter("TenQuyenHT", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenHT_insert", tenQuyenHTParameter);
        }
    
        public virtual int A_QuyenHT_update(Nullable<int> iDQuyenHT, string tenQuyenHT)
        {
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            var tenQuyenHTParameter = tenQuyenHT != null ?
                new ObjectParameter("TenQuyenHT", tenQuyenHT) :
                new ObjectParameter("TenQuyenHT", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_QuyenHT_update", iDQuyenHTParameter, tenQuyenHTParameter);
        }
    
        public virtual ObjectResult<A_TaiKhoan_ID_Result> A_TaiKhoan_ID(Nullable<int> iD)
        {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<A_TaiKhoan_ID_Result>("A_TaiKhoan_ID", iDParameter);
        }
    
        public virtual ObjectResult<A_TaiKhoan_select_Result> A_TaiKhoan_select(string search)
        {
            var searchParameter = search != null ?
                new ObjectParameter("search", search) :
                new ObjectParameter("search", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<A_TaiKhoan_select_Result>("A_TaiKhoan_select", searchParameter);
        }
    
        public virtual int A_TK_Update_GroupQuyen(Nullable<int> iD, string groupQuyen)
        {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));
    
            var groupQuyenParameter = groupQuyen != null ?
                new ObjectParameter("GroupQuyen", groupQuyen) :
                new ObjectParameter("GroupQuyen", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_TK_Update_GroupQuyen", iDParameter, groupQuyenParameter);
        }
    
        public virtual int A_TK_Update_Quyen(Nullable<int> iD, Nullable<int> iDQuyenHT)
        {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));
    
            var iDQuyenHTParameter = iDQuyenHT.HasValue ?
                new ObjectParameter("IDQuyenHT", iDQuyenHT) :
                new ObjectParameter("IDQuyenHT", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("A_TK_Update_Quyen", iDParameter, iDQuyenHTParameter);
        }
    
        public virtual int L_AuthorizationTV_delete(Nullable<int> iD)
        {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_AuthorizationTV_delete", iDParameter);
        }
    
        public virtual int L_AuthorizationTV_insert(Nullable<int> nhanVienID, Nullable<System.DateTime> createdate, Nullable<int> iDThuVien)
        {
            var nhanVienIDParameter = nhanVienID.HasValue ?
                new ObjectParameter("NhanVienID", nhanVienID) :
                new ObjectParameter("NhanVienID", typeof(int));
    
            var createdateParameter = createdate.HasValue ?
                new ObjectParameter("Createdate", createdate) :
                new ObjectParameter("Createdate", typeof(System.DateTime));
    
            var iDThuVienParameter = iDThuVien.HasValue ?
                new ObjectParameter("IDThuVien", iDThuVien) :
                new ObjectParameter("IDThuVien", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_AuthorizationTV_insert", nhanVienIDParameter, createdateParameter, iDThuVienParameter);
        }
    
        public virtual ObjectResult<L_AuthorizationTV_searchByIDTV_Result> L_AuthorizationTV_searchByIDTV(Nullable<int> iDThuVien)
        {
            var iDThuVienParameter = iDThuVien.HasValue ?
                new ObjectParameter("IDThuVien", iDThuVien) :
                new ObjectParameter("IDThuVien", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<L_AuthorizationTV_searchByIDTV_Result>("L_AuthorizationTV_searchByIDTV", iDThuVienParameter);
        }
    
        public virtual int L_NhomThuVienFile_delete(Nullable<int> iDNhom)
        {
            var iDNhomParameter = iDNhom.HasValue ?
                new ObjectParameter("IDNhom", iDNhom) :
                new ObjectParameter("IDNhom", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_NhomThuVienFile_delete", iDNhomParameter);
        }
    
        public virtual int L_NhomThuVienFile_insert(string tenNhomTV)
        {
            var tenNhomTVParameter = tenNhomTV != null ?
                new ObjectParameter("TenNhomTV", tenNhomTV) :
                new ObjectParameter("TenNhomTV", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_NhomThuVienFile_insert", tenNhomTVParameter);
        }
    
        public virtual int L_NhomThuVienFile_update(Nullable<int> iDNhom, string tenNhomTV)
        {
            var iDNhomParameter = iDNhom.HasValue ?
                new ObjectParameter("IDNhom", iDNhom) :
                new ObjectParameter("IDNhom", typeof(int));
    
            var tenNhomTVParameter = tenNhomTV != null ?
                new ObjectParameter("TenNhomTV", tenNhomTV) :
                new ObjectParameter("TenNhomTV", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_NhomThuVienFile_update", iDNhomParameter, tenNhomTVParameter);
        }
    
        public virtual int L_ThuVienFile_delete(Nullable<int> iD)
        {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_ThuVienFile_delete", iDParameter);
        }
    
        public virtual int L_ThuVienFile_insert(string tenTaiLieu, string fileName, string ghiChu, Nullable<int> iDNhom, Nullable<System.DateTime> createdate)
        {
            var tenTaiLieuParameter = tenTaiLieu != null ?
                new ObjectParameter("TenTaiLieu", tenTaiLieu) :
                new ObjectParameter("TenTaiLieu", typeof(string));
    
            var fileNameParameter = fileName != null ?
                new ObjectParameter("FileName", fileName) :
                new ObjectParameter("FileName", typeof(string));
    
            var ghiChuParameter = ghiChu != null ?
                new ObjectParameter("GhiChu", ghiChu) :
                new ObjectParameter("GhiChu", typeof(string));
    
            var iDNhomParameter = iDNhom.HasValue ?
                new ObjectParameter("IDNhom", iDNhom) :
                new ObjectParameter("IDNhom", typeof(int));
    
            var createdateParameter = createdate.HasValue ?
                new ObjectParameter("Createdate", createdate) :
                new ObjectParameter("Createdate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_ThuVienFile_insert", tenTaiLieuParameter, fileNameParameter, ghiChuParameter, iDNhomParameter, createdateParameter);
        }
    
        public virtual ObjectResult<L_ThuVienFile_searchByID_Result> L_ThuVienFile_searchByID(Nullable<int> iD)
        {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<L_ThuVienFile_searchByID_Result>("L_ThuVienFile_searchByID", iDParameter);
        }
    
        public virtual ObjectResult<L_ThuVienFile_select_Result> L_ThuVienFile_select(string search)
        {
            var searchParameter = search != null ?
                new ObjectParameter("search", search) :
                new ObjectParameter("search", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<L_ThuVienFile_select_Result>("L_ThuVienFile_select", searchParameter);
        }
    
        public virtual ObjectResult<L_ThuVienFile_selectbyUser_Result> L_ThuVienFile_selectbyUser(string search, Nullable<int> nhanVienID)
        {
            var searchParameter = search != null ?
                new ObjectParameter("search", search) :
                new ObjectParameter("search", typeof(string));
    
            var nhanVienIDParameter = nhanVienID.HasValue ?
                new ObjectParameter("NhanVienID", nhanVienID) :
                new ObjectParameter("NhanVienID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<L_ThuVienFile_selectbyUser_Result>("L_ThuVienFile_selectbyUser", searchParameter, nhanVienIDParameter);
        }
    
        public virtual int L_ThuVienFile_update(Nullable<int> iD, string tenTaiLieu, string fileName, string ghiChu, Nullable<int> iDNhom)
        {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));
    
            var tenTaiLieuParameter = tenTaiLieu != null ?
                new ObjectParameter("TenTaiLieu", tenTaiLieu) :
                new ObjectParameter("TenTaiLieu", typeof(string));
    
            var fileNameParameter = fileName != null ?
                new ObjectParameter("FileName", fileName) :
                new ObjectParameter("FileName", typeof(string));
    
            var ghiChuParameter = ghiChu != null ?
                new ObjectParameter("GhiChu", ghiChu) :
                new ObjectParameter("GhiChu", typeof(string));
    
            var iDNhomParameter = iDNhom.HasValue ?
                new ObjectParameter("IDNhom", iDNhom) :
                new ObjectParameter("IDNhom", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("L_ThuVienFile_update", iDParameter, tenTaiLieuParameter, fileNameParameter, ghiChuParameter, iDNhomParameter);
        }
    
        public virtual ObjectResult<Account_select_Result> Account_select(string search)
        {
            var searchParameter = search != null ?
                new ObjectParameter("search", search) :
                new ObjectParameter("search", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Account_select_Result>("Account_select", searchParameter);
        }
    }
}
