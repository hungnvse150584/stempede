using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task RemoveAllByUserIdAsync(int userId);
    }
}
