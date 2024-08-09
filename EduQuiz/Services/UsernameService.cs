using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using EduQuiz.DatabaseContext;
using EduQuiz.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Services
{
    public class UsernameService
    {
        private readonly EduQuizDBContext _context;
        private readonly IMemoryCache _cache;
        public UsernameService(EduQuizDBContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        public async Task<bool> IsUsernameAvailable(string username)
        {
            // Kiểm tra trong cache
            if (_cache.TryGetValue(username, out _))
            {
                return false;
            }

            // Kiểm tra cơ sở dữ liệu
            var userExists = await _context.Users.AnyAsync(x => x.Username == username);

            // Nếu không tồn tại trong cơ sở dữ liệu, lưu vào cache
            if (!userExists)
            {
                _cache.Set(username, true, TimeSpan.FromMinutes(5)); // Thời gian hết hạn cache
            }

            return !userExists;
        }
    }
}
