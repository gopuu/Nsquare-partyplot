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

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : MasterController
    {
        private readonly ImageUploadService _imageUploadService;
        private readonly string domainUrl;
        private string connString = string.Empty;

        public SupplierController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _imageUploadService = imageUploadService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        // ------------------------------------------------------------
        // 1. GET Supplier List
        // ------------------------------------------------------------
        [HttpPost("GetSupplierList")]
        public IActionResult GetSupplierList([FromBody] GetSupplierRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetSupplier> suppliers = new List<GetSupplier>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSupplierList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id ?? 0);

                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        GetSupplier supplier = new GetSupplier();
                        supplier.Id = Convert.ToInt64(row["Id"]);
                        supplier.Name = Convert.ToString(row["Name"]);
                        supplier.CompanyName = Convert.ToString(row["CompanyName"]);
                        supplier.Email = Convert.ToString(row["Email"]);
                        supplier.Phone = Convert.ToString(row["Phone"]);
                        supplier.GSTNo = Convert.ToString(row["GSTNo"]);
                        supplier.RegisterAddress = Convert.ToString(row["RegisterAddress"]);
                        supplier.RegisterStateId = Convert.ToInt64(row["RegisterStateId"]);
                        supplier.RegisterCity = Convert.ToInt64(row["RegisterCity"]);
                        supplier.RegisterPincode = Convert.ToString(row["RegisterPincode"]);
                        supplier.ShippingAddress = Convert.ToString(row["ShippingAddress"]);
                        supplier.FSSAI_LicNo = Convert.ToString(row["FSSAI_LicNo"]);
                        supplier.PAN_No = Convert.ToString(row["PAN_No"]);
                        supplier.MSME_No = Convert.ToString(row["MSME_No"]);
                        supplier.TAN_No = Convert.ToString(row["TAN_No"]);
                        supplier.CIN_No = Convert.ToString(row["CIN_No"]);
                        supplier.TCS_Percent = row["TCS_Percent"] != DBNull.Value ? Convert.ToDouble(row["TCS_Percent"]) : 0;
                        supplier.SupplierType = row["SupplierType"] != DBNull.Value ? Convert.ToInt64(row["SupplierType"]) : 0;
                        supplier.Documents = Convert.ToString(row["Documents"]);

                        supplier.Documents = row["Documents"] != DBNull.Value
                                && !string.IsNullOrEmpty(row["Documents"].ToString())
                            ? domainUrl + "/uploads/org/" + domain + "/supplierDocuments/" + row["Documents"].ToString()
                            : string.Empty;

                        supplier.IsActive = Convert.ToBoolean(row["IsActive"]);

                        suppliers.Add(supplier);
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Supplier list fetched successfully.";
                apiResponse.data = suppliers;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                apiResponse.status = "Error";
                apiResponse.message = "Error: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        // ------------------------------------------------------------
        // 2. SAVE / UPDATE Supplier
        // ------------------------------------------------------------
        [HttpPost("SaveSupplier")]
        public IActionResult SaveSupplier([FromBody] SaveSupplier request)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveSupplier", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@Name", request.Name);
                    cmd.Parameters.AddWithValue("@CompanyName", (object?)request.CompanyName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GSTNo", (object?)request.GSTNo ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@RegisterAddress", (object?)request.RegisterAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RegisterStateId", (object?)request.RegisterStateId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RegisterCity", (object?)request.RegisterCity ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RegisterPincode", (object?)request.RegisterPincode ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ShippingAddress", (object?)request.ShippingAddress ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@FSSAI_LicNo", (object?)request.FSSAI_LicNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PAN_No", (object?)request.PAN_No ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MSME_No", (object?)request.MSME_No ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TAN_No", (object?)request.TAN_No ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CIN_No", (object?)request.CIN_No ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@TCS_Percent", (object?)request.TCS_Percent ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplierType", (object?)request.SupplierType ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@Documents", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", request.IsActive);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    conn.Open();

                    Int64 rowid = Convert.ToInt64(cmd.ExecuteScalar());
                    string latestImagePath = string.Empty;
                    //photo upload
                    if (request.Documents != string.Empty && request.Documents != null)
                    {
                        string imgType = _imageUploadService.GetFileTypeFromBase64(request.Documents);

                        //string imgType = ".jpg";
                        //get filename with fileType
                        //request.Documents = _imageUploadService.UploadImage(request.Documents, "employeePic", rowid.ToString(), imgType);

                        string folderPath = $"org/{domain}/supplierDocuments"; // Updated to include domain
                        _imageUploadService.DeleteImages(folderPath, rowid.ToString());
                        //model.Logo = _imageUploadService.UploadImage(model.Logo, folderPath, rowid.ToString(), imgType);
                        request.Documents = _imageUploadService.UploadImage(request.Documents, folderPath, rowid.ToString(), imgType);

                        //update table
                        SqlCommand cmd2 = new SqlCommand("Save_DocumentFile", conn);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@Id", SqlDbType.BigInt).Value = rowid;
                        cmd2.Parameters.Add("@DocumentName", SqlDbType.NVarChar, -1).Value = request.Documents;
                        cmd2.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "SupplierMaster";
                        cmd2.ExecuteNonQuery();

                        latestImagePath = domainUrl + "/uploads/org/" + domain + "/supplierDocuments/" + request.Documents;
                    }

                    apiResponse.status = "Success";
                    apiResponse.message = "Supplier saved successfully!";
                    apiResponse.data = new { SupplierId = rowid };
                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                apiResponse.status = "Error";
                apiResponse.message = "Error: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        // ------------------------------------------------------------
        // 3. DELETE Supplier (Soft Delete)
        // ------------------------------------------------------------
        [HttpPost("DeleteSupplier")]
        public IActionResult DeleteSupplier([FromBody] DeleteSupplier request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteSupplier", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Supplier deleted successfully!";
                apiResponse.data = null;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                apiResponse.status = "Error";
                apiResponse.message = "Error: " + ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

    }
}
