using EduQuiz.DatabaseContext;
using EduQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Numerics;

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
        public async Task<IActionResult> FilterOrder(DateTime startdate, DateTime enddate)
        {
            var listorder = await _context.Orders
                .Where(n=>n.CreateAt >= startdate && n.CreateAt <= enddate)
                .Include(n => n.User).Select(n => new {
                    ProfilePicture = n.User.ProfilePicture,
                    Username = n.User.Username,
                    UserEmail = n.User.Email,
                    FirstName = n.FirstName,
                    LastName = n.LastName,
                    OrderEmail = n.Email,
                    Company = n.Company,
                    PhoneNumber = n.PhoneNumber,
                    PlanType = n.Quantity + " " + (n.Period == "year" ? "năm" : "tháng") + " " + (n.PlanType == "organization" ? "EduQuiz+ dành cho Tổ chức" : "EduQuiz+ Chuyên nghiệp"),
                    Period = n.Period,
                    PaymentMethod = n.PaymentMethod,
                    CreateAt = n.CreateAt.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    Status = n.Status == "Pending" ? "Đang chờ" : n.Status == "Success" ? "Thành công" : "Đã hủy",
                    OrderId = n.OrderId
                })
                .ToListAsync();
            return Json(new { status = true, data = JsonConvert.SerializeObject(listorder) });
        }
        public async Task<IActionResult> ExportReportOrder(DateTime startdate, DateTime enddate)
        {
            // Đường dẫn tới file Excel mẫu
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/src/templates", "ReportBillTemplate.xlsx");
            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var listorder = await _context.Orders
               .Where(n => n.CreateAt >= startdate && n.CreateAt <= enddate)
               .Select(n => new {
                   UserEmail = n.User.Email,
                   FirstName = n.FirstName,
                   LastName = n.LastName,
                   OrderEmail = n.Email,
                   Company = n.Company,
                   PhoneNumber = n.PhoneNumber,
                   PlanType = n.Quantity + " " + (n.Period == "year" ? "năm" : "tháng") + " " + (n.PlanType == "organization" ? "EduQuiz+ dành cho Tổ chức" : "EduQuiz+ Chuyên nghiệp"),
                   Period = n.Period,
                   PaymentMethod = n.PaymentMethod,
                   CreateAt = n.CreateAt.ToString("MM/dd/yyyy hh:mm:ss tt"),
                   Status = n.Status == "Pending" ? "Đang chờ" : n.Status == "Success" ? "Thành công" : "Đã hủy",
                   OrderId = n.OrderId
               })
               .ToListAsync();
                var worksheet = package.Workbook.Worksheets[0];
                var startRow = 5;
                worksheet.Cells[1, 1].Value ="Từ ngày: "+ startdate.ToString("dd/MM/yyyy");
                worksheet.Cells[1, 3].Value ="Đến ngày: "+ enddate.ToString("dd/MM/yyyy");
                foreach (var orderbill in listorder)
                {
                    worksheet.Cells[startRow, 2].Value = orderbill.OrderId;
                    worksheet.Cells[startRow, 3].Value = orderbill.FirstName +" "+orderbill.LastName;
                    worksheet.Cells[startRow, 4].Value = orderbill.OrderEmail;
                    worksheet.Cells[startRow, 5].Value = orderbill.Company;
                    worksheet.Cells[startRow, 6].Value = orderbill.PhoneNumber;
                    worksheet.Cells[startRow, 7].Value = orderbill.PlanType;
                    worksheet.Cells[startRow, 8].Value = orderbill.PaymentMethod;
                    worksheet.Cells[startRow, 9].Value = orderbill.CreateAt;
                    worksheet.Cells[startRow, 10].Value = orderbill.Status;
                    startRow++;
                }
                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
            }
        }
    }
}
