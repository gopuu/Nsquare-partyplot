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

}
