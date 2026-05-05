using DocumentFormat.OpenXml.EMMA;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using iTextSharp.text.pdf.codec;
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
        [Route("GetIncomeExpenseTransactionList")]
        public IActionResult GetIncomeExpenseTransactionList([FromBody] GetIncomeExpenseTransactionRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<IncomeExpenseTransactionModel> list = new List<IncomeExpenseTransactionModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetIncomeExpenseTransaction", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@AccountId", request.AccountId);
                    cmd.Parameters.AddWithValue("@PaymentModeId", request.PaymentModeId);

                    cmd.Parameters.AddWithValue("@IncomeCategoryId", request.IncomeCategoryId);
                    cmd.Parameters.AddWithValue("@IncomeSubCategoryId", request.IncomeSubCategoryId);

                    cmd.Parameters.AddWithValue("@ExpenseCategoryId", request.ExpenseCategoryId);
                    cmd.Parameters.AddWithValue("@ExpenseSubCategoryId", request.ExpenseSubCategoryId);

                    cmd.Parameters.AddWithValue("@FromDate", (object?)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object?)request.ToDate ?? DBNull.Value);
 
                    cmd.Parameters.AddWithValue("@FromAmount", (object?)request.FromAmount ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToAmount", (object?)request.ToAmount ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@TransactionType", request.TransactionType ?? "ALL");
                    cmd.Parameters.AddWithValue("@VoucherNo", (object?)request.VoucherNo ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    string transactionType = Convert.ToString(row["TransactionType"]);
                    string billFileName = Convert.ToString(row["BillFile"]);

                    string billFilePath = string.Empty;

                    if (!string.IsNullOrWhiteSpace(billFileName))
                    {
                        if (transactionType == "INCOME")
                        {
                            billFilePath = $"{domainUrl}/uploads/org/{domain}/IncomeTransactions/{billFileName}";
                        }
                        else if (transactionType == "EXPENSE")
                        {
                            billFilePath = $"{domainUrl}/uploads/org/{domain}/ExpenseTransactions/{billFileName}";
                        }
                    }

                    list.Add(new IncomeExpenseTransactionModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        TransactionType = transactionType,

                        AccountId = Convert.ToInt64(row["AccountId"]),
                        AccountName = Convert.ToString(row["AccountName"]),

                        CategoryId = Convert.ToInt64(row["CategoryId"]),
                        CategoryName = Convert.ToString(row["CategoryName"]),

                        SubCategoryId = row["SubCategoryId"] == DBNull.Value ? (long?)null : Convert.ToInt64(row["SubCategoryId"]),
                        SubCategoryName = Convert.ToString(row["SubCategoryName"]),

                        Amount = Convert.ToDecimal(row["Amount"]),

                        TransactionDate = Convert.ToString(row["TransactionDate"]),
                        TransactionDate_Format = Convert.ToString(row["TransactionDate_Format"]),

                        PaymentModeId = row["PaymentModeId"] == DBNull.Value ? 0 : Convert.ToInt64(row["PaymentModeId"]),
                        PaymentModeName = Convert.ToString(row["PaymentModeName"]),

                        TransactionId = Convert.ToString(row["TransactionId"]),
                        Notes = Convert.ToString(row["Notes"]),

                        VoucherNo = Convert.ToString(row["VoucherNo"]),
                        VoucherNo_Num = row["VoucherNo_Num"] == DBNull.Value ? (long?)null : Convert.ToInt64(row["VoucherNo_Num"]),

                        ChequeBank = Convert.ToString(row["ChequeBank"]),
                        ChequeNo = Convert.ToString(row["ChequeNo"]),

                        ChequeDate = row["ChequeDate"] == DBNull.Value ||
                                     string.IsNullOrWhiteSpace(row["ChequeDate"]?.ToString())
                                        ? string.Empty
                                        : Convert.ToDateTime(row["ChequeDate"]).ToString("yyyy-MM-dd"),

                        BillFile = billFilePath
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Transactions fetched successfully!";
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
                        InvoiceIncomes = Convert.ToDecimal(row["InvoiceIncomes"]),
                        CategorisedIncome = Convert.ToDecimal(row["CategorisedIncome"]),
                        TotalIncomes = Convert.ToDecimal(row["TotalIncomes"]),

                        PurchaseExpenses = Convert.ToDecimal(row["PurchaseExpenses"]),
                        CategorisedExpense = Convert.ToDecimal(row["CategorisedExpense"]),
                        TotalExpenses = Convert.ToDecimal(row["TotalExpenses"]),

                        GrossProfit = Convert.ToDecimal(row["GrossProfit"])
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
                                FullTitle = dr["FullTitle"].ToString(),
                                Amount = Convert.ToDecimal(dr["Amount"]),
                                EventTypeName = dr["EventTypeName"].ToString(),
                                EventSubTypeName = dr["EventSubTypeName"].ToString(),
                                ClientName = dr["ClientName"].ToString(),
                                InvoiceNo = dr["InvoiceNo"].ToString()
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
