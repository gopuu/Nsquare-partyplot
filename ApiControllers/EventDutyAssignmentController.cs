using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class EventDutyAssignmentController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public EventDutyAssignmentController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // ===========================
    // 1. GET EVENT DUTY ASSIGNMENT
    // ===========================
    [HttpPost]
    [Route("GetEventDutyAssignment")]
    public IActionResult GetEventDutyAssignment([FromBody] GetEventDutyAssignmentRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<EventDutyAssignmentModel> list = new List<EventDutyAssignmentModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("GetEventDutyAssignment", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);
                cmd.Parameters.AddWithValue("@StaffId", request.StaffId == 0 ? 0 : request.StaffId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new EventDutyAssignmentModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    BookingId = Convert.ToInt64(row["BookingId"]),
                    StaffId = Convert.ToInt64(row["StaffId"]),
                    DutyRoleId = Convert.ToInt64(row["DutyRoleId"]),

                    DutyStartTime = row["DutyStartTime"] == DBNull.Value ? null : Convert.ToString(row["DutyStartTime"]),
                    DutyEndTime = row["DutyEndTime"] == DBNull.Value ? null : Convert.ToString(row["DutyEndTime"]),

                    Remarks = Convert.ToString(row["Remarks"]),
                    StaffName = Convert.ToString(row["StaffName"]),
                    RoleName = Convert.ToString(row["RoleName"]),
                    StatusName = Convert.ToString(row["StatusName"]),
                    EventName = Convert.ToString(row["EventName"]),

                    StatusId = row["StatusId"] == DBNull.Value ? null : Convert.ToInt64(row["StatusId"])
                });
            }

            apiResponse.message = "Event duty assignments fetched successfully!";
            apiResponse.data = list;
            apiResponse.status = "Success";
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogException(ex, ControllerContext, Request, null);

            apiResponse.message = "Error: " + ex.Message;
            apiResponse.status = "Error";
            return StatusCode(500, apiResponse);
        }
    }

    // ===========================
    // 2. SAVE EVENT DUTY ASSIGNMENT
    // ===========================
    [HttpPost]
    [Route("SaveEventDutyAssignment")]
    public IActionResult SaveEventDutyAssignment([FromBody] SaveEventDutyAssignmentRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SaveEventDutyAssignment", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
                cmd.Parameters.AddWithValue("@DutyRoleId", model.DutyRoleId);

                cmd.Parameters.AddWithValue("@DutyStartTime", (object)model.DutyStartTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DutyEndTime", (object)model.DutyEndTime ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StatusId", (object)model.StatusId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0
                ? "Event duty updated successfully!"
                : "Event duty assigned successfully!";

            apiResponse.data = new { DutyId = rowId };
            apiResponse.status = "Success";
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogException(ex, ControllerContext, Request, null);

            apiResponse.message = "Error: " + ex.Message;
            apiResponse.status = "Error";
            return StatusCode(500, apiResponse);
        }
    }



    // ===========================
    // 3. DELETE EVENT DUTY ASSIGNMENT
    // ===========================
    [HttpPost]
    [Route("DeleteEventDutyAssignment")]
    public IActionResult DeleteEventDutyAssignment([FromBody] DeleteEventDutyAssignmentRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("DeleteEventDutyAssignment", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Event duty deleted successfully!";
            apiResponse.status = "Success";
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogException(ex, ControllerContext, Request, null);

            apiResponse.message = "Error: " + ex.Message;
            apiResponse.status = "Error";
            return StatusCode(500, apiResponse);
        }
    }
}
