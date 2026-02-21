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
    public class QuotationController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public QuotationController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetQuotation")]
        public IActionResult GetQuotation([FromBody] GetQuotationRequest request)
        {
            apiResponse apiResponse = new();
            List<GetQuotationResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetQuotation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request?.Id ?? (object)DBNull.Value);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new GetQuotationResponse
                            {
                                Id = Convert.ToInt64(reader["Id"]),
                                BookingId = reader["BookingId"] == DBNull.Value ? null : Convert.ToInt64(reader["BookingId"]),
                                BanquetId = Convert.ToInt64(reader["BanquetId"]),
                                BanquetName = reader["BanquetName"]?.ToString(),
                                SlotId = Convert.ToInt64(reader["SlotId"]),
                                SlotName = reader["SlotName"]?.ToString(),

                                FromDate = reader["SlotName"]?.ToString(),
                                TillDate = reader["TillDate"]?.ToString(),
                                StartTime = reader["StartTime"]?.ToString(),
                                EndTime = reader["EndTime"]?.ToString(),

                                ClientId = reader["ClientId"] == DBNull.Value ? null : Convert.ToInt64(reader["ClientId"]),
                                ClientName = Convert.ToString(reader["ClientName"]) ?? "",
                                Address = reader["Address"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                ContactNo = reader["ContactNo"]?.ToString(),
                                GSTIN = reader["GSTIN"]?.ToString(),
                                StateId = reader["StateId"] == DBNull.Value ? null : Convert.ToInt64(reader["StateId"]),
                                StateName = reader["StateName"]?.ToString(),

                                EventSubTypeId = reader["EventSubTypeId"]?.ToString(),
                                EventSubTypeName = reader["EventSubTypeName"]?.ToString(),

                                BanquetRent = Convert.ToDecimal(reader["BanquetRent"]),
                                Pax = Convert.ToInt32(reader["Pax"]),
                                ReferenceTypeId = reader["ReferenceTypeId"]?.ToString(),
                                ReferenceTypeName = reader["ReferenceTypeName"]?.ToString(),

                                DecorationChargesAmount = Convert.ToDecimal(reader["DecorationChargesAmount"]),
                                SoundSystemChargesAmount = Convert.ToDecimal(reader["SoundSystemChargesAmount"]),
                                ProjectorChargesAmount = Convert.ToDecimal(reader["ProjectorChargesAmount"]),

                                IsPrintPriceExclusive = Convert.ToBoolean(reader["IsPrintPriceExclusive"]),
                                IsPrintFoodItems = Convert.ToBoolean(reader["IsPrintFoodItems"]),

                                SubTotal = Convert.ToDecimal(reader["SubTotal"]),
                                TaxAmount = Convert.ToDecimal(reader["TaxAmount"]),
                                GrandTotal = Convert.ToDecimal(reader["GrandTotal"]),
                                Notes = reader["Notes"]?.ToString()
                            };

                            // Deserialize JSON children
                            var packagesJson = reader["Packages_JSON"]?.ToString();
                            if (!string.IsNullOrEmpty(packagesJson))
                            {
                                item.Packages = System.Text.Json.JsonSerializer
                                    .Deserialize<List<QuotationPackageModel>>(packagesJson) ?? new();
                            }

                            var servicesJson = reader["OtherServices_JSON"]?.ToString();
                            if (!string.IsNullOrEmpty(servicesJson))
                            {
                                item.OtherServices = System.Text.Json.JsonSerializer
                                    .Deserialize<List<QuotationOtherServiceModel>>(servicesJson) ?? new();
                            }

                            list.Add(item);
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Quotation fetched successfully!";
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
        [Route("SaveQuotation")]
        public IActionResult SaveQuotation([FromBody] SaveQuotationRequest model)
        {
            apiResponse apiResponse = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long quotationId;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveQuotation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BanquetId", model.BanquetId);
                    cmd.Parameters.AddWithValue("@SlotId", model.SlotId);
                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                    cmd.Parameters.AddWithValue("@TillDate", model.TillDate);
                    cmd.Parameters.AddWithValue("@StartTime", model.StartTime ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndTime", model.EndTime ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@ClientId", model.ClientId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ClientName", model.ClientName);
                    cmd.Parameters.AddWithValue("@Address", model.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNo", model.ContactNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GSTIN", model.GSTIN ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StateId", model.StateId ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@EventSubTypeId", model.EventSubTypeId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BanquetRent", model.BanquetRent);
                    cmd.Parameters.AddWithValue("@Pax", model.Pax);
                    cmd.Parameters.AddWithValue("@ReferenceTypeId", model.ReferenceTypeId ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@DecorationChargesAmount", model.DecorationChargesAmount);
                    cmd.Parameters.AddWithValue("@SoundSystemChargesAmount", model.SoundSystemChargesAmount);
                    cmd.Parameters.AddWithValue("@ProjectorChargesAmount", model.ProjectorChargesAmount);

                    cmd.Parameters.AddWithValue("@IsPrintPriceExclusive", model.IsPrintPriceExclusive);
                    cmd.Parameters.AddWithValue("@IsPrintFoodItems", model.IsPrintFoodItems);

                    cmd.Parameters.AddWithValue("@SubTotal", model.SubTotal);
                    cmd.Parameters.AddWithValue("@TaxAmount", model.TaxAmount);
                    cmd.Parameters.AddWithValue("@GrandTotal", model.GrandTotal);
                    cmd.Parameters.AddWithValue("@Notes", model.Notes ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@PackagesJson", model.PackagesJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherServicesJson", model.OtherServicesJson ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    quotationId = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Quotation updated successfully!"
                    : "Quotation created successfully!";
                apiResponse.data = new { QuotationId = quotationId };

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
        [Route("DeleteQuotation")]
        public IActionResult DeleteQuotation([FromBody] DeleteQuotationRequest model)
        {
            apiResponse apiResponse = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteQuotation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Quotation deleted successfully!";
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
    }
}
