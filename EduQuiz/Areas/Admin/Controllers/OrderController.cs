using EduQuiz.DatabaseContext;
using EduQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize("Admin")]
    public class OrderController : Controller
    {
        private readonly EduQuizDBContext _context;
        private readonly ZaloPayService _zaloPayService;
        private readonly MoMoService _momoService;
        public OrderController(EduQuizDBContext context,ZaloPayService zaloPayService, MoMoService momoService)
        {
            _context = context;
            _zaloPayService = zaloPayService;
            _momoService = momoService; 
        }
        [Route("admin/manage/bill")]
        public async Task<IActionResult> Index()
        {
            var listorder = await _context.Orders.Include(n=>n.User).ToListAsync();
            return View(listorder);
        }
        public async Task<IActionResult> CheckBill(string tranid,string paymentmethod)
        {
            Dictionary<string, string> result;
            var findorder = await _context.Orders.SingleOrDefaultAsync(x=>x.OrderId == tranid);
            if(paymentmethod == "ZALOPAY")
            {
                result = await _zaloPayService.QueryOrderStatusAsync(tranid);
                if (result["return_code"] != "1" && findorder.CreateAt.AddMinutes(20) < DateTime.Now) {
                    findorder.Status = "Failed";
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                result = await _momoService.QueryOrderStatusAsync(tranid);
                if (result["resultCode"] != "0" && findorder.CreateAt.AddMinutes(120) < DateTime.Now)
                {
                    findorder.Status = "Failed";
                    await _context.SaveChangesAsync();
                }
            }
            return Json(new {status = true ,data = result});
        }
    }
}
