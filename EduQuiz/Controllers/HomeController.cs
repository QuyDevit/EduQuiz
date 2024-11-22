using EduQuiz.Events;
using EduQuiz.Gemini.DTO;
using EduQuiz.Helper;
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

        private readonly ILogger<HomeController> _logger;
        private readonly ChatScope _chatScope;

		public HomeController(ILogger<HomeController> logger,ChatScope chatScope)
        {
            _logger = logger;
            _chatScope = chatScope;
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

        #region handle
        [HttpPost]
		public async Task<ActionResult<string>> GenerateAnswer([FromBody] Conversation request)
		{
			if (string.IsNullOrWhiteSpace(request.Question))
			{
				return Ok("Vui lòng nhập câu hỏi!");
			}

			if (GeneralHelper.GetTotalWords(request.Question) > 30)
			{
				return Ok("Hỏi ngắn thôi (dưới 30 từ ấy) 😡\nHỏi nhiều quá tôi ngộp.");
			}

			try
			{
				var result = await _chatScope.GenerateAnswer(request);

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cannot generate answer");
				return Ok("Nhắn từ từ thôi 😡\nChờ tôi suy nghĩ nữa.");
			}
		}

		#endregion
	}
}
