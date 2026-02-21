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
    public class ItemCategoryMasterController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public ItemCategoryMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetItemCategoryList")]
        public IActionResult GetItemCategoryList([FromBody] GetItemCategoryRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetItemCategoryList> categories = new List<GetItemCategoryList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetItemCategoryList", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", SqlDbType.BigInt).Value = request.Id == 0 ? 0 : request.Id;

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                con.Close();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GetItemCategoryList category = new GetItemCategoryList();

                        category.Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["Id"]);
                        category.CategoryName = row["CategoryName"] == DBNull.Value ? string.Empty : Convert.ToString(row["CategoryName"]);
                        category.Description = row["Description"] == DBNull.Value ? string.Empty : Convert.ToString(row["Description"]);
                        category.IsActive = row["IsActive"] == DBNull.Value ? false : Convert.ToBoolean(row["IsActive"]);

                        categories.Add(category);
                    }
                }

                apiResponse.message = "Item Categories fetched successfully!";
                apiResponse.data = categories;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveItemCategory")]
        public IActionResult SaveItemCategory([FromBody] SaveItemCategory model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveItemCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // Assign parameters
                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = model.CategoryName ?? string.Empty;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 300).Value = model.Description ?? (object)DBNull.Value;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                con.Open();

                // Stored procedure returns @Id (ScopeIdentity or existing Id)
                object result = cmd.ExecuteScalar();
                con.Close();

                long returnedId = 0;
                if (result != null && long.TryParse(result.ToString(), out long newId))
                {
                    returnedId = newId;
                }

                if (model.Id > 0)
                {
                    apiResponse.message = "Item Category updated successfully!";
                }
                else
                {
                    apiResponse.message = "Item Category inserted successfully!";
                }

                apiResponse.status = "Success";
                apiResponse.data = new { Id = returnedId };
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("DeleteItemCategory")]
        public IActionResult DeleteItemCategory([FromBody] DeleteItemCategory request)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteItemCategory", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = request.Id;
                    cmd.Parameters.AddWithValue("@actionBy", SqlDbType.Bit).Value = request.ActionBy;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                apiResponse.message = "ItemCategory deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

    }
}
