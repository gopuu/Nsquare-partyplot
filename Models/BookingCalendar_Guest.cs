namespace ERPAdmin.Models
{
    //GET
    public class GetBookingCalendar_GuestRequest
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
    }
    public class BookingCalendar_GuestModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string WhatsappContact { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public long CityId { get; set; }
        public string CityName { get; set; }
        public string Dob { get; set; }
        public string Dob2 { get; set; }
        public string AnniversaryDate { get; set; }
        public string AnniversaryDate2 { get; set; }
        public long BookingId { get; set; }
    }

    //SAVE
    public class SaveBookingCalendar_GuestRequest : BaseClass
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string WhatsappContact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public long CityId { get; set; } = 0;
        public string Dob { get; set; } = string.Empty;
        public string AnniversaryDate { get; set; } = string.Empty;
        public long BookingId { get; set; } = 0;
    }

    //DELETE
    public class DeleteBookingCalendar_GuestRequest : BaseClass
    {
        public long Id { get; set; }
    }
}
