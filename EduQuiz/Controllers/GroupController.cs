using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using EduQuiz.Helper;
using System.Linq;
using EduQuiz.Services;
using Azure.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.FileIO;
namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class GroupController : Controller
    {
        private readonly EduQuizDBContext _context; 
        private readonly IEmailService _emailService; 
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GroupController(EduQuizDBContext context, IEmailService emailService, IHttpContextAccessor httpContextAccessor) {
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("groups/joined")]
        public async Task<IActionResult> Index()
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
            var listGroupJoinbyuser = await (from gm in _context.GroupMembers.AsNoTracking()
                                             join g in _context.Groups.AsNoTracking() on gm.GroupId equals g.Id
                                      where gm.UserId == iduser && gm.UserId != g.UserId && g.Status
                                      select new GroupView
                                      {
                                          Uuid = g.Uuid,
                                          Name = g.Name,
                                      }).ToListAsync();  
            return View(listGroupJoinbyuser);
        }
        [Route("groups/owned")]
        public async Task<IActionResult> GroupOwned()
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

            var listGroupbyuser = await _context.Groups
                .Where(g => g.UserId == iduser && g.Status)
                .AsNoTracking()
                .ToListAsync();
            return View(listGroupbyuser);

        }
        [Route("groups/{id:guid}/activity")]
        public async Task<IActionResult> GroupActivity(Guid id)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var avatart = jwtToken.Claims.FirstOrDefault(c => c.Type == "Avatar")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var getGroup = await _context.Groups.Where(g => g.Uuid == id && g.Status).Include(g=>g.Members).FirstOrDefaultAsync();          
            if (getGroup == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var isHostOrMember = getGroup.UserId == iduser || getGroup.Members.Any(m => m.UserId == iduser);
            if (!isHostOrMember)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var getlistpost = await (from p in _context.GroupPosts 
                                     join u in _context.Users on p.UserId equals u.Id
                                     join pl in _context.GroupPostLikes
                                     on new { p.Id, UserId = (int?)iduser } equals new { Id = pl.GroupPostId, pl.UserId } into postLikes
                                     from pl in postLikes.DefaultIfEmpty()
                                     where p.GroupId == getGroup.Id
                                     orderby p.PostedDate descending
                                     select new PostData{
                                         PostId=p.Id,
                                         UserName = u.Username,
                                         UserCreate = p.UserId,
                                         Content =p.Content,
                                         CurrentUser = iduser,
                                         Image = p.Image,
                                         PostedDate=StringHelper.ConvertDateTimeToCustomString(p.PostedDate),
                                         IsLiked = pl != null,
                                         SumLike = _context.GroupPostLikes.Count(l => l.GroupPostId == p.Id)
                                     }).ToListAsync();
            var view = new GroupViewModel
            {
                Group = getGroup,
                Avatar = avatart,
                IsHost = getGroup.UserId == iduser,
                ListPost = getlistpost
            };
            return View(view);
        }
        public IActionResult _PartialPosts(int groupid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Login", "Accout");
               
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var getlistpost = (from p in _context.GroupPosts
                                    join u in _context.Users on p.UserId equals u.Id
                                    join pl in _context.GroupPostLikes
                                    on new { p.Id, UserId = (int?)iduser } equals new { Id = pl.GroupPostId, pl.UserId } into postLikes
                                    from pl in postLikes.DefaultIfEmpty()
                                    where p.GroupId == groupid
                                   orderby p.PostedDate descending
                                   select new PostData
                                            {
                                                PostId = p.Id,
                                                UserName = u.Username,
                                                Content = p.Content,
                                                CurrentUser = iduser,  
                                                UserCreate = p.UserId,
                                                PostedDate = StringHelper.ConvertDateTimeToCustomString(p.PostedDate),
                                                Image = p.Image,
                                                IsLiked = pl != null,
                                                SumLike= _context.GroupPostLikes.Count(l => l.GroupPostId == p.Id)
                                        }).ToList();
            return PartialView("_PartialPosts", getlistpost);
        }
        [Route("groups/{id:guid}/shared")]
        public async Task< IActionResult> GroupShared(Guid id)
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
            var getGroup = await _context.Groups.Where(g => g.Uuid == id && g.Status).Include(g => g.Members).FirstOrDefaultAsync();
            if (getGroup == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var isHostOrMember = getGroup.UserId == iduser || getGroup.Members.Any(m => m.UserId == iduser);
            if (!isHostOrMember)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var listEduQuizshared = await (from sg in _context.ShareGroups
                                           join eq in _context.EduQuizs on sg.EduQuizId equals eq.Id
                                           join u in _context.Users on eq.UserId equals u.Id
                                           where sg.GroupId == getGroup.Id
                                           select new EduQuizShared
                                           {
                                               Id = eq.Id,
                                               Title = eq.Title,
                                               Uuid = eq.Uuid,
                                               Description = eq.Description,
                                               Image = eq.ImageCover,
                                               Avatar = u.ProfilePicture,
                                               Username = u.Username,
                                               CountQuestion = _context.Questions.Count(q => q.EduQuizId == eq.Id)
                                           }).ToListAsync();
            var view = new GroupViewModel
            {
                Group = getGroup,
                IsHost = getGroup.UserId == iduser,
                ListEduQuizShared = listEduQuizshared,
            };
            return View(view);
        }
        [Route("groups/{id:guid}/settings")]
        public async Task<IActionResult> GroupSetting(Guid id)
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
            var getGroup = await _context.Groups.Where(g => g.Uuid == id && g.Status).Include(g => g.Members).FirstOrDefaultAsync();
            if (getGroup == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var isHostOrMember = getGroup.UserId == iduser || getGroup.Members.Any(m => m.UserId == iduser);
            if (!isHostOrMember)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var view = new GroupViewModel
            {
                Group = getGroup,
                IsHost = getGroup.UserId == iduser
            };
            return View(view);
        }
        [Route("groups/{id:guid}/assignments")]
        public async Task<IActionResult> GroupAssignments(Guid id)
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
            var getGroup = await _context.Groups.Where(g => g.Uuid == id && g.Status).Include(g => g.Members).FirstOrDefaultAsync();
            if (getGroup == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var isHostOrMember = getGroup.UserId == iduser || getGroup.Members.Any(m => m.UserId == iduser);
            if (!isHostOrMember)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var data = await (from ag in _context.AssignmentGroups join
                             quiz in _context.EduQuizSnapshots on ag.EduQuizSnapshotId equals quiz.Id join
                             quizss in _context.QuizSessions on ag.QuizSessionId equals quizss.Id join
                             u in _context.Users on ag.UserId equals u.Id
                             where ag.GroupId == getGroup.Id
                             select new EduQuizAssignment
                             {
                                 Id = quiz.Id,
                                 Title = quiz.Title,
                                 Uuid = quiz.Uuid,
                                 Pin = quizss.Pin,
                                 Description = quiz.Description,
                                 Image = quiz.ImageCover,
                                 Avatar = u.ProfilePicture,
                                 Username = u.Username,
                                 StartTime = StringHelper.ConvertDateTimeToCustomString(quizss.StartTime ?? DateTime.Now),
                                 EndTime = quizss.EndTime ?? DateTime.Now ,
                                 CountQuestion = _context.Questions.Count(q => q.EduQuizId == quiz.Id)
                             }).ToListAsync();
            var view = new GroupViewModel
            {
                Group = getGroup,
                IsHost = getGroup.UserId == iduser,
                ListEduQuiAssignment = data
            };
            return View(view);
        }
        [Route("groups/{id:guid}/members")]
        public async Task<IActionResult> GroupMember(Guid id)
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
            var getGroup = await _context.Groups.Where(g => g.Uuid == id && g.Status).Include(g => g.Members).ThenInclude(m => m.User).FirstOrDefaultAsync();
            if (getGroup == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var isHostOrMember = getGroup.Members.Any(m => m.UserId == iduser);
            if (!isHostOrMember)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var members = getGroup.Members.Select(m => new MemberData
            {
                UserId = m.UserId,
                Name = m.User.Username,
                Avatar = m.User.ProfilePicture,
                Role = m.UserId == getGroup.UserId ? "Admin" :"Thành viên",
                JoinDate = StringHelper.ConvertDateTimeToCustomString(m.JoinedDate)
            }).ToList();

            var view = new GroupViewModel
            {
                Group = getGroup,
                IsHost = getGroup.UserId == iduser,
                ListMember = members
            };
            return View(view);
        }
        [AllowAnonymous]
        [Route("groups/{id:guid}/join")]
        public async Task<IActionResult> GroupJoin(Guid id, Guid inviteCode)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                ViewBag.HasAccount = false;
                ViewBag.Data = new GroupMemberView
                {
                    GroupId = 0,
                    UserId = 0
                };
                return View();
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var getGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Uuid == id && g.InviteCode == inviteCode);
            if (getGroup == null) {
                return RedirectToAction("Error404","Home");
            }
            ViewBag.HasAccount = true;
            ViewBag.Data = new GroupMemberView
            {
                GroupId= getGroup.Id,
                UserId = iduser
            };
            return View();
        }
        [Route("challenge/{pin}")]
        public async Task<IActionResult> Challenge(string pin)
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
            var getquizsession = await (from q in _context.QuizSessions join
                                        ga in _context.AssignmentGroups on q.Id equals ga.QuizSessionId join
                                       e in _context.EduQuizSnapshots on q.EduQuizSnapshotId equals e.Id join
                                       u in _context.Users on e.UserId equals u.Id
                                        where q.Pin == pin
                                        select new ChallengeModel
                                        {
                                            QuizSessionId = q.Id,
                                            GroupId = ga.GroupId ?? 0,
                                            Title = e.Title,
                                            EndTime = q.EndTime ?? DateTime.Now,
                                            ImageCover = e.ImageCover,
                                            QuestionCount = _context.Questions.Count(t => t.EduQuizId == q.EduQuizId),
                                            UserCreate = u.Username
                                        }).AsNoTracking().FirstOrDefaultAsync();
            if (getquizsession == null)
            {
                return RedirectToAction("Error404", "Home");
            }
            var checkuser = await _context.GroupMembers.FirstOrDefaultAsync(c => c.UserId == iduser && c.GroupId == getquizsession.GroupId);
            if (checkuser == null) {
                var referer = Request.Headers["Referer"].ToString();

                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var listPlayer = await _context.PlayerSessions
                .Where(p => p.QuizSessionId == getquizsession.QuizSessionId)
                .Select(n => new PlayerChallenge
                {
                    PlayerId = n.Id,
                    Name = n.Nickname,
                    UserId = Convert.ToInt32(n.ConnectionId)
                }).ToListAsync();
            var player = listPlayer.FirstOrDefault(u => u.UserId == iduser);
            var countChallengeQuestion = 0;
            if (player != null)
            {
                var getorderQuestionPlayer = await _context.PlayerQuizSessionQuestions.FirstOrDefaultAsync(p=>p.PlayerSessionId == player.PlayerId);
                var listIdQuestion = JsonConvert.DeserializeObject<List<int>>(getorderQuestionPlayer.ListQuestionId);
                var lastAskedQuestionId = listIdQuestion.LastOrDefault();
                var checkAnswerQuestion = await _context.PlayerAnswers.FirstOrDefaultAsync(pa => pa.PlayerSessionId == player.PlayerId && pa.QuestionId == lastAskedQuestionId);
                countChallengeQuestion = checkAnswerQuestion == null? listIdQuestion.Count : listIdQuestion.Count+1;
            }
            var view = new ChallengeViewModel
            {
                Challenge = getquizsession,
                ListPlayer = listPlayer,
                IsPlayer = player !=null,
                PlayerId = player != null ? player.PlayerId : 0,
                QuestionChallengeCount = countChallengeQuestion
            };
            return View(view);

        }
        #region handle
        public async Task<IActionResult> AddMemberGroup(int groupid,int userid)
        {
            var getGroup = await _context.Groups.FindAsync(groupid);
            if (getGroup == null)
            {
                return Json(new { status = false});
            }
            if (getGroup.UserId == userid)
            {
                return Json(new { status = true });
            }
            var isMember = await _context.GroupMembers.AnyAsync(m => m.GroupId == getGroup.Id && m.UserId == userid);
            if (isMember)
            {
                return Json(new { status = true });
            }
            var newMember = new GroupMember
            {
                UserId = userid,
                GroupId = getGroup.Id,
            };

            _context.GroupMembers.Add(newMember);
            await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        public async Task <IActionResult> CreateGroup(string name)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new {result = false});
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var newGroup = new Models.EF.Group
            {
                Name = name,
                UserId = iduser,
                InviteCode = Guid.NewGuid(),
            };
            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();
            var newMember = new GroupMember
            {
                UserId = iduser,
                GroupId = newGroup.Id,
            };
            _context.GroupMembers.Add(newMember);
            var result = await _context.SaveChangesAsync();
            return Json(new { status = result,data = newGroup.Uuid });
        }
        public async Task<IActionResult> EditGroup(int groupid,string name,string description)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var getGroup = await _context.Groups.FindAsync(groupid);
            if (getGroup == null || getGroup.UserId != iduser || name =="")
            {
                return Json(new { result = false});
            }
            getGroup.Name = name;
            getGroup.Description = description;
            var result = await _context.SaveChangesAsync();
            return Json(new { status = result });
        }
        public async Task<IActionResult> EditRoleGroup(int groupid,int role,bool value)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var getGroup = await _context.Groups.FindAsync(groupid);
            if (getGroup == null || getGroup.UserId != iduser)
            {
                return Json(new { result = false });
            }
            switch (role)
            {
                case 1:
                    getGroup.CanInviteNewMembers = value;
                    break;
                case 2:
                    getGroup.CanSeeMemberList = value;
                    break;
                case 3:
                    getGroup.CanShareContent = value;
                    break;
                default:
                    getGroup.CanPostContent = value;
                    break;
            }
            var result = await _context.SaveChangesAsync();
            return Json(new { status = result });
        }
        public async Task<IActionResult> ShareEduQuizGroup(int groupid, List<int> listeduquizid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int iduser))
            {
                return Json(new { result = false });
            }

            // Fetch group and check if it exists
            var getGroup = await _context.Groups.FindAsync(groupid);
            if (getGroup == null)
            {
                return Json(new { result = false });
            }

            var listshare = await (from gs in _context.ShareGroups
                                   where gs.GroupId == groupid
                                   select new
                                   {
                                       UserId = gs.UserId,
                                       EduQuizId = gs.EduQuizId
                                   }).ToListAsync();
            var listmember = await (from gm in _context.GroupMembers
                                    join user in _context.Users on gm.UserId equals user.Id
                                    where gm.GroupId == groupid
                                    select new
                                    {
                                        UserName = user.Username,
                                        UserEmail = user.Email,
                                        Privacy = user.PrivacySettings,
                                        UserId = user.Id,
                                    }).ToListAsync();

            var hostName = listmember.FirstOrDefault(n => n.UserId == getGroup.Id)?.UserName;

            var filteredList = listmember.Where(n => n.UserId != getGroup.UserId).ToList();

            var eduquizzes = await _context.EduQuizs
                .Where(e => listeduquizid.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.Title);

            var newShares = new List<ShareGroup>();

            foreach (var eduQuizId in listeduquizid)
            {
                eduquizzes.TryGetValue(eduQuizId, out var eduQuizTitle);
                if (eduQuizTitle == null) continue; 

                var alreadyShared = listshare.Any(g => g.EduQuizId == eduQuizId);

                if (!alreadyShared)
                {
                    foreach (var user in filteredList)
                    {
                        dynamic privacySettings = JsonConvert.DeserializeObject<dynamic>(user.Privacy);
                        if (privacySettings["ShareEduQuizWithMe"] == true)
                        {
                             SendEmail(
                                user.UserEmail,
                                "Một EduQuiz đã được chia sẻ với bạn",
                                user.UserName,
                                $"{hostName} vừa chia sẻ EduQuiz của họ '{eduQuizTitle}' với bạn",
                                "nhóm "+getGroup.Name,
                                "Nội dung được chia sẻ",
                                $"/groups/{getGroup.Uuid}/shared"
                            );
                        }
                    }
                    newShares.Add(new ShareGroup
                    {
                        GroupId = groupid,
                        UserId = iduser,
                        EduQuizId = eduQuizId
                    });
                }
            }
            if (newShares.Count > 0)
            {
                _context.ShareGroups.AddRange(newShares);
                var result = await _context.SaveChangesAsync();
                return Json(new { status = result });
            }

            return Json(new { status = "Lỗi" });
        }
        public async Task<IActionResult> SaveAssignment(int groupid, List<int> listeduquizid,DateTime startdate, DateTime enddate,bool isRandomQuestion, bool isRandomAnswer)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");
            string pin = await GeneratePin();
            var eduQuiz = await _context.EduQuizs
                .Where(e => e.Id == listeduquizid[0])
                .Include(x=>x.Questions)
                .ThenInclude(x=>x.Choices)
                .FirstOrDefaultAsync();

            if (eduQuiz == null)
            {
                return Json(new { result = false });
            }
            var checkSnapshot = await _context.EduQuizSnapshots
               .SingleOrDefaultAsync(s => s.CreatedAt == eduQuiz.UpdateAt && s.EduQuizId == eduQuiz.Id);
            EduQuizSnapshot newSnapshot = null;
            if (checkSnapshot == null)
            {
                newSnapshot = new EduQuizSnapshot
                {
                    EduQuizId = eduQuiz.Id,
                    Title = eduQuiz.Title,
                    Uuid = Guid.NewGuid(),
                    ImageCover = eduQuiz.ImageCover,
                    Description = eduQuiz.Description,
                    Type = eduQuiz.Type,
                    Visibility = eduQuiz.Visibility,
                    ThemeId = eduQuiz.ThemeId,
                    MusicId = eduQuiz.MusicId,
                    OrderQuestion = eduQuiz.OrderQuestion,
                    CreatedAt = eduQuiz.UpdateAt,
                    UserId = iduser,
                    Questions = new List<QuestionSnapshot>()
                };
                // Sao chép các câu hỏi và lựa chọn
                foreach (var originalQuestion in eduQuiz.Questions)
                {
                    var newQuestion = new QuestionSnapshot
                    {
                        QuestionText = originalQuestion.QuestionText,
                        TypeQuestion = originalQuestion.TypeQuestion,
                        TypeAnswer = originalQuestion.TypeAnswer,
                        Time = originalQuestion.Time,
                        PointsMultiplier = originalQuestion.PointsMultiplier,
                        Image = originalQuestion.Image,
                        ImageEffect = originalQuestion.ImageEffect,
                        Choices = new List<ChoiceSnapshot>()
                    };

                    if (originalQuestion.Choices?.Count > 0)
                    {
                        var newChoices = originalQuestion.Choices.Select(choice => new ChoiceSnapshot
                        {
                            Question = newQuestion, // Gán trực tiếp vào Question mới
                            Answer = choice.Answer,
                            IsCorrect = choice.IsCorrect,
                            DisplayOrder = choice.DisplayOrder
                        }).ToList();

                        newQuestion.Choices = newChoices; // Gán danh sách Choices vào Question
                    }

                    newSnapshot.Questions.Add(newQuestion);
                }

                // Lưu EduQuiz mới vào cơ sở dữ liệu
                _context.EduQuizSnapshots.Add(newSnapshot);
                await _context.SaveChangesAsync();

                var newQuestionIds = newSnapshot.Questions.Select(q => q.Id).ToList();
                newSnapshot.OrderQuestion = JsonConvert.SerializeObject(newQuestionIds);
            }

            var newquizSession = new QuizSession
            {
                EduQuizSnapshotId= checkSnapshot == null? newSnapshot.Id: checkSnapshot.Id,
                EduQuizId = listeduquizid[0],
                HostUserId = iduser,
                Pin = pin,
                Title = eduQuiz.Title,
                StartTime = startdate,
                EndTime = enddate,
                IsActive = false,
                IsWaitingRoom = false,
                IsRandomQuestion = isRandomQuestion,
                IsRandomAnswer = isRandomAnswer,
                TypeQuizSession = 1
            };
            _context.QuizSessions.Add(newquizSession);
            await _context.SaveChangesAsync();
          
            var newassignment = new AssignmentGroup
            {
                GroupId = groupid,
                EduQuizSnapshotId = checkSnapshot == null ? newSnapshot.Id : checkSnapshot.Id,
                QuizSessionId= newquizSession.Id,
                UserId = iduser
            };
            _context.AssignmentGroups.Add(newassignment);
			List<int> orderquestion = checkSnapshot == null
		    ? JsonConvert.DeserializeObject<List<int>>(newSnapshot.OrderQuestion)
		    : JsonConvert.DeserializeObject<List<int>>(checkSnapshot.OrderQuestion);
			int i = 0;
            foreach (var order in orderquestion)
            {
                var questionOrder = new QuizSessionQuestion
                {
                    QuizSessionId = newquizSession.Id,
                    QuestionId = order,
                    Order = i + 1
                };
                i++;
                _context.QuizSessionQuestions.Add(questionOrder);
            }
            await _context.SaveChangesAsync();

            var listmember = await (from gm in _context.GroupMembers
                                    join user in _context.Users on gm.UserId equals user.Id
                                    where gm.GroupId == groupid
                                    select new
                                    {
                                        UserName = user.Username,
                                        UserEmail = user.Email,
                                        UserId = user.Id,
                                    }).ToListAsync();

            var getGroup = await _context.Groups.FindAsync(groupid);
            var hostName = listmember.FirstOrDefault(n => n.UserId == groupid)?.UserName;

            var filteredList = listmember.Where(n => n.UserId != getGroup.UserId).ToList();

            foreach (var user in filteredList)
            {
                SendEmail(
                    user.UserEmail,
                    "Một bài tập EduQuiz đã được giao với bạn",
                    user.UserName,
                    $"{hostName} vừa giao bài tập EduQuiz của họ '{eduQuiz.Title}' với bạn",
                    "nhóm " + getGroup.Name,
                    "Nội dung bài tập nhóm",
                    $"/groups/{getGroup.Uuid}/assignments"
                 );
            }
            return Json(new { status = true});
        }
        public async Task<string> GeneratePin()
        {
            Random random = new Random();
            string pin;
            bool pinExists;

            do
            {
                pin = random.Next(100000, 999999).ToString();  // Tạo mã PIN 6 chữ số
                pinExists = await _context.QuizSessions.AnyAsync(q => q.Pin == pin && q.IsActive);
            } while (pinExists);  // Lặp lại cho đến khi tìm được mã PIN không trùng

            return pin;  // Trả về mã PIN duy nhất
        }
        public async Task<IActionResult> SavePost(int groupid,string content,IFormFile image)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { status = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var getGroup = await _context.Groups.FindAsync(groupid);
            if (getGroup == null)
            {
                return Json(new { status = false });
            }
            string imagePath = "";
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src", "img", "posts");

                // Tạo thư mục nếu không tồn tại
                Directory.CreateDirectory(uploadsFolder);

                // Lấy phần đuôi file (file extension)
                var fileExtension = Path.GetExtension(image.FileName);
                var uniqueFileName = Guid.NewGuid() + fileExtension;
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
                imagePath = $"/src/img/posts/{uniqueFileName}";
            }
            
            var newPost = new GroupPost
            {
                GroupId = groupid,
                UserId = iduser,
                Content = content,
                Image = imagePath
            };
            _context.GroupPosts.Add(newPost);
            var result = await _context.SaveChangesAsync();
            return Json(new { status = result });
        }
        public async Task<IActionResult> LikePost(int postid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var getPostLike = await _context.GroupPostLikes.FirstOrDefaultAsync(p => p.GroupPostId == postid && p.UserId == iduser);
            if(getPostLike != null)
            {
                _context.GroupPostLikes.Remove(getPostLike);
            }
            else
            {
                var newlike = new GroupPostLike
                {
                    GroupPostId = postid,
                    UserId = iduser,
                }; 
                _context.GroupPostLikes.Add(newlike);
            }
            var result = await _context.SaveChangesAsync();
            var likesCount = await _context.GroupPostLikes.AsNoTracking().CountAsync(l=>l.GroupPostId == postid);
            return Json(new { status = result,data = likesCount });
        }
        public async Task<IActionResult> RemovePost(int postid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var getPost = await _context.GroupPosts
                .Include(p => p.Likes) 
                .FirstOrDefaultAsync(p => p.Id == postid);
            if (getPost== null)
            {
                return Json(new { status = false});
            }
            _context.GroupPostLikes.RemoveRange(getPost.Likes);

            _context.GroupPosts.Remove(getPost);
            var result = await _context.SaveChangesAsync();
            return Json(new { status = true});
        }
        public async Task<IActionResult> OutGroup(int groupid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(p => p.GroupId == groupid && p.UserId == iduser);
            if (member == null)
            {
                return Json(new { status = false });
            }
            _context.GroupMembers.Remove(member);
            var result = await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        public async Task<IActionResult> RemoveMember(int idmember,int groupid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var checkadmin = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupid && g.UserId == iduser);
            if (checkadmin == null) {
                return Json(new { result = false });
            }
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(p => p.GroupId == groupid && p.UserId == idmember);
            if (member == null)
            {
                return Json(new { status = false });
            }
            _context.GroupMembers.Remove(member);
            var result = await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        public async Task<IActionResult> GetEduQuizByUser(int groupid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var avatart = jwtToken.Claims.FirstOrDefault(c => c.Type == "Avatar")?.Value;
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var listshare = await _context.ShareGroups
                .Where(g => g.GroupId == groupid && g.UserId == iduser)
                .Select(n =>n.EduQuizId)
                .ToListAsync();
            var listEduquiz = await _context.EduQuizs
                .Where(e => e.UserId == iduser && e.Type == 1)
                .Include(e=>e.Questions)
                .ToListAsync();
            var dataresponse = new List<object>();
            foreach (var item in listEduquiz) {
                dataresponse.Add(new {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Image = item.ImageCover,
                    Avatart = avatart,
                    IsSelected = listshare.Contains(item.Id),
                    Username = username,
                    CountQuestion = item.Questions.Count,
                });
            }
            return Json(new { status = true,data = dataresponse });
        }
        public async Task<IActionResult> JoinChallenge(int quizsessionid,string name)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { result = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var checkplayer = await _context.PlayerSessions
                .FirstOrDefaultAsync(p => p.QuizSessionId == quizsessionid && p.Nickname == name);
            if (checkplayer != null)
            {
                return Json(new { status = false });
            }
            var random = new Random();
            var avatarIndex = random.Next(1, 9);
            var accessoryIndex = random.Next(1, 10);

            var avatarUrl = $"/src/img/avatar/avatar{avatarIndex}.svg";
            var accessory = $"/src/img/accessory/accessory{accessoryIndex}.svg";

            var playerSession = new PlayerSession
            {
                Nickname = name,
                AvatarUrl = avatarUrl,
                Accessory = accessory,
                QuizSessionId = quizsessionid,
                JoinedAt = DateTime.Now,
                ConnectionId = iduser.ToString(),
                TotalScore = 0,
                Rank = 0,
                IsPlayer = true
            };
            _context.PlayerSessions.Add(playerSession);

            var result = await _context.SaveChangesAsync();
            return Json(new { status = true,data = playerSession });
        }
        private void SendEmail(string recipientEmail, string subject, string username,string msgtitle,string groupname,string type,string redirecturl)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/src/templates", "EmailTemplate.html");
            string emailBody;

            try
            {
                emailBody = System.IO.File.ReadAllText(templatePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi đọc file", ex);
            }
            var domain = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            emailBody = emailBody.Replace("{domain}", domain);
            emailBody = emailBody.Replace("{type}", type);
            emailBody = emailBody.Replace("{username}", username);
            emailBody = emailBody.Replace("{msgtitle}", msgtitle); // pingvocuc333 vừa chia sẻ EduQuiz của họ "Tăng cường vốn từ vựng"" với bạn!
            emailBody = emailBody.Replace("{groupname}", groupname);
            emailBody = emailBody.Replace("{redirecturl}", domain+redirecturl);

            // Gửi email sử dụng dịch vụ email
            _emailService.SendEmail(recipientEmail, subject, emailBody);
        }
        #endregion
    }
}
