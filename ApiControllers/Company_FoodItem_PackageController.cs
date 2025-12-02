using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using ExcelDataReader.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.Json;
using System.Xml.Linq;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Company_FoodItem_PackageController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_FoodItem_PackageController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
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
        [Route("DeleteCompanyFoodPackage")]
        public IActionResult DeleteCompanyFoodPackage([FromBody] DeleteCompanyFoodPackageRequest model)
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
                    using (SqlCommand cmd = new SqlCommand("DeleteCompanyFoodPackage", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = model.ActionBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                apiResponse.message = "Food package deleted successfully!";
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
        [Route("GetCompanyFoodPackage")]
        public IActionResult GetCompanyFoodPackage([FromBody] GetCompanyFoodPackageRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            //List<dynamic> packages = new List<dynamic>();
            List<FoodPackageModel> packages = new List<FoodPackageModel>();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyFoodPackage", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? 0;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        FoodPackageModel package = new FoodPackageModel();
                        package.Id = Convert.ToInt64(reader["id"]);
                        package.PackageName = Convert.ToString(reader["package_name"]);
                        package.PackageType = Convert.ToString(reader["package_type"]);
                        package.RatePerPerson = Convert.ToDecimal(reader["rate_per_person"]);
                        package.MenuImagePath = reader["menu_image_path"] != DBNull.Value
                                    && !string.IsNullOrEmpty(reader["menu_image_path"].ToString())
                                ? domainUrl + $"/uploads/org/{domain}/Company_FoodItem_Package_Images/" + reader["menu_image_path"].ToString()
                                : string.Empty;
                        //InclusionList = Convert.ToString(reader["inclusions"]),
                        // Save into model
                        //package.InclusionList = inclusions;
                        string inclusionsRaw = Convert.ToString(reader["inclusions"]);
                        if (!string.IsNullOrEmpty(inclusionsRaw))
                        {
                            // Deserialize JSON to List<InclusionItem>
                            package.InclusionList = System.Text.Json.JsonSerializer.Deserialize<List<InclusionItem>>(inclusionsRaw, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new List<InclusionItem>();
                        }
                        packages.Add(package);
                    }
                    con.Close();
                }

                if (packages.Count > 0)
                {
                    apiResponse.message = "Food package(s) fetched successfully!";
                    apiResponse.data = packages;
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "No data found.";
                    apiResponse.data = null;
                    apiResponse.status = "Success";
                }

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
        [Route("SaveCompanyFoodPackage")]
        public IActionResult SaveCompanyFoodPackage([FromBody] List<SaveCompanyFoodPackageRequest> modelList)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    foreach (var model in modelList) 
                    { 
                        SqlCommand cmd = new SqlCommand("SaveCompanyFoodPackage", con);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@package_type", SqlDbType.NVarChar, 20).Value = string.IsNullOrEmpty(model.PackageType) ? (object)DBNull.Value : model.PackageType;
                        cmd.Parameters.Add("@package_name", SqlDbType.NVarChar, 200).Value = model.PackageName ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@rate_per_person", SqlDbType.Decimal).Value = model.RatePerPerson;
                        cmd.Parameters.Add("@menu_image_path", SqlDbType.NVarChar, 500).Value = (object)DBNull.Value; // : model.MenuImagePath;
                        cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;
                        //cmd.Parameters.Add("@inclusionList", SqlDbType.NVarChar).Value = model.InclusionList;

                        // ✅ Serialize InclusionList to JSON string
                        var inclusionJson = (model.InclusionList != null && model.InclusionList.Any())
                            ? Newtonsoft.Json.JsonConvert.SerializeObject(model.InclusionList)
                            : null;
                        cmd.Parameters.Add("@inclusionList", SqlDbType.NVarChar).Value =
                            string.IsNullOrEmpty(inclusionJson) ? (object)DBNull.Value : inclusionJson;

                        //cmd.ExecuteNonQuery();
                        Int64 rowid = Convert.ToInt64(cmd.ExecuteScalar());
                        // ✅ Handle single image (MenuImagePath)
                        if (!string.IsNullOrEmpty(model.MenuImagePath))
                        {
                            string fileType = _imageUploadService.GetFileTypeFromBase64(model.MenuImagePath);
                            string folderPath = $"/org/{domain}/Company_FoodItem_Package_Images"; // You can rename folder if needed

                            // Delete old image before uploading new one
                            _imageUploadService.DeleteImages(folderPath, rowid.ToString() + "_foodItemPackage");

                            // Upload new image
                            string uploadedFileName = _imageUploadService.UploadImage(
                                model.MenuImagePath,
                                folderPath,
                                rowid.ToString() + "_foodItemPackage",
                                fileType
                            );

                            // Save uploaded file name back to model
                            model.MenuImagePath = uploadedFileName;


                            // Update all media paths in database using the stored procedure
                            using (SqlCommand mediaCmd = new SqlCommand("SaveCompanyFoodPackageImage", con))
                            {
                                mediaCmd.CommandType = CommandType.StoredProcedure;
                                mediaCmd.Parameters.Add("@id", SqlDbType.BigInt).Value = rowid;
                                mediaCmd.Parameters.Add("@menu_image_path", SqlDbType.VarChar, 500).Value = (object)model.MenuImagePath ?? DBNull.Value;

                                mediaCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    con.Close();
                }

                apiResponse.message = "Food package saved successfully!";
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
        [Route("GetCompanyFoodPackage_Dropdown")]
        public IActionResult GetCompanyFoodPackage_Dropdown([FromBody] GetCompanyFoodPackageRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            //List<dynamic> packages = new List<dynamic>();
            List<FoodPackageModel_Dropdown> packages = new List<FoodPackageModel_Dropdown>();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyFoodPackage", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? 0;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        FoodPackageModel_Dropdown package = new FoodPackageModel_Dropdown();
                        package.Id = Convert.ToInt64(reader["id"]);
                        package.PackageName = Convert.ToString(reader["package_name"]);
                        package.PackageType = Convert.ToString(reader["package_type"]);
                        package.RatePerPerson = Convert.ToDecimal(reader["rate_per_person"]);
                        packages.Add(package);
                    }
                    con.Close();
                }

                if (packages.Count > 0)
                {
                    apiResponse.message = "Food package(s) fetched successfully!";
                    apiResponse.data = packages;
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "No data found.";
                    apiResponse.data = null;
                    apiResponse.status = "Success";
                }

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
public class FoodPackageModel_Dropdown
{
    public long? Id { get; set; }
    public string? PackageName { get; set; }
    public string? PackageType { get; set; }   // Veg / Non-Veg
    public decimal? RatePerPerson { get; set; }
}
public class FoodPackageModel
{
    public long? Id { get; set; }
    public string? PackageName { get; set; }
    public string? PackageType { get; set; }   // Veg / Non-Veg
    public decimal? RatePerPerson { get; set; }
    public string? MenuImagePath { get; set; }
    public List<InclusionItem>? InclusionList { get; set; } = new List<InclusionItem>();
}

public class SaveCompanyFoodPackageRequest : BaseClass
{
    public long? Id { get; set; }
    public string? PackageType { get; set; }
    public string? PackageName { get; set; }
    public decimal RatePerPerson { get; set; }
    public string? MenuImagePath { get; set; }
    public List<InclusionItem>? InclusionList { get; set; }
}
public class InclusionItem
{
    public long FoodItem_Id { get; set; }
    public bool IsIncluded { get; set; }
    public long Count_Size { get; set; }
}

public class GetCompanyFoodPackageRequest
{
    public long? Id { get; set; }
}

public class DeleteCompanyFoodPackageRequest : BaseClass
{
    public long Id { get; set; }
}