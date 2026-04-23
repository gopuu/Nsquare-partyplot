using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using ERPAdmin.Services;
using Humanizer;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<PurchaseHub> _purchaseHub;

        public PurchaseController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, 
            ImageUploadService imageUploadService, IWebHostEnvironment env, IHubContext<PurchaseHub> purchaseHub) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            _imageUploadService = imageUploadService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _purchaseHub = purchaseHub;
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
                    cmd.Parameters.AddWithValue("@PurchaseId", request.PurchaseId > 0 ? request.PurchaseId : 0);
                    cmd.Parameters.AddWithValue("@SupplierId", request.SupplierId > 0 ? request.SupplierId : 0);
                    cmd.Parameters.AddWithValue("@InvoiceNo", request.InvoiceNo);

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

                apiResponse.message = ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetPurchaseList_GRID")]
        public IActionResult GetPurchaseList_GRID([FromBody] GetPurchaseListGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetPurchaseListGridModel> list = new();
            GetPurchaseListGridTotals totals = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataSet dsReport = new DataSet();
                DataTable? dtList = new();
                DataTable? dtTotals = new();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPurchaseList_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PONo", request.PONo ?? "");
                    cmd.Parameters.AddWithValue("@InvoiceNo", request.InvoiceNo ?? "");
                    cmd.Parameters.AddWithValue("@filterSearchText", request.FilterSearchText ?? "");
                    cmd.Parameters.AddWithValue("@SupplierId", request.SupplierId > 0 ? request.SupplierId : 0);
                    cmd.Parameters.AddWithValue("@EventFrom", request.EventFrom ?? "");
                    cmd.Parameters.AddWithValue("@EventTo", request.EventTo ?? "");
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration ?? "");
                    cmd.Parameters.AddWithValue("@StatusId", request.StatusId);

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(dsReport);

                    dtList = dsReport.Tables[0];
                    dtTotals = dsReport.Tables.Count > 1 ? dsReport.Tables[1] : null;
                }

                // 🔹 MAP GRID DATA
                foreach (DataRow row in dtList.Rows)
                {
                    var model = new GetPurchaseListGridModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        SupplierName = Convert.ToString(row["SupplierName"]),
                        InvoiceDate_Format = Convert.ToString(row["InvoiceDate_Format"]),
                        InvoiceDate = Convert.ToString(row["InvoiceDate"]),
                        InvoiceNo = Convert.ToString(row["InvoiceNo"]),
                        PONo = Convert.ToString(row["PONo"]),

                        SubTotalAmount = row["SubTotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["SubTotalAmount"]),
                        Tax = row["Tax"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Tax"]),
                        OtherCharges = row["OtherCharges"] == DBNull.Value ? 0 : Convert.ToDecimal(row["OtherCharges"]),
                        Discount = row["Discount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Discount"]),
                        TotalAmount = row["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalAmount"]),

                        PaidAmount = row["PaidAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PaidAmount"]),
                        OutstandingAmount = row["OutstandingAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["OutstandingAmount"]),

                        CreatedBy = Convert.ToString(row["CreatedBy"]),
                        CreatedOn = Convert.ToString(row["CreatedOn"]),
                        ModifiedOn = Convert.ToString(row["ModifiedOn"]),
                        PaymentStatus = Convert.ToString(row["PaymentStatus"]),

                        IsUploadedScannedInvoice = Convert.ToBoolean(row["IsUploadedScannedInvoice"]),
                        IsDeleted = Convert.ToBoolean(row["IsDeleted"])
                    };

                    list.Add(model);
                }

                // 🔹 MAP TOTALS
                if (dtTotals != null && dtTotals.Rows.Count > 0)
                {
                    DataRow row = dtTotals.Rows[0];
                    totals = new GetPurchaseListGridTotals
                    {
                        TotalRecords = row["TotalRecords"] == DBNull.Value ? 0 : Convert.ToInt32(row["TotalRecords"]),
                        TotalSubTotalAmount = row["TotalSubTotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalSubTotalAmount"]),
                        TotalTax = row["TotalTax"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalTax"]),
                        TotalOtherCharges = row["TotalOtherCharges"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalOtherCharges"]),
                        TotalDiscount = row["TotalDiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalDiscount"]),
                        TotalAmount = row["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalAmount"]),
                        TotalPaidAmount = row["TotalPaidAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalPaidAmount"]),
                        TotalOutstandingAmount = row["TotalOutstandingAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalOutstandingAmount"])
                    };
                }

                // 🔹 FINAL RESPONSE
                apiResponse.message = "Purchase grid fetched successfully!";
                apiResponse.data = new
                {
                    List = list,
                    Totals = totals
                };
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
        [Route("SavePurchase")]
        public async Task<IActionResult> SavePurchase([FromBody] SavePurchaseRequest model)
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

                    apiResponse.message = "Purchase Saved!";
                    apiResponse.data = new { PurchaseId = rowid };
                    apiResponse.status = "Success";
                    // 🔔 Notify only on SUCCESS
                    if (apiResponse.status == "Success")
                    {
                        await _purchaseHub.Clients.All.SendAsync("OnPurchaseChanged");
                    }

                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("DeletePurchase")]
        public async Task<IActionResult> DeletePurchase([FromBody] DeletePurchaseRequest request)
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

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int status = Convert.ToInt32(dr["Status"]);
                            string message = Convert.ToString(dr["Message"]);

                            if (status == 0)
                            {
                                apiResponse.status = "Error";
                                apiResponse.message = message;
                                return BadRequest(apiResponse);
                            }

                            apiResponse.status = "Success";
                            apiResponse.message = message;
                        }
                    }
                }

                // 🔔 Notify only if SUCCESS
                await _purchaseHub.Clients.All.SendAsync("OnPurchaseChanged");

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
                apiResponse.message = ex.Message;
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
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        //Widgets
        [HttpPost]
        [Route("GetPurchaseList_Widgets")]
        public IActionResult GetPurchaseList_Widgets([FromBody] PurchaseWidget_Request request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                PurchaseWidget_Response widgetData = null;

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPurchaseList_Widgets", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EventFrom", request.EventFrom ?? "");
                    cmd.Parameters.AddWithValue("@EventTo", request.EventTo ?? "");
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration ?? "");

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())   // Only one row expected
                        {
                            widgetData = new PurchaseWidget_Response
                            {
                                TotalOrders = Convert.ToString(dr["TotalOrders"]),
                                TotalAmount = Convert.ToString(dr["TotalAmount"]),
                                TotalOutstandingAmount = Convert.ToString(dr["TotalOutstandingAmount"])
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
                apiResponse.message = ex.Message;
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
                            InvoiceDate = Convert.ToString(dr["InvoiceDate"]),
                            InvoiceDate_Format = Convert.ToString(dr["InvoiceDate_Format"]),

                            SupplierName = Convert.ToString(dr["SupplierName"]),

                            PaymentDate = Convert.ToString(dr["PaymentDate"]),
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
                response.message = "Payment success";
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

        [HttpPost]
        [Route("DownloadPurchaseList_GRID_Pdf")]
        public IActionResult DownloadPurchaseList_GRID_Pdf([FromBody] GetPurchaseListGridReport_Request request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // Company
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // 🔹 REPORT (GRID PDF/EXCEL)
                    cmd = new SqlCommand("GetPurchaseList_GRID_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 🔹 Columns JSON
                    string? columnsJson = request.Columns != null ? JsonConvert.SerializeObject(request.Columns) : null;

                    // 🔹 PARAMETERS
                    cmd.Parameters.AddWithValue("@Columns",
                        columnsJson ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@PONo",
                        string.IsNullOrEmpty(request.PONo) ? (object)DBNull.Value : request.PONo);

                    cmd.Parameters.AddWithValue("@InvoiceNo",
                        string.IsNullOrEmpty(request.InvoiceNo) ? (object)DBNull.Value : request.InvoiceNo);

                    cmd.Parameters.AddWithValue("@filterSearchText",
                        string.IsNullOrEmpty(request.FilterSearchText) ? (object)DBNull.Value : request.FilterSearchText);

                    cmd.Parameters.AddWithValue("@SupplierId",
                        request.SupplierId == 0 ? (object)DBNull.Value : request.SupplierId);

                    cmd.Parameters.AddWithValue("@EventFrom",
                        string.IsNullOrEmpty(request.EventFrom) ? (object)DBNull.Value : request.EventFrom);

                    cmd.Parameters.AddWithValue("@EventTo",
                        string.IsNullOrEmpty(request.EventTo) ? (object)DBNull.Value : request.EventTo);

                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    cmd.Parameters.AddWithValue("@StatusId",
                        request.StatusId == 0 ? (object)DBNull.Value : request.StatusId);

                    // 🔹 EXECUTE
                    dsReport = new DataSet();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dsReport);
                    }

                }

                // ================= ASSIGN & VALIDATION =================
                #region ASSIGN & VALIDATIONS
                string propertyName = string.Empty, companyName = string.Empty, companyAddress = string.Empty,
                       companyphoneNo = string.Empty, companygstin = "", companyemail = "",
                       logoFile = string.Empty, userName = string.Empty,
                       stateCode = string.Empty, logoFullPath = string.Empty, generatedDate = "";

                bool isCompanyAvailable = dtCompany != null && dtCompany.Rows.Count > 0;
                if (isCompanyAvailable)
                {
                    userName = dtCompany.Rows[0]["UserName"]?.ToString() ?? "";
                    generatedDate = dtCompany.Rows[0]["GeneratedDate"]?.ToString() ?? "";
                }

                if (request.WithHeader ?? false)
                {
                    if (isCompanyAvailable)
                    {
                        propertyName = dtCompany.Rows[0]["PropertyName"]?.ToString() ?? ""; companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
                        companyAddress = dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "";
                        companyphoneNo = dtCompany.Rows[0]["CompanyMobile"]?.ToString() ?? "";
                        logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? "";
                        companygstin = dtCompany.Rows[0]["CompanyGST"]?.ToString();
                        companyemail = dtCompany.Rows[0]["CompanyEmail"]?.ToString();
                        stateCode = dtCompany.Rows[0]["StateCode"]?.ToString() ?? ""; // adjust column name if needed

                        // ── Resolve logo path (same logic as before, just storing the path string) ──
                        if (!string.IsNullOrWhiteSpace(logoFile))
                        {
                            string p1 = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "CompanyInvoice_CompanyLogo_Image", logoFile);
                            if (System.IO.File.Exists(p1))
                                logoFullPath = p1;
                            else
                            {
                                string p2 = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);
                                if (System.IO.File.Exists(p2))
                                    logoFullPath = p2;
                            }
                        }
                    }
                    else
                    {
                        companyName = "[PAGE HEADER] Company Data Not Found";
                    }
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data is incomplete. Required tables are missing.");

                // ================= EXTRACT DATA =================
                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                float topMargin = (request.WithHeader ?? false) ? 80f : 36f;

                using var stream = new MemoryStream();

                bool isA4size = true; // default to A4
                Document document = null;
                if (dsReport.Tables[2].Columns.Count > 7)
                {
                    document = new Document(PageSize.A4.Rotate(), 16, 16, topMargin, 50);
                    isA4size = false;
                }
                else
                {
                    document = new Document(PageSize.A4, 16, 16, topMargin, 50);
                    isA4size = true;
                }

                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                #endregion

                // ── Attach StandardReport header-footer ──
                #region HEADER_FOOTER
                if (request.WithHeader ?? false)
                {
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4(
                            propertyName, companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companyemail,
                            stateCode,
                            logoFullPath,
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            propertyName, companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companyemail,
                            stateCode,
                            logoFullPath,
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }
                else
                {
                    // no company header, but still want page number / generated-by footer
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4(
                            "", "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            "", "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }
                #endregion

                document.Open();

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                // ── Report title ──
                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                // ── Data table ──
                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string text = row[col]?.ToString() ?? "-";

                        int alignment = col.ColumnName switch
                        {
                            // Center align (dates / small flags)
                            "Sr No" => Element.ALIGN_CENTER,
                            "Invoice Date" => Element.ALIGN_CENTER,
                            "Invoice No" => Element.ALIGN_CENTER,
                            "PO No" => Element.ALIGN_CENTER,
                            "Status" => Element.ALIGN_CENTER,

                            // Right align (numeric / currency fields)
                            "SubTotal" => Element.ALIGN_RIGHT,
                            "Tax" => Element.ALIGN_RIGHT,
                            "Other Charges" => Element.ALIGN_RIGHT,
                            "Discount" => Element.ALIGN_RIGHT,
                            "Total" => Element.ALIGN_RIGHT,
                            "Paid" => Element.ALIGN_RIGHT,
                            "Outstanding" => Element.ALIGN_RIGHT,

                            // Default fallback
                            _ => Element.ALIGN_LEFT
                        };

                        table.AddCell(Utils.CreateBodyCell(text, tdFont, alignment));
                    }
                }


                // ── Totals row ──
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = col.ColumnName switch
                        {
                            "SubTotal" => $"{totalRow["TotalSubTotal"]}",
                            "Tax" => $"{totalRow["TotalTax"]}",
                            "Other Charges" => $"{totalRow["TotalOther"]}",
                            "Discount" => $"{totalRow["TotalDiscount"]}",
                            "Total" => $"{totalRow["TotalAmount"]}",
                            "Paid" => $"{totalRow["TotalPaid"]}",
                            "Outstanding" => $"{totalRow["TotalOutstanding"]}",
                            _ => col.Ordinal == 0 ? "TOTAL" : ""
                        };

                        table.AddCell(new PdfPCell(new Phrase(value, totalFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            BorderColor = BaseColor.LIGHT_GRAY,
                            Padding = 6
                        });
                    }
                }

                document.Add(table);
                document.Close();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Purchase_Report_*.pdf");

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{domainUrl}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("DownloadPurchaseList_GRID_Excel")]
        public IActionResult DownloadPurchaseList_GRID_Excel([FromBody] GetPurchaseListGridReport_Request request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 REPORT (GRID PDF/EXCEL)
                    SqlCommand cmd = new SqlCommand("GetPurchaseList_GRID_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 🔹 Columns JSON
                    string? columnsJson = request.Columns != null ? JsonConvert.SerializeObject(request.Columns) : null;

                    // 🔹 PARAMETERS
                    cmd.Parameters.AddWithValue("@Columns",
                        columnsJson ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@PONo",
                        string.IsNullOrEmpty(request.PONo) ? (object)DBNull.Value : request.PONo);

                    cmd.Parameters.AddWithValue("@InvoiceNo",
                        string.IsNullOrEmpty(request.InvoiceNo) ? (object)DBNull.Value : request.InvoiceNo);

                    cmd.Parameters.AddWithValue("@filterSearchText",
                        string.IsNullOrEmpty(request.FilterSearchText) ? (object)DBNull.Value : request.FilterSearchText);

                    cmd.Parameters.AddWithValue("@SupplierId",
                        request.SupplierId == 0 ? (object)DBNull.Value : request.SupplierId);

                    cmd.Parameters.AddWithValue("@EventFrom",
                        string.IsNullOrEmpty(request.EventFrom) ? (object)DBNull.Value : request.EventFrom);

                    cmd.Parameters.AddWithValue("@EventTo",
                        string.IsNullOrEmpty(request.EventTo) ? (object)DBNull.Value : request.EventTo);

                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    cmd.Parameters.AddWithValue("@StatusId",
                        request.StatusId == 0 ? (object)DBNull.Value : request.StatusId);

                    // 🔹 EXECUTE
                    dsReport = new DataSet();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dsReport);
                    }
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Purchase Report");

                // ── Headers ──
                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // ── Data ──
                int row = 2;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    row++;
                }

                // ── Totals row ──
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string? value = columnName switch
                        {
                            "SubTotal" => $"{totalRow["TotalSubTotal"]}",
                            "Tax" => $"{totalRow["TotalTax"]}",
                            "Other Charges" => $"{totalRow["TotalOther"]}",
                            "Discount" => $"{totalRow["TotalDiscount"]}",
                            "Total" => $"{totalRow["TotalAmount"]}",
                            "Paid" => $"{totalRow["TotalPaid"]}",
                            "Outstanding" => $"{totalRow["TotalOutstanding"]}",
                            //"Grand Total" => totalRow["TotalGrandTotal"].ToString(),
                            _ => i == 0 ? "TOTAL" : ""
                        };

                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                }

                worksheet.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Purchase_Report_*.xlsx");

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);
                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Purchase Report Excel generated successfully.";
                response.data = new { FileUrl = $"{domainUrl}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

    }

}
