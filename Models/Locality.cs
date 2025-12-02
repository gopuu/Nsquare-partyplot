namespace ERPAdmin.Models
{
    public class Locality
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 CityId { get; set; }
        public string? CityName { get; set; }
    }
    public class SaveLocality: BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 CityId { get; set; }
    }
    public class GetLocalityList
    {
        public Int64 Id { get; set; }
        public Int64 CityId { get; set; }
    }
}
