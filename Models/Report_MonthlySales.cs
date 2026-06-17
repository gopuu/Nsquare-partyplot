namespace ERPAdmin.Models
{

    /* === 1️ GRID REQUEST ====== */
    public class MonthlySalesGridRequest
    {
        public int? YearId { get; set; }
        public int? Year { get; set; }
        public int? MonthId_From { get; set; }
        public int? MonthId_To { get; set; }
    }

    /* == 2️ MONTHLY REPORT EXPORT REQUEST ==*/
    public class MonthlySalesReportRequest : BaseClass
    {
        public int? YearId { get; set; }
        public int? Year { get; set; }
        public int? MonthId_From { get; set; }
        public int? MonthId_To { get; set; }

        public List<string>? Columns { get; set; }

        public bool? WithHeader { get; set; }
    }

    /* == 3️ MONTHLY GRID MODEL== */
    public class MonthlySalesGridModel
    {
        public long SrNo { get; set; }

        public long YearMaster_Id { get; set; }

        public int Mn { get; set; }

        public long MonthId { get; set; }

        public string? MonthName { get; set; }

        public int InvoiceGeneratedCount { get; set; }

        public decimal GrossAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal NetAmount { get; set; }

        public decimal TaxableAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal InvoiceAmount { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal DuePayment { get; set; }

        public string? PaymentMode { get; set; }
    }
    /* == 4️ MONTHLY GRID TOTALS ==*/
    public class MonthlySalesTotalsModel
    {
        public int TotalInvoices { get; set; }

        public decimal TotalGrossAmount { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalNetAmount { get; set; }

        public decimal TotalTaxableAmount { get; set; }

        public decimal TotalTaxAmount { get; set; }

        public decimal TotalInvoiceAmount { get; set; }

        public decimal TotalPaymentAmount { get; set; }

        public decimal TotalDuePayment { get; set; }
    }

    /* == 5️ MONTH DETAILS REQUEST (Drilldown) == */
    public class MonthlySalesMonthDetailsRequest : BaseClass
    {
        public int MonthId { get; set; }

        public List<string>? Columns { get; set; }

        public bool? WithHeader { get; set; }
    }

    /* == 6️ MONTH DETAILS GRID MODEL == */
    public class MonthlySalesMonthDetailsModel
    {
        public long SrNo { get; set; }

        public long? InvoiceNo { get; set; }
        public string? GSTInvoiceNo { get; set; }
        public string? InvoiceNo2 { get; set; }

        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }

        public long BookingId { get; set; }

        public string? ClientName { get; set; }

        public decimal GrossAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InvoiceAmount { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }

        public string? PaymentModes { get; set; }
    }
    /* == 7️ MONTH DETAILS TOTALS == */
    public class MonthlySalesMonthDetailsTotalsModel
    {
        public int TotalInvoices { get; set; }

        public decimal GrossAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal NetAmount { get; set; }

        public decimal TaxableAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal InvoiceAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal DueAmount { get; set; }
    }
}