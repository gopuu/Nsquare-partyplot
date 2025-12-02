using Newtonsoft.Json;
using System.Text;

namespace ERPAdmin
{
    public class EwayService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly string _baseUrl, _clientId, _clientSecret, _gstin, _username, _password;


        public EwayService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
            _baseUrl = cfg["Eway:BaseUrl"];
            _clientId = cfg["Eway:ClientId"];
            _clientSecret = cfg["Eway:ClientSecret"];
            _gstin = cfg["Eway:Gstin"];
            _username = cfg["Eway:Username"];
            _password = cfg["Eway:Password"];
        }

        // 1) Authenticate -> returns authtoken and decrypted 'sek'
        public async Task<(string Authtoken, string Sek)> AuthenticateAsync()
        {
            // 1. Generate a random app_key (AES key), e.g. 32 bytes base64
            var appKeyPlain = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // 2. Use NIC public key to RSA encrypt password and appKeyPlain
            // (Use official helper: RsaEncrypt(publicKey, password) and RsaEncrypt(publicKey, appKeyPlain))
            // string encPassword = CryptoHelper.AesEncrypt(_password); // use NIC sample implementation
            // string encAppKey = CryptoHelper.AesEncrypt(appKeyPlain);

            string encPassword = CryptoHelper.RsaEncrypt(_password, "nic_public.cer");
            string encAppKey = CryptoHelper.RsaEncrypt(appKeyPlain, "nic_public.cer");

            var authRequest = new
            {
                action = "ACCESSTOKEN",
                username = _username,
                password = encPassword,
                app_key = encAppKey
            };

            var reqJson = JsonConvert.SerializeObject(authRequest);
            var httpReq = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "auth");
            httpReq.Content = new StringContent(reqJson, Encoding.UTF8, "application/json");
            httpReq.Headers.Add("client-id", _clientId);
            httpReq.Headers.Add("client-secret", _clientSecret);
            httpReq.Headers.Add("gstin", _gstin);

            var resp = await _http.SendAsync(httpReq);
            var respStr = await resp.Content.ReadAsStringAsync();
            dynamic respObj = JsonConvert.DeserializeObject(respStr);

            // response contains authtoken and encryptedSek: decrypt encryptedSek using appKeyPlain (see NIC sample)
            string authtoken = respObj.authtoken;
            string encryptedSek = respObj.sek; // exact field name per docs
            string sek = CryptoHelper.AesDecrypt(encryptedSek, appKeyPlain); // official helper

            return (authtoken, sek);
        }

        // 2) Generate EWB using token and sek
        public async Task<dynamic> GenerateEwayBillAsync(object ewayPayload, string authtoken, string sek)
        {
            string requestJson = JsonConvert.SerializeObject(ewayPayload);
            string base64Req = Convert.ToBase64String(Encoding.UTF8.GetBytes(requestJson));

            // AES-encrypt base64Req using 'sek' (CryptoHelper.AesEncrypt)
            string encryptedReq = CryptoHelper.AesEncrypt(base64Req, sek);

            var payload = new { action = "GENEWB", request = encryptedReq };

            var httpReq = new HttpRequestMessage(HttpMethod.Post, _baseUrl);
            httpReq.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            httpReq.Headers.Add("client-id", _clientId);
            httpReq.Headers.Add("client-secret", _clientSecret);
            httpReq.Headers.Add("gstin", _gstin);
            httpReq.Headers.Add("authtoken", authtoken);

            var resp = await _http.SendAsync(httpReq);
            var respStr = await resp.Content.ReadAsStringAsync();
            dynamic respObj = JsonConvert.DeserializeObject(respStr);

            // response will have encrypted data - decrypt using sek
            string respEncrypted = respObj.response;
            string respDecryptedBase64 = CryptoHelper.AesDecrypt(respEncrypted, sek);
            string respDecryptedJson = Encoding.UTF8.GetString(Convert.FromBase64String(respDecryptedBase64));
            return JsonConvert.DeserializeObject(respDecryptedJson);
        }


       

    }
}
