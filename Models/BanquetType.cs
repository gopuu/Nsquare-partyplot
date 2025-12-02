namespace ERPAdmin.Models
{
    public class BanquetType
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class SaveBanquetType : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
}
