#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
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
        Assert.False(result.IsError);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<UserEntity>? addedUser = _mockContext.ChangeTracker.Entries<UserEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == userModel.Id);
        Assert.NotNull(addedUser);
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
        Assert.True(result.IsError);
        Assert.Equal(Errors.Users.UserAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<UserEntity>());
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
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(users, result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Act
        ErrorOr<IEnumerable<UserEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
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
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(userModel, result.Value);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByUsernameAsync("nonexistent", CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Value);
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
        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);

        UserEntity? modifiedUser = await _mockContext.Users.FirstOrDefaultAsync(u => u.Username == existingUser.Username);
        Assert.NotNull(modifiedUser);
        Assert.Equal(updatedUser.Password, modifiedUser.Password);
        Assert.Equal(updatedUser.TempPassword, modifiedUser.TempPassword);
        Assert.Equal(updatedUser.TotpSecret, modifiedUser.TotpSecret);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.CreateUserModel();

        // Act
        ErrorOr<Updated> result = await _sut.UpdateAsync(userModel, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Users.UserDoesNotExist, result.FirstError);
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
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(userModel.Id, result.Value.Id);
        Assert.Equal(userModel.Username, result.Value.Username);
        Assert.Equal(userModel.Password, result.Value.Password);
        Assert.Equal(userModel.Libraries, result.Value.Libraries);
        Assert.Equal(userModel.UserPermissions, result.Value.UserPermissions);
        Assert.Equal(userModel.UserRole, result.Value.UserRole);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ErrorOr<UserEntity?> result = await _sut.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Value);
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
        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);

        UserEntity? modifiedUser = await _mockContext.Users
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Username == existingUser.Username);
        Assert.NotNull(modifiedUser);
        Assert.Single(modifiedUser.UserPermissions);
        Assert.Equal(newPermission.PermissionName, modifiedUser.UserPermissions.First().Permission.PermissionName);
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
        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<UserPermissionEntity>? removedPermission = _mockContext.ChangeTracker
            .Entries<UserPermissionEntity>()
            .FirstOrDefault(e => e.State == EntityState.Deleted);
        Assert.NotNull(removedPermission);
        Assert.Equal(oldPermission.Id, removedPermission.Entity.PermissionId);

        EntityEntry<UserPermissionEntity>? addedPermission = _mockContext.ChangeTracker
            .Entries<UserPermissionEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added);
        Assert.NotNull(addedPermission);
        Assert.Equal(newPermission.Id, addedPermission.Entity.PermissionId);
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
        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<UserRoleEntity>? removedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Deleted);
        Assert.NotNull(removedRole);
        Assert.Equal(oldRole.Id, removedRole.Entity.RoleId);

        EntityEntry<UserRoleEntity>? addedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added);
        Assert.NotNull(addedRole);
        Assert.Equal(existingUser.Id, addedRole.Entity.UserId);
        Assert.Equal(newRole.Id, addedRole.Entity.RoleId);
        Assert.Equal(newRole, addedRole.Entity.Role);
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
        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<UserRoleEntity>? removedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Deleted);
        Assert.NotNull(removedRole);
        Assert.Equal(oldUserRole, removedRole.Entity);

        EntityEntry<UserRoleEntity>? addedRole = _mockContext.ChangeTracker
            .Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added);
        Assert.Null(addedRole);
    }
}
