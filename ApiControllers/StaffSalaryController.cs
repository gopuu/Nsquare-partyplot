
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class StaffSalaryController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public StaffSalaryController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // ====================== GET ======================
    [HttpPost]
    [Route("GetStaffSalary")]
    public IActionResult GetStaffSalary([FromBody] GetStaffSalaryRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<StaffSalaryModel> list = new List<StaffSalaryModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("GetStaffSalary", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);
              //  cmd.Parameters.AddWithValue("@StaffId", request.StaffId == 0 ? 0 : request.StaffId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new StaffSalaryModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    StaffId = Convert.ToInt64(row["StaffId"]),
                    StaffName = Convert.ToString(row["StaffName"]),
                    SalaryMonth = Convert.ToString(row["SalaryMonth"]),
                    BasicSalary = row["BasicSalary"] == DBNull.Value ? 0 : Convert.ToDecimal(row["BasicSalary"]),
                    Deductions = row["Deductions"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Deductions"]),
                    NetSalary = row["NetSalary"] == DBNull.Value ? 0 : Convert.ToDecimal(row["NetSalary"]),
                    StatusId = row["StatusId"] == DBNull.Value ? null : Convert.ToInt64(row["StatusId"]),
                    StatusName = Convert.ToString(row["StatusName"]),
                    PaidDate = row["PaidDate"] == DBNull.Value ? null : Convert.ToString(row["PaidDate"])
                });
            }

            apiResponse.message = "Salary list fetched successfully!";
            apiResponse.data = list;
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

    // ====================== SAVE ======================
    [HttpPost]
    [Route("SaveStaffSalary")]
    public IActionResult SaveStaffSalary([FromBody] SaveStaffSalaryRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveStaffSalary", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
                cmd.Parameters.AddWithValue("@SalaryMonth", (object)model.SalaryMonth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BasicSalary", model.BasicSalary);
                cmd.Parameters.AddWithValue("@Deductions", model.Deductions);
                cmd.Parameters.AddWithValue("@StatusId", (object)model.StatusId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PaidDate", (object)model.PaidDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0 ? "Salary updated successfully!" : "Salary inserted successfully!";
            apiResponse.data = new { SalaryId = rowId };
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
    [Route("DeleteStaffSalary")]
    public IActionResult DeleteStaffSalary([FromBody] DeleteStaffSalaryRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("DeleteStaffSalary", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Salary deleted successfully!";
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
}
