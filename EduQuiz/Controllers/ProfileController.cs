using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize]
    public class ProfileController : Controller
    {
        private readonly EduQuizDBContext _context;
        public ProfileController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("/profiles/manage")]
        public async Task <IActionResult> Index()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var getUser = await _context.Users.FindAsync(iduser);
                return View(getUser);
            }
            return RedirectToAction("Index", "Home");
        }
        [Route("/profiles/{id:guid}")]
        public IActionResult ProfilePage(Guid id)
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
        [Route("/profiles/{id:guid}/about")]
        public IActionResult ProfilePageAbout(Guid id)
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
