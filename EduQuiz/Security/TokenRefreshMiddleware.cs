using EduQuiz.Security;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TokenRefreshMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var auth = scope.ServiceProvider.GetRequiredService<CookieAuth>();
            var accessToken = context.Request.Cookies["acToken"];
            var refreshToken = context.Request.Cookies["rfToken"];

            if (string.IsNullOrEmpty(accessToken) || auth.IsTokenExpired(accessToken))
            {
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var validatedToken = await auth.ValidateRefreshToken(refreshToken);
                    if (validatedToken != null)
                    {
                        var newAccessToken = auth.GenerateToken(validatedToken);
                        context.Response.Cookies.Append("acToken", newAccessToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });
                        context.Request.Headers["Cookie"] = $"acToken={newAccessToken}; " + context.Request.Headers["Cookie"];

                        context.Response.Cookies.Append("rfToken", validatedToken.RefeshToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(7)
                        });
                    }                  
                }  
            }

            await _next(context);
        }
    }
}
