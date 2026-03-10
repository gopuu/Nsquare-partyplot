using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class TaskCategoryMasterController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public TaskCategoryMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // =======================================
    // 1. GET TASK CATEGORY MASTER
    // =======================================
    [HttpPost]
    [Route("GetTaskCategoryMaster")]
    public IActionResult GetTaskCategoryMaster([FromBody] GetTaskCategoryMasterRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<TaskCategoryMasterModel> list = new List<TaskCategoryMasterModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult? errorResult))
                return errorResult!;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("GetTaskCategoryMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TaskCategoryMasterModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    CategoryName = Convert.ToString(row["CategoryName"]),
                    PriorityNo = Convert.ToInt32(row["PriorityNo"])
                });
            }

            apiResponse.message = "Task category list fetched successfully!";
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



    // =======================================
    // 2. SAVE TASK CATEGORY MASTER
    // =======================================
    [HttpPost]
    [Route("SaveTaskCategoryMaster")]
    public IActionResult SaveTaskCategoryMaster([FromBody] SaveTaskCategoryMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult? errorResult))
                return errorResult!;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SaveTaskCategoryMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@CategoryName", (object)model.CategoryName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PriorityNo", model.PriorityNo);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0
                ? "Task category updated successfully!"
                : "Task category inserted successfully!";

            apiResponse.data = new { CategoryId = rowId };
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



    // =======================================
    // 3. DELETE TASK CATEGORY MASTER
    // =======================================
    [HttpPost]
    [Route("DeleteTaskCategoryMaster")]
    public IActionResult DeleteTaskCategoryMaster([FromBody] DeleteTaskCategoryMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult? errorResult))
                return errorResult!;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("DeleteTaskCategoryMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Task category deleted successfully!";
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
