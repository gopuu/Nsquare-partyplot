namespace ERPAdmin.Models
{
    //GET
    public class GetSettingsRequest
    {
        public long Id { get; set; } = 0;
    }
    public class GetSettingsResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    //SAVE
    public class SaveSettingsRequest : BaseClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    //DELETE
    public class DeleteSettingsRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
