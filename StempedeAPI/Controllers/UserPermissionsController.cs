using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.DTOs.User;
using BusinessLogic.DTOs;

namespace StempedeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensures that only authenticated users can access the endpoints
    public class UserPermissionsController : ControllerBase
    {
        private readonly IUserPermissionService _userPermissionService;
        private readonly ILogger<UserPermissionsController> _logger;

        public UserPermissionsController(IUserPermissionService userPermissionService, ILogger<UserPermissionsController> logger)
        {
            _userPermissionService = userPermissionService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the current user's permissions.
        /// </summary>
        /// <returns>A list of permissions assigned to the user.</returns>
        /// <response code="200">Returns the list of permissions.</response>
        /// <response code="400">If the user is not found.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("get-current-user")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserPermissionDto>>>> GetCurrentUserPermissions()
        {
            // Retrieve the username from the authenticated user's claims
            var userName = User.Identity.Name;

            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("Authenticated user does not have a username.");
                return BadRequest(ApiResponse<string>.FailureResponse("User is not authenticated.", new List<string> { "User authentication failed." }));
            }

            try
            {
                var permissions = await _userPermissionService.GetCurrentUserPermissionsAsync(userName);

                if (permissions == null || !permissions.Any())
                {
                    return NotFound(ApiResponse<string>.FailureResponse("No permissions found for the user.", new List<string> { "User has no assigned permissions." }));
                }

                return Ok(ApiResponse<IEnumerable<UserPermissionDto>>.SuccessResponse(permissions, "Permissions retrieved successfully."));
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Error fetching permissions for user: {UserName}", userName);
                return BadRequest(ApiResponse<string>.FailureResponse(argEx.Message, new List<string> { argEx.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching permissions for user: {UserName}", userName);
                return StatusCode(500, ApiResponse<string>.FailureResponse("An unexpected error occurred.", new List<string> { "Internal server error." }));
            }
        }
    }
}
