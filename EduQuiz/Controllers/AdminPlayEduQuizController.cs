using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace EduQuiz.Controllers
{
    [CustomAuthorize]
    public class AdminPlayEduQuizController : Controller
    {
        [Route("playmode")]
        public IActionResult Index(Guid quizId)
        {
            return View();
        }
    
        [Route("play")]
        public IActionResult Lobby(Guid quizId)
        {
            return View();
        }

        public IActionResult GenerateQRCode(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required.");
            }

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            using (var bitmap = qrCode.GetGraphic(20))
            {
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    return File(ms.ToArray(), "image/png");
                }
            }
        }
    }
}
