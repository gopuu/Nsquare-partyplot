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
    public class AccountMasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AccountMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAccount")]
        public IActionResult GetAccount([FromBody] GetAccountRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<AccountMasterModel> accounts = new List<AccountMasterModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAccountMasterList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    accounts.Add(new AccountMasterModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        AccountName = Convert.ToString(row["AccountName"]),
                        AccountType = Convert.ToString(row["AccountType"]),
                        OpeningBalance = Convert.ToDecimal(row["OpeningBalance"]),
                        CurrentBalance = Convert.ToDecimal(row["CurrentBalance"]),

                        BankName = Convert.ToString(row["BankName"]),
                        BranchName = Convert.ToString(row["BranchName"]),
                        IFSC = Convert.ToString(row["IFSC"]),
                        AccountNumber = Convert.ToString(row["AccountNumber"]),
                        AccountHolderName = Convert.ToString(row["AccountHolderName"]),

                        UPI_Id = Convert.ToString(row["UPI_Id"]),
                        Description = Convert.ToString(row["Description"]),
                        IsDefaultAccount = Convert.ToBoolean(row["IsDefaultAccount"])
                    });
                }

                apiResponse.message = "Account list fetched successfully!";
                apiResponse.data = accounts;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveAccount")]
        public IActionResult SaveAccount([FromBody] SaveAccountRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAccountMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
                    cmd.Parameters.AddWithValue("@AccountType", model.AccountType);
                    cmd.Parameters.AddWithValue("@OpeningBalance", model.OpeningBalance);
                    cmd.Parameters.AddWithValue("@CurrentBalance", model.CurrentBalance);

                    cmd.Parameters.AddWithValue("@BankName", (object)model.BankName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchName", (object)model.BranchName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IFSC", (object)model.IFSC ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountNumber", (object)model.AccountNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountHolderName", (object)model.AccountHolderName ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@UPI_Id", (object)model.UPI_Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDefaultAccount", model.IsDefaultAccount);

                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.message = model.Id > 0 ? "Account updated successfully!" : "Account inserted successfully!";
                apiResponse.data = new { AccountId = rowId };
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteAccount")]
        public IActionResult DeleteAccount([FromBody] DeleteAccountRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteAccountMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = "Account deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

    }
}
