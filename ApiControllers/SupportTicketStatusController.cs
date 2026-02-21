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
    public class SupportTicketStatusController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public SupportTicketStatusController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetSupportTicketStatusList")]
        public IActionResult GetSupportTicketStatusList([FromBody] GetSupportTicketStatusRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<SupportTicketStatusModel> statusList = new List<SupportTicketStatusModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSupportTicketStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    statusList.Add(new SupportTicketStatusModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        Code = Convert.ToString(row["Code"]),
                        Name = Convert.ToString(row["Name"]),
                        IsFinal = Convert.ToBoolean(row["IsFinal"]),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket status list fetched successfully!";
                apiResponse.data = statusList;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;

                return Ok(apiResponse); // 🔥 Always 200 OK
            }
        }

        // ========================= SAVE (INSERT / UPDATE) =========================
        [HttpPost]
        [Route("SaveSupportTicketStatus")]
        public IActionResult SaveSupportTicketStatus([FromBody] SaveSupportTicketStatusRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long updatedId = 0;
                string dbStatus = "Success";
                string dbMessage = "";

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveSupportTicketStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Code", model.Code);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@IsFinal", model.IsFinal);
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

                return Ok(apiResponse); // 🔥 Always 200 OK
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Something went wrong. Please try again.";

                return Ok(apiResponse); // 🔥 STILL 200 OK
            }
        }


        // ========================= DELETE =========================
        [HttpPost]
        [Route("DeleteSupportTicketStatus")]
        public IActionResult DeleteSupportTicketStatus([FromBody] DeleteSupportTicketStatusRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteSupportTicketStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket status deleted successfully!";

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
