using BusinessLogic.DTOs.User;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserPermissionService
    {
        Task<IEnumerable<UserPermissionDto>> GetCurrentUserPermissionsAsync(string userName);
    }
}
