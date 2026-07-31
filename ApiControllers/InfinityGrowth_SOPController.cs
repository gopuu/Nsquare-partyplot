using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Web.Helpers;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfinityGrowth_SOPController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;
        public InfinityGrowth_SOPController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _env = env;

        }

        [HttpPost]
        [Route("GetInfinityGrowth_SOP_Documents")]
        public IActionResult GetInfinityGrowth_SOP_Documents([FromBody] InfinityGrowth_SOPDocumentRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<InfinityGrowth_SOPDocumentModel> documents = new List<InfinityGrowth_SOPDocumentModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                if (request.ActionBy <= 0)
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid employee.";
                    return BadRequest(apiResponse);
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetInfinityGrowth_SOP_Documents", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeId", request.ActionBy);

                    con.Open();

                    using SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    documents.Add(new InfinityGrowth_SOPDocumentModel
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
                        Title = row["Title"] != DBNull.Value ? Convert.ToString(row["Title"]) ?? "" : "",
                        FileName = row["FileName"] != DBNull.Value ? Convert.ToString(row["FileName"]) ?? "" : "",
                        CategoryId = row["CategoryId"] != DBNull.Value ? Convert.ToInt64(row["CategoryId"]) : 0,
                        CategoryName = row["CategoryName"] != DBNull.Value ? Convert.ToString(row["CategoryName"]) ?? "" : "",
                        ParentCategoryId = row["ParentCategoryId"] != DBNull.Value ? Convert.ToInt64(row["ParentCategoryId"]) : 0,
                        ParentCategoryName = row["ParentCategoryName"] != DBNull.Value ? Convert.ToString(row["ParentCategoryName"]) ?? "" : "",
                        SortOrder = row["SortOrder"] != DBNull.Value ? Convert.ToInt32(row["SortOrder"]) : 0
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "SOP documents fetched successfully!";
                apiResponse.data = documents;

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

        [HttpPost]
        [Route("GetInfinityGrowth_SOP_DocumentsForView")]
        public IActionResult GetInfinityGrowth_SOP_DocumentsForView([FromBody] InfinityGrowth_SOPLogRequest request)
        {
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                if (request.DocumentId <= 0 || request.ActionBy <= 0)
                    return BadRequest("Invalid request.");

                InfinityGrowth_SOPDocumentViewModel? document = null;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetInfinityGrowth_SOP_DocumentsForView", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DocumentId", request.DocumentId);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();

                    using SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        document = new InfinityGrowth_SOPDocumentViewModel
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            Title = dr["Title"] != DBNull.Value ? Convert.ToString(dr["Title"]) ?? "" : "",
                            FileName = dr["FileName"] != DBNull.Value ? Convert.ToString(dr["FileName"]) ?? "" : "",
                            RelativeFilePath = dr["RelativeFilePath"] != DBNull.Value ? Convert.ToString(dr["RelativeFilePath"]) ?? "" : ""
                        };
                    }
                }

                if (document == null)
                    return Forbid();

                string rootPath = GetSOPRootPath();
                string rootFullPath = Path.GetFullPath(rootPath);
                string filePath = Path.GetFullPath(Path.Combine(rootFullPath, document.RelativeFilePath));

                if (!filePath.StartsWith(rootFullPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                if (!System.IO.File.Exists(filePath))
                    return NotFound("PDF file not found.");

                LogDocumentView(request.DocumentId, request.ActionBy, connString);

                FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                Response.Headers["Content-Disposition"] = $"inline; filename=\"{document.FileName}\"";
                Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";
                Response.Headers["X-Content-Type-Options"] = "nosniff";

                return File(stream, "application/pdf", enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                return StatusCode(500, ex.Message);
            }
        }

        private void LogDocumentView(long documentId, long employeeId, string connString)
        {
            try
            {
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                string userAgent = Request.Headers["User-Agent"].ToString();

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("SaveInfinityGrowth_SOP_LogView", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                cmd.Parameters.AddWithValue("@IPAddress", string.IsNullOrWhiteSpace(ipAddress) ? DBNull.Value : ipAddress);
                cmd.Parameters.AddWithValue("@UserAgent", string.IsNullOrWhiteSpace(userAgent) ? DBNull.Value : userAgent);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
            }
        }

        private string GetSOPRootPath()
        {
            string configuredPath = _configuration["SOPSettings:RootPath"] ?? "InfinityERP_PrivateFiles";

            if (Path.IsPathRooted(configuredPath))
                return configuredPath;

            return Path.Combine(_env.ContentRootPath, configuredPath);
        }
    }
}
