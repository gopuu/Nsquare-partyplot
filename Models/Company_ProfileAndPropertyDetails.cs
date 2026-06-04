namespace ERPAdmin.Models
{

    public class GetCompanyProfileRequest
    {
        public long? Id { get; set; }
    }
    public class SaveCompanyProfileRequest : BaseClass
    {
        public long Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public long BanquetType { get; set; }
        public long CompanyState { get; set; }
        public string CompanyCity { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPanNo { get; set; } = string.Empty;
        public string CompanyPanCardUpload { get; set; } = string.Empty;
        public string CompanyMobile { get; set; } = string.Empty;
        public string ReviewUrl { get; set; } = string.Empty;
        public bool AcceptGST { get; set; }
        public string GSTIN { get; set; } = string.Empty;
        public string VATNumber { get; set; } = string.Empty;
        public bool QuotationsWithTax { get; set; }
        public bool MultipleFoodGST { get; set; }
        public bool PrintCustomerDetails { get; set; }
    }

    public class SaveCompanyPropertyRequest : BaseClass
    {
        public long Id { get; set; }
        public long PropertyCity { get; set; }
        public long Locality { get; set; }
        public string Pincode { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public long PropertyType { get; set; }
        public string PropertyEmail { get; set; } = string.Empty;
        public string PropertyAddress { get; set; } = string.Empty;
        public string ManagerMobile { get; set; } = string.Empty;
        public long MonopolyPreference { get; set; }
        public bool HallRatesNegotiable { get; set; }
        public bool FoodPreference { get; set; }
        public List<string>? Facilities { get; set; }
        public List<string>? Rules { get; set; }
        public List<string>? BookingSteps { get; set; }
        public string SearchLocation { get; set; } = string.Empty;
        public string AboutUsAndTnC { get; set; } = string.Empty;
    }


}
