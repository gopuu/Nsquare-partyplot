using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class DutyRoleMasterController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public DutyRoleMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // ===========================
    // 1. GET DUTY ROLE MASTER
    // ===========================
    [HttpPost]
    [Route("GetDutyRoleMaster")]
    public IActionResult GetDutyRoleMaster([FromBody] GetDutyRoleMasterRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<DutyRoleMasterModel> list = new List<DutyRoleMasterModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("GetDutyRoleMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new DutyRoleMasterModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    RoleName = Convert.ToString(row["RoleName"]),
                    PriorityNo = Convert.ToInt32(row["PriorityNo"])
                });
            }

            apiResponse.message = "Duty role list fetched successfully!";
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
    // 2. SAVE DUTY ROLE MASTER
    // ===========================
    [HttpPost]
    [Route("SaveDutyRoleMaster")]
    public IActionResult SaveDutyRoleMaster([FromBody] SaveDutyRoleMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SaveDutyRoleMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@RoleName", (object)model.RoleName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PriorityNo", model.PriorityNo);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0
                ? "Duty role updated successfully!"
                : "Duty role inserted successfully!";

            apiResponse.data = new { RoleId = rowId };
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
    // 3. DELETE DUTY ROLE MASTER
    // ===========================
    [HttpPost]
    [Route("DeleteDutyRoleMaster")]
    public IActionResult DeleteDutyRoleMaster([FromBody] DeleteDutyRoleMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("DeleteDutyRoleMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Duty role deleted successfully!";
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
