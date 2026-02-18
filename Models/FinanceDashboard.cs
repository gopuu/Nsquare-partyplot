namespace ERPAdmin.Models
{
    public class FinanceDashboard
    {
    }

    public class DashboardFinanceSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal OutstandingReceivables { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
    }

    public class DashboardPaymentCollectionStatus
    {
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class UpcomingPaymentItem
    {
        public string? EventName { get; set; }
        public decimal Amount { get; set; }
    }

    public class VendorPayableItem
    {
        public string? VendorName { get; set; }
        public decimal PendingAmount { get; set; }
        public string? Status { get; set; }
    }

    public class TopRevenueEventItem
    {
        public string? EventName { get; set; }
        public decimal Amount { get; set; }
    }
}
