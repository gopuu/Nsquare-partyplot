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
    public class CustomerInvoiceController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;

        public CustomerInvoiceController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) 
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetCustomerInvoice_GRID")]
        public IActionResult GetCustomerInvoice_GRID(CustomerInvoiceRequest_GRID request)
        {
            apiResponse apiResponse = new apiResponse();
            List<CustomerInvoiceResponse_GRID> list = new List<CustomerInvoiceResponse_GRID>();

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
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoice_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@NameEmailContact", SqlDbType.NVarChar, 100).Value = (object?)request.NameEmailContact ?? DBNull.Value;
                    cmd.Parameters.Add("@SlotId", SqlDbType.BigInt).Value = request.SlotId;
                    cmd.Parameters.Add("@EventFrom", SqlDbType.NVarChar, 20).Value = (object?)request.EventFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@EventTo", SqlDbType.NVarChar, 20).Value = (object?)request.EventTo ?? DBNull.Value;
                    cmd.Parameters.Add("@BookedFrom", SqlDbType.NVarChar, 20).Value = (object?)request.BookedFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@BookedTill", SqlDbType.NVarChar, 20).Value = (object?)request.BookedTill ?? DBNull.Value;
                    cmd.Parameters.Add("@SubEventId", SqlDbType.BigInt).Value = request.SubEventId;
                    cmd.Parameters.Add("@PaymentStatusId", SqlDbType.BigInt).Value = request.PaymentStatusId;
                    cmd.Parameters.Add("@IsInvoiceGenerated", SqlDbType.Bit).Value = (object?)request.IsInvoiceGenerated ?? DBNull.Value;
                    cmd.Parameters.Add("@IsExclusiveOfGST", SqlDbType.Bit).Value = (object?)request.IsExclusiveOfGST ?? DBNull.Value;
                    cmd.Parameters.Add("@IsInclusiveOfGST", SqlDbType.Bit).Value = (object?)request.IsInclusiveOfGST ?? DBNull.Value;
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        /* =========================
                           🔹 RESULT 1: ROWS
                        ========================= */
                        while (dr.Read())
                        {
                            list.Add(new CustomerInvoiceResponse_GRID
                            {
                                BookingId = Convert.ToInt64(dr["BookingId"]),
                                ProformaInvoiceNo = Convert.ToInt64(dr["ProformaInvoiceNo"]),
                                FinalInvoiceNo = Convert.ToString(dr["FinalInvoiceNo"]),
                                InvoiceNo = Convert.ToInt64(dr["InvoiceNo"]),
                                GSTInvoiceNo = Convert.ToString(dr["GSTInvoiceNo"]),
                                IsExclusiveOfGST = Convert.ToBoolean(dr["IsExclusiveOfGST"]),
                                IsInclusiveOfGST = Convert.ToBoolean(dr["IsInclusiveOfGST"]),
                                EventDate = dr["EventDate"].ToString(),
                                EventDate_Format = dr["EventDate_Format"].ToString(),
                                IsInvoiceBlocked = Convert.ToBoolean(dr["IsInvoiceBlocked"]),
                                BookedOn = dr["BookedOn"].ToString(),
                                EventName = dr["EventName"].ToString(),
                                HallName = dr["HallName"].ToString(),
                                ClientName = dr["ClientName"].ToString(),
                                ClientContactNo = dr["ClientContactNo"].ToString(),
                                TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
                                PaidAmount = Convert.ToDecimal(dr["PaidAmount"]), // NEW
                                OutstandingAmount = Convert.ToDecimal(dr["OutstandingAmount"])
                            });
                        }

                        /* =========================
                           🔹 RESULT 2: TOTALS
                        ========================= */
                        decimal totalAmount = 0;
                        decimal totalPaid = 0;
                        decimal totalOutstanding = 0;
                        int totalBookings = 0;

                        if (dr.NextResult())
                        {
                            if (dr.Read())
                            {
                                totalBookings = Convert.ToInt32(dr["TotalBookings"]);
                                totalAmount = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0;
                                totalPaid = dr["TotalPaid"] != DBNull.Value ? Convert.ToDecimal(dr["TotalPaid"]) : 0;
                                totalOutstanding = dr["TotalOutstanding"] != DBNull.Value ? Convert.ToDecimal(dr["TotalOutstanding"]) : 0;
                            }
                        }

                        /* =========================
                           🔹 FINAL RESPONSE
                        ========================= */
                        apiResponse.status = "Success";
                        apiResponse.message = "Customer invoice fetched successfully.";
                        apiResponse.data = new
                        {
                            Rows = list,
                            Totals = new
                            {
                                TotalBookings = totalBookings,
                                TotalAmount = totalAmount,
                                TotalPaid = totalPaid,
                                TotalOutstanding = totalOutstanding
                            }
                        };
                    }
                }

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
        [Route("GetCustomerInvoiceByBookingId")]
        public IActionResult GetCustomerInvoiceByBookingId(CustomerInvoiceRequest_BookingId request)
        {
            apiResponse apiResponse = new apiResponse();
            CustomerInvoiceResponse_BookingId invoice = null;

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
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoiceByBookingId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@IsInclusiveGST", SqlDbType.Bit).Value = request.IsInclusiveGST;
                    cmd.Parameters.Add("@IsProformaInvoice", SqlDbType.Bit).Value = request.IsProformaInvoice;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            invoice = new CustomerInvoiceResponse_BookingId
                            {
                                ClientName = dr["Client_Name"]?.ToString(),
                                InvoiceDate = dr["InvoiceDate"]?.ToString(),
                                GSTIN = dr["Client_GSTIN"]?.ToString(),
                                Email = dr["Client_Email"]?.ToString(),
                                ClientStateId = dr["Client_StateId"] == DBNull.Value
                                                ? null
                                                : Convert.ToInt64(dr["Client_StateId"]),
                                Address = dr["Client_Address"]?.ToString(),
                                IsGSTIncluded = dr["IsGSTIncluded"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsGSTIncluded"]),
                                IsExclusiveOfGST = dr["IsExclusiveOfGST"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsExclusiveOfGST"]),
                                IsInclusiveOfGST = dr["IsInclusiveOfGST"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsInclusiveOfGST"]),
                                PaidAmount = Convert.ToDecimal(dr["PaidAmount"]?.ToString())
                            };

                            string itemsJson = dr["Items"]?.ToString();

                            if (!string.IsNullOrWhiteSpace(itemsJson))
                            {
                                invoice.ItemsJson =
                                    JsonConvert.DeserializeObject<List<CustomerInvoiceItem_BookingId>>(itemsJson);
                            }
                            else
                            {
                                invoice.ItemsJson = new List<CustomerInvoiceItem_BookingId>();
                            }
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice fetched successfully.";
                apiResponse.data = invoice;

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
        [Route("GetCustomerInvoiceById")]
        public IActionResult GetCustomerInvoiceById(CustomerInvoiceRequest_Id request)
        {
            apiResponse apiResponse = new apiResponse();
            CustomerInvoiceResponse_Id invoice = null;

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
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoiceById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.BigInt).Value = request.InvoiceNo;
                    cmd.Parameters.Add("@GSTInvoiceNo", SqlDbType.NVarChar, 20).Value = request.GSTInvoiceNo;
                    cmd.Parameters.Add("@ProformaInvoiceNo", SqlDbType.BigInt).Value = request.ProformaInvoiceNo;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            invoice = new CustomerInvoiceResponse_Id
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                InvoiceNo = Convert.ToInt64(dr["InvoiceNo"]?.ToString()),
                                ProformaInvoiceNo = Convert.ToInt64(dr["ProformaInvoiceNo"]?.ToString()),
                                GSTInvoiceNo = Convert.ToString(dr["GSTInvoiceNo"]?.ToString()),

                                IsExclusiveOfGST = Convert.ToBoolean(dr["IsExclusiveOfGST"]?.ToString()),
                                IsInclusiveOfGST = Convert.ToBoolean(dr["IsInclusiveOfGST"]?.ToString()),

                                CustomerName = dr["CustomerName"]?.ToString(),
                                InvoiceDate = dr["InvoiceDate"]?.ToString(),              // yyyy-MM-dd
                                InvoiceDate_Format = dr["InvoiceDate_Format"]?.ToString(), // dd-MMM-yyyy
                                GSTIN = dr["GSTIN"]?.ToString(),
                                Address = dr["Address"]?.ToString(),
                                EmailId = dr["EmailId"]?.ToString(),
                                CustomerStateId = Convert.ToInt64(dr["CustomerStateId"]),

                                SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                                BookingDiscount = Convert.ToDecimal(dr["BookingDiscount"]),
                                TotalTaxableAmount = Convert.ToDecimal(dr["TotalTaxableAmount"]),
                                TotalDiscountAcrossAllServices = Convert.ToDecimal(dr["TotalDiscountAcrossAllServices"]),
                                TotalGST = Convert.ToDecimal(dr["TotalGST"]),
                                TotalVAT = Convert.ToDecimal(dr["TotalVAT"]),
                                GrandTotal = Convert.ToDecimal(dr["GrandTotal"]),
                                RoundOff = Convert.ToDecimal(dr["RoundOff"]),
                                TotalAdvancePaid = Convert.ToDecimal(dr["TotalAdvancePaid"]),
                                BalanceAmount = Convert.ToDecimal(dr["BalanceAmount"])
                            };

                            string itemsJson = dr["JSON_Items"]?.ToString();

                            invoice.Items = !string.IsNullOrWhiteSpace(itemsJson)
                                ? JsonConvert.DeserializeObject<List<CustomerInvoiceItem_Id>>(itemsJson)
                                : new List<CustomerInvoiceItem_Id>();
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice fetched successfully.";
                apiResponse.data = invoice;

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
        [Route("SaveCustomerInvoice")]
        public IActionResult SaveCustomerInvoice([FromBody] SaveCustomerInvoiceRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long invoiceId;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveCustomerInvoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);

                    cmd.Parameters.AddWithValue("@InvoiceNo", request.InvoiceNo);
                    cmd.Parameters.AddWithValue("@ProformaInvoiceNo", request.ProformaInvoiceNo);
                    cmd.Parameters.AddWithValue("@GSTInvoiceNo", request.GSTInvoiceNo);

                    cmd.Parameters.AddWithValue("@IsExclusiveOfGST", request.IsExclusiveOfGST);
                    cmd.Parameters.AddWithValue("@IsInclusiveOfGST", request.IsInclusiveOfGST);

                    cmd.Parameters.AddWithValue("@CustomerName", request.CustomerName);
                    cmd.Parameters.AddWithValue("@InvoiceDate", request.InvoiceDate);
                    cmd.Parameters.AddWithValue("@GSTIN", (object)request.GSTIN ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)request.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailId", (object)request.EmailId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerStateId", (object)request.CustomerState ?? DBNull.Value);

                    // ================= PAYMENT PARAMETERS =================
                    cmd.Parameters.AddWithValue("@Payment_ModeId", (object?)request.Payment_ModeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_AccountId", (object?)request.Payment_AccountId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_TransactionId", (object?)request.Payment_TransactionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_ChequeBank", (object?)request.Payment_ChequeBank ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_ChequeNo", (object?)request.Payment_ChequeNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_ChequeDate", (object?)request.Payment_ChequeDate ?? DBNull.Value);

                    // 🔹 Numeric (Better to define SqlDbType)
                    var amountParam = cmd.Parameters.Add("@Payment_Amount", SqlDbType.Decimal);
                    amountParam.Precision = 18;
                    amountParam.Scale = 2;
                    amountParam.Value = (object?)request.Payment_Amount ?? DBNull.Value;

                    cmd.Parameters.AddWithValue("@Payment_PaidDate", (object?)request.Payment_PaidDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_PaidTime", (object?)request.Payment_PaidTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_DueDate", (object?)request.Payment_DueDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_Note", (object?)request.Payment_Note ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@SubTotal", request.SubTotal);
                    cmd.Parameters.AddWithValue("@BookingDiscount", request.BookingDiscount);
                    cmd.Parameters.AddWithValue("@TotalTaxableAmount", request.TotalTaxableAmount);
                    cmd.Parameters.AddWithValue("@TotalDiscountAcrossAllServices", request.TotalDiscountAcrossAllServices);
                    cmd.Parameters.AddWithValue("@TotalGST", request.TotalGST);
                    cmd.Parameters.AddWithValue("@TotalVAT", request.TotalVAT);
                    cmd.Parameters.AddWithValue("@GrandTotal", request.GrandTotal);
                    cmd.Parameters.AddWithValue("@RoundOff", request.RoundOff);
                    cmd.Parameters.AddWithValue("@TotalAdvancePaid", request.TotalAdvancePaid);
                    cmd.Parameters.AddWithValue("@BalanceAmount", request.BalanceAmount);

                    cmd.Parameters.AddWithValue(
                        "@JSON_Items",
                        request.JSON_Items != null
                            ? JsonConvert.SerializeObject(request.JSON_Items)
                            : DBNull.Value
                    );
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    invoiceId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice saved successfully.";
                apiResponse.data = new { InvoiceId = invoiceId };

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

        [HttpPost]
        [Route("DeleteCustomerInvoice")]
        public IActionResult DeleteCustomerInvoice(DeleteCustomerInvoice request)
        {
            apiResponse apiResponse = new apiResponse();

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
                using (SqlCommand cmd = new SqlCommand("DeleteCustomerInvoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice deleted successfully.";
                apiResponse.data = null;

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

        [HttpGet]
        [Route("GetCustomerInvoice_NextInvoiceNumbers")]
        public IActionResult GetCustomerInvoice_NextInvoiceNumbers()
        {
            apiResponse apiResponse = new apiResponse();

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

                NextInvoiceNumberResponse data = new NextInvoiceNumberResponse();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoice_NextInvoiceNumbers", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            data.NextInvoiceNo = dr["NextInvoiceNo"] == DBNull.Value
                                ? 0L
                                : Convert.ToInt64(dr["NextInvoiceNo"]);

                            data.NextProformaInvoiceNo = dr["NextProformaInvoiceNo"] == DBNull.Value
                                ? 0L
                                : Convert.ToInt64(dr["NextProformaInvoiceNo"]);

                            data.NextGSTInvoiceNo = dr["NextGSTInvoiceNo"]?.ToString();

                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Next invoice numbers fetched successfully.";
                apiResponse.data = data;

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

        //Send Invoice
        [HttpPost]
        [Route("SendCustomerInvoice")]
        public IActionResult SendCustomerInvoice([FromBody] SendInvoiceRequest model)
        {
            apiResponse apiResponse = new apiResponse();
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
                using (SqlCommand cmd = new SqlCommand("SendCustomerInvoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                    cmd.Parameters.AddWithValue("@InvoiceNo", model.InvoiceNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProformaInvoiceNo", model.ProformaInvoiceNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailId", model.EmailId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNo", model.ContactNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WhatsappNo", model.WhatsappNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            apiResponse.status =
                                Convert.ToInt32(reader["Status"]) == 1 ? "Success" : "Error";

                            apiResponse.message =
                                reader["Message"]?.ToString() ?? "Unknown response";
                        }
                    }

                    con.Close();
                }

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
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
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_PaymentCollection", con);
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
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
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
        [Route("GetDashboardInvoiceStatus")]
        public IActionResult GetDashboardInvoiceStatus([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            DashboardInvoiceStatus result = null;
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
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_InvoiceStatus", con);
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

                    result = new DashboardInvoiceStatus
                    {
                        InvoiceGeneratedCount = Convert.ToInt64(row["InvoiceGeneratedCount"]),
                        InvoicePendingCount = Convert.ToInt64(row["InvoicePendingCount"]),
                        InvoiceTotalCount = Convert.ToInt64(row["InvoiceTotalCount"])
                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Invoice status fetched successfully.";
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
