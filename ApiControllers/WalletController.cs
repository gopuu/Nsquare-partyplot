using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Razorpay.Api;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Helpers;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : MasterController
    {
        private readonly HttpClient _httpClient;
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IHubContext<WalletHub> _walletHub;
        private readonly IConfiguration _configuration;
        public WalletController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHubContext<WalletHub> walletHub, HttpClient httpClient)
            : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _walletHub = walletHub;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("CreateWallet")]
        public IActionResult CreateWallet([FromBody] WalletRequest_Create request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Wallet_Create", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CompanyId", request.CompanyId);
                cmd.Parameters.AddWithValue("@CreatedBy", Convert.ToInt64(request.ActionBy));

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);

                con.Close();

                if (dt.Rows.Count == 0)
                {
                    return Ok(new apiResponse
                    {
                        status = "Error",
                        message = "No response from server"
                    });
                }

                apiResponse.message = dt.Rows[0]["Message"].ToString();
                apiResponse.status = dt.Rows[0]["Status"].ToString() == "1" ? "Success" : "Error";
                apiResponse.data = null;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                return StatusCode(500, new apiResponse { message = ex.Message, status = "Error" });
            }
        }

        [HttpPost]
        [Route("GetWalletBalance")]
        public IActionResult GetWalletBalance([FromBody] WalletRequest_Balance request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                decimal balance = 0;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Wallet_GetBalance", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CompanyId", SqlDbType.BigInt).Value = request.CompanyId;

                con.Open();

                var result = cmd.ExecuteScalar(); // 🔥 clean

                if (result != null && result != DBNull.Value)
                    balance = Convert.ToDecimal(result);

                con.Close();

                apiResponse.data = new { Balance = balance };
                apiResponse.message = "Balance fetched successfully";
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                return StatusCode(500, new apiResponse { message = ex.Message, status = "Error" });
            }
        }

        #region RECHARGE / CREDIT
        [HttpPost]
        [Route("WalletCredit_CreateOrder")]
        public IActionResult WalletCredit_CreateOrder([FromBody] CreateOrderRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                var razorpaySection = _configuration.GetRequiredSection("Razorpay");

                var key = razorpaySection["KeyId"];
                var secret = razorpaySection["KeySecret"];
                RazorpayClient client = new RazorpayClient(key, secret);

                int amountPaise = (int)(request.Amount * 100);

                string guid = Guid.NewGuid().ToString("N").Substring(0, 10); // 32char GUID -> 10 char
                string datetime = DateTime.UtcNow.Ticks.ToString(); //18chars

                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", amountPaise);
                options.Add("currency", "INR");
                options.Add("receipt", $"rcpt_{datetime}_{guid}"); // 5 + 18 + 10 = 33 chars, unique enough for our use-case

                Order order = client.Order.Create(options);

                apiResponse.status = "Success";
                apiResponse.data = new
                {
                    OrderId = order["id"].ToString(),
                    Amount = amountPaise,
                    Key = key
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new apiResponse { status = "Error", message = ex.Message });
            }
        }

        [HttpPost]
        [Route("WalletCredit_VerifyPayment")]
        public async Task<IActionResult> WalletCredit_VerifyPayment([FromBody] VerifyPaymentRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }
                var razorpaySection = _configuration.GetRequiredSection("Razorpay");
                var secret = razorpaySection["KeySecret"];

                // 🔐 Step 1: Verify Signature
                string payload = request.RazorpayOrderId + "|" + request.RazorpayPaymentId;

                string generatedSignature;

                using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret)))
                {
                    byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                    generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }

                if (generatedSignature != request.RazorpaySignature)
                {
                    return Ok(new apiResponse
                    {
                        status = "Error",
                        message = "Invalid payment signature"
                    });
                }

                // 🔥 Step 2: Credit Wallet (CALL SP)
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CompanyId", SqlDbType.BigInt) { Value = request.CompanyId },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = request.Amount },
                    new SqlParameter("@ReferenceType", SqlDbType.VarChar) { Value = "RAZORPAY" },
                    new SqlParameter("@ReferenceId", SqlDbType.NVarChar) { Value = request.RazorpayPaymentId },
                    new SqlParameter("@ActionBy", SqlDbType.BigInt) { Value = Convert.ToInt64(request.ActionBy) }
                };

                var dt = await ExecuteSP(connString, "Wallet_Credit", parameters);

                var balance = Convert.ToDecimal(dt.Rows[0]["Balance"]);

                // 🔥 SignalR
                await _walletHub.Clients.Group(request.CompanyId.ToString())
                    .SendAsync("OnWalletChanged", new
                    {
                        companyid = request.CompanyId,
                        balance = balance,
                        amount = request.Amount,
                        type = "CREDIT"
                    });

                return Ok(new apiResponse
                {
                    status = "Success",
                    message = "Payment verified & wallet credited",
                    data = new { Balance = balance }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new apiResponse { status = "Error", message = ex.Message });
            }
        }
        #endregion

        #region RECHARGE / DEBIT
        [HttpPost]
        [Route("WalletDebit")]
        public async Task<IActionResult> WalletDebit([FromBody] WalletDebitRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CompanyId", SqlDbType.BigInt) { Value = request.CompanyId },
                    new SqlParameter("@Amount", SqlDbType.Decimal)
                    {
                        Precision = 18,
                        Scale = 2,
                        Value = request.Amount
                    },

                    new SqlParameter("@ReferenceType", SqlDbType.VarChar) { Value = request.ReferenceType },
                    new SqlParameter("@ReferenceId", SqlDbType.NVarChar) { Value = (object?)request.ReferenceId ?? DBNull.Value },
                    new SqlParameter("@ActionBy", SqlDbType.BigInt) { Value = Convert.ToInt64(request.ActionBy) }
                };

                DataTable dt = await ExecuteSP(connString, "Wallet_Debit", parameters);

                if (dt.Rows.Count == 0)
                {
                    return Ok(new apiResponse
                    {
                        status = "Error",
                        message = "No response from server"
                    });
                }

                var status = dt.Rows[0]["Status"].ToString();
                var message = dt.Rows[0]["Message"].ToString();
                var balance = dt.Rows[0]["Balance"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["Balance"]) : 0;

                apiResponse.message = message;
                apiResponse.status = status == "1" ? "Success" : "Error";
                apiResponse.data = new { Balance = balance };

                // 🔥 SignalR: Notify client
                if (status == "1")
                {
                    await _walletHub.Clients.Group(request.CompanyId.ToString())
                        .SendAsync("OnWalletChanged", new
                        {
                            companyid = request.CompanyId,
                            balance = balance,
                            amount = request.Amount,
                            type = "DEBIT" // or DEBIT
                        });
                }

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                return StatusCode(500, new apiResponse { message = ex.Message, status = "Error" });
            }
        }

        [HttpPost]
        [Route("SendWhatsApp")]
        public async Task<IActionResult> SendWhatsApp([FromBody] WhatsAppSendRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            string connString = "";

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                // 🔥 STEP 1: WALLET DEBIT
                var debitParams = new List<SqlParameter>
                {
                    new SqlParameter("@CompanyId", request.CompanyId),
                    new SqlParameter("@MessageType", request.MessageType),
                    new SqlParameter("@MessageCount", request.MessageCount),
                    new SqlParameter("@ReferenceId", request.ReferenceId),
                    new SqlParameter("@TemplateId", (object?)request.TemplateId ?? DBNull.Value),
                    new SqlParameter("@IsPromotional", request.IsPromotional),
                    new SqlParameter("@ActionBy", request.ActionBy)
                };

                DataTable debitDt = await ExecuteSP(connString, "Wallet_Debit_WhatsApp", debitParams);
                if (debitDt.Rows.Count == 0 || debitDt.Rows[0]["Status"].ToString() != "1")
                {
                    return Ok(new apiResponse
                    {
                        status = "Error",
                        message = debitDt.Rows[0]["Message"].ToString()
                    });
                }

                decimal deductedAmount = Convert.ToDecimal(debitDt.Rows[0]["DeductedAmount"]);
                decimal balance = Convert.ToDecimal(debitDt.Rows[0]["Balance"]);

                // 🔥 STEP 2: SEND WHATSAPP
                string sendStatus = await SendWhatsAppMessage(request);
                if (sendStatus != "Success")
                {
                    // 🔥 STEP 3: REFUND IF FAILED
                    var refundParams = new List<SqlParameter>
                    {
                        new SqlParameter("@CompanyId", request.CompanyId),
                        new SqlParameter("@ReferenceId", request.ReferenceId),
                        new SqlParameter("@RefundReferenceId", Guid.NewGuid().ToString()),
                        new SqlParameter("@ActionBy", request.ActionBy)
                    };

                    await ExecuteSP(connString, "Wallet_Refund_WhatsApp", refundParams);

                    return Ok(new apiResponse
                    {
                        status = "Error",
                        message = "WhatsApp send failed. Amount refunded."
                    });
                }

                // 🔥 STEP 4: SIGNALR UPDATE
                await _walletHub.Clients.Group(request.CompanyId.ToString())
                    .SendAsync("OnWalletChanged", new
                    {
                        companyid = request.CompanyId,
                        balance = balance,
                        amount = deductedAmount,
                        type = "DEBIT"
                    });
                
                return Ok(new apiResponse
                {
                    status = sendStatus, //"Success",
                    message = (sendStatus == "Success") ? "WhatsApp msg sent successfully" : "WhatsApp msg sending failed",
                    data = new
                    {
                        Balance = balance,
                        DeductedAmount = deductedAmount
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                return StatusCode(500, new apiResponse
                {
                    status = "Error",
                    message = ex.Message
                });
            }
        }

        private async Task<string> SendWhatsAppMessage(WhatsAppSendRequest request)
        {
            try
            {
                if (request.MobileNo.Length != 12)
                    return "Fail";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string apiUrl = "https://crm.officialwa.com/api/meta/v19.0/1094783270388218/messages";
                
                string[] msgParts = request.Message.Split('&');

                var parametersList = msgParts.Select(x => new
                {
                    type = "text",
                    text = x
                }).ToArray();

                var payload = new
                {
                    to = request.MobileNo,
                    recipient_type = "individual",
                    type = "template",
                    template = new
                    {
                        language = new
                        {
                            policy = "deterministic",
                            code = "en"
                        },
                        name = request.TemplateName,
                        components = new object[]
                        {
                    new
                    {
                        type = "body",
                        parameters = parametersList
                    }
                        }
                    }
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", "ypGdt57xJYtdOhTjkVyltE5AAPafjdFCi1WT9MPEoQj0XtidBDMttprF0REFTSAnYF6jIPxj9Cpn30bJDlIsRT3vREFTSARpb421ALJMH0DGKR7jNRVdGXD8vzaOVU5ERVJTQ09SRQ3zEnI6ZY");

                    //var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var content = new StringContent(jsonPayload);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        return "Fail";

                    return "Success";
                }
            }
            catch
            {
                return "Fail";
            }
        }
        #endregion

        [HttpPost]
        [Route("GetWalletLedger")]
        public IActionResult GetWalletLedger([FromBody] WalletRequest_Ledger request)
        {
            apiResponse apiResponse = new apiResponse();
            List<WalletResponse_Ledger> list = new List<WalletResponse_Ledger>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Wallet_GetLedger", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompanyId", request.CompanyId);

                con.Open();

                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new WalletResponse_Ledger
                    {
                        Id = reader["Id"] != DBNull.Value ? Convert.ToInt64(reader["Id"]) : 0,
                        TransactionType = reader["TransactionType"]?.ToString(),
                        Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                        BalanceAfter = reader["BalanceAfter"] != DBNull.Value ? Convert.ToDecimal(reader["BalanceAfter"]) : 0,
                        ReferenceType = reader["ReferenceType"]?.ToString(),
                        ReferenceId = reader["ReferenceId"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        CreatedAt = reader["CreatedAt"]?.ToString()
                    });
                }

                con.Close();

                apiResponse.data = list;
                apiResponse.status = "Success";
                apiResponse.message = "Ledger fetched successfully";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                return StatusCode(500, new apiResponse { message = ex.Message, status = "Error" });
            }
        }

        private async Task<DataTable> ExecuteSP(string connString, string spName, List<SqlParameter> parameters)
        {
            DataTable dt = new DataTable();

            using SqlConnection con = new SqlConnection(connString);
            using SqlCommand cmd = new SqlCommand(spName, con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters.ToArray());

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            dt.Load(reader);

            return dt;
        }

    }
}

