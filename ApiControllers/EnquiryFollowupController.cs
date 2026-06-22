using DocumentFormat.OpenXml.Drawing.Spreadsheet;
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
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnquiryFollowupController : MasterController
    {
        private string connString = string.Empty;
        private readonly ImageUploadService _imageUploadService;
        private readonly string domainUrl;
        private readonly WhatsAppService _whatsAppService;

        public EnquiryFollowupController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, 
            ImageUploadService imageUploadService, WhatsAppService whatsAppService) 
            : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _imageUploadService = imageUploadService;
            _whatsAppService = whatsAppService;
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetEnquiryFollowUp")]
        public IActionResult GetEnquiryFollowUp([FromBody] GetEnquiryFollowUpRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EnquiryFollowUpResponse> list = new List<EnquiryFollowUpResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FollowUpId", SqlDbType.BigInt).Value =
                        request.FollowUpId == 0 ? 0 : request.FollowUpId;

                    cmd.Parameters.Add("@EnquiryId", SqlDbType.BigInt).Value =
                        request.EnquiryId == 0 ? 0 : request.EnquiryId;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                if (dt.Rows.Count == 0)
                {
                    apiResponse.status = "Success";
                    apiResponse.message = "No data found.";
                    apiResponse.data = null;
                    return Ok(apiResponse);
                }

                DataRow row = dt.Rows[0];

                // Extract JSON
                string inquiryJson = row["Inquiry"]?.ToString();
                string followUpsJson = row["FollowUps"]?.ToString();

                // Deserialize Inquiry
                EnquiryDetailsModel inquiry = !string.IsNullOrEmpty(inquiryJson)
                    ? JsonConvert.DeserializeObject<EnquiryDetailsModel>(inquiryJson)
                    : null;

                // Deserialize FollowUp list
                List<EnquiryFollowUpResponse> followUps = !string.IsNullOrEmpty(followUpsJson)
                    ? JsonConvert.DeserializeObject<List<EnquiryFollowUpResponse>>(followUpsJson)
                    : new List<EnquiryFollowUpResponse>();

                // Combined response
                FollowUpDataResponse output = new FollowUpDataResponse
                {
                    Enquiry = inquiry,
                    FollowUps = followUps
                };

                apiResponse.message = "Follow-up data fetched successfully!";
                apiResponse.status = "Success";
                apiResponse.data = output;
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
        [Route("SaveEnquiryFollowUp")]
        public IActionResult SaveEnquiryFollowUp([FromBody] SaveEnquiryFollowUpRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveEnquiryFollowUp", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FollowUpId", SqlDbType.BigInt).Value = model.FollowUpId > 0 ? model.FollowUpId : 0;
                    cmd.Parameters.Add("@EnquiryId", SqlDbType.BigInt).Value = model.EnquiryId;
                    cmd.Parameters.Add("@FollowUpTypeId", SqlDbType.BigInt).Value = model.FollowUpTypeId;
                    cmd.Parameters.Add("@FollowUpLeadLostReasonCategoryId", SqlDbType.BigInt).Value = model.FollowUpLeadLostReasonCategoryId;
                    cmd.Parameters.Add("@FollowUpLeadLostReasonId", SqlDbType.BigInt).Value = model.FollowUpLeadLostReasonId;

                    cmd.Parameters.Add("@FollowUpDateTime", SqlDbType.NVarChar, 30).Value = model.FollowUpDateTime ?? "";
                    cmd.Parameters.Add("@NextFollowUpDateTime", SqlDbType.NVarChar, 30).Value = model.NextFollowUpDateTime ?? "";

                    cmd.Parameters.Add("@StatusId", SqlDbType.BigInt).Value = model.StatusId;
                    cmd.Parameters.Add("@MoodId", SqlDbType.BigInt).Value = model.MoodId;
                    cmd.Parameters.Add("@ProbabilityId", SqlDbType.BigInt).Value = model.ProbabilityId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@OutcomeId", SqlDbType.BigInt).Value = model.OutcomeId ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@DiscussionNotes", SqlDbType.NVarChar).Value = (object)model.DiscussionNotes ?? DBNull.Value;
                    cmd.Parameters.Add("@InternalRemarks", SqlDbType.NVarChar).Value = (object)model.InternalRemarks ?? DBNull.Value;
                    cmd.Parameters.Add("@CustomerRemarks", SqlDbType.NVarChar).Value = (object)model.CustomerRemarks ?? DBNull.Value;

                    cmd.Parameters.Add("@AssignedTo", SqlDbType.BigInt).Value = model.AssignedTo;
                    cmd.Parameters.Add("@IsClosed", SqlDbType.Bit).Value = model.IsClosed;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    long followUpId = Convert.ToInt64(cmd.ExecuteScalar());

                    string latestImagePath = string.Empty;
                    //photo upload
                    if (model.FileUpload != string.Empty && model.FileUpload != null)
                    {
                        string imgType = _imageUploadService.GetFileTypeFromBase64(model.FileUpload);

                        string folderPath = $"/uploads/org/{domain}/EnquiryFollowUp/EnquiryDealId_{model.EnquiryId}"; // Updated to include domain
                        string fileName = $"FollowUpId_{followUpId}";
                        _imageUploadService.DeleteImages(folderPath, fileName);
                        model.FileUpload = _imageUploadService.UploadImage(model.FileUpload, folderPath, fileName, imgType);

                        //update table
                        SqlCommand cmd2 = new SqlCommand("SaveEnquiryFollowUp_FileUpload", con);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@id", SqlDbType.BigInt).Value = followUpId;
                        cmd2.Parameters.Add("@fileUploadName", SqlDbType.NVarChar, -1).Value = model.FileUpload; // Kept max for nvarchar(max)
                        cmd2.ExecuteNonQuery();

                        latestImagePath = $"{domainUrl}/uploads/org/{domain}/EnquiryFollowUp/EnquiryDealId_{model.EnquiryId}/{model.FileUpload}";
                    }
                    con.Close();
                }

                apiResponse.message = model.FollowUpId > 0
                    ? "Follow-up updated successfully!"
                    : "Follow-up inserted successfully!";

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
        [Route("DeleteEnquiryFollowUp")]
        public IActionResult DeleteEnquiryFollowUp([FromBody] DeleteEnquiryFollowUpRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteEnquiryFollowUp", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FollowUpId", SqlDbType.BigInt).Value = request.FollowUpId;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = request.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.message = "Follow-up deleted successfully!";
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

        #region FOLLOW REMAINDER
        [HttpPost]
        [Route("GetFollowUpReminderStatus")]
        public IActionResult GetFollowUpReminderStatus([FromBody] GetFollowUpReminderStatusRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                FollowUpReminderStatusResponse status = null;
                List<FollowUpReminderHistoryResponse> history = new();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetFollowUpReminderStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EnquiryId", request.EnquiryId);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // Resultset 1 : Status
                        if (dr.Read())
                        {
                            status = new FollowUpReminderStatusResponse
                            {
                                FollowUpStage = Convert.ToByte(dr["FollowUpStage"]),

                                Enable1st = Convert.ToBoolean(dr["Enable1st"]),
                                Enable2nd = Convert.ToBoolean(dr["Enable2nd"]),
                                Enable3rd = Convert.ToBoolean(dr["Enable3rd"]),

                                Is1stSent = Convert.ToBoolean(dr["Is1stSent"]),
                                Is2ndSent = Convert.ToBoolean(dr["Is2ndSent"]),
                                Is3rdSent = Convert.ToBoolean(dr["Is3rdSent"])
                            };
                        }

                        // Resultset 2 : History
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                history.Add(new FollowUpReminderHistoryResponse
                                {
                                    Id = Convert.ToInt64(dr["Id"]),
                                    EventTitle = Convert.ToString(dr["EventTitle"]),
                                    FollowUpStage = Convert.ToByte(dr["FollowUpStage"]),
                                    LastModifiedOn = Convert.ToString(dr["LastModifiedOn"]),
                                    LastModifiedBy = Convert.ToString(dr["LastModifiedBy"])
                                });
                            }
                        }
                    }
                }

                apiResponse.data = new FollowUpReminderDataResponse
                {
                    Status = status,
                    History = history
                };

                apiResponse.status = "Success";
                apiResponse.message = "Follow-up reminder status fetched successfully.";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveFollowUpReminder")]
        public async Task<IActionResult> SaveFollowUpReminder([FromBody] SaveFollowUpReminderRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            long followUpReminderId = 0;
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString,
                        out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                SaveFollowUpReminderResponse response = new();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveFollowUpReminder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EnquiryId", SqlDbType.BigInt).Value = request.EnquiryId;
                    cmd.Parameters.Add("@FollowUpNo", SqlDbType.TinyInt).Value = request.FollowUpNo;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                    con.Open();

                    string whatsapp_name = "", whatsapp_bName = "", whatsapp_eventName = "", whatsapp_orgName = "",
                        whatsapp_client_contactno = "", templateName = "", whatsapp_closeGreeting = "";
                    int companyId = 0;
                    bool isSendMsg = false;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            followUpReminderId = Convert.ToInt64(dr["FollowUpReminderId"]);
                            response.FollowUpStage = Convert.ToByte(dr["FollowUpStage"]);
                            response.AffectedRows = Convert.ToInt32(dr["AffectedRows"]);

                            whatsapp_name = dr["Name"]?.ToString() ?? "";
                            whatsapp_bName = dr["BName"]?.ToString() ?? "";
                            whatsapp_eventName = dr["EventName"]?.ToString() ?? "";
                            whatsapp_orgName = dr["OrgName"]?.ToString() ?? "";
                            whatsapp_closeGreeting = dr["ClosingGreeting"]?.ToString() ?? "";

                            whatsapp_client_contactno = dr["ContactNo"]?.ToString() ?? "";
                            companyId = dr["CompanyId"] != DBNull.Value ? Convert.ToInt32(dr["CompanyId"]) : 0;
                            templateName = dr["TempName"]?.ToString() ?? "";
                            isSendMsg = dr["IsSendMsg"] != DBNull.Value ? Convert.ToBoolean(dr["IsSendMsg"]) : false;
                        }
                    }

                    //NOTIFY 8 SEND WHATSAPP - FOLLOW UP REMAINDER
                    if (followUpReminderId > 0 && isSendMsg)
                    {
                        List<string> variables = new List<string>
                        {
                            whatsapp_name,
                            whatsapp_bName,
                            whatsapp_eventName,
                            whatsapp_orgName
                        };
                        if (request.FollowUpNo == 2 || request.FollowUpNo == 3)
                        {
                            variables.Add(whatsapp_closeGreeting);
                        }

                        var whatsappResult =
                            await _whatsAppService.SendTemplateWhatsAppCommon(
                                companyId: companyId,
                                mobileNo: whatsapp_client_contactno,
                                templateName: templateName,
                                variables: variables,
                                actionBy: request.ActionBy,
                                isPromotional: false,
                                domainUrl: domainUrl
                            );

                        if (whatsappResult.status == "Success")
                        {
                            // Save FollowUpReminder
                            response.WhatsappStatus = whatsappResult.status;
                        }
                        else
                        {
                            // Show Error
                            response.WhatsappStatus = whatsappResult.status; // "Failed";

                            using SqlConnection rollbackCon = new SqlConnection(connString);
                            using SqlCommand rollbackCmd = new SqlCommand("RollbackFollowUpReminder", rollbackCon);
                            rollbackCmd.CommandType = CommandType.StoredProcedure;
                            rollbackCmd.Parameters.Add("@FollowUpReminderId", SqlDbType.BigInt).Value = followUpReminderId;

                            await rollbackCon.OpenAsync();
                            await rollbackCmd.ExecuteNonQueryAsync();

                            apiResponse.status = "Error";
                            apiResponse.message = $"WhatsApp sending failed.";
                            apiResponse.data = response;
                            return Ok(apiResponse);
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = $"Follow-up {request.FollowUpNo} saved successfully.";
                apiResponse.data = response;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return StatusCode(500, apiResponse);
            }
        }
        #endregion

        #region MASTER API
        [HttpGet]
        [Route("GetEnquiryFollowUp_Types")]
        public IActionResult GetEnquiryFollowUp_Types()
        {
            apiResponse apiResponse = new apiResponse();
            List<FollowUpTypeResponse> list = new List<FollowUpTypeResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp_Types", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new FollowUpTypeResponse
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"])
                    });
                }

                apiResponse.message = "Follow-up types fetched successfully!";
                apiResponse.data = list;
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
        [Route("GetEnquiryFollowUp_Status")]
        public IActionResult GetEnquiryFollowUp_Status()
        {
            apiResponse apiResponse = new apiResponse();
            List<FollowUpStatusResponse> list = new List<FollowUpStatusResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp_Status", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new FollowUpStatusResponse
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"])
                    });
                }

                apiResponse.message = "Follow-up status fetched successfully!";
                apiResponse.data = list;
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
        [Route("GetEnquiryFollowUp_MoodList")]
        public IActionResult GetEnquiryFollowUp_MoodList()
        {
            apiResponse apiResponse = new apiResponse();
            List<MoodResponse> list = new List<MoodResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp_MoodList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new MoodResponse
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"])
                    });
                }

                apiResponse.message = "Mood list fetched successfully!";
                apiResponse.data = list;
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
        [Route("GetEnquiryFollowUp_ProbabilityList")]
        public IActionResult GetEnquiryFollowUp_ProbabilityList()
        {
            apiResponse apiResponse = new apiResponse();
            List<MoodResponse> list = new List<MoodResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp_ProbabilityList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new MoodResponse
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"])
                    });
                }

                apiResponse.message = "Probability list fetched successfully!";
                apiResponse.data = list;
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
        [Route("GetEnquiryFollowUp_LeadLostReasonCategoryList")]
        public IActionResult GetEnquiryFollowUp_LeadLostReasonCategoryList()
        {
            apiResponse apiResponse = new apiResponse();
            List<LeadLostCategoryResponse> list = new List<LeadLostCategoryResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp_LeadLostReasonCategoryList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new LeadLostCategoryResponse
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"])
                    });
                }

                apiResponse.message = "Reason list fetched successfully!";
                apiResponse.data = list;
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
        [Route("GetEnquiryFollowUp_LeadLostReasonList")]
        public IActionResult GetEnquiryFollowUp_LeadLostReasonList([FromBody] LeadLostReasonRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<LeadLostReasonResponse> list = new List<LeadLostReasonResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp_LeadLostReasonList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@LeadLostCategoryId", SqlDbType.BigInt).Value = request.LeadLostCategoryId;

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                con.Close();
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new LeadLostReasonResponse
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    LeadLostCategoryId = Convert.ToInt32(row["LeadLostCategoryId"]),
                    LeadLostCategoryName = Convert.ToString(row["LeadLostCategoryName"])
                });
            }

            apiResponse.message = "Reason Category list fetched successfully!";
            apiResponse.data = list;
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
        #endregion

    }
}
