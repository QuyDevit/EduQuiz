using EduQuiz.Areas.Admin.Models;
using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize("Admin")]
    public class PageController : Controller
    {
        private readonly EduQuizDBContext _context;
        public PageController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("admin/manage/profile")]
        public async Task<IActionResult> Index()
        {
            var listProfile = await (from p in _context.Profile 
                                     join u in _context.Users on p.UserId equals u.Id
                                     where u.Id != 8
                                     select new PageViewModel
                                     {
                                         Id = p.Id,
                                         Avatar = u.ProfilePicture,
                                         Email = u.Email,
                                         UserName=u.Username,
                                         ImagePage =p.Image,
                                         DescriptionPage = p.DescriptionPage,
                                         Status = p.Status,
                                         TitlePage = p.TitlePage,
                                         Uuid=p.Uuid,
                                         CreateDate = p.CreateDate,
                                     }).ToListAsync();

            return View(listProfile);
        }
        #region handle
        public async Task<IActionResult> ChangeStatusProfile(int idprofile)
        {
            var findprofile = await _context.Profile.FindAsync(idprofile);
            if (findprofile == null)
            {
                return Json(new { status = false });
            }
            findprofile.Status = !findprofile.Status;
            await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        #endregion
    }
}
