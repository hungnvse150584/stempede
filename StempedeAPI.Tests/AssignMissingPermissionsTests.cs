using BusinessLogic.Auth.Helpers.Implementation;
using BusinessLogic.Configurations;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace StempedeAPI.Tests
{
    public class AssignMissingPermissionsTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<AssignMissingPermissions>> _mockLogger;
        private readonly AssignMissingPermissions _assignMissingPermissions;
        private readonly Mock<IOptions<DatabaseSettings>> _dbSettingsMock;

        public AssignMissingPermissionsTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<AssignMissingPermissions>>();
            _dbSettingsMock = new Mock<IOptions<DatabaseSettings>>();
            _assignMissingPermissions = new AssignMissingPermissions(_mockUnitOfWork.Object, _mockLogger.Object, _dbSettingsMock.Object);
        }

        //[Fact]
        //public async Task AssignMissingPermissionsAsync_ShouldAssignPermissions_WhenPermissionsAreMissing()
        //{
        //    // Arrange
        //    int userId = 1;
        //    List<string> roleNames = new List<string> { "Customer", "Staff" };

        //    // Mock permissions corresponding to roles
        //    var permissionCustomer = new Permission { PermissionId = 1, PermissionName = "Customer", Description = "Customer permission." };
        //    var permissionStaff = new Permission { PermissionId = 2, PermissionName = "Staff", Description = "Staff permission." };

        //    // Setup repository for Permission
        //    _mockUnitOfWork.Setup(uow => uow.GetRepository<Permission>().GetAsync(
        //            It.IsAny<Expression<Func<Permission, bool>>>(),
        //            It.IsAny<string>()))
        //        .ReturnsAsync((Expression<Func<Permission, bool>> predicate, string includeProperties) =>
        //            new List<Permission>
        //            {
        //                permissionCustomer,
        //                permissionStaff
        //            }.AsQueryable().FirstOrDefault(predicate));

        //    // Setup existing permissions (only Customer is already assigned)
        //    var existingUserPermissions = new List<UserPermission>
        //    {
        //        new UserPermission { UserId = userId, PermissionId = 1, AssignedBy = userId }
        //    };

        //    _mockUnitOfWork.Setup(uow => uow.GetRepository<UserPermission>().FindAsync(
        //            It.IsAny<Expression<Func<UserPermission, bool>>>(),
        //            It.IsAny<string>()))
        //        .ReturnsAsync(existingUserPermissions);

        //    // Mock AddAsync for UserPermission
        //    _mockUnitOfWork.Setup(uow => uow.GetRepository<UserPermission>().AddAsync(It.IsAny<UserPermission>()))
        //        .Returns(Task.CompletedTask)
        //        .Verifiable();

        //    // Mock CompleteAsync
        //    _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
        //        .ReturnsAsync(1)
        //        .Verifiable();

        //    // Act
        //    await _assignMissingPermissions.AssignMissingPermissionsAsync(userId, roleNames);

        //    // Assert
        //    // Verify that only the Staff permission was assigned
        //    _mockUnitOfWork.Verify(uow => uow.GetRepository<UserPermission>().AddAsync(It.Is<UserPermission>(
        //        up => up.UserId == userId && up.PermissionId == 2 && up.AssignedBy == userId)), Times.Once);

        //    // Verify that CompleteAsync was called to save changes
        //    _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        //}

        [Fact]
        public async Task AssignMissingPermissionsAsync_ShouldNotAssignPermissions_WhenPermissionsAlreadyExist()
        {
            // Arrange
            int userId = 1;
            List<string> roleNames = new List<string> { "Customer" };

            // Mock permissions corresponding to roles
            var permissionCustomer = new Permission { PermissionId = 1, PermissionName = "Customer", Description = "Customer permission." };

            // Setup repository for Permission
            _mockUnitOfWork.Setup(uow => uow.GetRepository<Permission>().GetAsync(
                    It.IsAny<Expression<Func<Permission, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(permissionCustomer);

            // Setup existing permissions (Customer is already assigned)
            var existingUserPermissions = new List<UserPermission>
            {
                new UserPermission { UserId = userId, PermissionId = 1, AssignedBy = userId }
            };

            _mockUnitOfWork.Setup(uow => uow.GetRepository<UserPermission>().FindAsync(
                    It.IsAny<Expression<Func<UserPermission, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(existingUserPermissions);

            // Act
            await _assignMissingPermissions.AssignMissingPermissionsAsync(userId, roleNames);

            // Assert
            // Verify that AddAsync was never called since permission already exists
            _mockUnitOfWork.Verify(uow => uow.GetRepository<UserPermission>().AddAsync(It.IsAny<UserPermission>()), Times.Never);

            // Verify that CompleteAsync was never called since no changes were made
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task AssignMissingPermissionsAsync_ShouldHandle_NoMatchingPermissions()
        {
            // Arrange
            int userId = 1;
            List<string> roleNames = new List<string> { "NonExistentRole" };

            // Setup repository for Permission to return null
            _mockUnitOfWork.Setup(uow => uow.GetRepository<Permission>().GetAsync(
                    It.IsAny<Expression<Func<Permission, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync((Permission)null);

            // Act
            await _assignMissingPermissions.AssignMissingPermissionsAsync(userId, roleNames);

            // Assert
            // Verify that AddAsync was never called since no matching permissions
            _mockUnitOfWork.Verify(uow => uow.GetRepository<UserPermission>().AddAsync(It.IsAny<UserPermission>()), Times.Never);

            // Verify that CompleteAsync was never called since no changes were made
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task AssignMissingPermissionsAsync_ShouldLogWarnings_WhenNoRolesProvided()
        {
            // Arrange
            int userId = 1;
            List<string> roleNames = null;

            // Act
            await _assignMissingPermissions.AssignMissingPermissionsAsync(userId, roleNames);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No roles provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that no repository methods were called
            _mockUnitOfWork.Verify(uow => uow.GetRepository<Permission>().GetAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<string>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.GetRepository<UserPermission>().AddAsync(It.IsAny<UserPermission>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }
    }
}