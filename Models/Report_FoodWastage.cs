namespace ERPAdmin.Models
{
    //GRID REQUEST
    public class Report_FoodWastageGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class Report_FoodWastageGridRequest_PdfExcel : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }
    }
    //GRID RESPONSE
    public class Report_FoodWastageGridModel
    {
        public long SrNo { get; set; }
        public long BookingId { get; set; }
        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }
        public string? ShiftCode { get; set; }
        public string? ClientName { get; set; }
        public decimal WastageKg { get; set; }
    }

    //DETAILS REQUEST
    public class Report_FoodWastageDetailsRequest
    {
        public long BookingId { get; set; }
    }
    public class Report_FoodWastageDetailsRequest_PdfExcel : BaseClass
    {
        public long BookingId { get; set; }
        public bool? WithHeader { get; set; }
    }
    //DETAILS RESPONSE
    public class Report_FoodWastageDetailsModel
    {
        public long BookingId { get; set; }
        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }
        public string? ShiftCode { get; set; }
        public string? ClientName { get; set; }
        public List<Report_FoodWastageDetailsModel_Items>? Items { get; set; }
    }
    //DETAILS RESPONSE - items
    public class Report_FoodWastageDetailsModel_Items
    {
        public long SrNo { get; set; }
        public string? MenuItemName { get; set; }
        public long? MenuItemId { get; set; }
        public decimal WastageKg { get; set; }
    }

}
