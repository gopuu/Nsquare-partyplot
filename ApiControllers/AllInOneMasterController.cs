using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.EMMA;
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
    public class AllInOneMasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly ImageUploadService _imageUploadService;

        public AllInOneMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("SaveAllInOneMaster")]
        public IActionResult SaveAllInOneMaster([FromBody] SaveAllInOneMasterRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAllInOneMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@Action_By", model.Action_By);
                    cmd.Parameters.AddWithValue("@Link", model.Link);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                    string latestImagePath = string.Empty;

                    //photo upload
                    if (model.Image != string.Empty && model.Image != null)
                    {
                        string imgType = _imageUploadService.GetFileTypeFromBase64(model.Image);
                        string folderPath = $"org/{domain}/AllInOne/"; // Updated to include domain
                        _imageUploadService.DeleteImages(folderPath, $"Id_{rowId.ToString()}");
                        model.Image = _imageUploadService.UploadImage(model.Image, folderPath, $"Id_{rowId.ToString()}", imgType);

                        //update table
                        SqlCommand cmd2 = new SqlCommand("Save_DocumentFile", con);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@Id", SqlDbType.BigInt).Value = rowId;
                        cmd2.Parameters.Add("@DocumentName", SqlDbType.NVarChar, -1).Value = model.Image; 
                        cmd2.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "AllInOne_Master";
                        cmd2.ExecuteNonQuery();
                        latestImagePath = $"{domainUrl}/uploads/org/{domain}/AllInOne/{model.Image}";
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0  ? "Record updated successfully!" : "Record added successfully!";

                apiResponse.data = new SaveAllInOneMasterRequest
                {
                    Id = rowId
                };


                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteAllInOneMaster")]
        public IActionResult DeleteAllInOneMaster([FromBody] DeleteAllInOneMasterRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteAllInOneMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@actionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Record deleted successfully!";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetAllInOneMaster")]
        public IActionResult GetAllInOneMaster([FromBody] GetAllInOneMasterRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<AllInOneMaster> list = new List<AllInOneMaster>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAllInOneMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    long id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0;
                    string title = row["Title"] != DBNull.Value ? Convert.ToString(row["Title"]) : string.Empty;
                    string link = row["Link"] != DBNull.Value ? Convert.ToString(row["Link"]) : string.Empty;
                    string imageName = row["Image"] != DBNull.Value ? Convert.ToString(row["Image"]) : string.Empty;

                    string imageUrl = string.Empty;

                    if (!string.IsNullOrWhiteSpace(imageName))
                    {
                        string relativePath = $"/uploads/org/{domain}/AllInOne/{imageName}";
                        string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "org", domain, "AllInOne", imageName);

                        if (System.IO.File.Exists(physicalPath))
                        {
                            imageUrl = $"{domainUrl}{relativePath}";
                        }
                        else
                        {
                            imageUrl = $"{domainUrl}/assets2/img/allinone_default/{imageName}";
                        }
                    }

                    list.Add(new AllInOneMaster
                    {
                        Id = id,
                        Title = title ?? string.Empty,
                        Link = link ?? string.Empty,
                        Image = imageUrl
                    });
                }
                apiResponse.status = "Success";
                apiResponse.message = "All-in-one master list fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetAllInOneLinkMaster")]
        public IActionResult GetAllInOneLinkMaster()
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                AllInOnePropertyResponse responseModel = new AllInOnePropertyResponse();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAllInOneLinkMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())   // Only expecting one property row
                        {
                            string logoName = dr["PropertyLogo"] != DBNull.Value ? Convert.ToString(dr["PropertyLogo"]) : string.Empty;
                            string logoUrl;
                            if (!string.IsNullOrWhiteSpace(logoName))
                            {
                                // Upload folder path
                                string uploadPhysicalPath = Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    "uploads",
                                    "org",
                                    domain,
                                    "CompanyInvoice_CompanyLogo_Image",
                                    logoName
                                );

                                if (System.IO.File.Exists(uploadPhysicalPath))
                                {
                                    logoUrl = $"{domainUrl}/uploads/org/{domain}/CompanyInvoice_CompanyLogo_Image/{logoName}";
                                }
                                else
                                {
                                    // Default folder path
                                    string defaultPhysicalPath = Path.Combine(
                                        Directory.GetCurrentDirectory(),
                                        "wwwroot",
                                        "assets2",
                                        "img",
                                        "allinone_default",
                                        logoName
                                    );

                                    if (System.IO.File.Exists(defaultPhysicalPath))
                                    {
                                        logoUrl = $"{domainUrl}/assets2/img/allinone_default/{logoName}";
                                    }
                                    else
                                    {
                                        logoUrl = $"{domainUrl}/assets2/img/allinone_default/default-logo.png";
                                    }
                                }
                            }
                            else
                            {
                                logoUrl = $"{domainUrl}/assets2/img/allinone_default/default-logo.png";
                            }

                            responseModel.PropertyLogo = logoUrl;

                            responseModel.PropertyName = dr["PropertyName"] != DBNull.Value ? Convert.ToString(dr["PropertyName"]) : string.Empty;
                            responseModel.CompanyName = dr["CompanyName"] != DBNull.Value ? Convert.ToString(dr["CompanyName"]) : string.Empty;

                            string jsonData = dr["AllInOneJSON"] != DBNull.Value ? Convert.ToString(dr["AllInOneJSON"]) : "[]";
                            var linkList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AllInOneLinkMaster>>(jsonData ?? "[]");

                            if (linkList != null)
                            {
                                foreach (var item in linkList)
                                {
                                    string imageName = item.Image ?? string.Empty;
                                    string imageUrl;

                                    if (!string.IsNullOrWhiteSpace(imageName))
                                    {
                                        string physicalPath = Path.Combine(
                                            Directory.GetCurrentDirectory(),
                                            "wwwroot",
                                            "uploads",
                                            "org",
                                            domain,
                                            "AllInOne",
                                            imageName
                                        );

                                        if (System.IO.File.Exists(physicalPath))
                                        {
                                            imageUrl = $"{domainUrl}/uploads/org/{domain}/AllInOne/{imageName}";
                                        }
                                        else
                                        {
                                            string defaultPhysicalPath = Path.Combine(
                                                Directory.GetCurrentDirectory(),
                                                "wwwroot",
                                                "assets2",
                                                "img",
                                                "allinone_default",
                                                imageName
                                            );

                                            if (System.IO.File.Exists(defaultPhysicalPath))
                                            {
                                                imageUrl = $"{domainUrl}/assets2/img/allinone_default/{imageName}";
                                            }
                                            else
                                            {
                                                imageUrl = $"{domainUrl}/assets2/img/image_default.png";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        imageUrl = $"{domainUrl}/assets2/img/image_default.png";
                                    }

                                    item.Image = imageUrl;
                                }
                            }

                            responseModel.Links = linkList ?? new List<AllInOneLinkMaster>();
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "All In One Link list fetched successfully!";
                apiResponse.data = responseModel;

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
    }
}
