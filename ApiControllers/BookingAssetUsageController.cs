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
    public class BookingAssetUsageController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public BookingAssetUsageController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetBookingAssetUsage")]
        public IActionResult GetBookingAssetUsage([FromBody] GetBookingAssetUsageRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetBookingAssetUsageResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingAssetUsage", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);
                    cmd.Parameters.AddWithValue("@BookingId", request?.BookingId ?? 0);
                    cmd.Parameters.AddWithValue("@AssetId", request?.AssetId ?? 0);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                    con.Close();
                }

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new GetBookingAssetUsageResponse
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        BookingId = Convert.ToInt64(dr["BookingId"]),
                        AssetId = Convert.ToInt64(dr["AssetId"]),
                        AssetName = Convert.ToString(dr["AssetName"]),
                        AssetUniqueCode = Convert.ToString(dr["AssetUniqueCode"]),
                        UsedQty = Convert.ToDecimal(dr["UsedQty"]),
                        IsDamaged = Convert.ToBoolean(dr["IsDamaged"]),
                        DamageRemarks = dr["DamageRemarks"] == DBNull.Value ? null : Convert.ToString(dr["DamageRemarks"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Booking asset usage fetched successfully!";
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
        [Route("SaveBookingAssetUsage")]
        public IActionResult SaveBookingAssetUsage([FromBody] SaveBookingAssetUsageRequest model)
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
                using (SqlCommand cmd = new SqlCommand("SaveBookingAssetUsage", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                    cmd.Parameters.AddWithValue("@AssetId", model.AssetId);
                    cmd.Parameters.AddWithValue("@UsedQty", model.UsedQty);
                    cmd.Parameters.AddWithValue("@IsDamaged", model.IsDamaged);
                    cmd.Parameters.AddWithValue("@DamageRemarks", model.DamageRemarks ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    id = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Asset usage updated successfully!"
                    : "Asset usage saved successfully!";
                apiResponse.data = new { BookingAssetUsageId = id };

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
