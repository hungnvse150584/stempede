using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Auth.Services.Interfaces;
using BusinessLogic.DTOs.Auth;
using AutoMapper;
using BusinessLogic.Configurations;
using Microsoft.Extensions.Options;
using BusinessLogic.Auth.Helpers.Interfaces;
using System.Data;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Auth.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DatabaseSettings _dbSettings;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;
        private readonly IAssignMissingPermissions _assignMissingPermissions;

        public AuthService(
            IUnitOfWork unitOfWork,
            IRefreshTokenService refreshTokenService,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger,
            IMapper mapper,
            IOptions<DatabaseSettings> dbSettings,
            IAssignMissingPermissions assignMissingPermissions)
        {
            _unitOfWork = unitOfWork;
            _refreshTokenService = refreshTokenService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _mapper = mapper;
            _dbSettings = dbSettings.Value;
            _assignMissingPermissions = assignMissingPermissions;
        }

        public async Task<AuthResponseDto> RegisterAsync(UserRegistrationDto registrationDto, string ipAddress)
        {
            _logger.LogInformation("Starting registration process from IP: {IpAddress}", ipAddress);
            _logger.LogInformation("Using Database Collation: {Collation}", _dbSettings.Collation);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(registrationDto.Email) ||
                string.IsNullOrWhiteSpace(registrationDto.Username) ||
                string.IsNullOrWhiteSpace(registrationDto.Password))
            {
                return new AuthResponseDto { Success = false, Message = "Invalid registration data." };
            }

            // Check if user already exists (by Email or Username)
            var userExists = await _unitOfWork.GetRepository<User>().AnyAsync(u =>
                EF.Functions.Collate(u.Email, _dbSettings.Collation) == registrationDto.Email ||
                EF.Functions.Collate(u.Username, _dbSettings.Collation) == registrationDto.Username);

            if (userExists)
            {
                _logger.LogWarning("User already exists with the provided email or username.");
                return new AuthResponseDto { Success = false, Message = "User already exists." };
            }

            // Validate role
            var allowedRoles = new List<string> { "Customer", "Staff", "Manager" };
            if (string.IsNullOrWhiteSpace(registrationDto.Role) ||
                !allowedRoles.Contains(registrationDto.Role, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogError("Invalid or missing role provided.");
                return new AuthResponseDto { Success = false, Message = "Invalid or missing role provided." };
            }

            // Retrieve role from the database
            var role = await _unitOfWork.GetRepository<Role>()
                .GetAsync(r => EF.Functions.Collate(r.RoleName, _dbSettings.Collation) == registrationDto.Role);

            if (role == null)
            {
                _logger.LogError("Role not found in the database.");
                return new AuthResponseDto { Success = false, Message = "Role not found." };
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Create a new user

                    var user = _mapper.Map<User>(registrationDto);
                    
                    user.Password = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password, workFactor: 12);
                    user.Status = true;
                    user.IsExternal = registrationDto.IsExternal;
                    user.ExternalProvider = registrationDto.IsExternal ? registrationDto.ExternalProvider : null;

                    await _unitOfWork.GetRepository<User>().AddAsync(user);
                    await _unitOfWork.CompleteAsync();

                    // Assign Role to the User via UserRoles
                    var userRole = new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = role.RoleId
                    };
                    await _unitOfWork.GetRepository<UserRole>().AddAsync(userRole);

                    // Save role assignment
                    await _unitOfWork.CompleteAsync();

                    // Retrieve the corresponding permission
                    var permission = await _unitOfWork.GetRepository<Permission>().GetAsync(p => EF.Functions.Collate(p.PermissionName, _dbSettings.Collation) == registrationDto.Role);

                    if (permission == null)
                    {
                        _logger.LogError("Permission matching the role '{RoleName}' not found.", registrationDto.Role);
                        throw new InvalidOperationException("Corresponding permission not found.");
                    }

                    var existingUserPermission = await _unitOfWork.GetRepository<UserPermission>().FindAsync(up => up.UserId == user.UserId && up.PermissionId == permission.PermissionId);

                    if (existingUserPermission.Any())
                    {
                        _logger.LogWarning("User already has the '{PermissionName}' permission.", permission.PermissionName);
                    }
                    else
                    {
                        // Proceed to assign the permission
                        var userPermission = new UserPermission
                        {
                            UserId = user.UserId,
                            PermissionId = permission.PermissionId,
                            AssignedBy = user.UserId
                        };

                        await _unitOfWork.GetRepository<UserPermission>().AddAsync(userPermission);
                        await _unitOfWork.CompleteAsync();
                    }

                    // Generate JWT Access Token
                    var accessToken = _jwtTokenService.GenerateJwtToken(user.UserId, user.Username, new List<string> { role.RoleName }, user.Status);

                    // Generate Refresh Token
                    var refreshToken = _refreshTokenService.GenerateRefreshToken(user.UserId, ipAddress);

                    // Save the Refresh Token
                    await _refreshTokenService.SaveRefreshTokenAsync(refreshToken);

                    // Commit transaction
                    await _unitOfWork.CompleteAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation("User registration successful.");
                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Registration successful.",
                        Token = accessToken,
                        RefreshToken = refreshToken.Token
                    };
                }
                catch (SqlException ex)
                {
                    _logger.LogError(ex, "Database connection error.");
                    await transaction.RollbackAsync();
                    return new AuthResponseDto { Success = false, Message = "A database connection error occurred. Please try again later." };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred during registration.");
                    await transaction.RollbackAsync();
                    return new AuthResponseDto { Success = false, Message = "An unexpected error occurred. Please try again." };
                }
            }
        }

        public async Task<LoginResponseDto> LoginAsync(UserLoginDto loginDto, string ipAddress)
        {
            _logger.LogInformation("User login attempt from IP: {IpAddress}", ipAddress);
            _logger.LogInformation("Using Database Collation: {Collation}", _dbSettings.Collation);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(loginDto.EmailOrUsername) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return new LoginResponseDto { Success = false, Message = "Invalid login data." };
            }

            var emailOrUsername = loginDto.EmailOrUsername.Trim();

            try
            {
                var user = await _unitOfWork.GetRepository<User>().GetAsync(u =>
                    EF.Functions.Collate(u.Email, _dbSettings.Collation) == emailOrUsername ||
                    EF.Functions.Collate(u.Username, _dbSettings.Collation) == emailOrUsername);

                if (user == null || !user.Status)
                {
                    _logger.LogWarning("Invalid credentials or user is banned.");
                    return new LoginResponseDto { Success = false, Message = "Invalid credentials or user is banned." };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    _logger.LogWarning("Invalid credentials provided.");
                    return new LoginResponseDto { Success = false, Message = "Invalid credentials." };
                }

                // Retrieve user roles
                var userRoles = await _unitOfWork.GetRepository<UserRole>()
                    .FindAsync(ur => ur.UserId == user.UserId, includeProperties: "Role");

                var roleNames = userRoles.Select(ur => ur.Role.RoleName).ToList();

                // Assign missing permissions based on roles using helper
                await _assignMissingPermissions.AssignMissingPermissionsAsync(user.UserId, roleNames);

                // Generate JWT token
                var accessToken = _jwtTokenService.GenerateJwtToken(user.UserId, user.Username, roleNames, user.Status);

                // Generate Refresh Token
                var refreshToken = _refreshTokenService.GenerateRefreshToken(user.UserId, ipAddress);

                // Save the Refresh Token
                await _refreshTokenService.SaveRefreshTokenAsync(refreshToken);

                _logger.LogInformation("User login successful for UserId: {UserId}", user.UserId);

                return new LoginResponseDto
                {
                    Success = true,
                    Token = accessToken,
                    RefreshToken = refreshToken.Token,
                    Message = "Login successful.",
                    Roles = roleNames
                };
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Database connection error occurred during login.");
                return new LoginResponseDto { Success = false, Message = "Login failed due to a database connection issue." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                return new LoginResponseDto { Success = false, Message = "Login failed. Please try again." };
            }
        }

        public async Task<AuthResponseDto> LogoutAsync(string refreshToken, string ipAddress)
        {
            _logger.LogInformation("Logout attempt from IP: {IpAddress}", ipAddress);

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token is null or empty during logout.");
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            var existingRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

            if (existingRefreshToken == null)
            {
                _logger.LogWarning("Refresh token not found during logout: {Token}", refreshToken);
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            // Remove the Refresh Token via RefreshTokenService
            await _refreshTokenService.InvalidateRefreshTokenAsync(refreshToken, ipAddress);

            _logger.LogInformation("User logged out successfully for UserId: {UserId}", existingRefreshToken.UserId);

            return new AuthResponseDto { Success = true, Message = "Logout successful." };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token, string ipAddress)
        {
            _logger.LogInformation("Refresh token attempt from IP: {IpAddress}", ipAddress);

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Refresh token is null or empty.");
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            var refreshResponse = await _refreshTokenService.RefreshTokensAsync(token, ipAddress);

            return refreshResponse;
        }
    }
}

