using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class TaskMasterController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;

    public TaskMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // =======================================
    // 1. GET TASK MASTER
    // =======================================
    [HttpPost]
    [Route("GetTaskMaster")]
    public IActionResult GetTaskMaster([FromBody] GetTaskMasterRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<TaskMasterModel> list = new List<TaskMasterModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("GetTaskMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);
                cmd.Parameters.AddWithValue("@AssignedTo", request.AssignedTo == 0 ? 0 : request.AssignedTo);
                cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId == 0 ? 0 : request.CategoryId);
                cmd.Parameters.AddWithValue("@StatusId", request.StatusId == 0 ? 0 : request.StatusId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TaskMasterModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    TaskTitle = Convert.ToString(row["TaskTitle"]),
                    TaskDescription = Convert.ToString(row["TaskDescription"]),

                    CategoryId = row["CategoryId"] == DBNull.Value ? null : (long?)Convert.ToInt64(row["CategoryId"]),
                    AssignedTo = row["AssignedTo"] == DBNull.Value ? null : (long?)Convert.ToInt64(row["AssignedTo"]),
                    AssignedBy = row["AssignedBy"] == DBNull.Value ? null : (long?)Convert.ToInt64(row["AssignedBy"]),

                    PriorityLevel = row["PriorityLevel"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["PriorityLevel"]),

                    StartDate = row["StartDate"] == DBNull.Value ? null : Convert.ToString(row["StartDate"]),
                    DueDate = row["DueDate"] == DBNull.Value ? null : Convert.ToString(row["DueDate"]),
                    CompletedDate = row["CompletedDate"] == DBNull.Value ? null : Convert.ToString(row["CompletedDate"]),

                    StatusId = row["StatusId"] == DBNull.Value ? null : (long?)Convert.ToInt64(row["StatusId"])
                });
            }

            apiResponse.message = "Task list fetched successfully!";
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



    // =======================================
    // 2. SAVE TASK MASTER
    // =======================================
    [HttpPost]
    [Route("SaveTaskMaster")]
    public IActionResult SaveTaskMaster([FromBody] SaveTaskMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SaveTaskMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@TaskTitle", (object)model.TaskTitle ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TaskDescription", (object)model.TaskDescription ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@CategoryId", (object)model.CategoryId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AssignedTo", (object)model.AssignedTo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AssignedBy", (object)model.AssignedBy ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@PriorityLevel", (object)model.PriorityLevel ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@StartDate", (object)model.StartDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DueDate", (object)model.DueDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CompletedDate", (object)model.CompletedDate ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@StatusId", (object)model.StatusId ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            apiResponse.message = model.Id > 0
                ? "Task updated successfully!"
                : "Task added successfully!";

            apiResponse.data = new { TaskId = rowId };
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
    // 3. DELETE TASK MASTER
    // =======================================
    [HttpPost]
    [Route("DeleteTaskMaster")]
    public IActionResult DeleteTaskMaster([FromBody] DeleteTaskMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("DeleteTaskMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Task deleted successfully!";
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


    [HttpPost]
    [Route("GetPriorityLevels")]
    public IActionResult GetPriorityLevels([FromBody] GetPriorityLevelsRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<PriorityLevelModel> list = new List<PriorityLevelModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connStr, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("GetPriorityLevels", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Only pass Id if required (kept for consistency)
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PriorityLevelModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    PriorityLevel = Convert.ToInt32(row["PriorityLevel"]),
                    PriorityName = Convert.ToString(row["PriorityName"])
                });
            }

            apiResponse.message = "Priority levels fetched successfully!";
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


}
