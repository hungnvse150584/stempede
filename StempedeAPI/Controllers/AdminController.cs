using BusinessLogic.DTOs.User;
using BusinessLogic.DTOs;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Utils.Implementation;

namespace StempedeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure only Managers can access these endpoints
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public AdminController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of all users with non-sensitive information.
        /// </summary>
        /// <returns>An ApiResponse containing the list of users.</returns>
        /// <response code="200">Returns the list of users.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("get-all-user")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReadUserDto>>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                return Ok(new ApiResponse<IEnumerable<ReadUserDto>>
                {
                    Success = true,
                    Data = users,
                    Message = "Users retrieved successfully.",
                    Errors = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users.");
                return StatusCode(500, new ApiResponse<IEnumerable<ReadUserDto>>
                {
                    Success = false,
                    Data = null,
                    Message = "An error occurred while retrieving users.",
                    Errors = new List<string> { "Internal server error." }
                });
            }
        }

        /// <summary>
        /// Retrieves a paginated list of users.
        /// </summary>
        /// <param name="queryParameters">Pagination parameters.</param>
        /// <returns>An ApiResponse containing a paginated list of users.</returns>
        /// <response code="200">Returns the paginated list of users.</response>
        /// <response code="400">If the query parameters are invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("get-all-users-pagination")]
        public async Task<ActionResult<ApiResponse<PaginatedList<ReadUserDto>>>> GetAllUsersPaginated([FromQuery] QueryParameters queryParameters)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid query parameters for GetAllUsersPaginated.");
                return BadRequest(new ApiResponse<PaginatedList<ReadUserDto>>
                {
                    Success = false,
                    Message = "Invalid query parameters.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                    Data = null
                });
            }

            try
            {
                var paginatedUsers = await _userService.GetAllUsersPaginatedAsync(queryParameters);
                var response = new ApiResponse<PaginatedList<ReadUserDto>>
                {
                    Success = true,
                    Data = paginatedUsers,
                    Message = "Users retrieved successfully.",
                    Errors = null
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving paginated users.");
                return StatusCode(500, new ApiResponse<PaginatedList<ReadUserDto>>
                {
                    Success = false,
                    Data = null,
                    Message = "An error occurred while retrieving paginated users.",
                    Errors = new List<string> { "Internal server error." }
                });
            }
        }

        /// <summary>
        /// Bans a user by setting their status to false.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to ban.</param>
        /// <returns>An ApiResponse indicating success or failure.</returns>
        /// <response code="200">User has been banned successfully.</response>
        /// <response code="400">If the user is not found or already banned.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("{userId}/ban")]
        public async Task<IActionResult> BanUser(int userId)
        {
            _logger.LogInformation("Manager {ManagerId} is attempting to ban UserId: {UserId}", User.Identity.Name, userId);

            try
            {
                var resultMessage = await _userService.BanUserAsync(userId);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = resultMessage,
                    Message = "User has been banned successfully.",
                    Errors = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("BanUserAsync failed for UserId: {UserId}. Reason: {Reason}", userId, ex.Message);
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while banning UserId: {UserId}.", userId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "An error occurred while banning the user.",
                    Errors = new List<string> { "Internal server error." }
                });
            }
        }

        /// <summary>
        /// Unbans a user by setting their status to true.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to unban.</param>
        /// <returns>An ApiResponse indicating success or failure.</returns>
        /// <response code="200">User has been unbanned successfully.</response>
        /// <response code="400">If the user is not found or not banned.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("{userId}/unban")]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            _logger.LogInformation("Manager {ManagerId} is attempting to unban UserId: {UserId}", User.Identity.Name, userId);

            try
            {
                var resultMessage = await _userService.UnbanUserAsync(userId);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = resultMessage,
                    Message = "User has been unbanned successfully.",
                    Errors = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("UnbanUserAsync failed for UserId: {UserId}. Reason: {Reason}", userId, ex.Message);
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while unbanning UserId: {UserId}.", userId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "An error occurred while unbanning the user.",
                    Errors = new List<string> { "Internal server error." }
                });
            }
        }
    }
}
