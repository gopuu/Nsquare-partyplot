namespace ERPAdmin.Models
{
    public class FundTransferModel
    {
        public long Id { get; set; }
        public long FromAccountId { get; set; }
        public string? FromAccountName { get; set; }
        public long ToAccountId { get; set; }
        public string? ToAccountName { get; set; }

        public decimal Amount { get; set; }
        public string? TransferDate { get; set; }
        public string? TransferDate_Format { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
    }
    public class GetFundTransferRequest
    {
        public long Id { get; set; } = 0;
        public long FromAccountId { get; set; } = 0;
        public long ToAccountId { get; set; } = 0;
        public string? FilterDuration { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public decimal? Amount { get; set; }
    }

    public class FundTransferReportRequest : BaseClass
    {
        public long Id { get; set; } = 0;
        public long FromAccountId { get; set; } = 0;
        public long ToAccountId { get; set; } = 0;
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public bool? WithHeader { get; set; }
        public decimal? Amount { get; set; }
    }

    public class SaveFundTransferRequest : BaseClass
    {
        public long Id { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? TransferDate { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
    }

    public class DeleteFundTransferRequest : BaseClass
    {
        public long Id { get; set; }
    }


}
