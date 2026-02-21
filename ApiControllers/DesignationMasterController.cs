
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class DesignationMasterController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public DesignationMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // ====================== GET ======================
    [HttpPost]
    [Route("GetDesignationMaster")]
    public IActionResult GetDesignationMaster([FromBody] GetDesignationMasterRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<DesignationMasterModel> list = new List<DesignationMasterModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("GetDesignationMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new DesignationMasterModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    DesignationName = Convert.ToString(row["DesignationName"]),
                    LevelId = Convert.ToInt64(row["LevelId"]),
                    LevelName = Convert.ToString(row["LevelName"])
                });
            }

            apiResponse.message = "Designation list fetched successfully!";
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
    [Route("SaveDesignationMaster")]
    public IActionResult SaveDesignationMaster([FromBody] SaveDesignationMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveDesignationMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@DesignationName", (object)model.DesignationName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LevelId", model.LevelId);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0 ? "Designation updated successfully!" : "Designation inserted successfully!";
            apiResponse.data = new { DesignationId = rowId };
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
    [Route("DeleteDesignationMaster")]
    public IActionResult DeleteDesignationMaster([FromBody] DeleteDesignationMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("DeleteDesignationMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Designation deleted successfully!";
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
