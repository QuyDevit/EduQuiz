
using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using EduQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class CreatorController : Controller
    {
        private readonly EduQuizDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly GeminiAiService _geminiAiService;
        private readonly string content = "Vui lòng chọn đúng 1 thể loại phù hợp với bộ câu hỏi dưới không cần giải thích ví dụ output [{\"id\":...,\"name\":\"...\"}]: [ { \"id\": 1, \"name\": \"Nghệ thuật\" }, { \"id\": 2, \"name\": \"Sinh học\" }, { \"id\": 3, \"name\": \"Kinh doanh\" }, { \"id\": 4, \"name\": \"Hóa học\" }, { \"id\": 5, \"name\": \"Tin tức hiện tại\" }, { \"id\": 6, \"name\": \"Kinh tế\" }, { \"id\": 7, \"name\": \"Tiếng Anh\" }, { \"id\": 8, \"name\": \"Giải trí\" }, { \"id\": 9, \"name\": \"Kiến thức tổng hợp\" }, { \"id\": 10, \"name\": \"Địa lý\" }, { \"id\": 11, \"name\": \"Lịch sử\" }, { \"id\": 12, \"name\": \"Ngôn ngữ\" }, { \"id\": 13, \"name\": \"Luật\" }, { \"id\": 14, \"name\": \"Toán học\" }, { \"id\": 15, \"name\": \"Âm nhạc\" }, { \"id\": 16, \"name\": \"Vật lý\" }, { \"id\": 17, \"name\": \"Chính trị\" }, { \"id\": 18, \"name\": \"Văn hóa phổ biến\" }, { \"id\": 19, \"name\": \"Tâm lý học\" }, { \"id\": 20, \"name\": \"Tôn giáo\" }, { \"id\": 21, \"name\": \"Khoa học\" }, { \"id\": 22, \"name\": \"Nghiên cứu xã hội\" }, { \"id\": 23, \"name\": \"Thể thao\" }, { \"id\": 24, \"name\": \"Công nghệ\" } ] Bộ câu hỏi:";
        public CreatorController(EduQuizDBContext context, HttpClient httpClient, GeminiAiService geminiAiService)
        {
            _context = context;
            _httpClient = httpClient;
            _geminiAiService = geminiAiService;
        }
        [Route("creator/{id:guid}")]
        public async Task<IActionResult> Index(Guid id)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            // Sử dụng các giá trị trong logic của bạn
            var iduser = int.Parse(userId ?? "1");

            var user = await _context.Users.FindAsync(iduser);
            if (user == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var check = await _context.EduQuizs
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(d => d.Uuid == id && d.Status == true);

            if (check == null)
            {
                ViewBag.Data = new { quizId = id, userId = user.Id, subscriptionType = user.SubscriptionType };
                return View(null);
            }
            if (check.UserId != user.Id)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var subsType = (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate > DateTime.Now) ? "vip" : "free";
            if(subsType != "vip")
            {
                var checkvip = await _context.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .ToListAsync();

                if (checkvip.Any(gm => gm.Group.SubscriptionEndDate.HasValue && gm.Group.SubscriptionEndDate > DateTime.Now))
                {
                    subsType = "vip";
                }
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

            ViewBag.Data = new { quizId = id, userId = user.Id, subscriptionType = subsType };
            return View(getdata);
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
                var list = await _context.Musics.Where(n=>n.Status).ToListAsync();
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
                var list = await _context.Themes.Where(n => n.Status).ToListAsync();
                return Json(new { data = list, result = "PASS" });
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL" });
            }
        }

        public async Task<IActionResult> AutoSaveEduQuiz([FromBody] EduQuizData data)
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
                        ImageCover = "/src/img/EduQuizDefault.png",
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
                var getdata = new EduQuizData
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

                if (!orderid.Contains(0))
                {
                    var orderLookup = orderid.Select((id, index) => new { id, index })
                             .ToDictionary(x => x.id, x => x.index);

                    // Sắp xếp danh sách câu hỏi theo thứ tự trong orderid
                    getdata.Questions = getdata.Questions
                        .OrderBy(q => orderLookup.GetValueOrDefault(q.Id, int.MaxValue))
                        .ToList();
                    eduQuiz.OrderQuestion = JsonConvert.SerializeObject(orderid);
                }
                else if (orderid.Count(x => x == 0) == 1)
                {
                    var zeroIndex = orderid.IndexOf(0);
                    var lastQuestion = getdata.Questions.Last();

                    orderid[zeroIndex] = lastQuestion.Id;

                    var orderedQuestions = getdata.Questions
                        .OrderBy(q => orderid.IndexOf(q.Id))
                        .ToList();

                    eduQuiz.OrderQuestion = JsonConvert.SerializeObject(orderid);
                    getdata.Questions = orderedQuestions;
                }
                else
                {
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
                var geteduquiz = await _context.EduQuizs.Where(q => q.Uuid == idquiz).Include(n=>n.Questions).ThenInclude(n=>n.Choices).FirstOrDefaultAsync();
                if(geteduquiz == null)
                {
                    return Json(new { result = "FAIL", msg = "Eduquiz không tồn tại" });
                }
                if (geteduquiz.UserId != userid)
                {
                    return Json(new { result = "FAIL", msg = "Bạn không có quyền chỉnh sửa" });
                }
                geteduquiz.Type = 1;
                await _context.SaveChangesAsync();
                var getdata = new EduQuizData
                {
                    Uuid = geteduquiz.Uuid,
                    Description = geteduquiz.Description,
                    Title = geteduquiz.Title,
                    ImageCover = geteduquiz.ImageCover,
                    Type = geteduquiz.Type ?? 0,
                    Visibility = geteduquiz.Visibility,
                    ThemeId = geteduquiz.ThemeId ?? 0,
                    MusicId = geteduquiz.MusicId ?? 0,
                    UserId = geteduquiz.UserId ?? 0,
                    Questions = geteduquiz.Questions.Select(q => new QuestionData
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

                await ProcessEduQuizAsync(geteduquiz, getdata);

                return Json(new { result = "PASS"});
            }
            catch (Exception ex) {
                return Json(new { result = "FAIL", msg = $"Lưu thất bại: {ex.Message}" });
            }
        }
        private async Task ProcessEduQuizAsync(Models.EF.EduQuiz geteduquiz, EduQuizData getdata)
        {
            var request = new ChatRequest
            {
                Model = "gemini-1.5-pro",
                MaxTokens = 2048,
                Messages = new[]
                {
                    new Message
                    {
                        Role = "user",
                        Content = $"{content}{getdata}"
                    }
                }
            };

            var responseContent = await _geminiAiService.GenerateResponse(request);
            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

            string extractedText = geminiResponse.Candidates[0].Content.Parts[0].Text;
            var getid = int.Parse(extractedText.Split(',')[0][^1].ToString());

            geteduquiz.TopicId = getid;
            await _context.SaveChangesAsync();
        }
        public async Task <IActionResult>ReadImportData(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { status = false });
                }
                var data = new List<QuestionData>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = 9;
                        var validTimeLimits = new List<int> { 5, 10, 20, 30, 60, 90, 120, 240 };

                        for (int row = rowCount; row <= 108; row++)
                        {
                            var questionText = worksheet.Cells[row, 2].Text;
                            if (string.IsNullOrWhiteSpace(questionText))
                            {
                                break;
                            }

                            if (questionText.Length > 120 ||
                                worksheet.Cells[row, 3].Text.Length > 75 ||
                                worksheet.Cells[row, 4].Text.Length > 75 ||
                                worksheet.Cells[row, 5].Text.Length > 75 ||
                                worksheet.Cells[row, 6].Text.Length > 75)
                            {
                                return Json(new { status = false, message = "Câu hỏi hoặc câu trả lời vượt quá giới hạn ký tự." });
                            }
                            var answers = new List<string>
                        {
                            worksheet.Cells[row, 3].Text,
                            worksheet.Cells[row, 4].Text,
                            worksheet.Cells[row, 5].Text,
                            worksheet.Cells[row, 6].Text
                        }.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

                            if (answers.Count < 2)
                            {
                                return Json(new { status = false, message = "Phải có ít nhất hai phương án trả lời." });
                            }
                            if (!int.TryParse(worksheet.Cells[row, 7].Text, out int timeLimit) || !validTimeLimits.Contains(timeLimit))
                            {
                                return Json(new { status = false, message = "Giá trị của TimeLimit không hợp lệ." });
                            }
                            var correctAnswers = worksheet.Cells[row, 8].Text;
                            if (string.IsNullOrWhiteSpace(correctAnswers) || !System.Text.RegularExpressions.Regex.IsMatch(correctAnswers, @"^(\d,?)+$"))
                            {
                                return Json(new { status = false, message = "Phải chọn ít nhất một câu trả lời đúng và định dạng phải là các số cách nhau bởi dấu phẩy." });
                            }

                            var correctAnswerIndices = correctAnswers.Split(',').Select(int.Parse).ToList();
                            if (correctAnswerIndices.Any(index => index < 1 || index > answers.Count))
                            {
                                return Json(new { status = false, message = "Các chỉ số câu trả lời đúng không hợp lệ." });
                            }
                            var choices = new List<ChoiceData>();
                            for (int i = 0; i < answers.Count; i++)
                            {
                                choices.Add(new ChoiceData
                                {
                                    Answer = answers[i],
                                    IsCorrect = correctAnswerIndices.Contains(i + 1),
                                    DisplayOrder = i,
                                });
                            }
                            var question = new QuestionData
                            {
                                QuestionText = worksheet.Cells[row, 2].Text,
                                TypeQuestion = "quiz",
                                TypeAnswer = correctAnswerIndices.Count > 1 ? 2 : 1,
                                PointsMultiplier = 1,
                                Choices = choices,
                                Image = "",
                                ImageEffect = "",
                                Time = timeLimit,
                            };
                            data.Add(question);
                        }
                    }
                }
                return Json(new { status = true, data = JsonConvert.SerializeObject(data) });
            }
            catch (Exception ex) {
                return Json(new { status = false });
            }
        }
        public async Task<IActionResult> GenerateQuesion(string language,string topic)
        {
            var contentask = $"Tạo cho tôi 5 câu hỏi \"mới\" gồm quiz(Câu đố 4 đáp án - 1 đáp án đúng) hoặc true_false(Đúng hoặc sai 2 đáp án - 1 đáp án đúng) về chủ đề \"{topic}\" với language \"{language}\" không cần giải thích và dẫn dắt trả về đúng định dạng: [{{\"QuestionText\":\"Đà Lạt ở nước nào?\",\"TypeQuestion\":\"quiz\",\"TypeAnswer\": 1(default),\"Time\": 20(default),\"PointsMultiplier\": 1(default),\"Image\": \"\",\"ImageEffect\": \"\",\"Choices\":[{{\"Answer\":\"Việt Nam\",\"IsCorrect\":true,\"DisplayOrder\":0}},{{}}...]}}]";
            var request = new ChatRequest
            {
                Model = "gemini-1.5-pro",
                MaxTokens = 2048,
                Messages = new[]
                {
                    new Message
                    {
                        Role = "user",
                        Content = contentask
                    }
                }
            };

            var responseContent = await _geminiAiService.GenerateResponse(request);
            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

            string extractedText = geminiResponse.Candidates[0].Content.Parts[0].Text;
            return Json(new { status = true, data = extractedText });
        }
        public async Task<IActionResult> ImportQuestion([FromBody] ImportData data)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var checkEduQuiz = await _context.EduQuizs.Include(e => e.Questions).FirstOrDefaultAsync(n=>n.Uuid == data.QuizId);
            if (checkEduQuiz == null) {
                var newEduQuiz = new Models.EF.EduQuiz
                {
                    Title = "",
                    Uuid = data.QuizId,
                    ImageCover = "/src/img/EduQuizDefault.png",
                    Description = "",
                    Type = 0,
                    Visibility = true,
                    ThemeId = 1,
                    MusicId = 1,
                    CreatedAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    UserId = iduser,
                    Questions = new List<Question>()
                };

                foreach (var question in data.Data)
                {
                    var newQuestion = new Question
                    {
                        QuestionText = question.QuestionText,
                        TypeQuestion = question.TypeQuestion,
                        TypeAnswer = question.TypeAnswer,
                        Time = question.Time,
                        PointsMultiplier = question.PointsMultiplier,
                        Image = question.Image,
                        ImageEffect = question.ImageEffect,
                        Choices = new List<Choice>()
                    };

                    if (question.Choices?.Count > 0)
                    {
                        var newChoices = question.Choices.Select(choice => new Choice
                        {
                            Question = newQuestion, 
                            Answer = choice.Answer,
                            IsCorrect = choice.IsCorrect,
                            DisplayOrder = choice.DisplayOrder
                        }).ToList();

                        newQuestion.Choices = newChoices; 
                    }

                    newEduQuiz.Questions.Add(newQuestion);
                }
                _context.EduQuizs.Add(newEduQuiz);
                await _context.SaveChangesAsync();

                var questionIds = newEduQuiz.Questions.Select(q => q.Id).ToList();
                newEduQuiz.OrderQuestion = JsonConvert.SerializeObject(questionIds);
                await _context.SaveChangesAsync();
            }
            else
            {
                foreach (var question in data.Data)
                {
                    var newQuestion = new Question
                    {
                        EduQuizId= checkEduQuiz.Id,
                        QuestionText = question.QuestionText,
                        TypeQuestion = question.TypeQuestion,
                        TypeAnswer = question.TypeAnswer,
                        Time = question.Time,
                        PointsMultiplier = question.PointsMultiplier,
                        Image = question.Image,
                        ImageEffect = question.ImageEffect,
                        Choices = new List<Choice>()
                    };

                    if (question.Choices?.Count > 0)
                    {
                        var newChoices = question.Choices.Select(choice => new Choice
                        {
                            Question = newQuestion,
                            Answer = choice.Answer,
                            IsCorrect = choice.IsCorrect,
                            DisplayOrder = choice.DisplayOrder
                        }).ToList();

                        newQuestion.Choices = newChoices;
                    }

                    checkEduQuiz.Questions.Add(newQuestion);
                }
                await _context.SaveChangesAsync();

                var existingQuestionIds = JsonConvert.DeserializeObject<List<int>>(checkEduQuiz.OrderQuestion) ?? new List<int>();
                var newQuestionIds = checkEduQuiz.Questions.Select(q => q.Id).ToList();
                existingQuestionIds.AddRange(newQuestionIds.Except(existingQuestionIds));

                checkEduQuiz.OrderQuestion = JsonConvert.SerializeObject(existingQuestionIds);
                await _context.SaveChangesAsync();
            }
            return Json(new { status = true});
        }
        #endregion
    }
}
