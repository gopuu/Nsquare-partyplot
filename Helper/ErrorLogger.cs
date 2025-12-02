using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.Helper
{
    public static class ErrorLogger
    {
        //private static readonly string logDbConnectionString = "YourCentralLogDbConnectionString";

        public static void LogException(Exception ex, ControllerContext controllerContext, HttpRequest request, string logDbConnectionString)
        {
            try
            {
                string controllerName = controllerContext.RouteData.Values["controller"]?.ToString() ?? "Unknown";
                string actionName = controllerContext.RouteData.Values["action"]?.ToString() ?? "Unknown";
                string httpMethod = request.Method;
                int statusCode = 500;
                string requestUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string requestBody = ""; // Optional: log body for POST/PUT

                using (SqlConnection conn = new SqlConnection(logDbConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("LogCriticalError", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ErrorMessage", ex.Message);
                        cmd.Parameters.AddWithValue("@StackTrace", ex.StackTrace ?? "");
                        cmd.Parameters.AddWithValue("@Source", ex.Source ?? "");
                        cmd.Parameters.AddWithValue("@Controller", controllerName);
                        cmd.Parameters.AddWithValue("@Action", actionName);
                        cmd.Parameters.AddWithValue("@HttpMethod", httpMethod);
                        cmd.Parameters.AddWithValue("@StatusCode", statusCode);
                        cmd.Parameters.AddWithValue("@RequestUrl", requestUrl);
                        cmd.Parameters.AddWithValue("@RequestBody", requestBody);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Fail silently or fallback log (e.g. file, event viewer)
            }
        }

    }
}
