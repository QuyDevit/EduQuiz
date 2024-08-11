using Microsoft.AspNetCore.Mvc;

namespace EduQuiz.Controllers
{
    public class CreatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
