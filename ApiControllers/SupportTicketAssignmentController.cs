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
    public class SupportTicketAssignmentController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public SupportTicketAssignmentController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetSupportTicketAssignmentList")]
        public IActionResult GetSupportTicketAssignmentList([FromBody] GetSupportTicketAssignmentRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<SupportTicketAssignmentModel> assignmentList = new List<SupportTicketAssignmentModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSupportTicketAssignment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    //cmd.Parameters.AddWithValue("@TicketId", request.TicketId);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    assignmentList.Add(new SupportTicketAssignmentModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        TicketId = Convert.ToInt64(row["TicketId"]),
                        TicketDisplay = row["TicketDisplay"].ToString(),
                        TicketNumber = row["TicketNumber"].ToString(),
                        Title = row["Title"].ToString(),
                        AssignedToName = row["AssignedToName"].ToString(),
                        AssignedByName = row["AssignedByName"].ToString(),
                        AssignedTo = Convert.ToInt64(row["AssignedTo"]),
                        AssignedBy = Convert.ToInt64(row["AssignedBy"]),
                        AssignedOn = Convert.ToString(row["AssignedOn"]),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket assignment list fetched successfully!";
                apiResponse.data = assignmentList;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return Ok(apiResponse); // ✅ Always 200 OK
            }
        }

        // ========================= SAVE (INSERT / UPDATE) =========================
        [HttpPost]
        [Route("SaveSupportTicketAssignment")]
        public IActionResult SaveSupportTicketAssignment([FromBody] SaveSupportTicketAssignmentRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveSupportTicketAssignment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@TicketId", model.TicketId);
                    cmd.Parameters.AddWithValue("@AssignedTo", model.AssignedTo);
                    cmd.Parameters.AddWithValue("@AssignedBy", model.AssignedBy);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Support ticket assignment updated successfully!"
                    : "Support ticket assigned successfully!";
                apiResponse.data = new { AssignmentId = rowId };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return Ok(apiResponse);
            }
        }

        // ========================= DELETE =========================
        [HttpPost]
        [Route("DeleteSupportTicketAssignment")]
        public IActionResult DeleteSupportTicketAssignment([FromBody] DeleteSupportTicketAssignmentRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteSupportTicketAssignment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Support ticket assignment deleted successfully!";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return Ok(apiResponse);
            }
        }



    }
}
