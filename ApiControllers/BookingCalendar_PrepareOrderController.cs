using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using Humanizer;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingCalendar_PrepareOrderController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;

        public BookingCalendar_PrepareOrderController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}";

            // Force HTTPS if needed
            if (!domainUrl.Contains("localhost") && domainUrl.StartsWith("http://") && !domainUrl.Contains("tenxsoftwarek-001-site1"))
            {
                domainUrl = domainUrl.Replace("http://", "https://");
            }
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetBookingCalendar_PrepareOrder")]
        public IActionResult GetBookingCalendar_PrepareOrder(GetPrepareOrderRequest request)
        {
            apiResponse response = new apiResponse();
            try
            {
                // 🔹 Validate and get connection string
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                // 🔹 Database setup
                PrepareOrderResponseModel model = new PrepareOrderResponseModel();
                List<PrepareOrderFoodItemModel> foodItems = new();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingCalendar_PrepareOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                    cmd.Parameters.AddWithValue("@PrepareOrderId", request.PrepareOrderId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.Id = Convert.ToInt64(reader["Id"]);
                            model.BookingId = Convert.ToInt64(reader["BookingId"]);
                            model.EnquiryDealId = Convert.ToInt64(reader["EnquiryDealId"]);
                            model.EnquiryDealName = reader["EnquiryDealName"]?.ToString();
                            model.PAX = Convert.ToInt32(reader["PAX"]);
                            model.PONumber = reader["PONumber"]?.ToString();
                            model.PODate = Convert.ToString(reader["PODate"]);
                            model.EventDate = Convert.ToString(reader["EventDate"]);
                            model.SlotId = Convert.ToInt64(reader["SlotId"]);
                            model.Notes = reader["Notes"]?.ToString();

                            // ===========================
                            // FoodItems JSON
                            // ===========================
                            string? foodItemsJson = reader["FoodItemsJson"] == DBNull.Value ? "[]" : reader["FoodItemsJson"].ToString();
                            foodItems = JsonConvert.DeserializeObject<List<PrepareOrderFoodItemModel>>(foodItemsJson!) 
                                        ?? new List<PrepareOrderFoodItemModel>();
                        }
                    }
                }

                model.FoodItems = foodItems;

                response.status = "Success";
                response.message = "Prepare order fetched successfully";
                response.data = model;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return StatusCode(500, response);
            }
        }


        [HttpPost]
        [Route("SaveBookingCalendar_PrepareOrder")]
        public IActionResult SaveBookingCalendar_PrepareOrder([FromBody] SavePrepareOrderRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                // 🔹 Validate connection
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                // 🔹 Build JSON exactly as SP expects
                string foodItemsJson = JsonConvert.SerializeObject(
                    model.FoodItems.Select(fi => new
                    {
                        FoodItemId = fi.FoodItemId,
                        IsAdded = fi.IsAdded,
                        FoodFormula = fi.FoodFormula == null ? null : new
                        {
                            PeopleCount = fi.FoodFormula.PeopleCount,
                            IsFormulaRequired = fi.FoodFormula.IsFormulaRequired,
                            Ingredients = fi.FoodFormula.Ingredients?.Select(i => new
                            {
                                IngredientName = i.IngredientName,
                                Quantity = i.Quantity,
                                UnitMasterId = i.UnitMasterId
                            })
                        }
                    }),
                    Formatting.None
                );

                long prepareOrderId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBookingCalendar_PrepareOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                    cmd.Parameters.AddWithValue("@EnquiryDealId", model.EnquiryDealId);
                    cmd.Parameters.AddWithValue("@PAX", model.PAX);
                    cmd.Parameters.AddWithValue("@PONumber", model.PONumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PODate", model.PODate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventDate", model.EventDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SlotId", model.SlotId);
                    cmd.Parameters.AddWithValue("@Notes", model.Notes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FoodItemsJson", foodItemsJson);
                    cmd.Parameters.AddWithValue("@Action_By", model.ActionBy);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            prepareOrderId = reader["PrepareOrderId"] != DBNull.Value
                                ? Convert.ToInt64(reader["PrepareOrderId"])
                                : 0;
                        }
                    }

                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Prepare Order updated successfully!"
                    : "Prepare Order created successfully!";
                apiResponse.data = new { PrepareOrderId = prepareOrderId };

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


        [HttpPost]
        [Route("DeleteBookingCalendar_PrepareOrder")]
        public IActionResult DeleteBookingCalendar_PrepareOrder([FromBody] DeletePrepareOrderRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                // 🔹 Validate connection
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteBookingCalendar_PrepareOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrepareOrderId", model.PrepareOrderId);
                    cmd.Parameters.AddWithValue("@Action_By", model.Action_By);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Prepare Order deleted successfully!";
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
    }
}
