using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize]
    public class DiscoverController : Controller
    {
        private readonly EduQuizDBContext _context;
        public DiscoverController(EduQuizDBContext context) {
            _context = context;
        }
        [Route("/discover")]
        public async Task<IActionResult> Index()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
