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
    public class FundTransferController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public FundTransferController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetFundTransferList")]
        public IActionResult GetFundTransferList([FromBody] GetFundTransferRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<FundTransferModel> list = new List<FundTransferModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetFundTransfer", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@FromAccountId", request.FromAccountId);
                    cmd.Parameters.AddWithValue("@ToAccountId", request.ToAccountId);
                    cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new FundTransferModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        FromAccountId = Convert.ToInt64(row["FromAccountId"]),
                        FromAccountName = Convert.ToString(row["FromAccountName"]),
                        ToAccountId = Convert.ToInt64(row["ToAccountId"]),
                        ToAccountName = Convert.ToString(row["ToAccountName"]),
                        Amount = Convert.ToDecimal(row["Amount"]),
                        TransferDate = row["TransferDate"] == DBNull.Value ? default : DateOnly.FromDateTime(Convert.ToDateTime(row["TransferDate"])),
                        ReferenceNo = Convert.ToString(row["ReferenceNo"]),
                        Notes = Convert.ToString(row["Notes"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Fund transfers fetched successfully!";
                apiResponse.data = list;
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
        [Route("SaveFundTransfer")]
        public IActionResult SaveFundTransfer([FromBody] SaveFundTransferRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                long rowId = 0;
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveFundTransfer", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@FromAccountId", model.FromAccountId);
                    cmd.Parameters.AddWithValue("@ToAccountId", model.ToAccountId);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@TransferDate", model.TransferDate);
                    cmd.Parameters.AddWithValue("@ReferenceNo", (object)model.ReferenceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", (object)model.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0 ? "Fund transfer updated successfully!" : "Fund transfer inserted successfully!";
                apiResponse.data = new { FundTransferId = rowId };
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteFundTransfer")]
        public IActionResult DeleteFundTransfer([FromBody] DeleteFundTransferRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteFundTransfer", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Fund transfer deleted successfully!";
                apiResponse.data = null;
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
