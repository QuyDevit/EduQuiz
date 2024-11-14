using EduQuiz.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZaloPay.Helper.Crypto;
using ZaloPay.Helper;

namespace EduQuiz.Services
{
    public class MoMoService
    {
        private readonly HttpClient _client;
        private readonly MomoConfig _momoConfig;
        public MoMoService(HttpClient client,IOptions<MomoConfig> momoConfig) { 
            _client = client;
            _momoConfig = momoConfig.Value;
        }
        public async Task<Dictionary<string, string>> CreateOrderAsync(long amount,string content)
        {
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();

            QuickPayResquest request = new QuickPayResquest
            {
                requestType = _momoConfig.RequestType,
                orderInfo = $"EduQuiz - Thanh toán {content}".Trim(),
                partnerCode = _momoConfig.PartnerCode.Trim(),
                redirectUrl = _momoConfig.RedirectUrl.Trim(),
                ipnUrl = _momoConfig.CallbackUrl.Trim(),
                amount = amount,
                orderId = myuuidAsString,
                requestId = myuuidAsString,
                extraData = "",
                partnerName = "MoMo Payment",
                storeId = "Test Store",
                orderGroupId = "",
                autoCapture = true,
                lang = "vi"
            };

            var rawSignature = "accessKey=" + _momoConfig.AccessKey +
                               "&amount=" + request.amount +
                               "&extraData=" + request.extraData +
                               "&ipnUrl=" + request.ipnUrl +
                               "&orderId=" + request.orderId +
                               "&orderInfo=" + request.orderInfo +
                               "&partnerCode=" + request.partnerCode +
                               "&redirectUrl=" + request.redirectUrl +
                               "&requestId=" + request.requestId +
                               "&requestType=" + request.requestType;

            request.signature = getSignature(rawSignature, _momoConfig.SecretKey);

            Console.WriteLine("Raw Signature: " + rawSignature);
            Console.WriteLine("Generated Signature: " + request.signature);

            StringContent httpContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var quickPayResponse = await _client.PostAsync(_momoConfig.PaymentUrl, httpContent);

            var contents = await quickPayResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(contents);
            var resultAsString = result.ToDictionary(k => k.Key, v => v.Value.ToString());
            return resultAsString;
        }
        private static string getSignature(string text, string key)
        {
            // change according to your needs, an UTF8Encoding
            // could be more suitable in certain situations
            var encoding = new UTF8Encoding();

            byte[] textBytes = encoding.GetBytes(text);
            byte[] keyBytes = encoding.GetBytes(key);

            using (var hash = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hash.ComputeHash(textBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public async Task<Dictionary<string, string>> QueryOrderStatusAsync(string appTransId)
        {
            var request = new QuickPayResquest
            {
                partnerCode = _momoConfig.PartnerCode,
                requestId = Guid.NewGuid().ToString(),
                orderId = appTransId,
                signature = ""
            };
            var rawSignature = "accessKey=" + _momoConfig.AccessKey +
                       "&orderId=" + request.orderId +
                       "&partnerCode=" + request.partnerCode +
                       "&requestId=" + request.requestId;
            request.signature = getSignature(rawSignature, _momoConfig.SecretKey);
            StringContent httpContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var queryResponse = await _client.PostAsync(_momoConfig.QueryUrl, httpContent);
            var contents = await queryResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(contents);
            var filteredResult = result.Where(kv => kv.Value != null)
                           .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            // Chuyển đổi kết quả đã lọc sang chuỗi
            var resultAsString = filteredResult.ToDictionary(k => k.Key, v => v.Value);
            return resultAsString;
        }
    }
}
