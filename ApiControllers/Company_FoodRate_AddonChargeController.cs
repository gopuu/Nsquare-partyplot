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
    public class Company_FoodRate_AddonChargeController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_FoodRate_AddonChargeController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
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
        [Route("DeleteCompanyFoodRateAddonCharge_Header")]
        public IActionResult DeleteCompanyFoodRateAddonCharge_Header([FromBody] DeleteCompanyFoodRateAddonChargeRequest_Header model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteCompanyFoodRateAddonCharge_Header", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = model.ActionBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                apiResponse.message = "Addon charge deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteCompanyFoodRateAddonCharge_Items")]
        public IActionResult DeleteCompanyFoodRateAddonCharge_Items([FromBody] DeleteCompanyFoodRateAddonChargeRequest_Items model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteCompanyFoodRateAddonCharge_Items", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = model.ActionBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                apiResponse.message = "Addon charge deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetCompanyFoodRateAddonCharge")]
        public IActionResult GetCompanyFoodRateAddonCharge([FromBody] GetCompanyFoodRateAddonChargeRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            List<dynamic> addonCharges = new List<dynamic>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyFoodRateAddonCharge", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? (object)DBNull.Value;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var addonCharge = new
                        {
                            Id = Convert.ToInt64(reader["id"]),
                            HeaderId = Convert.ToInt64(reader["header_id"]),
                            HeaderName = Convert.ToString(reader["header_name"]),
                            ItemName = Convert.ToString(reader["item_name"]),
                            Amount = Convert.ToDecimal(reader["amount"]),
                            Description = Convert.ToString(reader["description"])
                        };
                        addonCharges.Add(addonCharge);
                    }
                    con.Close();
                }

                if (addonCharges.Count > 0)
                {
                    apiResponse.message = "Addon charges fetched successfully!";
                    apiResponse.data = addonCharges;
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "No addon charges found.";
                    apiResponse.data = null;
                    apiResponse.status = "Fail";
                }

                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyFoodRateAddonCharge")]
        public IActionResult SaveCompanyFoodRateAddonCharge([FromBody] SaveCompanyFoodRateAddonChargeWrapper model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    foreach (var charge in model.AddonCharges)
                    {
                        SqlCommand cmd = new SqlCommand("SaveCompanyFoodRateAddonCharge", con);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = charge.Id ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@header_name", SqlDbType.NVarChar, 200).Value = charge.HeaderName ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@item_name", SqlDbType.NVarChar, 200).Value = charge.ItemName;
                        cmd.Parameters.Add("@amount", SqlDbType.Decimal).Value = charge.Amount;
                        cmd.Parameters["@amount"].Precision = 10;
                        cmd.Parameters["@amount"].Scale = 2;
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value = charge.Description ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;

                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }

                apiResponse.message = "Addon charges saved successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

    }
}

public class DeleteCompanyFoodRateAddonChargeRequest_Items : BaseClass
{
    public long Id { get; set; }
}
public class DeleteCompanyFoodRateAddonChargeRequest_Header : BaseClass
{
    public long Id { get; set; }
}
public class GetCompanyFoodRateAddonChargeRequest
{
    public long? Id { get; set; }
}
public class SaveCompanyFoodRateAddonChargeWrapper : BaseClass
{
    public List<SaveCompanyFoodRateAddonChargeRequest>? AddonCharges { get; set; }
}

public class SaveCompanyFoodRateAddonChargeRequest
{
    public long? Id { get; set; }
    public string? HeaderName { get; set; }
    public string? ItemName { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}


