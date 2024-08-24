using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EduQuiz.Controllers
{
    public class CreatorController : Controller
    {
        private readonly EduQuizDBContext _context;
        private readonly HttpClient _httpClient;
        public CreatorController(EduQuizDBContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }
        [Route("creator/{id:guid}")]
        public async  Task<IActionResult> Index(Guid id)
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (sessionData != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                string email = userInfo?.Email;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    var check = await _context.EduQuizs.FirstOrDefaultAsync(d => d.Uuid == id);
                    ViewBag.Data = new { quizId = id, userId = user.Id };
                    if (check == null)
                    {
                        return View(null);
                    }
                    else
                    {
                        List<int> orderquestion = JsonConvert.DeserializeObject<List<int>>(check.OrderQuestion);
                        var getdata = await _context.EduQuizs
                            .Where(d => d.Uuid == id)
                            .Select(dataEduQuizUpdated => new Models.EduQuizData
                            {
                                Uuid = dataEduQuizUpdated.Uuid,
                                Description = dataEduQuizUpdated.Description,
                                Title = dataEduQuizUpdated.Title,
                                ImageCover = dataEduQuizUpdated.ImageCover,
                                Type = dataEduQuizUpdated.Type ?? 0,
                                Visibility = dataEduQuizUpdated.Visibility,
                                ThemeId = dataEduQuizUpdated.ThemeId ?? 0,
                                MusicId = dataEduQuizUpdated.MusicId ?? 0,
                                UserId = dataEduQuizUpdated.UserId ?? 0,
                                Questions = dataEduQuizUpdated.Questions.Select(q => new QuestionData
                                {
                                    Id = q.Id,
                                    QuestionText = q.QuestionText,
                                    TypeQuestion = q.TypeQuestion,
                                    TypeAnswer = q.TypeAnswer ?? 0,
                                    Time = q.Time ?? 0,
                                    PointsMultiplier = q.PointsMultiplier ?? 0,
                                    Image = q.Image,
                                    ImageEffect = q.ImageEffect,
                                    Choices = q.Choices.Select(c => new ChoiceData
                                    {
                                        Id = c.Id,
                                        Answer = c.Answer,
                                        IsCorrect = c.IsCorrect,
                                        DisplayOrder = c.DisplayOrder
                                    }).ToList()
                                }).ToList()
                            })
                            .FirstOrDefaultAsync();
                        getdata.Questions = getdata.Questions
                            .OrderBy(q => orderquestion.IndexOf(q.Id))
                            .ToList();
                        return View(getdata);
                    }
                }  
            }
            return RedirectToAction("Index", "Home");
        }
        #region handle
        public async Task<IActionResult> saveImgQuestion([FromForm] IFormFile image, [FromForm] string quizid)
        {
            try
            {
                if (image != null && image.Length > 0)
                {
                    // Đường dẫn thư mục chứa ảnh
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src", "img", "question");

                    // Tạo thư mục nếu không tồn tại
                    Directory.CreateDirectory(uploadsFolder);

                    // Lấy phần đuôi file (file extension)
                    var fileExtension = Path.GetExtension(image.FileName);
                    var uniqueFileName = quizid + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Kiểm tra và xóa file nếu đã tồn tại
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Lưu file lên server
                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    return Json(new { result = "PASS", url = $"/src/img/question/{uniqueFileName}" });
                }

                return Json(new { result = "FAIL", msg = "File image không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = $"Lưu thất bại: {ex.Message}" });
            }
        }
        public async Task<IActionResult> AnalyzeImage([FromForm] IFormFile file, [FromForm] int userid)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var apiKey = "yjFKozhgMrnYyhUFDLyNj7DvXLab7vkG";
            var picpurifyUrl = "https://www.picpurify.com/analyse/1.1";

            using (var form = new MultipartFormDataContent())
            {
                // Thêm file vào form data
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                form.Add(fileContent, "file_image", file.FileName);

                // Thêm các tham số khác vào form data
                form.Add(new StringContent(apiKey), "API_KEY");
                form.Add(new StringContent("porn_moderation,weapon_moderation,gore_moderation"), "task");
                form.Add(new StringContent("web_app"), "origin_id"); 
                form.Add(new StringContent("user123_session456"), "reference_id"); 
                // Gửi yêu cầu POST
                var response = await _httpClient.PostAsync(picpurifyUrl, form);

                // Đọc phản hồi từ API
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (jsonResponse?.status == "success" && jsonResponse?.final_decision == "KO")
                {
                    var getuser = await _context.Users.FindAsync(userid);
                    if (getuser != null)
                    {
                        if (getuser.ViolationCount == 2)
                        {
                            getuser.Status = false;
                        }
                        getuser.ViolationCount += 1;
                        await _context.SaveChangesAsync();
                        // Cập nhật jsonResponse với ViolationCount mới
                        jsonResponse.violation_count = getuser.ViolationCount;
                    }

                }
                var updatedResponseString = JsonConvert.SerializeObject(jsonResponse);
                return Content(updatedResponseString, "application/json");
            }
        }
        public async Task<IActionResult> GetListMusic()
        {
            try
            {
                var list = await _context.Musics.ToListAsync();
                return Json(new { data = list, result = "PASS" });
            }
            catch (Exception ) {
                return Json(new { result = "FAIL" });
            }
        }
        public async Task<IActionResult> GetListTheme()
        {
            try
            {
                var list = await _context.Themes.ToListAsync();
                return Json(new { data = list, result = "PASS" });
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL" });
            }
        }

        public async Task<IActionResult> AutoSaveEduQuiz([FromBody] Models.EduQuizData data)
        {
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<int> orderid = new List<int>();
                var checkEduQuiz = await _context.EduQuizs
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.Uuid == data.Uuid);

                if (checkEduQuiz == null)
                {
                    // Tạo EduQuiz mới
                    var newEduQuiz = new Models.EF.EduQuiz
                    {
                        Uuid = data.Uuid,
                        Description = data.Description,
                        Title = data.Title,
                        ImageCover = data.ImageCover,
                        Type = data.Type,
                        Visibility = data.Visibility,
                        ThemeId = data.ThemeId,
                        MusicId = data.MusicId,
                        UserId = data.UserId,
                        CreatedAt = DateTime.Now
                    };

                    // Thêm các câu hỏi và lựa chọn vào EduQuiz mới
                    if (data.Questions?.Count > 0)
                    {
                        foreach (var item in data.Questions)
                        {
                            orderid.Add(item.Id);
                            var newQuestion = new Question
                            {
                                EduQuiz = newEduQuiz, // Gán trực tiếp vào EduQuiz mới
                                QuestionText = item.QuestionText,
                                TypeQuestion = item.TypeQuestion,
                                TypeAnswer = item.TypeAnswer,
                                Time = item.Time,
                                PointsMultiplier = item.PointsMultiplier,
                                Image = item.Image,
                                ImageEffect = item.ImageEffect
                            };

                            if (item.Choices?.Count > 0)
                            {
                                var newChoices = item.Choices.Select(choice => new Choice
                                {
                                    Question = newQuestion, // Gán trực tiếp vào Question mới
                                    Answer = choice.Answer,
                                    IsCorrect = choice.IsCorrect,
                                    DisplayOrder = choice.DisplayOrder
                                }).ToList();

                                newQuestion.Choices = newChoices; // Gán danh sách Choices vào Question
                            }

                            _context.Questions.Add(newQuestion);
                        }
                    }

                    _context.EduQuizs.Add(newEduQuiz);
                }
                else
                {
                    // Cập nhật EduQuiz đã tồn tại
                    checkEduQuiz.Description = data.Description;
                    checkEduQuiz.Title = data.Title;
                    checkEduQuiz.ImageCover = data.ImageCover;
                    checkEduQuiz.Type = data.Type;
                    checkEduQuiz.Visibility = data.Visibility;
                    checkEduQuiz.ThemeId = data.ThemeId;
                    checkEduQuiz.MusicId = data.MusicId;
                    checkEduQuiz.UserId = data.UserId;
                    checkEduQuiz.UpdateAt = DateTime.Now;

                    var existingQuestions = checkEduQuiz.Questions.ToDictionary(q => q.Id);

                    foreach (var item in data.Questions)
                    {
                        orderid.Add(item.Id);
                        if (item.Id == 0)
                        {
                            // Thêm câu hỏi mới
                            var newQuestion = new Question
                            {
                                EduQuiz = checkEduQuiz, // Gán trực tiếp vào EduQuiz đã tồn tại
                                QuestionText = item.QuestionText,
                                TypeQuestion = item.TypeQuestion,
                                TypeAnswer = item.TypeAnswer,
                                Time = item.Time,
                                PointsMultiplier = item.PointsMultiplier,
                                Image = item.Image,
                                ImageEffect = item.ImageEffect
                            };

                            if (item.Choices?.Count > 0)
                            {
                                var newChoices = item.Choices.Select(choice => new Choice
                                {
                                    Question = newQuestion, // Gán trực tiếp vào Question mới
                                    Answer = choice.Answer,
                                    IsCorrect = choice.IsCorrect,
                                    DisplayOrder = choice.DisplayOrder
                                }).ToList();

                                newQuestion.Choices = newChoices; // Gán danh sách Choices vào Question
                            }

                            _context.Questions.Add(newQuestion);
                        }
                        else if (existingQuestions.TryGetValue(item.Id, out var existingQuestion))
                        {
                            // Cập nhật câu hỏi đã tồn tại
                            existingQuestion.QuestionText = item.QuestionText;
                            existingQuestion.TypeQuestion = item.TypeQuestion;
                            existingQuestion.TypeAnswer = item.TypeAnswer;
                            existingQuestion.Time = item.Time;
                            existingQuestion.PointsMultiplier = item.PointsMultiplier;
                            existingQuestion.Image = item.Image;
                            existingQuestion.ImageEffect = item.ImageEffect;

                            var existingChoices = existingQuestion.Choices.ToDictionary(c => c.Id);
                            foreach (var choice in item.Choices)
                            {
                                if (choice.Id == 0)
                                {
                                    var newChoice = new Choice
                                    {
                                        Question = existingQuestion, // Gán trực tiếp vào Question đã tồn tại
                                        Answer = choice.Answer,
                                        IsCorrect = choice.IsCorrect,
                                        DisplayOrder = choice.DisplayOrder
                                    };
                                    _context.Choices.Add(newChoice);
                                }
                                else if (existingChoices.TryGetValue(choice.Id, out var existingChoice))
                                {
                                    existingChoice.Answer = choice.Answer;
                                    existingChoice.IsCorrect = choice.IsCorrect;
                                    existingChoice.DisplayOrder = choice.DisplayOrder;
                                }
                            }
                            // Xóa các lựa chọn không còn tồn tại
                            var newChoiceIds = item.Choices.Select(c => c.Id).ToHashSet();
                            var choicesToRemove = existingQuestion.Choices
                                .Where(c => !newChoiceIds.Contains(c.Id))
                                .ToList();
                            _context.Choices.RemoveRange(choicesToRemove);
                        }
                    }

                    // Xóa câu hỏi và lựa chọn không còn tồn tại
                    var newQuestionIds = data.Questions.Select(q => q.Id).ToHashSet();
                    var questionsToRemove = checkEduQuiz.Questions
                        .Where(q => !newQuestionIds.Contains(q.Id))
                        .ToList();
                    _context.Questions.RemoveRange(questionsToRemove);
                }

                await _context.SaveChangesAsync(); // Lưu mọi thay đổi một lần duy nhất
                await transaction.CommitAsync();

                var getdata = await _context.EduQuizs
                 .Where(d => d.Uuid == data.Uuid)
                 .Select(dataEduQuizUpdated => new Models.EduQuizData
                 {
                     Uuid = dataEduQuizUpdated.Uuid,
                     Description = dataEduQuizUpdated.Description,
                     Title = dataEduQuizUpdated.Title,
                     ImageCover = dataEduQuizUpdated.ImageCover,
                     Type = dataEduQuizUpdated.Type ?? 0,
                     Visibility = dataEduQuizUpdated.Visibility,
                     ThemeId = dataEduQuizUpdated.ThemeId ?? 0,
                     MusicId = dataEduQuizUpdated.MusicId ?? 0,
                     UserId = dataEduQuizUpdated.UserId ?? 0,
                     Questions = dataEduQuizUpdated.Questions.Select(q => new QuestionData
                     {
                         Id = q.Id,
                         QuestionText = q.QuestionText,
                         TypeQuestion = q.TypeQuestion,
                         TypeAnswer = q.TypeAnswer ?? 0,
                         Time = q.Time ?? 0,
                         PointsMultiplier = q.PointsMultiplier ?? 0,
                         Image = q.Image,
                         ImageEffect = q.ImageEffect,
                         Choices = q.Choices.Select(c => new ChoiceData
                         {
                             Id = c.Id,
                             Answer = c.Answer,
                             IsCorrect = c.IsCorrect,
                             DisplayOrder = c.DisplayOrder
                         }).ToList()
                     }).ToList()
                 })
                 .FirstOrDefaultAsync();
                if (getdata != null)
                {
                    var update = await _context.EduQuizs
                    .FirstOrDefaultAsync(d => d.Uuid == data.Uuid);
                    // Sắp xếp câu hỏi theo `orderid`
                    if (!orderid.Contains(0))
                    {
                        getdata.Questions = getdata.Questions
                            .OrderBy(q => orderid.IndexOf(q.Id))
                            .ToList();
                    }
                    else
                    {
                        if(orderid.Count(x => x == 0) == 1){
                            var questions = getdata.Questions;
                            var newQuestion = questions.Last(); // Phần tử mới (cuối cùng trong danh sách)

                            // Loại bỏ phần tử mới khỏi danh sách câu hỏi
                            var existingQuestions = questions.Take(questions.Count - 1).ToList();

                            // Xác định vị trí để chèn phần tử mới
                            var zeroIndex = orderid.IndexOf(0);

                            // Nếu có phần tử nào có Id = 0 trong orderid
                            if (zeroIndex != -1)
                            {
                                // Thay thế phần tử có Id = 0 bằng phần tử mới trong orderid
                                orderid[zeroIndex] = newQuestion.Id;
                            }
                            // Sắp xếp các câu hỏi còn lại theo thứ tự trong orderid
                            var orderedQuestions = existingQuestions
                                .OrderBy(q => orderid.IndexOf(q.Id))
                                .ToList();

                            // Thêm phần tử mới vào danh sách đã sắp xếp
                            orderedQuestions.Insert(zeroIndex, newQuestion);

                            // Cập nhật danh sách câu hỏi đã sắp xếp
                            getdata.Questions = orderedQuestions;
                            update.OrderQuestion = JsonConvert.SerializeObject(orderid);
                        }
                        else
                        {
                            update.OrderQuestion = JsonConvert.SerializeObject                          (getdata.Questions.Select(q => q.Id).ToList());
                        }
                        
                    }
                    
                    await _context.SaveChangesAsync();
                }

                return Json(new { result = "PASS", data = JsonConvert.SerializeObject(getdata) });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { result = "FAIL", message = ex.Message });
            }
        }

        #endregion
    }
}
