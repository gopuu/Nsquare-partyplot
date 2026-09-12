namespace ERPAdmin.Models
{
    //send invoice
    public class SendInvoiceRequest : BaseClass
    {
        public long BookingId { get; set; }
        public long? InvoiceNo { get; set; }
        public string? GSTInvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }
        public string? EmailId { get; set; }
        public string? ContactNo { get; set; }
        public string? WhatsappNo { get; set; }
    }

    public class NextInvoiceNumberResponse
    {
        public long NextInvoiceNo { get; set; }
        public long NextProformaInvoiceNo { get; set; }
        public string? NextGSTInvoiceNo { get; set; }
    }


    public class CustomerInvoiceRequest_GRID
    {
        public long BookingId { get; set; } = 0;
        public string? NameEmailContact { get; set; }
        public long SlotId { get; set; } = 0;
        public string? EventFrom { get; set; }
        public string? EventTo { get; set; }
        public string? FilterDuration { get; set; }
        public string? BookedFrom { get; set; }
        public string? BookedTill { get; set; }
        public long SubEventId { get; set; } = 0;
        public bool? IsInvoiceGenerated { get; set; }
        public bool? IsExclusiveOfGST { get; set; }
        public bool? IsInclusiveOfGST { get; set; }
        public long? PaymentStatusId { get; set; } = 0;
    }

    public class CustomerInvoiceResponse_GRID
    {
        public long BookingId { get; set; }
        public long? InvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }
        public string? FinalInvoiceNo { get; set; }
        public string? GSTInvoiceNo { get; set; }
        public bool? IsInvoiceGrouped { get; set; }
        public bool? IsExclusiveOfGST { get; set; }
        public bool? IsInclusiveOfGST { get; set; }
        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }
        public bool? IsInvoiceBlocked { get; set; }
        public string? BookedOn { get; set; }
        public string? EventName { get; set; }
        public string? HallName { get; set; }
        public string? ClientName { get; set; }
        public string? ClientContactNo { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? OutstandingAmount { get; set; }
    }
    // SAVE
    public class SaveCustomerInvoiceRequest : BaseClass
    {
        public long Id { get; set; }
        public long BookingId { get; set; }

        public long? InvoiceNo { get; set; } = null;
        public long? ProformaInvoiceNo { get; set; } = null;
        public string? GSTInvoiceNo { get; set; } = null;
        public bool? IsExclusiveOfGST { get; set; }
        public bool? IsInclusiveOfGST { get; set; }

        public string? CustomerName { get; set; }
        public string? InvoiceDate { get; set; }

        public string? GSTIN { get; set; }
        public string? Address { get; set; }
        public string? EmailId { get; set; }
        public long? CustomerState { get; set; }

        public long? Payment_ModeId { get; set; }
        public long? Payment_AccountId { get; set; }
        public string? Payment_TransactionId { get; set; }
        public string? Payment_ChequeBank { get; set; }
        public string? Payment_ChequeNo { get; set; }
        public string? Payment_ChequeDate { get; set; }  // Keep string if passing raw date
        public decimal? Payment_Amount { get; set; }
        public string? Payment_PaidDate { get; set; }   // NVARCHAR(20)
        public string? Payment_PaidTime { get; set; }
        public string? Payment_DueDate { get; set; }
        public string? Payment_Note { get; set; }

        public decimal? SubTotal { get; set; }
        public decimal? BookingDiscount { get; set; }
        public decimal? TotalTaxableAmount { get; set; }
        public decimal TotalDiscountAcrossAllServices { get; set; }
        public decimal? TotalGST { get; set; }
        public decimal? TotalVAT { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? RoundOff { get; set; }
        public decimal? TotalAdvancePaid { get; set; }
        public decimal? BalanceAmount { get; set; }

        public List<CustomerInvoiceItem_Save>? JSON_Items { get; set; }
    }
    public class CustomerInvoiceItem_Save
    {
        public int SrNo { get; set; }

        public string? Description { get; set; }
        public string? HSN { get; set; }

        public decimal Rate { get; set; }
        public decimal Qty { get; set; }
        public decimal SubTotal { get; set; }

        public decimal DiscountRate { get; set; }
        public int DiscountTypeId { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal TaxableAmount { get; set; }
        public decimal GSTRate { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal? IGST { get; set; } = 0;

        public decimal Total { get; set; }
        public bool IsPackage { get; set; } = false;
        public bool IsDuplicate { get; set; } = false;
    }
    //SAVE

    public class DeleteCustomerInvoice : BaseClass
    {
        public long Id { get; set; }
    }

    public class CustomerInvoiceResponse
    {
        public long Id { get; set; }
        public string InvoiceNo { get; set; }
        public string ProformaInvoiceNo { get; set; }
        public string GSTInvoiceNo { get; set; }

        public string CustomerName { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceDate_Format { get; set; }

        public decimal SubTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal BalanceAmount { get; set; }

        public string JSON_Items { get; set; }
    }

    #region CustomerInvoice By BookingId
    public class CustomerInvoiceRequest_BookingId
    {
        public long BookingId { get; set; }
        public bool? IsInclusiveGST { get; set; } = false;
        public bool? IsProformaInvoice { get; set; } = false;
    }
    public class Report_CustomerInvoiceRequest
    {
        public List<string>? Columns { get; set; }

        public long BookingId { get; set; } = 0;
        public string? NameEmailContact { get; set; }
        public long SlotId { get; set; } = 0;

        public string? EventFrom { get; set; }
        public string? EventTo { get; set; }

        public string? BookedFrom { get; set; }
        public string? BookedTill { get; set; }

        public long SubEventId { get; set; } = 0;

        public bool? IsInvoiceGenerated { get; set; }
        public long PaymentStatusId { get; set; } = 0;

        public bool? IsExclusiveOfGST { get; set; }
        public bool? IsInclusiveOfGST { get; set; }

        public string? FilterDuration { get; set; }

        public long ActionBy { get; set; }
        public bool? WithHeader { get; set; }
    }
    public class CustomerInvoiceResponse_BookingId
    {
        public string? ClientName { get; set; }
        public string? InvoiceDate { get; set; }
        public string? GSTIN { get; set; }
        public string? Email { get; set; }
        public long? ClientStateId { get; set; }
        public string? Address { get; set; }
        public bool? IsGSTIncluded { get; set; }
        public bool? IsExclusiveOfGST { get; set; }
        public bool? IsInclusiveOfGST { get; set; }
        public decimal? PaidAmount { get; set; }
        public List<CustomerInvoiceItem_BookingId>? ItemsJson { get; set; }   // JSON string from SQL
    }
    public class CustomerInvoiceItem_BookingId
    {
        public string? Item_Description { get; set; }
        public bool? IsPackage { get; set; } = false;
        public string? HSN { get; set; }

        public decimal? Rate { get; set; }
        public decimal? Qty { get; set; }

        public decimal? SubTotal { get; set; }

        public decimal? DiscountRate { get; set; }
        public int? DiscounType { get; set; }
        public decimal? DiscountAmount { get; set; }

        public decimal? Taxable { get; set; }
        public decimal? GSTAmount { get; set; }
        public decimal? GSTRate { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? IGST { get; set; }

        public decimal? Total { get; set; }
    }

    #endregion

    #region CustomerInvoice By Id
    public class CustomerInvoiceRequest_Id
    {
        public long BookingId { get; set; }
        public long? InvoiceNo { get; set; }
        public string? GSTInvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }
   }
    public class CustomerInvoiceItem_Id
    {
        public long? Id { get; set; }
        public int? SrNo { get; set; }
        public string? Description { get; set; }
        public string? HSN { get; set; }

        public decimal? Rate { get; set; }
        public decimal? Qty { get; set; }
        public decimal? SubTotal { get; set; }

        public decimal? DiscountRate { get; set; }
        public int? DiscountTypeId { get; set; }
        public decimal? DiscountAmount { get; set; }

        public decimal? TaxableAmount { get; set; }
        public decimal? GSTRate { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? IGST { get; set; }

        public decimal? Total { get; set; }
        public bool IsPackage { get; set; } = false;
        public bool IsDuplicate { get; set; } = false;

    }
    public class CustomerInvoiceResponse_Id
    {
        public long? Id { get; set; }

        public long? InvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }
        public string? GSTInvoiceNo { get; set; }
        public bool? IsExclusiveOfGST { get; set; }
        public bool? IsInclusiveOfGST { get; set; }

        public string? CustomerName { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }
        public string? GSTIN { get; set; }
        public string? Address { get; set; }
        public string? EmailId { get; set; }
        public long? CustomerStateId { get; set; }

        public decimal? SubTotal { get; set; }
        public decimal? BookingDiscount { get; set; }
        public decimal? TotalTaxableAmount { get; set; }        
        public decimal? TotalDiscountAcrossAllServices { get; set; }
        public decimal? TotalGST { get; set; }
        public decimal? TotalVAT { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? RoundOff { get; set; }
        public decimal? TotalAdvancePaid { get; set; }
        public decimal? BalanceAmount { get; set; }

        public List<CustomerInvoiceItem_Id>? Items { get; set; }
    }
    #endregion
}
