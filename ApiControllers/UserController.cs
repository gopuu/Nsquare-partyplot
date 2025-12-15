using ERPAdmin.Helper;
using ERPAdmin.Models;
using ExcelDataReader.Log;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ERPAdmin.ApiControllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : MasterController
    {
        private string connString = string.Empty;
        private readonly string domainUrl;
        private string httpProtocols = string.Empty;

        public UserController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            httpProtocols = $"{request?.Scheme}://";

        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] Login login)
        {
            apiResponse apiResponse = new apiResponse();
                        
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand("UserLogin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = login.Username ?? "";
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = login.Password ?? "";
                        cmd.Parameters.Add("@FCM", SqlDbType.NVarChar).Value = login.Fcm ?? "";

                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);

                        conn.Open();
                        da.Fill(dt);
                        conn.Close();

                        if (dt.Rows.Count == 0)
                        {
                            apiResponse.status = "Failed";
                            apiResponse.message = "Username or password incorrect!";
                            apiResponse.data = null;
                            return Ok(apiResponse);
                        }

                        DataRow dr = dt.Rows[0];

                        dynamic userDetail = new ExpandoObject();
                        userDetail.userid = Convert.ToInt64(dr["UserId"]);
                        userDetail.username = Convert.ToString(dr["Username"]) ?? "";

                        userDetail.IsSuperUser = Convert.ToBoolean(dr["IsSuperUser"]);
                        userDetail.Name = Convert.ToString(dr["Name"]) ?? "";
                        userDetail.Emp_Code = Convert.ToString(dr["Emp_Code"]);
                        userDetail.Address = Convert.ToString(dr["Address"]) ?? "";
                        userDetail.PhoneNo = Convert.ToString(dr["PhoneNo"]);
                        userDetail.Emergency_PhoneNo = Convert.ToString(dr["Emergency_PhoneNo"]) ?? "";
                        userDetail.Email = Convert.ToString(dr["Email"]);
                        userDetail.DesignationId = Convert.ToString(dr["DesignationId"]) ?? "";

                        userDetail.AadharCardNo = Convert.ToString(dr["AadharCardNo"]);
                        userDetail.JoiningDate = Convert.ToString(dr["JoiningDate"]) ?? "";
                        userDetail.Salary = Convert.ToDecimal(dr["Salary"]);
                        userDetail.Bank_Name = Convert.ToString(dr["Bank_Name"]) ?? "";
                        userDetail.Bank_BranchName = Convert.ToString(dr["Bank_BranchName"]);
                        userDetail.Bank_IFSC = Convert.ToString(dr["Bank_IFSC"]) ?? "";
                        userDetail.Bank_AccountNo = Convert.ToString(dr["Bank_AccountNo"]);
                        userDetail.Bank_AccountHolderName = Convert.ToString(dr["Bank_AccountHolderName"]) ?? "";
                        userDetail.Bank_UPI_Id = Convert.ToString(dr["Bank_UPI_Id"]);
                        userDetail.BirthDate = Convert.ToString(dr["BirthDate"]) ?? "";
                        userDetail.AnniversaryDate = Convert.ToString(dr["AnniversaryDate"]);
                        userDetail.IsAutoLogin = Convert.ToString(dr["IsAutoLogin"]) ?? "";

                        userDetail.FCM = Convert.ToString(dr["FCM"]);
                        userDetail.Company_Id = Convert.ToString(dr["Company_Id"]) ?? "";
                        userDetail.Company_Name = Convert.ToString(dr["Company_Name"]);
                        userDetail.Company_Logo = Convert.ToString(dr["Company_Logo"]) ?? "";

                        // User Photo
                        userDetail.userimage =
                            dr["Photo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Photo"].ToString())
                            ? $"{domainUrl}uploads/org/{domain}/StaffMaster/Photo/{dr["Photo"]}"
                            : $"{domainUrl}uploads/org/{domain}/StaffMaster/Photo/nsquare.png";

                        // Company Info
                        userDetail.companyid =
                            dr["Company_Id"] != DBNull.Value ? Convert.ToInt64(dr["Company_Id"]) : 0;

                        userDetail.companyname =
                            dr["Company_Name"] != DBNull.Value
                            ? dr["Company_Name"].ToString()
                            : "NSquare";

                        userDetail.companylogo = dr["Company_Logo"].ToString();
                        userDetail.issuperuser = Convert.ToBoolean(dr["IsSuperUser"]);

                        // ===== JWT TOKEN =====
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Encoding.ASCII.GetBytes("ERPADMIN975312498fhjfjh@ksjf&fkjbf#");

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new[]
                            {
                        new Claim(ClaimTypes.NameIdentifier, userDetail.userid.ToString()),
                        new Claim(ClaimTypes.Name, userDetail.username)
                    }),
                            Expires = DateTime.UtcNow.AddDays(7),
                            Issuer = "ERP-API",
                            Audience = "ERP-Mobile",
                            SigningCredentials = new SigningCredentials(
                                new SymmetricSecurityKey(key),
                                SecurityAlgorithms.HmacSha256Signature)
                        };

                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        userDetail.token = tokenHandler.WriteToken(token);

                        apiResponse.status = "Success";
                        apiResponse.message = "Login success.";
                        apiResponse.data = userDetail;

                        return Ok(apiResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse);
            }
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
