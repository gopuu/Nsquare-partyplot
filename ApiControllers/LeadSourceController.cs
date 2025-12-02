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
    public class LeadSourceController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public LeadSourceController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpGet]
        [Route("GetLeadSourceList")]
        public IActionResult GetLeadSourceList()
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            List<dynamic> leadSourcees = new List<dynamic>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString); //(ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty);
                SqlCommand cmd = new SqlCommand("GetLeadSourceMasterList", con);
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
                        LeadSource leadSource = new LeadSource();
                        leadSource.Id = Convert.ToInt64(dr["id"]);
                        leadSource.Name = Convert.ToString(dr["name"]);
                        leadSourcees.Add(leadSource);
                    }
                }
                apiResponse.message = "LeadSources fetched successfully!";
                apiResponse.data = leadSourcees;
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
        [Route("SaveLeadSource")]
        public IActionResult SaveLeadSource([FromBody] SaveLeadSource model)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            //con.ConnectionString = _connClass.GetChildConnectionString(); //ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty;
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }


                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveLeadSourceMaster", con);
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
                    apiResponse.message = "LeadSource updated successfully!";
                }
                else
                {
                    apiResponse.message = "LeadSource inserted successfully!";
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
        [Route("DeleteLeadSource")]
        public IActionResult DeleteLeadSource(DeleteLeadSource model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteLeadSourceMaster", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                apiResponse.message = "LeadSource deleted successfully!";
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
        [Route("GetLeadSourceDetailById/{id}")]
        public IActionResult GetLeadSourceDetailById(Int64 id)
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
                    SqlCommand cmd = new SqlCommand("GetLeadSourceMasterList", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = id;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    conn.Close();
                }

                LeadSource leadSource = new LeadSource();
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    leadSource.Id = Convert.ToInt64(dr["id"]);
                    leadSource.Name = Convert.ToString(dr["name"]) ?? string.Empty;
                    apiResponse.message = "LeadSource fetched successfully!";
                    apiResponse.data = leadSource;
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
        [Route("GetLeadSourceListForDropdown")]
        public IActionResult GetLeadSourceListForDropdown()
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }
                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetLeadSourceMasterList", con);
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = 0;
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);

                // Create a list to hold the leadSource data
                //var leadSourcees = new List<object>();
                List<LeadSource> leadSourcees = new List<LeadSource>();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        LeadSource leadSource = new LeadSource();
                        leadSource.Id = Convert.ToInt64(dr["id"]);
                        leadSource.Name = Convert.ToString(dr["name"]);
                        leadSourcees.Add(leadSource);
                    }
                }
                con.Close();

                // Return JSON response
                //return Ok(leadSourcees);
                apiResponse.message = "LeadSources fetched successfully!";
                apiResponse.data = leadSourcees;
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
