namespace ERPAdmin.Models
{
    //Income-Expense TXN
    public class GetIncomeExpenseTransactionRequest
    {
        public long? AccountId { get; set; } = 0;
        public long? PaymentModeId { get; set; } = 0;

        public long? IncomeCategoryId { get; set; } = 0;
        public long? IncomeSubCategoryId { get; set; } = 0;

        public long? ExpenseCategoryId { get; set; } = 0;
        public long? ExpenseSubCategoryId { get; set; } = 0;

        public string? FromDate { get; set; } = null;
        public string? ToDate { get; set; } = null;
        public decimal? FromAmount { get; set; } = null;
        public decimal? ToAmount { get; set; } = null;

        public string? TransactionType { get; set; } = "ALL"; // ALL / INCOME / EXPENSE

        public string? VoucherNo { get; set; } = null;
    }

    public class IncomeExpenseTransactionModel
    {
        public long Id { get; set; }
        public string TransactionType { get; set; }

        public long AccountId { get; set; }
        public string AccountName { get; set; }

        public long CategoryId { get; set; }
        public string CategoryName { get; set; }

        public long? SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }

        public decimal Amount { get; set; }

        public string TransactionDate { get; set; }
        public string TransactionDate_Format { get; set; }

        public long PaymentModeId { get; set; }
        public string PaymentModeName { get; set; }

        public string TransactionId { get; set; }
        public string Notes { get; set; }

        public string VoucherNo { get; set; }

        public string ChequeBank { get; set; }
        public string ChequeNo { get; set; }
        public string ChequeDate { get; set; }
    }

    public class DashboardFinanceSummary
    {
        public decimal TotalIncomes { get; set; }
        public decimal CategorisedIncome { get; set; }
        public decimal InvoiceIncomes { get; set; }

        public decimal TotalExpenses { get; set; }
        public decimal CategorisedExpense { get; set; }
        public decimal PurchaseExpenses { get; set; }

        public decimal GrossProfit { get; set; }
    }

    public class DashboardPaymentCollectionStatus
    {
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class DashboardInvoiceStatus
    {
        public long InvoiceGeneratedCount { get; set; }
        public long InvoicePendingCount { get; set; }
        public long InvoiceTotalCount { get; set; }
    }
    public class UpcomingPaymentItem
    {
        public string? EventName { get; set; }
        public decimal Amount { get; set; }
    }

    public class VendorPayableItem
    {
        public string? VendorName { get; set; }
        public decimal PendingAmount { get; set; }
        public string? Status { get; set; }
    }

    public class TopRevenueEventItem
    {
        public string? FullTitle { get; set; }
        public decimal Amount { get; set; }
        public string? EventTypeName { get; set; }
        public string? EventSubTypeName { get; set; }
        public string? ClientName { get; set; }
        public string? InvoiceNo { get; set; }
    }
}
