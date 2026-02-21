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
    public class EmergencyContactMasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public EmergencyContactMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetEmergencyContact_Master")]
        public IActionResult GetEmergencyContact_Master([FromBody] GetEmergencyContactRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetEmergencyContactResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("GetEmergencyContact_Master", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);
                cmd.Parameters.AddWithValue("@SubCategoryId", request?.SubCategoryId ?? 0);
                cmd.Parameters.AddWithValue("@IsActive",
                    request?.IsActive ?? (object)DBNull.Value);

                con.Open();

                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new GetEmergencyContactResponse
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        SubCategoryId = Convert.ToInt64(dr["SubCategoryId"]),
                        SubCategoryName = Convert.ToString(dr["SubCategoryName"]),
                        CategoryId = Convert.ToInt64(dr["CategoryId"]),
                        CategoryName = Convert.ToString(dr["CategoryName"]),
                        ContactType = Convert.ToString(dr["ContactType"]),
                        ContactName = Convert.ToString(dr["ContactName"]),
                        MobileNo = Convert.ToString(dr["MobileNo"]),
                        MobileNo2 = dr["MobileNo2"] == DBNull.Value
                            ? null
                            : Convert.ToString(dr["MobileNo2"]),
                        Location = dr["Location"] == DBNull.Value
                            ? null
                            : Convert.ToString(dr["Location"]),
                        Remarks = dr["Remarks"] == DBNull.Value
                            ? null
                            : Convert.ToString(dr["Remarks"]),
                        IsActive = Convert.ToBoolean(dr["IsActive"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Emergency contacts fetched successfully!";
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
        [Route("SaveEmergencyContact_Master")]
        public IActionResult SaveEmergencyContact_Master([FromBody] SaveEmergencyContactRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long id;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("SaveEmergencyContact_Master", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id <= 0 ? 0 : model.Id);
                cmd.Parameters.AddWithValue("@SubCategoryId", model.SubCategoryId);
                cmd.Parameters.AddWithValue("@ContactType", model.ContactType);
                cmd.Parameters.AddWithValue("@ContactName", model.ContactName);
                cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                cmd.Parameters.AddWithValue("@MobileNo2",
                    model.MobileNo2 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Location",
                    model.Location ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks",
                    model.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                id = Convert.ToInt64(cmd.ExecuteScalar());
                con.Close();

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Emergency contact updated successfully!"
                    : "Emergency contact added successfully!";
                apiResponse.data = new { Id = id };

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
        [Route("DeleteEmergencyContact_Master")]
        public IActionResult DeleteEmergencyContact_Master([FromBody] DeleteEmergencyContactRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("DeleteEmergencyContact_Master", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();

                apiResponse.status = "Success";
                apiResponse.message = "Emergency contact deleted successfully!";
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
