using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfinityDrive_MasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;
        private readonly ImageUploadService _imageUploadService;

        public InfinityDrive_MasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env, ImageUploadService imageUploadService) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _env = env;
            _imageUploadService = imageUploadService;
        }

        [HttpPost]
        [Route("GetInfinityDrive_Master")]
        public IActionResult GetInfinityDrive_Master([FromBody] InfinityDriveFolderRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<InfinityDriveFolderModel> folders = new List<InfinityDriveFolderModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetInfinityDrive_Master", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);

                    con.Open();

                    using SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    folders.Add(new InfinityDriveFolderModel
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
                        FolderName = row["FolderName"] != DBNull.Value ? Convert.ToString(row["FolderName"]) ?? "" : "",
                        ParentFolderId = row["ParentFolderId"] != DBNull.Value ? Convert.ToInt64(row["ParentFolderId"]) : 0,
                        ParentFolderName = row["ParentFolderName"] != DBNull.Value ? Convert.ToString(row["ParentFolderName"]) ?? "" : "",
                        SortOrder = row["SortOrder"] != DBNull.Value ? Convert.ToInt32(row["SortOrder"]) : 0
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Folders fetched successfully!";
                apiResponse.data = folders;

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

        /* ═══════════════════════════════════════════════════
           SAVE FOLDER - INSERT / UPDATE
        ═══════════════════════════════════════════════════ */
        [HttpPost]
        [Route("SaveInfinityDrive_Master")]
        public IActionResult SaveInfinityDrive_Master([FromBody] InfinityDriveFolderSaveRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                if (string.IsNullOrWhiteSpace(request.FolderName))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Folder name is required.";
                    return BadRequest(apiResponse);
                }

                long id = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveInfinityDrive_Master", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@FolderName", request.FolderName.Trim());
                    cmd.Parameters.AddWithValue("@ParentFolderId", request.ParentFolderId);
                    cmd.Parameters.AddWithValue("@SortOrder", request.SortOrder);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();

                    object? result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                        id = Convert.ToInt64(result);
                }

                apiResponse.status = "Success";
                apiResponse.message = request.Id == 0 ? "Folder created successfully!" : "Folder updated successfully!";
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

        /* ═══════════════════════════════════════════════════
           DELETE FOLDER - SOFT DELETE
        ═══════════════════════════════════════════════════ */
        [HttpPost]
        [Route("DeleteInfinityDrive_Master")]
        public IActionResult DeleteInfinityDrive_Master([FromBody] InfinityDriveFolderDeleteRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                if (request.Id <= 0)
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid folder.";
                    return BadRequest(apiResponse);
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteInfinityDrive_Master", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Folder deleted successfully!";

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
