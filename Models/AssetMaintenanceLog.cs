namespace ERPAdmin.Models
{
    public class GetAssetMaintenanceLogRequest
    {
        public long? Id { get; set; }
        public long? AssetId { get; set; }
        public long? BookingId { get; set; }
    }

    public class GetAssetMaintenanceLogResponse
    {
        public long Id { get; set; }
        public long AssetId { get; set; }
        public string AssetName { get; set; }
        public string MaintenanceDate { get; set; }
        public bool IsMaintenanceDone { get; set; }
        public decimal MaintenanceCost { get; set; }
        public string? Remarks { get; set; }
        public long? BookingId { get; set; }
    }

    public class SaveAssetMaintenanceLogRequest
    {
        public long Id { get; set; }
        public long AssetId { get; set; }
        public string MaintenanceDate { get; set; }
        public bool IsMaintenanceDone { get; set; }
        public decimal MaintenanceCost { get; set; }
        public string? Remarks { get; set; }
        public long? BookingId { get; set; }
        public long ActionBy { get; set; }
    }

}
