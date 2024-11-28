using DataAccess.Data;
using DataAccess.Entities;
using BusinessLogic.Auth.Services.Interfaces;
using BusinessLogic.DTOs.Auth;
using BusinessLogic.Utils.Interfaces;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Auth.Services.Implementation
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(
            IConfiguration configuration,
            IDateTimeProvider dateTimeProvider,
            IUnitOfWork unitOfWork,
            IJwtTokenService jwtTokenService,
            ILogger<RefreshTokenService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public RefreshToken GenerateRefreshToken(int userId, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                _logger.LogWarning("Refresh token creation attempted with null or empty IP address.");
                throw new ArgumentNullException(nameof(ipAddress), "IP address cannot be null or empty.");
            }

            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureToken(),
                UserId = userId,
                ExpirationTime = _dateTimeProvider.UtcNow.AddDays(30), // Refresh token valid for 30 days
                Created = _dateTimeProvider.UtcNow,
                CreatedByIp = ipAddress,
                // Revoked, RevokedByIp, ReplacedByToken are null by default
            };

            _logger.LogInformation("Refresh token generated successfully for UserId: {UserId}", userId);

            return refreshToken;
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken == null)
            {
                _logger.LogWarning("Attempted to save a null refresh token.");
                throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null.");
            }

            await _unitOfWork.GetRepository<RefreshToken>().AddAsync(refreshToken);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Refresh token saved successfully for UserId: {UserId}", refreshToken.UserId);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token, int userId)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Attempted to validate a null or empty refresh token.");
                return false;
            }

            var existingToken = await _unitOfWork.GetRepository<RefreshToken>().GetAsync(rt => rt.Token == token && rt.UserId == userId);

            if (existingToken == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", token);
                return false;
            }

            if (existingToken.ExpirationTime < _dateTimeProvider.UtcNow)
            {
                _logger.LogWarning("Refresh token expired: {Token}", token);
                return false;
            }

            if (existingToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token has been revoked: {Token}", token);
                return false;
            }

            _logger.LogInformation("Refresh token is valid: {Token}", token);
            return true;
        }

        public async Task InvalidateRefreshTokenAsync(string token, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Attempted to invalidate a null or empty refresh token.");
                throw new ArgumentNullException(nameof(token), "Token cannot be null or empty.");
            }

            var existingToken = await _unitOfWork.GetRepository<RefreshToken>().GetAsync(rt => rt.Token == token);
            if (existingToken == null)
            {
                _logger.LogWarning("Attempted to invalidate a non-existent refresh token: {Token}", token);
                return;
            }

            existingToken.Revoked = _dateTimeProvider.UtcNow;
            existingToken.RevokedByIp = ipAddress;

            _unitOfWork.GetRepository<RefreshToken>().Update(existingToken);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Refresh token invalidated: {Token}", token);
        }

        public async Task<AuthResponseDto> RefreshTokensAsync(string token, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Refresh token is null or empty.");
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            var existingToken = await _unitOfWork.GetRepository<RefreshToken>().GetAsync(rt => rt.Token == token);

            if (existingToken == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", token);
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            if (existingToken.ExpirationTime < _dateTimeProvider.UtcNow)
            {
                _logger.LogWarning("Refresh token expired: {Token}", token);
                return new AuthResponseDto { Success = false, Message = "Expired refresh token." };
            }

            if (existingToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token has been revoked: {Token}", token);
                return new AuthResponseDto { Success = false, Message = "Revoked refresh token." };
            }

            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(existingToken.UserId);
            if (user == null || !user.Status)
            {
                _logger.LogWarning("User not found or banned for Refresh Token: {Token}", token);
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            // Retrieve user roles
            var userRoles = await _unitOfWork.GetRepository<UserRole>()
                .FindAsync(ur => ur.UserId == user.UserId, includeProperties: "Role");
            var roleNames = userRoles.Select(ur => ur.Role.RoleName).ToList();

            // Generate new Access Token
            var newAccessToken = _jwtTokenService.GenerateJwtToken(user.UserId, user.Username, roleNames, user.Status);

            // Generate new Refresh Token
            var newRefreshToken = GenerateRefreshToken(user.UserId, ipAddress);

            // Invalidate the old Refresh Token
            existingToken.Revoked = _dateTimeProvider.UtcNow;
            existingToken.RevokedByIp = ipAddress;
            _unitOfWork.GetRepository<RefreshToken>().Update(existingToken);

            // Save the new Refresh Token
            await SaveRefreshTokenAsync(newRefreshToken);

            // Commit the changes
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Refresh token successfully refreshed for UserId: {UserId}", user.UserId);

            // Return the new tokens
            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        /// <summary>
        /// Generates a secure random token string.
        /// </summary>
        /// <returns>A secure, base64-encoded token string.</returns>
        private string GenerateSecureToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
