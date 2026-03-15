namespace ERPAdmin.Models
{
    public class FeedbackDashboard
    {
    }

    public class FeedbackInsightResponse
    {
        public List<MetricCard> MetricCards { get; set; }
        public SentimentSplit Sentiment { get; set; }
    }

    public class MetricCard
    {
        public string? MetricTitle { get; set; }
        public string? MetricValue { get; set; }
        public string? MetricKey { get; set; }
    }

    public class SentimentSplit
    {
        public int Positive { get; set; }
        public int Negative { get; set; }
        public int Neutral { get; set; }
    }

    public class RatingDistribution
    {
        public string? RatingLabel { get; set; }
        public int RatingValue { get; set; }
        public int RatingCount { get; set; }
        public decimal RatingPercent { get; set; }
    }

    public class CategoryRatingSummary
    {
        public string? CategoryName { get; set; }
        public decimal AvgRating { get; set; }
        public int PositiveCount { get; set; }
        public int NeutralCount { get; set; }
        public int NegativeCount { get; set; }
    }

    public class RatingBreakdownResponse
    {
        public List<RatingBreakdownItem> RatingBreakdown { get; set; }
    }

    public class RatingBreakdownItem
    {
        public int RatingValue { get; set; }
        public string? RatingLabel { get; set; }
        public string? IconUrl { get; set; }
        public int Count { get; set; }
        public string? Percentage { get; set; }
    }
    public class Dashbaord_Inventory_SupplierItems_Request
    {
        public long? SupplierId { get; set; }
    }

    public class DashboardFeedbackRequest
    {
        public string? MonthDuration { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    public class DashboardFeedbackCounts
    {
        public int TotalFeedback { get; set; }
        public string? AvgRatingOutOf5 { get; set; }
        public FeedbackReport JSON_FeedbackReport { get; set; }
        public List<CategoryRating> JSON_CategoryWiseRating { get; set; }
    }

    public class FeedbackReport
    {
        public string? Positive { get; set; }
        public string? Neutral { get; set; }
        public string? Negative { get; set; }
    }

    public class DashboardEventInsightResponse
    {
        public decimal? AvgbillingValue { get; set; }
        public decimal? CorporateEventValue { get; set; }
        public decimal? Familyeventbillingvalue { get; set; }
        public decimal? RevenueDistributionValue { get; set; }

        public string? AvgbillingText { get; set; }
        public string? CorporateEventText { get; set; }
        public string? FamilyeventbillingText { get; set; }
        public string? RevenueDistributionText { get; set; }


    }

    public class CategoryRating
    {
        public string? CategoryName { get; set; }
        public string? AvgRating { get; set; }
        public int PositiveCount { get; set; }
        public string? PositiveIcon { get; set; }
        public int NeutralCount { get; set; }
        public string? NeutralIcon { get; set; }
        public int NegativeCount { get; set; }
        public string? NegativeIcon { get; set; }
    }
    public class DashboardFeedbackCardResponse
    {
        public string? Title { get; set; }
        public int TotalFeedbacks { get; set; }
        public decimal AverageRating { get; set; }
        public decimal NegativeFeedbackPercent { get; set; }
        public string? FeedbackType { get; set; }
    }

    public class RatingsOverTimeResponse
    {
        public List<RatingsOverTimeItem> RatingsOverTime { get; set; }
    }

    public class RatingsOverTimeItem
    {
        public int RatingValue { get; set; }
        public string? RatingLabel { get; set; }
        public string? IconUrl { get; set; }
        public List<RatingsOverTimePoint> Data { get; set; }
    }
    public class RatingsOverTimePoint
    {
        public string? Date { get; set; }
        public int Count { get; set; }
    }

    //InventoryDashboardStockOutSummary -- START
    public class InventoryDashboardStockOutSummary
    {
        public long RawMaterialId { get; set; }
        public string? RawMaterialName { get; set; }
        public int StockOutCount { get; set; }
        public decimal TotalStockOutQty { get; set; }
    }

    public class DashboardInventoryStockOutRequest
    {
        public string? MonthDuration { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public long RawMaterialId { get; set; }
    }
    //InventoryDashboardStockOutSummary -- END

}
