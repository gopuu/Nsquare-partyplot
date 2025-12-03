
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class StaffAttendanceController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public StaffAttendanceController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // ====================== GET ======================
    [HttpPost]
    [Route("GetStaffAttendance")]
    public IActionResult GetStaffAttendance([FromBody] GetStaffAttendanceRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<StaffAttendanceModel> list = new List<StaffAttendanceModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("GetStaffAttendance", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);
                //cmd.Parameters.AddWithValue("@StaffId", request.StaffId == 0 ? 0 : request.StaffId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new StaffAttendanceModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    StaffId = Convert.ToInt64(row["StaffId"]),
                    StaffName = Convert.ToString(row["StaffName"]),
                    StatusName = Convert.ToString(row["StatusName"]),
                    AttendanceDate = row["AttendanceDate"] == DBNull.Value ? null : Convert.ToString(row["AttendanceDate"]),
                    CheckInTime = row["CheckInTime"] == DBNull.Value ? null : Convert.ToString(row["CheckInTime"]),
                    CheckOutTime = row["CheckOutTime"] == DBNull.Value ? null : Convert.ToString(row["CheckOutTime"]),
                    StatusId = row["StatusId"] == DBNull.Value ? null : Convert.ToInt64(row["StatusId"])
                });
            }

            apiResponse.message = "Attendance fetched successfully!";
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
    [Route("SaveStaffAttendance")]
    public IActionResult SaveStaffAttendance([FromBody] SaveStaffAttendanceRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveStaffAttendance", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
                cmd.Parameters.AddWithValue("@AttendanceDate", (object)model.AttendanceDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CheckInTime", (object)model.CheckInTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CheckOutTime", (object)model.CheckOutTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StatusId", (object)model.StatusId ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0 ? "Attendance updated successfully!" : "Attendance inserted successfully!";
            apiResponse.data = new { AttendanceId = rowId };
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
    [Route("DeleteStaffAttendance")]
    public IActionResult DeleteStaffAttendance([FromBody] DeleteStaffAttendanceRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("DeleteStaffAttendance", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Attendance deleted successfully!";
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
