namespace ERPAdmin.Models
{
    public class Designation 
    {
        public Int64 id { get; set; }
        public string? name { get; set; }
        public string? band { get; set; }
        public string? code { get; set; }
    }
    public class DesignationForDropdown 
    {
        public Int64 id { get; set; }
        public string? name { get; set; }
    }
}
