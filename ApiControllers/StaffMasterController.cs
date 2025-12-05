
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.EMMA;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

public class StaffMasterController : MasterController
{
    private readonly string domainUrl;
    private string connString = string.Empty;
    private readonly ImageUploadService _imageUploadService;

    public StaffMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService)
        : base(configuration, httpContextAccessor)
    {
        _imageUploadService = imageUploadService;
        var request = httpContextAccessor.HttpContext?.Request;
        domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
    }

    // ====================== GET ======================
    [HttpPost]
    [Route("GetStaffMaster")]
    public IActionResult GetStaffMaster([FromBody] GetStaffMasterRequest request)
    {
        apiResponse apiResponse = new apiResponse();
        List<StaffMasterModel> list = new List<StaffMasterModel>();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("GetStaffMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new StaffMasterModel
                {
                    Id = Convert.ToInt64(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Emp_Code = Convert.ToString(row["Emp_Code"]),
                    Address = Convert.ToString(row["Address"]),
                    PhoneNo = Convert.ToString(row["PhoneNo"]),
                    Emergency_PhoneNo = Convert.ToString(row["Emergency_PhoneNo"]),
                    Email = Convert.ToString(row["Email"]),
                    DesignationId = Convert.ToInt64(row["DesignationId"]),
                    AadharCardNo = Convert.ToString(row["AadharCardNo"]),
                    JoiningDate = row["JoiningDate"] == DBNull.Value ? null : Convert.ToString(row["JoiningDate"]),
                    Salary = row["Salary"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Salary"]),
                    //Photo = Convert.ToString(row["Photo"]),
                    Photo = row["Photo"] != DBNull.Value
                                && !string.IsNullOrEmpty(row["Photo"].ToString())
                            ? domainUrl + "/uploads/org/" + domain + "/StaffMaster/Photo/" + row["Photo"].ToString()
                            : string.Empty,

                    User_Name = Convert.ToString(row["UserName"]),
                    Password = Convert.ToString(row["Password"]),
                    IsSuperUser = row["IsSuperUser"] == DBNull.Value ? null : Convert.ToBoolean(row["IsSuperUser"]),
                    IsAutoLogin = row["IsAutoLogin"] == DBNull.Value ? null : Convert.ToBoolean(row["IsAutoLogin"]),
                    Bank_Name = Convert.ToString(row["Bank_Name"]),
                    Bank_BranchName = Convert.ToString(row["Bank_BranchName"]),
                    Bank_IFSC = Convert.ToString(row["Bank_IFSC"]),
                    Bank_AccountNo = Convert.ToString(row["Bank_AccountNo"]),
                    Bank_AccountHolderName = Convert.ToString(row["Bank_AccountHolderName"]),
                    Bank_UPI_Id = Convert.ToString(row["Bank_UPI_Id"])
                });
            }

            apiResponse.message = "Staff list fetched successfully!";
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

    // ====================== SAVE ======================
    [HttpPost]
    [Route("SaveStaffMaster")]
    public IActionResult SaveStaffMaster([FromBody] SaveStaffMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            long rowId = 0;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("SaveStaffMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@Name", (object)model.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Emp_Code", (object)model.Emp_Code ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)model.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNo", (object)model.PhoneNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Emergency_PhoneNo", (object)model.Emergency_PhoneNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)model.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DesignationId", model.DesignationId);

                cmd.Parameters.AddWithValue("@AadharCardNo", (object)model.AadharCardNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@JoiningDate", (object)model.JoiningDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Salary", model.Salary);

                cmd.Parameters.Add("@Photo", SqlDbType.NVarChar).Value = DBNull.Value;
                cmd.Parameters.AddWithValue("@UserName", (object)model.User_Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Password", (object)model.Password ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsSuperUser", (object)model.IsSuperUser ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsAutoLogin", (object)model.IsAutoLogin ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@Bank_Name", (object)model.Bank_Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bank_BranchName", (object)model.Bank_BranchName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bank_IFSC", (object)model.Bank_IFSC ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bank_AccountNo", (object)model.Bank_AccountNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bank_AccountHolderName", (object)model.Bank_AccountHolderName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bank_UPI_Id", (object)model.Bank_UPI_Id ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                rowId = Convert.ToInt64(cmd.ExecuteScalar());
                string latestImagePath = string.Empty;
                //photo upload
                if (model.Photo != string.Empty && model.Photo != null)
                {
                    string imgType = _imageUploadService.GetFileTypeFromBase64(model.Photo);

                     string folderPath = $"org/{domain}/StaffMaster/Photo/"; // Updated to include domain
                    _imageUploadService.DeleteImages(folderPath, $"StaffId_{rowId.ToString()}");
                    model.Photo = _imageUploadService.UploadImage(model.Photo, folderPath, $"StaffId_{rowId.ToString()}", imgType);

                    //update table
                    SqlCommand cmd2 = new SqlCommand("Save_DocumentFile", con);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.Add("@Id", SqlDbType.BigInt).Value = rowId;
                    cmd2.Parameters.Add("@DocumentName", SqlDbType.NVarChar, -1).Value = model.Photo; // Kept max for nvarchar(max)
                    cmd2.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "StaffMaster";
                    cmd2.ExecuteNonQuery();

                    latestImagePath = $"{domainUrl}/uploads/org/{domain}/StaffMaster/Photo/{model.Photo}";
                }

            }

            apiResponse.message = model.Id > 0 ? "Staff updated successfully!" : "Staff inserted successfully!";
            apiResponse.data = new { StaffId = rowId };
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

    // ====================== DELETE ======================
    [HttpPost]
    [Route("DeleteStaffMaster")]
    public IActionResult DeleteStaffMaster([FromBody] DeleteStaffMasterRequest model)
    {
        apiResponse apiResponse = new apiResponse();

        try
        {
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                return errorResult;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("DeleteStaffMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            apiResponse.message = "Staff deleted successfully!";
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
