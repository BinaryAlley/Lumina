#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Libraries;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Libraries;

/// <summary>
/// Contains integration tests for the <see cref="LibraryScanRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanRepositoryTests
{
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenAnEditableValueChanges_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-libraryscanrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        LibraryScanRepository sut = new(context);

        UserEntity storedUser = _userEntityFixture.Create(username: "scan-user", password: "Password123");
        context.Users.Add(storedUser);
        LibraryEntity storedLibrary = _libraryEntityFixture.Create(userId: storedUser.Id);
        context.Libraries.Add(storedLibrary);
        await context.SaveChangesAsync();

        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        LibraryScanEntity storedScan = _libraryScanEntityFixture.Create(libraryId: storedLibrary.Id, userId: storedUser.Id, status: LibraryScanJobStatus.Running, libraryEntity: storedLibrary);
        storedScan.CreatedOnUtc = storedCreatedOnUtc;
        storedScan.CreatedBy = storedCreatedBy;
        context.LibraryScans.Add(storedScan);
        await context.SaveChangesAsync();

        LibraryScanEntity updatedScan = _libraryScanEntityFixture.Create(id: storedScan.Id, libraryId: storedLibrary.Id, userId: storedUser.Id, status: LibraryScanJobStatus.Completed, libraryEntity: storedLibrary);
        updatedScan.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        updatedScan.CreatedBy = Guid.NewGuid();
        updatedScan.UpdatedOnUtc = DateTime.UtcNow.AddYears(-5);
        updatedScan.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await sut.UpdateAsync(updatedScan, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        LibraryScanEntity reloadedScan = await context.LibraryScans.AsNoTracking().FirstAsync(scan => scan.Id == storedScan.Id);
        Assert.Equal(LibraryScanJobStatus.Completed, reloadedScan.Status);
        Assert.Equal(storedCreatedOnUtc, reloadedScan.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, reloadedScan.CreatedBy);
        Assert.Null(reloadedScan.UpdatedOnUtc);
        Assert.Null(reloadedScan.UpdatedBy);
    }
}
