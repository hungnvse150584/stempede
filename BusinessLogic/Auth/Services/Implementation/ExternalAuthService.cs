using BusinessLogic.Services.Interfaces;
using BusinessLogic.Auth.Services.Interfaces;
using BusinessLogic.Auth.Helpers.Interfaces;
using BusinessLogic.DTOs.Auth;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Auth.Services.Implementation
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IGoogleTokenValidator _googleTokenValidator;
        private readonly ILogger<ExternalAuthService> _logger;

        public ExternalAuthService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IRefreshTokenService refreshTokenService,
            IJwtTokenService jwtTokenService,
            IGoogleTokenValidator googleTokenValidator,
            ILogger<ExternalAuthService> logger)
        {
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _jwtTokenService = jwtTokenService;
            _googleTokenValidator = googleTokenValidator;
            _logger = logger;
        }

        public async Task<AuthResponseDto> GoogleLoginAsync
            (string idToken, string ipAddress)
        {
            _logger.LogInformation("External Google login attempt from IP: {IpAddress}", ipAddress);

            try
            {
                var payload = await _googleTokenValidator.ValidateAsync(idToken);
                if (payload == null)
                {
                    _logger.LogWarning("Invalid Google token from IP: {IpAddress}", ipAddress);
                    return new AuthResponseDto { Success = false, Message = "Invalid Google token." };
                }

                // Check if user exists
                var user = await _userService.GetUserByEmailAsync(payload.Email);
                if (user == null)
                {
                    // Create a new user
                    user = new User
                    {
                        Email = payload.Email,
                        Username = payload.Name ?? payload.Email,
                        Password = null,
                        Phone = "N/A",
                        Address = "N/A",
                        IsExternal = true,
                        ExternalProvider = "Google",
                        Status = true,
                        FullName = payload.Name ?? payload.Email
                    };
                    await _userService.CreateUserAsync(user);

                    // Assign default role
                    await _userService.AssignRoleAsync(user.UserId, "Customer");
                }

                // Retrieve user roles
                var roles = await _userService.GetUserRolesAsync(user.UserId);

                // Generate JWT token with roles
                var token = _jwtTokenService.GenerateJwtToken(user.UserId, user.Username, roles, user.Status);

                // Generate Refresh Token with IP address
                var refreshToken = _refreshTokenService.GenerateRefreshToken(user.UserId, ipAddress);

                await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
                await _unitOfWork.CompleteAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken.Token,
                    Message = "Login successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during Google login.");
                return new AuthResponseDto { Success = false, Message = "Login failed. Please try again." };
            }
        }
    }
}
