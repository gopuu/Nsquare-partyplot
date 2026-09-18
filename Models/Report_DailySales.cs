namespace ERPAdmin.Models
{
    // GRID REQUEST
    public class DailySalesGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class DailySalesDateDetailsRequest: BaseClass
    {
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public string? InvoiceDate { get; set; }
    }

    // REPORT REQUEST
    public class DailySalesReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class DailySalesDateDetailsReportRequest
    {
        public string? InvoiceDate { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    // GRID RESPONSE
    public class DailySalesGridModel
    {
        public long SrNo { get; set; }

        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }

        public int Qty { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }

        public decimal InvoiceAmount { get; set; }

        public decimal PaymentAmount { get; set; }
        public string? PaymentMode { get; set; }

        public decimal DuePayment { get; set; }
    }
    public class DailySalesTotalsModel
    {
        public int TotalQty { get; set; }
        public decimal TotalSubTotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalTaxableAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }

        public decimal TotalInvoiceAmount { get; set; }

        public decimal TotalPaymentAmount { get; set; }

        public decimal TotalDuePayment { get; set; }
    }

    //DETAILS GRID
    public class DailySalesDateDetailsTotalsModel
    {
        public int TotalInvoices { get; set; }

        public decimal TotalSubTotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalTaxableAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }

        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal TotalDuePayment { get; set; }
    }

    public class DailySalesDateDetailsModel
    {
        public long SrNo { get; set; }

        public string? InvoiceNo { get; set; }

        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }

        public long BookingId { get; set; }

        public string? ClientName { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }

        public decimal NetAmount { get; set; }
        public decimal TaxableAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal InvoiceAmount { get; set; }

        public decimal PaymentAmount { get; set; }
        public decimal DuePayment { get; set; }

        public string? PaymentModes { get; set; }
    }

}