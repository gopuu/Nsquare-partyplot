namespace ERPAdmin.Models
{
    //GET
    public class GetQuotationRequest
    {
        public long? Id { get; set; }
    }
    public class GetQuotationResponse
    {
        public long Id { get; set; }
        public long? EnquiryDealId { get; set; }
        public long BanquetId { get; set; }
        public string? BanquetName { get; set; }
        public long SlotId { get; set; }
        public string? SlotName { get; set; }

        public string? FromDate { get; set; }
        public string? TillDate { get; set; }
        public string? FromDate2 { get; set; }
        public string? TillDate2 { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? StartTime1 { get; set; }
        public string? EndTime1 { get; set; }
        public string? QuotationDate { get; set; }
        public string? QuotationDate2 { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? ContactNo { get; set; }
        public string? GSTIN { get; set; }
        public long? StateId { get; set; }
        public string? StateName { get; set; }

        public string? EventSubTypeId { get; set; }
        public string? EventSubTypeName { get; set; }

        public decimal BanquetRent { get; set; }
        public int Pax { get; set; }
        public string? ReferenceTypeId { get; set; }
        public string? ReferenceTypeName { get; set; }

        public decimal DecorationChargesAmount { get; set; }
        public decimal SoundSystemChargesAmount { get; set; }
        public decimal ProjectorChargesAmount { get; set; }

        public bool IsPrintPriceExclusive { get; set; }
        public bool IsPrintFoodItems { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string? Notes { get; set; }

        public List<QuotationPackageModel>? Packages_JSON { get; set; } = new();
        public List<QuotationOtherServiceModel>? OtherServices_JSON { get; set; } = new();
    }
    public class QuotationPackageModel
    {
        public long Id { get; set; }
        public long PackageId { get; set; }
    }
    public class QuotationOtherServiceModel
    {
        public long Id { get; set; }
        public int? SrNo { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    //SAVE
    public class SaveQuotationRequest : BaseClass
    {
        public long Id { get; set; }

        public long? EnquiryDealId { get; set; }
        public long BanquetId { get; set; }
        public long SlotId { get; set; }
        public string? FromDate { get; set; }
        public string? TillDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }

        public long? ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? ContactNo { get; set; }
        public string? GSTIN { get; set; }
        public long? StateId { get; set; }

        public string? EventSubTypeId { get; set; }
        public decimal BanquetRent { get; set; }
        public int Pax { get; set; }
        public string? ReferenceTypeId { get; set; }

        public decimal DecorationChargesAmount { get; set; }
        public decimal SoundSystemChargesAmount { get; set; }
        public decimal ProjectorChargesAmount { get; set; }

        public bool IsPrintPriceExclusive { get; set; }
        public bool IsPrintFoodItems { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string? Notes { get; set; }

        // JSON strings
        public string? PackagesJson { get; set; }
        public string? OtherServicesJson { get; set; }

        public long ActionBy { get; set; }
    }

    //DELETE
    public class DeleteQuotationRequest : BaseClass
    {
        public long Id { get; set; }
    }

    // Get Quotation List By EnquiryDealId
    public class GetQuotationByEnquiryIdRequest
    {
        public long EnquiryId { get; set; }
        public long? QuotationId { get; set; } = 0;
    }
    public class GetQuotationByEnquiryIdResponse
    {
        public long SlotId { get; set; }
        public long StateId { get; set; }
        public long EnquiryId { get; set; }
        public long ProcessId { get; set; }
        public string ProcessName { get; set; }

        public string Name { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        public long EventTypeId { get; set; }
        public string EventTypeName { get; set; }

        public long EventSubTypeId { get; set; }
        public string EventSubTypeName { get; set; }

        public long BanquetHallId { get; set; }
        public string BanquetHallName { get; set; }

        public string EventDate { get; set; }
        public string EventDate_Format { get; set; }

        public string EventStartTime { get; set; }
        public string EventEndTime { get; set; }
        public string EventStartTime1 { get; set; }
        public string EventEndTime1 { get; set; }

        public int Pax { get; set; }

        public string FollowUpDate { get; set; }
        public string FollowUpDate_Format { get; set; }

        public decimal EstimatedAmount { get; set; }

        public long LeadSourceId { get; set; }
        public string LeadSourceName { get; set; }

        public string Remarks { get; set; }
        public long PackageId { get; set; }

        public long FollowUpCount { get; set; }

        // 🔥 NEW
        public List<GetQuotationResponse> Quotations { get; set; } = new();
    }
}
