using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using System.Xml.Linq;
using ExcelDataReader.Log;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Company_InvoiceSetupController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_InvoiceSetupController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";

            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpGet]
        [Route("GetCompanyInvoiceSetup")]
        public IActionResult GetCompanyInvoiceSetup()
        {
            var apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                List<dynamic> invoiceSetupList = new();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyInvoiceSetup", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object?)request.Id ?? DBNull.Value;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoiceSetupList.Add(new
                            {
                                Id = reader["id"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["id"]),
                                GstInvoiceStartNo = reader["gst_invoice_start_no"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["gst_invoice_start_no"]),
                                GstInvoicePrefix = reader["gst_invoice_prefix"] == DBNull.Value ? null : reader["gst_invoice_prefix"].ToString(),

                                ProformaInvoiceStartNo = reader["proforma_invoice_start_no"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["proforma_invoice_start_no"]),
                                ProformaInvoicePrefix = reader["proforma_invoice_prefix"] == DBNull.Value ? null : reader["proforma_invoice_prefix"].ToString(),

                                ReceiptPrefix = reader["receipt_prefix"] == DBNull.Value ? null : reader["receipt_prefix"].ToString(),

                                // ✅ Boolean fields
                                PrintLogoProspectus = reader["print_logo_prospectus"] == DBNull.Value ? false : Convert.ToBoolean(reader["print_logo_prospectus"]),
                                PrintLogoQuotation = reader["print_logo_quotation"] == DBNull.Value ? false : Convert.ToBoolean(reader["print_logo_quotation"]),
                                PrintLogoReceipt = reader["print_logo_receipt"] == DBNull.Value ? false : Convert.ToBoolean(reader["print_logo_receipt"]),
                                IncludeFyInvoiceNo = reader["include_fy_invoice_no"] == DBNull.Value ? false : Convert.ToBoolean(reader["include_fy_invoice_no"]),
                                UseCustomReceiptNo = reader["use_custom_receipt_no"] == DBNull.Value ? false : Convert.ToBoolean(reader["use_custom_receipt_no"]),
                                PrintFpPaymentTrail = reader["print_fp_payment_trail"] == DBNull.Value ? false : Convert.ToBoolean(reader["print_fp_payment_trail"]),
                                PrintBankDetails = reader["print_bank_details"] == DBNull.Value ? false : Convert.ToBoolean(reader["print_bank_details"]),

                                // ✅ Int64 fields
                                CustomReceiptStartNo = reader["custom_receipt_start_no"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["custom_receipt_start_no"]),
                                SignatoryLabel = reader["signatory_label"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["signatory_label"]),

                                // ✅ String (varchar/nvarchar) fields
                                BankName = reader["bank_name"] == DBNull.Value ? string.Empty : Convert.ToString(reader["bank_name"]),
                                AccountNo = reader["account_no"] == DBNull.Value ? string.Empty : Convert.ToString(reader["account_no"]),
                                BranchDetails = reader["branch_details"] == DBNull.Value ? string.Empty : Convert.ToString(reader["branch_details"]),
                                IFSCCode = reader["ifsc_code"] == DBNull.Value ? string.Empty : Convert.ToString(reader["ifsc_code"]),
                                UpiId = reader["upi_id"] == DBNull.Value ? string.Empty : Convert.ToString(reader["upi_id"]),

                                CompanyLogo = reader["company_logo"] != DBNull.Value
                                                        && !string.IsNullOrEmpty(reader["company_logo"].ToString())
                                                    ? domainUrl + $"/uploads/org/{domain}/CompanyInvoice_CompanyLogo_Image/" + reader["company_logo"].ToString()
                                                    : string.Empty,

                            });
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Invoice setup fetched successfully.";
                apiResponse.data = invoiceSetupList;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null ?? "");
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyInvoiceSetup")]
        public IActionResult SaveCompanyInvoiceSetup([FromBody] SaveInvoiceSetupRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                #region FILE VALIDATION

                if (!string.IsNullOrWhiteSpace(model.CompanyLogo))
                {
                    try
                    {
                        _imageUploadService.ValidateBase64File(
                            model.CompanyLogo,
                            new[] { ".jpg", ".jpeg", ".png" },
                            1
                        );
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new apiResponse
                        {
                            status = "Error",
                            message = $"Company logo: {ex.Message}"
                        });
                    }
                }

                #endregion

                long rowId = 0;
                string latestImagePath = string.Empty;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    #region SAVE INVOICE SETUP

                    using (SqlCommand cmd = new SqlCommand("SaveCompanyInvoiceSetup", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = (object?)model.Id ?? 0;

                        cmd.Parameters.Add("@gst_invoice_start_no", SqlDbType.BigInt).Value = (object?)model.GstInvoiceStartNo ?? 0;
                        cmd.Parameters.Add("@gst_invoice_prefix", SqlDbType.NVarChar, 50).Value = (object?)model.GstInvoicePrefix ?? string.Empty;

                        cmd.Parameters.Add("@proforma_invoice_start_no", SqlDbType.BigInt).Value = (object?)model.ProformaInvoiceStartNo ?? 0;
                        cmd.Parameters.Add("@proforma_invoice_prefix", SqlDbType.NVarChar, 50).Value = (object?)model.ProformaInvoicePrefix ?? string.Empty;

                        cmd.Parameters.Add("@receipt_prefix", SqlDbType.NVarChar, 50).Value = (object?)model.ReceiptPrefix ?? string.Empty;

                        cmd.Parameters.Add("@print_logo_prospectus", SqlDbType.Bit).Value = model.PrintLogoProspectus;
                        cmd.Parameters.Add("@print_logo_quotation", SqlDbType.Bit).Value = model.PrintLogoQuotation;
                        cmd.Parameters.Add("@print_logo_receipt", SqlDbType.Bit).Value = model.PrintLogoReceipt;
                        cmd.Parameters.Add("@include_fy_invoice_no", SqlDbType.Bit).Value = model.IncludeFyInvoiceNo;

                        cmd.Parameters.Add("@use_custom_receipt_no", SqlDbType.Bit).Value = model.UseCustomReceiptNo;
                        cmd.Parameters.Add("@custom_receipt_start_no", SqlDbType.BigInt).Value = (object?)model.CustomReceiptStartNo ?? 0;

                        cmd.Parameters.Add("@print_fp_payment_trail", SqlDbType.Bit).Value = model.PrintFpPaymentTrail;
                        cmd.Parameters.Add("@signatory_label", SqlDbType.BigInt).Value = (object?)model.SignatoryLabel ?? 0;
                        cmd.Parameters.Add("@print_bank_details", SqlDbType.Bit).Value = model.PrintBankDetails;

                        cmd.Parameters.Add("@bank_name", SqlDbType.NVarChar, 100).Value = (object?)model.BankName ?? string.Empty;
                        cmd.Parameters.Add("@account_no", SqlDbType.NVarChar, 50).Value = (object?)model.AccountNo ?? string.Empty;
                        cmd.Parameters.Add("@branch_details", SqlDbType.NVarChar, 100).Value = (object?)model.BranchDetails ?? string.Empty;
                        cmd.Parameters.Add("@ifsc_code", SqlDbType.NVarChar, 20).Value = (object?)model.IFSCCode ?? string.Empty;
                        cmd.Parameters.Add("@upi_id", SqlDbType.NVarChar, 100).Value = (object?)model.UpiId ?? string.Empty;

                        cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = (object?)model.ActionBy ?? 0;

                        rowId = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    #endregion

                    #region COMPANY LOGO UPLOAD

                    if (!string.IsNullOrWhiteSpace(model.CompanyLogo))
                    {
                        string fileType = _imageUploadService.GetFileTypeFromBase64(model.CompanyLogo);
                        string folderPath = $"org/{domain}/CompanyInvoice_CompanyLogo_Image/";
                        string filePrefix = rowId.ToString();

                        _imageUploadService.DeleteImages(folderPath, filePrefix);

                        string fileName = _imageUploadService.UploadImage(
                            model.CompanyLogo,
                            folderPath,
                            filePrefix,
                            fileType
                        );

                        using (SqlCommand fileCmd = new SqlCommand("SaveCompanyInvoiceLogo", con))
                        {
                            fileCmd.CommandType = CommandType.StoredProcedure;

                            fileCmd.Parameters.Add("@id", SqlDbType.BigInt).Value = rowId;
                            fileCmd.Parameters.Add("@company_logo", SqlDbType.NVarChar, -1).Value = fileName;

                            fileCmd.ExecuteNonQuery();
                        }

                        model.CompanyLogo = fileName;

                        latestImagePath = $"{domainUrl}/uploads/org/{domain}/CompanyInvoice_CompanyLogo_Image/{fileName}";
                    }

                    #endregion
                }

                apiResponse.status = "Success";
                apiResponse.message = (model.Id ?? 0) > 0
                    ? "Invoice setup updated successfully."
                    : "Invoice setup created successfully.";

                apiResponse.data = new
                {
                    Id = rowId,
                    CompanyLogo = latestImagePath
                };

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

        [HttpGet]
        [Route("GetCompanyInvoiceNumbers")]
        public IActionResult GetCompanyInvoiceNumbers()
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                        out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCompanyInvoiceNumbers", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var result = new
                            {
                                GSTInvoiceNumber = reader["GSTInvoiceNumber"].ToString(),
                                ProformaInvoiceNumber = reader["ProformaInvoiceNumber"].ToString(),
                                ReceiptPrefix = reader["ReceiptPrefix"].ToString()
                            };

                            apiResponse.status = "Success";
                            apiResponse.message = "Invoice numbers fetched successfully.";
                            apiResponse.data = result;

                            return Ok(apiResponse);
                        }
                    }
                }

                apiResponse.status = "Error";
                apiResponse.message = "No record found.";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return Ok(apiResponse);
            }
        }

    }
}
//public class GetInvoiceSetupRequest
//{
//    public long? Id { get; set; }
//}
public class SaveInvoiceSetupRequest
{
    public long? Id { get; set; }
    public long? GstInvoiceStartNo { get; set; }
    public string? GstInvoicePrefix { get; set; }
    public long? ProformaInvoiceStartNo { get; set; }
    public string? ProformaInvoicePrefix { get; set; }
    public string? ReceiptPrefix { get; set; }
    public bool PrintLogoProspectus { get; set; }
    public bool PrintLogoQuotation { get; set; }
    public bool PrintLogoReceipt { get; set; }
    public bool IncludeFyInvoiceNo { get; set; }
    public bool UseCustomReceiptNo { get; set; }
    public long? CustomReceiptStartNo { get; set; }
    public bool PrintFpPaymentTrail { get; set; }
    public long? SignatoryLabel { get; set; }
    public bool PrintBankDetails { get; set; }
    public string? BankName { get; set; }
    public string? AccountNo { get; set; }
    public string? BranchDetails { get; set; }
    public string? IFSCCode { get; set; }
    public string? UpiId { get; set; }
    public string? CompanyLogo { get; set; }
    public long? ActionBy { get; set; }
}
