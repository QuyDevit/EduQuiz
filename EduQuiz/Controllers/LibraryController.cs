using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static NuGet.Packaging.PackagingConstants;

namespace EduQuiz.Controllers
{
    public class LibraryController : Controller
    {
        private readonly EduQuizDBContext _context;
        public LibraryController(EduQuizDBContext context)
        {
            _context = context;
        }
        public IActionResult _PartialFolders()
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Index", "Home");
            }
            var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
            int.TryParse(userInfo?.Id?.ToString(), out int userId);
            var model = _context.Folders
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.Name)
                .ToList();
            return PartialView("_PartialFolders", model);
        }
        [Route("my-library/eduquizs/all")]
        public async Task<IActionResult> Index()
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Index", "Home");
            }
            var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
            int.TryParse(userInfo?.Id?.ToString(), out int userId);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            await EnsureFolderExistsAsync(user.Id);
            var folders = _context.Folders
              .Where(f => f.UserId == userId)
              .OrderBy(f => f.Name)
              .ToList();

            ViewBag.Folders = folders;
            var listEduQuizByUser = await _context.EduQuizs
                    .Where(d => d.UserId == user.Id && d.Type == 1)
                    .Include(e => e.Questions)
                    .OrderByDescending(d => d.UpdateAt)
                    .ToListAsync();
            ViewBag.Data = JsonConvert.SerializeObject(new { UserName = user.Username, Avatar = user.ProfilePicture });
            return View(listEduQuizByUser);
        }
        [Route("my-library/eduquizs/drafts")]
        public async Task<IActionResult> EduQuizDraft()
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Index", "Home");
            }
            var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
            int.TryParse(userInfo?.Id?.ToString(), out int userId);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var folders = _context.Folders
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.Name)
                .ToList();

            ViewBag.Folders = folders;
            var listEduQuizByUser = await _context.EduQuizs
                    .Where(d => d.UserId == user.Id && d.Type == 0)
                    .Include(e => e.Questions)
                    .OrderByDescending(d => d.UpdateAt)
                    .ToListAsync();
            ViewBag.Data = JsonConvert.SerializeObject(new { UserName = user.Username, Avatar = user.ProfilePicture });
            return View(listEduQuizByUser);
        }
        [Route("my-library/eduquizs/{id:guid}")]
        public async Task<IActionResult> LibrarybyFolder(Guid id)
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Index", "Home");
            }
            var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
            int.TryParse(userInfo?.Id?.ToString(), out int userId);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var folders = _context.Folders
                .Where(f => f.UserId == userId)
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
            var view = new FolderModelView
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
                var sessionData = HttpContext.Session.GetString("_USERCURRENT");
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                int.TryParse(userInfo?.Id?.ToString(), out int userId);
                if (string.IsNullOrEmpty(sessionData))
                {
                    return RedirectToAction("Login","Account");
                }
                if (string.IsNullOrEmpty(folderName))
                {
                    return Json(new { result = "FAIL",msg ="Vui lòng nhập tên thư mục" });
                }
                Folder folder = new Folder
                {
                    Name = folderName,
                    UserId = userId,
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

        public async Task<IActionResult> RenameEduQuiz(string name,int idquiz)
        {
            try
            {
                var sessionData = HttpContext.Session.GetString("_USERCURRENT");
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                int.TryParse(userInfo?.Id?.ToString(), out int userId);
                if (string.IsNullOrEmpty(sessionData))
                {
                    return RedirectToAction("Login", "Account");
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

        public async Task<IActionResult> GetFolderbyUser(int idfolder)
        {
            try
            {
                var sessionData = HttpContext.Session.GetString("_USERCURRENT");
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                int.TryParse(userInfo?.Id?.ToString(), out int userId);
                if (string.IsNullOrEmpty(sessionData))
                {
                    return RedirectToAction("Login", "Account");
                }
                if (idfolder == 0)
                {
                    var rootFolder = await _context.Folders
                       .Where(f => f.UserId == userId && f.ParentFolderId == null)
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
                        .Where(f => f.UserId == userId && f.ParentFolderId == rootFolder.Id)
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
                       .Where(f => f.UserId == userId && f.Id == idfolder)
                       .Select(f => new
                       {
                           Id = f.Id,
                           Name = f.Name,
                           ParentFolderId = f.ParentFolderId
                       })
                       .FirstOrDefaultAsync();
                    var folders = await _context.Folders
                        .Where(f => f.UserId == userId && f.ParentFolderId == idfolder).Select(f => new
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
        public async Task<IActionResult> MoveEduQuiz(int idfolder, int ideduquiz)
        {
            try
            {
                var sessionData = HttpContext.Session.GetString("_USERCURRENT");
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                int.TryParse(userInfo?.Id?.ToString(), out int userId);
                if (string.IsNullOrEmpty(sessionData))
                {
                    return RedirectToAction("Login", "Account");
                }
                var getfolder = await _context.Folders.FindAsync(idfolder);
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
                    return Json(new { result = "FAIL", msg = "EduQuiz đã tồn tại" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = ex.Message });
            }
        }
        #endregion
    }
}
