using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Linq;

namespace EduQuiz.Hubs
{
    public class SoloGameHub : Hub
    {
        private readonly EduQuizDBContext _context;
        public SoloGameHub(EduQuizDBContext context)
        {
            _context = context;
        }
        private static Dictionary<string, List<QuestionSession>> QuestionsPlayer = new Dictionary<string, List<QuestionSession>>();
        private static Dictionary<string, QuizOption> OptionQuizzesPlayer = new Dictionary<string, QuizOption>();
        private static Dictionary<string, List<int>> AskedQuestionsPlayer = new Dictionary<string, List<int>>();

        private static Dictionary<string, QuestionSession> CurrentQuestions = new Dictionary<string, QuestionSession>();
        private static Dictionary<string, QuizOption> CurrentOptions = new Dictionary<string, QuizOption>();

        public async Task StartGame(int idplayer, int idQuizSession)
        {
            var quizSession = await _context.QuizSessions.FindAsync(idQuizSession);
            if (quizSession != null)
            {
                var check = await _context.EduQuizSnapshots
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(d => d.Id == quizSession.EduQuizSnapshotId);
                List<int> orderquestion = JsonConvert.DeserializeObject<List<int>>(check.OrderQuestion);

                var getdata = new Models.EduQuizSession
                {
                    Uuid = check.Uuid,
                    Description = check.Description,
                    Title = check.Title,
                    ImageCover = check.ImageCover,
                    Type = check.Type,
                    Visibility = check.Visibility,
                    ThemeId = check.ThemeId,
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
                AskedQuestionsPlayer[idplayer.ToString()] = new List<int>();
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
                QuestionsPlayer[idplayer.ToString()] = getdata.Questions;
                OptionQuizzesPlayer[idplayer.ToString()] = quizOption;

                // random question
                if (quizSession.IsRandomQuestion)
                {
                    // Lấy ngẫu nhiên 1 câu hỏi và gửi đi
                    var randomQuestion = getdata.Questions.OrderBy(x => Guid.NewGuid()).First();
                    AskedQuestionsPlayer[idplayer.ToString()].Add(randomQuestion.Id);
                    var orderQuestionPlayer = new PlayerQuizSessionQuestion {
                        PlayerSessionId = idplayer,
                        ListQuestionId = JsonConvert.SerializeObject(AskedQuestionsPlayer[idplayer.ToString()]),
                    };
                    _context.PlayerQuizSessionQuestions.Add(orderQuestionPlayer);
                    await _context.SaveChangesAsync();
                    await SendQuestionWithCountdown(idplayer, randomQuestion, quizOption);
                    
                }
                else
                {
                    // Lấy câu hỏi đầu tiên theo thứ tự đã sắp xếp
                    var firstQuestion = getdata.Questions.First();
                    AskedQuestionsPlayer[idplayer.ToString()].Add(firstQuestion.Id);
                    var orderQuestionPlayer = new PlayerQuizSessionQuestion
                    {
                        PlayerSessionId = idplayer,
                        ListQuestionId = JsonConvert.SerializeObject(AskedQuestionsPlayer[idplayer.ToString()]),
                    };
                    _context.PlayerQuizSessionQuestions.Add(orderQuestionPlayer);
                    await _context.SaveChangesAsync();
                    await SendQuestionWithCountdown(idplayer, firstQuestion, quizOption);
                    
                }
            }
        }
      
        // Hàm gửi 1 câu hỏi kèm đếm ngược
        private async Task SendQuestionWithCountdown(int idplayer, QuestionSession question, QuizOption quizOption)
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
  
            CurrentQuestions[idplayer.ToString()] = question;
            CurrentOptions[idplayer.ToString()] = quizOption;

            await Clients.Caller.SendAsync("SendQuestion", quizOption, questionNotChoiceIscorrect); // Gửi câu hỏi cho các client
            List<int> listaskedquestion = AskedQuestionsPlayer[idplayer.ToString()];
            // Chờ 13 giây để client hoàn thành render và hoạt ảnh
            await Task.Delay(listaskedquestion.Count == 1 ? 13000 : 5500);

            // Gửi tín hiệu để bắt đầu tính thời gian
            await Clients.Caller.SendAsync("StartCountdown", true);

        }
    
        private async Task SendTimeUp(int idplayer, QuestionSession question, QuizOption quizOption)
        {
            var choiceCounts = await _context.ChoiceSnapshots
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
                .Where(c => c.QuestionId == question.Id && c.PlayerSession.QuizSessionId == quizOption.QuizSessionId && c.PlayerSessionId == idplayer).Select(n => new
                {
                    PlayerId = n.PlayerSessionId,
                    Score = n.Points,
                    IsCorrect = n.IsCorrect,
                })
                .ToListAsync();

            // Gửi kết quả cho người chơi
            await Clients.Caller.SendAsync("TimeUp", choiceCounts, getlistAnswer);
        }
        public async Task TimeUpQuestion(int playerId)
        {
            await SendTimeUp(playerId, CurrentQuestions[playerId.ToString()], CurrentOptions[playerId.ToString()]);
        }
        public async Task ScoreBoard(int idplayer, int idquizsession)
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

