namespace ERPAdmin.Models
{
    public class MonthList
    {
        public Int64 MonthId { get; set; }
        public string? MonthName { get; set; }
        public Int64 YearId { get; set; }
        public string? YearName { get; set; }
    }
    public class MonthList_Request
    {
        public Int64? MonthId { get; set; }
        public Int64? YearId { get; set; }
    }

}
