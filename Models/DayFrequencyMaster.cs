namespace ERPAdmin.Models
{
    public class GetDayFrequencyRequest
    {
        public long Id { get; set; }
    }
    public class GetDayFrequencyList
    {
        public long Id { get; set; }
        public string FrequencyTitle { get; set; }
    }
    public class SaveDayFrequency : BaseClass
    {
        public long Id { get; set; }
        public string FrequencyTitle { get; set; }
    }
    public class DeleteDayFrequency : BaseClass
    {
        public long Id { get; set; }
    }

}
