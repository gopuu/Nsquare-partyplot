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
    public class AssetCategoryController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetCategoryController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAssetCategoryMaster")]
        public IActionResult GetAssetCategoryMaster([FromBody] GetAssetCategoryRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetAssetCategoryResponse> list = new List<GetAssetCategoryResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetCategoryMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                // 🔹 MAP DATATABLE → MODEL
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new GetAssetCategoryResponse
                    {
                        Id = dr["Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Id"]),
                        Name = dr["Name"] == DBNull.Value ? "" : Convert.ToString(dr["Name"]),
                        Code = dr["Code"] == DBNull.Value ? "" : Convert.ToString(dr["Code"]),
                        Description = dr["Description"] == DBNull.Value ? null : Convert.ToString(dr["Description"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Asset categories fetched successfully!";
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
        [Route("SaveAssetCategoryMaster")]
        public IActionResult SaveAssetCategoryMaster([FromBody] SaveAssetCategoryRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                long assetCategoryId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAssetCategoryMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Code", model.Code);
                    cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    assetCategoryId = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Asset category updated successfully!"
                    : "Asset category created successfully!";
                apiResponse.data = new { AssetCategoryId = assetCategoryId };

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
        [Route("DeleteAssetCategoryMaster")]
        public IActionResult DeleteAssetCategoryMaster([FromBody] DeleteAssetCategoryRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteAssetCategoryMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Asset category deleted successfully!";
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
