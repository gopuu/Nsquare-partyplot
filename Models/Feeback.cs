namespace ERPAdmin.Models
{
    //SEND
    public class SendFeedbackRequest : BaseClass
    {
        public long? BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? WhatsappNo { get; set; }
    }

    //Dashboard Rating Overtime
    public class FeedbackRatingOverTimeRequest
    {
        public string FromDate { get; set; }   // "2025-01-01"
        public string ToDate { get; set; }     // "2025-01-31"
    }
    public class DashboardFeedbackRatingOverTimeResponse
    {
        public List<RatingOverTimeModel> RatingsOverTime { get; set; }
    }
    public class RatingOverTimeModel
    {
        public int RatingValue { get; set; }
        public string RatingLabel { get; set; }
        public string IconUrl { get; set; }
        public List<RatingOverTimeItem> Data { get; set; }
    }
    public class RatingOverTimeItem
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }



    //Dashboard rating breakdown
    public class DashboardRatingBreakdownResponse
    {
        public List<RatingBreakdownModel>? RatingBreakdown { get; set; }
    }
    public class RatingBreakdownModel
    {
        public int RatingValue { get; set; }
        public string? RatingLabel { get; set; }
        public string? IconUrl { get; set; }
        public int Count { get; set; }
        public string? Percentage { get; set; }
    }

    //Dashboard count
    public class FeedbackDashboardResponse
    {
        public long TotalFeedback { get; set; }
        public string AvgRatingOutOf5 { get; set; }
        public FeedbackSummaryReport Summary { get; set; }
        public List<CategoryRatingModel> CategoryRatings { get; set; }
    }
    public class FeedbackSummaryReport
    {
        public string Positive { get; set; }
        public string Neutral { get; set; }
        public string Negative { get; set; }
    }
    public class CategoryRatingModel
    {
        public string CategoryName { get; set; }
        public string AvgRating { get; set; }
        public int PositiveCount { get; set; }
        public string PositiveIcon { get; set; }
        public int NeutralCount { get; set; }
        public string NeutralIcon { get; set; }
        public int NegativeCount { get; set; }
        public string NegativeIcon { get; set; }
    }


    //SAVE
    public class SaveFeedbackRequest
    {
        public long? BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? WhatsappNo { get; set; }
        public List<FeedbackResponseRequest>? FeedbackResponseJSON { get; set; }
    }

    public class FeedbackResponseRequest
    {
        public long StepId { get; set; }
        public long? OptionId { get; set; }
        public string? TextResponse { get; set; }
    }
    public class GetFeedbackRequest
    {
        public long? FeedbackId { get; set; }
    }

    // Models for Feedback Form Data
    public class FeedbackOptionModel
    {
        public long OptionId { get; set; }
        public string? OptionName { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public string? HiddenMessage { get; set; }
    }
    public class FeedbackStepModel
    {
        public long StepId { get; set; }
        public string? StepName { get; set; }
        public string? StepTitle { get; set; }
        public string? StepDescription { get; set; }
        public int StepOrder { get; set; }
        public bool IsMultiSelect { get; set; }
        public bool HasTextBox { get; set; }
        public List<FeedbackOptionModel> Options { get; set; }
    }
    public class FeedbackData
    {
        public string CustomerName { get; set; }
        public string WhatsappNo { get; set; }
        public string Booking_SubEventName { get; set; }
        public string Created_At_Format { get; set; }
        public List<FeedbackResponse> Responses { get; set; }
    }

    public class FeedbackResponse
    {
        public long ResponseId { get; set; }
        public long StepId { get; set; }
        public long? OptionId { get; set; }
        public string TextResponse { get; set; }
        public string? Created_At { get; set; }
    }


}
