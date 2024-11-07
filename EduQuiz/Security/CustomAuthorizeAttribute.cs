using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public CustomAuthorizeAttribute(string role)
    {
        _role = role;
    }
    public void OnAuthorization(AuthorizationFilterContext filterContext)
    {
        var allowAnonymous = filterContext.ActionDescriptor.EndpointMetadata
                   .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

        if (allowAnonymous)
        {
            return;
        }
        var cookieAuth = filterContext.HttpContext.RequestServices.GetRequiredService<CookieAuth>();
        var urlHelperFactory = filterContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
        var urlHelper = urlHelperFactory.GetUrlHelper(filterContext);

        var token = filterContext.HttpContext.Request.Cookies["acToken"];
        if (string.IsNullOrEmpty(token) || !cookieAuth.ValidateToken(token, out string userRoleId))
        {
            HandleUnauthorizedRequest(filterContext, urlHelper);
            return;
        }
        var userRole = MapRoleIdToRole(userRoleId);
        if (userRole != _role && userRole != "Admin")
        {
            filterContext.Result = new ForbidResult();
        }
    }
    private string MapRoleIdToRole(string roleId)
    {
        return roleId switch
        {
            "1" => "User",
            "2" => "User",
            "3" => "User",
            "4" => "User",
            "5" => "Admin",
            _ => "Unknown"
        };
    }
    private void HandleUnauthorizedRequest(AuthorizationFilterContext filterContext, IUrlHelper urlHelper)
    {
        if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var redirectUrl = _role == "User" ? urlHelper.Action("Login", "Account") : urlHelper.Action("Index", "Auth", new { area = "Admin" });

            filterContext.Result = new JsonResult(new
            {
                redirectUrl = redirectUrl
            });
        }
        else
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                controller = _role == "User" ? "Account" : "Auth",
                action = _role == "User" ? "Login" : "Index",
                area = _role == "User" ? null : "Admin"
            }));
        }
    }
}
