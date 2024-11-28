using DataAccess.Entities;
using BusinessLogic.DTOs.User;
using BusinessLogic.Utils.Implementation;
using BusinessLogic.DTOs;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task AssignRoleAsync(int userId, string roleName);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<ReadUserDto>> GetAllUsersAsync();
        Task<PaginatedList<ReadUserDto>> GetAllUsersPaginatedAsync(QueryParameters queryParameters);
        /// <summary>
        /// Bans a user by setting their status to false.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to ban.</param>
        /// <returns>A success message indicating the user has been banned.</returns>
        Task<string> BanUserAsync(int userId);

        /// <summary>
        /// Unbans a user by setting their status to true.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to unban.</param>
        /// <returns>A success message indicating the user has been unbanned.</returns>
        Task<string> UnbanUserAsync(int userId);
    }
}
