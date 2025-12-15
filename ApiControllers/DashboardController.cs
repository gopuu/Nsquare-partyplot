using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : MasterController
    {
        private string connString = string.Empty;

        public DashboardController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetLeadSourceStatistics")]
        public IActionResult GetLeadSourceStatistics([FromBody] LeadSourceStatisticsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<LeadSourceStatisticsResponse> resultList = new List<LeadSourceStatisticsResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_LeadSourceStatistics", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@YearId", (object?)request.YearId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MonthId", (object?)request.MonthId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IncludeAmount", request.IncludeAmount);
                    cmd.Parameters.AddWithValue("@LeadReferenceId", (object?)request.LeadReferenceId ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    resultList.Add(new LeadSourceStatisticsResponse
                    {
                        LeadSourceId = Convert.ToInt64(row["LeadSourceId"]),
                        LeadSourceName = Convert.ToString(row["LeadSourceName"]),
                        BookingCount = Convert.ToInt32(row["BookingCount"]),
                        TotalAmount = row["TotalAmount"] == DBNull.Value ? null : Convert.ToDecimal(row["TotalAmount"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Lead source statistics fetched successfully.";
                apiResponse.data = resultList;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetOverallBookingStatistics")]
        public IActionResult GetOverallBookingStatistics([FromBody] OverallBookingStatsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<OverallBookingStatsResponse> resultList = new List<OverallBookingStatsResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_OverallBookingStatistics", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@YearId", request.YearId);
                    cmd.Parameters.AddWithValue("@MonthId", (object?)request.MonthId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BookingQty", request.BookingQty);
                    cmd.Parameters.AddWithValue("@BookingAmount", request.BookingAmount);
                    cmd.Parameters.AddWithValue("@EventTypeId", (object?)request.EventTypeId ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    resultList.Add(new OverallBookingStatsResponse
                    {
                        MonthNo = Convert.ToInt32(row["MonthNo"]),
                        MonthName = Convert.ToString(row["MonthName"]),
                        BookingCount = row["BookingCount"] == DBNull.Value ? null : Convert.ToInt32(row["BookingCount"]),
                        BookingAmount = row["BookingAmount"] == DBNull.Value ? null : Convert.ToDecimal(row["BookingAmount"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Overall booking statistics fetched successfully.";
                apiResponse.data = resultList;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetDashboardWidgetCounts")]
        public IActionResult GetDashboardWidgetCounts()
        {
            apiResponse apiResponse = new apiResponse();
            DashboardWidgetCountsResponse result = new DashboardWidgetCountsResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_WidgetCounts", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    result.DashboardStats = row["DashboardStats"].ToString();
                    result.Chart_CorporateVsFamily = row["Chart_CorporateVsFamily"].ToString();
                    result.Chart_CorporateSubTypes = row["Chart_CorporateSubTypes"].ToString();
                    result.Chart_FamilySubTypes = row["Chart_FamilySubTypes"].ToString();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard widgets fetched successfully.";
                apiResponse.data = result;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetDailyReportSummary")]
        public IActionResult GetDailyReportSummary(
        [FromBody] GetDailyReportSummaryRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                // 🔹 Validate connection
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                GetDailyReportSummaryResponse summary = null;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_GetDailyReportSummary", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ReportDate",request?.ReportDate);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary = new GetDailyReportSummaryResponse
                            {
                                ReportDate = Convert.ToDateTime(reader["ReportDate"])
                                                .ToString("yyyy-MM-dd"),

                                TotalEvents_Today = Convert.ToInt32(reader["TotalEvents_Today"]),
                                TotalEvents_TillDate = Convert.ToInt32(reader["TotalEvents_TillDate"]),

                                Revenue_Today = Convert.ToDecimal(reader["Revenue_Today"]),
                                Revenue_TillDate = Convert.ToDecimal(reader["Revenue_TillDate"]),

                                GuestsServed_Today = Convert.ToInt32(reader["GuestsServed_Today"]),
                                GuestsServed_TillDate = Convert.ToInt32(reader["GuestsServed_TillDate"]),

                                Enquiries_Today = Convert.ToInt32(reader["Enquiries_Today"]),
                                Enquiries_TillDate = Convert.ToInt32(reader["Enquiries_TillDate"]),

                                UpcomingEvents_Count = Convert.ToInt32(reader["UpcomingEvents_Count"]),
                                ExpectedMonthlyRevenue = Convert.ToDecimal(reader["ExpectedMonthlyRevenue"]),

                                RawMaterialExpense_Today = Convert.ToDecimal(reader["RawMaterialExpense_Today"]),
                                RawMaterialExpense_TillDate = Convert.ToDecimal(reader["RawMaterialExpense_TillDate"]),

                                TotalExpense_Today = Convert.ToDecimal(reader["TotalExpense_Today"]),
                                TotalExpense_TillDate = Convert.ToDecimal(reader["TotalExpense_TillDate"])
                            };
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Daily report summary fetched successfully!";
                apiResponse.data = summary;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }



        //UNUSED
        /*
        [HttpGet]
        [Route("GetProcessListWithDeals")]
        public IActionResult GetProcessListWithDeals()
        {
            apiResponse apiResponse = new apiResponse();
            List<ProcessDto> processes = new List<ProcessDto>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetProcessListWithDeals", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // ======================
                        // 1) Read Process List
                        // ======================
                        while (reader.Read())
                        {
                            ProcessDto process = new ProcessDto();
                            process.Id = reader.GetInt32(reader.GetOrdinal("ProcessId"));
                            process.Name = reader.GetString(reader.GetOrdinal("ProcessName"));
                            process.TotalDeals = reader.GetInt32(reader.GetOrdinal("TotalDeals"));
                            process.TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
                            process.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")).ToString("dd-MM-yyyy HH:mm:ss");
                            process.Deals = new List<DealDto>();
                            processes.Add(process);
                        }

                        // ======================
                        // 2) Read Deals
                        // ======================
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {

                                DealDto deal = new DealDto();
                                deal.Id = reader.GetInt32(reader.GetOrdinal("DealId"));
                                deal.Title = reader.GetString(reader.GetOrdinal("Title"));
                                deal.Customer = reader.GetString(reader.GetOrdinal("Customer"));
                                deal.Amount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                                deal.Date = reader.GetString(reader.GetOrdinal("DealDate"));

                                int processId = reader.GetInt32(reader.GetOrdinal("ProcessId"));
                                ProcessDto parent = processes.FirstOrDefault(p => p.Id == processId);
                                if (parent != null)
                                {
                                    parent.Deals.Add(deal);
                                }
                            }
                        }
                    }
                }

                apiResponse.message = "Processes fetched successfully!";
                apiResponse.data = processes;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }
        */
    }
}
