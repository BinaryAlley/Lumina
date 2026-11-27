#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.DataAccess.Core.Repositories.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Authorization;

/// <summary>
/// Contains integration tests for the <see cref="RoleRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleRepositoryTests
{
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenAnEditableValueChanges_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-rolerepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        RoleRepository sut = new(context);

        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        RoleEntity storedRole = _roleEntityFixture.Create(roleName: "Admin", createdOnUtc: storedCreatedOnUtc, createdBy: storedCreatedBy);
        context.Roles.Add(storedRole);
        await context.SaveChangesAsync();

        RoleEntity updatedRole = _roleEntityFixture.Create(id: storedRole.Id, roleName: "SuperAdmin", createdOnUtc: DateTime.UtcNow.AddYears(-10), createdBy: Guid.NewGuid());
        updatedRole.UpdatedOnUtc = DateTime.UtcNow.AddYears(-5);
        updatedRole.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await sut.UpdateAsync(updatedRole, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        RoleEntity reloadedRole = await context.Roles.AsNoTracking().FirstAsync(role => role.Id == storedRole.Id);
        Assert.Equal("SuperAdmin", reloadedRole.RoleName);
        Assert.Equal(storedCreatedOnUtc, reloadedRole.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, reloadedRole.CreatedBy);
        Assert.Null(reloadedRole.UpdatedOnUtc);
        Assert.Null(reloadedRole.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotDuplicateThePermissions()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-rolerepo-permissions-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        RoleRepository sut = new(context);

        PermissionEntity firstPermission = _permissionEntityFixture.Create();
        PermissionEntity secondPermission = _permissionEntityFixture.Create();
        RoleEntity storedRole = _roleEntityFixture.Create(roleName: $"perm-role-{Guid.NewGuid():N}");
        context.Permissions.AddRange(firstPermission, secondPermission);
        context.Roles.Add(storedRole);
        await context.SaveChangesAsync();
        storedRole.RolePermissions.Add(_rolePermissionEntityFixture.Create(role: storedRole, permission: firstPermission));
        storedRole.RolePermissions.Add(_rolePermissionEntityFixture.Create(role: storedRole, permission: secondPermission));
        await context.SaveChangesAsync();
        // Detach the seeded role, so that the update must load it through its include chain instead of returning the already populated seeded instance.
        context.ChangeTracker.Clear();

        RoleEntity incoming = await context.Roles.AsNoTracking().Include(candidate => candidate.RolePermissions).FirstAsync(candidate => candidate.Id == storedRole.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        RoleEntity? reloadedRole = await context.Roles.AsNoTracking().Include(candidate => candidate.RolePermissions).FirstOrDefaultAsync(candidate => candidate.Id == storedRole.Id);
        Assert.NotNull(reloadedRole);
        Assert.Equal(2, reloadedRole!.RolePermissions.Count);
    }
}
