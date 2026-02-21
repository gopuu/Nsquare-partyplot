using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawMaterialsMasterController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public RawMaterialsMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetRawMaterialList")]
        public IActionResult GetRawMaterialList([FromBody] GetRawMaterialsRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetRawMaterialsList> list = new List<GetRawMaterialsList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetRawMaterialsList", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id == 0 ? 0 : request.Id;
                cmd.Parameters.Add("@ItemCategoryId", SqlDbType.BigInt).Value = request.ItemCategoryId == 0 ? 0 : request.ItemCategoryId;

                DataTable dt = new DataTable();
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                con.Close();

                foreach (DataRow row in dt.Rows)
                {
                    GetRawMaterialsList obj = new GetRawMaterialsList();

                    obj.Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["Id"]);
                    obj.Name = row["Name"]?.ToString() ?? "";

                    obj.ConsumptionUnitId = row["ConsumptionUnitId"] == DBNull.Value ? 0 : Convert.ToInt64(row["ConsumptionUnitId"]);
                    obj.OnePurchaseUnitEquals_ConsumptionUnitQty =
                        row["OnePurchaseUnitEquals_ConsumptionUnitQty"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["OnePurchaseUnitEquals_ConsumptionUnitQty"]);

                    obj.ItemCategoryId = row["ItemCategoryId"] == DBNull.Value ? 0 : Convert.ToInt64(row["ItemCategoryId"]);
                    obj.ItemCategoryName = row["ItemCategoryName"] == DBNull.Value ? string.Empty : Convert.ToString(row["ItemCategoryName"]);

                    obj.PurchasePrice = row["PurchasePrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["PurchasePrice"]);
                    obj.TransferPrice = row["TransferPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["TransferPrice"]);
                    obj.ReconciliationPrice = row["ReconciliationPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["ReconciliationPrice"]);

                    obj.TaxTypeId = row["TaxTypeId"] == DBNull.Value ? (long?)null : Convert.ToInt64(row["TaxTypeId"]);
                    obj.TaxPercent = row["TaxPercent"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["TaxPercent"]);
                    obj.ExciseDutyAmount = row["ExciseDutyAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["ExciseDutyAmount"]);

                    obj.MinStockUnitId = row["MinStockUnitId"] == DBNull.Value ? (long?)null : Convert.ToInt64(row["MinStockUnitId"]);
                    obj.MinStockLevel = row["MinStockLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["MinStockLevel"]);
                    obj.AtParStockUnitId = row["AtParStockUnitId"] == DBNull.Value ? (long?)null : Convert.ToInt64(row["AtParStockUnitId"]);
                    obj.AtParStockLevel = row["AtParStockLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["AtParStockLevel"]);

                    obj.AllowReStockLevel = row["AllowReStockLevel"] != DBNull.Value && Convert.ToBoolean(row["AllowReStockLevel"]);

                    obj.Barcode = row["Barcode"]?.ToString();
                    obj.HSNCode = row["HSNCode"]?.ToString();
                    obj.GTIN = row["GTIN"]?.ToString();
                    obj.Brand = row["Brand"]?.ToString();

                    obj.ExclusiveToRestaurant = row["ExclusiveToRestaurant"] != DBNull.Value && Convert.ToBoolean(row["ExclusiveToRestaurant"]);
                    obj.IsExpiry = row["IsExpiry"] != DBNull.Value && Convert.ToBoolean(row["IsExpiry"]);
                    obj.AllowDecimalQty = row["AllowDecimalQty"] != DBNull.Value && Convert.ToBoolean(row["AllowDecimalQty"]);
                    obj.IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
                    obj.IsFavourite = row["IsFavourite"] != DBNull.Value && Convert.ToBoolean(row["IsFavourite"]);

                    obj.Description = row["Description"]?.ToString();
                    obj.NormalLossPercent = row["NormalLossPercent"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["NormalLossPercent"]);
                    obj.QuantityInGmMl = row["QuantityInGmMl"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["QuantityInGmMl"]);
                    obj.CurrentQty = row["CurrentQty"] == DBNull.Value ? 0 : Convert.ToDecimal(row["CurrentQty"]);

                    // -------------- JSON Parsing ------------------

                    // 1️⃣ Purchase Units (list of UnitId)
                    string jsonPurchaseUnits = row["JSON_PurchaseUnits"]?.ToString();
                    obj.JSON_PurchaseUnits = string.IsNullOrWhiteSpace(jsonPurchaseUnits)
                        ? new List<RM_PurchaseUnit>()
                        : JsonConvert.DeserializeObject<List<RM_PurchaseUnit>>(jsonPurchaseUnits);

                    // 2️⃣ Closing Stock Updated On (list of FrequencyTitleId)
                    string jsonClosing = row["JSON_ClosingStockUpdatedOn"]?.ToString();
                    obj.JSON_ClosingStockUpdatedOn = string.IsNullOrWhiteSpace(jsonClosing)
                        ? new List<RM_ClosingStockUpdatedOn>()
                        : JsonConvert.DeserializeObject<List<RM_ClosingStockUpdatedOn>>(jsonClosing);

                    // 3️⃣ Restock Levels (list of Qty + UnitId)
                    string jsonRestock = row["JSON_RestockLevel"]?.ToString();
                    obj.JSON_RestockLevel = string.IsNullOrWhiteSpace(jsonRestock)
                        ? new List<RM_RestockLevel>()
                        : JsonConvert.DeserializeObject<List<RM_RestockLevel>>(jsonRestock);

                    list.Add(obj);
                }

                apiResponse.status = "Success";
                apiResponse.message = "Raw Materials fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveRawMaterial")]
        public IActionResult SaveRawMaterial([FromBody] SaveRawMaterials model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveRawMaterials", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // ===============================
                    // 1️⃣ BASIC FIELDS
                    // ===============================
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = model.Name ?? "";

                    // ===============================
                    // 2️⃣ JSON FIELDS
                    // ===============================
                    cmd.Parameters.Add("@JSON_PurchaseUnits", SqlDbType.NVarChar)
                        .Value = (model.JSON_PurchaseUnits != null && model.JSON_PurchaseUnits.Any())
                            ? JsonConvert.SerializeObject(model.JSON_PurchaseUnits)
                            : DBNull.Value;

                    cmd.Parameters.Add("@JSON_ClosingStockUpdatedOn", SqlDbType.NVarChar)
                        .Value = (model.JSON_ClosingStockUpdatedOn != null && model.JSON_ClosingStockUpdatedOn.Any())
                            ? JsonConvert.SerializeObject(model.JSON_ClosingStockUpdatedOn)
                            : DBNull.Value;

                    cmd.Parameters.Add("@JSON_RestockLevel", SqlDbType.NVarChar)
                        .Value = (model.JSON_RestockLevel != null && model.JSON_RestockLevel.Any())
                            ? JsonConvert.SerializeObject(model.JSON_RestockLevel)
                            : DBNull.Value;

                    // ===============================
                    // 3️⃣ CONSUMPTION UNIT
                    // ===============================
                    cmd.Parameters.Add("@ConsumptionUnitId", SqlDbType.BigInt).Value = model.ConsumptionUnitId;
                    cmd.Parameters.Add("@OnePurchaseUnitEquals_ConsumptionUnitQty", SqlDbType.Decimal).Value =
                        (object)model.OnePurchaseUnitEquals_ConsumptionUnitQty ?? DBNull.Value;

                    // ===============================
                    // 4️⃣ CATEGORY
                    // ===============================
                    cmd.Parameters.Add("@ItemCategoryId", SqlDbType.BigInt).Value = model.ItemCategoryId;

                    // ===============================
                    // 5️⃣ PRICES
                    // ===============================
                    cmd.Parameters.Add("@PurchasePrice", SqlDbType.Decimal).Value =
                        (object)model.PurchasePrice ?? DBNull.Value;
                    cmd.Parameters.Add("@TransferPrice", SqlDbType.Decimal).Value =
                        (object)model.TransferPrice ?? DBNull.Value;
                    cmd.Parameters.Add("@ReconciliationPrice", SqlDbType.Decimal).Value =
                        (object)model.ReconciliationPrice ?? DBNull.Value;

                    // ===============================
                    // 6️⃣ TAXES
                    // ===============================
                    cmd.Parameters.Add("@TaxTypeId", SqlDbType.BigInt).Value =
                        (object)model.TaxTypeId ?? DBNull.Value;
                    cmd.Parameters.Add("@TaxPercent", SqlDbType.Decimal).Value =
                        (object)model.TaxPercent ?? DBNull.Value;
                    cmd.Parameters.Add("@ExciseDutyAmount", SqlDbType.Decimal).Value =
                        (object)model.ExciseDutyAmount ?? DBNull.Value;

                    // ===============================
                    // 7️⃣ STOCK LEVELS
                    // ===============================
                    cmd.Parameters.Add("@MinStockUnitId", SqlDbType.BigInt).Value =
                        (object)model.MinStockUnitId ?? DBNull.Value;
                    cmd.Parameters.Add("@MinStockLevel", SqlDbType.Decimal).Value =
                        (object)model.MinStockLevel ?? DBNull.Value;
                    cmd.Parameters.Add("@AtParStockUnitId", SqlDbType.BigInt).Value =
                        (object)model.AtParStockUnitId ?? DBNull.Value;
                    cmd.Parameters.Add("@AtParStockLevel", SqlDbType.Decimal).Value =
                        (object)model.AtParStockLevel ?? DBNull.Value;

                    cmd.Parameters.Add("@AllowReStockLevel", SqlDbType.Bit).Value = model.AllowReStockLevel;

                    // ===============================
                    // 8️⃣ BARCODE / HSN / BRAND
                    // ===============================
                    cmd.Parameters.Add("@Barcode", SqlDbType.NVarChar, 200).Value =
                        (object)model.Barcode ?? DBNull.Value;
                    cmd.Parameters.Add("@HSNCode", SqlDbType.NVarChar, 50).Value =
                        (object)model.HSNCode ?? DBNull.Value;
                    cmd.Parameters.Add("@ExclusiveToRestaurant", SqlDbType.Bit).Value = model.ExclusiveToRestaurant;
                    cmd.Parameters.Add("@IsExpiry", SqlDbType.Bit).Value = model.IsExpiry;
                    cmd.Parameters.Add("@AllowDecimalQty", SqlDbType.Bit).Value = model.AllowDecimalQty;
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value =
                        (object)model.Description ?? DBNull.Value;
                    cmd.Parameters.Add("@NormalLossPercent", SqlDbType.Decimal).Value =
                        (object)model.NormalLossPercent ?? DBNull.Value;
                    cmd.Parameters.Add("@QuantityInGmMl", SqlDbType.Decimal).Value =
                        (object)model.QuantityInGmMl ?? DBNull.Value;
                    cmd.Parameters.Add("@GTIN", SqlDbType.NVarChar, 50).Value =
                        (object)model.GTIN ?? DBNull.Value;
                    cmd.Parameters.Add("@Brand", SqlDbType.NVarChar, 200).Value =
                        (object)model.Brand ?? DBNull.Value;

                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

                    // ===============================
                    // 9️⃣ ACTION BY
                    // ===============================
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                    // ===============================
                    // 🔟 EXECUTE
                    // ===============================
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    con.Close();

                    long returnedId = 0;
                    long.TryParse(result?.ToString() ?? "0", out returnedId);

                    apiResponse.status = "Success";
                    apiResponse.message = (model.Id == 0)
                        ? "Raw Material inserted successfully!"
                        : "Raw Material updated successfully!";
                    apiResponse.data = new { Id = returnedId };

                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("DeleteRawMaterial")]
        public IActionResult DeleteRawMaterial([FromBody] DeleteRawMaterialsRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                SqlConnection conn = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("DeleteRawMaterials", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", SqlDbType.BigInt).Value = request.Id;
                cmd.Parameters.AddWithValue("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                apiResponse.status = "Success";
                apiResponse.message = "Raw Material deleted successfully!";
                apiResponse.data = null;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveRawMaterials_IsFavourite")]
        public IActionResult SaveRawMaterials_IsFavourite([FromBody] SaveFavouriteModel model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveRawMaterials_IsFavourite", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@IsFavourite", SqlDbType.Bit).Value = model.IsFavourite;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Favourite status updated successfully!";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return Ok(apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveRawMaterials_IsActive")]
        public IActionResult SaveRawMaterials_IsActive([FromBody] SaveActiveModel model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveRawMaterials_IsActive", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Active status updated successfully!";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return Ok(apiResponse);
            }
        }

        [HttpPost]
        [Route("GetRawMaterial_CategorySummary")]
        public IActionResult GetRawMaterial_CategorySummary([FromBody] GetCategorySummaryRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                List<RawMaterialCategorySummary> list = new List<RawMaterialCategorySummary>();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetRawMaterial_CategorySummary", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CategoryId", SqlDbType.BigInt).Value =
                        request.CategoryId == 0 ? 0 : request.CategoryId;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        RawMaterialCategorySummary obj = new RawMaterialCategorySummary();
                        obj.CategoryId = dr["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["CategoryId"]);
                        obj.CategoryName = dr["CategoryName"]?.ToString() ?? "";
                        obj.RawMaterialsCount = dr["RawMaterialsCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["RawMaterialsCount"]);

                        list.Add(obj);
                    }

                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Category summary fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }


    }
}
