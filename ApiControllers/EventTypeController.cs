using DocumentFormat.OpenXml.EMMA;
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
    public class EventTypeController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public EventTypeController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpGet]
        [Route("GetEventTypeList")]
        public IActionResult GetEventTypeList()
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            List<dynamic> eventTypes = new List<dynamic>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString); //(ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty);
                SqlCommand cmd = new SqlCommand("GetEventTypeMasterList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = 0;
                con.Open();

                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);
                con.Close();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        EventType eventType = new EventType();
                        eventType.Id = Convert.ToInt64(dr["id"]);
                        eventType.Name = Convert.ToString(dr["name"]);
                        eventTypes.Add(eventType);
                    }
                }
                apiResponse.message = "EventTypes fetched successfully!";
                apiResponse.data = eventTypes;
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveEventType")]
        public IActionResult SaveEventType([FromBody] SaveEventType model)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            //con.ConnectionString = _connClass.GetChildConnectionString(); //ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty;
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }


                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveEventTypeMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                // Retrieve and assign parameters
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = model.Name ?? string.Empty;
                cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;
                con.Open();
                //cmd.ExecuteNonQuery();
                var result = cmd.ExecuteScalar();
                long newId = result != null ? Convert.ToInt64(result) : 0;
                con.Close();

                if (model.Id > 0)
                {
                    apiResponse.message = "EventType updated successfully!";
                }
                else
                {
                    apiResponse.message = "EventType inserted successfully!";
                }
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteEventType")]
        public IActionResult DeleteEventType(DeleteEventType model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteEventTypeMaster", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                apiResponse.message = "EventType deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpGet]
        [Route("GetEventTypeDetailById/{id}")]
        public IActionResult GetEventTypeDetailById(Int64 id)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }
                DataSet ds = new DataSet();
                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("GetEventTypeMasterList", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = id;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    conn.Close();
                }

                EventType eventType = new EventType();
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    eventType.Id = Convert.ToInt64(dr["id"]);
                    eventType.Name = Convert.ToString(dr["name"]) ?? string.Empty;
                    apiResponse.message = "EventType fetched successfully!";
                    apiResponse.data = eventType;
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "Data not found!";
                    apiResponse.data = null;
                    apiResponse.status = "Failed";
                }
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpGet]
        [Route("GetEventTypeListForDropdown")]
        public IActionResult GetEventTypeListForDropdown()
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }
                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetEventTypeMasterList", con);
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = 0;
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);

                // Create a list to hold the eventType data
                //var eventTypes = new List<object>();
                List<EventType> eventTypes = new List<EventType>();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        EventType eventType = new EventType();
                        eventType.Id = Convert.ToInt64(dr["id"]);
                        eventType.Name = Convert.ToString(dr["name"]);
                        eventTypes.Add(eventType);
                    }
                }
                con.Close();

                // Return JSON response
                //return Ok(eventTypes);
                apiResponse.message = "EventTypes fetched successfully!";
                apiResponse.data = eventTypes;
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                //Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }
    }
}
