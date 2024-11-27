using EduQuiz.Areas.Admin.Models;
using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize("Admin")]
    public class GroupUserController : Controller
    {
        private readonly EduQuizDBContext _context;
        public GroupUserController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("admin/manage/group")]
        public async Task<IActionResult> Index()
        {
            var listgroupUser = await (from g in _context.Groups
                                       join gm in _context.GroupMembers on g.Id equals gm.GroupId
                                       group gm by new { g.Id, g.Name, g.SubscriptionType, g.SubscriptionStartDate, g.SubscriptionEndDate, g.Status,g.CreatedDate } into grp
                                       select new GroupUserViewModel
                                       {
                                           Id = grp.Key.Id,
                                           Name = grp.Key.Name,
                                           SubscriptionType = grp.Key.SubscriptionType,
                                           SubscriptionStartDate = grp.Key.SubscriptionStartDate ?? DateTime.Now,
                                           SubscriptionEndDate = grp.Key.SubscriptionEndDate ?? DateTime.Now,
                                           SumMember = grp.Count(),
                                           CreateAt = grp.Key.CreatedDate,
                                           Status = grp.Key.Status
                                       }).ToListAsync();

            return View(listgroupUser);
        }
        #region handle
        public async Task<IActionResult> ChangeStatusGroup(int idgroup)
        {
            var findGroup = await _context.Groups.FindAsync(idgroup);
            if (findGroup == null)
            {
                return Json(new { status = false });
            }
            findGroup.Status = !findGroup.Status;
            _context.SaveChanges();
            return Json(new { status = true });
        }
        #endregion
    }
}
