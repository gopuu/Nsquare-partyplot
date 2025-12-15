namespace ERPAdmin.Models
{
    public class GetAssetMonthlyUsageRequest
    {
        public long? Id { get; set; }
        public long? AssetId { get; set; }
        public int? UsageMonth { get; set; }
        public int? UsageYear { get; set; }
    }

    public class GetAssetMonthlyUsageResponse
    {
        public long Id { get; set; }
        public long AssetId { get; set; }
        public string AssetName { get; set; }
        public int UsageMonth { get; set; }
        public int UsageYear { get; set; }
        public long EventsUsedCount { get; set; }
        public long DamageCount { get; set; }
        public long? Condition_Start_StatusId { get; set; }
        public long? Condition_End_StatusId { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal ClosingQty { get; set; }
    }

    public class SaveAssetMonthlyUsageRequest
    {
        public long Id { get; set; }
        public long AssetId { get; set; }
        public int UsageMonth { get; set; }
        public int UsageYear { get; set; }
        public long EventsUsedCount { get; set; }
        public long DamageCount { get; set; }
        public long? Condition_Start_StatusId { get; set; }
        public long? Condition_End_StatusId { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal ClosingQty { get; set; }
        public long ActionBy { get; set; }
    }

}
