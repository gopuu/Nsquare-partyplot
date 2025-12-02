namespace ERPAdmin.Models
{
    public class Dashboard
    {
    }
    public class ProcessDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalDeals { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedDate { get; set; }
        public List<DealDto> Deals { get; set; }
    }

    public class DealDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Customer { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
    }

}
