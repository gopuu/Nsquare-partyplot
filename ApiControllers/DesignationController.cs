using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using ERPAdmin.Helper;
using ERPAdmin.Models;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignationController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public DesignationController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpGet]
        [Route("GetDesignationList")]
        public IActionResult GetDesignationList()
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            List<dynamic> designations = new List<dynamic>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString); //(ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty);
                SqlCommand cmd = new SqlCommand("GetDesignationList", con);                
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);
                con.Close();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        Designation designation = new Designation();
                        designation.id = Convert.ToInt64(dr["id"]);
                        designation.name = Convert.ToString(dr["name"]);
                        designation.band = Convert.ToString(dr["band"]);
                        designation.code = Convert.ToString(dr["code"]);
                        designations.Add(designation);
                    }
                }
                apiResponse.message = "Designations fetched successfully!";
                apiResponse.data = designations;
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
        [Route("SaveDesignation")]
        public IActionResult SaveDesignation([FromBody] Designation model)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            //con.ConnectionString = _connClass.GetChildConnectionString(); //ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty;
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }


                SqlConnection con = new SqlConnection(connString);
                //Int64 userid = 0;
                //string sesUserid = HttpContext.Session.GetString("User_id") ?? "";
                //if (!String.IsNullOrEmpty(sesUserid))
                //{
                //    userid = Convert.ToInt64(sesUserid);
                //}
                SqlCommand cmd = new SqlCommand("SaveDesignation", con);
                cmd.CommandType = CommandType.StoredProcedure;
                // Retrieve and assign parameters
                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.id > 0 ? model.id : 0;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 500).Value = model.name ?? string.Empty;
                cmd.Parameters.Add("@Band", SqlDbType.NVarChar, 10).Value = model.band ?? string.Empty;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 10).Value = model.code ?? string.Empty;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                if (model.id > 0)
                {
                    apiResponse.message = "Designation updated successfully!";
                }
                else
                {
                    apiResponse.message = "Designation inserted successfully!";
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

        [HttpGet("DeleteDesignation/{id}")]
        public IActionResult DeleteDesignation(Int64 id)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteDesignation", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", SqlDbType.BigInt).Value = id;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                apiResponse.message = "Designation deleted successfully!";
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
        [Route("GetDesignationDetailById/{id}")]
        public IActionResult GetDesignationDetailById(Int64 id)
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
                    SqlCommand cmd = new SqlCommand("GetDesignationById", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", SqlDbType.BigInt).Value = id;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    conn.Close();
                }

                Designation designation = new Designation();
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        designation.id = Convert.ToInt64(dr["id"]);
                        designation.name = Convert.ToString(dr["name"]) ?? "";
                        designation.band = Convert.ToString(dr["band"]) ?? "";
                        designation.code = Convert.ToString(dr["code"]) ?? "";
                    }
                    apiResponse.message = "Designation fetched successfully!";
                    apiResponse.data = designation;
                }
                else
                {
                    apiResponse.message = "Data not found!";
                    apiResponse.data = null;
                    //apiResponse.status = "Error";
                }
                apiResponse.status = "Success";
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
        [Route("GetDesignationListForDropdown")]
        public IActionResult GetDesignationListForDropdown()
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }
                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetDesignationList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);
               
                // Create a list to hold the designation data
                //var designations = new List<object>();
                List<DesignationForDropdown> designations = new List<DesignationForDropdown>();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        DesignationForDropdown designation = new DesignationForDropdown();
                        designation.id = Convert.ToInt64(dr["id"]);
                        designation.name = Convert.ToString(dr["name"]);
                        designations.Add(designation);
                    }
                }
                con.Close();
                //if (dataTable.Rows.Count > 0)
                //{
                //    foreach (DataRow dr in dataTable.Rows)
                //    {
                //        designations.Add(new
                //        {
                //            value = dr["id"].ToString(),
                //            text = dr["name"].ToString()
                //        });
                //    }
                //}

                // Return JSON response
                //return Ok(designations);
                apiResponse.message = "Designations fetched successfully!";
                apiResponse.data = designations;
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
