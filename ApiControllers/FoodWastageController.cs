using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodWastageController : MasterController
    {
        private string connString = string.Empty;

        public FoodWastageController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }


        [HttpPost]
        [Route("SaveFoodWastage")]
        public IActionResult SaveFoodWastage([FromBody] SaveFoodWastageRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!; // already 200 based on your pattern
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveFoodWastage", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = model.BookingId;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = model.UserId;
                    cmd.Parameters.Add("@FoodJson", SqlDbType.NVarChar).Value =
                        model.FoodJson ?? "[]";

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Food wastage saved successfully!";
                apiResponse.data = null;

                return Ok(apiResponse); // ✅ always 200
            }
            catch (SqlException ex)
            {
                // SQL validation errors (THROW)
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                return Ok(apiResponse); // ✅ still 200
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = "Something went wrong";
                apiResponse.data = null;

                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                return Ok(apiResponse); // ✅ still 200
            }
        }


        [HttpPost]
        [Route("GetFoodWastageByBookingId")]
        public IActionResult GetFoodWastageByBookingId([FromBody] GetFoodWastageByBookingIdRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<FoodWastage> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!; // already handled
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetFoodWastageByBookingId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                    con.Close();
                }

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new FoodWastage
                    {
                        FoodWastageId = Convert.ToInt64(dr["FoodWastageId"]),
                        MenuItem_Id = Convert.ToInt64(dr["MenuItem_Id"]),
                        MenuItem_Name = Convert.ToString(dr["MenuItem_Name"]),
                        WastageKg = Convert.ToDecimal(dr["WastageKg"]),
                        LastUpdated_By = Convert.ToInt64(dr["LastUpdated_By"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Food wastage list fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse); // ✅ always 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse); // ✅ still 200
            }
        }

    }
}
