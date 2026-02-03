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
    public class DashboardFeedbackRequest
    {
        public string? MonthDuration { get; set; }  // e.g., "ThisMonth", "LastMonth"
        public string? FromDate { get; set; }       // optional: "2026-01-01"
        public string? ToDate { get; set; }         // optional: "2026-01-24"
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
        public string FeedbackType { get; set; }
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
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

}
