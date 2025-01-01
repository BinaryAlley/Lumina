#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.DataAccess.Core.Repositories.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Authorization.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="RolePermissionRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly RolePermissionRepository _sut;
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionRepositoryTests"/> class.
    /// </summary>
    public RolePermissionRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new RolePermissionRepository(_mockContext);
        _rolePermissionEntityFixture = new RolePermissionEntityFixture();
    }

    [Fact]
    public async Task InsertAsync_WhenCalled_ShouldAddRolePermissionToContextAndReturnCreated()
    {
        // Arrange
        RolePermissionEntity rolePermissionModel = _rolePermissionEntityFixture.CreateRolePermissionModel();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(rolePermissionModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        // check if the role permission was added to the context's ChangeTracker
        EntityEntry<RolePermissionEntity>? addedRolePermission = _mockContext.ChangeTracker.Entries<RolePermissionEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == rolePermissionModel.Id);
        addedRolePermission.Should().NotBeNull();
    }
}
