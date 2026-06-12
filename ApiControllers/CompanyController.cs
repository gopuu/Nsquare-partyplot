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
