namespace ERPAdmin.Models
{
    //GET request
    public class GetHallBlockRequest
    {
        public long Id { get; set; }
    }
    //Get response
    public class GetHallBlockResponse
    {
        public long Id { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FromDate2 { get; set; }
        public string? ToDate2 { get; set; }
        public string? Reason { get; set; }
        public string? Remarks { get; set; }
        public List<HallBlockDetails>? HallListJson { get; set; }
    }
    public class HallBlockDetails
    {
        public long Id { get; set; }
        public long BlockMasterId { get; set; }
        public long HallId { get; set; }
        public string? HallName { get; set; }
    }

    //Save
    public class SaveHallBlockRequest : BaseClass
    {
        public long Id { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Reason { get; set; }
        public string? Remarks { get; set; }
        public string? HallBlockDetailsJson { get; set; }
    }
    //Delete
    public class DeleteHallBlockRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
