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
        public CustomerInvoiceController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
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
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoice_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@NameEmailContact", SqlDbType.NVarChar, 100)
                                  .Value = (object?)request.NameEmailContact ?? DBNull.Value;
                    cmd.Parameters.Add("@SlotId", SqlDbType.BigInt).Value = request.SlotId;
                    cmd.Parameters.Add("@EventFrom", SqlDbType.NVarChar, 20)
                                  .Value = (object?)request.EventFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@EventTo", SqlDbType.NVarChar, 20)
                                  .Value = (object?)request.EventTo ?? DBNull.Value;
                    cmd.Parameters.Add("@BookedFrom", SqlDbType.NVarChar, 20)
                                  .Value = (object?)request.BookedFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@BookedTill", SqlDbType.NVarChar, 20)
                                  .Value = (object?)request.BookedTill ?? DBNull.Value;
                    cmd.Parameters.Add("@SubEventId", SqlDbType.BigInt).Value = request.SubEventId;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new CustomerInvoiceResponse_GRID
                            {
                                BookingId = Convert.ToInt64(dr["BookingId"]),
                                InvoiceNo = dr["InvoiceNo"].ToString(),
                                ProformaInvoiceNo = dr["ProformaInvoiceNo"].ToString(),
                                GSTInvoiceNo = dr["GSTInvoiceNo"].ToString(),
                                EventDate = dr["EventDate"].ToString(),
                                BookedOn = dr["BookedOn"].ToString(),
                                EventName = dr["EventName"].ToString(),
                                HallName = dr["HallName"].ToString(),
                                ClientName = dr["ClientName"].ToString(),
                                ClientContactNo = dr["ClientContactNo"].ToString(),
                                TotalAmount = Convert.ToDecimal(dr["TotalAmount"])
                            });
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice fetched successfully.";
                apiResponse.data = list;

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
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoiceByBookingId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;

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
                                IsGSTIncluded = dr["IsGSTIncluded"] == DBNull.Value
                                                ? null
                                                : Convert.ToBoolean(dr["IsGSTIncluded"])
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
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoiceById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar, 50).Value = request.InvoiceNo;
                    cmd.Parameters.Add("@ProformaInvoiceNo", SqlDbType.NVarChar, 50).Value = request.ProformaInvoiceNo;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            invoice = new CustomerInvoiceResponse_Id
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                InvoiceNo = dr["InvoiceNo"]?.ToString(),
                                ProformaInvoiceNo = dr["ProformaInvoiceNo"]?.ToString(),
                                GSTInvoiceNo = dr["GSTInvoiceNo"]?.ToString(),

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
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                long invoiceId;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveCustomerInvoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                    cmd.Parameters.AddWithValue("@InvoiceNo", request.InvoiceNo);
                    cmd.Parameters.AddWithValue("@ProformaInvoiceNo", (object)request.ProformaInvoiceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GSTInvoiceNo", (object)request.GSTInvoiceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerName", request.CustomerName);
                    cmd.Parameters.AddWithValue("@InvoiceDate", request.InvoiceDate);
                    cmd.Parameters.AddWithValue("@GSTIN", (object)request.GSTIN ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)request.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailId", (object)request.EmailId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerStateId", (object)request.CustomerState ?? DBNull.Value);

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
                        out IActionResult errorResult))
                {
                    return errorResult;
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
                        out IActionResult errorResult))
                {
                    return errorResult;
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
                            data.NextInvoiceNo = dr["NextInvoiceNo"].ToString();
                            data.NextProformaInvoiceNo = dr["NextProformaInvoiceNo"].ToString();
                            data.NextGSTInvoiceNo = dr["NextGSTInvoiceNo"].ToString();
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

    }
}
