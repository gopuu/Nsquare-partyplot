namespace ERPAdmin.Models
{
    public class BanquetHall
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class SaveBanquetHall : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class DeleteBanquetHall : BaseClass
    {
        public Int64 Id { get; set; }
    }
}
