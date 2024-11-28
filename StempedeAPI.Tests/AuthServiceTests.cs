using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using AutoMapper;
using Microsoft.Extensions.Options;
using FluentAssertions;
using System.Linq.Expressions;
using DataAccess.Data;
using DataAccess.Entities;
using BusinessLogic.Auth.Services.Interfaces;
using BusinessLogic.Auth.Services.Implementation;
using BusinessLogic.Configurations;
using BusinessLogic.Auth.Helpers.Interfaces;
using BusinessLogic.Configurations.MappingProfiles;
using BusinessLogic.DTOs.Auth;

namespace StempedeAPI.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly Mock<IOptions<DatabaseSettings>> _dbSettingsMock;
        private readonly Mock<IAssignMissingPermissions> _assignMissingPermissionsMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            _dbSettingsMock = new Mock<IOptions<DatabaseSettings>>();
            _assignMissingPermissionsMock = new Mock<IAssignMissingPermissions>();

            // Configure AutoMapper with real mappings
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _dbSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
            {
                Collation = "SQL_Latin1_General_CP1_CI_AS"
            });

            _authService = new AuthService(
                _unitOfWorkMock.Object,
                _refreshTokenServiceMock.Object,
                _jwtTokenServiceMock.Object,
                _loggerMock.Object,
                _mapper,
                _dbSettingsMock.Object,
                _assignMissingPermissionsMock.Object
            );
        }

    //    [Fact]
    //    public async Task RegisterAsync_ShouldRegisterUserSuccessfully()
    //    {
    //        // Arrange
    //        var registrationDto = new UserRegistrationDto
    //        {
    //            Email = "test@example.com",
    //            Username = "testuser",
    //            Password = "SecurePassword123!",
    //            Role = "Customer",
    //            IsExternal = false
    //        };
    //        string ipAddress = "127.0.0.1";

    //        // Mock user existence check
    //        _unitOfWorkMock.Setup(uow => uow.GetRepository<User>()
    //.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
    //.ReturnsAsync(false);

    //        // Mock role retrieval
    //        var role = new Role { RoleId = 1, RoleName = "Customer" };
    //        _unitOfWorkMock.Setup(uow => uow.GetRepository<Role>()
    //            .GetAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<string>()))
    //            .ReturnsAsync(role);

    //        // Variable to capture the User object passed to AddAsync
    //        User capturedUser = null;

    //        // Mock repository Add and CompleteAsync
    //        _unitOfWorkMock.Setup(uow => uow.GetRepository<User>().AddAsync(It.IsAny<User>()))
    //            .Callback<User>(u => capturedUser = u) // Capture the User object
    //            .Returns(Task.CompletedTask);
    //        _unitOfWorkMock.Setup(uow => uow.CompleteAsync())
    //            .ReturnsAsync(1);
    //        _unitOfWorkMock.Setup(uow => uow.GetRepository<UserRole>().AddAsync(It.IsAny<UserRole>()))
    //            .Returns(Task.CompletedTask);

    //        // Mock JWT and Refresh Token generation
    //        _jwtTokenServiceMock.Setup(jwt => jwt.GenerateJwtToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
    //            .Returns("fake-jwt-token");
    //        var refreshToken = new RefreshToken { Token = "fake-refresh-token" };
    //        _refreshTokenServiceMock.Setup(rt => rt.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<string>()))
    //            .Returns(refreshToken);
    //        _refreshTokenServiceMock.Setup(rt => rt.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()))
    //            .Returns(Task.CompletedTask);

    //        // Mock transaction
    //        var transactionMock = new Mock<IDbContextTransaction>();
    //        _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync())
    //            .ReturnsAsync(transactionMock.Object);

    //        // Act
    //        var result = await _authService.RegisterAsync(registrationDto, ipAddress);

    //        // Assert
    //        result.Should().NotBeNull();
    //        result.Success.Should().BeTrue();
    //        result.Message.Should().Be("Registration successful.");
    //        result.Token.Should().Be("fake-jwt-token");
    //        result.RefreshToken.Should().Be("fake-refresh-token");

    //        // Ensure that the User object was captured
    //        capturedUser.Should().NotBeNull();
    //        capturedUser.Email.Should().Be(registrationDto.Email);
    //        capturedUser.Username.Should().Be(registrationDto.Username);
    //        capturedUser.Status.Should().BeTrue();
    //        capturedUser.IsExternal.Should().Be(registrationDto.IsExternal);

    //        // Verify that the password was hashed correctly
    //        BCrypt.Net.BCrypt.Verify(registrationDto.Password, capturedUser.Password).Should().BeTrue();

    //        // Verify that role was assigned
    //        _unitOfWorkMock.Verify(uow => uow.GetRepository<UserRole>().AddAsync(It.Is<UserRole>(ur =>
    //            ur.UserId == capturedUser.UserId && ur.RoleId == role.RoleId)), Times.Once);

    //        // Verify that tokens were generated and saved
    //        _jwtTokenServiceMock.Verify(jwt => jwt.GenerateJwtToken(capturedUser.UserId, capturedUser.Username, It.Is<List<string>>(roles => roles.Contains("Customer")), capturedUser.Status), Times.Once);
    //        _refreshTokenServiceMock.Verify(rt => rt.GenerateRefreshToken(capturedUser.UserId, ipAddress), Times.Once);
    //        _refreshTokenServiceMock.Verify(rt => rt.SaveRefreshTokenAsync(refreshToken), Times.Once);

    //        // Verify transaction commit
    //        transactionMock.Verify(t => t.CommitAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    //    }

        [Fact]
        public async Task RegisterAsync_ShouldHandleDatabaseException()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto
            {
                Email = "new@example.com",
                Username = "newuser",
                Password = "SecurePassword123!",
                Role = "Customer",
                IsExternal = false
            };
            string ipAddress = "127.0.0.1";

            // Mock user existence check to return false
            _unitOfWorkMock.Setup(uow => uow.GetRepository<User>()
    .AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
    .ReturnsAsync(false);

            // Mock role retrieval
            var role = new Role { RoleId = 1, RoleName = "Customer" };
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Role>()
                .GetAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(role);

            // Variable to capture the User object passed to AddAsync
            User capturedUser = null;

            // Mock repository Add to throw a generic exception instead of SqlException
            _unitOfWorkMock.Setup(uow => uow.GetRepository<User>().AddAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u) // Capture the User object
                .ThrowsAsync(new Exception("Database connection error."));

            // Mock transaction
            var transactionMock = new Mock<IDbContextTransaction>();
            _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            // Act
            var result = await _authService.RegisterAsync(registrationDto, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected error occurred. Please try again.");

            // Verify that transaction was rolled back
            transactionMock.Verify(t => t.RollbackAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldLoginSuccessfully()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                EmailOrUsername = "testuser",
                Password = "ValidPassword123!"
            };
            string ipAddress = "127.0.0.1";

            var user = new User
            {
                UserId = 1,
                Email = "test@example.com",
                Username = "testuser",
                Password = BCrypt.Net.BCrypt.HashPassword(loginDto.Password),
                Status = true
            };

            // Mock user retrieval
            _unitOfWorkMock.Setup(uow => uow.GetRepository<User>()
                .GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(user);

            // Mock user roles
            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = user.UserId, RoleId = 1, Role = new Role { RoleName = "Customer" } }
            };
            _unitOfWorkMock.Setup(uow => uow.GetRepository<UserRole>()
                .FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), "Role"))
                .ReturnsAsync(userRoles);

            // Variable to capture the UserRole object passed to FindAsync if needed

            // Mock JWT and Refresh Token generation
            _jwtTokenServiceMock.Setup(jwt => jwt.GenerateJwtToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Returns("fake-jwt-token");
            var refreshToken = new RefreshToken { Token = "fake-refresh-token" };
            _refreshTokenServiceMock.Setup(rt => rt.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(refreshToken);
            _refreshTokenServiceMock.Setup(rt => rt.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(loginDto, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Login successful.");
            result.Token.Should().Be("fake-jwt-token");
            result.RefreshToken.Should().Be("fake-refresh-token");

            // Verify that user was retrieved
            _unitOfWorkMock.Verify(uow => uow.GetRepository<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()), Times.Once);

            // Verify that roles were retrieved
            _unitOfWorkMock.Verify(uow => uow.GetRepository<UserRole>().FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), "Role"), Times.Once);

            // Verify that tokens were generated and saved
            _jwtTokenServiceMock.Verify(jwt => jwt.GenerateJwtToken(user.UserId, user.Username, It.Is<List<string>>(roles => roles.Contains("Customer")), user.Status), Times.Once);
            _refreshTokenServiceMock.Verify(rt => rt.GenerateRefreshToken(user.UserId, ipAddress), Times.Once);
            _refreshTokenServiceMock.Verify(rt => rt.SaveRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                EmailOrUsername = "nonexistentuser",
                Password = "WrongPassword!"
            };
            string ipAddress = "127.0.0.1";

            // Mock user retrieval to return null
            _unitOfWorkMock.Setup(uow => uow.GetRepository<User>()
                .GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.LoginAsync(loginDto, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid credentials or user is banned.");

            // Verify that user was retrieved
            _unitOfWorkMock.Verify(uow => uow.GetRepository<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()), Times.Once);

            // Verify that no tokens were generated
            _jwtTokenServiceMock.Verify(jwt => jwt.GenerateJwtToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Never);
            _refreshTokenServiceMock.Verify(rt => rt.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _refreshTokenServiceMock.Verify(rt => rt.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenUserIsBanned()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                EmailOrUsername = "banneduser",
                Password = "ValidPassword123!"
            };
            string ipAddress = "127.0.0.1";

            var user = new User
            {
                UserId = 2,
                Email = "banned@example.com",
                Username = "banneduser",
                Password = BCrypt.Net.BCrypt.HashPassword(loginDto.Password),
                Status = false // User is banned
            };

            // Mock user retrieval
            _unitOfWorkMock.Setup(uow => uow.GetRepository<User>()
                .GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid credentials or user is banned.");

            // Verify that user was retrieved
            _unitOfWorkMock.Verify(uow => uow.GetRepository<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()), Times.Once);

            // Verify that no tokens were generated
            _jwtTokenServiceMock.Verify(jwt => jwt.GenerateJwtToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Never);
            _refreshTokenServiceMock.Verify(rt => rt.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _refreshTokenServiceMock.Verify(rt => rt.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldHandleDatabaseException()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                EmailOrUsername = "testuser",
                Password = "ValidPassword123!"
            };
            string ipAddress = "127.0.0.1";

            // Mock user retrieval to throw a generic exception instead of SqlException
            _unitOfWorkMock.Setup(uow => uow.GetRepository<User>()
                .GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database connection error."));

            // Act
            var result = await _authService.LoginAsync(loginDto, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Login failed. Please try again.");

            // Verify that user was retrieved
            _unitOfWorkMock.Verify(uow => uow.GetRepository<User>().GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()), Times.Once);

            // Verify that no tokens were generated
            _jwtTokenServiceMock.Verify(jwt => jwt.GenerateJwtToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Never);
            _refreshTokenServiceMock.Verify(rt => rt.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _refreshTokenServiceMock.Verify(rt => rt.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_ShouldLogoutSuccessfully()
        {
            // Arrange
            string refreshToken = "valid-refresh-token";
            string ipAddress = "127.0.0.1";

            var existingRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                UserId = 1
            };

            // Mock refresh token retrieval
            _unitOfWorkMock.Setup(uow => uow.RefreshTokens.GetByTokenAsync(refreshToken))
                .ReturnsAsync(existingRefreshToken);

            // Mock InvalidateRefreshTokenAsync
            _refreshTokenServiceMock.Setup(rt => rt.InvalidateRefreshTokenAsync(refreshToken, ipAddress))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LogoutAsync(refreshToken, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Logout successful.");

            // Verify that refresh token was retrieved
            _unitOfWorkMock.Verify(uow => uow.RefreshTokens.GetByTokenAsync(refreshToken), Times.Once);

            // Verify that refresh token was invalidated
            _refreshTokenServiceMock.Verify(rt => rt.InvalidateRefreshTokenAsync(refreshToken, ipAddress), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldFail_WhenRefreshTokenIsNullOrEmpty()
        {
            // Arrange
            string refreshToken = "";
            string ipAddress = "127.0.0.1";

            // Act
            var result = await _authService.LogoutAsync(refreshToken, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid refresh token.");

            // Verify that no further operations were performed
            _unitOfWorkMock.Verify(uow => uow.RefreshTokens.GetByTokenAsync(It.IsAny<string>()), Times.Never);
            _refreshTokenServiceMock.Verify(rt => rt.InvalidateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_ShouldFail_WhenRefreshTokenNotFound()
        {
            // Arrange
            string refreshToken = "nonexistent-token";
            string ipAddress = "127.0.0.1";

            // Mock refresh token retrieval to return null
            _unitOfWorkMock.Setup(uow => uow.RefreshTokens.GetByTokenAsync(refreshToken))
                .ReturnsAsync((RefreshToken)null);

            // Act
            var result = await _authService.LogoutAsync(refreshToken, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid refresh token.");

            // Verify that refresh token was retrieved
            _unitOfWorkMock.Verify(uow => uow.RefreshTokens.GetByTokenAsync(refreshToken), Times.Once);

            // Verify that InvalidateRefreshTokenAsync was not called
            _refreshTokenServiceMock.Verify(rt => rt.InvalidateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldRefreshTokensSuccessfully()
        {
            // Arrange
            string token = "valid-refresh-token";
            string ipAddress = "127.0.0.1";

            var existingToken = new RefreshToken
            {
                Token = token,
                UserId = 1,
                ExpirationTime = DateTime.UtcNow.AddDays(1),
                Revoked = null,
                RevokedByIp = null,
                ReplacedByToken = null,
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            var user = new User
            {
                UserId = existingToken.UserId,
                Status = true
            };

            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = user.UserId, RoleId = 1, Role = new Role { RoleName = "Customer" } }
            };

            var newAccessToken = "new-jwt-token";
            var newRefreshToken = new RefreshToken { Token = "new-refresh-token" };

            // Mock RefreshTokensAsync to handle the refresh logic
            _refreshTokenServiceMock.Setup(rt => rt.RefreshTokensAsync(token, ipAddress))
                .ReturnsAsync(new AuthResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken.Token
                });

            // Act
            var result = await _authService.RefreshTokenAsync(token, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Token refreshed successfully.");
            result.Token.Should().Be(newAccessToken);
            result.RefreshToken.Should().Be(newRefreshToken.Token);

            // Verify that RefreshTokensAsync was called
            _refreshTokenServiceMock.Verify(rt => rt.RefreshTokensAsync(token, ipAddress), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldFail_WhenTokenIsNullOrEmpty()
        {
            // Arrange
            string token = "";
            string ipAddress = "127.0.0.1";

            // Act
            var result = await _authService.RefreshTokenAsync(token, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid refresh token.");

            // Verify that RefreshTokensAsync was not called
            _refreshTokenServiceMock.Verify(rt => rt.RefreshTokensAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldFail_WhenRefreshTokenIsExpired()
        {
            // Arrange
            string token = "expired-refresh-token";
            string ipAddress = "127.0.0.1";

            var existingToken = new RefreshToken
            {
                Token = token,
                UserId = 1,
                ExpirationTime = DateTime.UtcNow.AddDays(-1), // Expired
                Revoked = null,
                RevokedByIp = null,
                ReplacedByToken = null,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedByIp = ipAddress
            };

            // Mock RefreshTokensAsync to handle the expired token scenario
            _refreshTokenServiceMock.Setup(rt => rt.RefreshTokensAsync(token, ipAddress))
                .ReturnsAsync(new AuthResponseDto
                {
                    Success = false,
                    Message = "Expired refresh token."
                });

            // Act
            var result = await _authService.RefreshTokenAsync(token, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Expired refresh token.");

            // Verify that RefreshTokensAsync was called
            _refreshTokenServiceMock.Verify(rt => rt.RefreshTokensAsync(token, ipAddress), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldFail_WhenRefreshTokenIsRevoked()
        {
            // Arrange
            string token = "revoked-refresh-token";
            string ipAddress = "127.0.0.1";

            var existingToken = new RefreshToken
            {
                Token = token,
                UserId = 1,
                ExpirationTime = DateTime.UtcNow.AddDays(1),
                Revoked = DateTime.UtcNow.AddDays(-1), // Revoked
                RevokedByIp = ipAddress,
                ReplacedByToken = null,
                Created = DateTime.UtcNow.AddDays(-5),
                CreatedByIp = ipAddress
            };

            // Mock RefreshTokensAsync to handle the revoked token scenario
            _refreshTokenServiceMock.Setup(rt => rt.RefreshTokensAsync(token, ipAddress))
                .ReturnsAsync(new AuthResponseDto
                {
                    Success = false,
                    Message = "Revoked refresh token."
                });

            // Act
            var result = await _authService.RefreshTokenAsync(token, ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Revoked refresh token.");

            // Verify that RefreshTokensAsync was called
            _refreshTokenServiceMock.Verify(rt => rt.RefreshTokensAsync(token, ipAddress), Times.Once);
        }
    }
}