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
    public class CompanyController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public CompanyController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";

            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpGet]
        [Route("GetCompanyList")]
        public IActionResult GetCompanyList()
        {
            apiResponse apiResponse = new apiResponse();            
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }
                SqlConnection con = new SqlConnection(connString);
                //DataTable dataTable = new DataTable();
                SqlCommand cmd = new SqlCommand("GetCompanyList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                //dataTable.Load(sqlDataReader);
                Company company = new Company();
                //List<CompanyImage> imageList = new List<CompanyImage>();

                // === First result set: Company
                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        company = new Company();
                        company.Company_Id = Convert.ToInt64(sqlDataReader["Company_Id"]);
                        company.Name = Convert.ToString(sqlDataReader["Name"]);
                        company.ContactName = Convert.ToString(sqlDataReader["ContactName"]);
                        company.Contact = Convert.ToString(sqlDataReader["Contact"]);
                        company.Email = Convert.ToString(sqlDataReader["Email"]);
                        company.Address = Convert.ToString(sqlDataReader["Address"]);
                        company.GSTNo = Convert.ToString(sqlDataReader["GSTNo"]);
                        company.PANNo = Convert.ToString(sqlDataReader["PANNo"]);
                        company.StateCode = Convert.ToString(sqlDataReader["StateCode"]);
                        company.OwnerName = Convert.ToString(sqlDataReader["OwnerName"]);
                        company.FirmType = Convert.ToString(sqlDataReader["FirmType"]);
                        company.ContactNo2 = Convert.ToString(sqlDataReader["ContactNo2"]);

                        var logo = sqlDataReader["Logo"]?.ToString();
                        company.Logo = string.IsNullOrWhiteSpace(logo) || !System.IO.File.Exists(Path.Combine("wwwroot", "uploads", "org", domain, "companyPic", logo))
                            ? string.Empty
                            : $"{domainUrl}/uploads/org/{domain}/companyPic/{logo}";
                    }
                }

                // === Second result set: Flow & Project Images
                /*
                if (sqlDataReader.NextResult())
                {
                    while (sqlDataReader.Read())
                    {
                        CompanyImage img = new CompanyImage();
                        img.ImageId = Convert.ToInt64(sqlDataReader["CompanyImageId"]);
                        img.ImageType = Convert.ToString(sqlDataReader["ImageType"]);
                        img.ImagePath = Convert.ToString(sqlDataReader["ImageName"]);
                        img.ImageInfo = Convert.ToString(sqlDataReader["ImageInfo"]);

                        // Optional: prepend full image URL if needed
                        if (!string.IsNullOrWhiteSpace(img.ImagePath))
                        {
                            img.ImagePath = $"{domainUrl}/uploads/org/{domain}/companyPic/FlowAndProjectImages/{img.ImagePath}";
                        }
                        imageList.Add(img);
                    }
                }
                */
                sqlDataReader.Close();
                con.Close();
                // === Add images to company model
                if (company != null)
                {
                    //company.FlowAndProjectImages = imageList;
                    apiResponse.data = company;
                    apiResponse.message = "Company fetched successfully!";
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "No company data found.";
                    apiResponse.status = "Fail";
                    apiResponse.data = null;
                }
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompany")]
        public IActionResult SaveCompany([FromBody] CompanySave model)
        {
            apiResponse apiResponse = new apiResponse();            
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                SqlConnection con = new SqlConnection(connString);

                SqlCommand cmd = new SqlCommand("SaveCompany", con);
                cmd.CommandType = CommandType.StoredProcedure;
                // Retrieve and assign parameters
                cmd.Parameters.Add("@Company_Id", SqlDbType.BigInt).Value = model.Company_Id > 0 ? model.Company_Id : 0;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = model.Name ?? string.Empty;
                cmd.Parameters.Add("@ContactName", SqlDbType.NVarChar, 100).Value = model.ContactName ?? string.Empty;
                cmd.Parameters.Add("@Contact", SqlDbType.NVarChar, 15).Value = model.Contact ?? string.Empty;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = model.Email ?? string.Empty;
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = model.Address ?? string.Empty;
                cmd.Parameters.Add("@GSTNo", SqlDbType.NVarChar, 30).Value = model.GSTNo ?? string.Empty;
                cmd.Parameters.Add("@PANNo", SqlDbType.NVarChar, 20).Value = model.PANNo ?? string.Empty;
                cmd.Parameters.Add("@StateCode", SqlDbType.NVarChar, 20).Value = model.StateCode ?? string.Empty;
                cmd.Parameters.Add("@Logo", SqlDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@OwnerName", SqlDbType.NVarChar, 50).Value = model.OwnerName ?? string.Empty;
                cmd.Parameters.Add("@FirmType", SqlDbType.NVarChar, 50).Value = model.FirmType ?? string.Empty;
                cmd.Parameters.Add("@ContactNo2", SqlDbType.NVarChar, 15).Value = model.ContactNo2 ?? string.Empty;
                cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                con.Open();
                cmd.ExecuteNonQuery();

                cmd.CommandText = "select @@IDENTITY;";
                cmd.CommandType = CommandType.Text;

                Int64 rowid = model.Company_Id > 0 ? model.Company_Id : Convert.ToInt64(cmd.ExecuteScalar());
                //company photo upload
                string folderPath = $"org/{domain}/companyPic";

                // Upload and save company logo
                UploadAndSaveImage(model.Logo ?? "", folderPath, $"{rowid}_Logo", "SaveCompanyPhoto", "Logo", con, rowid);

                /*
                if (!string.IsNullOrEmpty(model.Logo))
                {
                    // Check file extension
                    string imgType = _imageUploadService.GetFileTypeFromBase64(model.Logo);

                    // Define folder path (including domain)
                    string folderPath = $"org/{domain}/companyPic";
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fileNamePrefix = $"{rowid}_{timestamp}";
                    _imageUploadService.DeleteImages(folderPath, rowid.ToString());
                    // Upload new image
                    model.Logo = _imageUploadService.UploadImage(model.Logo, folderPath, fileNamePrefix, imgType);

                    // Update table in database
                    using (SqlCommand cmd2 = new SqlCommand("SaveCompanyPhoto", con))
                    {
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@Company_Id", SqlDbType.BigInt).Value = rowid;
                        cmd2.Parameters.Add("@Logo", SqlDbType.NVarChar, -1).Value = model.Logo; // Keeping nvarchar(max)
                        cmd2.ExecuteNonQuery();
                    }
                }


                //company esign upload
                if (model.Esign != string.Empty && model.Esign != null)
                {
                    // Check file extension
                    string imgType = _imageUploadService.GetFileTypeFromBase64(model.Esign);

                    string folderPath = $"org/{domain}/companyPic"; // Updated to include domain
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fileNamePrefix = $"Esign_{rowid}_{timestamp}";
                    _imageUploadService.DeleteImages(folderPath, rowid.ToString());
                    // Upload new image
                    model.Logo = _imageUploadService.UploadImage(model.Logo, folderPath, fileNamePrefix, imgType);

                    //update table
                    SqlCommand cmd2 = new SqlCommand("SaveCompanyEsign", con);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.Add("@Company_Id", SqlDbType.BigInt).Value = rowid;
                    cmd2.Parameters.Add("@EsignImage", SqlDbType.NVarChar, -1).Value = model.Esign; // Kept max for nvarchar(max)
                    cmd2.ExecuteNonQuery();
                }
                */
                con.Close();

                if (model.Company_Id > 0)
                {
                    apiResponse.message = "Company updated successfully!";
                }
                else
                {
                    apiResponse.message = "Company inserted successfully!";
                }
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }
        private void UploadAndSaveImage(string image, string folderPath, string fileNamePrefix, string storedProcedureName, string columnName, SqlConnection con, long rowid)
        {
            if (!string.IsNullOrEmpty(image))
            {
                // Check file extension
                string imgType = _imageUploadService.GetFileTypeFromBase64(image);

                // Generate file name and timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileName = $"{fileNamePrefix}_{timestamp}";

                // Delete existing images for the given row ID
                _imageUploadService.DeleteImages(folderPath, rowid.ToString());

                // Upload new image
                image = _imageUploadService.UploadImage(image, folderPath, fileName, imgType);

                // Update table in database
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Company_Id", SqlDbType.BigInt).Value = rowid;
                    cmd.Parameters.Add($"@{columnName}", SqlDbType.NVarChar, -1).Value = image;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
    }
}
