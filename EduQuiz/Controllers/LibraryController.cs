using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static NuGet.Packaging.PackagingConstants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class LibraryController : Controller
    {
        private readonly EduQuizDBContext _context;
        public LibraryController(EduQuizDBContext context)
        {
            _context = context;
        }
        public IActionResult _PartialFolders()
        {
            var authCookie = Request.Cookies["acToken"];

            if (authCookie == null)
            {
                return RedirectToAction("Login", "Accout");
                
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int iduser = int.Parse(userId ?? "1");
            var model = _context.Folders
                .Where(f => f.UserId == iduser)
                .OrderBy(f => f.Name)
                .ToList();
            return PartialView("_PartialFolders", model);
        }
        [Route("my-library/eduquizs/all")]
        public async Task<IActionResult> Index()
        {
            var authCookie = Request.Cookies["acToken"];
            int iduser = 0;
            if (authCookie == null)
            {
                return RedirectToAction("Login", "Accout");
                
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            iduser = int.Parse(userId ?? "1");
            var user = await _context.Users.FindAsync(iduser);
            if (user == null)
            {
                return RedirectToAction("Login", "Accout");
            }
            await EnsureFolderExistsAsync(user.Id);
            var folders = _context.Folders
              .Where(f => f.UserId == iduser)
              .OrderBy(f => f.Name)
              .ToList();

            ViewBag.Folders = folders;
            var listEduQuizByUser = await _context.EduQuizs
                    .Where(d => d.UserId == user.Id && d.Type == 1 && d.Status == true)
                    .Include(e => e.Questions)
                    .OrderByDescending(d => d.UpdateAt)
                    .ToListAsync();
            ViewBag.Data = JsonConvert.SerializeObject(new { UserName = user.Username, Avatar = user.ProfilePicture });
            return View(listEduQuizByUser);
        }
        [Route("my-library/eduquizs/share")]
        public async Task<IActionResult> EduQuizShare()
        {
            var authCookie = Request.Cookies["acToken"];
            int iduser = 0;
            if (authCookie == null)
            {
                return RedirectToAction("Login", "Accout");

            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            iduser = int.Parse(userId ?? "1");
            var user = await _context.Users.FindAsync(iduser);
            if (user == null)
            {
                return RedirectToAction("Login", "Accout");
            }
            await EnsureFolderExistsAsync(user.Id);
            var folders = _context.Folders
              .Where(f => f.UserId == iduser)
              .OrderBy(f => f.Name)
              .ToList();

            ViewBag.Folders = folders;
            var listEduQuizShare = await (from s in _context.ShareGroups 
                                          join gm in _context.GroupMembers on s.GroupId equals gm.GroupId
                                          join e in _context.EduQuizs on s.EduQuizId equals e.Id
                                          join u in _context.Users on e.UserId equals u.Id
                                          where gm.UserId == iduser
                                          select new EduQuizView
                                          {
                                              Id = e.Id,
                                              Avatar = u.ProfilePicture,
                                              Image = e.ImageCover,
                                              SumQuestion = _context.Questions.Count(q=>q.EduQuizId == e.Id),
                                              Title = e.Title,
                                              Uuid= e.Uuid,
                                              UserName = u.Username
                                          }).ToListAsync();
            return View(listEduQuizShare);
        }
        [Route("my-library/eduquizs/favorite")]
        public async Task<IActionResult> EduQuizFavorite()
        {
            var authCookie = Request.Cookies["acToken"];
            int iduser = 0;
            if (authCookie == null)
            {
                return RedirectToAction("Login", "Accout");

            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            iduser = int.Parse(userId ?? "1");
            var user = await _context.Users.FindAsync(iduser);
            if (user == null)
            {
                return RedirectToAction("Login", "Accout");
            }
            await EnsureFolderExistsAsync(user.Id);
            var folders = _context.Folders
              .Where(f => f.UserId == iduser)
              .OrderBy(f => f.Name)
              .ToList();

            ViewBag.Folders = folders;
            var listEduQuizShare = await (from s in _context.EduQuizFavorite
                                          join e in _context.EduQuizs on s.EduQuizId equals e.Id
                                          join u in _context.Users on e.UserId equals u.Id
                                          where s.UserId == iduser
                                          select new EduQuizView
                                          {
                                              Id = e.Id,
                                              Avatar = u.ProfilePicture,
                                              Image = e.ImageCover,
                                              SumQuestion = _context.Questions.Count(q => q.EduQuizId == e.Id),
                                              Title = e.Title,
                                              Uuid = e.Uuid,
                                              UserName = u.Username
                                          }).ToListAsync();
            return View(listEduQuizShare);
        }
        [Route("my-library/eduquizs/drafts")]
        public async Task<IActionResult> EduQuizDraft()
        {
            var authCookie = Request.Cookies["acToken"];
            int iduser = 0;
            if (authCookie == null)
            {
                return RedirectToAction("Login", "Accout");
                
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            iduser = int.Parse(userId ?? "1");
            var user = await _context.Users.FindAsync(iduser);
            if (user == null)
            {
                return RedirectToAction("Login", "Accout");
            }
            await EnsureFolderExistsAsync(user.Id);
            var folders = _context.Folders
                .Where(f => f.UserId == iduser)
                .OrderBy(f => f.Name)
                .ToList();

            ViewBag.Folders = folders;
            var listEduQuizByUser = await _context.EduQuizs
                    .Where(d => d.UserId == user.Id && d.Type == 0 && d.Status == true)
                    .Include(e => e.Questions)
                    .OrderByDescending(d => d.UpdateAt)
                    .ToListAsync();
            ViewBag.Data = JsonConvert.SerializeObject(new { UserName = user.Username, Avatar = user.ProfilePicture });
            return View(listEduQuizByUser);
        }
        [Route("my-library/eduquizs/{id:guid}")]
        public async Task<IActionResult> LibrarybyFolder(Guid id)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Login", "Accout");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int iduser = int.Parse(userId ?? "1");
            var user = await _context.Users.FindAsync(iduser);
            if (user == null)
            {
                return RedirectToAction("Login", "Accout");
            }
            var folders = _context.Folders
                .Where(f => f.UserId == iduser)
                .OrderBy(f => f.Name)
                .ToList();
            // Tìm folder hiện tại dựa trên `Uuid`
            var selectedFolder = folders.FirstOrDefault(f => f.Uuid == id);
            if (selectedFolder == null)
            {
                return RedirectToAction("Error404", "Home");
            }
            var eduQuizByFolder = _context.QuizFolders
                 .Where(qf => qf.FolderId == selectedFolder.Id)
                .Include(qf => qf.EduQuiz) // Bao gồm cả EduQuiz
                .ThenInclude(eq => eq.Questions)
                .Select(qf => qf.EduQuiz)
                .Where(qf => qf.Type == 1)
                .ToList();
            var view = new FolderViewModel
            {
                Folders = folders,
                EduQuizs = eduQuizByFolder
            };
            ViewBag.Data = JsonConvert.SerializeObject(new { UserName = user.Username, Avatar = user.ProfilePicture });
            return View(view);
        }

        #region handle
        private async Task EnsureFolderExistsAsync(int userId)
        {
            var folderExists = await _context.Folders
                .AnyAsync(f => f.UserId == userId);

            if (!folderExists)
            {
                var folder = new Folder
                {
                    Name = "Thư mục của bạn",
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                };

                _context.Folders.Add(folder);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IActionResult> AddFolder(string folderName,int idroot)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                int iduser = 0;
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");
                   
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                iduser = int.Parse(userId ?? "1");
                if (string.IsNullOrEmpty(folderName))
                {
                    return Json(new { result = "FAIL",msg ="Vui lòng nhập tên thư mục" });
                }
                Folder folder = new Folder
                {
                    Name = folderName,
                    UserId = iduser,
                    ParentFolderId = idroot,
                    CreatedAt = DateTime.Now,
                };
                _context.Folders.Add(folder);
                await _context.SaveChangesAsync();
                return Json(new { result = "PASS"});
            }
            catch (Exception ex) {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        public async Task<IActionResult> RemoveFolder(int folderid)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");

                }
                var folderToDelete = await _context.Folders
                    .Include(f => f.ChildFolders) 
                    .Include(f => f.QuizFolders)
                    .FirstOrDefaultAsync(f => f.Id == folderid);

                if (folderToDelete == null)
                {
                    return Json(new { result = "FAIL", msg = "Thư mục không tồn tại." });
                }

                _context.QuizFolders.RemoveRange(folderToDelete.QuizFolders);

                DeleteChildFolders(folderToDelete.ChildFolders);
                _context.Folders.Remove(folderToDelete);

                await _context.SaveChangesAsync();
                return Json(new { result = "PASS" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        private void DeleteChildFolders(IEnumerable<Folder> childFolders)
        {
            foreach (var childFolder in childFolders.ToList())
            {
                _context.QuizFolders.RemoveRange(childFolder.QuizFolders);

                DeleteChildFolders(childFolder.ChildFolders);

                _context.Folders.Remove(childFolder);
            }
        }
        public async Task<IActionResult> RenameEduQuiz(string name,int idquiz)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");

                }
                var getquiz = await _context.EduQuizs.FindAsync(idquiz);
                if (getquiz == null) 
                {
                    return Json(new { result = "FAIL", msg = "EduQuiz không tồn tại" });
                }
                if (getquiz.UserId != getquiz.UserId)
                {
                    return Json(new { result = "FAIL", msg = "Bạn Không có quyền sửa" });
                }
                else
                {
                    getquiz.Title = name;
                    await _context.SaveChangesAsync();
                    return Json(new { result = "PASS" });
                }
            }
            catch (Exception ex) {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        public async Task<IActionResult> RemoveEduQuiz(int idquiz)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");

                }
                var getquiz = await _context.EduQuizs.FindAsync(idquiz);
                if (getquiz == null)
                {
                    return Json(new { result = "FAIL", msg = "EduQuiz không tồn tại" });
                }
                if (getquiz.UserId != getquiz.UserId)
                {
                    return Json(new { result = "FAIL", msg = "Bạn Không có quyền sửa" });
                }
                else
                {
                    getquiz.Status = false;
                    await _context.SaveChangesAsync();
                    return Json(new { result = "PASS" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        public async Task<IActionResult> RemoveEduQuizByFolder(int folderid, int idquiz)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");

                }
                var getquiz = await _context.EduQuizs.FindAsync(idquiz);
                if (getquiz == null)
                {
                    return Json(new { result = "FAIL", msg = "EduQuiz không tồn tại" });
                }
                if (getquiz.UserId != getquiz.UserId)
                {
                    return Json(new { result = "FAIL", msg = "Bạn Không có quyền sửa" });
                }
                else
                {
                    var getEduQuizbyFolder = await _context.QuizFolders.FirstOrDefaultAsync(f => f.FolderId == folderid && f.EduQuizId == idquiz);
                    _context.QuizFolders.Remove(getEduQuizbyFolder);
                    await _context.SaveChangesAsync();
                    return Json(new { result = "PASS" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }

        public async Task<IActionResult> GetFolderbyUser(int idfolder)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");
                    
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                int iduser = int.Parse(userId ?? "1");

                if (idfolder == 0)
                {
                    var rootFolder = await _context.Folders
                       .Where(f => f.UserId == iduser && f.ParentFolderId == null)
                       .Select(f => new
                       {
                           Id = f.Id,
                           Name = f.Name,
                           ParentFolderId = f.ParentFolderId
                       })
                       .FirstOrDefaultAsync();

                    if (rootFolder == null)
                    {
                        return Json(new { result = "FAIL", msg = "Thư mục không tồn tại" });
                    }

                    var childFolders = await _context.Folders
                        .Where(f => f.UserId == iduser && f.ParentFolderId == rootFolder.Id)
                        .Select(f => new
                        {
                            Id = f.Id,
                            Name = f.Name
                        })
                        .ToListAsync();

                    return Json(new { result = "PASS", data = childFolders,dataroot = rootFolder });
                }
                else
                {
                    var currentFolder = await _context.Folders
                       .Where(f => f.UserId == iduser && f.Id == idfolder)
                       .Select(f => new
                       {
                           Id = f.Id,
                           Name = f.Name,
                           ParentFolderId = f.ParentFolderId
                       })
                       .FirstOrDefaultAsync();
                    var folders = await _context.Folders
                        .Where(f => f.UserId == iduser && f.ParentFolderId == idfolder).Select(f => new
                        {
                            Id = f.Id,
                            Name = f.Name
                        })
                        .ToListAsync();

                    return Json(new { result = "PASS", data = folders , dataroot = currentFolder });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        public async Task<IActionResult> MoveEduQuiz(int idfolder, int ideduquiz,int idfoldercurrent)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");

                }
                var getfolder = await _context.Folders.FindAsync(idfolder);
                if (idfoldercurrent != 0)
                {
                    var currentQuizFolder = await _context.QuizFolders.FirstOrDefaultAsync(q => q.EduQuizId == ideduquiz && q.FolderId == idfoldercurrent);
                    if (currentQuizFolder != null)
                    {
                        _context.QuizFolders.Remove(currentQuizFolder);
                    }
                }
                var checkQuizFolder = await _context.QuizFolders.FirstOrDefaultAsync(q => q.EduQuizId == ideduquiz && q.FolderId == idfolder);
                if (checkQuizFolder == null) {
                    QuizFolder quizFolder = new QuizFolder()
                    {
                        EduQuizId = ideduquiz,
                        FolderId = idfolder
                    };
                    _context.QuizFolders.Add(quizFolder); 
                    await _context.SaveChangesAsync();
                    return Json(new { result = "PASS" ,data = getfolder?.Uuid});
                }
                else
                {
                    return Json(new { result = "FAIL", msg = "EduQuiz đã tồn tại" , data = getfolder?.Uuid });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        public async Task<IActionResult> MoveMutiEduQuiz(int idfolder, int[] ideduquiz, int idfoldercurrent)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");

                }
                var getfolder = await _context.Folders.FindAsync(idfolder);
                if (getfolder == null)
                {
                    return Json(new { result = "FAIL", msg = "Thư mục không tồn tại." });
                }
                if (idfoldercurrent != 0)
                {
                    var currentQuizFolders = await _context.QuizFolders
                        .Where(q => ideduquiz.Contains(q.EduQuizId) && q.FolderId == idfoldercurrent)
                        .ToListAsync();

                    if (currentQuizFolders.Any())
                    {
                        _context.QuizFolders.RemoveRange(currentQuizFolders);
                    }
                }
                foreach (var eduQuizId in ideduquiz)
                {
                    var checkQuizFolder = await _context.QuizFolders
                        .FirstOrDefaultAsync(q => q.EduQuizId == eduQuizId && q.FolderId == idfolder);

                    if (checkQuizFolder == null)
                    {
                        QuizFolder quizFolder = new QuizFolder()
                        {
                            EduQuizId = eduQuizId,
                            FolderId = idfolder
                        };
                        _context.QuizFolders.Add(quizFolder);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { result = "PASS", data = getfolder?.Uuid });
               
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        public async Task<IActionResult> RenameFolder(string name, int folderid)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");

                }
                var getfolder = await _context.Folders.FindAsync(folderid);
                if (getfolder == null)
                {
                    return Json(new { result = "FAIL", msg = "Thư mục không tồn tại" });
                }
                if (getfolder.UserId != getfolder.UserId)
                {
                    return Json(new { result = "FAIL", msg = "Bạn Không có quyền sửa" });
                }
                else
                {
                    getfolder.Name = name;
                    await _context.SaveChangesAsync();
                    return Json(new { result = "PASS" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }

        public async Task<IActionResult> DuplicateEduQuiz(int idquiz,int folderid)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Accout");
                   
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                int iduser = int.Parse(userId ?? "1");

                // Tìm EduQuiz cần sao chép
                var originalEduQuiz = await _context.EduQuizs
                    .Include(eq => eq.Questions)
                    .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(eq => eq.Id == idquiz);
                if (originalEduQuiz == null)
                {
                    return Json(new { result = "FAIL", msg = "EduQuiz không tồn tại." });
                }
                // Tạo một bản sao của EduQuiz
                var newEduQuiz = new Models.EF.EduQuiz
                {
                    Title = originalEduQuiz.Title + " (Bản sao)",
                    Uuid = Guid.NewGuid(),
                    ImageCover = originalEduQuiz.ImageCover,
                    Description = originalEduQuiz.Description,
                    Type = originalEduQuiz.Type,
                    Visibility = originalEduQuiz.Visibility,
                    ThemeId = originalEduQuiz.ThemeId,
                    MusicId = originalEduQuiz.MusicId,
                    OrderQuestion = originalEduQuiz.OrderQuestion,
                    CreatedAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    UserId = iduser,
                    Questions = new List<Question>()
                };
                // Sao chép các câu hỏi và lựa chọn
                foreach (var originalQuestion in originalEduQuiz.Questions)
                {
                    var newQuestion = new Question
                    {
                        QuestionText = originalQuestion.QuestionText,
                        TypeQuestion = originalQuestion.TypeQuestion,
                        TypeAnswer = originalQuestion.TypeAnswer,
                        Time = originalQuestion.Time,
                        PointsMultiplier = originalQuestion.PointsMultiplier,
                        Image = originalQuestion.Image,
                        ImageEffect = originalQuestion.ImageEffect,
                        Choices = new List<Choice>()
                    };

                    if (originalQuestion.Choices?.Count > 0)
                    {
                        var newChoices = originalQuestion.Choices.Select(choice => new Choice
                        {
                            Question = newQuestion, // Gán trực tiếp vào Question mới
                            Answer = choice.Answer,
                            IsCorrect = choice.IsCorrect,
                            DisplayOrder = choice.DisplayOrder
                        }).ToList();

                        newQuestion.Choices = newChoices; // Gán danh sách Choices vào Question
                    }

                    newEduQuiz.Questions.Add(newQuestion);
                }

                // Lưu EduQuiz mới vào cơ sở dữ liệu
                _context.EduQuizs.Add(newEduQuiz);

                if(folderid != 0)
                {
                    var quizFolder = new QuizFolder
                    {
                        EduQuiz = newEduQuiz, // Gán trực tiếp EduQuiz mới vào QuizFolder
                        FolderId = folderid
                    };

                    _context.QuizFolders.Add(quizFolder);
                }
                await _context.SaveChangesAsync();
                return Json(new { result = "PASS" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }

        #endregion
    }
}
