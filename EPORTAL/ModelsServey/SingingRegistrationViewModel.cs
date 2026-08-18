using System.Collections.Generic;

namespace EPORTAL.ModelsServey
{
    public class SingingRegistrationViewModel
    {
        public int IDSV { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public string RegistrantCode { get; set; }
        public string RegistrantName { get; set; }
        public string RegistrantPhone { get; set; }
        public string RegistrantDepartment { get; set; }
        public int MaxSlots { get; set; }
        public int SlotsRemaining { get; set; }
        public List<SingingOptionViewModel> Options { get; set; }
        public List<SingingRegistrationItemViewModel> Registrations { get; set; }
    }

    public class SingingOptionViewModel
    {
        public int IDOT { get; set; }
        public int IDGroup { get; set; }
        public string Content { get; set; }
        public int? OrderBy { get; set; }
        public bool RequiresPartner { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class SingingRegistrationItemViewModel
    {
        public int CTKhaoSatID { get; set; }
        public int IDOT { get; set; }
        public string Content { get; set; }
        public bool RequiresPartner { get; set; }
        public int? PairID { get; set; }
        public bool CanManage { get; set; }
        public string RegistrationOwnerPhone { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerPhone { get; set; }
        public string PartnerDepartment { get; set; }
    }

    public class SingingRegistrationRequest
    {
        public int IDSV { get; set; }
        public string RegistrantPhone { get; set; }
        public List<int> SelectedOptionIds { get; set; }
        public int? PartnerID { get; set; }
    }
}
