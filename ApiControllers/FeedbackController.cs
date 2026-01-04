using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.Json;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : MasterController
    {
        private string connString = string.Empty;
        private readonly string domainUrl;

        public FeedbackController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpGet]
        [Route("GetFeedbackFormData")]
        public IActionResult GetFeedbackFormData()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                List<FeedbackStepModel> steps = new List<FeedbackStepModel>();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetFeedbackFormData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        string iconBasePath = $"{domainUrl}assets2/img/Feedback/";

                        while (dr.Read())
                        {
                            // Deserialize options JSON
                            var optionsJson = dr["Options"]?.ToString();
                            var options = string.IsNullOrWhiteSpace(optionsJson)
                                ? new List<FeedbackOptionModel>()
                                : JsonConvert.DeserializeObject<List<FeedbackOptionModel>>(optionsJson)
                                  ?? new List<FeedbackOptionModel>();

                            // Prefix icon path
                            foreach (var option in options)
                            {
                                if (!string.IsNullOrWhiteSpace(option.IconUrl))
                                {
                                    option.IconUrl = iconBasePath + option.IconUrl;
                                }
                            }

                            // Create step model
                            steps.Add(new FeedbackStepModel
                            {
                                StepId = dr["StepId"] != DBNull.Value ? Convert.ToInt64(dr["StepId"]) : 0,
                                StepName = dr["StepName"]?.ToString(),
                                StepTitle = dr["StepTitle"]?.ToString(),
                                StepDescription = dr["StepDescription"]?.ToString(),
                                StepOrder = dr["StepOrder"] != DBNull.Value ? Convert.ToInt32(dr["StepOrder"]) : 0,
                                IsMultiSelect = dr["IsMultiSelect"] != DBNull.Value && Convert.ToBoolean(dr["IsMultiSelect"]),
                                HasTextBox = dr["HasTextBox"] != DBNull.Value && Convert.ToBoolean(dr["HasTextBox"]),
                                Options = options
                            });
                        }
                    }
                }
                response.status = "Success";
                response.message = "Feedback form loaded.";
                response.data = steps;
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("SaveFeedback")]
        public IActionResult SaveFeedback([FromBody] SaveFeedbackRequest request)
        {
            apiResponse response = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                // Convert to JSON for SP
                string jsonData = JsonConvert.SerializeObject(request.Responses);

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveFeedback", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar).Value = request.CustomerName;
                    cmd.Parameters.Add("@WhatsappNo", SqlDbType.NVarChar).Value = request.WhatsappNo;
                    cmd.Parameters.Add("@JsonData", SqlDbType.NVarChar).Value = jsonData;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            response.status = dr["Status"].ToString();
                            response.message = dr["Message"].ToString();

                            if (response.status == "Success")
                            {
                                response.data = new
                                {
                                    FeedbackId = dr["FeedbackId"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("GetFeedback")]
        public IActionResult GetFeedback([FromBody] GetFeedbackRequest request)
        {
            apiResponse response = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                dynamic feedbackData = null;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetFeedback", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FeedbackId", SqlDbType.BigInt).Value = request.FeedbackId;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            feedbackData = new
                            {
                                FeedbackId = Convert.ToInt64(dr["FeedbackId"]),
                                Created_At = dr["Created_At"].ToString(),
                                Created_By = dr["Created_By"],
                                Responses = JsonConvert.DeserializeObject(dr["Responses"]?.ToString() ?? "[]")
                            };
                        }
                    }
                }

                if (feedbackData == null)
                {
                    response.status = "Error";
                    response.message = "Feedback not found.";
                }
                else
                {
                    response.status = "Success";
                    response.message = "Feedback loaded.";
                    response.data = feedbackData;
                }
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("GetDashboard_Feedback_Count")]
        public IActionResult GetDashboard_Feedback_Count()
        {
            apiResponse response = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                FeedbackDashboardResponse dashboard = new FeedbackDashboardResponse();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_Count", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            dashboard.TotalFeedback = Convert.ToInt64(dr["TotalFeedback"]);
                            dashboard.AvgRatingOutOf5 = dr["AvgRatingOutOf5"].ToString();

                            // Parse Summary JSON
                            dashboard.Summary = JsonConvert.DeserializeObject<FeedbackSummaryReport>(
                                dr["JSON_FeedbackReport"]?.ToString() ?? "{}"
                            );

                            // Parse Category JSON
                            dashboard.CategoryRatings = JsonConvert.DeserializeObject<List<CategoryRatingModel>>(
                                dr["JSON_CategoryWiseRating"]?.ToString() ?? "[]"
                            );
                        }
                    }
                }

                response.status = "Success";
                response.message = "Dashboard data loaded.";
                response.data = dashboard;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("GetDashboard_Feedback_RatingBreakdown")]
        public IActionResult GetDashboard_Feedback_RatingBreakdown()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DashboardRatingBreakdownResponse dashboardData = new DashboardRatingBreakdownResponse();
                dashboardData.RatingBreakdown = new List<RatingBreakdownModel>();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_RatingBreakdown", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string json = dr["JSON_RatingBreakdown"]?.ToString() ?? "[]";

                            dashboardData.RatingBreakdown =
                                JsonConvert.DeserializeObject<List<RatingBreakdownModel>>(json);
                        }
                    }
                }

                response.status = "Success";
                response.message = "Rating breakdown loaded successfully.";
                response.data = dashboardData;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("GetDashboard_Feedback_RatingsOverTime")]
        public IActionResult GetDashboard_Feedback_RatingsOverTime([FromBody] FeedbackRatingOverTimeRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext,
                    Request,
                    out string connString,
                    out string domain,
                    out IActionResult errorResult))
                    return errorResult;

                DashboardFeedbackRatingOverTimeResponse result = new DashboardFeedbackRatingOverTimeResponse();
                result.RatingsOverTime = new List<RatingOverTimeModel>();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Feedback_RatingsOverTime", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar).Value = request.FromDate ?? "";
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar).Value = request.ToDate ?? "";

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string json = dr["JSON_RatingsOverTime"]?.ToString() ?? "[]";

                            result.RatingsOverTime =
                                JsonConvert.DeserializeObject<List<RatingOverTimeModel>>(json);
                        }
                    }
                }

                response.status = "Success";
                response.message = "Ratings over time loaded successfully.";
                response.data = result;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return Ok(response);
            }
        }

    }
}
