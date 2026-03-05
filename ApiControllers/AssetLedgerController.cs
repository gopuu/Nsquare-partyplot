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
    public class AssetLedgerController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetLedgerController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }
        [HttpPost]
        [Route("GetAssetLedger")]
        public IActionResult GetAssetLedger([FromBody] GetAssetLedgerRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<AssetLedgerModel> ledgerList = new List<AssetLedgerModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetLedger", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@AssetMaster_Id", request.AssetMaster_Id);
                    cmd.Parameters.AddWithValue("@TransactionType", (object?)request.TransactionType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReferenceId", request.ReferenceId);
                    cmd.Parameters.AddWithValue("@ReferenceTable", (object?)request.ReferenceTable ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@MonthDuration", (object?)request.MonthDuration ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object?)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object?)request.ToDate ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@IsReversal", (object?)request.IsReversal ?? DBNull.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    ledgerList.Add(new AssetLedgerModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        AssetMaster_Id = Convert.ToInt64(row["AssetMaster_Id"]),
                        AssetName = Convert.ToString(row["AssetName"]),

                        TransactionType = Convert.ToString(row["TransactionType"]),
                        ReferenceId = row["ReferenceId"] == DBNull.Value ? null : Convert.ToInt64(row["ReferenceId"]),
                        ReferenceTable = Convert.ToString(row["ReferenceTable"]),

                        QtyIn = Convert.ToDecimal(row["QtyIn"]),
                        QtyOut = Convert.ToDecimal(row["QtyOut"]),

                        TransactionDate = Convert.ToString(row["TransactionDate"]),
                        TransactionDate2 = Convert.ToString(row["TransactionDate2"]),

                        Remarks = Convert.ToString(row["Remarks"]),

                        IsReversal = row["IsReversal"] == DBNull.Value ? null : Convert.ToBoolean(row["IsReversal"]),

                        Created_At = Convert.ToString(row["Created_At"]),
                        Created_By = row["Created_By"] == DBNull.Value ? null : Convert.ToInt64(row["Created_By"])
                    });
                }

                apiResponse.message = "Asset ledger fetched successfully!";
                apiResponse.data = ledgerList;
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }
    }
}
