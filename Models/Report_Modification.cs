using System.Collections.Generic;

namespace ERPAdmin.Models
{
    public class ModificationReportGridRequest
    {
        public string? Module { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class ModificationReportRequest
    {
        public string? Module { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class ModificationLineItem
    {
        public long SrNo { get; set; }
        public string? ModDate { get; set; }
        public string? ModDate_Format { get; set; }
        public string? Module { get; set; }
        public string? RefNo { get; set; }
        public decimal Amount { get; set; }
    }

    public class ModificationSummary
    {
        public int TotalModifications { get; set; }
        public int SalesCount { get; set; }
        public int PurchaseCount { get; set; }
        public int ExpenseCount { get; set; }
        public int IncomeCount { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ModificationGridResponse
    {
        public List<ModificationLineItem> LineItems { get; set; } = new();
        public ModificationSummary Summary { get; set; } = new();
    }
}
