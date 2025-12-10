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
    public class EnquiryFollowupController : MasterController
    {
        private string connString = string.Empty;

        public EnquiryFollowupController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetEnquiryFollowUp")]
        public IActionResult GetEnquiryFollowUp([FromBody] GetEnquiryFollowUpRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EnquiryFollowUpResponse> list = new List<EnquiryFollowUpResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryFollowUp", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FollowUpId", SqlDbType.BigInt).Value = request.FollowUpId == 0 ? 0 : request.FollowUpId;
                    cmd.Parameters.Add("@EnquiryId", SqlDbType.BigInt).Value = request.EnquiryId == 0 ? 0 : request.EnquiryId;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new EnquiryFollowUpResponse
                    {
                        FollowUpId = Convert.ToInt64(row["FollowUpId"]),
                        EnquiryId = Convert.ToInt64(row["EnquiryId"]),

                        FollowUpDateTime = Convert.ToString(row["FollowUpDateTime"]),
                        FollowUpDate = Convert.ToString(row["FollowUpDate"]),
                        FollowUpTime = Convert.ToString(row["FollowUpTime"]),
                        FollowUpTime12hr = Convert.ToString(row["FollowUpTime12hr"]),

                        NextFollowUpDateTime = Convert.ToString(row["NextFollowUpDateTime"]),
                        NextFollowUpDate = Convert.ToString(row["NextFollowUpDate"]),
                        NextFollowUpTime = Convert.ToString(row["NextFollowUpTime"]),
                        NextFollowUpTime12hr = Convert.ToString(row["NextFollowUpTime12hr"]),

                        StatusId = Convert.ToInt64(row["StatusId"]),
                        StatusName = Convert.ToString(row["StatusName"]),
                        MoodId = Convert.ToInt64(row["MoodId"]),
                        MoodName = Convert.ToString(row["MoodName"]),
                        FollowUpTypeId = Convert.ToInt64(row["FollowUpTypeId"]),
                        FollowUpTypeName = Convert.ToString(row["FollowUpTypeName"]),

                        OutcomeId = row["OutcomeId"] == DBNull.Value ? null : Convert.ToInt64(row["OutcomeId"]),
                        Probability = row["Probability"] == DBNull.Value ? null : Convert.ToDecimal(row["Probability"]),
                        DiscussionNotes = Convert.ToString(row["DiscussionNotes"]),
                        InternalRemarks = Convert.ToString(row["InternalRemarks"]),
                        CustomerRemarks = Convert.ToString(row["CustomerRemarks"]),
                        AssignedTo = Convert.ToInt64(row["AssignedTo"]),
                        AssignedToName = Convert.ToString(row["AssignedToName"]),
                        IsClosed = Convert.ToBoolean(row["IsClosed"])
                    });
                }

                apiResponse.message = "Follow-up records fetched successfully!";
                apiResponse.data = list;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error: " + ex.Message;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveEnquiryFollowUp", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FollowUpId", SqlDbType.BigInt).Value = model.FollowUpId > 0 ? model.FollowUpId : 0;
                    cmd.Parameters.Add("@EnquiryId", SqlDbType.BigInt).Value = model.EnquiryId;
                    cmd.Parameters.Add("@FollowUpTypeId", SqlDbType.BigInt).Value = model.FollowUpTypeId;

                    cmd.Parameters.Add("@FollowUpDateTime", SqlDbType.NVarChar, 30).Value = model.FollowUpDateTime ?? "";
                    cmd.Parameters.Add("@NextFollowUpDateTime", SqlDbType.NVarChar, 30).Value = model.NextFollowUpDateTime ?? "";

                    cmd.Parameters.Add("@StatusId", SqlDbType.BigInt).Value = model.StatusId;
                    cmd.Parameters.Add("@MoodId", SqlDbType.BigInt).Value = model.MoodId;
                    cmd.Parameters.Add("@OutcomeId", SqlDbType.BigInt).Value = model.OutcomeId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@Probability", SqlDbType.Int).Value = model.Probability ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@DiscussionNotes", SqlDbType.NVarChar).Value = (object)model.DiscussionNotes ?? DBNull.Value;
                    cmd.Parameters.Add("@InternalRemarks", SqlDbType.NVarChar).Value = (object)model.InternalRemarks ?? DBNull.Value;
                    cmd.Parameters.Add("@CustomerRemarks", SqlDbType.NVarChar).Value = (object)model.CustomerRemarks ?? DBNull.Value;

                    cmd.Parameters.Add("@AssignedTo", SqlDbType.BigInt).Value = model.AssignedTo;
                    cmd.Parameters.Add("@IsClosed", SqlDbType.Bit).Value = model.IsClosed;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
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

                apiResponse.message = "Error: " + ex.Message;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

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

        [HttpGet]
        [Route("GetEnquiryFollowUp_Types")]
        public IActionResult GetEnquiryFollowUp_Types()
        {
            apiResponse apiResponse = new apiResponse();
            List<FollowUpTypeResponse> list = new List<FollowUpTypeResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

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

                apiResponse.message = "Error: " + ex.Message;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

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

                apiResponse.message = "Error: " + ex.Message;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

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

                apiResponse.message = "Error: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }


    }
}
