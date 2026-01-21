using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketAttachmentController : MasterController
    {

        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly ImageUploadService _imageUploadService;

        public SupportTicketAttachmentController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _imageUploadService = imageUploadService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }


        [HttpPost]
        [Route("GetSupportTicketAttachmentList")]
        public IActionResult GetSupportTicketAttachmentList([FromBody] GetSupportTicketAttachmentRequest request)
        {
            apiResponse response = new apiResponse();
            List<SupportTicketAttachment> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string conn, out string domain, out IActionResult error))
                    return Ok(error);

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(conn))
                using (SqlCommand cmd = new SqlCommand("GetSupportTicketAttachment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TicketId", request.TicketId);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new SupportTicketAttachment
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        TicketId = Convert.ToInt64(row["TicketId"]),
                        FileName = Convert.ToString(row["FileName"]),
                        TicketDisplay = Convert.ToString(row["TicketDisplay"]),

                        FilePath = row["FilePath"] != DBNull.Value
                            && !string.IsNullOrEmpty(row["FilePath"].ToString())
                            ? $"{domainUrl}/uploads/org/{domain}/SupportTicket/Ticket_{row["TicketId"]}/{row["FilePath"]}"
                            : string.Empty
                    });
                }
                response.status = "Success";
                response.message = "Ticket attachments fetched successfully!";
                response.data = list;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response); // ✅ ALWAYS 200
        }


        [HttpPost]
        [Route("SaveSupportTicketAttachment")]
        public IActionResult SaveSupportTicketAttachment([FromBody] SaveSupportTicketAttachmentRequest model)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string conn, out string domain, out IActionResult error))
                    return Ok(error);

                long attachmentId = 0;

                using (SqlConnection con = new SqlConnection(conn))
                using (SqlCommand cmd = new SqlCommand("SaveSupportTicketAttachment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@TicketId", model.TicketId);
                    cmd.Parameters.AddWithValue("@FileName", model.FileName);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    attachmentId = Convert.ToInt64(cmd.ExecuteScalar());

                    string latestImagePath = string.Empty;
                    //photo upload
                    if (model.FilePath != string.Empty && model.FilePath != null)
                    {
                        string imgType = _imageUploadService.GetFileTypeFromBase64(model.FilePath);

                        string folderPath = $"org/{domain}/SupportTicket/Ticket_{model.TicketId}/"; // Updated to include domain
                        _imageUploadService.DeleteImages(folderPath, $"Ticket_{attachmentId.ToString()}");
                        model.FilePath = _imageUploadService.UploadImage(model.FilePath, folderPath, $"Ticket_{attachmentId.ToString()}", imgType);

                        //update table
                        SqlCommand cmd2 = new SqlCommand("Save_DocumentFile", con);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@Id", SqlDbType.BigInt).Value = attachmentId;
                        cmd2.Parameters.Add("@DocumentName", SqlDbType.NVarChar, -1).Value = model.FilePath; 
                        cmd2.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "SupportTicketAttachment";
                        cmd2.ExecuteNonQuery();

                        latestImagePath = $"{domainUrl}/uploads/org/{domain}/SupportTicket/Ticket_{model.TicketId}/{model.FilePath}";
                    }
                }

                response.status = "Success";
                response.message = model.Id > 0
                    ? "Attachment updated successfully!"
                    : "Attachment uploaded successfully!";
                response.data = new { AttachmentId = attachmentId };
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response); // ✅ ALWAYS 200
        }


        [HttpPost]
        [Route("DeleteSupportTicketAttachment")]
        public IActionResult DeleteSupportTicketAttachment([FromBody] DeleteSupportTicketAttachmentRequest model)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string conn, out string domain, out IActionResult error))
                    return Ok(error);

                using (SqlConnection con = new SqlConnection(conn))
                using (SqlCommand cmd = new SqlCommand("DeleteSupportTicketAttachment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                response.status = "Success";
                response.message = "Attachment deleted successfully!";
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                response.status = "Error";
                response.message = ex.Message;
            }

            return Ok(response); // ✅ ALWAYS 200
        }


    }
}
