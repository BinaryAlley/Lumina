#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Users;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
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
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly UserRoleEntityFixture _userRoleEntityFixture = new();
    private readonly UserPermissionEntityFixture _userPermissionEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepositoryTests"/> class.
    /// </summary>
    public UserRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new UserRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenUserDoesNotExist_ShouldAddUserToContextAndReturnCreated()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(userModel, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<UserEntity>? addedUser = _mockContext.ChangeTracker.Entries<UserEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == userModel.Id);
        Assert.NotNull(addedUser);
    }

    [Fact]
    public async Task InsertAsync_WhenUserAlreadyExists_ShouldReturnError()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.Create();

        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(userModel, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Users.UserAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<UserEntity>());
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllUsers()
    {
        // Arrange
        List<UserEntity> users = _userEntityFixture.CreateMany();
        _mockContext.Users.AddRange(users);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<UserEntity>> result = await _sut.GetAllAsync<BaseFilterDto>(cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(users, result.Value.Data);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Act
        Result<PaginatedResultDto<UserEntity>> result = await _sut.GetAllAsync<BaseFilterDto>(cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Data);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.Create();
        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<UserEntity?> result = await _sut.GetByUsernameAsync(userModel.Username, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(userModel, result.Value);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<UserEntity?> result = await _sut.GetByUsernameAsync("nonexistent", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldUpdateUserAndReturnUpdated()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.Create();
        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        // Create updated user with same Id but different properties
        UserEntity updatedUser = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: "NewPassword123");
        updatedUser.TempPassword = "TempPass456";
        updatedUser.TotpSecret = "NewSecret";
        updatedUser.CreatedOnUtc = existingUser.CreatedOnUtc;
        updatedUser.UpdatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
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
        UserEntity userModel = _userEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(userModel, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Users.UserDoesNotExist, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingEntityCarriesADifferentId_ShouldPreserveTheStoredIdentity()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.Create();
        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        Guid mismatchedId = Guid.NewGuid();
        UserEntity updatedUser = _userEntityFixture.Create(id: mismatchedId, username: existingUser.Username, password: "NewPassword123");
        updatedUser.CreatedOnUtc = existingUser.CreatedOnUtc;
        updatedUser.CreatedBy = existingUser.CreatedBy;
        updatedUser.UpdatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        // The repository locates the row by username, so the stored identity must survive an incoming key that does not match it.
        EntityEntry<UserEntity> trackedUser = _mockContext.ChangeTracker.Entries<UserEntity>()
            .Single(entry => entry.Entity.Username == existingUser.Username);
        Assert.Equal(existingUser.Id, trackedUser.Entity.Id);
        Assert.NotEqual(mismatchedId, trackedUser.Entity.Id);
        Assert.Equal("NewPassword123", trackedUser.Entity.Password);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUserWithAllRelations()
    {
        // Arrange
        UserEntity userModel = _userEntityFixture.Create();
        _mockContext.Users.Add(userModel);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<UserEntity?> result = await _sut.GetByIdAsync(userModel.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
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
        Result<UserEntity?> result = await _sut.GetByIdAsync(nonExistentId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExistsWithPermissions_ShouldUpdatePermissionsAndReturnUpdated()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.Create();
        PermissionEntity oldPermission = _permissionEntityFixture.Create();
        PermissionEntity newPermission = _permissionEntityFixture.Create();

        _mockContext.Users.Add(existingUser);
        _mockContext.Permissions.Add(oldPermission);
        _mockContext.Permissions.Add(newPermission);
        await _mockContext.SaveChangesAsync();

        UserPermissionEntity newUserPermission = _userPermissionEntityFixture.Create(existingUser, newPermission);
        newUserPermission.CreatedOnUtc = DateTime.UtcNow;

        UserEntity updatedUser = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: "NewPassword", userPermissions: [newUserPermission], includeUserPermissions: true);
        updatedUser.CreatedBy = existingUser.CreatedBy;
        updatedUser.CreatedOnUtc = existingUser.CreatedOnUtc;
        updatedUser.UpdatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
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
    public async Task UpdateAsync_WhenThePermissionsAreReplaced_ShouldRemoveTheOldAndAddTheNew()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.Create();
        PermissionEntity oldPermission = _permissionEntityFixture.Create();
        PermissionEntity newPermission = _permissionEntityFixture.Create();

        UserEntity userWithPermissions = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: existingUser.Password, userPermissions: [_userPermissionEntityFixture.Create(existingUser, oldPermission)], includeUserPermissions: true);
        userWithPermissions.CreatedOnUtc = existingUser.CreatedOnUtc;
        userWithPermissions.CreatedBy = existingUser.CreatedBy;
        userWithPermissions.UpdatedOnUtc = existingUser.UpdatedOnUtc;
        userWithPermissions.UpdatedBy = existingUser.UpdatedBy;

        _mockContext.Users.Add(userWithPermissions);
        await _mockContext.SaveChangesAsync();

        UserEntity updatedUser = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: existingUser.Password, userPermissions: [_userPermissionEntityFixture.Create(existingUser, newPermission)], includeUserPermissions: true);
        updatedUser.CreatedBy = existingUser.CreatedBy;
        updatedUser.CreatedOnUtc = existingUser.CreatedOnUtc;
        updatedUser.UpdatedOnUtc = DateTime.UtcNow;
        updatedUser.UpdatedBy = existingUser.Id;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
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
    public async Task UpdateAsync_WhenAPermissionIsStillPresent_ShouldKeepItsIdentityAndAuditColumns()
    {
        // Arrange
        UserEntity existingUser = _userEntityFixture.Create();
        PermissionEntity keptPermission = _permissionEntityFixture.Create();
        PermissionEntity removedPermission = _permissionEntityFixture.Create();

        DateTime keptCreatedOnUtc = new(2020, 5, 6, 7, 8, 9, DateTimeKind.Utc);
        UserPermissionEntity keptUserPermission = _userPermissionEntityFixture.Create(existingUser, keptPermission);
        keptUserPermission.CreatedOnUtc = keptCreatedOnUtc;
        UserPermissionEntity removedUserPermission = _userPermissionEntityFixture.Create(existingUser, removedPermission);

        UserEntity userWithPermissions = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: existingUser.Password, userPermissions: [keptUserPermission, removedUserPermission], includeUserPermissions: true);
        userWithPermissions.CreatedOnUtc = existingUser.CreatedOnUtc;
        userWithPermissions.CreatedBy = existingUser.CreatedBy;

        _mockContext.Users.Add(userWithPermissions);
        _mockContext.Permissions.Add(keptPermission);
        _mockContext.Permissions.Add(removedPermission);
        await _mockContext.SaveChangesAsync();

        UserPermissionEntity incomingKeptPermission = _userPermissionEntityFixture.Create(userWithPermissions, keptPermission);
        UserEntity updatedUser = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: "NewPassword", userPermissions: [incomingKeptPermission], includeUserPermissions: true);
        updatedUser.CreatedBy = existingUser.CreatedBy;
        updatedUser.CreatedOnUtc = existingUser.CreatedOnUtc;
        updatedUser.UpdatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        UserEntity? modifiedUser = await _mockContext.Users
            .Include(u => u.UserPermissions)
            .FirstOrDefaultAsync(u => u.Username == existingUser.Username);
        Assert.NotNull(modifiedUser);
        UserPermissionEntity retainedPermission = Assert.Single(modifiedUser.UserPermissions);
        Assert.Equal(keptUserPermission.Id, retainedPermission.Id);
        Assert.Equal(keptPermission.Id, retainedPermission.PermissionId);
        Assert.Equal(keptCreatedOnUtc, retainedPermission.CreatedOnUtc);

        EntityEntry<UserPermissionEntity>? removedEntry = _mockContext.ChangeTracker
            .Entries<UserPermissionEntity>()
            .FirstOrDefault(e => e.Entity.PermissionId == removedPermission.Id);
        Assert.NotNull(removedEntry);
        Assert.Equal(EntityState.Deleted, removedEntry.State);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserRoleChanges_ShouldRemoveOldRoleAndAddNew()
    {
        // Arrange
        RoleEntity oldRole = _roleEntityFixture.Create(roleName: "OldRole");
        oldRole.CreatedOnUtc = DateTime.UtcNow;
        oldRole.CreatedBy = Guid.NewGuid();

        RoleEntity newRole = _roleEntityFixture.Create(roleName: "NewRole");
        newRole.CreatedOnUtc = DateTime.UtcNow;
        newRole.CreatedBy = Guid.NewGuid();

        UserEntity existingUser = _userEntityFixture.Create(username: "TestUser", password: "TestPass");
        existingUser.CreatedOnUtc = DateTime.UtcNow;
        existingUser.CreatedBy = Guid.NewGuid();

        UserRoleEntity oldUserRole = _userRoleEntityFixture.Create(userId: existingUser.Id, user: existingUser, roleId: oldRole.Id, role: oldRole);
        oldUserRole.CreatedOnUtc = DateTime.UtcNow;
        oldUserRole.CreatedBy = Guid.NewGuid();

        DateTime originalCreatedOnUtc = existingUser.CreatedOnUtc;
        Guid originalCreatedBy = existingUser.CreatedBy;
        existingUser = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: existingUser.Password, userRole: oldUserRole, includeUserRole: true);
        existingUser.CreatedOnUtc = originalCreatedOnUtc;
        existingUser.CreatedBy = originalCreatedBy;

        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        UserRoleEntity newUserRole = _userRoleEntityFixture.Create(userId: existingUser.Id, user: existingUser, roleId: newRole.Id, role: newRole);
        newUserRole.CreatedOnUtc = DateTime.UtcNow;
        newUserRole.CreatedBy = existingUser.Id;

        UserEntity updatedUser = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: existingUser.Password, userRole: newUserRole, includeUserRole: true);
        updatedUser.CreatedBy = existingUser.CreatedBy;
        updatedUser.CreatedOnUtc = existingUser.CreatedOnUtc;
        updatedUser.UpdatedOnUtc = DateTime.UtcNow;
        updatedUser.UpdatedBy = existingUser.Id;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
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
        UserRoleEntity oldUserRole = _userRoleEntityFixture.Create();

        UserEntity existingUser = _userEntityFixture.Create(username: "TestUser", password: "TestPass", userRole: oldUserRole, includeUserRole: true);
        existingUser.CreatedOnUtc = DateTime.UtcNow;
        existingUser.CreatedBy = Guid.NewGuid();

        _mockContext.Users.Add(existingUser);
        await _mockContext.SaveChangesAsync();

        UserEntity updatedUser = _userEntityFixture.Create(id: existingUser.Id, username: existingUser.Username, password: existingUser.Password);
        updatedUser.CreatedBy = existingUser.CreatedBy;
        updatedUser.CreatedOnUtc = existingUser.CreatedOnUtc;
        updatedUser.UpdatedOnUtc = DateTime.UtcNow;
        updatedUser.UpdatedBy = existingUser.Id;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
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
