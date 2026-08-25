#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.DataAccess.Core.Repositories.Authorization;
using Lumina.DataAccess.Core.UoW;
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

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="RoleRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly RoleRepository _sut;
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepositoryTests"/> class.
    /// </summary>
    public RoleRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new RoleRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenRoleDoesNotExist_ShouldAddRoleToContextAndReturnCreated()
    {
        // Arrange
        RoleEntity role = _roleEntityFixture.Create(roleName: "Admin");

        // Act
        Result<Created> result = await _sut.InsertAsync(role, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        // Check if the role was added to the context's ChangeTracker
        EntityEntry<RoleEntity>? addedRole = _mockContext.ChangeTracker.Entries<RoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == role.Id);
        Assert.NotNull(addedRole);
    }

    [Fact]
    public async Task InsertAsync_WhenRoleWithSameIdExists_ShouldReturnError()
    {
        // Arrange
        RoleEntity existingRole = _roleEntityFixture.Create(roleName: "Admin");

        _mockContext.Roles.Add(existingRole);
        await _mockContext.SaveChangesAsync();

        RoleEntity newRole = _roleEntityFixture.Create(id: existingRole.Id, roleName: "SuperAdmin");

        // Act
        Result<Created> result = await _sut.InsertAsync(newRole, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.RoleAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<RoleEntity>());
    }

    [Fact]
    public async Task InsertAsync_WhenRoleWithSameNameExists_ShouldReturnError()
    {
        // Arrange
        RoleEntity existingRole = _roleEntityFixture.Create(roleName: "Admin");

        _mockContext.Roles.Add(existingRole);
        await _mockContext.SaveChangesAsync();

        RoleEntity newRole = _roleEntityFixture.Create(roleName: "Admin");

        // Act
        Result<Created> result = await _sut.InsertAsync(newRole, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.RoleAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<RoleEntity>());
    }

    [Fact]
    public async Task GetAllAsync_WhenRolesExist_ShouldReturnAllRoles()
    {
        // Arrange
        List<RoleEntity> roles =
        [
            _roleEntityFixture.Create(roleName: "Admin"),
            _roleEntityFixture.Create(roleName: "User")
        ];

        _mockContext.Roles.AddRange(roles);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<RoleEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(roles, result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoRolesExist_ShouldReturnEmptyList()
    {
        // Act
        Result<IEnumerable<RoleEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByNameAsync_WhenRoleExists_ShouldReturnRoleWithPermissions()
    {
        // Arrange
        RoleEntity role = _roleEntityFixture.Create(roleName: "Admin");

        _mockContext.Roles.Add(role);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<RoleEntity?> result = await _sut.GetByNameAsync("Admin", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(role, result.Value);
    }

    [Fact]
    public async Task GetByNameAsync_WhenRoleDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<RoleEntity?> result = await _sut.GetByNameAsync("NonExistentRole", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRoleExists_ShouldReturnRoleWithPermissions()
    {
        // Arrange
        RolePermissionEntity rolePermission = _rolePermissionEntityFixture.Create();
        RoleEntity role = rolePermission.Role;

        _mockContext.Roles.Add(role);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<RoleEntity?> result = await _sut.GetByIdAsync(role.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(role.Id, result.Value.Id);
        Assert.Equal(role.RoleName, result.Value.RoleName);
        Assert.Equal(role.CreatedOnUtc, result.Value.CreatedOnUtc);
        Assert.Equal(role.CreatedBy, result.Value.CreatedBy);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRoleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Result<RoleEntity?> result = await _sut.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleExists_ShouldUpdateRoleAndReturnUpdated()
    {
        // Arrange
        RoleEntity existingRole = _roleEntityFixture.Create(roleName: "Admin");

        _mockContext.Roles.Add(existingRole);
        await _mockContext.SaveChangesAsync();

        RoleEntity updatedRole = _roleEntityFixture.Create(id: existingRole.Id, roleName: "SuperAdmin", createdBy: existingRole.CreatedBy, createdOnUtc: existingRole.CreatedOnUtc);
        updatedRole.UpdatedOnUtc = DateTime.UtcNow;
        updatedRole.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedRole, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<RoleEntity>? updatedEntry = _mockContext.ChangeTracker.Entries<RoleEntity>()
            .FirstOrDefault(e => e.Entity.Id == existingRole.Id);
        Assert.NotNull(updatedEntry);
        Assert.Equal("SuperAdmin", updatedEntry!.Entity.RoleName);
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleDoesNotExist_ShouldReturnError()
    {
        // Arrange
        RoleEntity nonExistentRole = _roleEntityFixture.Create(roleName: "Admin");

        // Act
        Result<Updated> result = await _sut.UpdateAsync(nonExistentRole, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.RoleNotFound, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleHasPermissions_ShouldUpdatePermissionsAndReturnUpdated()
    {
        // Arrange
        RolePermissionEntity rolePermission = _rolePermissionEntityFixture.Create();
        RoleEntity existingRole = rolePermission.Role;
        existingRole.RolePermissions = [rolePermission];

        _mockContext.Roles.Add(existingRole);
        await _mockContext.SaveChangesAsync();

        RolePermissionEntity newRolePermission = _rolePermissionEntityFixture.Create();
        RoleEntity updatedRole = _roleEntityFixture.Create(id: existingRole.Id, roleName: existingRole.RoleName, rolePermissions: [newRolePermission], includeRolePermissions: true, createdBy: existingRole.CreatedBy, createdOnUtc: existingRole.CreatedOnUtc);
        updatedRole.UpdatedOnUtc = DateTime.UtcNow;
        updatedRole.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedRole, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<RoleEntity>? updatedEntry = _mockContext.ChangeTracker.Entries<RoleEntity>()
            .FirstOrDefault(e => e.Entity.Id == existingRole.Id);
        Assert.NotNull(updatedEntry);
        Assert.Single(updatedEntry!.Entity.RolePermissions);
        Assert.Equal(newRolePermission, updatedEntry.Entity.RolePermissions.First());
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenRoleExists_ShouldDeleteRoleAndReturnDeleted()
    {
        // Arrange
        RoleEntity existingRole = _roleEntityFixture.Create(roleName: "Admin");

        _mockContext.Roles.Add(existingRole);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(existingRole.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        EntityEntry<RoleEntity>? deletedEntry = _mockContext.ChangeTracker.Entries<RoleEntity>()
            .FirstOrDefault(e => e.Entity.Id == existingRole.Id);
        Assert.NotNull(deletedEntry);
        Assert.Equal(EntityState.Deleted, deletedEntry!.State);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenRoleDoesNotExist_ShouldReturnError()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.RoleNotFound, result.FirstError);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenRoleHasPermissions_ShouldDeleteRoleWithPermissions()
    {
        // Arrange
        RolePermissionEntity rolePermission = _rolePermissionEntityFixture.Create();
        RoleEntity existingRole = rolePermission.Role;
        existingRole.RolePermissions = [rolePermission];

        _mockContext.Roles.Add(existingRole);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(existingRole.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        EntityEntry<RoleEntity>? deletedEntry = _mockContext.ChangeTracker.Entries<RoleEntity>()
            .FirstOrDefault(e => e.Entity.Id == existingRole.Id);
        Assert.NotNull(deletedEntry);
        Assert.Equal(EntityState.Deleted, deletedEntry!.State);
    }
}
