namespace ERPAdmin.Models
{
    public class FollowUpTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class FollowUpStatusResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class MoodResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class LeadLostCategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class LeadLostReasonRequest {
        public int? LeadLostCategoryId { get; set; } = 0;

    }
    public class LeadLostReasonResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LeadLostCategoryId { get; set; }
        public string LeadLostCategoryName { get; set; }
    }

    public class GetEnquiryFollowUpRequest
    {
        public long? FollowUpId { get; set; }     // 0 = get all by enquiry
        public long? EnquiryId { get; set; }      // required to fetch list
    }

    //Get followup data
    public class FollowUpDataResponse
    {
        public EnquiryDetailsModel? Enquiry { get; set; }
        public List<EnquiryFollowUpResponse>? FollowUps { get; set; }
    }
    public class EnquiryFollowUpResponse
    {
        public long? FollowUpId { get; set; }
        public long? EnquiryId { get; set; }

        public long? FollowUpTypeId { get; set; }
        public string? FollowUpTypeName { get; set; }  // optional for UI, if joined later

        public long? FollowUpLeadLostReasonId { get; set; }
        public string? FollowUpLeadLostReasonName { get; set; }  // optional for UI, if joined later

        public long? FollowUpLeadLostReasonCategoryId { get; set; }
        public string? FollowUpLeadLostReasonCategoryName { get; set; }  // optional for UI, if joined later

        public string? FollowUpDateTime { get; set; }
        public string? FollowUpDateTime2 { get; set; }
        public string? FollowUpDate { get; set; }
        public string? FollowUpTime { get; set; }
        public string? FollowUpTime12hr { get; set; }

        public string? NextFollowUpDateTime { get; set; }
        public string? NextFollowUpDateTime2 { get; set; }
        public string? NextFollowUpDate { get; set; }
        public string? NextFollowUpTime { get; set; }
        public string? NextFollowUpTime12hr { get; set; }

        public long? StatusId { get; set; }
        public string? StatusName { get; set; }        // optional (future use)

        public long? MoodId { get; set; }
        public string? MoodName { get; set; }          // optional (future use)

        public long? OutcomeId { get; set; }
        public string? OutcomeName { get; set; }       // optional (future use)

        public decimal? Probability { get; set; }

        public string? DiscussionNotes { get; set; }
        public string? InternalRemarks { get; set; }
        public string? CustomerRemarks { get; set; }

        public long? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public bool IsClosed { get; set; }
    }

    public class EnquiryDetailsModel
    {
        public string? Enquiry_Name { get; set; }
        public string? Enquiry_Contact { get; set; }
        public string? Enquiry_Slot { get; set; }
        public string? Enquiry_EventStartTime { get; set; }
        public string? Enquiry_EventEndTime { get; set; }
        public int? Enquiry_Pax { get; set; }
        public string? Enquiry_EventName { get; set; }
        public string? Enquiry_EventName_SubType { get; set; }

        public string? Enquiry_EventDate { get; set; }
        public string? Enquiry_EventDate_Format1 { get; set; }

        public string? Enquiry_Halls { get; set; }
        public string? Enquiry_Email { get; set; }
        public string? Enquiry_Address { get; set; }
        public string? Enquiry_Remarks { get; set; }
    }



    public class SaveEnquiryFollowUpRequest
    {
        public long? FollowUpId { get; set; }          // null or 0 = insert, >0 = update

        public long? EnquiryId { get; set; }
        public long? FollowUpTypeId { get; set; }
        public long? FollowUpLeadLostReasonCategoryId { get; set; }
        public long? FollowUpLeadLostReasonId { get; set; }

        public string? FollowUpDateTime { get; set; }   // string? because SP accepts NVARCHAR(30)
        public string? NextFollowUpDateTime { get; set; }

        public long? StatusId { get; set; }
        public long? MoodId { get; set; }

        public long? OutcomeId { get; set; }
        public decimal? Probability { get; set; }

        public string? DiscussionNotes { get; set; }
        public string? InternalRemarks { get; set; }
        public string? CustomerRemarks { get; set; }
        public string? FileUpload { get; set; }

        public long? AssignedTo { get; set; }
        public bool IsClosed { get; set; }

        public long? ActionBy { get; set; }             // user id
    }
    public class DeleteEnquiryFollowUpRequest
    {
        public long? FollowUpId { get; set; }
        public long? ActionBy { get; set; }
    }

}
