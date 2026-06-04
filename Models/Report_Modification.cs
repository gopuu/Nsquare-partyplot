using System.Collections.Generic;

namespace ERPAdmin.Models
{
    public class GetModuleMasterForModificationReportResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class ModificationReportGridRequest
    {
        public long? ModuleId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class ModificationReportRequest : BaseClass
    {
        public long? ModuleId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class ModificationReportRequest_PdfExcel : BaseClass
    {
        public long? ModuleId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
    }

    public class ModificationLineItem
    {
        public long SrNo { get; set; }
        public string ModuleName { get; set; }
        public long RowId { get; set; }
        public string ReferenceNo { get; set; }
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public decimal ImpactAmount { get; set; }
        public string ModifiedDateTime { get; set; }
        public string ModifiedDateTime_Format { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class ModificationSummary
    {
        public long TotalModifications { get; set; }
        public decimal TotalOldAmount { get; set; }
        public decimal TotalNewAmount { get; set; }
        public decimal TotalImpactAmount { get; set; }
    }

    public class ModificationGridResponse
    {
        public List<ModificationLineItem> LineItems { get; set; } = new();
        public ModificationSummary Summary { get; set; } = new();
    }
}
