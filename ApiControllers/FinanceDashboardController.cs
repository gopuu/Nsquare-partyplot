using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceDashboardController : MasterController
    {
        private string connString = string.Empty;
        private readonly string domainUrl;

        public FinanceDashboardController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService)
                : base(configuration, httpContextAccessor) 
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetDashboardFinanceSummary")]
        public IActionResult GetDashboardFinanceSummary([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            DashboardFinanceSummary result = null;

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid domain or connection.";
                    apiResponse.data = null;

                    return Ok(apiResponse); // ✅ ALWAYS 200
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_SummaryCount", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 🔹 Parameters (future ready)
                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    result = new DashboardFinanceSummary
                    {
                        TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                        OutstandingReceivables = Convert.ToDecimal(row["OutstandingReceivables"]),
                        TotalExpenses = Convert.ToDecimal(row["TotalExpenses"]),
                        NetProfit = Convert.ToDecimal(row["NetProfit"])
                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard finance summary fetched successfully.";
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
        [Route("GetDashboardPaymentCollectionStatus")]
        public IActionResult GetDashboardPaymentCollectionStatus([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            DashboardPaymentCollectionStatus result = null;

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
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_PaymentCollection_Status", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    result = new DashboardPaymentCollectionStatus
                    {
                        PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                        PendingAmount = Convert.ToDecimal(row["PendingAmount"]),
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"])
                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Payment collection status fetched successfully.";
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
        [Route("GetDashboardPaymentCollectionDetails")]
        public IActionResult GetDashboardPaymentCollectionDetails([FromBody] DashboardPaymentDetailsRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            List<DashboardPaymentCollectionDetails> details = new List<DashboardPaymentCollectionDetails>();
            DashboardPaymentCollectionTotals totals = new DashboardPaymentCollectionTotals();

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

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_PaymentCollection_Details", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 20).Value = (object?)request.Type ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthDuration", SqlDbType.NVarChar, 50).Value = (object?)request.MonthDuration ?? DBNull.Value;
                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar, 20).Value = (object?)request.FromDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar, 20).Value = (object?)request.ToDate ?? DBNull.Value;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                /* =========================
                   TABLE 1 → DETAILS
                ========================= */
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        details.Add(new DashboardPaymentCollectionDetails
                        {
                            BookingId = Convert.ToInt64(row["BookingId"]),
                            BookingRefNo = Convert.ToString(row["BookingRefNo"]),
                            CustomerName = Convert.ToString(row["Client_Name"]),
                            MobileNo = Convert.ToString(row["Client_Contact"]),
                            EventDate = Convert.ToString(row["EventDate"]),
                            EventDate_Format = Convert.ToString(row["EventDate_Format"]),

                            TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                            PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                            PendingAmount = Convert.ToDecimal(row["PendingAmount"]),

                            PaymentStatus = Convert.ToString(row["PaymentStatus"])
                        });
                    }
                }

                /* =========================
                   TABLE 2 → TOTALS
                ========================= */
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[1].Rows[0];

                    totals = new DashboardPaymentCollectionTotals
                    {
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                        PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                        PendingAmount = Convert.ToDecimal(row["PendingAmount"]),
                        TotalBookings = Convert.ToInt32(row["TotalBookings"])
                    };
                }

                /* =========================
                   FINAL RESPONSE
                ========================= */
                apiResponse.status = "Success";
                apiResponse.message = "Payment collection details fetched successfully.";
                apiResponse.data = new
                {
                    Rows = details,
                    Totals = totals
                };

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
        [Route("GetDashboardUpcomingPayments")]
        public IActionResult GetDashboardUpcomingPayments(DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<UpcomingPaymentItem> result = new List<UpcomingPaymentItem>();

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

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Finance_UpcomingPayments", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new UpcomingPaymentItem
                            {
                                EventName = dr["EventName"].ToString(),
                                Amount = Convert.ToDecimal(dr["DueAmount"])
                            });
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Upcoming payments fetched successfully.";
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
        [Route("GetDashboardVendorPayables")]
        public IActionResult GetDashboardVendorPayables([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<VendorPayableItem> result = new List<VendorPayableItem>();

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

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Finance_VendorPayables", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new VendorPayableItem
                            {
                                VendorName = dr["VendorName"].ToString(),
                                PendingAmount = Convert.ToDecimal(dr["PendingAmount"]),
                                Status = dr["Status"].ToString()
                            });
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Vendor payables fetched successfully.";
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
        [Route("GetDashboardTopRevenueEvents")]
        public IActionResult GetDashboardTopRevenueEvents(DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<TopRevenueEventItem> result = new List<TopRevenueEventItem>();

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

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Dashboard_Finance_TopRevenueEvents", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new TopRevenueEventItem
                            {
                                EventName = dr["EventName"].ToString(),
                                Amount = Convert.ToDecimal(dr["Amount"])
                            });
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Top revenue events fetched successfully.";
                apiResponse.data = result;
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
