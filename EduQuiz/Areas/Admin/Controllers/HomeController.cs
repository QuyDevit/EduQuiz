using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize("Admin")]
    public class HomeController : Controller
    {
        private readonly EduQuizDBContext _context;
        public HomeController(EduQuizDBContext context) {
            _context = context;
        } 
        [Route("admin/dashboard")]
        public async Task<IActionResult> Index()
        {

            return View();
        }
        public async Task<IActionResult> ManageFile()
        {

            return View();
        }
    }
}
