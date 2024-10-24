using EduQuiz.DatabaseContext;
using EduQuiz.Helper;
using EduQuiz.Hubs;
using EduQuiz.Models;
using EduQuiz.Security;
using EduQuiz.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
	.AddRazorRuntimeCompilation();
builder.Services.AddSignalR();
// Thêm dịch vụ session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Đăng ký EduQuizDBContext
builder.Services.AddDbContext<EduQuizDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EduQuizDBConnection")));
builder.Services.AddScoped<UsernameService>(); // Đăng ký UsernameService
builder.Services.AddScoped<GeminiAiService>(); // Đăng ký GeminiAiService
builder.Services.AddScoped<CookieAuth>();// Đăng ký CookieAuth
// Đọc cấu hình từ appsettings.json và thêm vào container DI
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Đăng ký EmailService
builder.Services.AddTransient<IEmailService, EmailService>();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.UseMiddleware<TokenRefreshMiddleware>();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Thêm middleware session
app.UseSession();

app.UseAuthentication(); 
app.UseAuthorization();

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.Redirect("/error404");
    }
    await Task.CompletedTask;
});
app.MapHub<GameHub>("/gameHub");
app.MapHub<SoloGameHub>("/sologameHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
