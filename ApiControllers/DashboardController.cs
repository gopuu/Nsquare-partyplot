using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : MasterController
    {
        private string connString = string.Empty;

        public DashboardController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpGet]
        [Route("GetProcessListWithDeals")]
        public IActionResult GetProcessListWithDeals()
        {
            apiResponse apiResponse = new apiResponse();
            List<ProcessDto> processes = new List<ProcessDto>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetProcessListWithDeals", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // ======================
                        // 1) Read Process List
                        // ======================
                        while (reader.Read())
                        {
                            ProcessDto process = new ProcessDto();
                            process.Id = reader.GetInt32(reader.GetOrdinal("ProcessId"));
                            process.Name = reader.GetString(reader.GetOrdinal("ProcessName"));
                            process.TotalDeals = reader.GetInt32(reader.GetOrdinal("TotalDeals"));
                            process.TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
                            process.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")).ToString("dd-MM-yyyy HH:mm:ss");
                            process.Deals = new List<DealDto>();
                            processes.Add(process);
                        }

                        // ======================
                        // 2) Read Deals
                        // ======================
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {

                                DealDto deal = new DealDto();
                                deal.Id = reader.GetInt32(reader.GetOrdinal("DealId"));
                                deal.Title = reader.GetString(reader.GetOrdinal("Title"));
                                deal.Customer = reader.GetString(reader.GetOrdinal("Customer"));
                                deal.Amount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                                deal.Date = reader.GetString(reader.GetOrdinal("DealDate"));

                                int processId = reader.GetInt32(reader.GetOrdinal("ProcessId"));
                                ProcessDto parent = processes.FirstOrDefault(p => p.Id == processId);
                                if (parent != null)
                                {
                                    parent.Deals.Add(deal);
                                }
                            }
                        }
                    }
                }

                apiResponse.message = "Processes fetched successfully!";
                apiResponse.data = processes;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

    }
}
