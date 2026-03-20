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
        public string? InvoiceNo { get; set; }
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
    }

    public class InvoiceItem
    {
        public string? HSN { get; set; } = "";
        public string? Description { get; set; }
        public decimal Rate { get; set; }
        public int Qty { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal CgstPct { get; set; }
        public decimal CgstAmt { get; set; }
        public decimal SgstPct { get; set; }
        public decimal SgstAmt { get; set; }
        public decimal IgstPct { get; set; } = 0;
        public decimal IgstAmt { get; set; } = 0;
    }

    public class InvoiceTotals
    {
        public decimal SubTotal { get; set; }
        public decimal Taxable { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal RoundOff { get; set; }
        public decimal TotalAdvance { get; set; }
        public decimal Balance { get; set; }
        public string? AmountInWords { get; set; }
    }
    #endregion

}
