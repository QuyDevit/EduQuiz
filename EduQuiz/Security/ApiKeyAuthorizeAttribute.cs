using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EduQuiz.Security
{
    public class ApiKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            if (configuration == null)
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Không tìm thấy cấu hình!" });
                return;
            }
            var API_KEY_HEADER = configuration["ApiConfig:ApiHeader"];
            var VALID_API_KEY = configuration["ApiConfig:ApiKey"];

            // Kiểm tra API Key trong header
            if (!request.Headers.TryGetValue(API_KEY_HEADER, out var apiKey) || apiKey != VALID_API_KEY)
            {
                // Trả về lỗi Unauthorized
                context.Result = new UnauthorizedObjectResult(new { error = "Key API không hợp lệ!" });
            }
        }
    }
}
