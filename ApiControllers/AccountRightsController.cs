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
    public class AccountRightsController : MasterController
    {
        private string connString = string.Empty;
        private readonly string domainUrl;

        public AccountRightsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService) : base(configuration, httpContextAccessor) 
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAccountRights")]
        public IActionResult GetAccountRights([FromBody] GetAccountRightsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                // Convert StaffIds to JSON
                string staffJson = request?.StaffIds != null && request.StaffIds.Any()
                    ? JsonConvert.SerializeObject(request.StaffIds)
                    : null;

                List<AccountRightsModel_Response> result = new List<AccountRightsModel_Response>();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAccountRights", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@StaffIds", SqlDbType.NVarChar)
                        .Value = (object)staffJson ?? DBNull.Value;

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new AccountRightsModel_Response
                            {
                                AccountId = Convert.ToInt64(dr["AccountId"]),
                                AccountName = dr["AccountName"].ToString(),
                                CanEdit = Convert.ToBoolean(dr["CanEdit"]),
                                CanView = Convert.ToBoolean(dr["CanView"]),
                                CanTransfer = Convert.ToBoolean(dr["CanTransfer"])
                            });
                        }
                    }
                }

                response.status = "Success";
                response.message = "Account rights loaded successfully";
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
        [Route("SaveAccountRights")]
        public IActionResult SaveAccountRights([FromBody] SaveAccountRightsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                    return errorResult!;

                if (request == null ||
                    request.StaffIds == null || !request.StaffIds.Any() ||
                    request.AccountRights == null || !request.AccountRights.Any())
                {
                    response.status = "Error";
                    response.message = "Invalid input data";
                    return Ok(response);
                }

                string staffJson = JsonConvert.SerializeObject(request.StaffIds);
                string rightsJson = JsonConvert.SerializeObject(request.AccountRights);

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAccountRights", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@StaffIds", SqlDbType.NVarChar).Value = staffJson;
                    cmd.Parameters.Add("@AccountRights", SqlDbType.NVarChar).Value = rightsJson;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                response.status = "Success";
                response.message = "Account rights saved successfully";
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
