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
    public class AssetPurchaseController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public AssetPurchaseController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetAssetPurchase")]
        public IActionResult GetAssetPurchase([FromBody] GetAssetPurchaseRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<AssetPurchaseModel> purchases = new List<AssetPurchaseModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetAssetPurchaseList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    purchases.Add(new AssetPurchaseModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        PurchaseDate = Convert.ToString(row["PurchaseDate"]),
                        PurchaseDate1 = Convert.ToString(row["PurchaseDate1"]),
                        VendorName = Convert.ToString(row["VendorName"]),
                        InvoiceNo = Convert.ToString(row["InvoiceNo"]),
                        Remarks = Convert.ToString(row["Remarks"]),
                        NetTotalAmount = Convert.ToDecimal(row["NetTotalAmount"]),

                        Items = string.IsNullOrEmpty(Convert.ToString(row["Items"]))
                            ? new List<AssetPurchaseItemModel>()
                            : JsonConvert.DeserializeObject<List<AssetPurchaseItemModel>>(row["Items"].ToString())
                    });
                }

                apiResponse.message = "Asset purchase list fetched successfully!";
                apiResponse.data = purchases;
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

        [HttpPost]
        [Route("SaveAssetPurchase")]
        public IActionResult SaveAssetPurchase([FromBody] SaveAssetPurchaseRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveAssetPurchase", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@PurchaseDate", model.PurchaseDate);
                    cmd.Parameters.AddWithValue("@VendorName", (object)model.VendorName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@InvoiceNo", (object)model.InvoiceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NetTotalAmount", model.NetTotalAmount);

                    cmd.Parameters.AddWithValue("@Items", JsonConvert.SerializeObject(model.Items));
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.message = model.Id > 0
                    ? "Asset purchase updated successfully!"
                    : "Asset purchase inserted successfully!";

                apiResponse.data = new { AssetPurchaseId = rowId };
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

        [HttpPost]
        [Route("DeleteAssetPurchase")]
        public IActionResult DeleteAssetPurchase([FromBody] DeleteAssetPurchaseRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteAssetPurchase", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = "Asset purchase deleted successfully!";
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

        [HttpGet]
        [Route("GetNextAssetPurchaseInvoiceNumber")]
        public IActionResult GetNextAssetPurchaseInvoiceNumber()
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long nextInvoiceNumber = 1;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetNextAssetPurchaseInvoiceNumber", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        nextInvoiceNumber = Convert.ToInt64(result);
                    }
                }

                apiResponse.message = "Next invoice number fetched successfully!";
                apiResponse.data = new { NextInvoiceNumber = nextInvoiceNumber };
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
