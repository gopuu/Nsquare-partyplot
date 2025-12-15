using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Web.Helpers;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetMonthlyUsageController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetMonthlyUsageController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }
        [HttpPost]
        [Route("GetAssetMonthlyUsage")]
        public IActionResult GetAssetMonthlyUsage([FromBody] GetAssetMonthlyUsageRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetAssetMonthlyUsageResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetMonthlyUsage", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);
                    cmd.Parameters.AddWithValue("@AssetId", request?.AssetId ?? 0);
                    cmd.Parameters.AddWithValue("@UsageMonth", request?.UsageMonth ?? 0);
                    cmd.Parameters.AddWithValue("@UsageYear", request?.UsageYear ?? 0);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                    con.Close();
                }

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new GetAssetMonthlyUsageResponse
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        AssetId = Convert.ToInt64(dr["AssetId"]),
                        AssetName = Convert.ToString(dr["AssetName"]),
                        UsageMonth = Convert.ToInt32(dr["UsageMonth"]),
                        UsageYear = Convert.ToInt32(dr["UsageYear"]),
                        EventsUsedCount = Convert.ToInt64(dr["EventsUsedCount"]),
                        DamageCount = Convert.ToInt64(dr["DamageCount"]),
                        Condition_Start_StatusId = dr["Condition_Start_StatusId"] == DBNull.Value ? null : Convert.ToInt64(dr["Condition_Start_StatusId"]),
                        Condition_End_StatusId = dr["Condition_End_StatusId"] == DBNull.Value ? null : Convert.ToInt64(dr["Condition_End_StatusId"]),
                        OpeningQty = Convert.ToDecimal(dr["OpeningQty"]),
                        ClosingQty = Convert.ToDecimal(dr["ClosingQty"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Asset monthly usage fetched successfully!";
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
        [Route("SaveAssetMonthlyUsage")]
        public IActionResult SaveAssetMonthlyUsage([FromBody] SaveAssetMonthlyUsageRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                long id;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAssetMonthlyUsage", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@AssetId", model.AssetId);
                    cmd.Parameters.AddWithValue("@UsageMonth", model.UsageMonth);
                    cmd.Parameters.AddWithValue("@UsageYear", model.UsageYear);
                    cmd.Parameters.AddWithValue("@EventsUsedCount", model.EventsUsedCount);
                    cmd.Parameters.AddWithValue("@DamageCount", model.DamageCount);
                    cmd.Parameters.AddWithValue("@Condition_Start_StatusId", model.Condition_Start_StatusId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Condition_End_StatusId", model.Condition_End_StatusId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OpeningQty", model.OpeningQty);
                    cmd.Parameters.AddWithValue("@ClosingQty", model.ClosingQty);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    id = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Asset monthly usage updated successfully!"
                    : "Asset monthly usage created successfully!";
                apiResponse.data = new { AssetMonthlyUsageId = id };

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

    }
}

