using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.Json;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRightsController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;
        //private readonly ImageUploadService _imageUploadService;
        private readonly string domainUrl;

        public UserRightsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_imageUploadService = imageUploadService;
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetUserRightsList")]
        public IActionResult GetUserRightsList([FromBody] GetUserRightsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                List<UserRightsGroupModel> result = new List<UserRightsGroupModel>();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetUserRightsList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@StaffId", SqlDbType.BigInt).Value = request.StaffId;

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string rightsJson = dr["JSON_UserRights"]?.ToString();

                            List<ModuleRights> rights =
                                JsonConvert.DeserializeObject<List<ModuleRights>>(rightsJson);

                            result.Add(new UserRightsGroupModel
                            {
                                GroupId = Convert.ToInt64(dr["Id"]),
                                GroupTitle = dr["Title"]?.ToString(),
                                UserRights = rights
                            });
                        }
                    }
                }

                response.status = "Success";
                response.message = "User rights loaded successfully";
                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("SaveUserRights")]
        public IActionResult SaveUserRights([FromBody] SaveUserRightsModel request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                // Convert model list to JSON string
                string jsonData = JsonConvert.SerializeObject(request.UserRights);

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveUserRights", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@JsonData", SqlDbType.NVarChar)
                        .Value = jsonData;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                response.status = "Success";
                response.message = "User rights saved successfully";
                return Ok(response);
            }
            catch (SqlException ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return Ok(response);
            }
        }

    }
}
