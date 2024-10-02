using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EduQuiz.Hubs
{
    public class GameHub : Hub
    {
        private readonly EduQuizDBContext _context;
        public GameHub(EduQuizDBContext context) {
            _context = context;
        }
        private static Dictionary<string, List<PlayerData>> WaitingRoomPlayers = new Dictionary<string, List<PlayerData>>();
        private static Dictionary<string, string> HostConnections = new Dictionary<string, string>(); // Lưu trữ kết nối của host
        private static Dictionary<string, bool> RoomLockStatus = new Dictionary<string, bool>();
        private static Dictionary<string, QuizOption> OptionQuizzes = new Dictionary<string, QuizOption>();
        private static Dictionary<string, QuestionState> QuestionStates = new Dictionary<string, QuestionState>();
        private static Dictionary<string, List<QuestionSession>> QuestionsRoom = new Dictionary<string, List<QuestionSession>>();
        private static Dictionary<string, List<int>> AskedQuestions = new Dictionary<string, List<int>>(); // Thêm từ điển để lưu các câu hỏi đã hỏi
        public async Task CreateRoom(string pin)
        {
            // Kiểm tra xem phòng đã tồn tại chưa
            if (!WaitingRoomPlayers.ContainsKey(pin))
            {
                WaitingRoomPlayers[pin] = new List<PlayerData>();
                HostConnections[pin] = Context.ConnectionId;
                RoomLockStatus[pin] = false; 
                await Groups.AddToGroupAsync(Context.ConnectionId, pin);
            }
        }
        //Bắt đầu game
        public async Task StartGame(string pin,int idQuizSession)
        {
            // Kiểm tra xem phòng đã tồn tại chưa
            if (WaitingRoomPlayers.ContainsKey(pin))
            {
                var quizSession = await _context.QuizSessions.FindAsync(idQuizSession);
                if (quizSession != null) {
                    quizSession.IsWaitingRoom = false;

                    var playerSessions = await _context.PlayerSessions
                        .Where(ps => ps.QuizSessionId == idQuizSession)
                        .ToListAsync();
                    playerSessions.ForEach(ps => ps.IsPlayer = true);

                    await _context.SaveChangesAsync();

                    var check = await _context.EduQuizs
                        .Include(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                        .FirstOrDefaultAsync(d => d.Id == quizSession.EduQuizId);
                    List<int> orderquestion = JsonConvert.DeserializeObject<List<int>>(check.OrderQuestion);

                    var getdata = new Models.EduQuizSession
                    {
                        Uuid = check.Uuid,
                        Description = check.Description,
                        Title = check.Title,
                        ImageCover = check.ImageCover,
                        Type = check.Type,
                        Visibility = check.Visibility,
                        ThemeId = check.ThemeId ,
                        MusicId = check.MusicId,
                        UserId = check.UserId,
                        Questions = check.Questions.Select(q => new QuestionSession
                        {
                            Id = q.Id,
                            QuestionText = q.QuestionText,
                            TypeQuestion = q.TypeQuestion,
                            TypeAnswer = q.TypeAnswer,
                            Time = q.Time,
                            PointsMultiplier = q.PointsMultiplier,
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
                    // Khởi tạo danh sách câu hỏi đã hỏi
                    AskedQuestions[pin] = new List<int>();
                    int questionlength = getdata.Questions.Count;
                    var quizOption = new QuizOption
                    {
                        QuizSessionId = quizSession.Id,
                        IsRandomAnswer = quizSession.IsRandomAnswer,
                        IsShowAvatar = quizSession.IsShowAvatar,
                        QuizTitle = getdata.Title,
                        QuestionLength = questionlength,
                        IsShowQAndA = quizSession.IsShowQuestionAndAnswer,
                        IsAuto = quizSession.IsAuto,
                        IsRandomQuestion = quizSession.IsRandomQuestion,
                    };
                    QuestionsRoom[pin] = getdata.Questions;
                    OptionQuizzes[pin] = quizOption;


                    if (quizSession.IsAuto)
                    {
                        // Trường hợp auto và random question
                        if (quizSession.IsRandomQuestion)
                        {
                            // Gửi tất cả các câu hỏi theo thứ tự ngẫu nhiên
                            var randomQuestions = getdata.Questions.OrderBy(x => Guid.NewGuid()).ToList();
                            await SendQuestionsWithCountdown(pin, randomQuestions, quizOption);
                        }
                        else
                        {
                            // Gửi tất cả các câu hỏi theo thứ tự sắp xếp sẵn
                            await SendQuestionsWithCountdown(pin, getdata.Questions.ToList(), quizOption);
                        }
                    }
                    else
                    {
                        // Trường hợp không auto nhưng random question
                        if (quizSession.IsRandomQuestion)
                        {
                            // Lấy ngẫu nhiên 1 câu hỏi và gửi đi
                            var randomQuestion = getdata.Questions.OrderBy(x => Guid.NewGuid()).First();
                            AskedQuestions[pin].Add(randomQuestion.Id);
                            await SendQuestionWithCountdown(pin, randomQuestion, quizOption);
                        }
                        else
                        {
                            // Lấy câu hỏi đầu tiên theo thứ tự đã sắp xếp
                            var firstQuestion = getdata.Questions.First();
                            AskedQuestions[pin].Add(firstQuestion.Id);
                            await SendQuestionWithCountdown(pin, firstQuestion, quizOption);
                        }
                    }
                }
                
            }
        }
        // Hàm gửi lần lượt các câu hỏi kèm đếm ngược
        private async Task SendQuestionsWithCountdown(string pin, List<QuestionSession> questions, QuizOption quizOption)
        {
            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var questionNotChoiceIscorrect = new
                {
                    Id = question.Id,
                    QuestionText = question.QuestionText,
                    TypeQuestion = question.TypeQuestion,
                    TypeAnswer = question.TypeAnswer,
                    Time = question.Time,
                    PointsMultiplier = question.PointsMultiplier,
                    Image = question.Image,
                    ImageEffect = question.ImageEffect,
                    Choices = question.TypeQuestion != "input_answer" ? question.Choices.Select(c => new ChoiceSession
                    {
                        Id = c.Id,
                        Answer = c.Answer,
                        DisplayOrder = c.DisplayOrder
                    }).ToList() : []
                };
                int timeRemaining = questionNotChoiceIscorrect.Time ?? 30;
                var questionOrder = new QuizSessionQuestion
                {
                    QuizSessionId = quizOption.QuizSessionId,
                    QuestionId = question.Id,
                    Order = i + 1
                };
                _context.QuizSessionQuestions.Add(questionOrder);
                await _context.SaveChangesAsync();

                // Gửi câu hỏi cho các client
                await Clients.Group(pin).SendAsync("SendQuestion", quizOption, questionNotChoiceIscorrect);
                AskedQuestions[pin].Add(questionNotChoiceIscorrect.Id);
                // Chờ 13 giây để client hoàn thành render và hoạt ảnh
                await Task.Delay(i == 0 ? 13000 : 5500);

                // Cập nhật trạng thái câu hỏi hiện tại
                QuestionStates[pin] = new QuestionState
                {
                    CurrentQuestionIndex = i + 1,  // Bắt đầu từ chỉ số 1
                    QuestionId = questionNotChoiceIscorrect.Id,
                    CountDowntime = TimeSpan.FromSeconds(timeRemaining),
                    AnsweredCount = 0,
                    HasTimeUpBeenSent = false
                };
                // Gửi tín hiệu để bắt đầu tính thời gian
                await Clients.Group(pin).SendAsync("StartCountdown", true);

                // Vòng lặp đếm ngược và gửi cập nhật cho client
                while (timeRemaining > 0)
                {
                    // Chờ 1 giây
                    await Task.Delay(1000);

                    // Giảm thời gian còn lại
                    timeRemaining--;
                    var totalPlayersInRoom = WaitingRoomPlayers[pin].Count;
                    if (QuestionStates[pin].AnsweredCount >= totalPlayersInRoom)
                    {
                        break;
                    }
                    // Cập nhật thời gian đếm ngược trong QuestionStates
                    QuestionStates[pin].CountDowntime = TimeSpan.FromSeconds(timeRemaining);
                }
                // Chỉ gửi TimeUp nếu nó chưa được gửi từ trước
                if (!QuestionStates[pin].HasTimeUpBeenSent)
                {
                    await SendTimeUp(pin, question, quizOption);
                    QuestionStates[pin].CountDowntime = TimeSpan.FromSeconds(0);
                    QuestionStates[pin].HasTimeUpBeenSent = true;  // Đặt cờ đã gửi TimeUp
                }
                // Chờ 10 giây để client hoàn thành render và hoạt ảnh
                await Task.Delay(5000);
                var getlistplayer = await _context.PlayerSessions
                    .Where(p => p.IsPlayer == true && p.QuizSessionId == quizOption.QuizSessionId).Select(n => new
                    {
                        Id = n.Id,
                        Accessory = n.Accessory,
                        AvatarUrl = n.AvatarUrl,
                        Nickname = n.Nickname,
                        TotalScore = n.TotalScore
                    }).OrderByDescending(q => q.TotalScore).Take(5).ToListAsync();
                if (HostConnections.TryGetValue(pin, out var hostConnectionId)){ 
                    await Clients.Client(hostConnectionId).SendAsync("ScoreBoardList", getlistplayer);
                }
                await Task.Delay(5000);
            }
            var getlistscore = await _context.PlayerSessions
                   .Where(p => p.IsPlayer == true && p.QuizSessionId == quizOption.QuizSessionId).Select(n => new
                   {
                       Id = n.Id,
                       Accessory = n.Accessory,
                       AvatarUrl = n.AvatarUrl,
                       Nickname = n.Nickname,
                       TotalScore = n.TotalScore
                   }).OrderByDescending(q => q.TotalScore).ToListAsync();
            var updateQuizSession = await _context.QuizSessions.FindAsync(quizOption.QuizSessionId);
            updateQuizSession.EndTime = DateTime.Now;
            updateQuizSession.IsActive = false;
            await _context.SaveChangesAsync();
            WaitingRoomPlayers.Remove(pin);
            HostConnections.Remove(pin);
            RoomLockStatus.Remove(pin);
            OptionQuizzes.Remove(pin);
            QuestionStates.Remove(pin);
            QuestionsRoom.Remove(pin);
            AskedQuestions.Remove(pin);
            await Clients.Group(pin).SendAsync("FinishedQuiz", getlistscore);
        }

        // Hàm gửi 1 câu hỏi kèm đếm ngược
        private async Task SendQuestionWithCountdown(string pin, QuestionSession question, QuizOption quizOption)
        {
            int timeRemaining = question.Time ?? 30;  // Mặc định là 30 giây nếu không có thời gian
            var questionNotChoiceIscorrect = new
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                TypeQuestion = question.TypeQuestion,
                TypeAnswer = question.TypeAnswer,
                Time = question.Time,
                PointsMultiplier = question.PointsMultiplier,
                Image = question.Image,
                ImageEffect = question.ImageEffect,  
                Choices = question.TypeQuestion != "input_answer" ? question.Choices.Select(c => new ChoiceSession
                {
                    Id = c.Id,
                    Answer = c.Answer,
                    DisplayOrder = c.DisplayOrder
                }).ToList() : []
            };
            int currentQuestionIndex = QuestionStates.ContainsKey(pin) ? QuestionStates[pin].CurrentQuestionIndex + 1 : 1;
            var questionOrder = new QuizSessionQuestion
            {
                QuizSessionId = quizOption.QuizSessionId,
                QuestionId = question.Id,
                Order = currentQuestionIndex
            };
            _context.QuizSessionQuestions.Add(questionOrder);
            await _context.SaveChangesAsync();

            await Clients.Group(pin).SendAsync("SendQuestion", quizOption, questionNotChoiceIscorrect); // Gửi câu hỏi cho các client
            List<int> listaskedquestion = AskedQuestions[pin];
            
            // Chờ 13 giây để client hoàn thành render và hoạt ảnh
            await Task.Delay(listaskedquestion.Count == 1 ? 13000 : 5500);

            // Cập nhật trạng thái câu hỏi hiện tại
            if (!QuestionStates.ContainsKey(pin))
            {
                // Nếu chưa có trạng thái cho pin này, bắt đầu từ câu hỏi 1
                QuestionStates[pin] = new QuestionState
                {
                    CurrentQuestionIndex = 1,  // Bắt đầu từ câu hỏi 1
                    QuestionId = question.Id,
                    CountDowntime = TimeSpan.FromSeconds(timeRemaining),
                    AnsweredCount = 0,
                    HasTimeUpBeenSent = false
                };
            }
            else
            {
                // Nếu đã có trạng thái, tăng chỉ số câu hỏi
                QuestionStates[pin].CurrentQuestionIndex++;
                QuestionStates[pin].QuestionId = question.Id;
                QuestionStates[pin].CountDowntime = TimeSpan.FromSeconds(timeRemaining);
                QuestionStates[pin].AnsweredCount = 0; 
                QuestionStates[pin].HasTimeUpBeenSent = false;
            }
            
            // Gửi tín hiệu để bắt đầu tính thời gian
            await Clients.Group(pin).SendAsync("StartCountdown",true);

            // Vòng lặp đếm ngược và gửi cập nhật cho client
            while (timeRemaining > 0)
            {
                // Chờ 1 giây
                await Task.Delay(1000);

                // Giảm thời gian còn lại
                timeRemaining--;
                var totalPlayersInRoom = WaitingRoomPlayers[pin].Count;
                if (QuestionStates[pin].AnsweredCount >= totalPlayersInRoom)
                {
                    break;
                }
                // Cập nhật thời gian đếm ngược trong QuestionStates
                QuestionStates[pin].CountDowntime = TimeSpan.FromSeconds(timeRemaining);
            }
            // Chỉ gửi TimeUp nếu nó chưa được gửi từ trước
            if (!QuestionStates[pin].HasTimeUpBeenSent)
            {
                await SendTimeUp(pin, question, quizOption);
                QuestionStates[pin].CountDowntime = TimeSpan.FromSeconds(0);
                QuestionStates[pin].HasTimeUpBeenSent = true;  // Đặt cờ đã gửi TimeUp
            }
        }
        private async Task SendTimeUp(string pin, QuestionSession question, QuizOption quizOption)
        {
            var choiceCounts = await _context.Choices
                .Where(c => c.QuestionId == question.Id)
                .GroupJoin(
                    _context.PlayerAnswers.Where(pa => pa.PlayerSession.QuizSessionId == quizOption.QuizSessionId),
                    choice => choice.Id,
                    answer => answer.ChoiceId,
                    (choice, answers) => new
                    {
                        ChoiceId = choice.Id,
                        AnswerText = choice.Answer,
                        AnswerCorrect = choice.IsCorrect,
                        Count = answers.Count()
                    })
                .ToListAsync();
            var getlistAnswer = await _context.PlayerAnswers
                .Where(c => c.QuestionId == question.Id && c.PlayerSession.QuizSessionId == quizOption.QuizSessionId).Select(n => new
                {
                    PlayerId = n.PlayerSessionId,
                    Score = n.Points,
                    IsCorrect = n.IsCorrect,
                })
                .ToListAsync();
            if (HostConnections.TryGetValue(pin, out var hostConnectionId))
            {
                await Clients.Client(hostConnectionId).SendAsync("ReportQuesion", choiceCounts);
            }
            // Gửi kết quả cho người chơi
            await Clients.Group(pin).SendAsync("TimeUp", getlistAnswer);
        }
        public async Task SubmitAnswer(int playerId, int choiceId, int questionId, string pin,double timeTaken)
        {
            if (!WaitingRoomPlayers.ContainsKey(pin))
            {
                await Clients.Caller.SendAsync("RoomNotFound", true);
                return;
            }
            var getquestion = await _context.Questions.Include(q => q.Choices).FirstOrDefaultAsync(d => d.Id == questionId);
            var getPlayerSession = await _context.PlayerSessions.FindAsync(playerId);
            bool isCorrect = CheckAnswer(getquestion, choiceId);
            int point = CalculatePoints(getquestion, choiceId, timeTaken);
            if (isCorrect)
            {
                getPlayerSession.TotalScore += point;
            }
            var playerAnswer = new PlayerAnswer
            {
                PlayerSessionId = playerId,
                QuestionId = questionId,
                ChoiceId = choiceId,
                TimeTaken = timeTaken,
                IsCorrect = isCorrect,  
                Points = point  
            };

            // Lưu câu trả lời vào cơ sở dữ liệu
            _context.PlayerAnswers.Add(playerAnswer);
            await _context.SaveChangesAsync();
            QuestionStates[pin].AnsweredCount++;
            if (HostConnections.TryGetValue(pin, out var hostConnectionId))
            {
                await Clients.Client(hostConnectionId).SendAsync("UpdateCountAnswer", QuestionStates[pin].AnsweredCount);
            }
        }
        public async Task SubmitInputAnswer(int playerId, string answerText, int questionId, string pin, double timeTaken)
        {
            if (!WaitingRoomPlayers.ContainsKey(pin))
            {
                await Clients.Caller.SendAsync("RoomNotFound", true);
                return;
            }
            var getquestion = await _context.Questions.Include(q => q.Choices).FirstOrDefaultAsync(d => d.Id == questionId);
            var getPlayerSession = await _context.PlayerSessions.FindAsync(playerId);
            var findAnswer = getquestion.Choices.FirstOrDefault(a => a.Answer.ToLower() == answerText.ToLower());
            bool isCorrect = findAnswer != null;
            int point = CalculatePoints(getquestion, findAnswer?.Id ?? 0, timeTaken);
            if (isCorrect)
            {
                getPlayerSession.TotalScore += point;
                var playerAnswer = new PlayerAnswer
                {
                    PlayerSessionId = playerId,
                    QuestionId = questionId,
                    ChoiceId = findAnswer.Id,
                    TimeTaken = timeTaken,
                    IsCorrect = isCorrect,
                    Points = point,
                    AnswerText = answerText,
                };
                // Lưu câu trả lời vào cơ sở dữ liệu
                _context.PlayerAnswers.Add(playerAnswer);
            }
            else
            {
                var playerAnswer = new PlayerAnswer
                {
                    PlayerSessionId = playerId,
                    QuestionId = questionId,
                    TimeTaken = timeTaken,
                    IsCorrect = isCorrect,
                    Points = point,
                    AnswerText = answerText,
                };
                // Lưu câu trả lời vào cơ sở dữ liệu
                _context.PlayerAnswers.Add(playerAnswer);
            }
            await _context.SaveChangesAsync();
            QuestionStates[pin].AnsweredCount++;
            if (HostConnections.TryGetValue(pin, out var hostConnectionId))
            {
                await Clients.Client(hostConnectionId).SendAsync("UpdateCountAnswer", QuestionStates[pin].AnsweredCount);
            }
        }
        public async Task SubmitMultiAnswer(int playerId, List<int> choiceIds, int questionId, string pin, double timeTaken)
        {
            if (!WaitingRoomPlayers.ContainsKey(pin))
            {
                await Clients.Caller.SendAsync("RoomNotFound", true);
                return;
            }

            var getquestion = await _context.Questions.Include(q => q.Choices).FirstOrDefaultAsync(d => d.Id == questionId);
            var getPlayerSession = await _context.PlayerSessions.FindAsync(playerId);

            var correctChoices = getquestion.Choices.Where(c => c.IsCorrect).Count();

            // Kiểm tra nếu số lựa chọn vượt quá số đáp án đúng
            bool tooManyChoices = choiceIds.Count > correctChoices;
            foreach (var choiceId in choiceIds)
            {
                bool isCorrect = CheckAnswer(getquestion, choiceId);
                int point = tooManyChoices ? 0 : CalculatePoints(getquestion, choiceId, timeTaken);

                if (isCorrect)
                {
                    getPlayerSession.TotalScore += point;
                }

                var playerAnswer = new PlayerAnswer
                {
                    PlayerSessionId = playerId,
                    QuestionId = questionId,
                    ChoiceId = choiceId,
                    TimeTaken = timeTaken,
                    IsCorrect = isCorrect && !tooManyChoices,
                    Points = point
                };

                _context.PlayerAnswers.Add(playerAnswer);
            }
            await _context.SaveChangesAsync();

            QuestionStates[pin].AnsweredCount++;
            if (HostConnections.TryGetValue(pin, out var hostConnectionId))
            {
                await Clients.Client(hostConnectionId).SendAsync("UpdateCountAnswer", QuestionStates[pin].AnsweredCount);
            }
        }
        // Hàm kiểm tra đáp án
        private bool CheckAnswer(Question question, int choiceId)
        {
            return question.Choices.Any(c => c.Id == choiceId && c.IsCorrect);
        }
        private double GetPointsMultiplier(int pointsMultiplier)
        {
            switch (pointsMultiplier)
            {
                case 1: return 1.0; // Tiêu chuẩn
                case 2: return 2.0; // Nhân đôi
                case 3: return 0.0; // Không cho điểm
                default: return 1.0; // Mặc định là tiêu chuẩn nếu không rõ
            }
        }
        private int CalculatePoints(Question question, int choiceId, double timeTaken)
        {
            int maxPoints = 1000; // Điểm mặc định là 1000
            double totalTime = question.Time ?? 30; // Thời gian tối đa từ câu hỏi, mặc định là 30 nếu không có

            // Hệ số thời gian, đảm bảo không lớn hơn 1
            double timeFactor = timeTaken < totalTime ? (totalTime - timeTaken) / totalTime : 0;

            // Kiểm tra câu trả lời
            bool isCorrect = CheckAnswer(question, choiceId);

            // Nếu câu hỏi loại "input_answer"
            if (question.TypeQuestion == "input_answer")
            {
                return isCorrect ? (int)(maxPoints * timeFactor) : 0; // Điểm full nếu đúng, không có điểm nếu sai
            }
            int multiplier = question.PointsMultiplier ?? 1; // Sử dụng 1 nếu null
            // Xử lý theo từng loại đáp án
            switch (question.TypeAnswer)
            {
                case 1: // Chọn một
                    return isCorrect ? (int)(maxPoints * timeFactor * GetPointsMultiplier(multiplier)) : 0;

                case 2: // Chọn nhiều
                    var correctChoices = question.Choices.Where(c => c.IsCorrect).Count();
                    if (isCorrect)
                    {
                        return (int)(maxPoints * timeFactor * GetPointsMultiplier(multiplier)) / correctChoices; // Tính điểm cho đáp án đúng
                    }
                    else
                    {
                        return 0; // Không điểm cho lựa chọn sai
                    }

                default:
                    return 0; // Mặc định trả về 0 nếu không rơi vào trường hợp nào
            }
        }
        public async Task ScoreBoard(string pin, int idquizsession)
        {
            if (WaitingRoomPlayers.ContainsKey(pin))
            {
                await Clients.Caller.SendAsync("RoomExists", true); // Phòng tồn tại
            }
            var getlistplayer = await _context.PlayerSessions
                    .Where(p => p.IsPlayer == true && p.QuizSessionId == idquizsession).Select(n => new
                    {
                        Id = n.Id,
                        Accessory = n.Accessory,
                        AvatarUrl = n.AvatarUrl,
                        Nickname = n.Nickname,
                        TotalScore = n.TotalScore
                    }).OrderByDescending(q => q.TotalScore).Take(5).ToListAsync();
            if (HostConnections.TryGetValue(pin, out var hostConnectionId))
            {
                await Clients.Client(hostConnectionId).SendAsync("ScoreBoardList", getlistplayer);
            }
        }
        public async Task NextQuestion(string pin, int idquizsession)
        {
            if (WaitingRoomPlayers.ContainsKey(pin))
            {
                await Clients.Caller.SendAsync("RoomExists", true); // Phòng tồn tại
            }
            List<QuestionSession> getquestion = QuestionsRoom[pin];
            List<int> listidquestion = AskedQuestions[pin];
            QuizOption option = OptionQuizzes[pin];

            var availableQuestions = getquestion
                .Where(q => !listidquestion.Contains(q.Id)) 
                .ToList();
            if (availableQuestions.Count != 0) {
                // Trường hợp không auto nhưng random question
                if (option.IsRandomQuestion)
                {
                    // Lấy ngẫu nhiên 1 câu hỏi và gửi đi
                    var randomQuestion = availableQuestions.OrderBy(x => Guid.NewGuid()).First();
                    AskedQuestions[pin].Add(randomQuestion.Id);
                    await SendQuestionWithCountdown(pin, randomQuestion, option);
                }
                else
                {
                    // Lấy câu hỏi đầu tiên theo thứ tự đã sắp xếp
                    var firstQuestion = availableQuestions.First();
                    AskedQuestions[pin].Add(firstQuestion.Id);
                    await SendQuestionWithCountdown(pin, firstQuestion, option);
                }
            }
            else
            {
                var getlistplayer = await _context.PlayerSessions
                   .Where(p => p.IsPlayer == true && p.QuizSessionId == idquizsession).Select(n => new
                   {
                       Id = n.Id,
                       Accessory = n.Accessory,
                       AvatarUrl = n.AvatarUrl,
                       Nickname = n.Nickname,
                       TotalScore = n.TotalScore
                   }).OrderByDescending(q => q.TotalScore).ToListAsync();
                var updateQuizSession = await _context.QuizSessions.FindAsync(option.QuizSessionId);
                updateQuizSession.EndTime = DateTime.Now;
                updateQuizSession.IsActive = false;
                await _context.SaveChangesAsync();
                WaitingRoomPlayers.Remove(pin);
                HostConnections.Remove(pin);
                RoomLockStatus.Remove(pin);
                OptionQuizzes.Remove(pin);
                QuestionStates.Remove(pin);
                QuestionsRoom.Remove(pin);
                AskedQuestions.Remove(pin);
                await Clients.Group(pin).SendAsync("FinishedQuiz", getlistplayer);
            }
           
        }
        public async Task ReloadConnect(string pin)
        {
            if (!WaitingRoomPlayers.ContainsKey(pin))
            {
                await Clients.Caller.SendAsync("RoomNotFound", true);
                return;
            }
            QuestionState questionState = QuestionStates[pin];
            await Clients.Caller.SendAsync("ReloadQuestion",questionState.CountDowntime.TotalSeconds, questionState.CurrentQuestionIndex);

        }
        // Kiểm tra phòng tồn tại khi người dùng nhập mã PIN
        public async Task CheckRoomExists(string pin)
        {
            if (WaitingRoomPlayers.ContainsKey(pin))
            {
                await Clients.Caller.SendAsync("RoomExists", true); // Phòng tồn tại
            }
            else
            {
                await Clients.Caller.SendAsync("RoomExists", false); // Phòng không tồn tại
            }
        }
        // Thêm người chơi vào sảnh chờ
        public async Task AddPlayerToWaitingRoom(string pin, string nickname, int idquizsession)
        {
            try
            {
                if (!WaitingRoomPlayers.ContainsKey(pin))
                {
                    await Clients.Caller.SendAsync("RoomNotFound", true);
                    return;
                }
                bool isLocked = RoomLockStatus[pin];
                if (isLocked) {
                    await Clients.Caller.SendAsync("RoomLock", true);
                    return;
                }
                var nameExists = await _context.PlayerSessions
                    .FirstOrDefaultAsync(p =>p.QuizSessionId == idquizsession && p.Nickname == nickname);

                if (nameExists != null)
                {
                    await Clients.Caller.SendAsync("NameCheck", true);
                    return;
                }
                // Kiểm tra gói subscription của người tổ chức quiz
                var checkQuiz = await _context.QuizSessions
                    .Include(q => q.HostUser) 
                    .FirstOrDefaultAsync(q => q.Id == idquizsession);

                // Nếu là gói 'free', kiểm tra số lượng người chơi trong phòng
                if (checkQuiz?.HostUser.SubscriptionType == "free")
                {
                    // Giới hạn số lượng người chơi là 10 cho gói 'free'
                    var currentPlayerCount = WaitingRoomPlayers[pin].Count;
                    if (currentPlayerCount >= 10)
                    {
                        await Clients.Caller.SendAsync("RoomFull", true);
                        return;
                    }
                }

                var random = new Random();
                var avatarIndex = random.Next(1, 9);
                var accessoryIndex = random.Next(1, 10);

                var avatarUrl = $"/src/img/avatar/avatar{avatarIndex}.svg";
                var accessory = $"/src/img/accessory/accessory{accessoryIndex}.svg";

                var playerSession = new PlayerSession
                {
                    Nickname = nickname,
                    AvatarUrl = avatarUrl,
                    Accessory = accessory,
                    QuizSessionId = idquizsession,
                    JoinedAt = DateTime.Now,
                    ConnectionId = Context.ConnectionId,
                    TotalScore = 0,
                    Rank = 0,
                    IsPlayer = checkQuiz.IsWaitingRoom==true? false : true
                };

                _context.PlayerSessions.Add(playerSession);

                await _context.SaveChangesAsync();

                var getlistplayer =await _context.PlayerSessions
                    .Where(p =>p.QuizSessionId == idquizsession).Select(n=> new PlayerData
                    {
                        Id = n.Id,
                        Accessory  = n.Accessory,
                        AvatarUrl=n.AvatarUrl,
                        Nickname=n.Nickname,
                    }).ToListAsync();

                WaitingRoomPlayers[pin] = getlistplayer;

                await Clients.Caller.SendAsync("PlayerJoined", true, playerSession.ConnectionId);

                if (HostConnections.TryGetValue(pin, out var hostConnectionId))
                {
                    await Clients.Client(hostConnectionId).SendAsync("UpdatePlayerList", WaitingRoomPlayers[pin]);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task RemoveQuizSession(string pin, int idQuizSession)
        {
            try
            {
                var checkquiz= await _context.QuizSessions.FindAsync(idQuizSession);
                if (checkquiz != null && checkquiz.IsWaitingRoom)
                {
                    var getlistplayer = await _context.PlayerSessions
                        .Where(p => p.IsPlayer == false && p.QuizSessionId == idQuizSession).ToListAsync();
                    if (getlistplayer.Any())
                    {
                        _context.PlayerSessions.RemoveRange(getlistplayer);
                    }
                    _context.QuizSessions.Remove(checkquiz);
                }
                else
                {
                    checkquiz.EndTime = DateTime.Now;
                    checkquiz.IsActive = false;
                }
                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();
                WaitingRoomPlayers.Remove(pin);
                HostConnections.Remove(pin);


                // Gửi thông báo cho nhóm
                await Clients.Group(pin).SendAsync("RoomDelete", true);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task LockRoom(string pin,bool lockRoom)
        {
            try
            {
                if (WaitingRoomPlayers.ContainsKey(pin) && HostConnections[pin] == Context.ConnectionId)
                {
                    RoomLockStatus[pin] = lockRoom;
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task ChangeAvatar(string pin, int playerid,string urlItem,string urlavatar)
        {
            try
            {
                if (!WaitingRoomPlayers.ContainsKey(pin))
                {
                    await Clients.Caller.SendAsync("RoomNotFound", true);
                    return;
                }
                var getplayer = _context.PlayerSessions.Find(playerid);
                if (getplayer != null) {
                    getplayer.Accessory = urlItem;
                    getplayer.AvatarUrl = urlavatar;
                }
                await _context.SaveChangesAsync();
                if (HostConnections.TryGetValue(pin, out var hostConnectionId))
                {
                    await Clients.Client(hostConnectionId).SendAsync("UpdateAvartar", playerid, urlItem, urlavatar);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task ReConnectPlayer(string pin)
        {
            try
            {
                if (!WaitingRoomPlayers.ContainsKey(pin))
                {
                    await Clients.Caller.SendAsync("RoomNotFound", true);
                    return;
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, pin);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task ChangeAuto(string pin,int idquestion,bool isbool)
        {
            try
            {
                var getquestion = await _context.QuizSessions.FindAsync(idquestion);
                if (getquestion != null) { 
                    getquestion.IsAuto = isbool;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task ChangeShowQuestionAndAnswer(string pin, int idquestion, bool isbool)
        {
            try
            {
                var getquestion = await _context.QuizSessions.FindAsync(idquestion);
                if (getquestion != null)
                {
                    getquestion.IsShowQuestionAndAnswer = isbool;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task ChangeOptionAvatar(string pin, int idquestion, bool isbool)
        {
            try
            {
                var getquestion = await _context.QuizSessions.FindAsync(idquestion);
                if (getquestion != null)
                {
                    getquestion.IsShowAvatar = isbool;
                }
                await _context.SaveChangesAsync();
                await Clients.Group(pin).SendAsync("OptionAvatar", isbool);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task ChangeQuestion(string pin, int idquestion, bool isbool)
        {
            try
            {
                var getquestion = await _context.QuizSessions.FindAsync(idquestion);
                if (getquestion != null)
                {
                    getquestion.IsRandomQuestion = isbool;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task ChangeAnswer(string pin, int idquestion, bool isbool)
        {
            try
            {
                var getquestion = await _context.QuizSessions.FindAsync(idquestion);
                if (getquestion != null)
                {
                    getquestion.IsRandomAnswer = isbool;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        public async Task PlayerOutRoom(string pin, int playerid)
        {
            try
            {
                if (!WaitingRoomPlayers.ContainsKey(pin))
                {
                    await Clients.Caller.SendAsync("RoomNotFound", true);
                    return;
                }
                var checkplayer = await _context.PlayerSessions.FindAsync(playerid);
                int quizsession = checkplayer.QuizSessionId;
                bool flag = false;
                if (!checkplayer.IsPlayer)
                {
                    _context.PlayerSessions.Remove(checkplayer);
                    flag = true;
                }

                await _context.SaveChangesAsync();

                var getlistplayer =await _context.PlayerSessions
                    .Where(p =>p.QuizSessionId == quizsession).Select(n => new PlayerData
                    {
                        Id = n.Id,
                        Accessory = n.Accessory,
                        AvatarUrl = n.AvatarUrl,
                        Nickname = n.Nickname,
                    }).ToListAsync();

                WaitingRoomPlayers[pin] = getlistplayer;

                await Clients.Group(pin).SendAsync("PlayerOut",flag,playerid);

                if (HostConnections.TryGetValue(pin, out var hostConnectionId))
                {
                    await Clients.Client(hostConnectionId).SendAsync("UpdatePlayerList", WaitingRoomPlayers[pin]);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra trong quá trình tham gia.");
                Console.WriteLine($"Error adding player: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}
