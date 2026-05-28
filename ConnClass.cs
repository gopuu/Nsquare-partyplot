using System.Data;
using System.Data.SqlClient;
using ERPAdmin.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.Ocsp;

namespace ERPAdmin
{
    public class ConnClass
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _masterConnString;
        private string _childConnString = string.Empty;
        private string _domain = string.Empty;
        private string _errorLog = string.Empty;

        public ConnClass(IHttpContextAccessor httpContextAccessor, string masterConnString)
        {
            _httpContextAccessor = httpContextAccessor;
            _masterConnString = masterConnString;
            SetChildConnectionString();
        }

        private void SetChildConnectionString()
        {
            try
            {
                string domainOrCode = GetDomainOrTenant(); //_httpContextAccessor.HttpContext?.Request.Host.Value.Replace(":", "") ?? "Unknown";

                using (SqlConnection conn = new SqlConnection(_masterConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("GetAdminClientByDomainOrCode", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DomainOrCode", domainOrCode);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapter.Fill(ds);

                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                _childConnString = ds.Tables[0].Rows[0]["connection_string"].ToString() ?? "";
                                _domain = ds.Tables[0].Rows[0]["domain"].ToString() ?? "";
                                _errorLog = ds.Tables[0].Rows[0]["reason"].ToString() ?? "";

                                if (!string.IsNullOrEmpty(_domain))
                                {
                                    // Define base paths
                                    //string quotationPath = Path.Combine("wwwroot", "Quotation", "org", _domain);
                                    string uploadsPath = Path.Combine("wwwroot", "uploads", "org", _domain);

                                    // Ensure domain folder in Quotation (no subfolders)
                                    //if (!Directory.Exists(quotationPath))
                                    //    Directory.CreateDirectory(quotationPath);

                                    // Ensure domain folder in uploads (with subfolders)
                                    if (!Directory.Exists(uploadsPath))
                                    {
                                        Directory.CreateDirectory(uploadsPath);
                                        string[] folderNames = {
                                            "CompanyProfile_PanCard_Image", "Company_HallRentAndDetails_Images", "CompanyInvoice_CompanyLogo_Image"
                                        };

                                        foreach (string folderName in folderNames)
                                        {
                                            string folderPath = Path.Combine(uploadsPath, folderName);
                                            if (!Directory.Exists(folderPath))
                                                Directory.CreateDirectory(folderPath);
                                        }
                                    }
                                    //Console.WriteLine("Directories and folders created successfully.");
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _errorLog = ex.Message;
                _childConnString = string.Empty;
            }
        }
        public string GetDomainOrTenant()
        {
            // 1️⃣ First, check custom header (For Mobile Apps)
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request?.Headers.ContainsKey("X-Client-Domain") == true)
            {
                return request.Headers["X-Client-Domain"].ToString();
            }

            // 2️⃣ If no header, check JWT claims (For Authenticated Users)
            var userClaims = _httpContextAccessor.HttpContext?.User;
            if (userClaims != null && userClaims.HasClaim(c => c.Type == "Tenant"))
            {
                return userClaims.FindFirst("Tenant")?.Value ?? "Unknown";
            }

            // 3️⃣ Check API URL (For URL-based Tenant Identifiers)
            if (request != null)
            {
                string fullUrl = request.Path.Value.ToLower();  // e.g., /api/client123/account
                string queryString = request.QueryString.Value.ToLower();  // e.g., ?domain=client123

                // Extract from URL Path (Assuming pattern: /api/{tenant}/resource)
                var pathSegments = fullUrl.Split('/');
                if (pathSegments.Length > 2 && pathSegments[1] != "api")
                {
                    return pathSegments[1]; // This assumes the second segment is the tenant
                }

                // Extract from Query Parameter (e.g., /api/account?domain=client123)
                if (request.Query.ContainsKey("domain"))
                {
                    return request.Query["domain"].ToString();
                }
            }

            // 4️⃣ Fallback to Request.Host (For Web Requests)
            return request?.Host.Value.Replace(":", "") ?? "UnknownDomain";
        }
        public string GetChildConnectionString()
        {
            return _childConnString;
        }
        public string GetChildErrorLog()
        {
            return _errorLog;
        }
        public string GetDomainName()
        {
            return _domain;
        }
    }
}
