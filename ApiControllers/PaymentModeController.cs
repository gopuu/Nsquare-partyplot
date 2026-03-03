using DocumentFormat.OpenXml.EMMA;
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

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentModeController : MasterController
    {
        //private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl;
        private string connString = string.Empty;
        public PaymentModeController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_imageUploadService = imageUploadService;
            //var request = httpContextAccessor.HttpContext?.Request;
            //domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetPaymentMode")]
        public IActionResult GetPaymentMode([FromBody] GetPaymentMode_Request request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetPaymentMode> list = new List<GetPaymentMode>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPaymentModeList", con))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id;
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GetPaymentMode model = new GetPaymentMode
                        {
                            Id = Convert.ToInt64(row["Id"]),
                            PaymentModeName = Convert.ToString(row["PaymentModeName"]),
                            Description = Convert.ToString(row["Description"]),
                            IsActive = Convert.ToBoolean(row["IsActive"]),
                        };

                        list.Add(model);
                    }
                }

                apiResponse.message = "Payment modes fetched successfully!";
                apiResponse.data = list;
                apiResponse.status = "Success";
                return Ok(apiResponse);
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

        [HttpPost]
        [Route("SavePaymentMode")]
        public IActionResult SavePaymentMode([FromBody] SavePaymentMode model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SavePaymentMode", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@PaymentModeName", SqlDbType.NVarChar).Value = model.PaymentModeName;
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = (object)model.Description ?? DBNull.Value;
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = model.Id > 0 ? "Payment mode updated successfully!" : "Payment mode added successfully!";
                apiResponse.status = "Success";
                apiResponse.data = null;

                return Ok(apiResponse);
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

        [HttpPost]
        [Route("DeletePaymentMode")]
        public IActionResult DeletePaymentMode([FromBody] DeletePaymentMode request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeletePaymentMode", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = "Payment mode deleted successfully!";
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