            await Clients.Caller.SendAsync("ScoreBoardList", getlistplayer);
        }
        public async Task ContinueGame(int idplayer, int idquizsession)
        {
            var quizSession = await _context.QuizSessions.FindAsync(idquizsession);
            var playerQuestionData = await _context.PlayerQuizSessionQuestions.FirstOrDefaultAsync(q => q.PlayerSessionId == idplayer);

            var check = await _context.EduQuizSnapshots
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(d => d.Id == quizSession.EduQuizSnapshotId);
            if (check == null || quizSession == null) return;

            List<int> orderquestion = JsonConvert.DeserializeObject<List<int>>(check.OrderQuestion);
            var getdata = new EduQuizSession
            {
                Uuid = check.Uuid,
                Description = check.Description,
                Title = check.Title,
                ImageCover = check.ImageCover,
                Type = check.Type,
                Visibility = check.Visibility,
                ThemeId = check.ThemeId,
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
            AskedQuestionsPlayer[idplayer.ToString()] = playerQuestionData != null
                ? JsonConvert.DeserializeObject<List<int>>(playerQuestionData.ListQuestionId)
                : new List<int>();
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
            QuestionsPlayer[idplayer.ToString()] = getdata.Questions;
            OptionQuizzesPlayer[idplayer.ToString()] = quizOption;

            List<int> listIdQuestion = AskedQuestionsPlayer[idplayer.ToString()];
            var lastAskedQuestionId = listIdQuestion.LastOrDefault();

            var checkAnswerQuestion = await _context.PlayerAnswers.FirstOrDefaultAsync(pa => pa.PlayerSessionId == idplayer && pa.QuestionId == lastAskedQuestionId);
            if (checkAnswerQuestion == null) { // chưa có đáp án
                await SendQuestionWithCountdown(idplayer, getdata.Questions.FirstOrDefault(q => q.Id == lastAskedQuestionId), quizOption);
                return;
            }
            var availableQuestions = getdata.Questions.Where(q => !listIdQuestion.Contains(q.Id)).ToList();

            if (availableQuestions.Count == 0) return; 

            QuestionSession nextQuestion;
            if (quizOption.IsRandomQuestion)
            {
                nextQuestion = availableQuestions.OrderBy(x => Guid.NewGuid()).First();
            }
            else
            {
                nextQuestion = availableQuestions.First();
            }

            AskedQuestionsPlayer[idplayer.ToString()].Add(nextQuestion.Id);
            playerQuestionData.ListQuestionId = JsonConvert.SerializeObject(AskedQuestionsPlayer[idplayer.ToString()]);
            await _context.SaveChangesAsync();

            await SendQuestionWithCountdown(idplayer, nextQuestion, quizOption);
        }
        public async Task NextQuestion(int idplayer, int idquizsession)
        {
            List<QuestionSession> getquestion = QuestionsPlayer[idplayer.ToString()];
            List<int> listidquestion = AskedQuestionsPlayer[idplayer.ToString()];
            QuizOption option = OptionQuizzesPlayer[idplayer.ToString()];

            var availableQuestions = getquestion
                .Where(q => !listidquestion.Contains(q.Id))
                .ToList();
            var getorderQuestionPlayer = await _context.PlayerQuizSessionQuestions.FirstOrDefaultAsync(q => q.PlayerSessionId == idplayer);

            if (availableQuestions.Count != 0)
            {
                // Trường hợp không auto nhưng random question
                if (option.IsRandomQuestion)
                {
                    // Lấy ngẫu nhiên 1 câu hỏi và gửi đi
                    var randomQuestion = availableQuestions.OrderBy(x => Guid.NewGuid()).First();
                    AskedQuestionsPlayer[idplayer.ToString()].Add(randomQuestion.Id);
                    getorderQuestionPlayer.ListQuestionId = JsonConvert.SerializeObject(AskedQuestionsPlayer[idplayer.ToString()]);
                    await _context.SaveChangesAsync();
                    await SendQuestionWithCountdown(idplayer, randomQuestion, option);
                }
                else
                {
                    // Lấy câu hỏi đầu tiên theo thứ tự đã sắp xếp
                    var firstQuestion = availableQuestions.First();
                    AskedQuestionsPlayer[idplayer.ToString()].Add(firstQuestion.Id);
                    getorderQuestionPlayer.ListQuestionId = JsonConvert.SerializeObject(AskedQuestionsPlayer[idplayer.ToString()]);
                    await _context.SaveChangesAsync();
                    await SendQuestionWithCountdown(idplayer, firstQuestion, option);
                }
            }
            else
            {
                OptionQuizzesPlayer.Remove(idplayer.ToString());
                QuestionsPlayer.Remove(idplayer.ToString());
                AskedQuestionsPlayer.Remove(idplayer.ToString());
                CurrentQuestions.Remove(idplayer.ToString());
                CurrentOptions.Remove(idplayer.ToString());
            }
        }
        public async Task SubmitAnswer(int playerId, int choiceId, int questionId, double timeTaken)
        {
            var getquestion = await _context.QuestionSnapshots.Include(q => q.Choices).FirstOrDefaultAsync(d => d.Id == questionId);
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
            await SendTimeUp(playerId, CurrentQuestions[playerId.ToString()], CurrentOptions[playerId.ToString()]);
        }
        private bool CheckAnswer(QuestionSnapshot question, int choiceId)
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
        private int CalculatePoints(QuestionSnapshot question, int choiceId, double timeTaken)
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
        public async Task SubmitMultiAnswer(int playerId, List<int> choiceIds, int questionId, double timeTaken)
        {

            var getquestion = await _context.QuestionSnapshots.Include(q => q.Choices).FirstOrDefaultAsync(d => d.Id == questionId);
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
            await SendTimeUp(playerId, CurrentQuestions[playerId.ToString()], CurrentOptions[playerId.ToString()]);
        }
        public async Task SubmitInputAnswer(int playerId, string answerText, int questionId,double timeTaken)
        {
            var getquestion = await _context.QuestionSnapshots.Include(q => q.Choices).FirstOrDefaultAsync(d => d.Id == questionId);
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
            await SendTimeUp(playerId, CurrentQuestions[playerId.ToString()], CurrentOptions[playerId.ToString()]);
        }
    }
}
