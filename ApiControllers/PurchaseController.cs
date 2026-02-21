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
    public class PurchaseController : MasterController
    {
        private readonly ImageUploadService _imageUploadService;
        private readonly string domainUrl;
        private string connString = string.Empty;
        public PurchaseController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _imageUploadService = imageUploadService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetPurchaseList")]
        public IActionResult GetPurchaseList([FromBody] GetPurchaseRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetPurchaseListModel> purchases = new();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString,
                    out string domain,
                    out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPurchaseList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id > 0 ? request.Id : 0);
                    cmd.Parameters.AddWithValue("@SupplierId", request.SupplierId > 0 ? request.SupplierId : 0);

                    con.Open();
                    using SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    var model = new GetPurchaseListModel();
                    model.Id = Convert.ToInt64(row["Id"]);
                    model.PurchaseFrom = Convert.ToInt64(row["PurchaseFrom"]);
                    model.PurchaseFromName = Convert.ToString(row["PurchaseFromName"]);

                    model.SupplierId = Convert.ToInt64(row["SupplierId"]);
                    model.SupplierName = Convert.ToString(row["SupplierName"]);
                    model.SupplierCompany = Convert.ToString(row["SupplierCompany"]);
                    model.SupplierPhone = Convert.ToString(row["SupplierPhone"]);

                    model.InvoiceDate = row["InvoiceDate"] == DBNull.Value
                        ? null
                        : DateOnly.FromDateTime(Convert.ToDateTime(row["InvoiceDate"]));

                    model.InvoiceDate_Format = Convert.ToString(row["InvoiceDate_Format"]);
                    model.InvoiceNo = Convert.ToString(row["InvoiceNo"]);
                    model.PONo = Convert.ToString(row["PONo"]);

                    model.DiscountType = row["DiscountType"] == DBNull.Value ? 0 : Convert.ToInt64(row["DiscountType"]);
                    model.TotalDiscountInInvoice = Convert.ToDecimal(row["TotalDiscountInInvoice"]);
                    model.DiscountPercentage = Convert.ToDecimal(row["DiscountPercentage"]);
                    model.DetermineWhenTaxApplicable = row["DetermineWhenTaxApplicable"] == DBNull.Value
                        ? 0
                        : Convert.ToInt64(row["DetermineWhenTaxApplicable"]);

                    model.SubTotal_TotalDiscount_Amount = Convert.ToDecimal(row["SubTotal_TotalDiscount_Amount"]);
                    model.SubTotal_OtherCharges_Amount = Convert.ToDecimal(row["SubTotal_OtherCharges_Amount"]);

                    model.DeliveryChargeIncludedInInvoice = Convert.ToDecimal(row["DeliveryChargeIncludedInInvoice"]);
                    model.DeliveryCharge_CGST = Convert.ToDecimal(row["DeliveryCharge_CGST"]);
                    model.DeliveryCharge_SGST = Convert.ToDecimal(row["DeliveryCharge_SGST"]);

                    model.SubTotal_OtherTaxes_Amount = Convert.ToDecimal(row["SubTotal_OtherTaxes_Amount"]);
                    model.TCS_Percent = Convert.ToDecimal(row["TCS_Percent"]);
                    model.TDS_Percent = Convert.ToDecimal(row["TDS_Percent"]);

                    model.GrandTotal_Amount = Convert.ToDecimal(row["GrandTotal_Amount"]);
                    model.PaidAmount = Convert.ToDecimal(row["PaidAmount"]);
                    model.Remaining_Amount = Convert.ToDecimal(row["Remaining_Amount"]);

                    // 🔹 FIRST PAYMENT (FOR EDIT)
                    model.PaymentType = row["PaymentType"] == DBNull.Value ? 0 : Convert.ToInt64(row["PaymentType"]);
                    model.PaymentDate = row["PaymentDate"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(row["PaymentDate"]).ToString("yyyy-MM-dd");

                    model.PaymentModeId = row["PaymentModeId"] == DBNull.Value ? 0 : Convert.ToInt64(row["PaymentModeId"]);
                    model.PaymentAccountId = row["PaymentAccountId"] == DBNull.Value ? 0 : Convert.ToInt64(row["PaymentAccountId"]);
                    model.PaymentRefNo = Convert.ToString(row["PaymentRefNo"]);
                    model.PaymentRemarks = Convert.ToString(row["PaymentRemarks"]);

                    model.ChequeBank = Convert.ToString(row["ChequeBank"]);
                    model.ChequeNo = Convert.ToString(row["ChequeNo"]);
                    model.ChequeDate = row["ChequeDate"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(row["ChequeDate"]).ToString("yyyy-MM-dd");

                    model.IsUpdateInventoryStock = Convert.ToBoolean(row["IsUpdateInventoryStock"]);

                    model.UploadInvoiceFile =
                        row["UploadInvoiceFile"] != DBNull.Value &&
                        !string.IsNullOrWhiteSpace(row["UploadInvoiceFile"].ToString())
                            ? $"{domainUrl}/uploads/org/{domain}/purchaseDocuments/{row["UploadInvoiceFile"]}"
                            : string.Empty;

                    model.IsActive = Convert.ToBoolean(row["IsActive"]);
                    model.Created_By_EmpId = Convert.ToInt64(row["Created_By_EmpId"]);
                    model.Created_By_EmpName = Convert.ToString(row["Created_By_EmpName"]);
                    model.Created_On = Convert.ToString(row["Created_On"]);
                    model.Modified_On = Convert.ToString(row["Modified_On"]);
                    model.Status = Convert.ToString(row["Status"]);

                    // 🔹 PURCHASE ITEMS JSON
                    var itemsJson = Convert.ToString(row["JSON_PurchaseItems"]);
                    model.JSON_PurchaseItems = !string.IsNullOrWhiteSpace(itemsJson)
                        ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<GetPurchaseItems>>(itemsJson)
                        : new List<GetPurchaseItems>();

                    purchases.Add(model);
                }

                apiResponse.message = "Purchase data fetched successfully!";
                apiResponse.data = purchases;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SavePurchase")]
        public IActionResult SavePurchase([FromBody] SavePurchaseRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SavePurchase", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;

                    cmd.Parameters.Add("@PurchaseFrom", SqlDbType.BigInt).Value = (object)model.PurchaseFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@SupplierId", SqlDbType.BigInt).Value = model.SupplierId;
                    cmd.Parameters.Add("@InvoiceDate", SqlDbType.NVarChar).Value = model.InvoiceDate;
                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = (object)model.InvoiceNo ?? DBNull.Value;
                    cmd.Parameters.Add("@PONo", SqlDbType.NVarChar).Value = (object)model.PONo ?? DBNull.Value;

                    cmd.Parameters.Add("@DiscountType", SqlDbType.BigInt).Value = (object)model.DiscountType ?? DBNull.Value;
                    cmd.Parameters.Add("@TotalDiscountInInvoice", SqlDbType.Decimal).Value = model.TotalDiscountInInvoice;
                    cmd.Parameters.Add("@DiscountPercentage", SqlDbType.Decimal).Value = model.DiscountPercentage;
                    cmd.Parameters.Add("@DetermineWhenTaxApplicable", SqlDbType.BigInt).Value = (object)model.DetermineWhenTaxApplicable ?? DBNull.Value;

                    cmd.Parameters.Add("@SubTotal_TotalDiscount_Amount", SqlDbType.Decimal).Value = model.SubTotal_TotalDiscount_Amount;
                    cmd.Parameters.Add("@SubTotal_OtherCharges_Amount", SqlDbType.Decimal).Value = model.SubTotal_OtherCharges_Amount;
                    cmd.Parameters.Add("@SubTotal_OtherTaxes_Amount", SqlDbType.Decimal).Value = model.SubTotal_OtherTaxes_Amount;

                    cmd.Parameters.Add("@DeliveryChargeIncludedInInvoice", SqlDbType.Decimal).Value = model.DeliveryChargeIncludedInInvoice;
                    cmd.Parameters.Add("@DeliveryCharge_CGST", SqlDbType.Decimal).Value = model.DeliveryCharge_CGST;
                    cmd.Parameters.Add("@DeliveryCharge_SGST", SqlDbType.Decimal).Value = model.DeliveryCharge_SGST;

                    cmd.Parameters.Add("@TCS_Percent", SqlDbType.Decimal).Value = model.TCS_Percent;
                    cmd.Parameters.Add("@TDS_Percent", SqlDbType.Decimal).Value = model.TDS_Percent;

                    cmd.Parameters.Add("@GrandTotal_Amount", SqlDbType.Decimal).Value = model.GrandTotal_Amount;

                    cmd.Parameters.Add("@PaymentType", SqlDbType.BigInt).Value = (object)model.PaymentType ?? DBNull.Value;
                    cmd.Parameters.Add("@PaymentDate", SqlDbType.NVarChar).Value = (object)model.PaymentDate ?? DBNull.Value;
                    //cmd.Parameters.Add("@PaidAmount", SqlDbType.Decimal).Value = (object)model.PaidAmount ?? DBNull.Value;
                    var pPaid = cmd.Parameters.Add("@PaidAmount", SqlDbType.Decimal);
                    pPaid.Precision = 18;
                    pPaid.Scale = 2;
                    pPaid.Value = (object)model.PaidAmount ?? DBNull.Value;

                    cmd.Parameters.Add("@PaymentModeId", SqlDbType.BigInt).Value = (object)model.PaymentModeId ?? DBNull.Value;
                    cmd.Parameters.Add("@PaymentRefNo", SqlDbType.NVarChar).Value = (object)model.PaymentRefNo ?? DBNull.Value;
                    cmd.Parameters.Add("@PaymentAccountId", SqlDbType.BigInt).Value = (object)model.PaymentAccountId ?? DBNull.Value;
                    cmd.Parameters.Add("@PaymentRemarks", SqlDbType.NVarChar).Value = (object)model.PaymentRemarks ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeBank", SqlDbType.VarChar).Value = (object)model.ChequeBank ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeNo", SqlDbType.VarChar).Value = (object)model.ChequeNo ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeDate", SqlDbType.NVarChar).Value = (object)model.ChequeDate ?? DBNull.Value;

                    cmd.Parameters.Add("@IsUpdateInventoryStock", SqlDbType.Bit).Value = model.IsUpdateInventoryStock;

                    cmd.Parameters.Add("@UploadInvoiceFile", SqlDbType.NVarChar).Value = DBNull.Value;

                    var itemsJson = model.JSON_PurchaseItems != null
                        ? Newtonsoft.Json.JsonConvert.SerializeObject(model.JSON_PurchaseItems)
                        : null;
                    cmd.Parameters.Add("@JSON_PurchaseItems", SqlDbType.NVarChar)
                        .Value = (object)itemsJson ?? DBNull.Value;

                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                    conn.Open();

                    Int64 rowid = Convert.ToInt64(cmd.ExecuteScalar());
                    string latestImagePath = string.Empty;
                    //photo upload
                    if (model.UploadInvoiceFile != string.Empty && model.UploadInvoiceFile != null)
                    {
                        string imgType = _imageUploadService.GetFileTypeFromBase64(model.UploadInvoiceFile);

                        //string imgType = ".jpg";
                        //get filename with fileType
                        //model.UploadInvoiceFile = _imageUploadService.UploadImage(model.UploadInvoiceFile, "employeePic", rowid.ToString(), imgType);

                        string folderPath = $"org/{domain}/purchaseDocuments/"; // Updated to include domain
                        _imageUploadService.DeleteImages(folderPath, $"Invoice_PurchaseId_{rowid.ToString()}");
                        //model.Logo = _imageUploadService.UploadImage(model.Logo, folderPath, rowid.ToString(), imgType);
                        model.UploadInvoiceFile = _imageUploadService.UploadImage(model.UploadInvoiceFile, folderPath, $"Invoice_PurchaseId_{rowid.ToString()}", imgType);

                        //update table
                        SqlCommand cmd2 = new SqlCommand("Save_DocumentFile", conn);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@Id", SqlDbType.BigInt).Value = rowid;
                        cmd2.Parameters.Add("@DocumentName", SqlDbType.NVarChar, -1).Value = model.UploadInvoiceFile; // Kept max for nvarchar(max)
                        cmd2.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "Purchase";
                        cmd2.ExecuteNonQuery();

                        latestImagePath = $"{domainUrl}/uploads/org/{domain}/purchaseDocuments/{model.UploadInvoiceFile}";
                    }

                    apiResponse.message = model.Id > 0 ? "Purchase updated successfully!" : "Purchase inserted successfully!";
                    apiResponse.data = new { PurchaseId = rowid };
                    apiResponse.status = "Success";

                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("DeletePurchase")]
        public IActionResult DeletePurchase([FromBody] DeletePurchaseRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeletePurchase", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = "Purchase deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpGet]
        [Route("GetNextPurchaseOrderNo")]
        public IActionResult GetNextPurchaseOrderNo()
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetNextPurchaseOrderNo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    apiResponse.status = "Success";
                    apiResponse.message = "Next Purchase Order No fetched!";
                    apiResponse.data = new { NextPONo = result?.ToString() ?? "1" };

                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpGet]
        [Route("GetNextInvoiceNo")]
        public IActionResult GetNextInvoiceNo()
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetNextInvoiceNo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    apiResponse.status = "Success";
                    apiResponse.message = "Next Invoice No fetched!";
                    apiResponse.data = new { NextInvoiceNo = result?.ToString() ?? "1" };

                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        //Widgets
        [HttpGet]
        [Route("GetPurchaseList_Widgets")]
        public IActionResult GetPurchaseList_Widgets()
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                PurchaseWidgetDto widgetData = null;

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPurchaseList_Widgets", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())   // Only one row expected
                        {
                            widgetData = new PurchaseWidgetDto
                            {
                                GrandTotal_Amount = Convert.ToString(dr["GrandTotal_Amount"]),
                                Outstanding_Amount = Convert.ToString(dr["Outstanding_Amount"]),
                                TaxPaidToSeller = Convert.ToString(dr["TaxPaidToSeller"])
                            };
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Purchase widget data fetched!";
                apiResponse.data = widgetData;   // RETURN SINGLE OBJECT

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        //All Txn
        [HttpPost]
        [Route("GetPurchasePaymentTransactions")]
        public IActionResult GetPurchasePaymentTransactions([FromBody] GetPurchasePaymentTransactionsRequest request)
        {
            apiResponse response = new();
            List<PurchasePaymentTransactionModel> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPurchasePaymentTransactions", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PurchaseId", request.PurchaseId);
                    cmd.Parameters.AddWithValue("@MonthDuration", (object)request.MonthDuration ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", (object)request.FromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)request.ToDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplierId", request.SupplierId);
                    cmd.Parameters.AddWithValue("@PaymentModeId", request.PaymentModeId);
                    cmd.Parameters.AddWithValue("@AccountId", request.AccountId);
                    cmd.Parameters.AddWithValue("@FromAmount", (object)request.FromAmount ?? 0);
                    cmd.Parameters.AddWithValue("@ToAmount", (object)request.ToAmount ?? 0);

                    con.Open();
                    using SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        list.Add(new PurchasePaymentTransactionModel
                        {
                            PaymentId = Convert.ToInt64(dr["PaymentId"]),
                            PurchaseId = Convert.ToInt64(dr["PurchaseId"]),

                            InvoiceNo = Convert.ToString(dr["InvoiceNo"]),
                            PONo = Convert.ToString(dr["PONo"]),
                            InvoiceDate_Format = Convert.ToString(dr["InvoiceDate_Format"]),

                            SupplierName = Convert.ToString(dr["SupplierName"]),

                            PaymentDate_Format = Convert.ToString(dr["PaymentDate_Format"]),
                            PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),

                            PaymentMode = Convert.ToString(dr["PaymentMode"]),
                            AccountName = Convert.ToString(dr["AccountName"]),

                            ReferenceNo = Convert.ToString(dr["ReferenceNo"]),
                            Remarks = Convert.ToString(dr["Remarks"]),

                            ChequeBank = Convert.ToString(dr["ChequeBank"]),
                            ChequeNo = Convert.ToString(dr["ChequeNo"]),
                            ChequeDate = dr["ChequeDate"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(dr["ChequeDate"]).ToString("dd-MMM-yyyy"),

                            PaidBy = Convert.ToString(dr["PaidBy"]),
                            PaidOn = Convert.ToDateTime(dr["PaidOn"]).ToString("dd-MMM-yyyy HH:mm")
                        });
                    }
                }

                response.status = "Success";
                response.message = "Payment transactions fetched successfully";
                response.data = list;
                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                response.status = "Error";
                response.message = ex.Message;
                response.data = null;
                return StatusCode(500, response);
            }
        }

        //Save Outstanding
        [HttpPost]
        [Route("SavePurchaseOutstandingPayment")]
        public IActionResult SavePurchaseOutstandingPayment([FromBody] SavePurchaseOutstandingPaymentRequest model)
        {
            apiResponse response = new();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString,
                    out string domain,
                    out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SavePurchaseOutstandingPayment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PurchaseId", model.PurchaseId);
                    cmd.Parameters.AddWithValue("@PaymentDate", model.PaymentDate);
                    cmd.Parameters.AddWithValue("@PaidAmount", model.PaidAmount);
                    cmd.Parameters.AddWithValue("@PaymentModeId", model.PaymentModeId);
                    cmd.Parameters.AddWithValue("@AccountId", model.AccountId);

                    cmd.Parameters.AddWithValue("@ReferenceNo", (object)model.ReferenceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ChequeBank", (object)model.ChequeBank ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChequeNo", (object)model.ChequeNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChequeDate", (object)model.ChequeDate ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                response.status = "Success";
                response.message = "Outstanding payment saved successfully";
                response.data = null;
                return Ok(response);
            }
            catch (SqlException ex)
            {
                response.status = "Error";
                response.message = ex.Message; // shows SP RAISERROR messages
                response.data = null;
                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                response.status = "Error";
                response.message = ex.Message;
                response.data = null;
                return StatusCode(500, response);
            }
        }


    }
}
