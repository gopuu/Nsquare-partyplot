
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
    public StaffDocumentsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, 
        ImageUploadService imageUploadService)
        : base(configuration, httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        _imageUploadService = imageUploadService;
    }

    // ====================== DELETE ======================
    [HttpPost]
    [Route("DeleteStaffDocuments")]
    public IActionResult DeleteStaffDocuments([FromBody] DeleteStaffDocumentsRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

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
            apiResponse.message = ex.Message;
            apiResponse.status = "Error";
            return StatusCode(500, apiResponse);
        }
    }

}
