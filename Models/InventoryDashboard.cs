namespace ERPAdmin.Models
{
    public class InventoryDashboard
    {
    }
    public class InventorySupplierItem
    {
        public string? SupplierName { get; set; }
        public string? ItemName { get; set; }
        public int Quantity { get; set; }
    }

    public class DashboardinventoryRequest
    {
        public string? MonthDuration { get; set; }  
        public string? FromDate { get; set; }     
        public string? ToDate { get; set; }
        public string? Category { get; set; }
    }

    public class InventoryDashboardKeyStatisticsCount
    {
        public decimal TotalSales { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalWastage { get; set; }
    }
    public class InventoryDashboardStockLevel
    {
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class InventoryDashboardGetCOGSCount
    {
        public decimal WorthofStockConsumed { get; set; }
       
    }
    public class InventoryDashboardPending_Purchases_Approvals
    {
        public decimal PendingPurchases { get; set; }
        public decimal PendingApprovals { get; set; }


    }
}
