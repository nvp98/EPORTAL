using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDoToChuc.Models
{
    public class Home
    {
    }
    public class TotalList
    {
        public int TPTotal { get; set; }
        public int QDTotal { get; set; }
        public int PTTotal { get; set; }
        public int TPKipTotal { get; set; }
        public int KTVTotal { get; set; }
        public int TTTPTotal { get; set; }
        public int NVTotal { get; set; }
        public int PBGPMB { get; set; }
        public int NVVPC { get; set; }
    }
    public class dataBGD
    {
        public string HoTen { get; set; }
        public string TenViTri { get; set; }
        public string ImagePath { get; set; }
        public int TT_BGD { get; set; }
        
    }
    public class dataBP
    {
        public int tongns { get; set; }
        public string tenpb { get; set; }
        public List<dataBGD> listtp { get; set; }
        public List<dsnhom>  listnhom { get; set; }
        public List<dspx> listpx { get; set; }
        public int slqdpb { get; set; }
        public int sltptpb { get; set; }
        public int sltpkpb { get; set; }
        public int nvkpvc { get; set; }
    }
    public class dsnhom
    {
        public string TenNhom { get; set; }
        public List<dataBGD> listpt { get; set; }
        public int slpt { get; set; }
        public int slktv { get; set; }
        public int sltpk { get; set; }
        public int slnv { get; set; }
        public int TPT { get; set; }
        public int IDnhom { get; set; }
        public List<dataBGD> listtpt { get; set; }
        public int tongns { get; set; }
        public List<nhomchitiet> listchitiet { get; set; }
        public List<nhomchitiet> listto { get; set; }
    }
    public class nhomchitiet
    {
        public string TenNhom { get; set; }
        public int slpt { get; set; }
        public int slktv { get; set; }
        public int slnv { get; set; }
        public int sltttp { get; set; }
        public int tongns { get; set; }
        public List<dataBGD> listpt { get; set; }
    }
    public class dspx
    {
        public string TenPhanXuong { get; set; }
        public int slqd { get; set; }
        public int slpt { get; set; }
        public int sltpk { get; set; }
        public int slktv { get; set; }
        public int sltttp { get; set; }
        public int slnv { get; set; }
        public List<dataBGD> listqd { get; set; }
        public int tongns { get; set; }
    }
}