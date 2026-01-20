using ERPAdmin.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using ERPAdmin.Models;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketPriorityController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public SupportTicketPriorityController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }


        [HttpPost]
        [Route("GetSupportTicketPriorityList")]
        public IActionResult GetSupportTicketPriorityList([FromBody] GetSupportTicketPriorityRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<SupportTicketPriorityModel> priorityList = new List<SupportTicketPriorityModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSupportTicketPriority", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    priorityList.Add(new SupportTicketPriorityModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        Code = Convert.ToString(row["Code"]),
                        Name = Convert.ToString(row["Name"]),
                        SortOrder = Convert.ToInt64(row["SortOrder"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket priority list fetched successfully!";
                apiResponse.data = priorityList;

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

        

        [HttpPost]
        [Route("SaveSupportTicketPriority")]
        public IActionResult SaveSupportTicketPriority([FromBody] SaveSupportTicketPriorityRequest model)
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
                using (SqlCommand cmd = new SqlCommand("SaveSupportTicketPriority", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Code", model.Code);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@SortOrder", model.SortOrder);
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
        [Route("DeleteSupportTicketPriority")]
        public IActionResult DeleteSupportTicketPriority([FromBody] DeleteSupportTicketPriorityRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteSupportTicketPriority", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket priority deleted successfully!";

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
