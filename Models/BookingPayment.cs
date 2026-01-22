namespace ERPAdmin.Models
{
    public class BookingPayment
    {
        public long PaymentId { get; set; }
        public long BookingId { get; set; }
        public long ModeId { get; set; }
        public string? DateOfPayment { get; set; }
        public string? PaymentDateTimeFormat { get; set; }

        public TimeSpan? PaymentTime { get; set; }
        public string? TxnId { get; set; }
        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }
        public decimal Amount { get; set; }
        public decimal DueBalance { get; set; }
        public string? NextDueDate { get; set; }

    }
    public class GetBookingPayments_Request
    {
        public long? PaymentId { get; set; }
        public long? BookingId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    public class SaveBookingPaymentRequest : BaseClass
    {
        public long BookingId { get; set; }
        public long ModeId { get; set; }
        public string? DateOfPayment { get; set; }
        public string? PaymentTime { get; set; }
        public string? TxnId { get; set; }
        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }
        public decimal Amount { get; set; }
        public decimal DueBalance { get; set; }
        public string? NextDueDate { get; set; }

    }

}
