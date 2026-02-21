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
    public class UnitMasterController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public UnitMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetUnitMasterList")]
        public IActionResult GetUnitMasterList([FromBody] GetUnitMasterRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetUnitMaster> units = new List<GetUnitMaster>();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetUnitMasterList", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", SqlDbType.BigInt).Value = request.Id == 0 ? 0 : request.Id;

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                con.Close();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GetUnitMaster unit = new GetUnitMaster();
                        unit.Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["Id"]);
                        unit.UnitName = row["UnitName"] == DBNull.Value ? string.Empty : Convert.ToString(row["UnitName"]);
                        unit.ShortName = row["ShortName"] == DBNull.Value ? string.Empty : Convert.ToString(row["ShortName"]);
                        unit.IsActive = row["IsActive"] == DBNull.Value ? false : Convert.ToBoolean(row["IsActive"]);

                        units.Add(unit);
                    }
                }

                apiResponse.message = "Units fetched successfully!";
                apiResponse.data = units;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveUnitMaster")]
        public IActionResult SaveUnitMaster([FromBody] SaveUnitMaster model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("SaveUnitMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // Assign parameters
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                cmd.Parameters.Add("@UnitName", SqlDbType.NVarChar, 255).Value = model.UnitName ?? string.Empty;
                cmd.Parameters.Add("@ShortName", SqlDbType.NVarChar, 100).Value = model.ShortName ?? string.Empty;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy;

                con.Open();

                // The procedure returns @id
                object result = cmd.ExecuteScalar();
                con.Close();

                long returnedId = 0;
                if (result != null && long.TryParse(result.ToString(), out long newId))
                {
                    returnedId = newId;
                }

                if (model.Id > 0)
                {
                    apiResponse.message = "Unit updated successfully!";
                }
                else
                {
                    apiResponse.message = "Unit inserted successfully!";
                }

                apiResponse.status = "Success";
                apiResponse.data = new { Id = returnedId };
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log Error
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("DeleteUnitMaster")]
        public IActionResult DeleteUnitMaster([FromBody] DeleteUnitMaster request)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                SqlConnection conn = new SqlConnection(connString);
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteUnitMaster", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", SqlDbType.BigInt).Value = request.Id;
                    cmd.Parameters.AddWithValue("@actionBy", SqlDbType.Bit).Value = request.ActionBy;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                apiResponse.message = "Unit deleted successfully!";
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


    }
}
