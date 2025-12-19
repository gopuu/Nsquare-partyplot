
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

                cmd.Parameters.AddWithValue("@StaffId", request.StaffId == 0 ? (object)DBNull.Value : request.StaffId);
                cmd.Parameters.AddWithValue("@StatusId", request.StatusId == 0 ? (object)DBNull.Value : request.StatusId);
                cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new StaffAttendanceModel
                {
                    AttendanceId = row["AttendanceId"] == DBNull.Value ? 0 : Convert.ToInt64(row["AttendanceId"]),

                    StaffId = Convert.ToInt64(row["StaffId"]),
                    StaffName = Convert.ToString(row["StaffName"]),
                    AttendanceDate = row["AttendanceDate"] == DBNull.Value ? null : Convert.ToDateTime(row["AttendanceDate"]).ToString("yyyy-MM-dd"),
                    AttendanceDateFormat = row["AttendanceDateFormat"] == DBNull.Value ? null : Convert.ToString(row["AttendanceDateFormat"]),
                    DayName = Convert.ToString(row["DayName"]),

                    CheckInTime = row["CheckInTime"] == DBNull.Value ? "" : Convert.ToString(row["CheckInTime"]),
                    CheckOutTime = row["CheckOutTime"] == DBNull.Value ? "" : Convert.ToString(row["CheckOutTime"]),

                    StatusId = row["StatusId"] == DBNull.Value ? 0 : Convert.ToInt64(row["StatusId"]),
                    StatusName = Convert.ToString(row["StatusName"])
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

            long resultStatus = 0;

            // Convert attendance list to JSON string
            string jsonAttendance = JsonConvert.SerializeObject(model.AttendanceList ?? new List<StaffAttendanceItem>());

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveStaffAttendance", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@AttendanceList", jsonAttendance);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                resultStatus = Convert.ToInt64(cmd.ExecuteScalar() ?? 1);
            }

            apiResponse.message = "Attendance Saved!";
            apiResponse.data = null; // new { Status = resultStatus };
            apiResponse.status = "Success";
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogException(ex, ControllerContext, Request, null);

            apiResponse.message = ex.Message;
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
