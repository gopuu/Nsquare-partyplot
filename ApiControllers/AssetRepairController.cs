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
    public class AssetRepairController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetRepairController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAssetRepair")]
        public IActionResult GetAssetRepair([FromBody] GetAssetRepairRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<AssetRepairModel> repairs = new List<AssetRepairModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetRepairList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    repairs.Add(new AssetRepairModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        BookingId = row["BookingId"] == DBNull.Value ? null : Convert.ToInt64(row["BookingId"]),
                        BookingClientName = Convert.ToString(row["BookingClientName"]),
                        RepairVendor = Convert.ToString(row["RepairVendor"]),
                        TotalItems = row["TotalItems"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalItems"]),
                        TotalQty = row["TotalQty"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalQty"]),
                        RepairTotalCost = row["RepairTotalCost"] == DBNull.Value ? 0 : Convert.ToDecimal(row["RepairTotalCost"]),
                        Remarks = Convert.ToString(row["Remarks"]),
                        Items = string.IsNullOrEmpty(Convert.ToString(row["Items"]))
                            ? new List<AssetRepairItemModel>()
                            : JsonConvert.DeserializeObject<List<AssetRepairItemModel>>(row["Items"].ToString())
                    });
                }

                apiResponse.message = "Asset repair list fetched successfully!";
                apiResponse.data = repairs;
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error: " + ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveAssetRepair")]
        public IActionResult SaveAssetRepair([FromBody] SaveAssetRepairRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAssetRepair", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@BookingId", (object)model.BookingId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepairVendor", (object)model.RepairVendor ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepairTotalCost", model.RepairTotalCost);
                    cmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@Items", JsonConvert.SerializeObject(model.Items));
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.message = model.Id > 0
                    ? "Asset repair updated successfully!"
                    : "Asset repair inserted successfully!";

                apiResponse.data = new { AssetRepairId = rowId };
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error: " + ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteAssetRepair")]
        public IActionResult DeleteAssetRepair([FromBody] DeleteAssetRepairRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteAssetRepair", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = "Asset repair deleted successfully!";
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error: " + ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }
    }
}
