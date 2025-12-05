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
using Newtonsoft.Json;
using Spire.Pdf;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Dynamic;
using ZXing;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceTemplateMasterController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;


        public InvoiceTemplateMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            _env = env;
            //_imageUploadService = imageUploadService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}";
            // Force HTTPS if needed
            if (!domainUrl.Contains("localhost") && domainUrl.StartsWith("http://"))
            {
                domainUrl = domainUrl.Replace("http://", "https://");
            }

        }

        [HttpPost]
        [Route("SaveInvoiceTemplate_Selected")]
        public IActionResult SaveInvoiceTemplate_Selected([FromBody] UpdateTemplateSelectedRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveInvoiceTemplate_Selected", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TemplateId", model.TemplateId);
                    cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    apiResponse.status = "Success";
                    apiResponse.message = "Template selection updated.";
                }
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
            }

            return Ok(apiResponse);
        }


        [HttpPost]
        [Route("GetInvoiceTemplateMaster")]
        public IActionResult GetInvoiceTemplateMaster([FromBody] GetInvoiceTemplateMasterRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetInvoiceTemplateMaster", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@CategoryId", (object)model.CategoryId ?? DBNull.Value);
                    conn.Open();

                    List<InvoiceTemplateMasterModel> list = new List<InvoiceTemplateMasterModel>();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var obj = new InvoiceTemplateMasterModel
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                Title = dr["Title"].ToString(),
                                ImageName = dr["ImageName"] == DBNull.Value ? "" : dr["ImageName"].ToString(),
                                PdfName = dr["PdfName"] == DBNull.Value ? "" : dr["PdfName"].ToString(),
                                CategoryId = dr["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["CategoryId"]),
                                CategoryName = dr["CategoryName"] == DBNull.Value ? "" : dr["CategoryName"].ToString(),
                                MethodName = dr["MethodName"] == DBNull.Value ? "" : dr["MethodName"].ToString(),
                                Is_Selected = dr["Is_Selected"] == DBNull.Value ? false : Convert.ToBoolean(dr["Is_Selected"])
                            };

                            list.Add(obj);
                        }
                    }

                    // --------------------------------------------------------
                    // STEP 1: Check if existing file physically exists
                    // --------------------------------------------------------
                    string baseDir = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "invoiceTemplate");
                    if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

                    foreach (var item in list)
                    {
                        // ---- Check PDF File ----
                        if (!string.IsNullOrWhiteSpace(item.PdfName))
                        {
                            string pdfFileName = Path.GetFileName(item.PdfName);
                            string pdfFullPath = Path.Combine(baseDir, pdfFileName);

                            if (!System.IO.File.Exists(pdfFullPath))
                            {
                                item.PdfName = "";  // file missing → regenerate
                            }
                            else {
                                item.PdfName = $"{domainUrl}/uploads/org/{domain}/invoiceTemplate/{pdfFileName}";
                            }
                        }

                        // ---- Check Image File ----
                        if (!string.IsNullOrWhiteSpace(item.ImageName))
                        {
                            string imgFileName = Path.GetFileName(item.ImageName);
                            string imgFullPath = Path.Combine(baseDir, imgFileName);

                            if (!System.IO.File.Exists(imgFullPath))
                            {
                                item.ImageName = ""; // file missing → regenerate
                            }
                            else {
                                item.ImageName = $"{domainUrl}/uploads/org/{domain}/invoiceTemplate/{imgFileName}";
                            }
                        }
                    }

                    // --------------------------------------------------------
                    // STEP 2: Auto-generate PDF + PNG ONLY if missing
                    // --------------------------------------------------------
                    foreach (var item in list)
                    {
                        if (string.IsNullOrWhiteSpace(item.PdfName))
                        {
                            var result = GetPdfAndImageUrl(connString, domain, domainUrl, item.Id, item.MethodName);

                            item.PdfName = result.pdfUrl;
                            item.ImageName = result.imageUrl;

                            string pdfFileName = result.pdfFileName;
                            string imgFileName = result.imgFileName;
                            // OPTIONAL: Call a DB update SP here
                            SaveInvoiceTemplateMaster_Files(item.Id, pdfFileName, imgFileName, connString);
                        }
                    }

                    apiResponse.status = "Success";
                    apiResponse.data = list;
                }
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
            }

            return Ok(apiResponse);
        }

        private void SaveInvoiceTemplateMaster_Files(long templateId, string pdfFileName, string imgFileName, string connString)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveInvoiceTemplateMaster_Files", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TemplateId", templateId);
                cmd.Parameters.AddWithValue("@PdfName", pdfFileName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ImageName", imgFileName ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        //------------- PurchaseInvoiceDefault1 PDF (A4 Portrait) ---------------------
        private (string pdfUrl, string imageUrl, string pdfFileName, string imgFileName) GetPdfAndImageUrl(string connString, string domain, string domainUrl, long templateId, string methodName)
        {
            string baseDir = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "invoiceTemplate");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            Utils.deleteOldFiles(baseDir, $"InvoiceTemplate_{templateId}*.*");

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string pdfFileName = $"InvoiceTemplate_{templateId}_{timestamp}.pdf";
            string imgFileName = $"InvoiceTemplate_{templateId}_{timestamp}.png";

            string pdfFullPath = Path.Combine(baseDir, pdfFileName);
            string imgFullPath = Path.Combine(baseDir, imgFileName);

            // ⬅ CALL TEMPLATE BY METHOD NAME
            ExecuteTemplateMethod(methodName, pdfFullPath);

            // Convert PDF → Image
            ConvertPdfToImage(pdfFullPath, imgFullPath);

            string pdfUrl = $"{domainUrl}/uploads/org/{domain}/invoiceTemplate/{pdfFileName}";
            string imgUrl = $"{domainUrl}/uploads/org/{domain}/invoiceTemplate/{imgFileName}";

            return (pdfUrl, imgUrl, pdfFileName, imgFileName);
        }

        private void ExecuteTemplateMethod(string methodName, string pdfFullPath)
        {
            var builder = new InvoiceTemplateBuilder(_env);

            switch (methodName)
            {
                case "Generate_PurchaseInvoiceDefault1_A4_Portrait":
                    builder.Generate_PurchaseInvoiceDefault1_A4_Portrait(pdfFullPath);
                    break;

                case "Generate_PurchaseInvoiceDefault1_A4_Landscape":
                    builder.Generate_PurchaseInvoiceDefault1_A4_Landscape(pdfFullPath);
                    break;

                default:
                    throw new Exception("Unknown MethodName: " + methodName);
            }
        }

        private void ConvertPdfToImage(string pdfPath, string outputPath)
        {
            // Use fully qualified name to avoid collision
            Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
            pdf.LoadFromFile(pdfPath);

            // Convert 1st page → PNG
            System.Drawing.Image img = pdf.SaveAsImage(0,
                Spire.Pdf.Graphics.PdfImageType.Bitmap,
                300, 300);

            img.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);

            pdf.Close();
        }

    }
}
