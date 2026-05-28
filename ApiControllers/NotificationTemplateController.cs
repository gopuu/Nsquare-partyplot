using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
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
    public class NotificationTemplateController : MasterController
    {
        private string connString = string.Empty;
        public NotificationTemplateController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetNotification_TemplateParameterList")]
        public IActionResult GetNotification_TemplateParameterList([FromBody] GetNotification_TemplateParameterList request)
        {
            apiResponse apiResponse = new apiResponse();
            List<Notification_TemplateParameterList> templates = new List<Notification_TemplateParameterList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetNotification_TemplateParameterList", con);
                cmd.Parameters.Add("@EventId", SqlDbType.BigInt).Value = request.EventId;
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);
                con.Close();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        Notification_TemplateParameterList model = new Notification_TemplateParameterList();

                        model.TempName = Convert.ToString(dr["TempName"]);
                        model.TempName_Title = Convert.ToString(dr["TempName_Title"]);
                        model.PTFlag = Convert.ToString(dr["PTFlag"]);
                        model.EventId = dr["EventId"] != DBNull.Value ? Convert.ToInt64(dr["EventId"]) : 0;
                        model.IsSendMsg = dr["IsSendMsg"] != DBNull.Value ? Convert.ToBoolean(dr["IsSendMsg"]) : false;

                        templates.Add(model);
                    }
                }

                apiResponse.message = "Notification template list fetched successfully!";
                apiResponse.data = templates;
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveNotification_TemplateParameterList")]
        public IActionResult SaveNotification_TemplateParameterList([FromBody] SaveNotification_TemplateParameterList model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveNotification_TemplateParameterList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EventId", SqlDbType.BigInt).Value = model.EventId;
                cmd.Parameters.Add("@IsSendMsg", SqlDbType.Bit).Value = model.IsSendMsg;

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                apiResponse.message = "Notification template updated successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        #region EVENT REMINDER SETTINGS
        [HttpPost]
        [Route("GetNotification_EventReminderSettingsList")]
        public IActionResult GetNotification_EventReminderSettingsList([FromBody] GetNotification_EventReminderSettingsList request)
        {
            apiResponse apiResponse = new apiResponse();

            List<Notification_EventReminderSettingsList> reminders = new List<Notification_EventReminderSettingsList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dataTable = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetNotification_EventReminderSettingsList",con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EventId", SqlDbType.BigInt).Value = request.EventId;

                    con.Open();

                    SqlDataReader sqlDataReader = cmd.ExecuteReader();
                    dataTable.Load(sqlDataReader);
                    con.Close();
                }

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        Notification_EventReminderSettingsList model = new Notification_EventReminderSettingsList();
                        model.Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0;
                        model.Title = Convert.ToString(dr["Title"]) ?? "";
                        model.TriggerDays = dr["TriggerDays"] != DBNull.Value ? Convert.ToInt32(dr["TriggerDays"]) : 0;
                        model.TriggerTime = Convert.ToString(dr["TriggerTime2"]) ?? "";
                        model.TriggerTime_Format = Convert.ToString(dr["TriggerTime_Format"]) ?? "";
                        model.IsEnabled = dr["IsEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsEnabled"]) : false;
                        model.EventId = dr["EventId"] != DBNull.Value ? Convert.ToInt64(dr["EventId"]) : 0;
                        model.LastModifiedOn = Convert.ToString(dr["LastModifiedOn"]) ?? "";
                        reminders.Add(model);
                    }
                }
                apiResponse.message = "Event reminder settings fetched successfully!";
                apiResponse.data = reminders;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveNotification_EventReminderSettings")]
        public IActionResult SaveNotification_EventReminderSettings([FromBody] SaveNotification_EventReminderSettings request)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveNotification_EventReminderSettings",con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id;
                    cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 100).Value = request.Title ?? "";
                    cmd.Parameters.Add("@TriggerDays", SqlDbType.Int).Value = request.TriggerDays ?? 0;
                    cmd.Parameters.Add("@TriggerTime", SqlDbType.Time).Value = string.IsNullOrWhiteSpace(request.TriggerTime) ? DBNull.Value : TimeSpan.Parse(request.TriggerTime);
                    cmd.Parameters.Add("@IsEnabled", SqlDbType.Bit).Value = request.IsEnabled ?? true;
                    cmd.Parameters.Add("@EventId", SqlDbType.BigInt).Value = request.EventId ?? 0;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                    con.Open();

                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.message = request.Id > 0
                    ? "Event reminder updated successfully!"
                    : "Event reminder added successfully!";

                apiResponse.data = new{Id = rowId};
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }
        #endregion

    }
}
