using EduQuiz.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ZaloPay.Helper;
using ZaloPay.Helper.Crypto;

namespace EduQuiz.Services
{
    public class ZaloPayService
    {
        private readonly ZaloPayConfig _zaloConfig;
        public ZaloPayService(IOptions<ZaloPayConfig> zaloConfig)
        {
            _zaloConfig = zaloConfig.Value;
        }
        public async Task<Dictionary<string, string>> CreateOrderAsync(string appUser, long amount,string content)
        {
            Random rnd = new Random();
            var embed_data = new { redirecturl = _zaloConfig.RedirectUrl};
            var items = new[] { new { } };
            var app_trans_id = DateTime.Now.ToString("yyMMdd") + "_" + rnd.Next(1000000); // mã giao dich có định dạng yyMMdd_xxxx

            var param = new Dictionary<string, string>
            {
                { "app_id", _zaloConfig.Appid },
                { "app_user", appUser },
                { "app_time",  Utils.GetTimeStamp().ToString() },
                { "amount", amount.ToString() },
                { "app_trans_id", app_trans_id },
                { "embed_data", JsonConvert.SerializeObject(embed_data) },
                { "item", JsonConvert.SerializeObject(items) },
                { "description", $"EduQuiz - Thanh toán {content} - Mã đơn hàng #" + app_trans_id },
                { "bank_code", "zalopayapp" },
                {"callback_url", _zaloConfig.CallbackUrl}
            };

            var data = _zaloConfig.Appid + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"] + "|"
                + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];
            param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, _zaloConfig.Key1, data));

            var result = await HttpHelper.PostFormAsync(_zaloConfig.PaymentUrl, param);
            result["idtrans"] = app_trans_id;
            return result.ToDictionary(k => k.Key, k => k.Value.ToString());
        }
        public bool VerifyCallback(string data, string requestMac)
        {
            try
            {
                string mac = HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, _zaloConfig.Key2, data);

                return requestMac.Equals(mac);
            }
            catch
            {
                return false;
            }
        }
        public async Task<Dictionary<string, string>> QueryOrderStatusAsync(string appTransId)
        {
            var param = new Dictionary<string, string>
            {
                { "app_id", _zaloConfig.Appid },
                { "app_trans_id", appTransId },
                { "mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, _zaloConfig.Key1, _zaloConfig.Appid + "|" + appTransId + "|" + _zaloConfig.Key1) }
            };

            var result = await HttpHelper.PostFormAsync(_zaloConfig.QueryUrl, param);
            return result.ToDictionary(k => k.Key, k => k.Value.ToString());
        }

    }
}
