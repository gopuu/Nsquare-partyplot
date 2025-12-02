using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using System.Xml.Linq;
using ExcelDataReader.Log;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Company_FoodRate_LabelPrintingController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_FoodRate_LabelPrintingController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";

            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("DeleteCompanyFoodRateLabelPrinting")]
        public IActionResult DeleteCompanyFoodRateLabelPrinting([FromBody] DeleteCompanyFoodRateLabelPrintingRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteCompanyFoodRateLabelPrinting", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = model.ActionBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                apiResponse.message = "Label printing record deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetCompanyFoodRateLabelPrinting")]
        public IActionResult GetCompanyFoodRateLabelPrinting([FromBody] GetCompanyFoodRateLabelPrintingRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            dynamic labelPrinting = null;

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyFoodRateLabelPrinting", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? (object)DBNull.Value;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read()) // ✅ Only first row
                    {
                        labelPrinting = new
                        {
                            Id = Convert.ToInt64(reader["id"]),
                            FontName = Convert.ToString(reader["font_name"]),
                            FontSize = Convert.ToString(reader["font_size"]),
                            //PreviewImagePath = Convert.ToString(reader["preview_image_path"]),
                            PlaqueSizeInches = Convert.ToString(reader["plaque_size_inches"]),
                            PlaqueSizeCm = Convert.ToString(reader["plaque_size_cm"]),
                            PlaqueSizeMm = Convert.ToString(reader["plaque_size_mm"])
                        };
                    }
                    con.Close();
                }

                if (labelPrinting != null)
                {
                    apiResponse.message = "Label printing data fetched successfully!";
                    apiResponse.data = labelPrinting; // ✅ single object, not list
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "No label printing data found.";
                    apiResponse.data = null;
                    apiResponse.status = "Fail";
                }

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyFoodRateLabelPrinting")]
        public IActionResult SaveCompanyFoodRateLabelPrinting([FromBody] SaveCompanyFoodRateLabelPrintingRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SaveCompanyFoodRateLabelPrinting", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@font_name", SqlDbType.NVarChar, 100).Value = model.FontName;
                    cmd.Parameters.Add("@font_size", SqlDbType.Int).Value = model.FontSize;
                    //cmd.Parameters.Add("@preview_image_path", SqlDbType.NVarChar, 500).Value = (object)DBNull.Value;
                    cmd.Parameters.Add("@plaque_size_inches", SqlDbType.NVarChar, 50).Value = model.PlaqueSizeInches ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@plaque_size_cm", SqlDbType.NVarChar, 50).Value = model.PlaqueSizeCm ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@plaque_size_mm", SqlDbType.NVarChar, 50).Value = model.PlaqueSizeMm ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    //cmd.ExecuteNonQuery();
                    // First execute the main command to get rowid
                    Int64 rowid = Convert.ToInt64(cmd.ExecuteScalar());
                    /*
                    if (!string.IsNullOrEmpty(model.PreviewImagePath))
                    {
                        bool isBase64 = model.PreviewImagePath.Contains("base64,");

                        if (isBase64)
                        {
                            string fileType = _imageUploadService.GetFileTypeFromBase64(model.PreviewImagePath);
                            string folderPath = $"/org/{domain}/Company_FoodItem_LabelPrint_Images"; // You can rename folder if needed

                            // Delete old image before uploading new one
                            _imageUploadService.DeleteImages(folderPath, rowid.ToString() + "_labelPrintImage");

                            // Upload new image
                            string uploadedFileName = _imageUploadService.UploadImage(
                                model.PreviewImagePath,
                                folderPath,
                                rowid.ToString() + "_labelPrintImage",
                                fileType
                            );

                            // Save uploaded file name back to model
                            model.PreviewImagePath = uploadedFileName;
                        }
                        else
                        {
                            // Path already provided (static or pre-uploaded), just keep it as is
                            model.PreviewImagePath = model.PreviewImagePath;
                        }

                        // Update in database
                        using (SqlCommand mediaCmd = new SqlCommand("SaveCompanyFoodRateLabelPrinting_Image", con))
                        {
                            mediaCmd.CommandType = CommandType.StoredProcedure;
                            mediaCmd.Parameters.Add("@id", SqlDbType.BigInt).Value = rowid;
                            mediaCmd.Parameters.Add("@preview_image_path", SqlDbType.VarChar, 500).Value = (object)model.PreviewImagePath ?? DBNull.Value;

                            mediaCmd.ExecuteNonQuery();
                        }
                    }
                    */
                    con.Close();
                }

                apiResponse.message = model.Id.HasValue && model.Id > 0
                    ? "Label printing updated successfully!"
                    : "Label printing added successfully!";
                apiResponse.status = "Success";

                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

    }
}
public class DeleteCompanyFoodRateLabelPrintingRequest : BaseClass
{
    public long Id { get; set; }
}
public class GetCompanyFoodRateLabelPrintingRequest
{
    public long? Id { get; set; }
}
public class SaveCompanyFoodRateLabelPrintingRequest : BaseClass
{
    public long? Id { get; set; }
    public string? FontName { get; set; }
    public int? FontSize { get; set; }
    //public string? PreviewImagePath { get; set; }
    public string? PlaqueSizeInches { get; set; }
    public string? PlaqueSizeCm { get; set; }
    public string? PlaqueSizeMm { get; set; }
}


