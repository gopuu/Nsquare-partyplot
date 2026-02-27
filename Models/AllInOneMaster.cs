namespace ERPAdmin.Models
{
    public class AllInOneMaster
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Image { get; set; }
        public long Action_By { get; set; }
        public string? Link { get; set; }
    }
    public class SaveAllInOneMasterRequest : BaseClass
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Image { get; set; }
        public long Action_By { get; set; }
        public string? Link { get; set; }
    }

    public class DeleteAllInOneMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class GetAllInOneMasterRequest
    {
        public long Id { get; set; }
    }

    //All In One Link
    public class AllInOnePropertyResponse
    {
        public string? PropertyLogo { get; set; }
        public string? PropertyName { get; set; }
        public string? CompanyName { get; set; }
        public List<AllInOneLinkMaster>? Links { get; set; }
    }
    public class AllInOneLinkMaster
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public string? Link { get; set; }
        public string? Image { get; set; }
    }
}
