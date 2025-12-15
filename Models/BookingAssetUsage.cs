namespace ERPAdmin.Models
{
    public class GetBookingAssetUsageRequest
    {
        public long? Id { get; set; }
        public long? BookingId { get; set; }
        public long? AssetId { get; set; }
    }

    public class GetBookingAssetUsageResponse
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public long AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetUniqueCode { get; set; }

        public decimal UsedQty { get; set; }
        public bool IsDamaged { get; set; }
        public string? DamageRemarks { get; set; }
    }
    public class SaveBookingAssetUsageRequest
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public long AssetId { get; set; }

        public decimal UsedQty { get; set; }
        public bool IsDamaged { get; set; }
        public string? DamageRemarks { get; set; }

        public long ActionBy { get; set; }
    }

}
