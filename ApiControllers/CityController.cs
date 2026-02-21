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
    public class CityController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public CityController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetCityList")]
        public IActionResult GetCityList([FromBody] GetCityRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            List<GetCityList> cities = new List<GetCityList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString); //(ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty);
                SqlCommand cmd = new SqlCommand("GetCityList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = request.Id == 0 ? 0 : request.Id;
                cmd.Parameters.AddWithValue("@stateid", SqlDbType.BigInt).Value = request.StateId == 0 ? 0 : request.StateId;
                con.Open();

                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);
                con.Close();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        GetCityList city = new GetCityList();
                        city.Id = Convert.ToInt64(dr["id"]);
                        city.Name = Convert.ToString(dr["city_name"]);
                        city.StateId = Convert.ToInt64(dr["state_id"]);
                        city.StateName = Convert.ToString(dr["state_name"]);
                        city.Pincode = Convert.ToInt64(dr["pincode"]);
                        //city.IsActive = Convert.ToBoolean(dr["isactive"]);
                        cities.Add(city);
                    }
                }
                apiResponse.message = "Cities fetched successfully!";
                apiResponse.data = cities;
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
        [Route("SaveCity")]
        public IActionResult SaveCity([FromBody] SaveCity model)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            //con.ConnectionString = _connClass.GetChildConnectionString(); //ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty;
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }


                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveCity", con);
                cmd.CommandType = CommandType.StoredProcedure;
                // Retrieve and assign parameters
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = model.Name ?? string.Empty;
                cmd.Parameters.Add("@stateid", SqlDbType.BigInt).Value = model.StateId > 0 ? model.StateId : 0;
                cmd.Parameters.Add("@pincode", SqlDbType.BigInt).Value = model.Pincode > 0 ? model.Pincode : 0;
                cmd.Parameters.Add("@isactive", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                if (model.Id > 0)
                {
                    apiResponse.message = "City updated successfully!";
                }
                else
                {
                    apiResponse.message = "City inserted successfully!";
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

        [HttpPost("DeleteCity")]
        public IActionResult DeleteCity(DeleteCityRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteCity", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = request.Id;
                    cmd.Parameters.AddWithValue("@actionBy", SqlDbType.BigInt).Value = request.ActionBy;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                apiResponse.message = "City deleted successfully!";
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


        //delete later
        /*
        [HttpGet]
        [Route("GetCityDetailById/{id}")]
        private IActionResult GetCityDetailById(Int64 id)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }
                DataSet ds = new DataSet();
                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("GetCityList", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = id;
                    cmd.Parameters.AddWithValue("@stateid", SqlDbType.BigInt).Value = 0;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    conn.Close();
                }

                City city = new City();
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        city.Id = Convert.ToInt64(dr["id"]);
                        city.Name = Convert.ToString(dr["city_name"]);
                        city.StateId = Convert.ToInt64(dr["state_id"]);
                        city.StateName = Convert.ToString(dr["state_name"]);
                    }
                    apiResponse.message = "City fetched successfully!";
                    apiResponse.data = city;
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
        */
    }
}
