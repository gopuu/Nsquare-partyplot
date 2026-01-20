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
    public class SupportTicketController : MasterController
    {

        private readonly string domainUrl;
        private string connString = string.Empty;
        public SupportTicketController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetSupportTicketList")]
        public IActionResult GetSupportTicketList([FromBody] GetSupportTicketRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<SupportTicket> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSupportTicket", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new SupportTicket
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        TicketTypeId = Convert.ToInt64(row["TicketTypeId"]),
                        PriorityId = Convert.ToInt64(row["PriorityId"]),
                        StatusId = Convert.ToInt64(row["StatusId"]),
                        TicketNumber = row["TicketNumber"].ToString(),
                        TicketTypeName = row["TicketTypeName"].ToString(),
                        PriorityName = row["PriorityName"].ToString(),
                        StatusName = row["StatusName"].ToString(),
                        Title = row["Title"].ToString(),
                        Description = row["Description"].ToString(),
                        IsClosed = Convert.ToBoolean(row["IsClosed"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support tickets fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return Ok(apiResponse);
            }
        }


        [HttpPost]
        [Route("SaveSupportTicket")]
        public IActionResult SaveSupportTicket([FromBody] SaveSupportTicketRequest model)
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

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveSupportTicket", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@TicketNumber", model.TicketNumber);
                    cmd.Parameters.AddWithValue("@TicketTypeId", model.TicketTypeId);
                    cmd.Parameters.AddWithValue("@PriorityId", model.PriorityId);
                    cmd.Parameters.AddWithValue("@StatusId", model.StatusId);
                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@Description", model.Description);
                    cmd.Parameters.AddWithValue("@IsClosed", model.IsClosed);
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

                return Ok(apiResponse);
              
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return Ok(apiResponse);
            }
        }


        [HttpPost]
        [Route("DeleteSupportTicket")]
        public IActionResult DeleteSupportTicket([FromBody] DeleteSupportTicketRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteSupportTicket", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket deleted successfully!";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return Ok(apiResponse);
            }
        }

    }
}
