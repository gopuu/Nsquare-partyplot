namespace ERPAdmin.Models
{
    //SUMMARY GRID
    public class ExpenseSummaryGridRequest
    {
        public string? Month { get; set; }       // yyyy-MM
        public long CategoryId { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
    }

    public class ExpenseSummaryGridModel
    {
        public string MonthName { get; set; } = "";
        public string MonthName2 { get; set; } = "";
        public string ExpenseCategoryName { get; set; } = "";
        public long ExpenseCategoryId { get; set; } = 0;
        public decimal Amount { get; set; }
    }

    //SUMMARY PDF-EXCEL
    public class ExpenseSummaryReportRequest : BaseClass
    {
        public List<string>? Columns { get; set; }

        public string? Month { get; set; }   // yyyy-MM

        public long CategoryId { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
    
        public bool? WithHeader { get; set; }
    }



    public class ExpenseReportGridRequest
    {
        public long CategoryId { get; set; } = 0;
        public long SubCategoryId { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
        public string? Month { get; set; }
        public string? VoucherNo { get; set; }
    }

    public class ExpenseReportRequest : BaseClass
    {
        public long CategoryId { get; set; } = 0;
        public long SubCategoryId { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Month { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public string? VoucherNo { get; set; }
    }

    public class ExpenseReportGridModel
    {
        public long SrNo { get; set; }
        public long TransactionId { get; set; }
        public string? TransactionDate { get; set; }
        public string? TransactionDate_Format { get; set; }
        public string? VoucherNo { get; set; }
        public long VoucherNo_Num { get; set; }
        public string? AccountName { get; set; }
        public string? CategoryName { get; set; }
        public long CategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public long SubCategoryId { get; set; }
        public string? PaymentModeName { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
