namespace ERPAdmin.Helper
{
    public class apiResponse
    {
        public string status {  get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public dynamic? data { get; set; } = null;
    }
}
