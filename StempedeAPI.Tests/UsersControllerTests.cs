using BusinessLogic.DTOs.User;
using BusinessLogic.DTOs;
using DataAccess.Data;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StempedeAPI.Controllers;
using StempedeAPI.Tests.Helpers;
using System.Security.Claims;

namespace StempedeAPI.Tests
{
    public class UsersControllerTests
    {
        // Mocks
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;

        // Controller under test
        private readonly UsersController _usersController;

        public UsersControllerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepositoryMock = new Mock<IGenericRepository<User>>();

            // Setup IUnitOfWork to return the mock repository for User
            _unitOfWorkMock.Setup(uow => uow.GetRepository<User>()).Returns(_userRepositoryMock.Object);

            _usersController = new UsersController(_unitOfWorkMock.Object);
        }

        // Helper method to set user claims
        private void SetUserClaims(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // Helper method to set unauthenticated user
        private void SetUnauthenticatedUser()
        {
            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task GetUserProfile_ValidUserId_ReturnsUserProfile()
        {
            // Arrange
            int userId = 1;
            SetUserClaims(userId);

            var user = new User
            {
                UserId = userId,
                FullName = "John Doe",
                Username = "johndoe",
                Email = "johndoe@example.com",
                Phone = "123-456-7890",
                Address = "123 Main St",
                Status = true
            };

            _userRepositoryMock.SetupGetByIdAsync(userId, user);

            // Act
            var result = await _usersController.GetUserProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserProfileDto>>(okResult.Value);

            Assert.True(apiResponse.Success);
            Assert.Equal("User profile retrieved successfully.", apiResponse.Message);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(user.UserId, apiResponse.Data.UserId);
            Assert.Equal(user.FullName, apiResponse.Data.FullName);
            Assert.Equal(user.Username, apiResponse.Data.Username);
            Assert.Equal(user.Email, apiResponse.Data.Email);
            Assert.Equal(user.Phone, apiResponse.Data.Phone);
            Assert.Equal(user.Address, apiResponse.Data.Address);
            Assert.Equal(user.Status, apiResponse.Data.Status);
        }

        [Fact]
        public async Task GetUserProfile_MissingUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetUnauthenticatedUser();

            // Act
            var result = await _usersController.GetUserProfile();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(unauthorizedResult.Value);

            Assert.False(apiResponse.Success);
            Assert.Equal("Invalid user ID.", apiResponse.Message);
            Assert.Null(apiResponse.Data);
        }

        [Fact]
        public async Task GetUserProfile_InvalidUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            // Manually set invalid user ID claim (non-integer)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid_id")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _usersController.GetUserProfile();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(unauthorizedResult.Value);

            Assert.False(apiResponse.Success);
            Assert.Equal("Invalid user ID.", apiResponse.Message);
            Assert.Null(apiResponse.Data);
        }

        [Fact]
        public async Task GetUserProfile_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            int userId = 2;
            SetUserClaims(userId);

            // Setup repository to return null (user not found)
            _userRepositoryMock.SetupGetByIdAsync(userId, (User)null);

            // Act
            var result = await _usersController.GetUserProfile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);

            Assert.False(apiResponse.Success);
            Assert.Equal("User not found.", apiResponse.Message);
            Assert.Null(apiResponse.Data);
        }

        [Fact]
        public async Task GetUserProfile_RepositoryThrowsException_ReturnsServerError()
        {
            // Arrange
            int userId = 3;
            SetUserClaims(userId);

            // Setup repository to throw an exception
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _usersController.GetUserProfile();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(objectResult.Value);

            Assert.False(apiResponse.Success);
            Assert.Equal("An error occurred while retrieving the profile.", apiResponse.Message);
            Assert.Null(apiResponse.Data);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}

