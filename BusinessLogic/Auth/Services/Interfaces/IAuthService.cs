using BusinessLogic.DTOs.Auth;

namespace BusinessLogic.Auth.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegistrationDto registrationDto, string ipAddress);
        Task<LoginResponseDto> LoginAsync(UserLoginDto loginDto, string ipAddress);
        Task<AuthResponseDto> LogoutAsync(string refreshToken, string ipAddress);
        Task<AuthResponseDto> RefreshTokenAsync(string token, string ipAddress);

    }
}
