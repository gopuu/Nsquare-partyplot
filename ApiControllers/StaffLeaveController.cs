
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class StaffLeaveController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public StaffLeaveController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // ====================== GET ======================
    [HttpPost]
    [Route("GetStaffLeave")]
    public IActionResult GetStaffLeave([FromBody] GetStaffLeaveRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<StaffLeaveModel> list = new List<StaffLeaveModel>();
        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("GetStaffLeave", con))
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
                list.Add(new StaffLeaveModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    StaffId = Convert.ToInt64(row["StaffId"]),
                    LeaveTypeId = Convert.ToInt64(row["LeaveTypeId"]),
                    FromDate = row["FromDate"] == DBNull.Value ? null : Convert.ToString(row["FromDate"]),
                    ToDate = row["ToDate"] == DBNull.Value ? null : Convert.ToString(row["ToDate"]),
                    Reason = Convert.ToString(row["Reason"]),
                    StatusId = row["StatusId"] == DBNull.Value ? null : Convert.ToInt64(row["StatusId"]),
                    StatusName = Convert.ToString(row["StatusName"]),
                    StaffName = Convert.ToString(row["StaffName"]),
                    LeaveTypeName=Convert.ToString(row["LeaveTypeName"])

                });
            }

            apiResponse.message = "Leave records fetched successfully!";
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

    // ====================== SAVE ======================
    [HttpPost]
    [Route("SaveStaffLeave")]
    public IActionResult SaveStaffLeave([FromBody] SaveStaffLeaveRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveStaffLeave", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
                cmd.Parameters.AddWithValue("@LeaveTypeId", model.LeaveTypeId);
                cmd.Parameters.AddWithValue("@FromDate", (object)model.FromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object)model.ToDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Reason", (object)model.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StatusId", (object)model.StatusId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0 ? "Leave updated successfully!" : "Leave inserted successfully!";
            apiResponse.data = new { LeaveId = rowId };
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

    // ====================== DELETE ======================
    [HttpPost]
    [Route("DeleteStaffLeave")]
    public IActionResult DeleteStaffLeave([FromBody] DeleteStaffLeaveRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("DeleteStaffLeave", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Leave deleted successfully!";
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
