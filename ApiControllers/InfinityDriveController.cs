using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfinityDriveController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;
        private readonly ImageUploadService _imageUploadService;

        public InfinityDriveController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env, ImageUploadService imageUploadService) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _env = env;
            _imageUploadService = imageUploadService;
        }

        // =========================================================
        // 1. GET FILES - ALL / BY ID / BY CATEGORY
        // =========================================================
        [HttpPost]
        [Route("GetInfinityDrive_Files")]
        public IActionResult GetInfinityDrive_Files([FromBody] InfinityDriveFileRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<InfinityDriveFileModel> files = new List<InfinityDriveFileModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetInfinityDrive_Files", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);

                    con.Open();

                    using SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    files.Add(new InfinityDriveFileModel
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
                        CategoryId = row["CategoryId"] != DBNull.Value ? Convert.ToInt64(row["CategoryId"]) : 0,
                        CategoryName = row["CategoryName"] != DBNull.Value ? Convert.ToString(row["CategoryName"]) ?? "" : "",
                        DocumentName = row["DocumentName"] != DBNull.Value ? Convert.ToString(row["DocumentName"]) ?? "" : "",

                        DocumentFileName = row["DocumentFileName"] != DBNull.Value && !string.IsNullOrEmpty(row["DocumentFileName"].ToString())
                            ? $"{domainUrl}/uploads/org/{domain}/InfinityDrive/Documents/{row["DocumentFileName"]}"
                            : string.Empty,

                        SortOrder = row["SortOrder"] != DBNull.Value ? Convert.ToInt32(row["SortOrder"]) : 0
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Files fetched successfully!";
                apiResponse.data = files;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return StatusCode(500, apiResponse);
            }
        }


        // =========================================================
        // 2. SAVE FILE - INSERT / UPDATE
        // =========================================================
        [HttpPost]
        [Route("SaveInfinityDrive_Files")]
        public IActionResult SaveInfinityDrive_Files([FromBody] InfinityDriveFileSaveRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                #region VALIDATION

                if (model.Id == 0 && string.IsNullOrWhiteSpace(model.FileBase64))
                    return BadRequest(new apiResponse { status = "Error", message = "Please select a file." });

                if (!string.IsNullOrWhiteSpace(model.FileBase64))
                {
                    try
                    {
                        _imageUploadService.ValidateBase64File(
                            model.FileBase64,
                            new[] { ".pdf", ".jpg", ".jpeg", ".png" },
                            1
                        );
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new apiResponse
                        {
                            status = "Error",
                            message = ex.Message
                        });
                    }
                }

                #endregion

                long fileId;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    #region SAVE FILE MASTER

                    using (SqlCommand cmd = new SqlCommand("SaveInfinityDrive_Files", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", model.Id);
                        cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                        cmd.Parameters.AddWithValue("@DocumentName", (object)model.DocumentName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SortOrder", model.SortOrder);
                        cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                        fileId = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    #endregion

                    #region DOCUMENT UPLOAD

                    if (!string.IsNullOrWhiteSpace(model.FileBase64))
                    {
                        string fileType = _imageUploadService.GetFileTypeFromBase64(model.FileBase64);
                        string folderPath = $"org/{domain}/InfinityDrive/Documents/";

                        _imageUploadService.DeleteImages(
                            folderPath,
                            $"Document_{fileId}"
                        );

                        string fileName = _imageUploadService.UploadImage(
                            model.FileBase64,
                            folderPath,
                            $"Document_{fileId}",
                            fileType
                        );

                        using SqlCommand fileCmd = new SqlCommand("Save_DocumentFile", con);
                        fileCmd.CommandType = CommandType.StoredProcedure;
                        fileCmd.Parameters.AddWithValue("@Id", fileId);
                        fileCmd.Parameters.AddWithValue("@DocumentName", fileName);
                        fileCmd.Parameters.AddWithValue("@TableName", "InfinityDrive_Files");

                        fileCmd.ExecuteNonQuery();
                    }

                    #endregion
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Document updated successfully!"
                    : "Document uploaded successfully!";

                apiResponse.data = new { Id = fileId };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return StatusCode(500, apiResponse);
            }
        }

        // =========================================================
        // 3. DELETE FILE - SOFT DELETE
        // =========================================================
        [HttpPost]
        [Route("DeleteInfinityDrive_Files")]
        public IActionResult DeleteInfinityDrive_Files([FromBody] InfinityDriveFileDeleteRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long id = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteInfinityDrive_Files", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();

                    object? result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                        id = Convert.ToInt64(result);
                }

                apiResponse.status = "Success";
                apiResponse.message = "File deleted successfully!";
                apiResponse.data = new
                {
                    Id = id
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return StatusCode(500, apiResponse);
            }
        }

    }
}
