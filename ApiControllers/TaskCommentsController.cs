using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class TaskCommentsController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public TaskCommentsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }


    // =======================================
    // 1. GET TASK COMMENTS
    // =======================================
    [HttpPost]
    [Route("GetTaskComments")]
    public IActionResult GetTaskComments([FromBody] GetTaskCommentsRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<TaskCommentsModel> list = new List<TaskCommentsModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult? errorResult))
                return errorResult!;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("GetTaskComments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TaskId", request.TaskId == 0 ? 0 : request.TaskId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TaskCommentsModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    TaskId = Convert.ToInt64(row["TaskId"]),
                    CommentText = Convert.ToString(row["CommentText"]),
                    CommentedAt = Convert.ToString(row["CommentedAt"]),
                    TaskTitle = Convert.ToString(row["TaskTitle"])
                });
            }

            apiResponse.message = "Task comments fetched successfully!";
            apiResponse.status = "Success";
            apiResponse.data = list;
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



    // =======================================
    // 2. SAVE TASK COMMENTS
    // =======================================
    [HttpPost]
    [Route("SaveTaskComments")]
    public IActionResult SaveTaskComments([FromBody] SaveTaskCommentsRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult? errorResult))
                return errorResult!;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SaveTaskComments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@TaskId", model.TaskId);
                cmd.Parameters.AddWithValue("@CommentText", (object)model.CommentText ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0
                ? "Task comment updated successfully!"
                : "Task comment added successfully!";

            apiResponse.data = new { CommentId = rowId };
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



    // =======================================
    // 3. DELETE TASK COMMENT
    // =======================================
    [HttpPost]
    [Route("DeleteTaskComments")]
    public IActionResult DeleteTaskComments([FromBody] DeleteTaskCommentsRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult? errorResult))
                return errorResult!;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("DeleteTaskComments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Task comment deleted successfully!";
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
