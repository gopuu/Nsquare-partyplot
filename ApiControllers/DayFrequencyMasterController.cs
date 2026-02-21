using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;


namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DayFrequencyMasterController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public DayFrequencyMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetDayFrequencyList")]
        public IActionResult GetDayFrequencyList([FromBody] GetDayFrequencyRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetDayFrequencyList> frequencies = new List<GetDayFrequencyList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetDayFrequencyList", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", SqlDbType.BigInt).Value = request.Id == 0 ? 0 : request.Id;

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                con.Close();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GetDayFrequencyList freq = new GetDayFrequencyList();
                        freq.Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["Id"]);
                        freq.FrequencyTitle = row["FrequencyTitle"] == DBNull.Value ? string.Empty : row["FrequencyTitle"].ToString();
                        frequencies.Add(freq);
                    }
                }

                apiResponse.message = "Day Frequency list fetched successfully!";
                apiResponse.data = frequencies;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveDayFrequency")]
        public IActionResult SaveDayFrequency([FromBody] SaveDayFrequency model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveDayFrequency", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // parameters
                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                cmd.Parameters.Add("@FrequencyTitle", SqlDbType.NVarChar, 50).Value = model.FrequencyTitle ?? string.Empty;
                cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                con.Open();
                object result = cmd.ExecuteScalar();   // returns @Id from SP
                con.Close();

                long returnedId = 0;
                if (result != null && long.TryParse(result.ToString(), out long newId))
                {
                    returnedId = newId;
                }

                apiResponse.status = "Success";
                apiResponse.data = new { Id = returnedId };
                apiResponse.message = model.Id > 0
                    ? "Day Frequency updated successfully!"
                    : "Day Frequency inserted successfully!";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("DeleteDayFrequency")]
        public IActionResult DeleteDayFrequency([FromBody] DeleteDayFrequency request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                SqlConnection con = new SqlConnection(connString);
                con.Open();

                SqlCommand cmd = new SqlCommand("DeleteDayFrequency", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", SqlDbType.BigInt).Value = request.Id;
                cmd.Parameters.AddWithValue("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                cmd.ExecuteNonQuery();
                con.Close();

                apiResponse.status = "Success";
                apiResponse.message = "Day Frequency deleted successfully!";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

    }
}
