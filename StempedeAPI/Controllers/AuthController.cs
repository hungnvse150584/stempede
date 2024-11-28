using BusinessLogic.Auth.Services.Interfaces;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace StempedeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IExternalAuthService _externalAuthService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IExternalAuthService externalAuthService)
        {
            _externalAuthService = externalAuthService;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            var ipAddress = GetClientIpAddress();

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid registration data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var result = await _authService.RegisterAsync(registrationDto, ipAddress);
            if (!result.Success)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = result.Message
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            var ipAddress = GetClientIpAddress();

            _logger.LogInformation("Login endpoint called from IP: {IpAddress}", ipAddress);


            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Validation errors occurred.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var result = await _authService.LoginAsync(loginDto, ipAddress);
            if (!result.Success)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = result.Message
                });
            }
            var loginResponse = new LoginResponseDto
            {
                Success = true,
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                Message = "Login successful.",
                Roles = result.Roles,
            };

            return Ok(new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Data = loginResponse,
                Message = loginResponse.Message
            });
        }

        [HttpGet("login-google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] ExternalAuthDto externalAuth)
        {
            var ipAddress = GetClientIpAddress();

            if (string.IsNullOrEmpty(externalAuth.IdToken))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "ID token is required." });
            }

            var authResponse = await _externalAuthService.GoogleLoginAsync(externalAuth.IdToken, ipAddress);

            if (!authResponse.Success)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto logoutRequest)
        {
            var ipAddress = GetClientIpAddress();

            if (logoutRequest == null || string.IsNullOrEmpty(logoutRequest.RefreshToken))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Invalid refresh token."
                });
            }

            var result = await _authService.LogoutAsync(logoutRequest.RefreshToken, ipAddress);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = true,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = result.Message
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto refreshRequest)
        {
            var ipAddress = GetClientIpAddress();

            if (refreshRequest == null || string.IsNullOrEmpty(refreshRequest.RefreshToken))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Invalid refresh token."
                });
            }

            var result = await _authService.RefreshTokenAsync(refreshRequest.RefreshToken, ipAddress);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = true,
                    Message = result.Message
                });
            }

            return Ok(new
            {
                accessToken = result.Token,
                refreshToken = result.RefreshToken,
                message = result.Message
            });
        }

        private string GetClientIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(ip))
                {
                    return ip.Split(',').First().Trim();
                }
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
