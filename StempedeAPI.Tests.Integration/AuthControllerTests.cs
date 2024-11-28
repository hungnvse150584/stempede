using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Stemkit.DTOs.Auth;
using Stemkit.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace Stemkit.Tests.Integration
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "SecurePassword123!",
                FullName = "Test User",
                Phone = "123-456-7890",
                Address = "123 Test St",
                Role = "Customer",
                IsExternal = false
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(registrationDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(responseString);

            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Message.Should().Be("Registration successful.");
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenDataIsInvalid()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto
            {
                // Missing required fields like Email and Password
                Username = "tu",
                Email = "invalid-email",
                Password = "123", // Too short
                FullName = "",
                Phone = "invalid-phone",
                Address = "",
                Role = "InvalidRole",
                IsExternal = false
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(registrationDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(responseString);

            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().Be("Invalid registration data.");
            apiResponse.Errors.Should().Contain("The Email field is not a valid e-mail address.");
            apiResponse.Errors.Should().Contain("The field Password must be a string or array type with a minimum length of '6'.");
        }

        [Fact]
        public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            // First, register a user
            var registrationDto = new UserRegistrationDto
            {
                Username = "loginuser",
                Email = "loginuser@example.com",
                Password = "LoginPassword123!",
                FullName = "Login User",
                Phone = "987-654-3210",
                Address = "456 Login Ave",
                Role = "Customer",
                IsExternal = false
            };

            var registerContent = new StringContent(
                JsonConvert.SerializeObject(registrationDto),
                Encoding.UTF8,
                "application/json");

            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Prepare login DTO
            var loginDto = new UserLoginDto
            {
                EmailOrUsername = "loginuser",
                Password = "LoginPassword123!"
            };

            var loginContent = new StringContent(
                JsonConvert.SerializeObject(loginDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<AuthResponse>>(responseString);

            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Message.Should().Be("Login successful.");
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data.Token.Should().NotBeEmpty();
            apiResponse.Data.RefreshToken.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                EmailOrUsername = "nonexistentuser",
                Password = "WrongPassword!"
            };

            var loginContent = new StringContent(
                JsonConvert.SerializeObject(loginDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(responseString);

            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().Be("Invalid credentials or user is banned.");
        }

        [Fact]
        public async Task Refresh_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            // First, register and login a user to obtain tokens
            var registrationDto = new UserRegistrationDto
            {
                Username = "refreshtestuser",
                Email = "refreshtestuser@example.com",
                Password = "RefreshPassword123!",
                FullName = "Refresh Test User",
                Phone = "555-555-5555",
                Address = "789 Refresh Blvd",
                Role = "Customer",
                IsExternal = false
            };

            var registerContent = new StringContent(
                JsonConvert.SerializeObject(registrationDto),
                Encoding.UTF8,
                "application/json");

            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Login to get tokens
            var loginDto = new UserLoginDto
            {
                EmailOrUsername = "refreshtestuser",
                Password = "RefreshPassword123!"
            };

            var loginContent = new StringContent(
                JsonConvert.SerializeObject(loginDto),
                Encoding.UTF8,
                "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
            var loginApiResponse = JsonConvert.DeserializeObject<ApiResponse<AuthResponse>>(loginResponseString);
            loginApiResponse.Data.Should().NotBeNull();

            // Prepare refresh request
            var refreshRequestDto = new RefreshRequestDto
            {
                RefreshToken = loginApiResponse.Data.RefreshToken
            };

            var refreshContent = new StringContent(
                JsonConvert.SerializeObject(refreshRequestDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/refresh", refreshContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var newTokens = JsonConvert.DeserializeObject<dynamic>(responseString);

            ((bool)newTokens.success).Should().BeTrue();
            ((string)newTokens.accessToken).Should().NotBeNullOrEmpty();
            ((string)newTokens.refreshToken).Should().NotBeNullOrEmpty();
            ((string)newTokens.message).Should().Be("Token refreshed successfully.");
        }

        [Fact]
        public async Task Refresh_ShouldReturnBadRequest_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var refreshRequestDto = new RefreshRequestDto
            {
                RefreshToken = "invalid-refresh-token"
            };

            var refreshContent = new StringContent(
                JsonConvert.SerializeObject(refreshRequestDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/refresh", refreshContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(responseString);

            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().Be("Invalid refresh token.");
        }


    }
}