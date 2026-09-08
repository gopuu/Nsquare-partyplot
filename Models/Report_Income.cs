namespace ERPAdmin.Models
{
    #region Income Report Models

    /* ═══════════════════════════════════════════════════
       1. INCOME SUMMARY - GRID REQUEST
       API: GetIncomeSummaryReportGrid
    ═══════════════════════════════════════════════════ */
    public class IncomeSummaryGridRequest
    {
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        public string FilterDuration { get; set; } = "";
        //public string? Month { get; set; } // yyyy-MM
        public long CategoryId { get; set; } = 0;
    }


    /* ═══════════════════════════════════════════════════
       2. INCOME SUMMARY - GRID MODEL
    ═══════════════════════════════════════════════════ */

    public class IncomeSummaryGridModel
    {
        public string MonthName { get; set; } = "";  // MMM-yyyy
        public string MonthName2 { get; set; } = ""; // yyyy-MM

        public long IncomeCategoryId { get; set; }
        public string IncomeCategoryName { get; set; } = "";

        public decimal Amount { get; set; }
    }


    /* ═══════════════════════════════════════════════════
       3. INCOME SUMMARY - PDF / EXCEL REQUEST
       APIs:
       DownloadIncomeSummaryReport_Pdf
       DownloadIncomeSummaryReport_Excel
    ═══════════════════════════════════════════════════ */
    public class IncomeSummaryReportRequest : BaseClass
    {
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        public string FilterDuration { get; set; } = "";
        //public string? Month { get; set; } // yyyy-MM
        public long CategoryId { get; set; } = 0;

        public List<string>? Columns { get; set; }

        public bool? WithHeader { get; set; }
    }


    /* ═══════════════════════════════════════════════════
       4. INCOME DETAIL - GRID REQUEST
       API: GetIncomeReportGrid
    ═══════════════════════════════════════════════════ */

    public class IncomeReportGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public string? VoucherNo { get; set; }

        public long CategoryId { get; set; } = 0;
        public long SubCategoryId { get; set; } = 0;

        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
    }


    /* ═══════════════════════════════════════════════════
       5. INCOME DETAIL - GRID MODEL
    ═══════════════════════════════════════════════════ */

    public class IncomeReportGridModel
    {
        public long SrNo { get; set; }

        public long TransactionId { get; set; }

        public string VoucherNo { get; set; } = "";
        public long VoucherNo_Num { get; set; }

        public string TransactionDate { get; set; } = "";
        public string TransactionDate_Format { get; set; } = "";

        public string AccountName { get; set; } = "";

        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = "";

        public long SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = "";

        public string Notes { get; set; } = "";

        public string PaymentModeName { get; set; } = "";

        public decimal Amount { get; set; }
    }


    /* ═══════════════════════════════════════════════════
       6. INCOME DETAIL - PDF / EXCEL REQUEST
       APIs:
       DownloadIncomeReport_Pdf
       DownloadIncomeReport_Excel
    ═══════════════════════════════════════════════════ */

    public class IncomeReportRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }

        public string? VoucherNo { get; set; }

        public long CategoryId { get; set; } = 0;
        public long SubCategoryId { get; set; } = 0;

        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;

        public List<string>? Columns { get; set; }

        public bool? WithHeader { get; set; }
    }

    #endregion
}
