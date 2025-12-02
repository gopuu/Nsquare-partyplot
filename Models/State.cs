namespace ERPAdmin.Models
{
    public class GetStateRequest
    {
        public Int64 Id { get; set; }
    }

    public class State
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        //public bool? IsActive { get; set; }
    }
    public class SaveState : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        //public bool? IsActive { get; set; }
    }
    public class DeleteState : BaseClass
    {
        public Int64 Id { get; set; }
    }

}

