namespace ERPAdmin.Models
{
    public class FoodWastage
    {
        public long FoodWastageId { get; set; }
        public long MenuItem_Id { get; set; }
        public string? MenuItem_Name { get; set; }
        public decimal WastageKg { get; set; }
        public long LastUpdated_By { get; set; }

    }
    public class GetFoodWastageByBookingIdRequest
    {
        public long BookingId { get; set; }
    }

    public class SaveFoodWastageRequest
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
      
        public string? FoodJson { get; set; }
    }

}
