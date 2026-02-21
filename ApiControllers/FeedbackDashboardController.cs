using ERPAdmin.Helper;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using ERPAdmin.Models;
using System.Text.Json;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackDashboardController : MasterController
    {
        private string connString = string.Empty;
        private readonly string domainUrl;

        public FeedbackDashboardController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }


        [HttpPost]
        [Route("GetDashboardFeedbackCounts")]
        public IActionResult GetDashboardFeedbackCounts([FromBody] DashboardFeedbackRequest request)
        {
            var apiResponse = new apiResponse();
            var result = new DashboardFeedbackCounts();

            try
            {
                // Get connection string and domain
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_Count", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Pass parameters
                        cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration);
                        cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                        cmd.Parameters.AddWithValue("@ToDate", request.ToDate);

                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // Total Feedback Count
                                result.TotalFeedback = dr["TotalFeedback"] != DBNull.Value ? Convert.ToInt32(dr["TotalFeedback"]) : 0;

                                // Avg Rating
                                result.AvgRatingOutOf5 = dr["AvgRatingOutOf5"]?.ToString() ?? "0%";

                                // JSON Feedback Report
                                var feedbackJson = dr["JSON_FeedbackReport"]?.ToString();
                                if (!string.IsNullOrEmpty(feedbackJson))
                                    result.JSON_FeedbackReport = JsonSerializer.Deserialize<FeedbackReport>(feedbackJson);

                                // JSON Category Wise Rating
                                var categoryJson = dr["JSON_CategoryWiseRating"]?.ToString();
                                if (!string.IsNullOrEmpty(categoryJson))
                                    result.JSON_CategoryWiseRating = JsonSerializer.Deserialize<List<CategoryRating>>(categoryJson);
                            }
                        }
                        con.Close();
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard feedback counts fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                // Return 200 OK even on error if you want consistency
                return Ok(apiResponse);
            }
        }

        [HttpPost]
        [Route("GetDashboardRatingBreakdown")]
        public IActionResult GetDashboardRatingBreakdown([FromBody] DashboardFeedbackRequest request)
        {
            var apiResponse = new apiResponse();
            var result = new RatingBreakdownResponse();

            try
            {
                // 🔹 Get connection & domain
                if (!TryGetConnectionAndDomain(ControllerContext,Request,out string connString,out string domain,out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_RatingBreakdown", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var json = dr["JSON_RatingBreakdown"]?.ToString();
                            if (!string.IsNullOrEmpty(json))
                            {
                                result.RatingBreakdown =
                                    JsonSerializer.Deserialize<List<RatingBreakdownItem>>(json);
                            }
                            else
                            {
                                result.RatingBreakdown = new List<RatingBreakdownItem>();
                            }

                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard rating breakdown fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);   // ✅ 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse);   // ✅ 200 even on error
            }
        }

        [HttpPost]
        [Route("GetFeedbackInsightSummaryCount")]
        public IActionResult GetFeedbackInsightSummaryCount()
        {
            apiResponse apiResponse = new apiResponse();
            FeedbackInsightResponse result = new FeedbackInsightResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid domain or connection.";
                    apiResponse.data = null;
                    return Ok(apiResponse);   // ✅ ALWAYS 200
                }

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetFeedbackInsightSummaryCount", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                /* ===== METRIC CARDS ===== */
                result.MetricCards = new List<MetricCard>();
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        result.MetricCards.Add(new MetricCard
                        {
                            MetricTitle = row["MetricTitle"].ToString(),
                            MetricValue = row["MetricValue"].ToString(),
                            MetricKey = row["MetricKey"].ToString()
                        });
                    }
                }

                /* ===== SENTIMENT ===== */
                SentimentSplit sentiment = new SentimentSplit();
                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        string title = row["SentimentTitle"].ToString();
                        int value = Convert.ToInt32(row["Percentage"]);

                        if (title == "Positive") sentiment.Positive = value;
                        if (title == "Negative") sentiment.Negative = value;
                        if (title == "Neutral") sentiment.Neutral = value;
                    }
                }

                result.Sentiment = sentiment;

                apiResponse.status = "Success";
                apiResponse.message = "Feedback insight summary fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);   // ✅ ALWAYS 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse);   // ✅ STILL 200
            }
        }



        [HttpPost]
        [Route("GetRatingDistributionSummary")]
        public IActionResult GetRatingDistributionSummary()
        {
            apiResponse apiResponse = new apiResponse();
            List<RatingDistribution> result = new List<RatingDistribution>();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid domain or connection.";
                    apiResponse.data = null;
                    return Ok(apiResponse);
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetRatingDistributionSummary", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new RatingDistribution
                    {
                        RatingLabel = row["RatingLabel"].ToString(),
                        RatingValue = Convert.ToInt32(row["RatingValue"]),
                        RatingCount = Convert.ToInt32(row["RatingCount"]),
                        RatingPercent = Convert.ToDecimal(row["RatingPercent"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Rating distribution fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);   // ✅ ALWAYS 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse);   // ✅ STILL 200
            }
        }


        [HttpPost]
        [Route("GetCategoryRatingSummary")]
        public IActionResult GetCategoryRatingSummary()
        {
            apiResponse apiResponse = new apiResponse();
            List<CategoryRatingSummary> result = new List<CategoryRatingSummary>();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid domain or connection.";
                    apiResponse.data = null;
                    return Ok(apiResponse); // ✅ ALWAYS 200
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCategoryRatingSummary", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new CategoryRatingSummary
                    {
                        CategoryName = row["CategoryName"].ToString(),
                        AvgRating = Convert.ToDecimal(row["AvgRating"]),
                        PositiveCount = Convert.ToInt32(row["PositiveCount"]),
                        NeutralCount = Convert.ToInt32(row["NeutralCount"]),
                        NegativeCount = Convert.ToInt32(row["NegativeCount"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Category rating summary fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse); // ✅ ALWAYS 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse); // ✅ STILL 200
            }
        }

        [HttpPost]
        [Route("GetDashboardRatingsOverTime")]
        public IActionResult GetDashboardRatingsOverTime([FromBody] DashboardFeedbackRequest request)
        {
            var apiResponse = new apiResponse();
            var result = new RatingsOverTimeResponse();

            try
            {
                // 🔹 Get connection & domain
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_RatingsOverTime", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var json = dr["JSON_RatingsOverTime"]?.ToString();

                            if (!string.IsNullOrEmpty(json))
                            {
                                result.RatingsOverTime =
                                    JsonSerializer.Deserialize<List<RatingsOverTimeItem>>(json);
                            }
                            else
                            {
                                result.RatingsOverTime = new List<RatingsOverTimeItem>();
                            }
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard ratings over time fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse);   // ✅ always 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse);   // ✅ consistent response
            }
        }


        [HttpPost]
        [Route("GetFeedbackDashboardHots_Visitors")]
        public IActionResult GetFeedbackDashboardHots_Visitors()
        {
            apiResponse apiResponse = new apiResponse();
            List<DashboardFeedbackCardResponse> list = new();

            try
            {
                // 🔹 Connection + Domain
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_GetFeedbackHots_Visitors", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new DashboardFeedbackCardResponse
                            {
                                Title = dr["Title"]?.ToString(),
                                TotalFeedbacks = dr["TotalFeedbacks"] != DBNull.Value
                                    ? Convert.ToInt32(dr["TotalFeedbacks"])
                                    : 0,
                                AverageRating = dr["AverageRating"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["AverageRating"])
                                    : 0,
                                NegativeFeedbackPercent = dr["NegativeFeedbackPercent"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["NegativeFeedbackPercent"])
                                    : 0,
                                FeedbackType = dr["FeedbackType"]?.ToString()
                            });
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard feedback summary fetched successfully.";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return Ok(apiResponse);
            }
        }


        [HttpPost]
        [Route("GetDashboardEventInsights")]
        public IActionResult GetDashboardEventInsights()
        {
            apiResponse apiResponse = new apiResponse();
            DashboardEventInsightResponse data = null;

            try
            {
                // 🔹 Connection + Domain
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_GetEvent_Insights", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            data = new DashboardEventInsightResponse
                            {
                                AvgbillingText = dr["AvgbillingText"]?.ToString(),
                                CorporateEventText = dr["CorporateEventText"]?.ToString(),
                                FamilyeventbillingText = dr["FamilyeventbillingText"]?.ToString(),
                                RevenueDistributionText = dr["RevenueDistributionText"]?.ToString(),
                                AvgbillingValue = dr["AvgbillingValue"] != DBNull.Value ? Convert.ToDecimal(dr["AvgbillingValue"]) : 0,
                                CorporateEventValue = dr["CorporateEventValue"] != DBNull.Value ? Convert.ToDecimal(dr["CorporateEventValue"]) : 0,
                                Familyeventbillingvalue = dr["Familyeventbillingvalue"] != DBNull.Value ? Convert.ToDecimal(dr["Familyeventbillingvalue"]) : 0,
                                RevenueDistributionValue = dr["RevenueDistributionValue"] != DBNull.Value ? Convert.ToDecimal(dr["RevenueDistributionValue"]) : 0,

                            };
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard event insights fetched successfully.";
                apiResponse.data = data;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return Ok(apiResponse);
            }
        }






    }
}
