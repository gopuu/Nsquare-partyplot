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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string conn, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                #region VALIDATION

                if (model.Id == 0 && string.IsNullOrWhiteSpace(model.FilePath))
                    return BadRequest(new apiResponse { status = "Error", message = "Please select an attachment." });

                if (!string.IsNullOrWhiteSpace(model.FilePath))
                {
                    try
                    {
                        _imageUploadService.ValidateBase64File(
                            model.FilePath,
                            new[] { ".pdf", ".jpg", ".jpeg", ".png" },
                            1
                        );
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new apiResponse
                        {
                            status = "Error",
                            message = ex.Message
                        });
                    }
                }

                #endregion

                long attachmentId;

                using (SqlConnection con = new SqlConnection(conn))
                {
                    con.Open();

                    #region SAVE ATTACHMENT

                    using (SqlCommand cmd = new SqlCommand("SaveSupportTicketAttachment", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Id", model.Id);
                        cmd.Parameters.AddWithValue("@TicketId", model.TicketId);
                        cmd.Parameters.AddWithValue("@FileName", (object?)model.FileName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                        attachmentId = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    #endregion

                    #region FILE UPLOAD

                    if (!string.IsNullOrWhiteSpace(model.FilePath))
                    {
                        string fileType = _imageUploadService.GetFileTypeFromBase64(model.FilePath);
                        string folderPath = $"org/{domain}/SupportTicket/Ticket_{model.TicketId}/";

                        _imageUploadService.DeleteImages(
                            folderPath,
                            $"Ticket_{attachmentId}"
                        );

                        string fileName = _imageUploadService.UploadImage(
                            model.FilePath,
                            folderPath,
                            $"Ticket_{attachmentId}",
                            fileType
                        );

                        using SqlCommand fileCmd = new SqlCommand("Save_DocumentFile", con);
                        fileCmd.CommandType = CommandType.StoredProcedure;
                        fileCmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = attachmentId;
                        fileCmd.Parameters.Add("@DocumentName", SqlDbType.NVarChar, -1).Value = fileName;
                        fileCmd.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "SupportTicketAttachment";

                        fileCmd.ExecuteNonQuery();
                    }

                    #endregion
                }

                response.status = "Success";
                response.message = model.Id > 0
                    ? "Attachment updated successfully!"
                    : "Attachment uploaded successfully!";
                response.data = new { AttachmentId = attachmentId };

                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                response.status = "Error";
                response.message = ex.Message;

                return Ok(response); // Keep your existing ALWAYS 200 behavior
            }
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
