using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize("Admin")]
    public class AccountController : Controller
    {
        private readonly EduQuizDBContext _context;
        public AccountController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("admin/manage/account")]
        public async Task<IActionResult> Index()
        {
            var listaccount = await _context.Users.Include(n=>n.Role).Where(n=>n.RoleId != 5).ToListAsync();
            return View(listaccount);
        }
        #region handle
        public async Task<IActionResult> ChangeStatusAccount(int iduser)
        {
            var finduser = await _context.Users.FindAsync(iduser);
            if (finduser == null)
            {
                return Json(new { status = false });
            }
            finduser.Status = !finduser.Status;
            _context.SaveChanges();
            return Json(new {status = true});
        }
        #endregion
    }
}
