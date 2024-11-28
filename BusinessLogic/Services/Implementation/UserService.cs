using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.DTOs.User;
using BusinessLogic.Services.Interfaces;
using AutoMapper.QueryableExtensions;
using BusinessLogic.Utils.Implementation;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.Extensions.Logging;
using BusinessLogic.DTOs;

namespace BusinessLogic.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var userRepository = _unitOfWork.GetRepository<User>();
            return await userRepository.GetAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required.");

            var userRepository = _unitOfWork.GetRepository<User>();
            await userRepository.AddAsync(user);
            await _unitOfWork.CompleteAsync();
            return user;
        }

        public async Task AssignRoleAsync(int userId, string roleName)
        {
            var roleRepository = _unitOfWork.GetRepository<Role>();
            var userRoleRepository = _unitOfWork.GetRepository<UserRole>();

            var role = await roleRepository.GetAsync(r => r.RoleName == roleName);
            if (role == null)
            {
                throw new Exception("Role not found.");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.RoleId
            };

            await userRoleRepository.AddAsync(userRole);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var userRoleRepository = _unitOfWork.GetRepository<UserRole>();
            var roleRepository = _unitOfWork.GetRepository<Role>();

            var userRoles = await userRoleRepository.FindAsync(ur => ur.UserId == userId);
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roles = await roleRepository.FindAsync(r => roleIds.Contains(r.RoleId));

            return roles.Select(r => r.RoleName).ToList();
        }

        /// <summary>
        /// Retrieves all users without pagination.
        /// </summary>
        /// <returns>An ApiResponse containing the list of users.</returns>
        public async Task<IEnumerable<ReadUserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.GetRepository<User>()
                                         .GetAllQueryable(includeProperties: "UserRoles.Role")
                                         .AsNoTracking()
                                         .ToListAsync();

            var userDtos = _mapper.Map<IEnumerable<ReadUserDto>>(users);

            return userDtos;
        }

        /// <summary>
        /// Retrieves users with pagination.
        /// </summary>
        /// <param name="queryParameters">Parameters for pagination.</param>
        /// <returns>An ApiResponse containing a paginated list of users.</returns>
        public async Task<PaginatedList<ReadUserDto>> GetAllUsersPaginatedAsync(QueryParameters queryParameters)
        {
            var userRepository = _unitOfWork.GetRepository<User>();
            var usersQuery = userRepository.GetAllQueryable(includeProperties: "UserRoles.Role");

            // Project to ReadUserDto
            var projectedQuery = usersQuery.ProjectTo<ReadUserDto>(_mapper.ConfigurationProvider);

            // Create the paginated list
            var paginatedList = await PaginatedList<ReadUserDto>.CreateAsync(
                projectedQuery,
                queryParameters.PageNumber,
                queryParameters.PageSize
            );

            return paginatedList;
        }

        /// <summary>
        /// Retrieves a user by their unique ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The user object if found; otherwise, null.</returns>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);
        }

        /// <summary>
        /// Bans a user by setting their status to false.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to ban.</param>
        /// <returns>A success message indicating the user has been banned.</returns>
        /// <exception cref="ArgumentException">Thrown when the user is not found or already banned.</exception>
        public async Task<string> BanUserAsync(int userId)
        {
            _logger.LogInformation("Attempting to ban UserId: {UserId}", userId);

            var userRepository = _unitOfWork.GetRepository<User>();
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                throw new ArgumentException("User not found.");
            }

            if (!user.Status)
            {
                _logger.LogWarning("User with ID {UserId} is already banned.", userId);
                throw new ArgumentException("User is already banned.");
            }

            user.Status = false; // Set status to false to ban the user
            userRepository.Update(user);
            await _unitOfWork.CompleteAsync();

            // Revoke all refresh tokens for this user
            var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken>();
            var refreshTokens = await refreshTokenRepository.FindAsync(rt => rt.UserId == userId && rt.Revoked == null);

            foreach (var token in refreshTokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = "System"; // Or your system's IP
                refreshTokenRepository.Update(token);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User with ID {UserId} has been banned and all refresh tokens revoked.", userId);
            return "User has been banned successfully and all tokens revoked.";
        }

        /// <summary>
        /// Unbans a user by setting their status to true.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to unban.</param>
        /// <returns>A success message indicating the user has been unbanned.</returns>
        /// <exception cref="ArgumentException">Thrown when the user is not found or not banned.</exception>
        public async Task<string> UnbanUserAsync(int userId)
        {
            _logger.LogInformation("Attempting to unban UserId: {UserId}", userId);

            var userRepository = _unitOfWork.GetRepository<User>();
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                throw new ArgumentException("User not found.");
            }

            if (user.Status)
            {
                _logger.LogWarning("User with ID {UserId} is not banned.", userId);
                throw new ArgumentException("User is not banned.");
            }

            user.Status = true; // Set status to true to unban the user
            userRepository.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User with ID {UserId} has been unbanned successfully.", userId);
            return "User has been unbanned successfully.";
        }
    }
}
