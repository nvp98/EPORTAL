using System.Collections.Generic;

namespace EPORTAL.ModelsServey
{
    public class PickleballGroupView
    {
        public int IDGroup { get; set; }
        public int IDSV { get; set; }
        public string TenNhom { get; set; }
        public string LoaiDoi { get; set; }  // DoiNam | DoiNu | HonHop | HonHopTrinhCao | TeamDongDoi | HonHopNam (legacy)
        public bool IsExperienceQuestion { get; set; }
        public bool IsTournamentQuestion { get; set; }
        public bool IsPhoneQuestion { get; set; }
        public int? SelectedOptionId { get; set; }
        public string TextAnswer { get; set; }
        public bool IsRegistered { get; set; }
        public PartTogetherValidation ExistingPair { get; set; }
        public List<OptionValidation> Options { get; set; }
    }
}
