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
    public class SupportTicketHistoryController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public SupportTicketHistoryController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetSupportTicketHistoryList")]
        public IActionResult GetSupportTicketHistoryList([FromBody] GetSupportTicketHistoryRequest request)
        {
            apiResponse response = new apiResponse();
            List<SupportTicketHistory> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string conn, out string domain, out IActionResult error))
                    return Ok(error);

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(conn))
                using (SqlCommand cmd = new SqlCommand("GetSupportTicketHistory", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TicketId", request.TicketId);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new SupportTicketHistory
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        TicketId = Convert.ToInt64(row["TicketId"]),
                        Status = Convert.ToString(row["Status"]),
                        Remarks = Convert.ToString(row["Remarks"]),
                        ActionBy = Convert.ToInt64(row["ActionBy"]),
                        ActionDate = Convert.ToDateTime(row["ActionDate"])
                    });
                }

                response.status = "Success";
                response.message = "Ticket history fetched successfully!";
                response.data = list;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response);
        }


        [HttpPost]
        [Route("SaveSupportTicketHistory")]
        public IActionResult SaveSupportTicketHistory([FromBody] SaveSupportTicketHistoryRequest model)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string conn, out string domain, out IActionResult error))
                    return Ok(error);

                long id = 0;

                using (SqlConnection con = new SqlConnection(conn))
                using (SqlCommand cmd = new SqlCommand("SaveSupportTicketHistory", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@TicketId", model.TicketId);
                    cmd.Parameters.AddWithValue("@Status", model.Status);
                    cmd.Parameters.AddWithValue("@Remarks", model.Remarks);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    id = Convert.ToInt64(cmd.ExecuteScalar());
                }

                response.status = "Success";
                response.message = model.Id > 0
                    ? "Ticket history updated successfully!"
                    : "Ticket history added successfully!";
                response.data = new { HistoryId = id };
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response);
        }



        [HttpPost]
        [Route("DeleteSupportTicketHistory")]
        public IActionResult DeleteSupportTicketHistory([FromBody] DeleteSupportTicketHistoryRequest model)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string conn, out string domain, out IActionResult error))
                    return Ok(error);

                using (SqlConnection con = new SqlConnection(conn))
                using (SqlCommand cmd = new SqlCommand("DeleteSupportTicketHistory", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                response.status = "Success";
                response.message = "Ticket history deleted successfully!";
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response);
        }


    }
}
