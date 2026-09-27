#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Users;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Users;

/// <summary>
/// Contains integration tests for the <see cref="UserRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRepositoryTests
{
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();
    private readonly UserPermissionEntityFixture _userPermissionEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenAnEditableValueChanges_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-userrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        UserRepository sut = new(context);

        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        UserEntity storedUser = _userEntityFixture.Create(username: "audit-user", password: "Password123");
        storedUser.CreatedOnUtc = storedCreatedOnUtc;
        storedUser.CreatedBy = storedCreatedBy;
        context.Users.Add(storedUser);
        await context.SaveChangesAsync();

        UserEntity updatedUser = _userEntityFixture.Create(id: storedUser.Id, username: "audit-user", password: "NewPassword123");
        updatedUser.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        updatedUser.CreatedBy = Guid.NewGuid();
        updatedUser.UpdatedOnUtc = DateTime.UtcNow.AddYears(-5);
        updatedUser.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await sut.UpdateAsync(updatedUser, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        UserEntity reloadedUser = await context.Users.AsNoTracking().FirstAsync(user => user.Id == storedUser.Id);
        Assert.Equal("NewPassword123", reloadedUser.Password);
        Assert.Equal(storedCreatedOnUtc, reloadedUser.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, reloadedUser.CreatedBy);
        Assert.Null(reloadedUser.UpdatedOnUtc);
        Assert.Null(reloadedUser.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotDuplicateThePermissions()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-userrepo-permissions-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        UserRepository sut = new(context);

        PermissionEntity firstPermission = _permissionEntityFixture.Create();
        PermissionEntity secondPermission = _permissionEntityFixture.Create();
        UserEntity storedUser = _userEntityFixture.Create(username: $"perm-user-{Guid.NewGuid():N}", password: "Password123");
        context.Permissions.AddRange(firstPermission, secondPermission);
        context.Users.Add(storedUser);
        await context.SaveChangesAsync();
        storedUser.UserPermissions.Add(_userPermissionEntityFixture.Create(storedUser, firstPermission));
        storedUser.UserPermissions.Add(_userPermissionEntityFixture.Create(storedUser, secondPermission));
        await context.SaveChangesAsync();
        // Detach the seeded user, so that the update must load it through its include chain instead of returning the already populated seeded instance.
        context.ChangeTracker.Clear();

        UserEntity incoming = await context.Users.AsNoTracking().Include(candidate => candidate.UserPermissions).FirstAsync(candidate => candidate.Id == storedUser.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        UserEntity? reloadedUser = await context.Users.AsNoTracking().Include(candidate => candidate.UserPermissions).FirstOrDefaultAsync(candidate => candidate.Id == storedUser.Id);
        Assert.NotNull(reloadedUser);
        Assert.Equal(2, reloadedUser!.UserPermissions.Count);
    }
}
