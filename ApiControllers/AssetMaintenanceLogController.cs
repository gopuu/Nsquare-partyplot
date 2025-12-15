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
    public class AssetMaintenanceLogController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetMaintenanceLogController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAssetMaintenanceLog")]
        public IActionResult GetAssetMaintenanceLog([FromBody] GetAssetMaintenanceLogRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetAssetMaintenanceLogResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetMaintenanceLog", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);
                    cmd.Parameters.AddWithValue("@AssetId", request?.AssetId ?? 0);
                    cmd.Parameters.AddWithValue("@BookingId", request?.BookingId ?? 0);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                    con.Close();
                }

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new GetAssetMaintenanceLogResponse
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        AssetId = Convert.ToInt64(dr["AssetId"]),
                        AssetName = Convert.ToString(dr["AssetName"]),
                        MaintenanceDate = Convert.ToDateTime(dr["MaintenanceDate"]).ToString("yyyy-MM-dd"),
                        IsMaintenanceDone = Convert.ToBoolean(dr["IsMaintenanceDone"]),
                        MaintenanceCost = Convert.ToDecimal(dr["MaintenanceCost"]),
                        Remarks = dr["Remarks"] == DBNull.Value ? null : Convert.ToString(dr["Remarks"]),
                        BookingId = dr["BookingId"] == DBNull.Value ? null : Convert.ToInt64(dr["BookingId"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Asset maintenance logs fetched successfully!";
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
        [Route("SaveAssetMaintenanceLog")]
        public IActionResult SaveAssetMaintenanceLog([FromBody] SaveAssetMaintenanceLogRequest model)
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
                using (SqlCommand cmd = new SqlCommand("SaveAssetMaintenanceLog", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@AssetId", model.AssetId);
                    cmd.Parameters.AddWithValue("@MaintenanceDate", model.MaintenanceDate);
                    cmd.Parameters.AddWithValue("@IsMaintenanceDone", model.IsMaintenanceDone);
                    cmd.Parameters.AddWithValue("@MaintenanceCost", model.MaintenanceCost);
                    cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    id = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Asset maintenance log updated successfully!"
                    : "Asset maintenance log created successfully!";
                apiResponse.data = new { AssetMaintenanceLogId = id };

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
