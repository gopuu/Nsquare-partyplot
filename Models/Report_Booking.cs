namespace ERPAdmin.Models
{

    //GRID RESPONSE
    public class BookingReportGridModel
    {
        public long BRN { get; set; }

        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }

        public string? EventTypeName { get; set; }
        public string? EventSubTypeName { get; set; }
        public string? EventDisplay { get; set; }

        public string? HallName { get; set; }

        public int Pax { get; set; }

        public string? Package { get; set; }

        public decimal DealAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InvoiceAmount { get; set; }

        public string? Client_Name { get; set; }
        public string? Client_Contact { get; set; }

        public long? InvoiceNo { get; set; }
        public string? GSTInvoiceNo { get; set; }
    }
    public class BookingReportRequest : BaseClass
    {
        public string? NameEmailContactNo { get; set; } = string.Empty;
        public long EventTypeId { get; set; } = 0;
        public long EventSubTypeId { get; set; } = 0;
        public long HallId { get; set; } = 0;

        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }

        public bool? WithHeader { get; set; }

        // 🔹 NEW
        public List<string>? Columns { get; set; }
    }
    
    public class BookingReportRequestGRID : BaseClass
    {
        public string? NameEmailContactNo { get; set; } = string.Empty;
        public long EventTypeId { get; set; } = 0;
        public long EventSubTypeId { get; set; } = 0;
        public long HallId { get; set; } = 0;

        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? IsInvoiceGenerated { get; set; } = null;
    }
}

