namespace ERPAdmin.Models
{
    public class FoodWastageGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class FoodWastageReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class FoodWastageGridModel
    {
        public long SrNo { get; set; }
        public long BookingId { get; set; }
        public string? EventDate { get; set; }
        public string? ClientName { get; set; }
        public string? MenuItemName { get; set; }
        public long MenuItemId { get; set; }
        public decimal WastageKg { get; set; }
        public int HasWastage { get; set; }
    }
}
