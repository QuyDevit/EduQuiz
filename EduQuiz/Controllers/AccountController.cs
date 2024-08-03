using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using EduQuiz.Models.EF;
using EduQuiz.Models;
using EduQuiz.Services;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Mail;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Controllers
{
	public class AccountController : Controller
    {
        private readonly EduQuizDBContext _context;
        private readonly UsernameService _usernameService;
        private readonly Random _random;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(EduQuizDBContext context, UsernameService usernameService, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _usernameService = usernameService;
            _random = new Random(); // Khởi tạo đối tượng Random
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Register()
		{
			return View();
		}
		public IActionResult ChooseBirthday()
		{
			return View();
		}
        public IActionResult Username()
        {
            return View();
        }
        public IActionResult SignupOption() { 
            return View();
        }
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
        [HttpPost]
        public async Task<IActionResult> RegisterAccount(string password, string email)
        {
            try
            {
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

                        User user = new User
                        {
                            Email = email,
                            Username = getusername,
                            Password = BCrypt.Net.BCrypt.HashPassword(password),
                            FirstName = "", // Bạn có thể sửa đổi theo nhu cầu
                            LastName = "", // Bạn có thể sửa đổi theo nhu cầu
                            RoleId = gettypeaccount.Value,
                            LinkToken = verifyToken,
                            DateOfBirth = getbirthday,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();

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

            var secret = "6LdFqR4qAAAAAOdBNXXRzgOEhERH9rn4aHLdYWLB"; // Thay bằng secret key của bạn
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
        public IActionResult CheckUserName(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                var get2userrandom = GenerateUniqueUsernames(2);
                return Json(new { status = false, data = get2userrandom });
            }
            if (username.Length < 6)
            {
                return Json(new { status = false, message = "Tên tài khoản phải lớn hơn hoặc bằng 6 ký tự!" });
            }
            bool isAvailable = _usernameService.IsUsernameAvailable(username);
            if (!isAvailable)
            {
                return Json(new { status = false, message = "Tên tài khoản đã tồn tại!" });
            }
            HttpContext.Session.SetString("_USERNAME", username);
            return Json(new { status = true });
        }
        [HttpPost]
        public IActionResult RandomUserName()
        {
            var username = GenerateUniqueUsernames(1);
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
        private List<string> GenerateUniqueUsernames(int count)
        {
            var usernames = new List<string>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                string username;
                do
                {
                    username = GenerateRandomUsername();
                } while (_context.Users.Any(x => x.Username == username) || usernames.Contains(username));

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
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("pingvocuc555@gmail.com", "EduQuiz");
                mail.To.Add(recipientEmail);
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = template; // Set your HTML template as the email body

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("pingvocuc555@gmail.com", "bkxflmlmnyxxrzrz");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
        #endregion
    }
}
