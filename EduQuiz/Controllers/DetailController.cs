using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EduQuiz.Controllers
{
    public class DetailController : Controller
    {
        private readonly EduQuizDBContext _context;
        public DetailController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("detail/{id:guid}")]
        public async Task<IActionResult> Index(Guid id)
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (sessionData != null)
            {
                        var check = await _context.EduQuizs
                            .Include(e => e.Questions)
                            .ThenInclude(q => q.Choices)
                            .FirstOrDefaultAsync(d =>d.Uuid == id && d.Type==1);

                if (check == null)
                {
                    var referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer))
                    {
                        return Redirect(referer);
                    }

                    return RedirectToAction("Index", "HomeDashboard");
                }
                else
                {
                    List<int> orderquestion = JsonConvert.DeserializeObject<List<int>>(check.OrderQuestion);

                    var getdata = new Models.EduQuizData
                    {
                        Uuid = check.Uuid,
                        Description = check.Description,
                        Title = check.Title,
                        ImageCover = check.ImageCover,
                        Type = check.Type ?? 0,
                        Visibility = check.Visibility,
                        ThemeId = check.ThemeId ?? 0,
                        MusicId = check.MusicId ?? 0,
                        UserId = check.UserId ?? 0,
                        Questions = check.Questions.Select(q => new QuestionData
                        {
                            Id = q.Id,
                            QuestionText = q.QuestionText,
                            TypeQuestion = q.TypeQuestion,
                            TypeAnswer = q.TypeAnswer ?? 0,
                            Time = q.Time ?? 0,
                            PointsMultiplier = q.PointsMultiplier ?? 0,
                            Image = q.Image,
                            ImageEffect = q.ImageEffect,
                            Choices = q.Choices.OrderBy(c => c.DisplayOrder).Select(c => new ChoiceData
                            {
                                Id = c.Id,
                                Answer = c.Answer,
                                IsCorrect = c.IsCorrect,
                                DisplayOrder = c.DisplayOrder
                            }).ToList()
                        }).ToList()
                    };
                    var orderLookup = orderquestion.Select((id, index) => new { id, index })
                     .ToDictionary(x => x.id, x => x.index);

                    // Sắp xếp danh sách câu hỏi theo thứ tự trong orderid
                    getdata.Questions = getdata.Questions
                        .OrderBy(q => orderLookup.GetValueOrDefault(q.Id, int.MaxValue))
                        .ToList();
                    var getInfoUser = await _context.Users.FindAsync(getdata.UserId);
                    if(getInfoUser != null)
                    {
                        ViewBag.Data = JsonConvert.SerializeObject(new { UserName = getInfoUser.Username, Avatar = getInfoUser.ProfilePicture });
                    }
                    return View(getdata);
                }
            }
            else
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
        }
    }
}
