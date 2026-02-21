using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text;
using System.Dynamic;
using ERPAdmin.Services;
using Newtonsoft.Json;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : MasterController
    {
        private readonly ImageUploadService _imageUploadService;
        private readonly string domainUrl;
        private string connString = string.Empty;
        private string httpProtocols = string.Empty;

        public AuthController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            httpProtocols = $"{request?.Scheme}://";

            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }
        /*
        [HttpGet]
        [Route("GetDomainByOrganizationCode")]
        public IActionResult GetDomainByOrganizationCode(string orgCode)
        {
            string connString = ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB_Master") ?? string.Empty;
            SqlConnection conn = new SqlConnection(connString);

            apiResponse apiResponse = new apiResponse();
            try
            {
                DataSet ds = new DataSet();
                SqlDataAdapter adapter;
                conn.Open();
                SqlCommand cmd = new SqlCommand("GetAdminClientByDomainOrCode", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DomainOrCode", SqlDbType.NVarChar).Value = orgCode; //DomainOrCode;
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                conn.Close();

                dynamic companyDetail = new ExpandoObject();
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["id"] != DBNull.Value && !string.IsNullOrEmpty(ds.Tables[0].Rows[0]["id"].ToString()))
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    companyDetail.domain = httpProtocols + Convert.ToString(dr["domain"]) ?? "";

                    apiResponse.data = companyDetail;
                    apiResponse.message = "domain fetched successfully.";
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.message = "Invalid organization code.";
                    apiResponse.status = "Failed";
                }
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
            finally
            {
                conn.Close();
            }
            //return apiResponse;
        }

        [HttpPost]
        [Route("EmployeeLogin")]
        public IActionResult EmployeeLogin([FromBody] Login login)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                DataSet ds = new DataSet();
                SqlDataAdapter adapter;
                SqlConnection conn = new SqlConnection(connString);
                conn.Open();
                SqlCommand cmd = new SqlCommand("EmployeeLogin", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", SqlDbType.NVarChar).Value = login.Username;
                cmd.Parameters.AddWithValue("@Password", SqlDbType.NVarChar).Value = login.Password;
                cmd.Parameters.AddWithValue("@FCM", SqlDbType.NVarChar).Value = login.Fcm;
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                conn.Close();

                dynamic userDetail = new ExpandoObject();
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        userDetail.emp_id = Convert.ToInt64(dr["Employee_Id"]);
                        userDetail.name = Convert.ToString(dr["Name"]) ?? string.Empty;
                        userDetail.emp_code = Convert.ToString(dr["Emp_Code"]);
                        userDetail.designation = Convert.ToString(dr["Designation"]);
                        userDetail.address = Convert.ToString(dr["Address"]);
                        userDetail.contact_no = Convert.ToString(dr["Contact_No"]);
                        userDetail.mail_id = Convert.ToString(dr["Mail_Id"]);
                        userDetail.birthdate = dr["Birthdate"] != DBNull.Value ? Convert.ToDateTime(dr["Birthdate"]).ToString("yyyy-MM-dd") : null;
                        userDetail.joindate = dr["Joining_Date"] != DBNull.Value ? Convert.ToDateTime(dr["Joining_Date"]).ToString("yyyy-MM-dd") : null;
                        userDetail.birthdate_SHOW = dr["Birthdate"] != DBNull.Value ? Convert.ToDateTime(dr["Birthdate"]).ToString("dd-MMM-yyyy") : null;
                        userDetail.joindate_SHOW = dr["Joining_Date"] != DBNull.Value ? Convert.ToDateTime(dr["Joining_Date"]).ToString("dd-MMM-yyyy") : null;
                        userDetail.blood_group = Convert.ToString(dr["Blood_Group"]);
                        //userDetail.photo = "/uploads/employeePic/" + dr["Photo"].ToString();
                        //userDetail.photo = "/uploads/org/" + domain + "/employeePic/" + dr["Photo"].ToString();
                        userDetail.userimage = dr["Photo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Photo"].ToString())
                            ? $"{domainUrl}uploads/org/{domain}/employeePic/{dr["Photo"].ToString()}"
                            : $"{domainUrl}uploads/org/{domain}/employeePic/nsquare.png";

                        userDetail.aadhar_card_no = Convert.ToString(dr["Aadhar_Card_No"]);
                        userDetail.emergency_contact_no = Convert.ToString(dr["Emergency_Contact_No"]);
                        userDetail.bank_account_no = Convert.ToString(dr["Bank_Account_No"]);
                        userDetail.bank_branch = Convert.ToString(dr["Bank_Branch"]);
                        userDetail.bank_ifsc = Convert.ToString(dr["Bank_ifsc"]);
                        userDetail.bank_name = Convert.ToString(dr["Bank_Name"]);
                        userDetail.username = Convert.ToString(dr["User_Name"]);
                        userDetail.issuperuser = Convert.ToBoolean(dr["IsSuperUser"]);
                        userDetail.isautologin = Convert.ToBoolean(dr["IsAutoLogin"]);

                        userDetail.companyid = dr["Company_Id"] != DBNull.Value ? Convert.ToInt64(dr["Company_Id"]) : 0;
                        userDetail.companyname = dr["Company_Name"] != DBNull.Value && !string.IsNullOrEmpty(dr["Company_Name"].ToString())
                            ? dr["Company_Name"].ToString()
                            : "NSquare";
                        //userDetail.companylogo = Convert.ToString(dr["Company_Logo"]) ?? "/assets/img/icons/spot-illustrations/falcon.png";
                        //userDetail.companylogo = dr["Company_Logo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Company_Logo"].ToString())
                        //    ? domainUrl + "uploads/companyPic/" + dr["Company_Logo"].ToString()
                        //    : domainUrl + "uploads/companyPic/nsquare.png";
                        userDetail.companylogo = dr["Company_Logo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Company_Logo"].ToString())
                            ? $"{domainUrl}uploads/org/{domain}/companyPic/{dr["Company_Logo"].ToString()}"
                            : $"{domainUrl}uploads/org/{domain}/companyPic/nsquare.png";
                    }

                    //generate token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes("ERPADMIN975312498fhjfjh@ksjf&fkjbf#");
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "username"),
                            // Add additional claims as needed
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        Audience = "ERP-Mobile",
                        Issuer = "ERP-API",
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);
                    userDetail.token = tokenString;

                    apiResponse.data = userDetail;
                    apiResponse.message = "Login success.";
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.data = null;
                    apiResponse.message = "Username or password incorrect!";
                    apiResponse.status = "Failed";
                }
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
            //return apiResponse;
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] Login login)
        {
            apiResponse apiResponse = new apiResponse();            
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }
                SqlConnection conn = new SqlConnection(connString);
                DataSet ds = new DataSet();
                SqlDataAdapter adapter;
                conn.Open();
                SqlCommand cmd = new SqlCommand("UserLogin", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", SqlDbType.NVarChar).Value = login.Username;
                cmd.Parameters.AddWithValue("@Password", SqlDbType.NVarChar).Value = login.Password;
                cmd.Parameters.AddWithValue("@FCM", SqlDbType.NVarChar).Value = login.Fcm;
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                conn.Close();

                dynamic userDetail = new ExpandoObject();
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        userDetail.userid = Convert.ToInt64(dr["User_Id"]);
                        userDetail.username = Convert.ToString(dr["UserName"]) ?? "";
                        userDetail.userimage = dr["Photo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Photo"].ToString())
                            ? $"{domainUrl}uploads/org/{domain}/employeePic/{dr["Photo"].ToString()}"
                            : $"{domainUrl}uploads/org/{domain}/employeePic/nsquare.png";

                        userDetail.companyid = dr["Company_Id"] != DBNull.Value ? Convert.ToInt64(dr["Company_Id"]) : 0;
                        userDetail.companyname = dr["Company_Name"] != DBNull.Value && !string.IsNullOrEmpty(dr["Company_Name"].ToString())
                            ? dr["Company_Name"].ToString()
                            : "NSquare";
                        //userDetail.companylogo = Convert.ToString(dr["Company_Logo"]) ?? "/assets/img/icons/spot-illustrations/falcon.png";
                        //userDetail.companylogo = dr["Company_Logo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Company_Logo"].ToString())
                        //    ? domainUrl + "uploads/companyPic/" + dr["Company_Logo"].ToString()
                        //    : domainUrl + "uploads/companyPic/nsquare.png";
                        userDetail.companylogo = dr["Company_Logo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Company_Logo"].ToString())
                            ? $"{domainUrl}uploads/org/{domain}/companyPic/{dr["Company_Logo"].ToString()}"
                            : $"{domainUrl}uploads/org/{domain}/companyPic/nsquare.png";
                        userDetail.issuperuser = Convert.ToBoolean(dr["IsSuperUser"]);
                    }

                    //generate token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes("ERPADMIN975312498fhjfjh@ksjf&fkjbf#");
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "username"),
                            // Add additional claims as needed
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        Audience = "ERP-Mobile",
                        Issuer = "ERP-API",
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);
                    userDetail.token = tokenString;

                    apiResponse.data = userDetail;
                    apiResponse.message = "Login success.";
                    apiResponse.status = "Success";
                }
                else
                {
                    apiResponse.data = null;
                    apiResponse.message = "Username or password incorrect!";
                    apiResponse.status = "Failed";
                }
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

        [HttpPost("CheckLogin")]
        public IActionResult CheckLogin([FromBody] LoginInfo loginInfo)
        {
            // Validate if userId, userName, and token are present
            if (string.IsNullOrEmpty(loginInfo.UserId) || string.IsNullOrEmpty(loginInfo.UserName) || string.IsNullOrEmpty(loginInfo.Token))
            {
                return Unauthorized(new { status = "Error", message = "Invalid session data" });
            }

            // Validate the token
            bool isValidToken = ValidateToken(loginInfo.Token);
            if (!isValidToken)
            {
                return Unauthorized(new { status = "Error", message = "Invalid or expired token" });
            }

            // Further checks like verifying user from database, etc.
            var user = GetUserFromDatabase(loginInfo.UserId, loginInfo.UserName);
            if (user != null && (user.UserName == loginInfo.UserName) )
            {
                return Ok(new { status = "Success", message = "Login successful" });
            }

            return Unauthorized(new { status = "Error", message = "User not found" });
        }

        //[HttpPost("ValidateToken")]
        //validate token
        private bool ValidateToken(string token)
        {
            try
            {
                // Token handler to read and validate the token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("ERPADMIN975312498fhjfjh@ksjf&fkjbf#"); // Use the same key for validation

                // Parameters to validate the token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "ERP-API",  // The issuer from token creation
                    ValidateAudience = true,
                    ValidAudience = "ERP-Mobile",  // The audience from token creation
                    ValidateLifetime = true,  // Validate the expiration date
                    IssuerSigningKey = new SymmetricSecurityKey(key),  // Signing key used in token generation
                    ClockSkew = TimeSpan.Zero  // Prevents slight time discrepancies
                };

                // Validate the token and extract the claims
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // If token validation succeeds, return true
                return true;
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., token expired, invalid signature, etc.)
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return false;
            }
        }

        // Example function to get user from the database (you should implement this)
        private User GetUserFromDatabase(string userId, string userName)
        {
            string connString = _connClass.GetChildConnectionString(); //ConfigurationExtensions.GetConnectionString(_configuration, "ERPAdminDB") ?? string.Empty;

            DataSet ds = new DataSet();
            SqlDataAdapter adapter;
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            SqlCommand cmd = new SqlCommand("GetUserById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", SqlDbType.BigInt).Value = userId;
            cmd.Parameters.AddWithValue("@UserName", SqlDbType.NVarChar).Value = userName;
            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(ds);
            conn.Close();

            dynamic userDetail = new ExpandoObject();
            DataTable dt = new DataTable();
            dt = ds.Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    userDetail.userid = Convert.ToInt64(dr["User_Id"]);
                    userDetail.username = Convert.ToString(dr["UserName"]) ?? "";
                    //userDetail.email = Convert.ToString(dr["Email"]) ?? "";
                }
            }
            else
            {
                return null;
            }

            // Fetch user database
            return new User
            {
                UserId = Convert.ToString(userDetail.userid),
                UserName = userDetail.username,
                //Email = userDetail.email
            };
        }
        */
    }
}
public class LoginInfo
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Token { get; set; }
}

public class User
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    //public string? Email{ get; set; }
}
