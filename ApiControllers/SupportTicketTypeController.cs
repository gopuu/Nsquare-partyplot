using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketTypeController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public SupportTicketTypeController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }


        [HttpPost]
        [Route("GetSupportTicketTypeList")]
        public IActionResult GetSupportTicketTypeList([FromBody] GetSupportTicketTypeRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<SupportTicketTypeModel> types = new List<SupportTicketTypeModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSupportTicketType", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    types.Add(new SupportTicketTypeModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        Code = Convert.ToString(row["Code"]),
                        Name = Convert.ToString(row["Name"]),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket type list fetched successfully!";
                apiResponse.data = types;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log error (custom logger)
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                return Ok(apiResponse); // Always 200 OK, even for errors
            }
        }


        [HttpPost]
        [Route("SaveSupportTicketType")]
        public IActionResult SaveSupportTicketType([FromBody] SaveSupportTicketTypeRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                long updatedId = 0;
                string dbStatus = "Success";
                string dbMessage = "";

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveSupportTicketType", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Code", model.Code);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            updatedId = Convert.ToInt64(dr["UpdatedId"]);
                            dbStatus = Convert.ToString(dr["Status"]);
                            dbMessage = Convert.ToString(dr["Message"]);
                        }
                    }
                }

                apiResponse.status = dbStatus;
                apiResponse.message = dbMessage;
                apiResponse.data = new { StatusId = updatedId };

                return Ok(apiResponse); // 🔥 ALWAYS 200 OK
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Something went wrong. Please try again.";

                return Ok(apiResponse);
            }
        }



        [HttpPost]
        [Route("DeleteSupportTicketType")]
        public IActionResult DeleteSupportTicketType([FromBody] DeleteSupportTicketTypeRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteSupportTicketType", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket type deleted successfully!";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                return Ok(apiResponse);
            }
        }
    }
}
