#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Nemera.DataAccess.Core.Repositories.Authorization;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.UnitTests.Core.Repositories.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="RolePermissionRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionRepositoryTests
{
    private readonly NemeraDbContext _mockContext;
    private readonly RolePermissionRepository _sut;
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionRepositoryTests"/> class.
    /// </summary>
    public RolePermissionRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<NemeraDbContext>();
        _sut = new RolePermissionRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenCalled_ShouldAddRolePermissionToContextAndReturnCreated()
    {
        // Arrange
        RolePermissionEntity rolePermissionModel = _rolePermissionEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(rolePermissionModel, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        // Check if the role permission was added to the context's ChangeTracker.
        EntityEntry<RolePermissionEntity>? addedRolePermission = _mockContext.ChangeTracker.Entries<RolePermissionEntity>()
        .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == rolePermissionModel.Id);
        Assert.NotNull(addedRolePermission);
    }
}
