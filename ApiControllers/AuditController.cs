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
    public class AuditController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AuditController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAccountBalanceAuditList")]
        public IActionResult GetAccountBalanceAuditList([FromBody] GetAccountBalanceAuditRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<AccountBalanceAuditModel> auditList = new List<AccountBalanceAuditModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAccountBalanceAudit", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@AccountId", request.AccountId);
                    cmd.Parameters.AddWithValue("@ActionType", (object)request.ActionType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    auditList.Add(new AccountBalanceAuditModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        AccountId = Convert.ToInt64(row["AccountId"]),
                        AccountName = Convert.ToString(row["AccountName"]),
                        PreviousBalance = Convert.ToDecimal(row["PreviousBalance"]),
                        AmountChanged = Convert.ToDecimal(row["AmountChanged"]),
                        NewBalance = Convert.ToDecimal(row["NewBalance"]),
                        ActionType = Convert.ToString(row["ActionType"]),
                        ReferenceId = row["ReferenceId"] == DBNull.Value ? null : Convert.ToInt64(row["ReferenceId"]),
                        Created_At = Convert.ToString(row["created_at"]),
                        Created_By = row["created_by"] == DBNull.Value ? null : Convert.ToInt64(row["created_by"]),
                        Created_By_Name = Convert.ToString(row["created_by_name"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Account balance audit log fetched successfully!";
                apiResponse.data = auditList;

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

    }
}
