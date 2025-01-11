#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.DataAccess.Core.Seed;
using Lumina.DataAccess.UnitTests.Core.Repositories.Users.Fixtures;
using Lumina.Domain.Common.Enums.Authorization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Seed;

/// <summary>
/// Contains unit tests for the <see cref="DataSeedService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DataSeedServiceTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IDateTimeProvider _mockDateTimeProvider;
    private readonly IPermissionRepository _mockPermissionRepository;
    private readonly DataSeedService _sut;
    private readonly DateTime _fixedUtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeedServiceTests"/> class.
    /// </summary>
    public DataSeedServiceTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        _mockPermissionRepository = Substitute.For<IPermissionRepository>();
        _fixedUtcNow = DateTime.UtcNow;

        _mockDateTimeProvider.UtcNow.Returns(_fixedUtcNow);
        _mockUnitOfWork.GetRepository<IPermissionRepository>().Returns(_mockPermissionRepository);

        _sut = new DataSeedService(_mockUnitOfWork, _mockDateTimeProvider);
    }

    [Fact]
    public async Task SetDefaultAuthorizationPermissionsAsync_WhenSuccessful_ShouldInsertAllPermissionsAndReturnCreated()
    {
        // Arrange
        Guid adminId = Guid.NewGuid();
        _mockPermissionRepository
            .InsertAsync(Arg.Any<PermissionEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<Created> result = await _sut.SetDefaultAuthorizationPermissionsAsync(adminId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        await _mockPermissionRepository
            .Received(4)
            .InsertAsync(
                Arg.Is<PermissionEntity>(p =>
                    p.CreatedBy == adminId &&
                    p.CreatedOnUtc == _fixedUtcNow),
                Arg.Any<CancellationToken>());

        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetDefaultAuthorizationPermissionsAsync_WhenInsertFails_ShouldReturnError()
    {
        // Arrange
        Guid adminId = Guid.NewGuid();
        Error expectedError = Error.Conflict("Permission.Conflict", "Permission already exists");
        _mockPermissionRepository
            .InsertAsync(Arg.Any<PermissionEntity>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetDefaultAuthorizationPermissionsAsync(adminId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetDefaultAuthorizationPermissionsAsync_ShouldCreateAllDefaultPermissions()
    {
        // Arrange
        Guid adminId = Guid.NewGuid();
        List<AuthorizationPermission> capturedPermissions = [];

        _mockPermissionRepository
            .InsertAsync(Arg.Any<PermissionEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created)
            .AndDoes(info => capturedPermissions.Add(info.Arg<PermissionEntity>().PermissionName));

        // Act
        await _sut.SetDefaultAuthorizationPermissionsAsync(adminId, CancellationToken.None);

        // Assert
        capturedPermissions.Should().BeEquivalentTo(
        [
            AuthorizationPermission.CanViewUsers,
            AuthorizationPermission.CanDeleteUsers,
            AuthorizationPermission.CanRegisterUsers,
            AuthorizationPermission.CanCreateLibraries
        ]);
    }

    [Fact]
    public async Task SetDefaultAuthorizationRolesAsync_WhenSuccessful_ShouldInsertAllRolesAndReturnCreated()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        mockRoleRepository
            .InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<Created> result = await _sut.SetDefaultAuthorizationRolesAsync(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        await mockRoleRepository
            .Received(1)
            .InsertAsync(
                Arg.Is<RoleEntity>(r =>
                    r.CreatedBy == userId &&
                    r.CreatedOnUtc == _fixedUtcNow &&
                    r.RoleName == "Admin"),
                Arg.Any<CancellationToken>());

        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetDefaultAuthorizationRolesAsync_WhenInsertFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);

        Error expectedError = Error.Conflict("Role.Conflict", "Role already exists");
        mockRoleRepository
            .InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetDefaultAuthorizationRolesAsync(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetDefaultAuthorizationRolesAsync_ShouldCreateDefaultAdminRole()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        List<string> capturedRoleNames = [];

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);

        mockRoleRepository
            .InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created)
            .AndDoes(info => capturedRoleNames.Add(info.Arg<RoleEntity>().RoleName));

        // Act
        await _sut.SetDefaultAuthorizationRolesAsync(userId, CancellationToken.None);

        // Assert
        capturedRoleNames.Should().BeEquivalentTo(["Admin"]);
    }

    [Fact]
    public async Task SetDefaultAuthorizationRolesAsync_ShouldSetCorrectAuditProperties()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity? capturedRole = null;

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);

        mockRoleRepository
            .InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created)
            .AndDoes(info => capturedRole = info.Arg<RoleEntity>());

        // Act
        await _sut.SetDefaultAuthorizationRolesAsync(userId, CancellationToken.None);

        // Assert
        capturedRole.Should().NotBeNull();
        capturedRole!.CreatedBy.Should().Be(userId);
        capturedRole.CreatedOnUtc.Should().Be(_fixedUtcNow);
        capturedRole.UpdatedBy.Should().BeNull();
        capturedRole.UpdatedOnUtc.Should().BeNull();
    }

    [Fact]
    public async Task SetAdminRolePermissionsAsync_WhenSuccessful_ShouldAssignAllPermissionsToAdminRole()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new() { Id = Guid.NewGuid(), RoleName = "Admin" };
        List<PermissionEntity> permissions =
        [
            new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanViewUsers },
            new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanDeleteUsers },
            new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanRegisterUsers }
        ];

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        IPermissionRepository mockPermissionRepository = Substitute.For<IPermissionRepository>();
        IRolePermissionRepository mockRolePermissionRepository = Substitute.For<IRolePermissionRepository>();

        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        _mockUnitOfWork.GetRepository<IPermissionRepository>().Returns(mockPermissionRepository);
        _mockUnitOfWork.GetRepository<IRolePermissionRepository>().Returns(mockRolePermissionRepository);

        mockRoleRepository.GetByNameAsync("Admin", Arg.Any<CancellationToken>()).Returns(adminRole);
        mockPermissionRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(permissions);
        mockRolePermissionRepository.InsertAsync(Arg.Any<RolePermissionEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRolePermissionsAsync(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        await mockRolePermissionRepository
            .Received(3)
            .InsertAsync(
                Arg.Is<RolePermissionEntity>(rp =>
                    rp.RoleId == adminRole.Id &&
                    rp.CreatedBy == userId &&
                    rp.CreatedOnUtc == _fixedUtcNow),
                Arg.Any<CancellationToken>());

        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRolePermissionsAsync_WhenAdminRoleNotFound_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);

        mockRoleRepository
            .GetByNameAsync("Admin", Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRolePermissionsAsync(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.AdminAccountNotFound);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRolePermissionsAsync_WhenGetPermissionsFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new() { Id = Guid.NewGuid(), RoleName = "Admin" };
        Error expectedError = Error.Failure("Permissions.NotFound", "Failed to retrieve permissions");

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        IPermissionRepository mockPermissionRepository = Substitute.For<IPermissionRepository>();

        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        _mockUnitOfWork.GetRepository<IPermissionRepository>().Returns(mockPermissionRepository);

        mockRoleRepository.GetByNameAsync("Admin", Arg.Any<CancellationToken>()).Returns(adminRole);
        mockPermissionRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRolePermissionsAsync(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRolePermissionsAsync_WhenInsertRolePermissionFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new() { Id = Guid.NewGuid(), RoleName = "Admin" };
        List<PermissionEntity> permissions = [new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanViewUsers }];
        Error expectedError = Error.Failure("RolePermission.InsertFailed", "Failed to insert role permission");

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        IPermissionRepository mockPermissionRepository = Substitute.For<IPermissionRepository>();
        IRolePermissionRepository mockRolePermissionRepository = Substitute.For<IRolePermissionRepository>();

        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        _mockUnitOfWork.GetRepository<IPermissionRepository>().Returns(mockPermissionRepository);
        _mockUnitOfWork.GetRepository<IRolePermissionRepository>().Returns(mockRolePermissionRepository);

        mockRoleRepository.GetByNameAsync("Admin", Arg.Any<CancellationToken>()).Returns(adminRole);
        mockPermissionRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(permissions);
        mockRolePermissionRepository.InsertAsync(Arg.Any<RolePermissionEntity>(), Arg.Any<CancellationToken>()).Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRolePermissionsAsync(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRolePermissionsAsync_WhenGetAdminRoleFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Error expectedError = Error.Failure("Role.NotFound", "Failed to retrieve admin role");

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);

        mockRoleRepository
            .GetByNameAsync("Admin", Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRolePermissionsAsync(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRoleToAdministratorAccount_WhenSuccessful_ShouldAssignAdminRoleToUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new() { Id = Guid.NewGuid(), RoleName = "Admin" };
        UserEntityFixture userFixture = new();
        UserEntity adminUser = userFixture.CreateUserModel();
        // Use the adminUser.Id instead of the randomly generated userId
        userId = adminUser.Id;

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        IUserRepository mockUserRepository = Substitute.For<IUserRepository>();
        IUserRoleRepository mockUserRoleRepository = Substitute.For<IUserRoleRepository>();

        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(mockUserRepository);
        _mockUnitOfWork.GetRepository<IUserRoleRepository>().Returns(mockUserRoleRepository);

        mockRoleRepository.GetByNameAsync("Admin", Arg.Any<CancellationToken>()).Returns(adminRole);
        mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(adminUser);
        mockUserRoleRepository.InsertAsync(Arg.Any<UserRoleEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRoleToAdministratorAccount(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        await mockUserRoleRepository
            .Received(1)
            .InsertAsync(
                Arg.Is<UserRoleEntity>(ur =>
                    ur.UserId == userId &&
                    ur.RoleId == adminRole.Id &&
                    ur.CreatedBy == userId &&
                    ur.CreatedOnUtc == _fixedUtcNow),
                Arg.Any<CancellationToken>());

        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRoleToAdministratorAccount_WhenAdminRoleNotFound_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);

        mockRoleRepository
            .GetByNameAsync("Admin", Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRoleToAdministratorAccount(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.AdminRoleNotFound);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRoleToAdministratorAccount_WhenUserNotFound_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new() { Id = Guid.NewGuid(), RoleName = "Admin" };

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        IUserRepository mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(mockUserRepository);

        mockRoleRepository.GetByNameAsync("Admin", Arg.Any<CancellationToken>()).Returns(adminRole);
        mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserEntity?)null);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRoleToAdministratorAccount(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.AdminAccountNotFound);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRoleToAdministratorAccount_WhenInsertUserRoleFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new() { Id = Guid.NewGuid(), RoleName = "Admin" };
        UserEntity adminUser = new UserEntityFixture().CreateUserModel();

        Error expectedError = Error.Failure("UserRole.InsertFailed", "Failed to insert user role");

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        IUserRepository mockUserRepository = Substitute.For<IUserRepository>();
        IUserRoleRepository mockUserRoleRepository = Substitute.For<IUserRoleRepository>();

        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(mockUserRepository);
        _mockUnitOfWork.GetRepository<IUserRoleRepository>().Returns(mockUserRoleRepository);

        mockRoleRepository.GetByNameAsync("Admin", Arg.Any<CancellationToken>()).Returns(adminRole);
        mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(adminUser);
        mockUserRoleRepository.InsertAsync(Arg.Any<UserRoleEntity>(), Arg.Any<CancellationToken>()).Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRoleToAdministratorAccount(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRoleToAdministratorAccount_WhenGetAdminRoleFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Error expectedError = Error.Failure("Role.NotFound", "Failed to retrieve admin role");

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);

        mockRoleRepository
            .GetByNameAsync("Admin", Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRoleToAdministratorAccount(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAdminRoleToAdministratorAccount_WhenGetUserFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new() { Id = Guid.NewGuid(), RoleName = "Admin" };
        Error expectedError = Error.Failure("User.NotFound", "Failed to retrieve user");

        IRoleRepository mockRoleRepository = Substitute.For<IRoleRepository>();
        IUserRepository mockUserRepository = Substitute.For<IUserRepository>();

        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(mockRoleRepository);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(mockUserRepository);

        mockRoleRepository.GetByNameAsync("Admin", Arg.Any<CancellationToken>()).Returns(adminRole);
        mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(expectedError);

        // Act
        ErrorOr<Created> result = await _sut.SetAdminRoleToAdministratorAccount(userId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);

        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
