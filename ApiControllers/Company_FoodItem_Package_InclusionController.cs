using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using System.Xml.Linq;
using ExcelDataReader.Log;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Company_FoodItem_Package_InclusionController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_FoodItem_Package_InclusionController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            //_configuration = configuration;
            _imageUploadService = imageUploadService;

            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";

            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("DeleteCompanyFoodPackageInclusion")]
        public IActionResult DeleteCompanyFoodPackageInclusion([FromBody] DeleteCompanyFoodPackageInclusionRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteCompanyFoodPackageInclusion", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id;
                        cmd.Parameters.Add("@deleted_by", SqlDbType.BigInt).Value = model.DeletedBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                apiResponse.message = "Food package inclusion deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetCompanyFoodPackageInclusions")]
        public IActionResult GetCompanyFoodPackageInclusions([FromBody] GetCompanyFoodPackageInclusionsRequest model)
        {
            var apiResponse = new apiResponse();
            var packagesDict = new Dictionary<long, GetCompanyFoodPackageInclusionsResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCompanyFoodPackageInclusions", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@foodpackage_id", SqlDbType.BigInt).Value = model.FoodPackageId ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@isIncluded", SqlDbType.Bit).Value = model.IsIncluded ?? (object)DBNull.Value;

                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pkgId = Convert.ToInt64(reader["FoodPackageId"]);
                            if (!packagesDict.ContainsKey(pkgId))
                            {
                                packagesDict[pkgId] = new GetCompanyFoodPackageInclusionsResponse
                                {
                                    FoodPackageId = pkgId,
                                    FoodPackageName = Convert.ToString(reader["FoodPackageName"]),
                                    //IsIncluded = reader["IsIncluded"] != DBNull.Value ? (bool?)Convert.ToBoolean(reader["IsIncluded"]) : null
                                    IsIncluded = reader["IsIncluded"] != DBNull.Value ? (Convert.ToInt32(reader["IsIncluded"]) == 1) : (bool?)null
                                };
                            }

                            var pkg = packagesDict[pkgId];

                            // Add Menu Header
                            var headerId = Convert.ToInt64(reader["MenuHeaderId"]);
                            var header = pkg.MenuList.FirstOrDefault(h => h.MenuHeaderId == headerId);
                            if (header == null)
                            {
                                header = new GetCompanyFoodPackageInclusionsResponse_MenuHeader
                                {
                                    Id = Convert.ToInt64(reader["Id"]),
                                    MenuHeaderId = headerId,
                                    MenuHeaderName = Convert.ToString(reader["MenuHeaderName"]),
                                    CountSize = reader["CountSize"] != DBNull.Value ? (long?)Convert.ToInt64(reader["CountSize"]) : null
                                };
                                pkg.MenuList.Add(header);
                            }

                            // Add Menu Items
                            var menuItemId = reader["MenuItemId"] != DBNull.Value ? (long?)Convert.ToInt64(reader["MenuItemId"]) : null;
                            var menuItemName = reader["MenuItemName"] != DBNull.Value ? Convert.ToString(reader["MenuItemName"]) : null;

                            if (menuItemId != null && menuItemName != null)
                            {
                                header.ItemsList.Add(new GetCompanyFoodPackageInclusionsResponse_MenuItems
                                {
                                    MenuItemId = menuItemId,
                                    MenuItemName = menuItemName
                                });
                            }
                        }
                    }
                }

                var resultList = packagesDict.Values.ToList();
                if (resultList.Count > 0)
                {
                    apiResponse.status = "Success";
                    apiResponse.message = "Food package inclusions fetched successfully!";
                    apiResponse.data = resultList;
                }
                else
                {
                    apiResponse.status = "Fail";
                    apiResponse.message = "No data found.";
                    apiResponse.data = null;
                }

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


        [HttpPost]
        [Route("SaveCompanyFoodPackageInclusion")]
        public IActionResult SaveCompanyFoodPackageInclusion([FromBody] SaveCompanyFoodPackageInclusionRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SaveCompanyFoodPackageInclusion", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = model.Id ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@foodpackage_id", SqlDbType.BigInt).Value = model.FoodPackageId;
                    cmd.Parameters.Add("@inclusion_yesno", SqlDbType.NVarChar, 200).Value = model.InclusionYesNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@action_by", SqlDbType.BigInt).Value = model.ActionBy ?? (object)DBNull.Value;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.message = model.Id.HasValue && model.Id > 0
                    ? "Food package inclusion updated successfully!"
                    : "Food package inclusion added successfully!";

                apiResponse.status = "Success";
                return Ok(apiResponse); //return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }


    }
}

public class DeleteCompanyFoodPackageInclusionRequest
{
    public long Id { get; set; }
    public long DeletedBy { get; set; }
}
public class GetCompanyFoodPackageInclusionsRequest
{
    public long? Id { get; set; }
    public long? FoodPackageId { get; set; }
    public bool? IsIncluded { get; set; }
}
public class GetCompanyFoodPackageInclusionsResponse
{
    public long? FoodPackageId { get; set; }
    public string? FoodPackageName { get; set; }
    public bool? IsIncluded { get; set; }
    public List<GetCompanyFoodPackageInclusionsResponse_MenuHeader> MenuList { get; set; } = new();
}
public class GetCompanyFoodPackageInclusionsResponse_MenuHeader
{
    public long? Id { get; set; }
    public long? MenuHeaderId { get; set; }
    public string? MenuHeaderName { get; set; }
    public long? CountSize { get; set; }
    public List<GetCompanyFoodPackageInclusionsResponse_MenuItems> ItemsList { get; set; } = new();
}
public class GetCompanyFoodPackageInclusionsResponse_MenuItems
{
    public long? MenuItemId { get; set; }
    public string? MenuItemName { get; set; }
}

public class SaveCompanyFoodPackageInclusionRequest
{
    public long? Id { get; set; }
    public long FoodPackageId { get; set; }
    public string InclusionYesNo { get; set; }
    public long? ActionBy { get; set; }
}


