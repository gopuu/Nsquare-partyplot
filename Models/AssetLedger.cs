namespace ERPAdmin.Models
{
    //API-GetAssetLedger
    public class GetAssetLedgerRequest
    {
        public long Id { get; set; }

        public long AssetMaster_Id { get; set; }

        public string? TransactionType { get; set; }

        public long ReferenceId { get; set; }

        public string? ReferenceTable { get; set; }

        public string? MonthDuration { get; set; }

        public string? FromDate { get; set; }

        public string? ToDate { get; set; }

        public bool? IsReversal { get; set; }
    }
    public class AssetLedgerModel
    {
        public long Id { get; set; }
        public long AssetMaster_Id { get; set; }
        public string? AssetName { get; set; }
        public string? TransactionType { get; set; }
        public long? ReferenceId { get; set; }
        public string? ReferenceTable { get; set; }
        public decimal QtyIn { get; set; }
        public decimal QtyOut { get; set; }
        public string? TransactionDate { get; set; }

        public string? TransactionDate2 { get; set; }

        public string? Remarks { get; set; }

        public bool? IsReversal { get; set; }

        public string? Created_At { get; set; }

        public long? Created_By { get; set; }
    }


}
