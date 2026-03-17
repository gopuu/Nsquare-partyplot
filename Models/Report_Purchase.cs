namespace ERPAdmin.Models
{
    //invoice request
    public class GetPurchaseRequestGRID
    {
        public long? PurchaseId { get; set; }
        public string? InvoiceNo { get; set; }
    }

    //invoice response
    public class GetPurchaseListModelGRID
    {
        public long Id { get; set; }
        public long PurchaseFrom { get; set; }
        public string? PurchaseFromName { get; set; }
        public long SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCompany { get; set; }
        public string? SupplierPhone { get; set; }

        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }
        public string? InvoiceNo { get; set; }
        public string? PONo { get; set; }

        public long DiscountType { get; set; }
        public decimal TotalDiscountInInvoice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public long DetermineWhenTaxApplicable { get; set; }

        public decimal SubTotal_TotalDiscount_Amount { get; set; }
        public decimal SubTotal_OtherCharges_Amount { get; set; }

        public decimal DeliveryChargeIncludedInInvoice { get; set; }
        public decimal DeliveryCharge_CGST { get; set; }
        public decimal DeliveryCharge_SGST { get; set; }

        public decimal SubTotal_OtherTaxes_Amount { get; set; }
        public decimal TCS_Percent { get; set; }
        public decimal TDS_Percent { get; set; }
        public decimal GrandTotal_Amount { get; set; }

        public long PaymentType { get; set; }
        public string? PaymentDate { get; set; }
        public decimal Remaining_Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public long PaymentModeId { get; set; }
        public long PaymentAccountId { get; set; }
        public string? PaymentRefNo { get; set; }
        public string? PaymentRemarks { get; set; }
        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }

        public bool IsUpdateInventoryStock { get; set; }
        public string? UploadInvoiceFile { get; set; }
        public bool IsActive { get; set; }
        public long Created_By_EmpId { get; set; }
        public string? Created_By_EmpName { get; set; }
        public string? Created_On { get; set; }
        public string? Modified_On { get; set; }
        public string? Status { get; set; }

        public List<GetPurchaseItemsGRID>? JSON_PurchaseItems { get; set; }
    }
    public class GetPurchaseItemsGRID
    {
        public long SrNo { get; set; }
        //public long RawMaterialId { get; set; }
        public string? RawMaterial { get; set; }
        public decimal Qty { get; set; }
        //public long UnitId { get; set; }
        public string? UnitName { get; set; }
        public decimal Price { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal CGST_Amount { get; set; }
        public decimal SGST_Amount { get; set; }
        public decimal IGST_Amount { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalAmount { get; set; }
        //public bool IsSelected { get; set; }
    }


    //report purchase invoice
    public class PurchaseInvoiceReportRequest : BaseClass
    {
        public long? PurchaseId { get; set; }
        //public string? PurchaseOrderNo { get; set; }
        public string? InvoiceNo { get; set; }
        public bool? WithHeader { get; set; }
    }

    public class PurchaseReportGridRequest
    {
        public long SupplierId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class PurchaseReportRequest
    {
        public long SupplierId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class PurchaseReportGridModel
    {
        public long SrNo { get; set; }
        public long PurchaseId { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceNo { get; set; }
        public string? SupplierName { get; set; }
        public long SupplierId { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Outstanding { get; set; }
    }
}
