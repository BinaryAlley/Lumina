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
/// Contains unit tests for the <see cref="UserRoleRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRoleRepositoryTests
{
    private readonly NemeraDbContext _mockContext;
    private readonly UserRoleRepository _sut;
    private readonly UserRoleEntityFixture _userRoleEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleRepositoryTests"/> class.
    /// </summary>
    public UserRoleRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<NemeraDbContext>();
        _sut = new UserRoleRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenUserRoleIsValid_ShouldAddToContextAndReturnCreated()
    {
        // Arrange
        UserRoleEntity userRole = _userRoleEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(userRole, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<UserRoleEntity>? addedUserRole = _mockContext.ChangeTracker.Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == userRole.Id);
        Assert.NotNull(addedUserRole);
        Assert.Equal(EntityState.Added, addedUserRole!.State);
    }
}
