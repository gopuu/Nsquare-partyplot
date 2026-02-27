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
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyFoodItems", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? (object)DBNull.Value;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    var headersDict = new Dictionary<long, FoodHeaderResponse>();

                    while (reader.Read())
                    {
                        long headerId = Convert.ToInt64(reader["Id"]);
                        string headerName = Convert.ToString(reader["header_name"]);
                        string itemsJson = Convert.ToString(reader["ItemsJson"]);

                        if (!headersDict.ContainsKey(headerId))
                        {
                            headersDict[headerId] = new FoodHeaderResponse
                            {
                                Id = headerId,
                                HeaderName = headerName,
                                Items = new List<ItemsRequest>()
                            };
                        }

                        if (!string.IsNullOrEmpty(itemsJson))
                        {
                            // ✅ Deserialize directly into List<ItemsRequest>
                            var items = JsonConvert.DeserializeObject<List<ItemsRequest>>(itemsJson);
                            headersDict[headerId].Items.AddRange(items);
                        }
                    }
                    foodItems = headersDict.Values.ToList();

                    // ---------- 2nd Result Set : Cuisines ----------
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            string name = Convert.ToString(reader["cuisinename"]);
                            bool isActive = Convert.ToBoolean(reader["isactive"]);

                            if (!string.IsNullOrEmpty(name))
                            {
                                cuisines[name] = isActive;
                            }
                        }
                    }

                    reader.Close();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Data fetched successfully.";
                apiResponse.data = new
                {
                    FoodItems = foodItems, // headers with child items array
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

                    // 1️⃣ Save All Food Headers + Items (Items passed as JSON)
                    foreach (var header in model.FoodItems)
                    {
                        // Convert child items list to JSON for SQL
                        string itemsJson = JsonConvert.SerializeObject(header.Items);

                        using (SqlCommand cmd = new SqlCommand("SaveCompanyFoodItem", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object?)header.Id ?? DBNull.Value;
                            cmd.Parameters.Add("@header_name", SqlDbType.NVarChar, 200).Value = (object?)header.HeaderName ?? DBNull.Value;
                            cmd.Parameters.Add("@items_json", SqlDbType.NVarChar, -1).Value = (object?)itemsJson ?? DBNull.Value;
                            cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = (object?)model.ActionBy ?? DBNull.Value;

                            rowsAffected += cmd.ExecuteNonQuery();
                        }
                    }

                    // 2️⃣ Save Cuisines (unchanged)
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
                                rowsAffected += Convert.ToInt32(result);
                            }
                        }
                    }

                    con.Close();

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

        /*
        [HttpPost]
        [Route("SaveCompanyFoodItems2")]
        public IActionResult SaveCompanyFoodItems2([FromBody] SaveCompanyFoodItemsRequest model)
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

                    // 1️⃣ Save all FoodItems
                    foreach (var item in model.FoodItems)
                    {
                        using (SqlCommand cmd = new SqlCommand("SaveCompanyFoodItem", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object?)item.Id ?? DBNull.Value;
                            cmd.Parameters.Add("@header_name", SqlDbType.NVarChar, 200).Value = (object?)item.HeaderName ?? DBNull.Value;
                            cmd.Parameters.Add("@item_name", SqlDbType.NVarChar, 200).Value = (object?)item.ItemName ?? DBNull.Value;
                            cmd.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value = (object?)item.Description ?? DBNull.Value;
                            cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = (object?)model.ActionBy ?? DBNull.Value;

                            rowsAffected += cmd.ExecuteNonQuery();
                        }
                    }

                    // 2️⃣ Save Cuisines (once only)
                    if (model.Cuisines != null && model.Cuisines.Any())
                    {
                        foreach (var cuisine in model.Cuisines)
                        {
                            using (SqlCommand cmdCuisine = new SqlCommand("SaveCompanyFoodItem_Cuisines", con))
                            {
                                cmdCuisine.CommandType = CommandType.StoredProcedure;

                                cmdCuisine.Parameters.Add("@Id", SqlDbType.Int).Value = 0; // new insert
                                cmdCuisine.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = cuisine.Key;
                                cmdCuisine.Parameters.Add("@Is_Active", SqlDbType.Bit).Value = cuisine.Value;

                                object result = cmdCuisine.ExecuteScalar();
                                rowsAffected += Convert.ToInt32(result);
                            }
                        }
                    }

                    con.Close();

                    if (rowsAffected > 0)
                    {
                        apiResponse.message = "Food items and cuisines saved successfully!";
                        apiResponse.status = "Success";
                    }
                    else
                    {
                        apiResponse.message = "No changes were made.";
                        apiResponse.status = "Fail";
                    }
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
        */
    }
}
public class SaveCompanyFoodItemsRequest
{
    public List<FoodItemRequest> FoodItems { get; set; } = new();
    public Dictionary<string, bool> Cuisines { get; set; } = new(); // Common cuisines (once only)
    public long? ActionBy { get; set; }
}

public class FoodItemRequest
{
    public long? Id { get; set; }
    public string? HeaderName { get; set; }
    public List<ItemsRequest>? Items { get; set; } = new();
}
public class FoodHeaderResponse
{
    public long? Id { get; set; }
    public string? HeaderName { get; set; }
    public List<ItemsRequest>? Items { get; set; } = new();
}
public class ItemsRequest
{
    public long? ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? Description { get; set; }
}
