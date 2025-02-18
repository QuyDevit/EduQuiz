﻿using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using EduQuiz.Models.EF;
using EduQuiz.Models;
using EduQuiz.Services;
using Newtonsoft.Json;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using BcryptNet = BCrypt.Net.BCrypt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NuGet.Common;
using Newtonsoft.Json.Linq;
using EduQuiz.Security;


namespace EduQuiz.Controllers
{
    public class AccountController : Controller
    {
        private readonly EduQuizDBContext _context;
        private readonly UsernameService _usernameService;
        private readonly Random _random;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly CookieAuth _cookieAuth;

        public AccountController(EduQuizDBContext context, UsernameService usernameService, 
            IHttpClientFactory httpClientFactory, IEmailService emailService, IConfiguration config,CookieAuth cookieAuth)
        {
            _context = context;
            _usernameService = usernameService;
            _random = new Random(); // Khởi tạo đối tượng Random
            _httpClientFactory = httpClientFactory;
            _emailService = emailService;
            _config = config;
            _cookieAuth = cookieAuth;
        }
        [Route("auth/typeaccount")]
        public IActionResult Register()
		{
			return View();
		}
        [Route("auth/birthday")]
        public IActionResult ChooseBirthday()
		{
			return View();
		}
        [Route("auth/username")]
        public IActionResult Username()
        {
            return View();
        }
        [Route("auth/signup")]
        public IActionResult SignupOption() { 
            return View();
        }
        [Route("auth/login")]
        public IActionResult Login()
        {
            var actoken = HttpContext.Request.Cookies["acToken"];
            if (!string.IsNullOrEmpty(actoken))
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            return View();
        }
       
