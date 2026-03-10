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
    public class AssetRemoveController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetRemoveController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAssetRemove")]
        public IActionResult GetAssetRemove([FromBody] GetAssetRemoveRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<AssetRemoveModel> removes = new List<AssetRemoveModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetRemoveList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    removes.Add(new AssetRemoveModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        BookingId = Convert.ToInt64(row["BookingId"]),
                        BookingClientName = Convert.ToString(row["BookingClientName"]),
                        RemovedDate = Convert.ToString(row["RemovedDate"]),
                        RemovedDate1 = Convert.ToString(row["RemovedDate1"]),
                        Remarks = Convert.ToString(row["Remarks"]),

                        TotalItems = Convert.ToInt64(row["TotalItems"]),
                        TotalQty = Convert.ToDecimal(row["TotalQty"]),

                        Items = string.IsNullOrEmpty(Convert.ToString(row["Items"]))
                            ? new List<AssetRemoveItemModel>()
                            : JsonConvert.DeserializeObject<List<AssetRemoveItemModel>>(row["Items"].ToString())
                    });
                }

                apiResponse.message = "Asset remove list fetched successfully!";
                apiResponse.data = removes;
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveAssetRemove")]
        public IActionResult SaveAssetRemove([FromBody] SaveAssetRemoveRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAssetRemove", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                    cmd.Parameters.AddWithValue("@RemovedDate", model.RemovedDate);
                    cmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@Items", JsonConvert.SerializeObject(model.Items));
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.message = model.Id > 0 ? "Asset removal updated successfully!" : "Asset removal inserted successfully!";
                apiResponse.data = new { AssetRemoveId = rowId };
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteAssetRemove")]
        public IActionResult DeleteAssetRemove([FromBody] DeleteAssetRemoveRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteAssetRemove", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = "Asset removal deleted successfully!";
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }
    }
}
