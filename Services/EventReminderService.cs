using ERPAdmin.Helper;
using ERPAdmin.Models;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.Services
{
    public class EventReminderService 
    {
        private readonly WhatsAppService _whatsAppService;
        private readonly IConfiguration _configuration;
        public EventReminderService(WhatsAppService whatsAppService, IConfiguration configuration)
        {
            _whatsAppService = whatsAppService;
            _configuration = configuration;
        }

        // =====================================================
        // MAIN PROCESS
        // =====================================================
        long cnt = 0;
        public async Task ProcessReminders(string connString, string domainUrl)
        {
            cnt++;
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEventReminderBookings", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                long bookingId = reader["BookingId"] != DBNull.Value ? Convert.ToInt64(reader["BookingId"]) : 0;
                                long settingId = reader["SettingId"] != DBNull.Value ? Convert.ToInt64(reader["SettingId"]) : 0;
                                string mobileNo = reader["Client_Contact"]?.ToString() ?? "";
                                string name = reader["Name"]?.ToString() ?? "";
                                string bName = reader["BName"]?.ToString() ?? "";
                                string eDate = reader["EDate"]?.ToString() ?? "";
                                string orgMobile = reader["OrgMobile"]?.ToString() ?? "";
                                string orgName = reader["OrgName"]?.ToString() ?? "";
                                string templateName = reader["TempName"]?.ToString() ?? "";
                                int companyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : 0;
                                bool isSendMsg = reader["SendMsg"] != DBNull.Value ? Convert.ToBoolean(reader["SendMsg"]) : false;

                                if (!isSendMsg)
                                    continue;

                                List<string> variables = new List<string>
                                {
                                    name,
                                    bName,
                                    eDate,
                                    orgMobile,
                                    orgName
                                };

                                await _whatsAppService.SendTemplateWhatsAppCommon(
                                    companyId: companyId,
                                    mobileNo: mobileNo,
                                    templateName: templateName,
                                    variables: variables,
                                    actionBy: 0,
                                    isPromotional: false,
                                    domainUrl: domainUrl
                                );

                                await SaveReminderLog(connString, bookingId, settingId, true);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("/Services/EventReminderService ProcessReminders(conn, domainUrl): " + ex.Message + ", cnt:" + cnt);
            }
        }

        // =====================================================
        // SAVE LOG
        // =====================================================
        private async Task SaveReminderLog(string connString, long bookingId, long settingId, bool isSuccess)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveEventReminderLog", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = bookingId;
                cmd.Parameters.Add("@SettingId", SqlDbType.BigInt).Value = settingId;
                cmd.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = isSuccess;

                await conn.OpenAsync();

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}