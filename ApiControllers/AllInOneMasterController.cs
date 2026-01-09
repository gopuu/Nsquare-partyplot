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
                        out IActionResult errorResult))
                {
                    return errorResult;
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
                        out IActionResult errorResult))
                    return errorResult;

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
                        out IActionResult errorResult))
                    return errorResult;

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
                    list.Add(new AllInOneMaster
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        Title = Convert.ToString(row["Title"]),
                        Link = Convert.ToString(row["Link"]),
                        Image = row["Image"] != DBNull.Value
                                && !string.IsNullOrEmpty(row["Image"].ToString())
                            ? domainUrl + "/uploads/org/" + domain + "/AllInOne/" + row["Image"].ToString()
                            : string.Empty
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
            List<AllInOneLinkMaster> list = new List<AllInOneLinkMaster>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAllInOneLinkMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new AllInOneLinkMaster
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                HallName = Convert.ToString(dr["HallName"]),
                                Title = Convert.ToString(dr["Title"]),
                                Link = Convert.ToString(dr["Link"]),
                                Image = dr["Image"] != DBNull.Value
                                    ? $"{domainUrl}/uploads/org/{domain}/AllInOne/{dr["Image"]}"
                                    : string.Empty
                            });
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "All In One Link list fetched successfully!";
                apiResponse.data = list;

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
