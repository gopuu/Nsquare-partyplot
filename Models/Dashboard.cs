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

    #endregion

    #region Get Dashboard Widget Counts
    public class DashboardWidgetCountsResponse
    {
        public string? DashboardStats { get; set; }
        public string? Chart_CorporateVsFamily { get; set; }
        public string? Chart_CorporateSubTypes { get; set; }
        public string? Chart_FamilySubTypes { get; set; }
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
