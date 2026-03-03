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

        //1 Widgets - Counts & Amount
        [HttpPost]
        [Route("GetDashboardWidgetCounts")]
        public IActionResult GetDashboardWidgetCounts(DashboardWidgetCountsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            DashboardWidgetCounts result = new DashboardWidgetCounts();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_Widgetcount", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    result.TotalSales = Convert.ToDecimal(row["TotalSales"]);
                    result.TotalCustomer = Convert.ToInt64(row["TotalCustomer"]);
                    result.TotalEvents = Convert.ToInt64(row["TotalEvents"]);
                    result.EventLostCount = Convert.ToInt64(row["EventLostCount"]);
                    result.TotalEventsLost = Convert.ToDecimal(row["TotalEventsLost"]);
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

        //1 Widgets - Upcoming Events Chart
        [HttpPost]
        [Route("GetDashboardUpcomingEvents")]
        public IActionResult GetDashboardUpcomingEvents(DashboardUpcomingEventsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            DashboardUpcomingEventsResponse result = new DashboardUpcomingEventsResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand("Dashboard_UpcomingEvents", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            dt.Load(dr);
                        }
                        con.Close();
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    result.UpcomingEventsCount = row["UpcomingEventsCount"] != DBNull.Value
                        ? Convert.ToInt64(row["UpcomingEventsCount"])
                        : 0;

                    result.UpcomingEventsAmount = row["UpcomingEventsAmount"] != DBNull.Value
                        ? Convert.ToDecimal(row["UpcomingEventsAmount"])
                        : 0;
                }

                apiResponse.status = "Success";
                apiResponse.message = "Upcoming events data fetched successfully.";
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

        //2 Comparative Insights Chart 
        [HttpPost]
        [Route("Dashboard_GetComparativeInsightsChart")]
        public IActionResult Dashboard_GetComparativeInsightsChart([FromBody] MonthlyPnLChartRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<MonthlyPnLChartResponse> result = new List<MonthlyPnLChartResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd =
                    new SqlCommand("Dashboard_GetComparativeInsightsChart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Filter",
                        request.Filter);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new MonthlyPnLChartResponse
                            {
                                MonthKey = dr["MonthKey"].ToString(),

                                Revenue = Convert.ToDecimal(dr["Revenue"]),
                                Purchase = Convert.ToDecimal(dr["Purchase"]),
                                Salary = Convert.ToDecimal(dr["Salary"]),
                                Rent = Convert.ToDecimal(dr["Rent"]),
                                Electricity = Convert.ToDecimal(dr["Electricity"]),
                                Gas = Convert.ToDecimal(dr["Gas"]),
                                OtherExpense = Convert.ToDecimal(dr["OtherExpense"]),
                                ProfitAndLoss = Convert.ToDecimal(dr["ProfitAndLoss"]),

                                PurchasePercent = Convert.ToDecimal(dr["PurchasePercent"]),
                                SalaryPercent = Convert.ToDecimal(dr["SalaryPercent"]),
                                RentPercent = Convert.ToDecimal(dr["RentPercent"]),
                                ElectricityPercent = Convert.ToDecimal(dr["ElectricityPercent"]),
                                GasPercent = Convert.ToDecimal(dr["GasPercent"]),
                                OtherExpensePercent = Convert.ToDecimal(dr["OtherExpensePercent"]),
                                ProfitPercent = Convert.ToDecimal(dr["ProfitPercent"])
                            });
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Monthly Profit & Loss chart data fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        //3 Corporate Event vs Family Events
        [HttpPost]
        [Route("GetBookingEventTypeCount")]
        public IActionResult GetBookingEventTypeCount(DashboardWidgetCountsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EventTypePieChart> result = new List<EventTypePieChart>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_GetBookingEventTypeCount", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", (object)request.MonthDuration ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new EventTypePieChart
                        {
                            EventLabel = dr["eventLabel"].ToString(),
                            EventCount = Convert.ToInt32(dr["eventcount"])
                        });
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Event type pie chart data fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        //4 Corporate & Sub type of Event
        [HttpPost]
        [Route("GetBookingSubEventTypeCorporate")]
        public IActionResult GetBookingSubEventTypeCorporate([FromBody] DashboardWidgetCountsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EventTypePieChart> result = new List<EventTypePieChart>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_GetBookingSubEventTypeCorporate", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", (object)request.MonthDuration ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new EventTypePieChart
                        {
                            EventLabel = dr["EventSubTypeName"].ToString(),
                            EventCount = Convert.ToInt32(dr["SubTypeCount"])
                        });
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Corporate Event sub-type pie chart data fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // 🔹 Log error (your custom logger)
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        //5 Family & Subtype of Event
        [HttpPost]
        [Route("GetBookingSubEventTypeFamilyFunction")]
        public IActionResult GetBookingSubEventTypeFamilyFunction([FromBody] DashboardWidgetCountsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EventTypePieChart> result = new List<EventTypePieChart>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_GetBookingSubEventType_FamilyFunction", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", (object)request.MonthDuration ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new EventTypePieChart
                        {
                            EventLabel = dr["EventSubTypeName"].ToString(),
                            EventCount = Convert.ToInt32(dr["SubTypeCount"])
                        });
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Corporate Event sub-type pie chart data fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // 🔹 Log error (your custom logger)
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        //6 Booking Statistics Widgets
        [HttpPost]
        [Route("GetOverallBookingStatistics")]
        public IActionResult GetOverallBookingStatistics([FromBody] OverallBookingStatsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<OverallBookingStatsResponse> resultList = new List<OverallBookingStatsResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

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

        //7 OLD - Lead Source Widget
        [HttpPost]
        [Route("GetLeadSourceStatistics")]
        private IActionResult GetLeadSourceStatistics([FromBody] LeadSourceStatisticsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<LeadSourceStatisticsResponse> resultList = new List<LeadSourceStatisticsResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_LeadSourceStatistics_OLD", con);
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

        //7 Lead Source Widget
        [HttpPost]
        [Route("GetDashboardLeadSourceCount")]
        public IActionResult GetDashboardLeadSourceCount([FromBody] DashboardWidgetCountsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<LeadSourcePieChart> result = new List<LeadSourcePieChart>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_LeadSourcecount", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@MonthDuration",
                        (object)request.MonthDuration ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@FromDate",
                        (object)request.FromDate ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@ToDate",
                        (object)request.ToDate ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        result.Add(new LeadSourcePieChart
                        {
                            LeadSourceName = dr["LeadSourceName"].ToString(),
                            LeadSourceCount = Convert.ToInt32(
                                dr["LeadSourceCount"])
                        });
                    }

                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message =
                    "Lead source pie chart data fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(
                    ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        //7 Overall Bookings Statistics
        [HttpPost]
        [Route("Dashboard_GetOverallBookingsStatistics")]
        public IActionResult Dashboard_GetOverallBookingsStatistics([FromBody] DashboardEventSummaryRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<DashboardEventSummaryResponse> result = new List<DashboardEventSummaryResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_GetOverallBookingsStatistics", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@YearId",request.YearId);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new DashboardEventSummaryResponse
                            {
                                MonthName = dr["MonthName"].ToString(),
                                Year = Convert.ToInt32(dr["Year"]),
                                MonthNo = Convert.ToInt32(dr["MonthNo"]),

                                TotalEvents =
                                    Convert.ToInt32(dr["TotalEvents"]),
                                TotalFamilyFunctionCount =
                                    Convert.ToInt32(dr["TotalFamilyFunctionCount"]),
                                TotalCorporateCount =
                                    Convert.ToInt32(dr["TotalCorporateCount"]),

                                BookingQty =
                                    Convert.ToInt32(dr["BookingQty"]),
                                TotalAmount =
                                    Convert.ToDecimal(dr["TotalAmount"])
                            });
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message =
                    "Month wise event summary fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(
                    ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        //UnUsed - Daily Report Summary
        [HttpPost]
        [Route("GetDailyReportSummary")]
        public IActionResult GetDailyReportSummary([FromBody] GetDailyReportSummaryRequest request)
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
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                GetDailyReportSummaryResponse summary = null;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_GetDailyReportSummary", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ReportDate", request?.ReportDate);

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
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

    }
}
