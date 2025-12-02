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
    public class Company_FoodRate_OtherChargeController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_FoodRate_OtherChargeController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
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
        [Route("DeleteCompanyFoodRateOtherCharge")]
        public IActionResult DeleteCompanyFoodRateOtherCharge([FromBody] DeleteCompanyFoodRateOtherChargeRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("DeleteCompanyFoodRateOtherCharge", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.message = "Other charge deleted successfully!";
                apiResponse.status = "Success";
                apiResponse.data = null;
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetCompanyFoodRateOtherCharge")]
        public IActionResult GetCompanyFoodRateOtherCharge([FromBody] GetCompanyFoodRateOtherChargeRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            List<CompanyFoodRateOtherCharge> charges = new List<CompanyFoodRateOtherCharge>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyFoodRateOtherCharge", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object)model.Id ?? 0;
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var charge = new CompanyFoodRateOtherCharge
                            {
                                Id = Convert.ToInt64(reader["id"]),
                                ServiceName = Convert.ToString(reader["service_name"]) ?? "",
                                ChargeType = Convert.ToString(reader["charge_type"]) ?? "",
                                Amount = Convert.ToDecimal(reader["amount"]),
                                TaxType = Convert.ToString(reader["tax_type"]) ?? "",
                                GSTRate = reader["gst_rate"] != DBNull.Value ? Convert.ToDecimal(reader["gst_rate"]) : (decimal?)null,
                                HSNSAC = Convert.ToString(reader["hsn_sac"]) ?? "",
                                IsFixedService = reader["isFixedService"] != DBNull.Value
                                                ? Convert.ToBoolean(reader["isFixedService"])
                                                : false, 
                                TallyLedgerName = Convert.ToString(reader["tally_ledger_name"]) ?? ""
                            };
                            charges.Add(charge);
                        }
                    }
                    con.Close();
                }

                apiResponse.message = "Other charges fetched successfully!";
                apiResponse.data = charges;
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error occurred. Details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyFoodRateOtherCharge")]
        public IActionResult SaveCompanyFoodRateOtherCharge([FromBody] SaveCompanyFoodRateOtherChargeWrapper model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    foreach (var charge in model.Charges)
                    {
                        using (SqlCommand cmd = new SqlCommand("SaveCompanyFoodRateOtherCharge", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object)charge.Id ?? DBNull.Value;
                            cmd.Parameters.Add("@service_name", SqlDbType.NVarChar, 200).Value = charge.ServiceName ?? string.Empty;
                            cmd.Parameters.Add("@charge_type", SqlDbType.NVarChar, 50).Value = (object)charge.ChargeType ?? DBNull.Value;
                            var amountParam = cmd.Parameters.Add("@amount", SqlDbType.Decimal);
                            amountParam.Precision = 10;
                            amountParam.Scale = 2;
                            amountParam.Value = (object)charge.Amount ?? DBNull.Value;

                            cmd.Parameters.Add("@tax_type", SqlDbType.NVarChar, 10).Value = (object)charge.TaxType ?? DBNull.Value;

                            var gstRateParam = cmd.Parameters.Add("@gst_rate", SqlDbType.Decimal);
                            gstRateParam.Precision = 5;
                            gstRateParam.Scale = 2;
                            gstRateParam.Value = (object)charge.GSTRate ?? DBNull.Value;

                            cmd.Parameters.Add("@hsn_sac", SqlDbType.NVarChar, 50).Value = (object)charge.HSNSAC ?? DBNull.Value;
                            cmd.Parameters.Add("@tally_ledger_name", SqlDbType.NVarChar, 200).Value = (object)charge.TallyLedgerName ?? DBNull.Value;
                            cmd.Parameters.Add("@isFixedService", SqlDbType.Bit).Value = charge.IsFixedService;
                            cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;

                            cmd.ExecuteNonQuery();
                        }
                    }
                    con.Close();
                }

                apiResponse.message = "Charges saved successfully!";
                apiResponse.status = "Success";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error occurred. Details: " + ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }


    }
}
// Model class
public class CompanyFoodRateOtherCharge
{
    public long Id { get; set; }
    public string ServiceName { get; set; }
    public string ChargeType { get; set; }
    public decimal Amount { get; set; }
    public string TaxType { get; set; }
    public decimal? GSTRate { get; set; }
    public string HSNSAC { get; set; }
    public bool? IsFixedService { get; set; }
    public string TallyLedgerName { get; set; }
}
public class SaveCompanyFoodRateOtherChargeWrapper : BaseClass
{
    public List<SaveCompanyFoodRateOtherChargeRequest> Charges { get; set; } = new();
}

public class SaveCompanyFoodRateOtherChargeRequest
{
    public long? Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? ChargeType { get; set; }
    public decimal Amount { get; set; }
    public string? TaxType { get; set; }
    public decimal? GSTRate { get; set; }
    public string? HSNSAC { get; set; }
    public bool? IsFixedService { get; set; }
    public string? TallyLedgerName { get; set; }
}

public class DeleteCompanyFoodRateOtherChargeRequest : BaseClass
{
    public long Id { get; set; }
}
public class GetCompanyFoodRateOtherChargeRequest
{
    public long? Id { get; set; }
}

