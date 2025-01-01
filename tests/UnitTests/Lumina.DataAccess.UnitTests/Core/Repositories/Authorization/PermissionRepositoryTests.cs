#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.DataAccess.Core.Repositories.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Authorization.Fixtures;
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
/// Contains unit tests for the <see cref="PermissionRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly PermissionRepository _sut;
    private readonly PermissionEntityFixture _permissionEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRepositoryTests"/> class.
    /// </summary>
    public PermissionRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new PermissionRepository(_mockContext);
        _permissionEntityFixture = new PermissionEntityFixture();
    }

    [Fact]
    public async Task InsertAsync_WhenPermissionDoesNotExist_ShouldAddPermissionToContextAndReturnCreated()
    {
        // Arrange
        PermissionEntity permissionModel = _permissionEntityFixture.CreatePermissionModel();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(permissionModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        // Check if the permission was added to the context's ChangeTracker
        EntityEntry<PermissionEntity>? addedPermission = _mockContext.ChangeTracker.Entries<PermissionEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == permissionModel.Id);
        addedPermission.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_WhenPermissionAlreadyExists_ShouldReturnError()
    {
        // Arrange
        PermissionEntity permissionModel = _permissionEntityFixture.CreatePermissionModel();

        _mockContext.Permissions.Add(permissionModel);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(permissionModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.PermissionAlreadyExists);
        _mockContext.ChangeTracker.Entries<PermissionEntity>().Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WhenPermissionsExist_ShouldReturnAllPermissions()
    {
        // Arrange
        List<PermissionEntity> permissions =
        [
            _permissionEntityFixture.CreatePermissionModel(),
            _permissionEntityFixture.CreatePermissionModel(),
            _permissionEntityFixture.CreatePermissionModel()
        ];
        _mockContext.Permissions.AddRange(permissions);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<IEnumerable<PermissionEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().BeEquivalentTo(permissions);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoPermissionsExist_ShouldReturnEmptyList()
    {
        // Act
        ErrorOr<IEnumerable<PermissionEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_WhenPermissionsExist_ShouldReturnMatchingPermissions()
    {
        // Arrange
        List<PermissionEntity> permissions =
        [
            _permissionEntityFixture.CreatePermissionModel(),
            _permissionEntityFixture.CreatePermissionModel(),
            _permissionEntityFixture.CreatePermissionModel()
        ];
        _mockContext.Permissions.AddRange(permissions);
        await _mockContext.SaveChangesAsync();

        List<Guid> requestedIds = permissions.Take(2).Select(p => p.Id).ToList();

        // Act
        ErrorOr<IEnumerable<PermissionEntity>> result = await _sut.GetByIdsAsync(requestedIds, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Select(p => p.Id).Should().BeEquivalentTo(requestedIds);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNoPermissionsMatch_ShouldReturnEmptyList()
    {
        // Arrange
        List<Guid> requestedIds = [Guid.NewGuid(), Guid.NewGuid()];

        // Act
        ErrorOr<IEnumerable<PermissionEntity>> result = await _sut.GetByIdsAsync(requestedIds, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
