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
    public class EmergencyContactSubCategoryMasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public EmergencyContactSubCategoryMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetEmergencyContact_SubCategoryMaster")]
        public IActionResult GetEmergencyContact_SubCategoryMaster([FromBody] GetEmergencyContactSubCategoryRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetEmergencyContactSubCategoryResponse> subCategories =
                new List<GetEmergencyContactSubCategoryResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("GetEmergencyContact_SubCategoryMaster", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);
                cmd.Parameters.AddWithValue("@CategoryId", request?.CategoryId ?? 0);

                con.Open();

                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    subCategories.Add(new GetEmergencyContactSubCategoryResponse
                    {
                        Id = dr["Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Id"]),
                        CategoryId = dr["EmergencyContactCategoryId"] == DBNull.Value
                            ? 0
                            : Convert.ToInt64(dr["EmergencyContactCategoryId"]),
                        CategoryName = dr["CategoryName"] == DBNull.Value
                            ? string.Empty
                            : Convert.ToString(dr["CategoryName"]),
                        SubCategoryName = dr["Name"] == DBNull.Value
                            ? string.Empty
                            : Convert.ToString(dr["Name"]),
                        Description = dr["Description"] == DBNull.Value
                            ? null
                            : Convert.ToString(dr["Description"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Emergency contact sub-categories fetched successfully!";
                apiResponse.data = subCategories;

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
        [Route("SaveEmergencyContact_SubCategoryMaster")]
        public IActionResult SaveEmergencyContact_SubCategoryMaster([FromBody] SaveEmergencyContactSubCategoryRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                long subCategoryId;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("SaveEmergencyContact_SubCategoryMaster", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id ?? 0);
                cmd.Parameters.AddWithValue("@EmergencyContactCategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@Name", model.SubCategoryName);
                cmd.Parameters.AddWithValue("@Description",
                    model.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                subCategoryId = Convert.ToInt64(cmd.ExecuteScalar());
                con.Close();

                apiResponse.status = "Success";
                apiResponse.message = (model.Id ?? 0) > 0
                    ? "Emergency contact sub-category updated successfully!"
                    : "Emergency contact sub-category added successfully!";
                apiResponse.data = new { SubCategoryId = subCategoryId };

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
        [Route("DeleteEmergencyContact_SubCategoryMaster")]
        public IActionResult DeleteEmergencyContact_SubCategoryMaster([FromBody] DeleteEmergencyContactSubCategoryRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("DeleteEmergencyContact_SubCategoryMaster", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    bool success = Convert.ToInt32(reader["Success"]) == 1;
                    apiResponse.status = success ? "Success" : "Failed";
                    apiResponse.message = reader["Message"]?.ToString();
                }
                else
                {
                    apiResponse.status = "Failed";
                    apiResponse.message = "No response from database.";
                }

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
