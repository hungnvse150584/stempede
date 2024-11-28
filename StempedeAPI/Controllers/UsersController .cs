using BusinessLogic.DTOs;
using BusinessLogic.DTOs.User;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace StempedeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Only authenticated users can access these endpoints
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves current users information.
        /// </summary>
        /// <returns>An ApiResponse to view user profile.</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid user ID."
                });
            }

            try
            {
                var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                var userProfile = new UserProfileDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Username = user.Username,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Status = user.Status
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Data = userProfile,
                    Message = "User profile retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the profile."
                });
            }
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid profile data."
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid user ID."
                });
            }

            try
            {
                var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Update fields
                user.FullName = updateDto.FullName ?? user.FullName;
                user.Username = updateDto.Username ?? user.Username;
                user.Phone = updateDto.Phone ?? user.Phone;
                user.Address = updateDto.Address ?? user.Address;

                _unitOfWork.GetRepository<User>().Update(user);
                await _unitOfWork.CompleteAsync();

                var updatedUserProfile = new UserProfileDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Username = user.Username,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Status = user.Status
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Data = updatedUserProfile,
                    Message = "User profile updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while updating the profile. Please try again later."
                });
            }
        }
    }
}
