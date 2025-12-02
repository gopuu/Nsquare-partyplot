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
    public class MonthMasterController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public MonthMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetMonthList")]
        public IActionResult GetMonthList([FromBody] MonthList_Request request)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            List<MonthList> months = new List<MonthList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString); //(ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty);
                SqlCommand cmd = new SqlCommand("GetMonthList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MonthId", SqlDbType.BigInt).Value = request.MonthId == 0 ? 0 : request.MonthId;
                cmd.Parameters.AddWithValue("@YearId", SqlDbType.BigInt).Value = request.YearId == 0 ? 0 : request.YearId;
                con.Open();

                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);
                con.Close();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        MonthList month = new MonthList();
                        month.MonthId = Convert.ToInt64(dr["Month_Id"]);
                        month.MonthName = Convert.ToString(dr["MonthName"]);
                        month.YearId = Convert.ToInt64(dr["YearMaster_Id"]);
                        month.YearName = Convert.ToString(dr["YearName"]);
                        months.Add(month);
                    }
                }
                apiResponse.message = "Months fetched successfully!";
                apiResponse.data = months;
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);                
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

    }
}
