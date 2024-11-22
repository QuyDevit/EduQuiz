using Microsoft.AspNetCore.Mvc;

namespace EduQuiz.ApiControllers
{
    [ApiController]  
    [Route("api/[controller]")]
    public class HomeApiController : Controller
    {
        public async Task <IActionResult> GetHomeData()
        {
            return Json(new {ok = "letgo"});
        }
    }
}
