using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.Services
{
    public class EventReminderWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly string _masterConnString;

        public EventReminderWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Add Delay Before First Run
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        DataTable domains = await GetAllDomains();
                        foreach (DataRow row in domains.Rows)
                        {
                            try
                            {
                                EventReminderService reminderService = scope.ServiceProvider.GetRequiredService<EventReminderService>();
                                string connString = row["connection_string"]?.ToString() ?? "";
                                string domain = row["domain"]?.ToString() ?? "";
                                string domainUrl = $"https://{domain}/";
                                await reminderService.ProcessReminders(connString, domainUrl);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                // RUN EVERY 5 MINUTES
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task<DataTable> GetAllDomains(string connectionType = "banquet")
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_masterConnString))
            using (SqlCommand cmd = new SqlCommand("GetActiveClientConnectionString", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ConnectionType", SqlDbType.NVarChar, 20).Value = string.IsNullOrWhiteSpace(connectionType) ? DBNull.Value : connectionType;
                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }
    }
}