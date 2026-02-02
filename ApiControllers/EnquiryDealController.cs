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
using System.Text.Json;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnquiryDealController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;
        private readonly ImageUploadService _imageUploadService;
        private readonly string domainUrl;

        public EnquiryDealController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            _imageUploadService = imageUploadService;
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpGet]
        [Route("GetEnquiryDealList")]
        public IActionResult GetEnquiryDealList()
        {
            apiResponse apiResponse = new apiResponse();
            List<dynamic> enquiryDeals = new List<dynamic>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryDealList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = 0;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var deal = new
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                ProcessId = reader.GetInt64(reader.GetOrdinal("ProcessId")),
                                ProcessName = reader.GetString(reader.GetOrdinal("ProcessName")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Contact = reader.GetString(reader.GetOrdinal("Contact")),
                                Address = reader["Address"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Address")) : null,
                                Email = reader["Email"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Email")) : null,
                                EventTypeId = reader.GetInt64(reader.GetOrdinal("EventTypeId")),
                                EventTypeName = reader.GetString(reader.GetOrdinal("EventTypeName")),
                                EventSubTypeId = reader.GetInt64(reader.GetOrdinal("EventSubTypeId")),
                                EventSubTypeName = reader.GetString(reader.GetOrdinal("EventSubTypeName")),
                                BanquetHallId = reader.GetInt64(reader.GetOrdinal("BanquetHallId")),
                                BanquetHallName = reader.GetString(reader.GetOrdinal("BanquetHallName")),
                                //EventDate = reader.GetString(reader.GetOrdinal("EventDate")),
                                //EventDate_Format = reader.GetString(reader.GetOrdinal("EventDate_Format")),

                                EventDate = reader["EventDate"] != DBNull.Value ? ((DateTime)reader["EventDate"]).ToString("yyyy-MM-dd") : null,
                                EventDate_Format = reader["EventDate_Format"] != DBNull.Value ? reader["EventDate_Format"].ToString() : null,
                                FollowUpDate = reader["FollowUpDate"] != DBNull.Value ? ((DateTime)reader["FollowUpDate"]).ToString("yyyy-MM-dd") : null,
                                FollowUpDate_Format = reader["FollowUpDate_Format"] != DBNull.Value ? reader["FollowUpDate_Format"].ToString() : null,
                                EventStartTime = reader["EventStartTime"] != DBNull.Value ? reader["EventStartTime"].ToString() : null,
                                EventEndTime = reader["EventEndTime"] != DBNull.Value ? reader["EventEndTime"].ToString() : null,

                                //EventStartTime = reader.GetTimeSpan(reader.GetOrdinal("EventStartTime")),
                                //EventEndTime = reader.GetTimeSpan(reader.GetOrdinal("EventEndTime")),
                                Pax = reader.GetInt64(reader.GetOrdinal("Pax")),
                                //FollowUpDate = reader.GetString(reader.GetOrdinal("FollowUpDate")),
                                //FollowUpDate_Format = reader.GetString(reader.GetOrdinal("FollowUpDate_Format")),
                                EstimatedAmount = reader.GetDecimal(reader.GetOrdinal("EstimatedAmount")),
                                LeadSourceId = reader.GetInt64(reader.GetOrdinal("LeadSourceId")),
                                LeadSourceName = reader.GetString(reader.GetOrdinal("LeadSourceName")),
                                Remarks = reader["Remarks"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Remarks")) : null,
                                FollowUpCount = reader.GetInt64(reader.GetOrdinal("FollowUpCount")),
                            };
                            enquiryDeals.Add(deal);
                        }
                    }
                }

                apiResponse.message = "EnquiryDeals fetched successfully!";
                apiResponse.data = enquiryDeals;
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

        [HttpPost]
        [Route("SaveEnquiryDeal")]
        public IActionResult SaveEnquiryDeal([FromBody] SaveEnquiryDeal model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveEnquiryDeal", con)) // You need to create this SP
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                    cmd.Parameters.Add("@ProcessId", SqlDbType.BigInt).Value = model.ProcessId;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255).Value = model.Name ?? string.Empty;
                    cmd.Parameters.Add("@Contact", SqlDbType.NVarChar, 50).Value = model.Contact ?? string.Empty;
                    cmd.Parameters.Add("@Address", SqlDbType.NVarChar, -1).Value = model.Address ?? string.Empty;
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = model.Email ?? string.Empty;
                    cmd.Parameters.Add("@EventTypeId", SqlDbType.BigInt).Value = model.EventTypeId;
                    cmd.Parameters.Add("@EventSubTypeId", SqlDbType.BigInt).Value = model.EventSubTypeId;
                    cmd.Parameters.Add("@BanquetHallId", SqlDbType.BigInt).Value = model.BanquetHallId;
                    cmd.Parameters.Add("@EventDate", SqlDbType.NVarChar, 20).Value = model.EventDate;
                    cmd.Parameters.Add("@EventStartTime", SqlDbType.NVarChar, 10).Value = model.EventStartTime;
                    cmd.Parameters.Add("@EventEndTime", SqlDbType.NVarChar, 10).Value = model.EventEndTime;
                    cmd.Parameters.Add("@Pax", SqlDbType.BigInt).Value = model.Pax;
                    cmd.Parameters.Add("@FollowUpDate", SqlDbType.NVarChar, 20).Value = (object)model.FollowUpDate ?? DBNull.Value;
                    cmd.Parameters.Add("@EstimatedAmount", SqlDbType.Decimal).Value = model.EstimatedAmount;
                    cmd.Parameters.Add("@LeadSourceId", SqlDbType.BigInt).Value = model.LeadSourceId;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, -1).Value = model.Remarks ?? string.Empty;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    var result = cmd.ExecuteScalar();
                    long newId = result != null ? Convert.ToInt64(result) : 0;
                    con.Close();

                    apiResponse.message = model.Id > 0 ? "EnquiryDeal updated successfully!" : "EnquiryDeal inserted successfully!";
                    apiResponse.status = "Success";
                    return Ok(apiResponse);
                }
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
        [Route("DeleteEnquiryDeal")]
        public IActionResult DeleteEnquiryDeal([FromBody] DeleteEnquiryDeal model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteEnquiryDeal", con)) // You need to create this SP
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.message = "EnquiryDeal deleted successfully!";
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
        [Route("GetEnquiryDealById/{enquiryDealId}")]
        public IActionResult GetEnquiryDealById(long enquiryDealId)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;
                EnquiryDeal enquiryDeal = new EnquiryDeal();
                //dynamic enquiryDeal = null;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryDealList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = enquiryDealId;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // single record
                        {
                            enquiryDeal.Id = reader.GetInt64(reader.GetOrdinal("Id"));
                            enquiryDeal.ProcessId = reader.GetInt64(reader.GetOrdinal("ProcessId"));
                            enquiryDeal.ProcessName = reader.GetString(reader.GetOrdinal("ProcessName"));
                            enquiryDeal.Name = reader.GetString(reader.GetOrdinal("Name"));
                            enquiryDeal.Contact = reader.GetString(reader.GetOrdinal("Contact"));
                            enquiryDeal.Address = reader["Address"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Address")) : null;
                            enquiryDeal.Email = reader["Email"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Email")) : null;
                            enquiryDeal.EventTypeId = reader.GetInt64(reader.GetOrdinal("EventTypeId"));
                            enquiryDeal.EventTypeName = reader.GetString(reader.GetOrdinal("EventTypeName"));
                            enquiryDeal.EventSubTypeId = reader.GetInt64(reader.GetOrdinal("EventSubTypeId"));
                            enquiryDeal.EventSubTypeName = reader.GetString(reader.GetOrdinal("EventSubTypeName"));
                            enquiryDeal.BanquetHallId = reader.GetInt64(reader.GetOrdinal("BanquetHallId"));
                            enquiryDeal.BanquetHallName = reader.GetString(reader.GetOrdinal("BanquetHallName"));
                            //enquiryDeal.EventDate = reader.GetDateTime(reader.GetOrdinal("EventDate")).ToString("yyyy-MM-dd");
                            //enquiryDeal.EventDate_Format = reader.GetString(reader.GetOrdinal("EventDate_Format"));
                            //enquiryDeal.EventStartTime = reader.GetString(reader.GetOrdinal("EventStartTime"));
                            //enquiryDeal.EventEndTime = reader.GetString(reader.GetOrdinal("EventEndTime"));
                            enquiryDeal.Pax = reader.GetInt64(reader.GetOrdinal("Pax"));
                            //enquiryDeal.FollowUpDate = reader["FollowUpDate"] != DBNull.Value
                            //                ? reader.GetDateTime(reader.GetOrdinal("FollowUpDate")).ToString("yyyy-MM-dd")
                            //                : null;
                            //enquiryDeal.FollowUpDate_Format = reader["FollowUpDate_Format"] != DBNull.Value
                            //                ? reader.GetString(reader.GetOrdinal("FollowUpDate_Format"))
                            //                : null;
                            enquiryDeal.EventDate = reader["EventDate"] != DBNull.Value ? ((DateTime)reader["EventDate"]).ToString("yyyy-MM-dd") : null;
                            enquiryDeal.EventDate_Format = reader["EventDate_Format"] != DBNull.Value ? reader["EventDate_Format"].ToString() : null;
                            enquiryDeal.FollowUpDate = reader["FollowUpDate"] != DBNull.Value ? ((DateTime)reader["FollowUpDate"]).ToString("yyyy-MM-dd") : null;
                            enquiryDeal.FollowUpDate_Format = reader["FollowUpDate_Format"] != DBNull.Value ? reader["FollowUpDate_Format"].ToString() : null;
                            enquiryDeal.EventStartTime = reader["EventStartTime"] != DBNull.Value ? reader["EventStartTime"].ToString() : null;
                            enquiryDeal.EventEndTime = reader["EventEndTime"] != DBNull.Value ? reader["EventEndTime"].ToString() : null;

                            enquiryDeal.EstimatedAmount = reader.GetDecimal(reader.GetOrdinal("EstimatedAmount"));
                            enquiryDeal.LeadSourceId = reader.GetInt64(reader.GetOrdinal("LeadSourceId"));
                            enquiryDeal.LeadSourceName = reader.GetString(reader.GetOrdinal("LeadSourceName"));
                            enquiryDeal.Remarks = reader["Remarks"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Remarks")) : null; 
                            enquiryDeal.FollowUpCount = reader.GetInt64(reader.GetOrdinal("FollowUpCount"));

                        }
                    }
                }

                if (enquiryDeal != null)
                {
                    apiResponse.message = "EnquiryDeal fetched successfully!";
                    apiResponse.data = enquiryDeal;
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "Data not found!";
                    apiResponse.data = null;
                    apiResponse.status = "Failed";
                }

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

        [HttpGet]
        [Route("GetEnquiryDealListForDropdown")]
        public IActionResult GetEnquiryDealListForDropdown()
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                DataTable dataTable = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryDealList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = 0; // Fetch all for dropdown

                    con.Open();
                    using (SqlDataReader sqlDataReader = cmd.ExecuteReader())
                    {
                        dataTable.Load(sqlDataReader);
                    }
                    con.Close();
                }

                List<dynamic> enquiryDeals = new List<dynamic>();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        var deal = new
                        {
                            Id = Convert.ToInt64(dr["Id"]),
                            Name = Convert.ToString(dr["Name"]),
                            ProcessId = Convert.ToInt64(dr["ProcessId"]),
                            ProcessName = Convert.ToString(dr["ProcessName"])
                        };
                        enquiryDeals.Add(deal);
                    }
                }

                apiResponse.message = "EnquiryDeals fetched successfully!";
                apiResponse.data = enquiryDeals;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log error
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetEnquiryDealList_forDragNDrop")]
        public IActionResult GetEnquiryDealList_forDragNDrop(EnquiryDeal_forDragNDrop request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EnquiryProcessDto> processes = new List<EnquiryProcessDto>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryDealList_forDragNDrop", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ProcessId", SqlDbType.BigInt).Value = request.ProcessId;
                    cmd.Parameters.Add("@Duration", SqlDbType.NVarChar, 20).Value = request.Duration;
                    cmd.Parameters.Add("@FromAmount", SqlDbType.Decimal).Value = request.FromAmount;
                    cmd.Parameters.Add("@ToAmount", SqlDbType.Decimal).Value = request.ToAmount;
                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar, 20).Value = request.FromDate;
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar, 20).Value = request.ToDate;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Safely read EnquiryDealsJson column
                            string enquiryDealsJsonStr = reader["EnquiryDealsJson"] != DBNull.Value
                                ? reader.GetString(reader.GetOrdinal("EnquiryDealsJson"))
                                : "[]";

                            List<dynamic> enquiryDealsJsonObj = new List<dynamic>();
                            try
                            {
                                enquiryDealsJsonObj = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(enquiryDealsJsonStr) ?? new List<dynamic>();
                            }
                            catch
                            {
                                // In case JSON parsing fails, fallback to empty list
                                enquiryDealsJsonObj = new List<dynamic>();
                            }

                            // Construct DTO
                            var process = new EnquiryProcessDto
                            {
                                ProcessId = reader.GetInt64(reader.GetOrdinal("ProcessId")),
                                ProcessName = reader.GetString(reader.GetOrdinal("ProcessName")),
                                TotalEnquiryDeals = reader.GetInt32(reader.GetOrdinal("TotalEnquiryDeals")),
                                TotalAmount = reader["TotalAmount"] != DBNull.Value
                                    ? reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                                    : 0,
                                EnquiryDealsJson = enquiryDealsJsonObj
                            };

                            processes.Add(process);
                        }
                    }
                }

                apiResponse.message = "EnquiryDealList fetched successfully!";
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

        [HttpPost]
        [Route("SaveEnquiryDeal_forDragNDrop")]
        public IActionResult SaveEnquiryDeal_forDragNDrop([FromBody] SaveEnquiryDeal_forDragNDrop model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveEnquiryDeal_forDragNDrop", con)) // You need to create this SP
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EnquiryDealId", SqlDbType.BigInt).Value = model.EnquiryDealId;
                    cmd.Parameters.Add("@ProcessId", SqlDbType.BigInt).Value = model.ProcessId;
                    cmd.Parameters.Add("@FollowUpDate", SqlDbType.NVarChar).Value = model.FollowUpDate;
                    cmd.Parameters.Add("@FileUploadName", SqlDbType.NVarChar).Value = DBNull.Value; //model.FileUpload;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = model.Remarks;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    var result = cmd.ExecuteScalar();
                    long rowid = result != null ? Convert.ToInt64(result) : 0;

                    string latestImagePath = string.Empty;
                    //photo upload
                    if (model.FileUpload != string.Empty && model.FileUpload != null)
                    {
                        string imgType = _imageUploadService.GetFileTypeFromBase64(model.FileUpload);

                        string folderPath = $"/org/{domain}/Booking_Processing/EnquiryDealId_{model.EnquiryDealId}"; // Updated to include domain
                        string fileName = $"EnquiryDealsProcessingId_{rowid}";
                        _imageUploadService.DeleteImages(folderPath, fileName);
                        model.FileUpload = _imageUploadService.UploadImage(model.FileUpload, folderPath, fileName, imgType);

                        //update table
                        SqlCommand cmd2 = new SqlCommand("SaveEnquiryDeal_forDragNDrop_FileUpload", con);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@id", SqlDbType.BigInt).Value = rowid;
                        cmd2.Parameters.Add("@fileUploadName", SqlDbType.NVarChar, -1).Value = model.FileUpload; // Kept max for nvarchar(max)
                        cmd2.ExecuteNonQuery();

                        latestImagePath = domainUrl + $"/uploads/org/{domain}/Booking_Processing/" + model.FileUpload;
                    }

                    con.Close();

                    apiResponse.message = "EnquiryDeal Moved Successfully!";
                    apiResponse.status = "Success";
                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        //VALIDATE SLOT
        [HttpPost]
        [Route("CheckBookingSlotAvailability")]
        public IActionResult CheckBookingSlotAvailability([FromBody] CheckSlotAvailabilityRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                int isAvailable = 0;
                string message = "";

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("CheckBookingSlotAvailability", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EventDate", SqlDbType.NVarChar, 20).Value = model.EventDate ?? "";
                    cmd.Parameters.Add("@StartTime", SqlDbType.NVarChar, 20).Value = model.StartTime ?? "";
                    cmd.Parameters.Add("@EndTime", SqlDbType.NVarChar, 20).Value = model.EndTime ?? "";
                    cmd.Parameters.Add("@BanquetId", SqlDbType.BigInt).Value = model.BanquetId;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            isAvailable = Convert.ToInt32(dr["IsAvailable"]);
                            message = dr["Message"].ToString();
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = message;
                apiResponse.data = new
                {
                    IsAvailable = isAvailable
                };

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


    }
}
