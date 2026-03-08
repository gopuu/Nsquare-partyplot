namespace ERPAdmin.Models
{

    public class DailySalesDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class PaymentReceiptRequest : BaseClass
    {
        public long? PaymentId { get; set; }
        public long? BookingId { get; set; }
    }
    public class MonthlySalesDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }
    }
    public class RevenueLossRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class GSTDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class PurchaseDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class FoodWastageDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class ExpenseDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class IncomeDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class ProfitLossDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class CashflowDetailsRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
    public class ModificationRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }

    }
}
