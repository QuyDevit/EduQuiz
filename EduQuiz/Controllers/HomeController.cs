using EduQuiz.Models;
using EduQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace EduQuiz.Controllers
{
    public class HomeController : Controller
    {
        private readonly GeminiAiService _geminiAiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, GeminiAiService geminiAiService)
        {
            _logger = logger;
            _geminiAiService = geminiAiService;
        }
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("error404")]
        public IActionResult Error404()
        {
            return View();
        }
        [Route("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult EmailTemplate()
        {
            return View();
        }

        #region handle
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                var responseContent = await _geminiAiService.GenerateResponse(request);
                return Ok(responseContent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = new { message = ex.Message } });
            }
        }
      
        #endregion
    }
}
