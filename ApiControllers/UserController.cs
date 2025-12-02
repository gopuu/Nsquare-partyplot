using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Humanizer;

namespace ERPAdmin.ApiControllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : MasterController
    {
        private string connString = string.Empty;

        public UserController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
        }
        /*
        // POST: api/<UserRightsController>
        [Route("GetUserRightsList")]
        [HttpPost]
        public IActionResult GetUserRightsList(UserRightsFetch userRightsFetch)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }


                DataSet ds = new DataSet();
                SqlDataAdapter adapter;
                SqlConnection conn = new SqlConnection(connString);
                conn.Open();
                SqlCommand cmd = new SqlCommand("GetUserRightsList", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmployeeId", SqlDbType.BigInt).Value = userRightsFetch.EmployeeId;
                cmd.Parameters.AddWithValue("@Ex1", SqlDbType.NVarChar).Value = userRightsFetch.Ex1;
                cmd.Parameters.AddWithValue("@Ex2", SqlDbType.NVarChar).Value = userRightsFetch.Ex2;
                cmd.Parameters.AddWithValue("@Ex3", SqlDbType.NVarChar).Value = userRightsFetch.Ex3;
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);

                conn.Close();

                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    List<UserRights> userRights = new List<UserRights>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        UserRights userRight = new UserRights();
                        userRight.ModuleId = Convert.ToInt64(dr["ModuleId"]); 
                        userRight.ModuleName = Convert.ToString(dr["ModuleName"]) ?? "";
                        userRight.Link = Convert.ToString(dr["Link"]) ?? "";
                        userRight.EmployeeId = Convert.ToInt64(dr["EmployeeId"]);
                        userRight.CanAdd = Convert.ToBoolean(dr["CanAdd"]);
                        userRight.CanEdit = Convert.ToBoolean(dr["CanEdit"]);
                        userRight.CanDelete = Convert.ToBoolean(dr["CanDelete"]);
                        userRight.CanView = Convert.ToBoolean(dr["CanView"]);

                        userRight.IsAddNeeded = Convert.ToBoolean(dr["IsAddNeeded"]);
                        userRight.IsEditNeeded = Convert.ToBoolean(dr["IsEditNeeded"]);
                        userRight.IsDeleteNeeded = Convert.ToBoolean(dr["IsDeleteNeeded"]);
                        userRight.IsViewNeeded = Convert.ToBoolean(dr["IsViewNeeded"]);
                        userRights.Add(userRight);
                    }
                    apiResponse.data = userRights;
                    apiResponse.message = "UserRights fetched successfully!";
                }
                else
                {
                    apiResponse.data = null;
                    apiResponse.message = "No Data found!";
                }
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
            //return apiResponse;
        }

        // POST api/<UserRightsController>/SaveUserRights
        [Route("SaveUserRights")]
        [HttpPost]
        public IActionResult SaveUserRights([FromBody] List<UserRightsSave> userRightsList)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                SqlConnection conn = new SqlConnection(connString);
                conn.Open();
                // Create DataTable to match UserRightsType structure
                DataTable tvp = new DataTable();
                tvp.Columns.Add("Module_Id", typeof(long));
                tvp.Columns.Add("Employee_Id", typeof(long));
                tvp.Columns.Add("Can_Add", typeof(bool));
                tvp.Columns.Add("Can_Edit", typeof(bool));
                tvp.Columns.Add("Can_Delete", typeof(bool));
                tvp.Columns.Add("Can_View", typeof(bool));
                tvp.Columns.Add("Action_By", typeof(long));

                // Populate DataTable from userRightsList
                foreach (var userRights in userRightsList)
                {
                    tvp.Rows.Add(
                        userRights.ModuleId,
                        (object?)userRights.EmployeeId ?? DBNull.Value,
                        (object?)userRights.CanAdd ?? DBNull.Value,
                        (object?)userRights.CanEdit ?? DBNull.Value,
                        (object?)userRights.CanDelete ?? DBNull.Value,
                        (object?)userRights.CanView ?? DBNull.Value,
                        (object?)userRights.ActionBy ?? DBNull.Value
                    );
                }
                // Execute the stored procedure with TVP
                SqlCommand cmd = new SqlCommand("SaveUserRights", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter param = cmd.Parameters.AddWithValue("@UserRightsList", tvp);
                param.SqlDbType = SqlDbType.Structured; // Specify table-valued parameter type
                cmd.ExecuteNonQuery();

                conn.Close();

                apiResponse.message = "UserRights saved!"; 
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

        // Get: api/<UserRightsController>
        [Route("GetMenuList")]
        [HttpGet]
        public IActionResult GetMenuList()
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                DataSet ds = new DataSet();
                SqlDataAdapter adapter;
                SqlConnection conn = new SqlConnection(connString);
                conn.Open();
                SqlCommand cmd = new SqlCommand("GetMenuList", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);

                conn.Close();
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    List<UserMenu> userMenus = new List<UserMenu>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        UserMenu userMenu = new UserMenu();
                        userMenu.Id = Convert.ToInt64(dr["ID"]);
                        userMenu.MenuName = Convert.ToString(dr["Menu_Name"]);
                        userMenu.IsVisible = Convert.ToBoolean(dr["Is_Visible"]);
                        userMenu.Priority = Convert.ToInt64(dr["priority"]);
                        userMenu.Type = Convert.ToString(dr["type"]);
                        userMenus.Add(userMenu);
                    }
                    apiResponse.data = userMenus;
                    apiResponse.message = "Menu fetched successfully!";
                }
                else
                {
                    apiResponse.data = null;
                    apiResponse.message = "No Data found!";
                }
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

        [HttpPost]
        [Route("SaveMenu")]
        public IActionResult SaveMenu([FromBody] UserMenu model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                { return errorResult; }

                SqlConnection con = new SqlConnection(connString);

                SqlCommand cmd = new SqlCommand("SaveMenu", con);
                cmd.CommandType = CommandType.StoredProcedure;
                 // Retrieve and assign parameters
                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id > 0 ? model.Id : 0;
                cmd.Parameters.Add("@Menu_Name", SqlDbType.NVarChar, 255).Value = model.MenuName ?? string.Empty;
                cmd.Parameters.Add("@Is_visible", SqlDbType.Bit).Value = model.IsVisible;
                cmd.Parameters.Add("@priority", SqlDbType.BigInt).Value = model.Priority > 0 ? model.Priority : 0;
                cmd.Parameters.Add("@type", SqlDbType.NVarChar, 100).Value = model.Type ?? string.Empty;

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                if (model.Id > 0)
                {
                    apiResponse.message = "Menu updated successfully!";
                }
                else
                {
                    apiResponse.message = "Menu inserted successfully!";
                }
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

        */
    }
}
