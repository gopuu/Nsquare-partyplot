using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryDashboardController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public InventoryDashboardController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetInventoryDashboardKeyStatisticsCount")]
        public IActionResult GetInventoryDashboardKeyStatisticsCount([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            InventoryDashboardKeyStatisticsCount result = null;

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
                    SqlCommand cmd = new SqlCommand("Dashboard_Inventory_GetKeyStatisticsCount", con);
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

                    result = new InventoryDashboardKeyStatisticsCount
                    {
                        TotalSales = Convert.ToDecimal(row["TotalSales"]),
                        TotalPurchase = Convert.ToDecimal(row["TotalPurchase"]),
                        TotalWastage = Convert.ToDecimal(row["TotalWastage"])
                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard Inventory KeyStatistics fetched successfully.";
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
        [Route("GetInventoryDashboardGetCOGSCount")]
        public IActionResult GetInventoryDashboardGetCOGSCount([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            InventoryDashboardGetCOGSCount result = null;

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
                    SqlCommand cmd = new SqlCommand("Dashboard_Inventory_GetCOGSCount", con);
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

                    result = new InventoryDashboardGetCOGSCount
                    {
                        WorthofStockConsumed = Convert.ToDecimal(row["WorthofStockConsumed"])
                       
                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard Inventory KeyStatistics fetched successfully.";
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
        [Route("GetInventoryDashboardGetPending_Purchases_ApprovalsCount")]
        public IActionResult GetInventoryDashboardGetPending_Purchases_ApprovalsCount([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            InventoryDashboardPending_Purchases_Approvals result = null;

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
                    SqlCommand cmd = new SqlCommand("Dashboard_Inventory_GetPending_Purchases_Approvals", con);
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

                    result = new InventoryDashboardPending_Purchases_Approvals
                    {
                        PendingPurchases = Convert.ToDecimal(row["PendingPurchases"]),
                        PendingApprovals = Convert.ToDecimal(row["PendingApprovals"])

                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard Purchases_Approvals fetched successfully.";
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
        [Route("GetInventoryDashboardStockLevel")]
        public IActionResult GetInventoryDashboardStockLevel([FromBody] DashboardinventoryRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<InventoryDashboardStockLevel> result = new();

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
                    SqlCommand cmd = new SqlCommand("Dashboard_Inventory_GetStockLevel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 🔹 (optional future params)
                    cmd.Parameters.AddWithValue("@ItemCategoryId", request.ItemCategoryId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new InventoryDashboardStockLevel
                    {
                        ProductName = row["ProductName"].ToString(),
                        Quantity = Convert.ToInt32(row["Quantity"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Dashboard Stock Level fetched successfully.";
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
        [Route("GetInventoryDashboardSuppliers")]
        public IActionResult GetInventoryDashboardSuppliers([FromBody] Dashbaord_Inventory_SupplierItems_Request request)
        {
            apiResponse apiResponse = new apiResponse();
            List<InventorySupplierItem> result = new();

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
                    SqlCommand cmd = new SqlCommand("Dashboard_Inventory_GetSuppliersWithItems", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierId", request.SupplierId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new InventorySupplierItem
                    {
                        SupplierName = row["SupplierName"].ToString(),
                        ItemName = row["ItemName"].ToString(),
                        Quantity = Convert.ToInt32(row["Quantity"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Suppliers & Items fetched successfully.";
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
        [Route("GetInventoryDashboardStockOutSummary")]
        public IActionResult GetInventoryDashboardStockOutSummary([FromBody] DashboardInventoryStockOutRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<InventoryDashboardStockOutSummary> result = new();

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
                    using (SqlCommand cmd = new SqlCommand("Dashboard_Inventory_GetStockOutSummary_RawMaterialWise", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MonthDuration", (object?)request.MonthDuration ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", (object?)request.FromDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", (object?)request.ToDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RawMaterialId", request.RawMaterialId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new InventoryDashboardStockOutSummary
                    {
                        RawMaterialId = Convert.ToInt64(row["RawMaterialId"]),
                        RawMaterialName = row["RawMaterialName"].ToString(),
                        StockOutCount = Convert.ToInt32(row["StockOutCount"]),
                        TotalStockOutQty = Convert.ToDecimal(row["TotalStockOutQty"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Inventory dashboard stock-out summary fetched successfully.";
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
