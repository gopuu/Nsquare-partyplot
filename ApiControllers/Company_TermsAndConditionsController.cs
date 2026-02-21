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
    public class Company_TermsAndConditionsController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_TermsAndConditionsController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
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
        [Route("DeleteCompanyTermsAndConditions")]
        public IActionResult DeleteCompanyTermsAndConditions([FromBody] DeleteTermsRequest request)
        {
            var apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("DeleteCompanyTermsAndConditions", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = request.Id;
                    cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = request.DeletedBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Terms and Conditions deleted successfully.";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null ?? "");
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        public class DeleteTermsRequest
        {
            public long Id { get; set; }
            public long DeletedBy { get; set; }
        }

        [HttpGet]
        [Route("GetCompanyTermsAndConditions")]
        public IActionResult GetCompanyTermsAndConditions()
        {
            var apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                var list = new List<dynamic>();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyTermsAndConditions", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new
                            {
                                Id = reader["id"] as long? ?? 0,
                                PrintInProspectus = reader["print_in_prospectus"],
                                PrintInInvoice = reader["print_in_invoice"],
                                TermsContent = reader["terms_content"]
                            });
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Terms and Conditions fetched successfully.";
                apiResponse.data = list;
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

        [HttpPost]
        [Route("SaveCompanyTermsAndConditions")]
        public IActionResult SaveCompanyTermsAndConditions([FromBody] SaveTermsRequest model)
        {
            var apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SaveCompanyTermsAndConditions", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object?)model.Id ?? DBNull.Value;
                    cmd.Parameters.Add("@print_in_prospectus", SqlDbType.Bit).Value = model.PrintInProspectus;
                    cmd.Parameters.Add("@print_in_invoice", SqlDbType.Bit).Value = model.PrintInInvoice;
                    cmd.Parameters.Add("@terms_content", SqlDbType.NVarChar).Value = (object?)model.TermsContent ?? DBNull.Value;
                    cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = (object?)model.ActionBy ?? DBNull.Value;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id == null ? "Terms and Conditions added successfully." : "Terms and Conditions updated successfully.";
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

        public class SaveTermsRequest
        {
            public long? Id { get; set; }
            public bool PrintInProspectus { get; set; }
            public bool PrintInInvoice { get; set; }
            public string? TermsContent { get; set; }
            public long? ActionBy { get; set; }
        }

    }
}


