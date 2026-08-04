using System.Collections.Generic;

namespace EPORTAL.ModelsServey
{
    public class PickleballGroupView
    {
        public int IDGroup { get; set; }
        public int IDSV { get; set; }
        public string TenNhom { get; set; }
        public string LoaiDoi { get; set; }  // DoiNam | DoiNu | HonHop | HonHopTrinhCao | TeamDongDoi | HonHopNam (legacy)
        public bool IsRegistered { get; set; }
        public PartTogetherValidation ExistingPair { get; set; }
        public List<OptionValidation> Options { get; set; }
    }
}
