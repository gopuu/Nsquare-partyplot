using ERPAdmin.Models;
using Newtonsoft.Json;
using System.Text;

namespace ERPAdmin.Services
{
    public class WhatsAppService
    {
        public async Task SendTemplateWhatsAppCommon(long companyId, string mobileNo, string templateName, List<string> variables, 
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
