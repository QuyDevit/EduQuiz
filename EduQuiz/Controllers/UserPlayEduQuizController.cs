using Microsoft.AspNetCore.Mvc;

namespace EduQuiz.Controllers
{
    public class UserPlayEduQuizController : Controller
    {
        [Route("pin")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("join")]
        public IActionResult JoinGame()
        {
            return View();
        }
    }
}
