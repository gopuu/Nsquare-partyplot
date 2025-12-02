namespace ERPAdmin.Models
{
    public class GetCityList
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 StateId { get; set; }
        public string? StateName { get; set; }
        public Int64? Pincode { get; set; }
        //public bool IsActive { get; set; }
    }
    public class SaveCity : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 StateId { get; set; }
        public Int64 Pincode { get; set; }
        //public bool IsActive { get; set; }
    }
    public class GetCityRequest
    {
        public Int64 Id { get; set; }
        public Int64 StateId { get; set; }
    }
    public class DeleteCityRequest :  BaseClass
    {
        public Int64 Id { get; set; }
    }
}
