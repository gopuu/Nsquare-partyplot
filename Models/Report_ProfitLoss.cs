namespace ERPAdmin.Models
{
    public class ProfitLossGridRequest
    {
        public int YearId { get; set; }
        public int Year { get; set; }
        public int MonthId_From { get; set; }
        public int MonthId_To { get; set; }
    }

    public class ProfitLossReportRequest : BaseClass
    {
        public int YearId { get; set; }
        public int Year { get; set; }
        public int MonthId_From { get; set; }
        public int MonthId_To { get; set; }

        public bool? WithHeader { get; set; }
    }

    // shared for both grid and download
    public class ProfitLossLineItem
    {
        public long SrNo { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string? Nature { get; set; }
    }

    public class ProfitLossSummary
    {
        public decimal TotalSales { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalOtherIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
    }

    public class ProfitLossGridResponse
    {
        public List<ProfitLossLineItem> LineItems { get; set; } = new();
        public ProfitLossSummary Summary { get; set; } = new();
    }
}
