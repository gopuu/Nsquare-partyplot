using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using System.Xml.Linq;
using ExcelDataReader.Log;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Company_ProfileAndPropertyDetailsController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_ProfileAndPropertyDetailsController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";

            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("DeleteCompanyProfileAndPropertyDetails")]
        public IActionResult DeleteCompanyProfileAndPropertyDetails([FromBody] long id)
        {
            var apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("DeleteCompanyProfileAndPropertyDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Company profile deleted successfully.";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpGet]
        [Route("GetCompanyProfileAndPropertyDetailsList")]
        public IActionResult GetCompanyProfileAndPropertyDetailsList() //([FromBody] GetCompanyProfileRequest request)
        {
            var apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                var list = new List<dynamic>();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyProfileAndPropertyDetailsList", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = (object?)request.Id ?? DBNull.Value;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new
                            {
                                Id = reader["id"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["id"]),
                                CompanyName = reader["CompanyName"] == DBNull.Value ? null : reader["CompanyName"].ToString(),
                                BanquetType = reader["BanquetType"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["BanquetType"]),
                                CompanyState = reader["CompanyState"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["CompanyState"]),
                                CompanyCity = reader["CompanyCity"] == DBNull.Value ? null : reader["CompanyCity"].ToString(),
                                CompanyEmail = reader["CompanyEmail"] == DBNull.Value ? null : reader["CompanyEmail"].ToString(),
                                CompanyAddress = reader["CompanyAddress"] == DBNull.Value ? null : reader["CompanyAddress"].ToString(),
                                CompanyPanNo = reader["CompanyPanNo"] == DBNull.Value ? null : reader["CompanyPanNo"].ToString(),

                                CompanyPanCardUpload = reader["CompanyPanCardUpload"] != DBNull.Value
                                                        && !string.IsNullOrEmpty(reader["CompanyPanCardUpload"].ToString())
                                                    ? domainUrl + $"/uploads/org/{domain}/CompanyProfile_PanCard_Image/" + reader["CompanyPanCardUpload"].ToString()
                                                    : string.Empty,

                                CompanyMobile = reader["CompanyMobile"] == DBNull.Value ? null : reader["CompanyMobile"].ToString(),
                                ReviewUrl = reader["ReviewUrl"] == DBNull.Value ? null : reader["ReviewUrl"].ToString(),
                                AcceptGST = reader["AcceptGST"] == DBNull.Value ? false : Convert.ToBoolean(reader["AcceptGST"]),
                                GSTIN = reader["GSTIN"] == DBNull.Value ? null : reader["GSTIN"].ToString(),
                                VATNumber = reader["VATNumber"] == DBNull.Value ? null : reader["VATNumber"].ToString(),
                                QuotationsWithTax = reader["QuotationsWithTax"] == DBNull.Value ? false : Convert.ToBoolean(reader["QuotationsWithTax"]),
                                MultipleFoodGST = reader["MultipleFoodGST"] == DBNull.Value ? false : Convert.ToBoolean(reader["MultipleFoodGST"]),
                                PrintCustomerDetails = reader["PrintCustomerDetails"] == DBNull.Value ? false : Convert.ToBoolean(reader["PrintCustomerDetails"]),

                                PropertyCity = reader["PropertyCity"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["PropertyCity"]),
                                Locality = reader["Locality"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["Locality"]),
                                Pincode = reader["Pincode"] == DBNull.Value ? null : reader["Pincode"].ToString(),
                                ManagerName = reader["ManagerName"] == DBNull.Value ? null : reader["ManagerName"].ToString(),
                                PropertyName = reader["PropertyName"] == DBNull.Value ? null : reader["PropertyName"].ToString(),
                                PropertyType = reader["PropertyType"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["PropertyType"]),
                                PropertyEmail = reader["PropertyEmail"] == DBNull.Value ? null : reader["PropertyEmail"].ToString(),
                                PropertyAddress = reader["PropertyAddress"] == DBNull.Value ? null : reader["PropertyAddress"].ToString(),
                                ManagerMobile = reader["ManagerMobile"] == DBNull.Value ? null : reader["ManagerMobile"].ToString(),
                                MonopolyPreference = reader["MonopolyPreference"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["MonopolyPreference"]),
                                HallRatesNegotiable = reader["HallRatesNegotiable"] == DBNull.Value ? false : Convert.ToBoolean(reader["HallRatesNegotiable"]),
                                FoodPreference = reader["FoodPreference"] == DBNull.Value ? false : Convert.ToBoolean(reader["FoodPreference"]),
                                // Split comma-separated strings into arrays
                                Facilities = reader["Facilities"]?.ToString()?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                                Rules = reader["Rules"]?.ToString()?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                                SearchLocation = reader["SearchLocation"] == DBNull.Value ? null : reader["SearchLocation"].ToString(),
                                BookingSteps = reader["BookingSteps"]?.ToString()?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                                AboutUsAndTnC = reader["AboutUsAndTnC"] == DBNull.Value ? null : reader["AboutUsAndTnC"].ToString(),
                            });
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Company profiles fetched successfully.";
                apiResponse.data = list;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null ?? "");
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyProfile")]
        public IActionResult SaveCompanyProfile([FromBody] SaveCompanyProfileRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                #region FILE VALIDATION

                if (!string.IsNullOrWhiteSpace(model.CompanyPanCardUpload))
                {
                    try
                    {
                        _imageUploadService.ValidateBase64File(
                            model.CompanyPanCardUpload,
                            new[] { ".pdf", ".jpg", ".jpeg", ".png" },
                            1
                        );
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new apiResponse
                        {
                            status = "Error",
                            message = $"PAN Card file: {ex.Message}"
                        });
                    }
                }

                #endregion

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    #region SAVE COMPANY PROFILE

                    using (SqlCommand cmd = new SqlCommand("SaveCompanyProfile", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 200).Value = (object?)model.CompanyName ?? DBNull.Value;
                        cmd.Parameters.Add("@BanquetType", SqlDbType.BigInt).Value = model.BanquetType;
                        cmd.Parameters.Add("@CompanyState", SqlDbType.BigInt).Value = model.CompanyState;
                        cmd.Parameters.Add("@CompanyCity", SqlDbType.NVarChar, 200).Value = (object?)model.CompanyCity ?? DBNull.Value;
                        cmd.Parameters.Add("@CompanyEmail", SqlDbType.NVarChar, 200).Value = (object?)model.CompanyEmail ?? DBNull.Value;
                        cmd.Parameters.Add("@CompanyAddress", SqlDbType.NVarChar, 240).Value = (object?)model.CompanyAddress ?? DBNull.Value;
                        cmd.Parameters.Add("@CompanyPanNo", SqlDbType.NVarChar, 10).Value = (object?)model.CompanyPanNo ?? DBNull.Value;
                        cmd.Parameters.Add("@CompanyMobile", SqlDbType.NVarChar, 10).Value = (object?)model.CompanyMobile ?? DBNull.Value;
                        cmd.Parameters.Add("@ReviewUrl", SqlDbType.NVarChar, -1).Value = (object?)model.ReviewUrl ?? DBNull.Value;

                        cmd.Parameters.Add("@AcceptGST", SqlDbType.Bit).Value = model.AcceptGST;
                        cmd.Parameters.Add("@GSTIN", SqlDbType.NVarChar, 15).Value = (object?)model.GSTIN ?? DBNull.Value;
                        cmd.Parameters.Add("@VATNumber", SqlDbType.NVarChar, 15).Value = (object?)model.VATNumber ?? DBNull.Value;

                        cmd.Parameters.Add("@QuotationsWithTax", SqlDbType.Bit).Value = model.QuotationsWithTax;
                        cmd.Parameters.Add("@MultipleFoodGST", SqlDbType.Bit).Value = model.MultipleFoodGST;
                        cmd.Parameters.Add("@PrintCustomerDetails", SqlDbType.Bit).Value = model.PrintCustomerDetails;

                        cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                        rowId = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    #endregion

                    #region PAN CARD UPLOAD

                    if (!string.IsNullOrWhiteSpace(model.CompanyPanCardUpload))
                    {
                        string fileType = _imageUploadService.GetFileTypeFromBase64(model.CompanyPanCardUpload);
                        string folderPath = $"org/{domain}/CompanyProfile_PanCard_Image/";
                        string filePrefix = $"Company_{rowId}_PanCard";

                        _imageUploadService.DeleteImages(folderPath, filePrefix);

                        string fileName = _imageUploadService.UploadImage(
                            model.CompanyPanCardUpload,
                            folderPath,
                            filePrefix,
                            fileType
                        );

                        using (SqlCommand fileCmd = new SqlCommand("SaveCompanyPanCardUpload", con))
                        {
                            fileCmd.CommandType = CommandType.StoredProcedure;

                            fileCmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = rowId;
                            fileCmd.Parameters.Add("@CompanyPanCardUpload", SqlDbType.NVarChar, -1).Value = fileName;

                            fileCmd.ExecuteNonQuery();
                        }

                        model.CompanyPanCardUpload = fileName;
                    }

                    #endregion
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Company profile updated successfully!"
                    : "Company profile added successfully!";

                apiResponse.data = new
                {
                    CompanyId = rowId
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyPropertyDetails")]
        public IActionResult SaveCompanyPropertyDetails([FromBody] SaveCompanyPropertyRequest model)
        {
            var apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SaveCompanyPropertyDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Convert lists to comma-separated strings
                    string facilitiesString = model.Facilities != null ? string.Join("|", model.Facilities) : "";
                    string rulesString = model.Rules != null ? string.Join("|", model.Rules) : "";
                    string bookingStepsString = model.BookingSteps != null ? string.Join("|", model.BookingSteps) : "";

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@PropertyCity", SqlDbType.BigInt).Value = model.PropertyCity;
                    cmd.Parameters.Add("@Locality", SqlDbType.BigInt).Value = model.Locality;
                    cmd.Parameters.Add("@Pincode", SqlDbType.NVarChar, 6).Value = model.Pincode;
                    cmd.Parameters.Add("@ManagerName", SqlDbType.NVarChar, 200).Value = model.ManagerName;
                    cmd.Parameters.Add("@PropertyName", SqlDbType.NVarChar, 200).Value = model.PropertyName;
                    cmd.Parameters.Add("@PropertyType", SqlDbType.BigInt).Value = model.PropertyType;
                    cmd.Parameters.Add("@PropertyEmail", SqlDbType.NVarChar, 200).Value = model.PropertyEmail;
                    cmd.Parameters.Add("@PropertyAddress", SqlDbType.NVarChar, 200).Value = model.PropertyAddress;
                    cmd.Parameters.Add("@ManagerMobile", SqlDbType.NVarChar, 10).Value = model.ManagerMobile;
                    cmd.Parameters.Add("@MonopolyPreference", SqlDbType.BigInt).Value = model.MonopolyPreference;
                    cmd.Parameters.Add("@HallRatesNegotiable", SqlDbType.Bit).Value = model.HallRatesNegotiable;
                    cmd.Parameters.Add("@FoodPreference", SqlDbType.Bit).Value = model.FoodPreference;
                    cmd.Parameters.Add("@Facilities", SqlDbType.NVarChar, -1).Value = facilitiesString;
                    cmd.Parameters.Add("@Rules", SqlDbType.NVarChar, -1).Value = rulesString;
                    cmd.Parameters.Add("@BookingSteps", SqlDbType.NVarChar, -1).Value = bookingStepsString;
                    cmd.Parameters.Add("@SearchLocation", SqlDbType.NVarChar, 500).Value = model.SearchLocation;
                    cmd.Parameters.Add("@AboutUsAndTnC", SqlDbType.NVarChar).Value = model.AboutUsAndTnC;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Company Property Data Saved!"; // model.Id == 0 ? "Company profile added successfully." : "Company profile updated successfully.";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }
    }
}
