using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using System.Xml.Linq;
using ExcelDataReader.Log;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Company_FoodRate_ItemController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_FoodRate_ItemController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";

            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("DeleteCompanyFoodItem")]
        public IActionResult DeleteCompanyFoodItem([FromBody] DeleteCompanyFoodItemRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteCompanyFoodItem", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = model.DeletedBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                apiResponse.message = "Food item deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteCompanyFoodItem_Items")]
        public IActionResult DeleteCompanyFoodItem_Items([FromBody] DeleteCompanyFoodItemRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteCompanyFoodItem_Items", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@actionby", SqlDbType.BigInt).Value = model.DeletedBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                apiResponse.message = "Food item deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetCompanyFoodItem")]
        public IActionResult GetCompanyFoodItem([FromBody] GetCompanyFoodItemsRequestById model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                List<FoodHeaderResponse> foodItems = new List<FoodHeaderResponse>();
                Dictionary<string, bool> cuisines = new Dictionary<string, bool>();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCompanyFoodItems", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? 0;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // ============================================
                        // 1️⃣ FIRST RESULT SET: HEADERS + ITEMS
                        // ============================================

                        while (reader.Read())
                        {
                            long headerId = Convert.ToInt64(reader["Id"]);

                            string? headerName = reader["header_name"] == DBNull.Value
                                ? null
                                : Convert.ToString(reader["header_name"]);

                            string? itemsJson = reader["ItemsJson"] == DBNull.Value
                                ? null
                                : Convert.ToString(reader["ItemsJson"]);

                            List<ItemsRequest> items = new List<ItemsRequest>();

                            if (!string.IsNullOrWhiteSpace(itemsJson))
                            {
                                items = JsonConvert.DeserializeObject<List<ItemsRequest>>(itemsJson) ?? new List<ItemsRequest>();
                            }

                            foodItems.Add(new FoodHeaderResponse
                            {
                                Id = headerId,
                                HeaderName = headerName,
                                Items = items
                            });
                        }

                        // ============================================
                        // 2️⃣ SECOND RESULT SET: CUISINES
                        // ============================================

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                string? cuisineName = reader["CuisineName"] == DBNull.Value
                                    ? null
                                    : Convert.ToString(reader["CuisineName"]);

                                bool isActive = reader["IsActive"] != DBNull.Value
                                    && Convert.ToBoolean(reader["IsActive"]);

                                if (!string.IsNullOrWhiteSpace(cuisineName))
                                {
                                    cuisines[cuisineName] = isActive;
                                }
                            }
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Data fetched successfully.";
                apiResponse.data = new
                {
                    FoodItems = foodItems,
                    Cuisines = cuisines
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyFoodItems")]
        public IActionResult SaveCompanyFoodItems([FromBody] SaveCompanyFoodItemsRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    int rowsAffected = 0;

                    // 1️⃣ Save Food Headers + Layer 2 + Layer 3 Items
                    if (model.FoodItems != null && model.FoodItems.Any())
                    {
                        foreach (var header in model.FoodItems)
                        {
                            // Convert Layer 2 + Layer 3 items into JSON
                            string itemsJson = JsonConvert.SerializeObject(header.Items ?? new List<ItemsRequest>());

                            using (SqlCommand cmd = new SqlCommand("SaveCompanyFoodItem", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object?)header.Id ?? DBNull.Value;
                                cmd.Parameters.Add("@header_name", SqlDbType.NVarChar, 200).Value = (object?)header.HeaderName ?? DBNull.Value;
                                cmd.Parameters.Add("@items_json", SqlDbType.NVarChar, -1).Value = itemsJson;
                                cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = (object?)model.ActionBy ?? DBNull.Value;

                                rowsAffected += cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // 2️⃣ Save Cuisines
                    if (model.Cuisines != null && model.Cuisines.Any())
                    {
                        foreach (var cuisine in model.Cuisines)
                        {
                            using (SqlCommand cmdCuisine = new SqlCommand("SaveCompanyFoodItem_Cuisines", con))
                            {
                                cmdCuisine.CommandType = CommandType.StoredProcedure;

                                cmdCuisine.Parameters.Add("@Id", SqlDbType.Int).Value = 0;

                                cmdCuisine.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = cuisine.Key;

                                cmdCuisine.Parameters.Add("@Is_Active", SqlDbType.Bit).Value = cuisine.Value;

                                object result = cmdCuisine.ExecuteScalar();

                                if (result != null && result != DBNull.Value)
                                {
                                    rowsAffected += Convert.ToInt32(result);
                                }
                            }
                        }
                    }

                    apiResponse.message = rowsAffected > 0
                        ? "Food items and cuisines saved successfully!"
                        : "No changes were made.";

                    apiResponse.status = rowsAffected > 0 ? "Success" : "Fail";
                }

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

    }
}
//SAVE
public class SaveCompanyFoodItemsRequest
{
    public List<FoodItemRequest> FoodItems { get; set; } = new();
    public Dictionary<string, bool> Cuisines { get; set; } = new(); // Common cuisines (once only)
    public long? ActionBy { get; set; }
}
//LAYER 1
public class FoodItemRequest
{
    public long? Id { get; set; }
    public string? HeaderName { get; set; }
    public List<ItemsRequest>? Items { get; set; } = new();
}
//LAYER 2
public class ItemsRequest
{
    public long? ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? Description { get; set; }

    // Layer 3 Children
    public List<ItemsRequest> Children { get; set; } = new();
}

//GET
public class FoodHeaderResponse
{
    public long? Id { get; set; }
    public string? HeaderName { get; set; }
    public List<ItemsRequest>? Items { get; set; } = new();
}
