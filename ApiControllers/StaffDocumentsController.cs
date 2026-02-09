
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class StaffDocumentsController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;
    private readonly ImageUploadService _imageUploadService;
    public StaffDocumentsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService)
        : base(configuration, httpContextAccessor)
    {
        _imageUploadService = imageUploadService;
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        _imageUploadService = imageUploadService;
    }
    /*
    // ====================== GET ======================
    [HttpPost]
    [Route("GetStaffDocuments")]
    private IActionResult GetStaffDocuments([FromBody] GetStaffDocumentsRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<StaffDocumentsModel> list = new List<StaffDocumentsModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("GetStaffDocuments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new StaffDocumentsModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    StaffId = Convert.ToInt64(row["StaffId"]),
                    StaffName = Convert.ToString(row["StaffName"]),
                    DocumentName = Convert.ToString(row["DocumentName"]),
                    //  FilePath = Convert.ToString(row["FilePath"]),
                    FilePath = row["FilePath"] != DBNull.Value
                                && !string.IsNullOrEmpty(row["FilePath"].ToString())
                            ? domainUrl + "/uploads/org/" + domain + "/StaffMaster/Documents/" + row["FilePath"].ToString()
                            : string.Empty,
                });
            }

            apiResponse.message = "Documents fetched successfully!";
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
    [Route("SaveStaffDocuments")]
    private IActionResult SaveStaffDocuments([FromBody] SaveStaffDocumentsRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveStaffDocuments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
                cmd.Parameters.AddWithValue("@DocumentName", (object)model.DocumentName ?? DBNull.Value);
                cmd.Parameters.Add("@FilePath", SqlDbType.NVarChar).Value = DBNull.Value;
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
                string latestImagePath = string.Empty;
                //photo upload
                if (model.FilePath != string.Empty && model.FilePath != null)
                {
                    string imgType = _imageUploadService.GetFileTypeFromBase64(model.FilePath);

                    string folderPath = $"org/{domain}/StaffMaster/Documents/"; // Updated to include domain
                    _imageUploadService.DeleteImages(folderPath, $"StaffId_{rowId.ToString()}");
                    model.FilePath = _imageUploadService.UploadImage(model.FilePath, folderPath, $"StaffId_{rowId.ToString()}", imgType);

                    //update table
                    SqlCommand cmd2 = new SqlCommand("Save_DocumentFile", con);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.Add("@Id", SqlDbType.BigInt).Value = rowId;
                    cmd2.Parameters.Add("@DocumentName", SqlDbType.NVarChar, -1).Value = model.FilePath; // Kept max for nvarchar(max)
                    cmd2.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "StaffDocuments";
                    cmd2.ExecuteNonQuery();

                    latestImagePath = $"{domainUrl}/uploads/org/{domain}/StaffMaster/Documents/{model.FilePath}";
                }
            }

            apiResponse.message = model.Id > 0 ? "Document updated successfully!" : "Document inserted successfully!";
            apiResponse.data = new { DocumentId = rowId };
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
    */
    // ====================== DELETE ======================
    [HttpPost]
    [Route("DeleteStaffDocuments")]
    public IActionResult DeleteStaffDocuments([FromBody] DeleteStaffDocumentsRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("DeleteStaffDocuments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Document deleted successfully!";
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
