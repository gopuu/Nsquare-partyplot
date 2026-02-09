namespace ERPAdmin.Models
{
    public class NextInvoiceNumberResponse
    {
        public long NextInvoiceNo { get; set; }
        public long NextProformaInvoiceNo { get; set; }
        public long NextGSTInvoiceNo { get; set; }
    }


    public class CustomerInvoiceRequest_GRID
    {
        public long BookingId { get; set; } = 0;
        public string? NameEmailContact { get; set; }
        public long SlotId { get; set; } = 0;
        public string? EventFrom { get; set; }
        public string? EventTo { get; set; }
        public string? BookedFrom { get; set; }
        public string? BookedTill { get; set; }
        public long SubEventId { get; set; } = 0;
    }

    public class CustomerInvoiceResponse_GRID
    {
        public long BookingId { get; set; }
        public long InvoiceNo { get; set; }
        public long ProformaInvoiceNo { get; set; }
        public long GSTInvoiceNo { get; set; }
        public string EventDate { get; set; }
        public string BookedOn { get; set; }
        public string EventName { get; set; }
        public string HallName { get; set; }
        public string ClientName { get; set; }
        public string ClientContactNo { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
    // SAVE
    public class SaveCustomerInvoiceRequest : BaseClass
    {
        public long Id { get; set; }
        public long BookingId { get; set; }

        public long? InvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }
        public long? GSTInvoiceNo { get; set; }

        public string? CustomerName { get; set; }
        public string? InvoiceDate { get; set; }

        public string? GSTIN { get; set; }
        public string? Address { get; set; }
        public string? EmailId { get; set; }
        public long? CustomerState { get; set; }

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

        public decimal Total { get; set; }
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
        public List<CustomerInvoiceItem_BookingId>? ItemsJson { get; set; }   // JSON string from SQL
    }
    public class CustomerInvoiceItem_BookingId
    {
        public string? Item_Description { get; set; }
        public string? HSN { get; set; }

        public decimal? Rate { get; set; }
        public decimal? Qty { get; set; }

        public decimal? SubTotal { get; set; }

        public decimal? DiscountRate { get; set; }
        public int? DiscounType { get; set; }
        public decimal? DiscountAmount { get; set; }

        public decimal? Taxable { get; set; }
        public decimal? GSTRate { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }

        public decimal? Total { get; set; }
    }

    #endregion

    #region CustomerInvoice By Id
    public class CustomerInvoiceRequest_Id
    {
        public long BookingId { get; set; }
        public long? InvoiceNo { get; set; }
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

        public decimal? Total { get; set; }
    }
    public class CustomerInvoiceResponse_Id
    {
        public long? Id { get; set; }

        public long? InvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }
        public long? GSTInvoiceNo { get; set; }

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
