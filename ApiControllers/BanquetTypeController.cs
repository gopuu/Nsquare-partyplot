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
    public class BanquetTypeController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public BanquetTypeController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpGet]
        [Route("GetBanquetTypeList")]
        public IActionResult GetBanquetTypeList()
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            List<dynamic> banquettypes = new List<dynamic>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataTable dataTable = new DataTable();
                SqlConnection con = new SqlConnection(connString); //(ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty);
                SqlCommand cmd = new SqlCommand("GetBanquetTypeList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = 0;
                con.Open();

                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dataTable.Load(sqlDataReader);
                con.Close();

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        BanquetType banquettype = new BanquetType();
                        banquettype.Id = Convert.ToInt64(dr["id"]);
                        banquettype.Name = Convert.ToString(dr["name"]);
                        banquettypes.Add(banquettype);
                    }
                }
                apiResponse.message = "BanquetTypes fetched successfully!";
                apiResponse.data = banquettypes;
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
        [Route("SaveBanquetType")]
        public IActionResult SaveBanquetType([FromBody] SaveBanquetType model)
        {
            apiResponse apiResponse = new apiResponse();
            //dynamic response = new ExpandoObject();
            //con.ConnectionString = _connClass.GetChildConnectionString(); //ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty;
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveBanquetType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                // Retrieve and assign parameters
                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 500).Value = model.Name ?? string.Empty;
                cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                if (model.Id > 0)
                {
                    apiResponse.message = "BanquetType updated successfully!";
                }
                else
                {
                    apiResponse.message = "BanquetType inserted successfully!";
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

        [HttpDelete("DeleteBanquetType/{id}")]
        public IActionResult DeleteBanquetType(Int64 id)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteBanquetType", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = id;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                apiResponse.message = "BanquetType deleted successfully!";
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
        [Route("GetBanquetTypeDetailById/{id}")]
        public IActionResult GetBanquetTypeDetailById(Int64 id)
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
                    SqlCommand cmd = new SqlCommand("GetBanquetTypeList", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = id;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                    conn.Close();
                }

                BanquetType banquettype = new BanquetType();
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        banquettype.Id = Convert.ToInt64(dr["id"]);
                        banquettype.Name = Convert.ToString(dr["name"]) ?? string.Empty;
                    }
                    apiResponse.message = "BanquetType fetched successfully!";
                    apiResponse.data = banquettype;
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
    }
}
