namespace ERPAdmin.Models
{
    public class EmergencyContactReportRequest : BaseClass
    {
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public string? Columns { get; set; }
        public bool? WithHeader { get; set; }
    }

    #region GST Invoice
    public class CompanyInfo
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? GST { get; set; }
        public string? PAN { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; }
        public string? BankName { get; set; }
        public string? AccountNo { get; set; }
        public string? Branch { get; set; }
        public string? IFSC { get; set; }
        public string? UPI { get; set; }
        public string? Logo { get; set; }
    }

    public class InvoiceHeader
    {
        public long? ProformaInvoiceNo { get; set; }
        public string? InvoiceNo { get; set; }
        //public string? GSTInvoiceNo { get; set; }
        public string? Date { get; set; }
        public string? BookingRef { get; set; }
        public string? EventDate { get; set; }
        public string? EventType { get; set; }
        public string? Hall { get; set; }
    }

    public class BillToInfo
    {
        public string? Name { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? GSTIN { get; set; }
        public string? PAN { get; set; }
        public bool? IsInterState { get; set; }
    }

    public class InvoiceItem
    {
        public string? HSN { get; set; } = "";
        public string? Description { get; set; }
        public decimal Rate { get; set; }
        public decimal Qty { get; set; }
        public decimal SubTotal { get; set; }
        public decimal? DiscountRate { get; set; } = 0;
        public int? DiscountTypeId { get; set; } = 0;
        public string? DiscountTypeName { get; set; } = "";
        public decimal? DiscountAmount { get; set; } = 0;
        public decimal TaxableValue { get; set; }
        public decimal CGST_Percent { get; set; }
        public decimal CGST_Amount { get; set; }
        public decimal SGST_Percent { get; set; }
        public decimal SGST_Amount { get; set; }
        public decimal IGST_Percent { get; set; }
        public decimal IGST_Amount { get; set; }
        public decimal Total { get; set; }
    }

    public class InvoiceTotals
    {
        public decimal SubTotal { get; set; }
        public decimal Taxable { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal RoundOff { get; set; }
        public decimal TotalAdvance { get; set; }
        public decimal Balance { get; set; }
        public string? AmountInWords { get; set; }
    }
    #endregion


    #region BOOKING NOTES
    public class BookingNotePdf
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }

        public long SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }

        public string Value { get; set; }
        public string Remarks { get; set; }

        public List<BookingNoteOptionPdf> Options { get; set; }
    }

    public class BookingNoteOptionPdf
    {
        public long OptionId { get; set; }
        public string OptionName { get; set; }
    }
    #endregion
}
