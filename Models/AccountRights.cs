namespace ERPAdmin.Models
{
    //Get Account Rights Request
    public class GetAccountRightsRequest
    {
        // JSON array → [1,2,3]
        public List<long> StaffIds { get; set; }
    }

    //Save Account Rights Request
    public class SaveAccountRightsRequest : BaseClass
    {
        // JSON array → [1,2,3]
        public List<long> StaffIds { get; set; }

        public List<AccountRightsModel> AccountRights { get; set; }
    }

    //Account Rights Model
    public class AccountRightsModel
    {
        public long AccountId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
        public bool CanTransfer { get; set; }
    }
    public class AccountRightsModel_Response
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
        public bool CanTransfer { get; set; }
    }


}