        [Route("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var user = await _context.Users.FindAsync(iduser);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                HttpContext.Response.Cookies.Append("acToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1)
                });
            }

            var refreshTokenCookie = Request.Cookies["rfToken"];
            if (refreshTokenCookie != null)
            {
                HttpContext.Response.Cookies.Append("rfToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(-7)
                });
            }
            return RedirectToAction("Login", "Account");
        }
        [Route("auth/deleteaccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var authCookie = Request.Cookies["acToken"];

            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Status = false;
                    await _context.SaveChangesAsync();
                }
                HttpContext.Response.Cookies.Append("acToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1)
                });
                var refreshTokenCookie = Request.Cookies["rfToken"];
                if (refreshTokenCookie != null)
                {
                    HttpContext.Response.Cookies.Append("rfToken", "", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTimeOffset.UtcNow.AddDays(-7)
                    });
                }
            } 
            return RedirectToAction("Index", "Home");
        }
        [Route("auth/verifyemail")]
        [HttpGet]
        public ActionResult VerifyEmail(string token, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email.Equals(email) && u.LinkToken.Equals(token));
            if (user != null)
            {
                user.EmailVerified = true;
                _context.SaveChanges();
                ViewBag.IsCheck = true;
                ViewBag.Notification = "Xác thực Email thành công!";
                ViewBag.Content = "Email của bạn đã được xác minh thành công. Bây giờ bạn có thể đăng nhập vào tài khoản của mình.";
                ViewBag.Redirect = Url.Action("Login", "Account");

				return View();
            }
            else
            {
                ViewBag.IsCheck = false;
                ViewBag.Notification = "Xác thực Email không thành công!";
                ViewBag.Content = "Đã xảy ra lỗi khi xác minh email của bạn. Hãy đảm bảo rằng bạn đã nhấp vào đúng liên kết xác minh hoặc liên hệ với bộ phận hỗ trợ.";
				ViewBag.Redirect = Url.Action("Index", "Home");
				return View();
            }
        }

        #region handle
        public IActionResult GetFirebaseConfig()
        {
            var firebaseConfig = new
            {
                apiKey = "AIzaSyA47Xp0olzLcKWmXFTARF6Wwnf1uCKe4M4",
                authDomain = "eduquiz-d2936.firebaseapp.com",
                projectId = "eduquiz-d2936",
                storageBucket = "eduquiz-d2936.appspot.com",
                messagingSenderId = "762295753722",
                appId = "1:762295753722:web:1a333ae03e9f4004a86243",
                measurementId = "G-P7GLMFJL3J"
            };
            return Json(firebaseConfig);
        }
        [HttpPost]
        public async Task<IActionResult> LoginWithSocial(string fullname, string email, string avatar)
        {
            try
            {
                if (!string.IsNullOrEmpty(fullname) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(avatar))
                {
                    var checkUser = _context.Users.Where(n => n.Email == email).SingleOrDefault();
                    var rfToken = _cookieAuth.GenerateRefreshToken();
                    if (checkUser == null)
                    {
                        var privacySettings = JsonConvert.SerializeObject(new PrivacyModel());
                        User user = new User
                        {
                            Email = email,
                            Username = fullname,
                            Password = "",
                            FirstName = "", // Bạn có thể sửa đổi theo nhu cầu
                            LastName = "", // Bạn có thể sửa đổi theo nhu cầu
                            RoleId = 2,
                            WorkplaceTypeId=1,
                            PrivacySettings = privacySettings,
                            ProfilePicture = avatar,
                            EmailVerified = true,
                            SubscriptionType ="free",
                            Favorite = "[]",
                            DateOfBirth = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            RefeshToken = rfToken
                        };
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                        user.RefeshToken = rfToken;
                        var acToken = _cookieAuth.GenerateToken(user);

                        HttpContext.Response.Cookies.Append("acToken", acToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });

                        HttpContext.Response.Cookies.Append("rfToken", rfToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(7)
                        });
                        return Json(new { status = true, msg = "Đăng nhập thành công" });
                    }
                    else
                    {
                        checkUser.RefeshToken = rfToken;
                        var acToken = _cookieAuth.GenerateToken(checkUser);

                        HttpContext.Response.Cookies.Append("acToken", acToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });

                        HttpContext.Response.Cookies.Append("rfToken", rfToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(7)
                        });
                        await _context.SaveChangesAsync();

                        return Json(new { status = true, msg = "Đăng nhập thành công" });
                    }
                }
                else
                {
                    return Json(new { status = false, msg = "Có lỗi xảy ra" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CheckLogin(string username, string pass)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass))
            {
                return Json(new { status = false, msg = "Vui lòng nhập mật khẩu và tài khoản" });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == username || u.Email == username) && u.Status);

            if (user == null)
            {
                return Json(new { status = false, msg = "Sai mật khẩu hoặc tài khoản" });
            }
            if (!string.IsNullOrEmpty(user.Password))
            {
                if (!BcryptNet.Verify(pass, user.Password))
                {
                    return Json(new { status = false, msg = "Sai mật khẩu hoặc tài khoản" });
                }
            }
            else
            {
               return Json(new { status = false, msg = "Sai mật khẩu hoặc tài khoản" });
            }

            if (!user.EmailVerified)
            {
                return Json(new { status = false, msg = "Vui lòng kiểm tra email để kích hoạt tài khoản" });
            }
            var rfToken = _cookieAuth.GenerateRefreshToken();
            user.RefeshToken = rfToken;
            var acToken = _cookieAuth.GenerateToken(user);
          

            HttpContext.Response.Cookies.Append("acToken", acToken, new CookieOptions
            {
                HttpOnly = true, 
                Secure = true, 
                Expires = DateTimeOffset.UtcNow.AddDays(1) 
            });

            HttpContext.Response.Cookies.Append("rfToken", rfToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        
        [HttpPost]
        public async Task<IActionResult> RegisterAccount(string password, string email)
        {
            try
            {
                if (!IsValidEmail(email))
                {
                    return Json(new { result = "FAIL", msg = "Địa chỉ email không hợp lệ." });
                }
                string getusername = HttpContext.Session.GetString("_USERNAME");
                int? gettypeaccount = HttpContext.Session.GetInt32("_TYPEACCOUNT");
                string birthdayString = HttpContext.Session.GetString("_DATEOFBIRTH");
                

                // Kiểm tra giá trị
                if (string.IsNullOrEmpty(getusername) || gettypeaccount == null || string.IsNullOrEmpty(birthdayString))
                {
                    return Json(new { result = "FAIL", msg = "Thiếu thông tin cần thiết." });
                }

                // Chuyển đổi chuỗi ngày sinh thành DateTime
                if (!DateTime.TryParse(birthdayString, out DateTime getbirthday))
                {
                    return Json(new { result = "FAIL", msg = "Ngày sinh không hợp lệ." });
                }

                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(email))
                {
                    var checkUser = await _context.Users
                        .Where(n => n.Email == email || n.Username == getusername)
                        .SingleOrDefaultAsync();

                    if (checkUser == null)
                    {
                        var verifyToken = Guid.NewGuid().ToString();
                        var confirmationLink = Url.Action("VerifyEmail", "Account", new { token = verifyToken, email = email }, Request.Scheme);
                        SendEmail(email, "Xác thực tài khoản EduQuiz", confirmationLink, 0);
                        int workplaceTypeId;
                        switch (gettypeaccount.Value)
                        {
                            case 1:
                                workplaceTypeId = 4;
                                break;
                            case 2:
                            case 3:
                                workplaceTypeId = 2;
                                break;
                            default:
                                workplaceTypeId = 5; 
                                break;
                        }
                        var privacySettings = JsonConvert.SerializeObject(new PrivacyModel());

                        User user = new User
                        {
                            Email = email,
                            Username = getusername,
                            Password = BcryptNet.HashPassword(password),
                            FirstName = "", // Bạn có thể sửa đổi theo nhu cầu
                            LastName = "", // Bạn có thể sửa đổi theo nhu cầu
                            RoleId = gettypeaccount.Value,
                            WorkplaceTypeId = workplaceTypeId,
                            ProfilePicture = "/src/img/defaultimguser.png",
                            PrivacySettings= privacySettings,
                            LinkToken = verifyToken,
                            SubscriptionType = "free",
                            Favorite = "[]",
                            DateOfBirth = getbirthday,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                        HttpContext.Session.Clear();

                        return Json(new { result = "PASS", msg = "Vui lòng kiểm tra Email để xác minh tài khoản" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", msg = "Tài khoản hoặc Email đã tồn tại!" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", msg = "Thông tin đăng ký không hợp lệ." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CheckCaptcha(string captcharesponse)
        {
            if (string.IsNullOrEmpty(captcharesponse))
            {
                return Json(new { result = "FAIL", message = "reCAPTCHA không được giải." });
            }

            var secret = "6Lf_34EqAAAAAJo06L5MoVxrfsO8Xs3N3wVawNKk"; // Thay bằng secret key của bạn
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={captcharesponse}");
            var reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(response);

            if (reCaptchaResponse.Success)
            {
                return Json(new { result = "PASS" });
            }
            else
            {
                return Json(new { result = "FAIL", message = "Xác minh reCAPTCHA thất bại." });
            }
        }
        [HttpPost]
        public IActionResult SaveBirthday(DateTime value)
        {
            //1: Trường học, 2: Giáo viên, 3: Học sinh, Gia đình và bạn bè
            HttpContext.Session.SetString("_DATEOFBIRTH", value.ToString());

            // Trả về JSON với trạng thái thành công
            return Json(new { status = true });
        }
        [HttpPost]
        public IActionResult CheckTypeAccount(int value)
		{
            //1:Trường học, 2:Giáo viên, 3:Học sinh, Gia đình và bạn bè
            HttpContext.Session.SetInt32("_TYPEACCOUNT", value);

            // Trả về JSON với trạng thái
            return Json(new { status = true });
        }
        [HttpPost]
        public async Task<IActionResult> CheckUserName(string username)
        {
            var get2userrandom = await GenerateUniqueUsernames(2);
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { status = false, data = get2userrandom });
            }
            if (username.Length < 6)
            {
                return Json(new { status = false, message = "Tên tài khoản phải lớn hơn hoặc bằng 6 ký tự!", data = get2userrandom });
            }
            bool isAvailable = await _usernameService.IsUsernameAvailable(username);
            if (!isAvailable)
            {
                return Json(new { status = false, message = "Tên tài khoản đã tồn tại!" , data = get2userrandom });
            }
            HttpContext.Session.SetString("_USERNAME", username);
            return Json(new { status = true });
        }
        [HttpGet]
        public async Task<IActionResult> RandomUserName()
        {
            var username = await GenerateUniqueUsernames(1);
            return Json(new { status = true ,data = username });
        }

        private string GenerateRandomUsername()
        {
            var lastname = new[] { "tuan", "hoang", "kiet", "binh", "thuan", "quang", "truong", "khang", "hung", "tai" };
            var firstname = new[] { "le", "dang", "nguyen", "luong", "tran", "do", "mai", "lam" };

            string adjective = lastname[_random.Next(lastname.Length)];
            string animal = firstname[_random.Next(firstname.Length)];
            int number = _random.Next(100, 999);

            return $"{adjective}_{animal}{number}";
        }

        // Phương thức tạo danh sách tên người dùng ngẫu nhiên
        private async Task<List<string>> GenerateUniqueUsernames(int count)
        {
            var usernames = new List<string>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                string username;
                do
                {
                    username = GenerateRandomUsername();
                } while (await  _context.Users.AnyAsync(x => x.Username == username) || usernames.Contains(username));

                usernames.Add(username);
            }

            return usernames;
        }

        private void SendEmail(string recipientEmail, string subject, string message, int select)
        {
            string template;
            if (select == 0)
            {
                template = $@"
            <table  
                border=""0"" align=""center"" cellspacing=""0"" cellpadding=""0"" bgcolor=""white"" width=""650"" style=""background: #f5d9c0;padding: 0 20px 0 65px;border-radius: 10px;justify-content: center;display: flex;"">
                <tr>
                    <td>
                        <!--Child table-->
                        <table border=""0"" cellspacing=""0"" cellpadding=""0"" style=""color:#0f3462; font-family: sans-serif;"">
                            <tr>
                                <td>
                                    <h1 style=""text-align:center; margin-top: 25px;font-size:50px;"">
                                        <i>Edu</i><span style=""color:lightcoral"">Quiz</span>
                                    </h1>
                                </td>
                            </tr>
                            <tr>
                                <td style=""text-align: center;"">
                                                    <p style=""font-size:30px;margin: 0 0 10px 0;color: #36b445;text-align:center;text-align: center;"">Click vào nút bên dưới để xác thực Email</p>
                                                    <a href=""{message}"" style=""padding:8px 10px;background:green;font-size:20px;color:white;margin: 0;font-weight: 600;text-decoration: none;border-radius: 10px;"">Xác thực Email</p>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""font-size:30px;color: #0f3462;font-weight: 600;"">Vui lòng kiểm tra thông tin, Cảm ơn!</p>                                   
                                </td>
                            </tr>
                        </table>
                        <!--/Child table-->
                    </td>
                </tr>
            </table>";
            }
            else
            {
                template = $@"
            <table  
                border=""0"" align=""center"" cellspacing=""0"" cellpadding=""0"" bgcolor=""white"" width=""650"" style=""background: #f5d9c0;padding: 0 20px 0 65px;border-radius: 10px;justify-content: center;display: flex;"">
                <tr>
                    <td>
                        <!--Child table-->
                        <table border=""0"" cellspacing=""0"" cellpadding=""0"" style=""color:#0f3462; font-family: sans-serif;"">
                            <tr>
                                <td>
                                    <h1 style=""text-align:center; margin-top: 25px;font-size:50px;"">
                                        <i>Edu</i><span style=""color:lightcoral"">Quiz</span>
                                    </h1>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""font-size:30px;color: #36b445;margin: 0;text-align:center;text-align: center;"">Mã xác minh của bạn là:</p>
                                    <p style=""font-size:60px;color:lightcoral;margin: 0;text-align: center;font-weight: 600;"">{message}</p>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""font-size:30px;color: #0f3462;font-weight: 600;"">Vui lòng kiểm tra thông tin, Cảm ơn!</p>                                   
                                </td>
                            </tr>
                        </table>
                        <!--/Child table-->
                    </td>
                </tr>
            </table>";
            }
            _emailService.SendEmail(recipientEmail, subject, template);
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
