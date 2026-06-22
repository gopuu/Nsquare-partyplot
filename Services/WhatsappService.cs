using ERPAdmin.Helper;
using ERPAdmin.Models;
using Newtonsoft.Json;
using System.Text;

namespace ERPAdmin.Services
{
    public class WhatsAppService
    {

        public async Task<apiResponse> SendTemplateWhatsAppCommon(
            long companyId,
            string mobileNo,
            string templateName,
            List<string> variables,
            long actionBy,
            bool isPromotional = false,
            string domainUrl = "")
        {
            try
            {
                WhatsAppSendRequest request = new WhatsAppSendRequest
                {
                    CompanyId = companyId,
                    MobileNo = $"91{mobileNo}",
                    MessageType = "TEMPLATE",
                    MessageCount = 1,
                    ReferenceId = Guid.NewGuid().ToString(),
                    TemplateId = null,
                    IsPromotional = isPromotional,
                    ActionBy = actionBy,
                    TemplateName = templateName,
                    Message = string.Join("&", variables)
                };

                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"{domainUrl}api/Wallet/SendWhatsApp";
                    apiUrl = $"http://localhost:6168/api/Wallet/SendWhatsApp";

                    StringContent content = new StringContent(
                        JsonConvert.SerializeObject(request),
                        Encoding.UTF8,
                        "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    string responseJson = await response.Content.ReadAsStringAsync();
                    apiResponse result = JsonConvert.DeserializeObject<apiResponse>(responseJson);

                    return result ?? new apiResponse
                    {
                        status = "Error",
                        message = "Empty response"
                    };
                }
            }
            catch (Exception ex)
            {
                return new apiResponse
                {
                    status = "Error",
                    message = ex.Message
                };
            }
        }


        private async Task SendTemplateWhatsAppCommon_OLD(long companyId, string mobileNo, string templateName, List<string> variables, 
            long actionBy, bool isPromotional = false, string domainUrl = "")
        {
            try
            {
                WhatsAppSendRequest request = new WhatsAppSendRequest
                {
                    CompanyId = companyId,
                    MobileNo = $"91{mobileNo}",
                    MessageType = "TEMPLATE",
                    MessageCount = 1,
                    ReferenceId = Guid.NewGuid().ToString(),
                    TemplateId = null,
                    IsPromotional = isPromotional,
                    ActionBy = actionBy,
                    TemplateName = templateName,
                    Message = string.Join("&", variables)
                };

                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"{domainUrl}api/Wallet/SendWhatsApp";

                    StringContent content = new StringContent(
                        JsonConvert.SerializeObject(request),
                        Encoding.UTF8,
                        "application/json"
                    );
                    //apiUrl = $"http://localhost:6168/api/Wallet/SendWhatsApp";
                    await client.PostAsync(apiUrl, content);
                }
            }
            catch
            {

            }
        }
    }
}
