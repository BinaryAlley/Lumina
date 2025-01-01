#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.DataAccess.Core.Repositories.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Authorization.Fixtures;
using Lumina.Domain.Common.Enums.Authorization;
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
/// Contains unit tests for the <see cref="UserRoleRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRoleRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly UserRoleRepository _sut;
    private readonly UserRoleEntityFixture _userRoleEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleRepositoryTests"/> class.
    /// </summary>
    public UserRoleRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new UserRoleRepository(_mockContext);
        _userRoleEntityFixture = new UserRoleEntityFixture();
    }

    [Fact]
    public async Task InsertAsync_WhenUserRoleIsValid_ShouldAddToContextAndReturnCreated()
    {
        // Arrange
        UserRoleEntity userRole = _userRoleEntityFixture.CreateUserRoleModel();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(userRole, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        EntityEntry<UserRoleEntity>? addedUserRole = _mockContext.ChangeTracker.Entries<UserRoleEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == userRole.Id);
        addedUserRole.Should().NotBeNull();
        addedUserRole!.State.Should().Be(EntityState.Added);
    }
}
