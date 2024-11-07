using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduQuiz.Security
{
    public class CookieAuth
    {
        private readonly IConfiguration _config;
        private readonly EduQuizDBContext _context;
        public CookieAuth(IConfiguration config, EduQuizDBContext context) {
            _config = config;
            _context = context;
        }
        public bool ValidateToken(string token, out string userRoleId)
        {
            userRoleId = null;
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,  
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = securityKey
                };

                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == "RoleId");
                if (roleClaim != null)
                {
                    userRoleId = roleClaim.Value;
                    return true;
                }

                return false; 
            }
            catch
            {
                return false; 
            }
        }

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("Avatar", user.ProfilePicture),
                new Claim("Email", user.Email),
                new Claim("UserName", user.Username),
            };
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
        public async Task<User> ValidateRefreshToken(string refreshToken)
        {
            var checkRFtoken = _context.Users.FirstOrDefault(u => u.RefeshToken == refreshToken);
            if (checkRFtoken != null)
            {
                var newRefreshToken = GenerateRefreshToken();
                checkRFtoken.RefeshToken = newRefreshToken;
                checkRFtoken.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();


                return checkRFtoken;
            }
            return null;
        }
        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }
        public UserClaims GetUserClaimsFromToken(string token)
        {
            var userClaims = new UserClaims();
            try
            {
                
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = securityKey
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var userNameClaim = principal.Claims.FirstOrDefault(c => c.Type == "UserName");
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "UserId");
                var avatarClaim = principal.Claims.FirstOrDefault(c => c.Type == "Avatar");
                var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == "Email");
                var roleIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "RoleId");

                userClaims.Avatar = avatarClaim?.Value;
                userClaims.UserId = userIdClaim?.Value;
                userClaims.Email = emailClaim?.Value;
                userClaims.RoleId = roleIdClaim?.Value;
                userClaims.UserName = userNameClaim?.Value;
            }
            catch (Exception)
            {
                // Handle any exceptions or log them as needed
            }
            return userClaims;

        }
    }
}
