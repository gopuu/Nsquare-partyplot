namespace ERPAdmin.Models
{
    #region GetLeadSourceStatistics
    public class LeadSourceStatisticsRequest
    {
        public int? YearId { get; set; }
        public int? MonthId { get; set; }
        public bool IncludeAmount { get; set; }
        public long? LeadReferenceId { get; set; }
    }
    public class LeadSourceStatisticsResponse
    {
        public long LeadSourceId { get; set; }
        public string? LeadSourceName { get; set; }
        public int BookingCount { get; set; }
        public decimal? TotalAmount { get; set; }
    }
    #endregion GetLeadSourceStatistics

    #region Get Overall Booking Statistics
    public class OverallBookingStatsRequest
    {
        public int YearId { get; set; }
        public int? MonthId { get; set; }
        public bool BookingQty { get; set; }
        public bool BookingAmount { get; set; }
        public long? EventTypeId { get; set; }
    }
    public class OverallBookingStatsResponse
    {
        public int MonthNo { get; set; }
        public string? MonthName { get; set; }
        public int? BookingCount { get; set; }
        public decimal? BookingAmount { get; set; }
    }

    public class DashboardSummaryVM
    {
        public decimal TotalSales { get; set; }
        public int TotalEvents { get; set; }
        public int TotalCustomerVisits { get; set; }
    }

    public class DashboardSummaryRequest
    {
        public int? Month { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    public class DashboardWidgetCountsRequest
    {
        public string? MonthDuration { get; set; }
        public string? FromDate { get; set; }

        public string? ToDate { get; set; }

    }

    public class EventTypePieChart
    {
        public string? EventLabel { get; set; }
        public int EventCount { get; set; }
    }

    public class LeadSourcePieChart
    {
        public string? LeadSourceName { get; set; }
        public int LeadSourceCount { get; set; }
    }

    public class DashboardEventSummaryResponse
    {
        public string? MonthName { get; set; }
        public int Year { get; set; }
        public int MonthNo { get; set; }

        public int TotalEvents { get; set; }
        public int TotalFamilyFunctionCount { get; set; }
        public int TotalCorporateCount { get; set; }

        public int BookingQty { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DashboardEventSummaryRequest
    {
        public int? YearId { get; set; }   // ex: 2026
    }


    #endregion

    #region Comparative insights
    public class MonthlyPnLChartRequest
    {
        public string Filter { get; set; } // last3months, last6months, etc.
    }
    public class MonthlyPnLChartResponse
    {
        public string MonthKey { get; set; }

        public decimal Revenue { get; set; }
        public decimal Purchase { get; set; }
        public decimal Salary { get; set; }
        public decimal Rent { get; set; }
        public decimal Electricity { get; set; }
        public decimal Gas { get; set; }
        public decimal OtherExpense { get; set; }

        public decimal ProfitAndLoss { get; set; }

        public decimal PurchasePercent { get; set; }
        public decimal SalaryPercent { get; set; }
        public decimal RentPercent { get; set; }
        public decimal ElectricityPercent { get; set; }
        public decimal GasPercent { get; set; }
        public decimal OtherExpensePercent { get; set; }
        public decimal ProfitPercent { get; set; }
    }

    #endregion


    #region Get Dashboard Widget Counts
    public class DashboardWidgetCountsResponse
    {
        public string? DashboardStats { get; set; }
        public string? Chart_CorporateVsFamily { get; set; }
        public string? Chart_CorporateSubTypes { get; set; }
        public string? Chart_FamilySubTypes { get; set; }
    }

    public class DashboardWidgetCounts
    {
        public decimal? TotalSales { get; set; }
        public long? TotalCustomer { get; set; }
        public long? TotalEvents { get; set; }
        public long? EventLostCount { get; set; }
        public decimal? TotalEventsLost { get; set; }
        public long? UpcomingEventsCount { get; set; }
        public decimal? UpcomingEventsAmount { get; set; }
    }
    #endregion


    #region daily summary
    public class GetDailyReportSummaryRequest
    {
        public string? ReportDate { get; set; }
    }
    public class GetDailyReportSummaryResponse
    {
        public string ReportDate { get; set; }

        public int TotalEvents_Today { get; set; }
        public int TotalEvents_TillDate { get; set; }

        public decimal Revenue_Today { get; set; }
        public decimal Revenue_TillDate { get; set; }

        public int GuestsServed_Today { get; set; }
        public int GuestsServed_TillDate { get; set; }

        public int Enquiries_Today { get; set; }
        public int Enquiries_TillDate { get; set; }

        public int UpcomingEvents_Count { get; set; }
        public decimal ExpectedMonthlyRevenue { get; set; }

        public decimal RawMaterialExpense_Today { get; set; }
        public decimal RawMaterialExpense_TillDate { get; set; }

        public decimal TotalExpense_Today { get; set; }
        public decimal TotalExpense_TillDate { get; set; }
    }

    #endregion

    /*
    public class ProcessDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int TotalDeals { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CreatedDate { get; set; }
        public List<DealDto> Deals { get; set; }
    }

    public class DealDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Customer { get; set; }
        public decimal Amount { get; set; }
        public string? Date { get; set; }
    }
    */

}
