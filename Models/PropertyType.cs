namespace ERPAdmin.Models
{
    public class PropertyType
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class SavePropertyType : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
}
