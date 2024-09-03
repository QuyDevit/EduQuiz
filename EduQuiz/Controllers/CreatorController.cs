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
        public async Task<IActionResult> Index(Guid id)
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (sessionData != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);

                int.TryParse(userInfo?.Id?.ToString(), out int userId);
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    var check = await _context.EduQuizs
                        .Include(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                        .FirstOrDefaultAsync(d => d.Uuid == id);

                    if (check == null)
                    {
                        ViewBag.Data = new { quizId = id, userId = user.Id };
                        return View(null);
                    }
                    else
                    {
                        if (check.UserId != user.Id)
                        {
                            return RedirectToAction("Index", "HomeDashboard");
                        }
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

                        ViewBag.Data = new { quizId = id, userId = user.Id };
                        return View(getdata);
                    }
                }

            }
            return RedirectToAction("Index", "HomeDashboard");
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
                Models.EF.EduQuiz checkEduQuiz;
                if (data.Id != 0)
                {
                    checkEduQuiz = await _context.EduQuizs.FindAsync(data.Id);

                    await _context.Entry(checkEduQuiz)
                        .Collection(q => q.Questions)
                        .Query()
                        .Include(q => q.Choices)
                        .LoadAsync();
                }
                else
                {
                    checkEduQuiz = await _context.EduQuizs
                        .Include(q => q.Questions)
                        .ThenInclude(q => q.Choices)
                        .FirstOrDefaultAsync(q => q.Uuid == data.Uuid);
                }

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
                        CreatedAt = DateTime.Now,
                        UpdateAt = DateTime.Now,
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
                    if(checkEduQuiz.UserId != data.UserId)
                    {
                        return Json(new { result = "FAIL", message = "Bạn không có quyền chỉnh sửa!" });
                    }
                    // Cập nhật EduQuiz đã tồn tại
                    checkEduQuiz.Description = data.Description;
                    checkEduQuiz.Title = data.Title;
                    checkEduQuiz.ImageCover = data.ImageCover;
                    checkEduQuiz.Type = 0;
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

                var eduQuiz = await _context.EduQuizs
                    .Include(d => d.Questions)
                        .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(d => d.Uuid == data.Uuid);
                var getdata = new Models.EduQuizData
                {
                    Uuid = eduQuiz.Uuid,
                    Description = eduQuiz.Description,
                    Title = eduQuiz.Title,
                    ImageCover = eduQuiz.ImageCover,
                    Type = eduQuiz.Type ?? 0,
                    Visibility = eduQuiz.Visibility,
                    ThemeId = eduQuiz.ThemeId ?? 0,
                    MusicId = eduQuiz.MusicId ?? 0,
                    UserId = eduQuiz.UserId ?? 0,
                    Questions = eduQuiz.Questions.Select(q => new QuestionData
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

                // Sort questions based on `orderid`
                if (!orderid.Contains(0))
                {
                    var orderLookup = orderid.Select((id, index) => new { id, index })
                             .ToDictionary(x => x.id, x => x.index);

                    // Sắp xếp danh sách câu hỏi theo thứ tự trong orderid
                    getdata.Questions = getdata.Questions
                        .OrderBy(q => orderLookup.GetValueOrDefault(q.Id, int.MaxValue))
                        .ToList();

                }
                else if (orderid.Count(x => x == 0) == 1)
                {
                    var zeroIndex = orderid.IndexOf(0);
                    var lastQuestion = getdata.Questions.Last();

                    // Replace 0 with last question's ID
                    orderid[zeroIndex] = lastQuestion.Id;

                    var orderedQuestions = getdata.Questions
                        .OrderBy(q => orderid.IndexOf(q.Id))
                        .ToList();

                    // Update the order question data in the tracked entity
                    eduQuiz.OrderQuestion = JsonConvert.SerializeObject(orderid);
                    getdata.Questions = orderedQuestions;
                }
                else
                {
                    // Update order question if orderid has more than one 0
                    eduQuiz.OrderQuestion = JsonConvert.SerializeObject(getdata.Questions.Select(q => q.Id).ToList());
                }
                await _context.SaveChangesAsync();

                return Json(new { result = "PASS", data = JsonConvert.SerializeObject(getdata) });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { result = "FAIL", message = ex.Message });
            }
        }

        public async Task<IActionResult> SaveTypeEduQuiz(Guid idquiz,int userid)
        {
            try
            {
                var geteduquiz = await _context.EduQuizs.FirstOrDefaultAsync(q=>q.Uuid == idquiz);
                if(geteduquiz == null)
                {
                    return Json(new { result = "FAIL", msg = "Eduquiz không tồn tại" });
                }
                else
                {
                    if(geteduquiz.UserId != userid)
                    {
                        return Json(new { result = "FAIL", msg = "Bạn không có quyền chỉnh sửa" });
                    }
                    else
                    {
                        geteduquiz.Type = 1;
                        await _context.SaveChangesAsync();
                        return Json(new { result = "PASS" });
                    }
                }
                
            }
            catch (Exception ex) {
                return Json(new { result = "FAIL", msg = $"Lưu thất bại: {ex.Message}" });
            }
        }
        #endregion
    }
}
