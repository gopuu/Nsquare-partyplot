using ERPAdmin.Helper;
using ERPAdmin.Models;
using iTextSharp.text;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

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
            string domainUrl = "",
            string msgTemplate = "")
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
                    Message = string.Join("&", variables),
                    MsgTemplate = msgTemplate,
                    MsgSample = BuildWhatsAppMessage(msgTemplate, variables)
                };

                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"{domainUrl}api/Wallet/SendWhatsApp";
                    //apiUrl = $"http://localhost:6168/api/Wallet/SendWhatsApp";

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

        private string BuildWhatsAppMessage(string template, List<string> variables)
        {
            if (string.IsNullOrWhiteSpace(template))
                return "";

            string message = template;

            var matches = Regex.Matches(template, @"\{([^}]+)\}");

            for (int i = 0; i < matches.Count && i < variables.Count; i++)
            {
                string placeholder = matches[i].Value;

                message = message.Replace(
                    placeholder,
                    variables[i] ?? ""
                );
            }

            return message;
        }


    }
}
