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
}
