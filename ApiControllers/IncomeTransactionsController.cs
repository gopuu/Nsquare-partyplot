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
    public class IncomeTransactionsController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public IncomeTransactionsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetIncomeTransactionList")]
        public IActionResult GetIncomeTransactionList([FromBody] GetIncomeTransactionRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<IncomeTransactionModel> list = new List<IncomeTransactionModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetIncomeTransaction", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@AccountId", request.AccountId);
                    cmd.Parameters.AddWithValue("@PaymentModeId", request.PaymentModeId);
                    cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);
                    cmd.Parameters.AddWithValue("@SubCategoryId", request.SubCategoryId);
                    cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    var model = new IncomeTransactionModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        AccountId = Convert.ToInt64(row["AccountId"]),
                        AccountName = Convert.ToString(row["AccountName"]),

                        CategoryId = Convert.ToInt64(row["CategoryId"]),
                        CategoryName = Convert.ToString(row["CategoryName"]),

                        SubCategoryId = row["SubCategoryId"] == DBNull.Value ? 0 : Convert.ToInt64(row["SubCategoryId"]),
                        SubCategoryName = Convert.ToString(row["SubCategoryName"]),

                        Amount = Convert.ToDecimal(row["Amount"]),
                        TransactionDate = Convert.ToString(row["TransactionDate"]),
                        TransactionDate2 = Convert.ToString(row["TransactionDate2"]),
                        PaymentModeId = row["PaymentModeId"] == DBNull.Value ? 0 : Convert.ToInt64(row["PaymentModeId"]),
                        PaymentModeName = Convert.ToString(row["PaymentModeName"]),
                        TransactionId = Convert.ToString(row["TransactionId"]),
                        Notes = Convert.ToString(row["Notes"]),
                        VoucherNo = Convert.ToString(row["VoucherNo"]),
                        ChequeBank = Convert.ToString(row["ChequeBank"]),
                        ChequeNo = Convert.ToString(row["ChequeNo"]),
                        ChequeDate = row["ChequeDate"] == DBNull.Value ? null : Convert.ToDateTime(row["ChequeDate"]).ToString("yyyy-MM-dd"),
                    };

                    list.Add(model);
                }

                apiResponse.status = "Success";
                apiResponse.message = "Income transactions fetched successfully!";
                apiResponse.data = list;
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

        [HttpPost]
        [Route("SaveIncomeTransaction")]
        public IActionResult SaveIncomeTransaction([FromBody] SaveIncomeTransactionRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long rowId = 0;
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveIncomeTransaction", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@AccountId", model.AccountId);
                    cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                    cmd.Parameters.AddWithValue("@SubCategoryId", (object)model.SubCategoryId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@TransactionDate", model.TransactionDate);
                    cmd.Parameters.AddWithValue("@PaymentModeId", (object)model.PaymentModeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TransactionId", (object)model.TransactionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", (object)model.Notes ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@VoucherNo", (object)model.VoucherNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChequeBank", (object)model.ChequeBank ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChequeNo", (object)model.ChequeNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChequeDate", (object)model.ChequeDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0 ? "Income transaction updated successfully!" : "Income transaction inserted successfully!";
                apiResponse.data = new { IncomeId = rowId };
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

        [HttpPost]
        [Route("DeleteIncomeTransaction")]
        public IActionResult DeleteIncomeTransaction([FromBody] DeleteIncomeTransactionRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteIncomeTransaction", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Income transaction deleted successfully!";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        //NEXT VOUCHER NO
        [HttpGet]
        [Route("GetNextVoucherNo")]
        public IActionResult GetNextVoucherNo()
        {
            apiResponse apiResponse = new apiResponse();

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

                NextVoucherNoResponse result = null;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetNextVoucherNo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new NextVoucherNoResponse
                            {
                                LastVoucherNo_Income = reader["LastVoucherNo_Income"].ToString(),
                                LastVoucherNo_Expense = reader["LastVoucherNo_Expense"].ToString()
                            };
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Next voucher numbers fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse); // ✅ Always 200
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


    }
}
