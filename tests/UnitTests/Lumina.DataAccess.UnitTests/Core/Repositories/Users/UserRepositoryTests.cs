#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Users;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Authorization.Fixtures;
using Lumina.DataAccess.UnitTests.Core.Repositories.Users.Fixtures;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly UserRepository _sut;
    private readonly UserEntityFixture _userEntityFixture;
    private readonly PermissionEntityFixture _permissionEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepositoryTests"/> class.
    /// </summary>
    public UserRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new UserRepository(_mockContext);
        _userEntityFixture = new UserEntityFixture();
        _permissionEntityFixture = new PermissionEntityFixture();
    }

    [Fact]
    public async Task InsertAsync_WhenUserDoesNotExist_ShouldAddUserToContextAndReturnCreated()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(userModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        EntityEntry<UserEntity>? addedUser = _mockContext.ChangeTracker.Entries<UserEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == userModel.Id);
        addedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_WhenUserAlreadyExists_ShouldReturnError()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();

        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(userModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Users.UserAlreadyExists);
        _mockContext.ChangeTracker.Entries<UserEntity>().Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllUsers()
    {
        // Arrange
        List<UserEntity> users =
        [
            _userEntityFixture.CreateUserModel(),
            _userEntityFixture.CreateUserModel(),
            _userEntityFixture.CreateUserModel()
        ];
        _mockContext.Users.AddRange(users);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<IEnumerable<UserEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Act
        ErrorOr<IEnumerable<UserEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();
        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByUsernameAsync(userModel.Username, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(userModel);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByUsernameAsync("nonexistent", CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldUpdateUserAndReturnUpdated()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.CreateUserModel();
        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        // Create updated user with same Id but different properties
        UserEntity updatedUser = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username, // Keep username as it's used for lookup
            Password = "NewPassword123",
            TempPassword = "TempPass456",
            TotpSecret = "NewSecret",
            Libraries = [],
            UserRole = null,
            UserPermissions = [],
            CreatedBy = existingUser.Id,
            CreatedOnUtc = existingUser.CreatedOnUtc,
            UpdatedOnUtc = DateTime.UtcNow
        };

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);

        UserEntity? modifiedUser = await _mockContext.Users.FirstOrDefaultAsync(u => u.Username == existingUser.Username);
        modifiedUser.Should().NotBeNull();
        modifiedUser!.Password.Should().Be(updatedUser.Password);
        modifiedUser.TempPassword.Should().Be(updatedUser.TempPassword);
        modifiedUser.TotpSecret.Should().Be(updatedUser.TotpSecret);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(userModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Users.UserDoesNotExist);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUserWithAllRelations()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();
        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByIdAsync(userModel.Id, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(userModel, options => options
            .Including(x => x.Id)
            .Including(x => x.Username)
            .Including(x => x.Password)
            .Including(x => x.Libraries)
            .Including(x => x.UserPermissions)
            .Including(x => x.UserRole));
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExistsWithPermissions_ShouldUpdatePermissionsAndReturnUpdated()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.CreateUserModel();
        PermissionEntity oldPermission = _permissionEntityFixture.CreatePermissionModel();
        PermissionEntity newPermission = _permissionEntityFixture.CreatePermissionModel();

        _mockContext.Users.Add(existingUser);
        _mockContext.Permissions.Add(oldPermission);
        _mockContext.Permissions.Add(newPermission);
        await _mockContext.SaveChangesAsync();

        UserEntity updatedUser = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = "NewPassword",
            Libraries = [],
            UserRole = null,
            UserPermissions =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = existingUser.Id,
                    User = existingUser,
                    PermissionId = newPermission.Id,
                    Permission = newPermission,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedBy = existingUser.Id
                }
            ],
            CreatedBy = existingUser.CreatedBy,
            CreatedOnUtc = existingUser.CreatedOnUtc,
            UpdatedOnUtc = DateTime.UtcNow
        };

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);

        UserEntity? modifiedUser = await _mockContext.Users
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Username == existingUser.Username);
        modifiedUser.Should().NotBeNull();
        modifiedUser!.UserPermissions.Should().HaveCount(1);
        modifiedUser.UserPermissions.First().Permission.PermissionName.Should().Be(newPermission.PermissionName);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserHasExistingPermissions_ShouldRemoveOldPermissionsAndAddNew()
    {
        // Arrange
        UserEntityFixture userFixture = new();
        PermissionEntityFixture permissionFixture = new();
        UserPermissionEntityFixture userPermissionFixture = new();

        UserEntity existingUser = userFixture.CreateUserModel();
        PermissionEntity oldPermission = permissionFixture.CreatePermissionModel();
        PermissionEntity newPermission = permissionFixture.CreatePermissionModel();

        UserEntity userWithPermissions = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = existingUser.Password,
            Libraries = existingUser.Libraries,
            UserRole = null,
            UserPermissions =
            [
                userPermissionFixture.CreateUserPermissionModel(existingUser, oldPermission)
            ],
            CreatedOnUtc = existingUser.CreatedOnUtc,
            CreatedBy = existingUser.CreatedBy,
            UpdatedOnUtc = existingUser.UpdatedOnUtc,
            UpdatedBy = existingUser.UpdatedBy
        };

        _mockContext.Users.Add(userWithPermissions);
        await _mockContext.SaveChangesAsync();

        UserEntity updatedUser = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = existingUser.Password,
            Libraries = existingUser.Libraries,
            UserRole = null,
            UserPermissions =
            [
                userPermissionFixture.CreateUserPermissionModel(existingUser, newPermission)
            ],
            CreatedBy = existingUser.CreatedBy,
            CreatedOnUtc = existingUser.CreatedOnUtc,
            UpdatedOnUtc = DateTime.UtcNow,
            UpdatedBy = existingUser.Id
        };

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);

        EntityEntry<UserPermissionEntity>? removedPermission = _mockContext.ChangeTracker
            .Entries<UserPermissionEntity>()
            .FirstOrDefault(e => e.State == EntityState.Deleted);
        removedPermission.Should().NotBeNull();
        removedPermission!.Entity.PermissionId.Should().Be(oldPermission.Id);

        EntityEntry<UserPermissionEntity>? addedPermission = _mockContext.ChangeTracker
            .Entries<UserPermissionEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added);
        addedPermission.Should().NotBeNull();
        addedPermission!.Entity.PermissionId.Should().Be(newPermission.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserRoleChanges_ShouldRemoveOldRoleAndAddNew()
    {
        // Arrange
        RoleEntity oldRole = new()
        {
            Id = Guid.NewGuid(),
            RoleName = "OldRole",
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        RoleEntity newRole = new()
        {
            Id = Guid.NewGuid(),
            RoleName = "NewRole",
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        UserEntity existingUser = new()
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Password = "TestPass",
            Libraries = [],
            UserRole = null,
            UserPermissions = [],
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        UserRoleEntity oldUserRole = new()
        {
            Id = Guid.NewGuid(),
            UserId = existingUser.Id,
            User = existingUser,
            RoleId = oldRole.Id,
            Role = oldRole,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        existingUser = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = existingUser.Password,
            Libraries = [],
            UserRole = oldUserRole,
            UserPermissions = [],
            CreatedOnUtc = existingUser.CreatedOnUtc,
            CreatedBy = existingUser.CreatedBy
        };

        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        UserRoleEntity newUserRole = new()
        {
            Id = Guid.NewGuid(),
            UserId = existingUser.Id,
            User = existingUser,
            RoleId = newRole.Id,
            Role = newRole,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = existingUser.Id
        };

        UserEntity updatedUser = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = existingUser.Password,
            Libraries = [],
            UserRole = newUserRole,
            UserPermissions = [],
            CreatedBy = existingUser.CreatedBy,
            CreatedOnUtc = existingUser.CreatedOnUtc,
            UpdatedOnUtc = DateTime.UtcNow,
            UpdatedBy = existingUser.Id
        };

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);

        EntityEntry<UserRoleEntity>? removedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Deleted);
        removedRole.Should().NotBeNull();
        removedRole!.Entity.RoleId.Should().Be(oldRole.Id);

        EntityEntry<UserRoleEntity>? addedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added);
        addedRole.Should().NotBeNull();
        addedRole!.Entity.UserId.Should().Be(existingUser.Id);
        addedRole.Entity.RoleId.Should().Be(newRole.Id);
        addedRole.Entity.Role.Should().Be(newRole);
    }

    [Fact]
    public async Task UpdateAsync_WhenRemovingUserRole_ShouldRemoveRoleAndNotAddNew()
    {
        // Arrange
        UserEntityFixture userFixture = new();
        UserRoleEntityFixture userRoleFixture = new();

        UserRoleEntity oldUserRole = userRoleFixture.CreateUserRoleModel();

        UserEntity existingUser = new()
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Password = "TestPass",
            Libraries = [],
            UserRole = oldUserRole,
            UserPermissions = [],
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        UserEntity updatedUser = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = existingUser.Password,
            Libraries = [],
            UserRole = null,
            UserPermissions = [],
            CreatedBy = existingUser.CreatedBy,
            CreatedOnUtc = existingUser.CreatedOnUtc,
            UpdatedOnUtc = DateTime.UtcNow,
            UpdatedBy = existingUser.Id
        };

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);

        EntityEntry<UserRoleEntity>? removedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Deleted);
        removedRole.Should().NotBeNull();
        removedRole!.Entity.Should().Be(oldUserRole);

        EntityEntry<UserRoleEntity>? addedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added);
        addedRole.Should().BeNull();
    }
}
