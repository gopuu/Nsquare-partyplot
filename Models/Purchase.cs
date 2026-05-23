//using System.Transactions;

namespace ERPAdmin.Models
{

    #region Purchase GRID
    public class GetPurchaseListGridRequest
    {
        public string? PONo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? FilterSearchText { get; set; }
        public long SupplierId { get; set; }
        public string? EventFrom { get; set; }
        public string? EventTo { get; set; }
        public string? FilterDuration { get; set; }
        public int StatusId { get; set; }
    }
    public class GetPurchaseListGridModel
    {
        public long Id { get; set; }
        public string? SupplierName { get; set; }
        public string? InvoiceDate_Format { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceNo { get; set; }
        public string? PONo { get; set; }

        public decimal SubTotalAmount { get; set; }
        public decimal Tax { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifiedOn { get; set; }
        public string? PaymentStatus { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsUploadedScannedInvoice { get; set; }
    }
    public class GetPurchaseListGridTotals
    {
        public int TotalRecords { get; set; }
        public decimal TotalSubTotalAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalOtherCharges { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
    }
    #endregion

    #region REPORT GRID
    public class GetPurchaseListGridReport_Request : BaseClass
    {
        public string? PONo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? FilterSearchText { get; set; }
        public long SupplierId { get; set; }
        public string? EventFrom { get; set; }
        public string? EventTo { get; set; }
        public string? FilterDuration { get; set; }
        public int StatusId { get; set; }
        //default
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }

    }

    #endregion
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
        public decimal CGST_Amount { get; set; }
        public decimal SGST_Amount { get; set; }
        public decimal IGST_Amount { get; set; }
        public bool IsSelected { get; set; }
    }

    public class GetPurchaseRequest
    {
        public long? PurchaseId { get; set; }
        public long? SupplierId { get; set; } = 0;
        public string? InvoiceNo { get; set; }
    }

    #region SAVE
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
        public long? PaymentAccountId { get; set; }
        public string? PaymentRemarks { get; set; }
        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }

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
    #endregion

    #region DELETE
    //DELETE
    public class DeletePurchaseRequest : BaseClass
    {
        public long Id { get; set; }
    }
    #endregion

    #region WIDGET
    public class PurchaseWidget_Response
    {
        public string? TotalOrders { get; set; }
        public string? TotalAmount { get; set; }
        public string? TotalOutstandingAmount { get; set; }
    }
    public class PurchaseWidget_Request
    {
        public string? EventFrom { get; set; }
        public string? EventTo { get; set; }
        public string? FilterDuration { get; set; }
    }
    #endregion


    //All txn - Request model – Get transactions
    public class GetPurchasePaymentTransactionsRequest
    {
        public long PurchaseId { get; set; } = 0;
        public string? MonthDuration { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public long SupplierId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
    }
    //All Txn - Response model – Transaction row
    public class PurchasePaymentTransactionModel
    {
        public long PaymentId { get; set; }
        public long PurchaseId { get; set; }

        public string? InvoiceNo { get; set; }
        public string? PONo { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }

        public string? SupplierName { get; set; }

        public string? PaymentDate { get; set; }
        public string? PaymentDate_Format { get; set; }
        public decimal PaidAmount { get; set; }

        public string? PaymentMode { get; set; }
        public string? AccountName { get; set; }

        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }

        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }

        public string? PaidBy { get; set; }
        public string? PaidOn { get; set; }
    }

    //Outstanding payment - Request model
    public class SavePurchaseOutstandingPaymentRequest : BaseClass
    {
        public long PurchaseId { get; set; }
        public string? PaymentDate { get; set; }
        public decimal PaidAmount { get; set; }
        public long PaymentModeId { get; set; }
        public long AccountId { get; set; }

        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }

        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }
    }


}
