using BusinessLogic.DTOs.Auth;

namespace BusinessLogic.Auth.Services.Interfaces
{
    public interface IExternalAuthService
    {
        Task<AuthResponseDto> GoogleLoginAsync(string idToken, string ipAddress);

    }
}
