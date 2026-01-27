namespace ERPAdmin.Models
{
    public class BookingReportRequest
    {
        public long EventTypeId { get; set; } = 0;
        public long EventSubTypeId { get; set; } = 0;
        public long HallId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }

    public class DailySalesDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class PaymentReceiptRequest : BaseClass
    {
        public long? PaymentId { get; set; }
        public long? BookingId { get; set; }
    }
    public class MonthlySalesDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class RevenueLossRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class GSTDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class PurchaseDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }
    public class FoodWastageDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class ExpenseDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class ProfitLossDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class CashflowDetailsRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

}
