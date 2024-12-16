using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EduQuiz.ApiControllers
{
    [ApiController]  
    [Route("api/[controller]")]
    public class HomeApiController : Controller
    {
        private readonly EduQuizDBContext _context;
        public HomeApiController(EduQuizDBContext context)
        {
            _context = context;
        }
        [ApiKeyAuthorize]
        [HttpGet("GetHomeData")]
        public async Task <IActionResult> GetHomeData()
        {
            var getlistTopic = await _context.Interests.Select(n=>new
            {
                n.Id,
                n.Name,
                n.Image
            })
            .ToListAsync();
            List<HomeEduQuizView> listEduQuizHot = listEduQuizHot = await _context.EduQuizs
                    .Where(n => n.Visibility && n.Type == 1 && n.Status && n.UserId != 8)
                    .Include(n => n.User)
                    .Select(n => new HomeEduQuizView
                    {
                        Id = n.Id,
                        Uuid = n.Uuid,
                        Title = n.Title,
                        Image = n.ImageCover,
                        Avatar = n.User.ProfilePicture,
                        UserName = n.User.Username,
                        SumPlay = _context.QuizSessions.Count(p => p.EduQuizId == n.Id),
                        SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id)
                    })
                    .OrderByDescending(n => n.SumPlay)
                    .Take(8)
                    .ToListAsync();
            var listProfileUser = await _context.Profile
              .Where(n => n.UserId != 8 && n.Status)
              .Include(n => n.User)
              .Select(n => new ProfileDiscover
              {
                  Avatar = n.User.ProfilePicture,
                  Id = n.Id,
                  ImgCover = n.Image,
                  TitlePage = n.TitlePage,
                  UserName = n.User.Username,
                  Uuid = n.Uuid,
                  SumEduQuiz = _context.EduQuizs.Count(p => p.UserId == n.UserId),
              }).ToListAsync();
            var data = new
            {
                getlistTopic,
                listEduQuizHot,
                listProfileUser
            };
            return Json(data);
        }
    }
}
