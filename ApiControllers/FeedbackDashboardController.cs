using ERPAdmin.Helper;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using ERPAdmin.Models;

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
        [Route("GetFeedbackInsightSummaryCount")]
        public IActionResult GetFeedbackInsightSummaryCount()
        {
            apiResponse apiResponse = new apiResponse();
            FeedbackInsightResponse result = new FeedbackInsightResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
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
                    out string connString, out string domain, out IActionResult errorResult))
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
                    out string connString, out string domain, out IActionResult errorResult))
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


    }
}
