using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Humanizer;
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
    public class InventoryController : MasterController
    {
        private string connString = string.Empty;

        public InventoryController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetStockList")]
        public IActionResult GetStockList([FromBody] GetStockListRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetStockListModel> stocks = new List<GetStockListModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetStockList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ItemCategoryId", request.ItemCategoryId == 0 ? 0 : request.ItemCategoryId);
                    cmd.Parameters.AddWithValue("@RawMaterialId", request.RawMaterialId == 0 ? 0 : request.RawMaterialId);
                    cmd.Parameters.AddWithValue("@UnitId", request.UnitId == 0 ? 0 : request.UnitId);

                    cmd.Parameters.AddWithValue("@ItemCategoryName", request.ItemCategoryName);
                    cmd.Parameters.AddWithValue("@RawMaterialName", request.RawMaterialName);
                    cmd.Parameters.AddWithValue("@UnitName", request.UnitName);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GetStockListModel m = new GetStockListModel();

                        m.RawMaterialId = Convert.ToInt64(row["RawMaterialId"]);
                        m.RawMaterialName = Convert.ToString(row["RawMaterialName"]);
                        m.ItemCategoryId = Convert.ToInt64(row["ItemCategoryId"]);
                        m.ItemCategoryName = Convert.ToString(row["ItemCategoryName"]);
                        m.UnitId = Convert.ToInt64(row["UnitId"]);
                        m.UnitName = Convert.ToString(row["UnitName"]);
                        m.CurrentQty = Convert.ToDecimal(row["CurrentQty"]);

                        stocks.Add(m);
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Stock list fetched successfully!";
                apiResponse.data = stocks;

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
        [Route("SaveStockInOut")]
        public IActionResult SaveStockInOut([FromBody] SaveStockInOutRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                // Convert StockInOutList to JSON
                string jsonStockInOutList = JsonConvert.SerializeObject(model.StockInOutList);

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveStockInOut", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@JSON_StockInOutList", SqlDbType.NVarChar).Value =
                        (object)jsonStockInOutList ?? DBNull.Value;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                    conn.Open();
                    // Read SQL return message (Success/Error)
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string status = reader["Status"].ToString();
                            string message = reader["Message"].ToString();

                            apiResponse.status = status;
                            apiResponse.message = message;
                            apiResponse.data = null;

                            if (status == "Error")
                                return BadRequest(apiResponse);   // return 400 for validation fail
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "StockOut saved successfully!";
                apiResponse.data = null;

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
        [Route("GetStockOutHistory")]
        public IActionResult GetStockOutHistory([FromBody] GetStockOutHistoryRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetStockOutHistoryList> list = new List<GetStockOutHistoryList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetStockOutHistory", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@RawMaterialId", SqlDbType.BigInt)
                    .Value = request.RawMaterialId == 0 ? 0 : request.RawMaterialId;

                DataTable dt = new DataTable();

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                con.Close();

                foreach (DataRow row in dt.Rows)
                {
                    GetStockOutHistoryList obj = new GetStockOutHistoryList();

                    obj.Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["Id"]);
                    obj.RawMaterialId = row["RawMaterialId"] == DBNull.Value ? 0 : Convert.ToInt64(row["RawMaterialId"]);
                    obj.RawMaterialName = row["RawMaterialName"]?.ToString() ?? "";

                    obj.UnitId = row["UnitId"] == DBNull.Value ? 0 : Convert.ToInt64(row["UnitId"]);
                    obj.UnitName = row["UnitName"]?.ToString() ?? "";

                    obj.Quantity_Out = row["Quantity_Out"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Quantity_Out"]);
                    obj.Quantity_In = row["Quantity_In"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Quantity_In"]);
                    obj.Remarks = row["Remarks"]?.ToString() ?? "";

                    obj.ModifiedOn = Convert.ToString(row["ModifiedOn"]);
                    obj.ModifiedOn2 = row["ModifiedOn2"]?.ToString() ?? "";

                    obj.ModifiedBy = row["ModifiedBy"]?.ToString() ?? "";

                    list.Add(obj);
                }

                apiResponse.status = "Success";
                apiResponse.message = "Stock Out History fetched successfully!";
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
