using DocumentFormat.OpenXml.Office2010.Excel;
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
    public class AssetMasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAssetMaster")]
        public IActionResult GetAssetMaster([FromBody] GetAssetRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetAssetResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);
                    cmd.Parameters.AddWithValue("@CategoryId", request?.CategoryId ?? 0);
                    cmd.Parameters.AddWithValue("@SubCategoryId", request?.SubCategoryId ?? 0);
                    cmd.Parameters.AddWithValue("@IsActive", request?.IsActive ?? (object)DBNull.Value);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                    con.Close();
                }

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new GetAssetResponse
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        AssetName = Convert.ToString(dr["AssetName"]),
                        AssetUniqueCode = Convert.ToString(dr["AssetUniqueCode"]),
                        CategoryId = Convert.ToInt64(dr["CategoryId"]),
                        CategoryName = Convert.ToString(dr["CategoryName"]),
                        SubCategoryId = dr["SubCategoryId"] == DBNull.Value ? null : Convert.ToInt64(dr["SubCategoryId"]),
                        SubCategoryName = dr["SubCategoryName"] == DBNull.Value ? null : Convert.ToString(dr["SubCategoryName"]),
                        Location = dr["Location"] == DBNull.Value ? null : Convert.ToString(dr["Location"]),
                        Remarks = dr["Remarks"] == DBNull.Value ? null : Convert.ToString(dr["Remarks"]),
                        OpeningQty = Convert.ToDecimal(dr["OpeningQty"]),
                        ClosingQty = Convert.ToDecimal(dr["ClosingQty"]),
                        IsActive = Convert.ToBoolean(dr["IsActive"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Assets fetched successfully!";
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
        [Route("SaveAssetMaster")]
        public IActionResult SaveAssetMaster([FromBody] SaveAssetRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long assetId;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAssetMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@AssetName", model.AssetName);
                    cmd.Parameters.AddWithValue("@AssetUniqueCode", model.AssetUniqueCode);
                    cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                    cmd.Parameters.AddWithValue("@SubCategoryId", model.SubCategoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", model.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OpeningQty", model.OpeningQty);
                    cmd.Parameters.AddWithValue("@ClosingQty", model.ClosingQty);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    assetId = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Asset updated successfully!"
                    : "Asset created successfully!";
                apiResponse.data = new { AssetId = assetId };

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
        [Route("DeleteAssetMaster")]
        public IActionResult DeleteAssetMaster([FromBody] DeleteAssetRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteAssetMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Asset deleted successfully!";
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
