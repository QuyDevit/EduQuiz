using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;

public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext filterContext)
    {
        var cookieAuth = filterContext.HttpContext.RequestServices.GetRequiredService<CookieAuth>(); 
        var urlHelperFactory = filterContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
        var urlHelper = urlHelperFactory.GetUrlHelper(filterContext);

        var token = filterContext.HttpContext.Request.Cookies["acToken"];
        if (string.IsNullOrEmpty(token) || !cookieAuth.ValidateToken(token))
        {
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var redirectUrl = urlHelper.Action("Login", "Account");

                 filterContext.Result = new JsonResult(new
                {
                    redirectUrl = redirectUrl
                });
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Account",
                    action = "Login"
                }));
            }
        }
    }
}
