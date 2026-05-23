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
    public class Company_HallDetailsController : MasterController
    {
        //private readonly IConfiguration _configuration;
        private readonly ImageUploadService _imageUploadService;
        //private readonly string domainUrl = "http://tenxsoftwarek-001-site1.qtempurl.com/";
        private readonly string domainUrl;
        //private readonly ConnClass _connClass;
        private string connString = string.Empty;

        public Company_HallDetailsController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
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
        [Route("DeleteCompanyHallDetails")]
        public IActionResult DeleteCompanyHallDetails([FromBody] CompanyHallDeleteRequest request)
        {
            var apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("DeleteCompanyHallDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Hall details deleted successfully.";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }
        
        [HttpPost]
        [Route("GetCompanyHallDetailsList")]
        public IActionResult GetCompanyHallDetailsList([FromBody] GetCompanyHallDetailsRequest request)
        {
            var apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                List<dynamic> halls = new();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("GetCompanyHallDetailsList", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = (object)request.Id ?? DBNull.Value;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var hall = new
                            {
                                Id = reader["Id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Id"]),
                                HallName = reader["HallName"] == DBNull.Value ? string.Empty : reader["HallName"].ToString(),
                                HallName_Display = reader["HallName_Display"] == DBNull.Value ? string.Empty : reader["HallName_Display"].ToString(),
                                CapacityRange = reader["CapacityRange"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CapacityRange"]),
                                CapacityRangeText = reader["CapacityRangeText"] == DBNull.Value ? string.Empty : Convert.ToString(reader["CapacityRangeText"]),
                                RentPerDay = reader["RentPerDay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["RentPerDay"]),
                                IsPartitioned = reader["IsPartitioned"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsPartitioned"]),
                                Video360Link = reader["Video360Link"] == DBNull.Value ? string.Empty : reader["Video360Link"].ToString(),
                                PromoVideo1 = reader["PromoVideo1"] == DBNull.Value ? string.Empty : reader["PromoVideo1"].ToString(),
                                PromoVideo2 = reader["PromoVideo2"] == DBNull.Value ? string.Empty : reader["PromoVideo2"].ToString(),
                                Image1 = reader["Image1"] != DBNull.Value 
                                            && !string.IsNullOrEmpty(reader["Image1"].ToString())
                                        ? domainUrl + $"/uploads/org/{domain}/Company_HallRentAndDetails_Images/" + reader["Image1"].ToString()
                                        : string.Empty,

                                Image2 = reader["Image2"] != DBNull.Value 
                                            && !string.IsNullOrEmpty(reader["Image2"].ToString())
                                        ? domainUrl + $"/uploads/org/{domain}/Company_HallRentAndDetails_Images/" + reader["Image2"].ToString()
                                        : string.Empty,

                                Image3 = reader["Image3"] != DBNull.Value 
                                            && !string.IsNullOrEmpty(reader["Image3"].ToString())
                                        ? domainUrl + $"/uploads/org/{domain}/Company_HallRentAndDetails_Images/" + reader["Image3"].ToString()
                                        : string.Empty,

                                Image4 = reader["Image4"] != DBNull.Value 
                                            && !string.IsNullOrEmpty(reader["Image4"].ToString())
                                        ? domainUrl + $"/uploads/org/{domain}/Company_HallRentAndDetails_Images/" + reader["Image4"].ToString()
                                        : string.Empty,

                                Image5 = reader["Image5"] != DBNull.Value 
                                            && !string.IsNullOrEmpty(reader["Image5"].ToString())
                                        ? domainUrl + $"/uploads/org/{domain}/Company_HallRentAndDetails_Images/" + reader["Image5"].ToString()
                                        : string.Empty,

                                // ✅ Boolean values
                                GoodFor_GrandWedding = reader["GoodFor_GrandWedding"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_GrandWedding"]),
                                GoodFor_Weddings = reader["GoodFor_Weddings"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_Weddings"]),
                                GoodFor_BirthdayParty = reader["GoodFor_BirthdayParty"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_BirthdayParty"]),
                                GoodFor_Engagement = reader["GoodFor_Engagement"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_Engagement"]),
                                GoodFor_FamilyFunctions = reader["GoodFor_FamilyFunctions"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_FamilyFunctions"]),
                                GoodFor_Communion = reader["GoodFor_Communion"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_Communion"]),
                                GoodFor_BoardMeetings = reader["GoodFor_BoardMeetings"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_BoardMeetings"]),
                                GoodFor_ProductLaunch = reader["GoodFor_ProductLaunch"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_ProductLaunch"]),
                                GoodFor_OfficialEvent = reader["GoodFor_OfficialEvent"] == DBNull.Value ? false : Convert.ToBoolean(reader["GoodFor_OfficialEvent"]),

                                // ✅ Int64 values
                                Seating_Cluster = reader["Seating_Cluster"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Seating_Cluster"]),
                                Seating_Theatre = reader["Seating_Theatre"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Seating_Theatre"]),
                                Seating_UShape = reader["Seating_UShape"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Seating_UShape"]),
                                Seating_HollowSquare = reader["Seating_HollowSquare"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Seating_HollowSquare"]),
                                Seating_Boardroom = reader["Seating_Boardroom"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Seating_Boardroom"]),
                                Seating_DanceFloor = reader["Seating_DanceFloor"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Seating_DanceFloor"]),

                                // ✅ Boolean values (Amenities)
                                Amenity_ChangingRoom = reader["Amenity_ChangingRoom"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_ChangingRoom"]),
                                Amenity_AirCondition = reader["Amenity_AirCondition"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_AirCondition"]),
                                Amenity_CoupleChairs = reader["Amenity_CoupleChairs"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_CoupleChairs"]),
                                Amenity_Sofa = reader["Amenity_Sofa"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Sofa"]),
                                Amenity_MusicSystem = reader["Amenity_MusicSystem"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_MusicSystem"]),
                                Amenity_Mic = reader["Amenity_Mic"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Mic"]),
                                Amenity_Podium = reader["Amenity_Podium"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Podium"]),
                                Amenity_Lights = reader["Amenity_Lights"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Lights"]),
                                Amenity_FalseCeiling = reader["Amenity_FalseCeiling"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_FalseCeiling"]),
                                Amenity_Fans = reader["Amenity_Fans"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Fans"]),
                                Amenity_Cloakroom = reader["Amenity_Cloakroom"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Cloakroom"]),
                                Amenity_Lockers = reader["Amenity_Lockers"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Lockers"]),
                                Amenity_Projectors = reader["Amenity_Projectors"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Projectors"]),
                                Amenity_WiFi = reader["Amenity_WiFi"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_WiFi"]),
                                Amenity_Stage = reader["Amenity_Stage"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_Stage"]),
                                Amenity_WashRoom = reader["Amenity_WashRoom"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_WashRoom"]),
                                Amenity_PlasmaScreen = reader["Amenity_PlasmaScreen"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_PlasmaScreen"]),
                                Amenity_PowerBackup = reader["Amenity_PowerBackup"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_PowerBackup"]),
                                Amenity_BoardroomSetup = reader["Amenity_BoardroomSetup"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_BoardroomSetup"]),
                                Amenity_WallClock = reader["Amenity_WallClock"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_WallClock"]),
                                Amenity_ShoeRack = reader["Amenity_ShoeRack"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_ShoeRack"]),
                                Amenity_RedCarpet = reader["Amenity_RedCarpet"] == DBNull.Value ? false : Convert.ToBoolean(reader["Amenity_RedCarpet"]),
                                Is_Blocked = reader["Is_Blocked"] == DBNull.Value ? false : Convert.ToBoolean(reader["Is_Blocked"]),

                                CapacityWithPartition_Id = reader["CapacityWithPartition_Id"] == DBNull.Value ? (int?)null
                                        : Convert.ToInt32(reader["CapacityWithPartition_Id"]),

                                RentWithPartition = reader["RentWithPartition"] == DBNull.Value ? (decimal?)null
                                        : Convert.ToDecimal(reader["RentWithPartition"]),

                                IsHallMerged = reader["IsHallMerged"] == DBNull.Value ? false
                                        : Convert.ToBoolean(reader["IsHallMerged"]),

                                MergeHall_List = reader["MergeHall_JSON"] == DBNull.Value ? new List<MergeHall_List>()
                                        : JsonConvert.DeserializeObject<List<MergeHall_List>>(reader["MergeHall_JSON"].ToString()),
                            };
                            halls.Add(hall);
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Hall fetched successfully.";
                apiResponse.data = halls;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyHallDetails")]
        public IActionResult SaveCompanyHallDetails([FromBody] SaveCompanyHallDetailsRequest model)
        {
            var apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SaveCompanyHallDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@HallName", SqlDbType.VarChar, 255).Value = model.HallName;
                    cmd.Parameters.Add("@CapacityRange", SqlDbType.BigInt).Value = model.CapacityRange;
                    cmd.Parameters.Add("@RentPerDay", SqlDbType.Decimal).Value = model.RentPerDay;
                    cmd.Parameters.Add("@IsPartitioned", SqlDbType.Bit).Value = model.IsPartitioned;
                    cmd.Parameters.Add("@Video360Link", SqlDbType.VarChar, 500).Value = (object?)model.Video360Link ?? DBNull.Value;
                    cmd.Parameters.Add("@PromoVideo1", SqlDbType.VarChar, 500).Value = (object?)model.PromoVideo1 ?? DBNull.Value;
                    cmd.Parameters.Add("@PromoVideo2", SqlDbType.VarChar, 500).Value = (object?)model.PromoVideo2 ?? DBNull.Value;
                    cmd.Parameters.Add("@Image1", SqlDbType.VarChar, 500).Value = DBNull.Value; //(object?)model.Image1 ?? DBNull.Value;
                    cmd.Parameters.Add("@Image2", SqlDbType.VarChar, 500).Value = DBNull.Value; //(object?)model.Image2 ?? DBNull.Value;
                    cmd.Parameters.Add("@Image3", SqlDbType.VarChar, 500).Value = DBNull.Value; //(object?)model.Image3 ?? DBNull.Value;
                    cmd.Parameters.Add("@Image4", SqlDbType.VarChar, 500).Value = DBNull.Value; //(object?)model.Image4 ?? DBNull.Value;
                    cmd.Parameters.Add("@Image5", SqlDbType.VarChar, 500).Value = DBNull.Value; //(object?)model.Image5 ?? DBNull.Value;
                    cmd.Parameters.Add("@GoodFor_GrandWedding", SqlDbType.Bit).Value = model.GoodFor_GrandWedding;
                    cmd.Parameters.Add("@GoodFor_Weddings", SqlDbType.Bit).Value = model.GoodFor_Weddings;
                    cmd.Parameters.Add("@GoodFor_BirthdayParty", SqlDbType.Bit).Value = model.GoodFor_BirthdayParty;
                    cmd.Parameters.Add("@GoodFor_Engagement", SqlDbType.Bit).Value = model.GoodFor_Engagement;
                    cmd.Parameters.Add("@GoodFor_FamilyFunctions", SqlDbType.Bit).Value = model.GoodFor_FamilyFunctions;
                    cmd.Parameters.Add("@GoodFor_Communion", SqlDbType.Bit).Value = model.GoodFor_Communion;
                    cmd.Parameters.Add("@GoodFor_BoardMeetings", SqlDbType.Bit).Value = model.GoodFor_BoardMeetings;
                    cmd.Parameters.Add("@GoodFor_ProductLaunch", SqlDbType.Bit).Value = model.GoodFor_ProductLaunch;
                    cmd.Parameters.Add("@GoodFor_OfficialEvent", SqlDbType.Bit).Value = model.GoodFor_OfficialEvent;
                    cmd.Parameters.Add("@Seating_Cluster", SqlDbType.Bit).Value = model.Seating_Cluster;
                    cmd.Parameters.Add("@Seating_Theatre", SqlDbType.Bit).Value = model.Seating_Theatre;
                    cmd.Parameters.Add("@Seating_UShape", SqlDbType.Bit).Value = model.Seating_UShape;
                    cmd.Parameters.Add("@Seating_HollowSquare", SqlDbType.Bit).Value = model.Seating_HollowSquare;
                    cmd.Parameters.Add("@Seating_Boardroom", SqlDbType.Bit).Value = model.Seating_Boardroom;
                    cmd.Parameters.Add("@Seating_DanceFloor", SqlDbType.Bit).Value = model.Seating_DanceFloor;
                    cmd.Parameters.Add("@Amenity_ChangingRoom", SqlDbType.Bit).Value = model.Amenity_ChangingRoom;
                    cmd.Parameters.Add("@Amenity_AirCondition", SqlDbType.Bit).Value = model.Amenity_AirCondition;
                    cmd.Parameters.Add("@Amenity_CoupleChairs", SqlDbType.Bit).Value = model.Amenity_CoupleChairs;
                    cmd.Parameters.Add("@Amenity_Sofa", SqlDbType.Bit).Value = model.Amenity_Sofa;
                    cmd.Parameters.Add("@Amenity_MusicSystem", SqlDbType.Bit).Value = model.Amenity_MusicSystem;
                    cmd.Parameters.Add("@Amenity_Mic", SqlDbType.Bit).Value = model.Amenity_Mic;
                    cmd.Parameters.Add("@Amenity_Podium", SqlDbType.Bit).Value = model.Amenity_Podium;
                    cmd.Parameters.Add("@Amenity_Lights", SqlDbType.Bit).Value = model.Amenity_Lights;
                    cmd.Parameters.Add("@Amenity_FalseCeiling", SqlDbType.Bit).Value = model.Amenity_FalseCeiling;
                    cmd.Parameters.Add("@Amenity_Fans", SqlDbType.Bit).Value = model.Amenity_Fans;
                    cmd.Parameters.Add("@Amenity_Cloakroom", SqlDbType.Bit).Value = model.Amenity_Cloakroom;
                    cmd.Parameters.Add("@Amenity_Lockers", SqlDbType.Bit).Value = model.Amenity_Lockers;
                    cmd.Parameters.Add("@Amenity_Projectors", SqlDbType.Bit).Value = model.Amenity_Projectors;
                    cmd.Parameters.Add("@Amenity_WiFi", SqlDbType.Bit).Value = model.Amenity_WiFi;
                    cmd.Parameters.Add("@Amenity_Stage", SqlDbType.Bit).Value = model.Amenity_Stage;
                    cmd.Parameters.Add("@Amenity_WashRoom", SqlDbType.Bit).Value = model.Amenity_WashRoom;
                    cmd.Parameters.Add("@Amenity_PlasmaScreen", SqlDbType.Bit).Value = model.Amenity_PlasmaScreen;
                    cmd.Parameters.Add("@Amenity_PowerBackup", SqlDbType.Bit).Value = model.Amenity_PowerBackup;
                    cmd.Parameters.Add("@Amenity_BoardroomSetup", SqlDbType.Bit).Value = model.Amenity_BoardroomSetup;
                    cmd.Parameters.Add("@Amenity_WallClock", SqlDbType.Bit).Value = model.Amenity_WallClock;
                    cmd.Parameters.Add("@Amenity_ShoeRack", SqlDbType.Bit).Value = model.Amenity_ShoeRack;
                    cmd.Parameters.Add("@Amenity_RedCarpet", SqlDbType.Bit).Value = model.Amenity_RedCarpet;
                    cmd.Parameters.Add("@Is_Blocked", SqlDbType.Bit).Value = model.Is_Blocked;

                    cmd.Parameters.Add("@CapacityWithPartition_Id", SqlDbType.Int).Value = model.CapacityWithPartition_Id ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@RentWithPartition", SqlDbType.Decimal).Value = model.RentWithPartition ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@IsHallMerged", SqlDbType.Bit).Value = model.IsHallMerged;

                    string? mergeHallJson = (model.MergeHall_JSON != null && model.MergeHall_JSON.Any())
                            ? JsonConvert.SerializeObject(model.MergeHall_JSON)
                            : null;

                    cmd.Parameters.Add("@MergeHall_JSON", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(mergeHallJson)
                            ? (object)DBNull.Value
                            : mergeHallJson;

                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    //cmd.ExecuteNonQuery();
                    // First execute the main command to get rowid
                    Int64 rowid = Convert.ToInt64(cmd.ExecuteScalar());

                    // Handle images (Image1 through Image5)
                    for (int i = 1; i <= 5; i++)
                    {
                        string imageProperty = $"Image{i}";
                        var imageValue = (string)model.GetType().GetProperty(imageProperty).GetValue(model);

                        if (!string.IsNullOrEmpty(imageValue))
                        {
                            string fileType = _imageUploadService.GetFileTypeFromBase64(imageValue);
                            string folderPath = $"/org/{domain}/Company_HallRentAndDetails_Images"; // Updated to include domain
                            //string folderName = "/Company_HallRentAndDetails_Images";
                            _imageUploadService.DeleteImages(folderPath, rowid.ToString() + "_img" + i);
                            string uploadedFileName = _imageUploadService.UploadImage(imageValue, folderPath, rowid.ToString() + "_img" + i, fileType);
                            model.GetType().GetProperty(imageProperty).SetValue(model, uploadedFileName);
                        }
                    }

                    // Update all media paths in database using the stored procedure
                    using (SqlCommand mediaCmd = new SqlCommand("SaveCompanyHallDetailsMedia", con))
                    {
                        mediaCmd.CommandType = CommandType.StoredProcedure;
                        mediaCmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = rowid;
                        mediaCmd.Parameters.Add("@Image1", SqlDbType.VarChar, 500).Value = (object)model.Image1 ?? DBNull.Value;
                        mediaCmd.Parameters.Add("@Image2", SqlDbType.VarChar, 500).Value = (object)model.Image2 ?? DBNull.Value;
                        mediaCmd.Parameters.Add("@Image3", SqlDbType.VarChar, 500).Value = (object)model.Image3 ?? DBNull.Value;
                        mediaCmd.Parameters.Add("@Image4", SqlDbType.VarChar, 500).Value = (object)model.Image4 ?? DBNull.Value;
                        mediaCmd.Parameters.Add("@Image5", SqlDbType.VarChar, 500).Value = (object)model.Image5 ?? DBNull.Value;

                        mediaCmd.ExecuteNonQuery();
                    }

                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id == 0 ? "Hall details inserted successfully." : "Hall details updated successfully.";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveCompanyHallDetails_IsBlocked")]
        public IActionResult SaveCompanyHallDetails_IsBlocked([FromBody] GetCompanyHallDetailsRequest_IsBlocked model)
        {
            var apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SaveCompanyHallDetails_IsBlocked", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.Id;
                    cmd.Parameters.Add("@Is_Blocked", SqlDbType.Bit).Value = model.Is_Blocked;
                    cmd.Parameters.Add("@Action_By", SqlDbType.BigInt).Value = model.ActionBy;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                apiResponse.status = "Success";
                apiResponse.message = "Hall Status updated";
                apiResponse.data = null;
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
        }

    }
}
public class GetCompanyHallDetailsRequest
{
    public long? Id { get; set; }
}

public class GetCompanyHallDetailsRequest_IsBlocked : BaseClass
{
    public long? Id { get; set; }
    public bool Is_Blocked { get; set; }

}
public class SaveCompanyHallDetailsRequest : BaseClass
{
    public long Id { get; set; }
    public string HallName { get; set; } = string.Empty;
    public long CapacityRange { get; set; }
    public decimal RentPerDay { get; set; }
    public bool IsPartitioned { get; set; }
    public string? Video360Link { get; set; }
    public string? PromoVideo1 { get; set; }
    public string? PromoVideo2 { get; set; }
    public string? Image1 { get; set; }
    public string? Image2 { get; set; }
    public string? Image3 { get; set; }
    public string? Image4 { get; set; }
    public string? Image5 { get; set; }
    public bool GoodFor_GrandWedding { get; set; }
    public bool GoodFor_Weddings { get; set; }
    public bool GoodFor_BirthdayParty { get; set; }
    public bool GoodFor_Engagement { get; set; }
    public bool GoodFor_FamilyFunctions { get; set; }
    public bool GoodFor_Communion { get; set; }
    public bool GoodFor_BoardMeetings { get; set; }
    public bool GoodFor_ProductLaunch { get; set; }
    public bool GoodFor_OfficialEvent { get; set; }
    public bool Seating_Cluster { get; set; }
    public bool Seating_Theatre { get; set; }
    public bool Seating_UShape { get; set; }
    public bool Seating_HollowSquare { get; set; }
    public bool Seating_Boardroom { get; set; }
    public bool Seating_DanceFloor { get; set; }
    public bool Amenity_ChangingRoom { get; set; }
    public bool Amenity_AirCondition { get; set; }
    public bool Amenity_CoupleChairs { get; set; }
    public bool Amenity_Sofa { get; set; }
    public bool Amenity_MusicSystem { get; set; }
    public bool Amenity_Mic { get; set; }
    public bool Amenity_Podium { get; set; }
    public bool Amenity_Lights { get; set; }
    public bool Amenity_FalseCeiling { get; set; }
    public bool Amenity_Fans { get; set; }
    public bool Amenity_Cloakroom { get; set; }
    public bool Amenity_Lockers { get; set; }
    public bool Amenity_Projectors { get; set; }
    public bool Amenity_WiFi { get; set; }
    public bool Amenity_Stage { get; set; }
    public bool Amenity_WashRoom { get; set; }
    public bool Amenity_PlasmaScreen { get; set; }
    public bool Amenity_PowerBackup { get; set; }
    public bool Amenity_BoardroomSetup { get; set; }
    public bool Amenity_WallClock { get; set; }
    public bool Amenity_ShoeRack { get; set; }
    public bool Amenity_RedCarpet { get; set; }
    public bool Is_Blocked { get; set; }
    public int? CapacityWithPartition_Id { get; set; }
    public decimal? RentWithPartition { get; set; }
    public bool IsHallMerged { get; set; }
    public List<MergeHall_List>? MergeHall_JSON { get; set; }
}

public class MergeHall_List
{
    public long? Hall_Id { get; set; }
}