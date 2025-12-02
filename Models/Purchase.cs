namespace ERPAdmin.Models
{

    //GET
    public class GetPurchaseListModel
    {
        public long Id { get; set; }
        public long PurchaseFrom { get; set; }
        public string? PurchaseFromName { get; set; }
        public long SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCompany { get; set; }
        public string? SupplierPhone { get; set; }

        public DateOnly? InvoiceDate { get; set; }
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
        public decimal PaidAmount { get; set; }
        public long PaymentModeId { get; set; }
        public string? PaymentRefNo { get; set; }

        public bool IsUpdateInventoryStock { get; set; }
        public string? UploadInvoiceFile { get; set; }
        public bool IsActive { get; set; }
        public long Created_By_EmpId { get; set; }
        public string? Created_By_EmpName { get; set; }
        public string? Created_On { get; set; }
        public string? Modified_On { get; set; }
        public string? Status { get; set; }

        public List<GetPurchaseItems>? JSON_PurchaseItems { get; set; }
    }
    public class GetPurchaseItems
    {
        public long PurchaseId { get; set; }
        public long RawMaterialId { get; set; }
        public string? RawMaterialName { get; set; }
        public decimal Qty { get; set; }
        public long UnitId { get; set; }
        public string? UnitName { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public bool IsSelected { get; set; }
    }

    public class GetPurchaseRequest
    {
        public long Id { get; set; }
        public long SupplierId { get; set; }
    }

    //SAVE
    public class SavePurchaseRequest : BaseClass
    {
        public long Id { get; set; }
        public long? PurchaseFrom { get; set; }
        public long SupplierId { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceNo { get; set; }
        public string? PONo { get; set; }

        public long? DiscountType { get; set; }
        public decimal TotalDiscountInInvoice { get; set; }
        public decimal DiscountPercentage { get; set; }

        public long? DetermineWhenTaxApplicable { get; set; }

        public decimal SubTotal_TotalDiscount_Amount { get; set; }
        public decimal SubTotal_OtherCharges_Amount { get; set; }

        public decimal DeliveryChargeIncludedInInvoice { get; set; }
        public decimal DeliveryCharge_CGST { get; set; }
        public decimal DeliveryCharge_SGST { get; set; }

        public decimal SubTotal_OtherTaxes_Amount { get; set; }
        public decimal TCS_Percent { get; set; }
        public decimal TDS_Percent { get; set; }

        public decimal GrandTotal_Amount { get; set; }

        public long? PaymentType { get; set; }
        public string? PaymentDate { get; set; }
        public long? PaymentModeId { get; set; }
        public decimal? PaidAmount { get; set; }
        public string? PaymentRefNo { get; set; }

        public bool IsUpdateInventoryStock { get; set; }

        public string? UploadInvoiceFile { get; set; }      // base64 or filename
        public List<SavePurchaseItemsRequest>? JSON_PurchaseItems { get; set; }

        public bool IsActive { get; set; }
    }

    public class SavePurchaseItemsRequest
    {
        public long PurchaseId { get; set; }
        public long RawMaterialId { get; set; }
        public decimal Qty { get; set; }
        public long UnitId { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public bool IsSelected { get; set; }
    }

    //DELETE
    public class DeletePurchaseRequest : BaseClass
    {
        public long Id { get; set; }
    }
    //Widgets
    public class PurchaseWidgetDto
    {
        public string? GrandTotal_Amount { get; set; }
        public string? Outstanding_Amount { get; set; }
        public string? TaxPaidToSeller { get; set; }
    }

}
