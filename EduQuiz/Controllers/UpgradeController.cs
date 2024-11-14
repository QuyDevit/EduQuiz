using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using EduQuiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client.Extensions.Msal;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ZaloPay.Helper;
using ZaloPay.Helper.Crypto;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class UpgradeController : Controller
    {
        private readonly ZaloPayService _zaloPayService;
        private readonly MoMoService _moMoService;
        private readonly IEmailService _emailService;
        private readonly EduQuizDBContext _context;
        public UpgradeController(EduQuizDBContext context, ZaloPayService zaloPayService,MoMoService moMoService, IEmailService emailService)
        {
            _context = context;
            _zaloPayService = zaloPayService;
            _moMoService = moMoService;
            _emailService = emailService;
        }
        [Route("/upgrade/plan")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("/payment/")]
        public IActionResult PaymentInfo(string plan)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            ViewBag.Data = JsonConvert.SerializeObject(new
            {
                Email = email,
                Plan = plan
            });
            return View();
        }
        public async Task<IActionResult> CreateOrder([FromBody] PaymentData data)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userid = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            StringBuilder sb = new StringBuilder();
            sb.Append("Gói ");
            sb.Append(data.Quantity).Append(data.Period == "year" ? " năm " : " tháng ");
            sb.Append(data.Plan == "organization" ? "EduQuiz+ dành cho Tổ chức" : "EduQuiz+ Chuyên nghiệp");
            Dictionary<string, string> result;
            if (data.Paymentmethod == "MOMO")
            {
                result = await _moMoService.CreateOrderAsync(data.Totalprice, sb.ToString());
            }
            else
            {
                result = await _zaloPayService.CreateOrderAsync(userid,data.Totalprice, sb.ToString());
            }
            var newOrder = new Order
            {
                Company = data.Company,
                CreateAt = DateTime.Now,
                Email = data.Email,
                FirstName = data.Firstname,
                LastName = data.Lastname,
                OrderId =data.Paymentmethod == "MOMO"? result["orderId"] : result["idtrans"],
                PlanType = data.Plan,
                PhoneNumber = data.Phone,
                Status = "Pending",
                UserId = int.Parse(userid),
                PaymentMethod = data.Paymentmethod,
                TotalPrice = data.Totalprice,
                Period = data.Period,
                Quantity = data.Quantity,
            };
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();
            return Json(result);
        }
        [AllowAnonymous]
        [Route("/payment/callbackzalo")]
        [HttpPost]
        public async Task<JsonResult> CallbackZalo([FromBody] CallbackRequest cbdata)
        {
            var result = new Dictionary<string, object>();

            try
            {
                var dataStr = Convert.ToString(cbdata.Data);
                var reqMac = Convert.ToString(cbdata.Mac);

                var isValidCallback = _zaloPayService.VerifyCallback(dataStr, reqMac);

                // kiểm tra callback hợp lệ (đến từ ZaloPay server)
                if (!isValidCallback)
                {
                    // callback không hợp lệ
                    result["returncode"] = -1;
                    result["returnmessage"] = "mac not equal";
                }
                else
                {
                    var dataJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
                    var apptransid = dataJson["app_trans_id"].ToString();
                    var fintorder = await _context.Orders.SingleOrDefaultAsync(x => x.OrderId.Equals(apptransid));
                    if (fintorder != null) {
                        fintorder.Status = "Success";
                        var finduser = await _context.Users.SingleOrDefaultAsync(x => x.Id == fintorder.UserId);
                        if (finduser != null)
                        {
                            finduser.SubscriptionType = "vip";
                            finduser.SubscriptionStartDate = DateTime.Now;
                            finduser.SubscriptionEndDate = fintorder.Period == "year"
                                ? DateTime.Now.AddYears(fintorder.Quantity)
                                : DateTime.Now.AddMonths(fintorder.Quantity);
                            if(fintorder.PlanType == "organization")
                            {
                                var groupbyuser = await _context.Groups.Where(g=>g.UserId == fintorder.UserId).ToListAsync();
                                foreach (var group in groupbyuser)
                                {
                                    group.SubscriptionType = "vip";
                                    group.SubscriptionStartDate = DateTime.Now;
                                    group.SubscriptionEndDate = fintorder.Period == "year"
                                       ? DateTime.Now.AddYears(fintorder.Quantity)
                                       : DateTime.Now.AddMonths(fintorder.Quantity);
                                } 
                            }
                        }
                        await _context.SaveChangesAsync();
                        SendEmail(fintorder);
                    }

                    result["returncode"] = 1;
                    result["returnmessage"] = "success";
                }
                // thông báo kết quả cho zalopay server
                return Json(result);
            }
            catch (Exception ex)
            {
                result["returncode"] = 0; // ZaloPay server sẽ callback lại (tối đa 3 lần)
                result["returnmessage"] = ex.Message;
                return Json(result);
            }
        }
        [AllowAnonymous]
        [Route("/payment/callbackmomo")]
        [HttpPost]
        public async Task<IActionResult> CallbackMoMo([FromBody] MomoCallbackResponse cbdata)
        {
            if (cbdata.resultCode == 0)
            {
                var fintorder = await _context.Orders.SingleOrDefaultAsync(x => x.OrderId.Equals(cbdata.orderId));
                if (fintorder != null)
                {
                    fintorder.Status = "Success";
                    var finduser = await _context.Users.SingleOrDefaultAsync(x => x.Id == fintorder.UserId);
                    if (finduser != null)
                    {
                        finduser.SubscriptionType = "vip";
                        finduser.SubscriptionStartDate = DateTime.Now;
                        finduser.SubscriptionEndDate = fintorder.Period == "year"
                            ? DateTime.Now.AddYears(fintorder.Quantity)
                            : DateTime.Now.AddMonths(fintorder.Quantity);
                        if (fintorder.PlanType == "organization")
                        {
                            var groupbyuser = await _context.Groups.Where(g => g.UserId == fintorder.UserId).ToListAsync();
                            foreach (var group in groupbyuser)
                            {
                                group.SubscriptionType = "vip";
                                group.SubscriptionStartDate = DateTime.Now;
                                group.SubscriptionEndDate = fintorder.Period == "year"
                                   ? DateTime.Now.AddYears(fintorder.Quantity)
                                   : DateTime.Now.AddMonths(fintorder.Quantity);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    SendEmail(fintorder);
                }
                return Ok("Giao dịch thành công");
            }
            else 
            {
                return BadRequest(); 
            }
        }
        private void SendEmail( Order data)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/src/templates", "InvoiceEmailTemplate.html");
            string emailBody;

            try
            {
                emailBody = System.IO.File.ReadAllText(templatePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi đọc file", ex);
            }

            emailBody = emailBody.Replace("{fullname}", $"{data.FirstName} {data.LastName}");
            emailBody = emailBody.Replace("{company}", data.Company);
            emailBody = emailBody.Replace("{phone}", data.PhoneNumber);
            emailBody = emailBody.Replace("{orderid}", data.OrderId); 
            emailBody = emailBody.Replace("{paymentmethod}", data.PaymentMethod);
            emailBody = emailBody.Replace("{date}", data.CreateAt.ToString("MM/dd/yyyy HH:mm:ss"));
            emailBody = emailBody.Replace("{plantype}", data.PlanType == "organization" ?"dành cho tổ chức" :"chuyên nghiệp");
            emailBody = emailBody.Replace("{quantity}", data.Quantity.ToString());
            emailBody = emailBody.Replace("{price}", data.PlanType == "organization" ? "249,000 vnđ" : "69,000 vnđ");
            emailBody = emailBody.Replace("{period}", data.Period =="year" ?"năm":"tháng");
            emailBody = emailBody.Replace("{totalprice}", data.TotalPrice.ToString("N0")+" vnđ");

            // Gửi email sử dụng dịch vụ email
            _emailService.SendEmail(data.Email, "EduQuiz - Bạn đã thanh toán thành công gói dịch vụ", emailBody);
        }
    }
}
