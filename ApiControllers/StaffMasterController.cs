
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.EMMA;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            if (!TryGetConnectionAndDomain(
                ControllerContext, Request,
                out string connString,
                out string domain,
                out IActionResult? errorResult))
                return errorResult!;

            using (SqlConnection con = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("GetStaffMaster", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request.Id == 0 ? 0 : request.Id);

                con.Open();
                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    var staff = new StaffMasterModel
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        Name = Convert.ToString(dr["Name"]),
                        Emp_Code = Convert.ToString(dr["Emp_Code"]),
                        Address = Convert.ToString(dr["Address"]),
                        PhoneNo = Convert.ToString(dr["PhoneNo"]),
                        Emergency_PhoneNo = Convert.ToString(dr["Emergency_PhoneNo"]),
                        Email = Convert.ToString(dr["Email"]),
                        DesignationId = Convert.ToInt64(dr["DesignationId"]),
                        AadharCardNo = Convert.ToString(dr["AadharCardNo"]),
                        JoiningDate = dr["JoiningDate"] == DBNull.Value ? null : dr["JoiningDate"].ToString(),
                        BirthDate = dr["BirthDate"] == DBNull.Value ? null : dr["BirthDate"].ToString(),
                        AnniversaryDate = dr["AnniversaryDate"] == DBNull.Value ? null : dr["AnniversaryDate"].ToString(),
                        JoiningDate_Format = dr["JoiningDate_Format"] == DBNull.Value ? null : dr["JoiningDate_Format"].ToString(),
                        BirthDate_Format = dr["BirthDate_Format"] == DBNull.Value ? null : dr["BirthDate_Format"].ToString(),
                        AnniversaryDate_Format = dr["AnniversaryDate_Format"] == DBNull.Value ? null : dr["AnniversaryDate_Format"].ToString(),
                        Salary = dr["Salary"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Salary"]),

                        Photo = dr["Photo"] != DBNull.Value && !string.IsNullOrEmpty(dr["Photo"].ToString())
                            ? $"{domainUrl}/uploads/org/{domain}/StaffMaster/Photo/{dr["Photo"]}"
                            : string.Empty,

                        User_Name = Convert.ToString(dr["UserName"]),
                        Password = Convert.ToString(dr["Password"]),
                        IsSuperUser = dr["IsSuperUser"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsSuperUser"]),
                        IsAutoLogin = dr["IsAutoLogin"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsAutoLogin"]),
                        IsActive = dr["IsActive"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsActive"]),

                        Bank_Name = Convert.ToString(dr["Bank_Name"]),
                        Bank_BranchName = Convert.ToString(dr["Bank_BranchName"]),
                        Bank_IFSC = Convert.ToString(dr["Bank_IFSC"]),
                        Bank_AccountNo = Convert.ToString(dr["Bank_AccountNo"]),
                        Bank_AccountHolderName = Convert.ToString(dr["Bank_AccountHolderName"]),
                        Bank_UPI_Id = Convert.ToString(dr["Bank_UPI_Id"]),

                        Documents = new List<StaffDocumentsModel2>()
                    };

                    // 🔹 Deserialize documents JSON
                    if (dr["StaffDocumentsJSON"] != DBNull.Value)
                    {
                        var docs = JsonConvert.DeserializeObject<List<StaffDocumentsModel2>>(dr["StaffDocumentsJSON"].ToString());
                        foreach (var d in docs)
                        {
                            d.FilePath = !string.IsNullOrEmpty(d.FilePath)
                                ? $"{domainUrl}/uploads/org/{domain}/StaffMaster/Documents/{d.FilePath}"
                                : string.Empty;
                        }
                        staff.Documents = docs;
                    }
                    list.Add(staff);
                }
            }

            apiResponse.status = "Success";
            apiResponse.message = "Staff data fetched successfully!";
            apiResponse.data = list;
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogException(ex, ControllerContext, Request, null);
            return StatusCode(500, new apiResponse
            {
                status = "Error",
                message = ex.Message
            });
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
            if (!TryGetConnectionAndDomain(ControllerContext, Request,
                out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

            #region FILE VALIDATION

            // Profile Photo: Images only, max 1 MB
            if (!string.IsNullOrWhiteSpace(model.Photo))
            {
                try
                {
                    _imageUploadService.ValidateBase64File(
                        model.Photo,
                        new[] { ".jpg", ".jpeg", ".png" },
                        1
                    );
                }
                catch (Exception ex)
                {
                    return BadRequest(new apiResponse
                    {
                        status = "Error",
                        message = $"Profile photo: {ex.Message}"
                    });
                }
            }

            // Staff Documents: PDF + Images, max 1 MB each
            if (model.Documents != null && model.Documents.Any())
            {
                foreach (var doc in model.Documents)
                {
                    if (string.IsNullOrWhiteSpace(doc.FileBase64))
                        continue;

                    try
                    {
                        _imageUploadService.ValidateBase64File(
                            doc.FileBase64,
                            new[] { ".pdf", ".jpg", ".jpeg", ".png" },
                            1
                        );
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new apiResponse
                        {
                            status = "Error",
                            message = $"{doc.DocumentName}: {ex.Message}"
                        });
                    }
                }
            }

            #endregion

            long staffId;

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                #region SAVE STAFF MASTER

                using (SqlCommand cmd = new SqlCommand("SaveStaffMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Name", (object?)model.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Emp_Code", (object?)model.Emp_Code ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNo", (object?)model.PhoneNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Emergency_PhoneNo", (object?)model.Emergency_PhoneNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DesignationId", model.DesignationId);

                    cmd.Parameters.AddWithValue("@AadharCardNo", (object?)model.AadharCardNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@JoiningDate", (object?)model.JoiningDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BirthDate", (object?)model.BirthDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AnniversaryDate", (object?)model.AnniversaryDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Salary", model.Salary);

                    cmd.Parameters.AddWithValue("@UserName", (object?)model.User_Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Password", (object?)model.Password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsSuperUser", model.IsSuperUser);
                    cmd.Parameters.AddWithValue("@IsAutoLogin", model.IsAutoLogin);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                    cmd.Parameters.AddWithValue("@Bank_Name", (object?)model.Bank_Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bank_BranchName", (object?)model.Bank_BranchName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bank_IFSC", (object?)model.Bank_IFSC ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bank_AccountNo", (object?)model.Bank_AccountNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bank_AccountHolderName", (object?)model.Bank_AccountHolderName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bank_UPI_Id", (object?)model.Bank_UPI_Id ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    staffId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                #endregion

                #region PHOTO UPLOAD

                if (!string.IsNullOrWhiteSpace(model.Photo))
                {
                    string fileType = _imageUploadService.GetFileTypeFromBase64(model.Photo);
                    string folderPath = $"org/{domain}/StaffMaster/Photo/";

                    _imageUploadService.DeleteImages(
                        folderPath,
                        $"StaffId_{staffId}"
                    );

                    string fileName = _imageUploadService.UploadImage(
                        model.Photo,
                        folderPath,
                        $"StaffId_{staffId}",
                        fileType
                    );

                    using SqlCommand photoCmd = new SqlCommand("Save_DocumentFile", con);
                    photoCmd.CommandType = CommandType.StoredProcedure;
                    photoCmd.Parameters.AddWithValue("@Id", staffId);
                    photoCmd.Parameters.AddWithValue("@DocumentName", fileName);
                    photoCmd.Parameters.AddWithValue("@TableName", "StaffMaster");

                    photoCmd.ExecuteNonQuery();
                }

                #endregion

                #region STAFF DOCUMENTS

                if (model.Documents != null && model.Documents.Any())
                {
                    foreach (var doc in model.Documents)
                    {
                        long docId;

                        using (SqlCommand docCmd = new SqlCommand("SaveStaffDocuments", con))
                        {
                            docCmd.CommandType = CommandType.StoredProcedure;

                            docCmd.Parameters.AddWithValue("@Id", doc.Id);
                            docCmd.Parameters.AddWithValue("@StaffId", staffId);
                            docCmd.Parameters.AddWithValue("@DocumentName", (object?)doc.DocumentName ?? DBNull.Value);
                            docCmd.Parameters.AddWithValue("@FilePath", DBNull.Value);
                            docCmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                            docId = Convert.ToInt64(docCmd.ExecuteScalar());
                        }

                        if (!string.IsNullOrWhiteSpace(doc.FileBase64))
                        {
                            string fileType = _imageUploadService.GetFileTypeFromBase64(doc.FileBase64);
                            string folderPath = $"org/{domain}/StaffMaster/Documents/";

                            _imageUploadService.DeleteImages(
                                folderPath,
                                $"StaffId_{staffId}_Doc_{docId}"
                            );

                            string fileName = _imageUploadService.UploadImage(
                                doc.FileBase64,
                                folderPath,
                                $"StaffId_{staffId}_Doc_{docId}",
                                fileType
                            );

                            using SqlCommand docFileCmd = new SqlCommand("Save_DocumentFile", con);
                            docFileCmd.CommandType = CommandType.StoredProcedure;
                            docFileCmd.Parameters.AddWithValue("@Id", docId);
                            docFileCmd.Parameters.AddWithValue("@DocumentName", fileName);
                            docFileCmd.Parameters.AddWithValue("@TableName", "StaffDocuments");

                            docFileCmd.ExecuteNonQuery();
                        }
                    }
                }

                #endregion
            }

            apiResponse.status = "Success";
            apiResponse.message = model.Id > 0
                ? "Staff updated successfully!"
                : "Staff inserted successfully!";

            apiResponse.data = new { StaffId = staffId };

            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogException(ex, ControllerContext, Request, null);

            apiResponse.status = "Error";
            apiResponse.message = ex.Message;

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
            if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                return errorResult!;

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

            apiResponse.message = ex.Message;
            apiResponse.status = "Error";
            return StatusCode(500, apiResponse);
        }
    }
}
