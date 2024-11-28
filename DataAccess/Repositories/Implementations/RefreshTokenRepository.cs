using DataAccess;
using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories.Implementations
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                                 .Include(rt => rt.User)
                                 .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RemoveAllByUserIdAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                                       .Where(rt => rt.UserId == userId)
                                       .ToListAsync();
            _context.RefreshTokens.RemoveRange(tokens);
        }
    }
}
