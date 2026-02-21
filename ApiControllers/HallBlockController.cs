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
    public class HallBlockController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public HallBlockController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetHallBlock")]
        public IActionResult GetHallBlock([FromBody] GetHallBlockRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetHallBlockResponse> list = new List<GetHallBlockResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetHallBlock", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new GetHallBlockResponse
                    {
                        Id = dr["Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Id"]),
                        FromDate = dr["FromDate"] == DBNull.Value ? "" : Convert.ToString(dr["FromDate"]),
                        ToDate = dr["ToDate"] == DBNull.Value ? "" : Convert.ToString(dr["ToDate"]),
                        FromDate2 = dr["FromDate2"] == DBNull.Value ? "" : Convert.ToString(dr["FromDate2"]),
                        ToDate2 = dr["ToDate2"] == DBNull.Value ? "" : Convert.ToString(dr["ToDate2"]),
                        Reason = dr["Reason"] == DBNull.Value ? "" : Convert.ToString(dr["Reason"]),
                        Remarks = dr["Remarks"] == DBNull.Value ? "" : Convert.ToString(dr["Remarks"]),
                        HallListJson = dr["HallListJson"] == DBNull.Value
                            ? new List<HallBlockDetails>()
                            : JsonConvert.DeserializeObject<List<HallBlockDetails>>(
                                Convert.ToString(dr["HallListJson"]) ?? "[]"
                              ) ?? new List<HallBlockDetails>()
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Hall block data fetched successfully!";
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
        [Route("SaveHallBlock")]
        public IActionResult SaveHallBlock([FromBody] SaveHallBlockRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long hallBlockId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveHallBlock", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
                    cmd.Parameters.AddWithValue("@Reason", model.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);
                    cmd.Parameters.AddWithValue("@HallBlockDetailsJson",
                        string.IsNullOrEmpty(model.HallBlockDetailsJson)
                        ? (object)DBNull.Value
                        : model.HallBlockDetailsJson);

                    con.Open();
                    hallBlockId = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Hall block updated successfully!"
                    : "Hall block created successfully!";
                apiResponse.data = new { HallBlockId = hallBlockId };

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
        [Route("DeleteHallBlock")]
        public IActionResult DeleteHallBlock([FromBody] DeleteHallBlockRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteHallBlock", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@DeletedBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Hall block deleted successfully!";
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
