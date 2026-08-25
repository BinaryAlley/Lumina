#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.Repositories.Libraries;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Libraries;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly LibraryScanRepository _sut;
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanRepositoryTests"/> class.
    /// </summary>
    public LibraryScanRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new LibraryScanRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenLibraryScanDoesNotExist_ShouldAddScanToContextAndReturnCreated()
    {
        // Arrange
        LibraryScanEntity libraryScan = _libraryScanEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(libraryScan, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<LibraryScanEntity>? addedScan = _mockContext.ChangeTracker.Entries<LibraryScanEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Added && entityEntry.Entity.Id == libraryScan.Id);
        Assert.NotNull(addedScan);
    }

    [Fact]
    public async Task InsertAsync_WhenLibraryScanAlreadyExists_ShouldReturnError()
    {
        // Arrange
        LibraryScanEntity libraryScan = _libraryScanEntityFixture.Create();
        _mockContext.LibraryScans.Add(libraryScan);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(libraryScan, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryScanAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<LibraryScanEntity>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenLibraryScanExists_ShouldReturnScanWithLibraryAndUser()
    {
        // Arrange
        LibraryScanEntity libraryScan = _libraryScanEntityFixture.Create();
        _mockContext.LibraryScans.Add(libraryScan);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<LibraryScanEntity?> result = await _sut.GetByIdAsync(libraryScan.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(libraryScan.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLibraryScanDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<LibraryScanEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetPastMonthScansByLibraryIdAsync_WhenCalled_ShouldReturnOnlyRecentScansOfTheLibrary()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create();
        LibraryScanEntity recentScan = _libraryScanEntityFixture.Create(libraryId: library.Id, libraryEntity: library, createdOnUtc: DateTime.UtcNow.AddDays(-1));
        LibraryScanEntity oldScan = _libraryScanEntityFixture.Create(libraryId: library.Id, libraryEntity: library, createdOnUtc: DateTime.UtcNow.AddMonths(-2));
        LibraryScanEntity scanOfAnotherLibrary = _libraryScanEntityFixture.Create(createdOnUtc: DateTime.UtcNow.AddDays(-1));
        _mockContext.LibraryScans.AddRange(recentScan, oldScan, scanOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<LibraryScanEntity>> result = await _sut.GetPastMonthScansByLibraryIdAsync(library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryScanEntity retrievedScan = Assert.Single(result.Value);
        Assert.Equal(recentScan.Id, retrievedScan.Id);
    }

    [Fact]
    public async Task GetRunningScansAsync_WhenCalled_ShouldReturnOnlyRunningScans()
    {
        // Arrange
        LibraryScanEntity runningScan = _libraryScanEntityFixture.Create(status: LibraryScanJobStatus.Running);
        LibraryScanEntity completedScan = _libraryScanEntityFixture.Create(status: LibraryScanJobStatus.Completed);
        LibraryScanEntity pendingScan = _libraryScanEntityFixture.Create(status: LibraryScanJobStatus.Pending);
        _mockContext.LibraryScans.AddRange(runningScan, completedScan, pendingScan);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<LibraryScanEntity>> result = await _sut.GetRunningScansAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryScanEntity retrievedScan = Assert.Single(result.Value);
        Assert.Equal(runningScan.Id, retrievedScan.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenLibraryScanExists_ShouldUpdateItsScalarProperties()
    {
        // Arrange
        LibraryScanEntity libraryScan = _libraryScanEntityFixture.Create();
        _mockContext.LibraryScans.Add(libraryScan);
        await _mockContext.SaveChangesAsync();

        LibraryScanEntity updatedScan = _libraryScanEntityFixture.Create(id: libraryScan.Id, libraryId: libraryScan.LibraryId, userId: libraryScan.UserId, status: LibraryScanJobStatus.Completed, libraryEntity: libraryScan.Library, createdOnUtc: libraryScan.CreatedOnUtc);
        updatedScan.UpdatedOnUtc = DateTime.UtcNow;
        updatedScan.UpdatedBy = libraryScan.UserId;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedScan, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        LibraryScanEntity? retrievedScan = await _mockContext.LibraryScans.FindAsync(libraryScan.Id);
        Assert.NotNull(retrievedScan);
        Assert.Equal(LibraryScanJobStatus.Completed, retrievedScan!.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenLibraryScanDoesNotExist_ShouldReturnError()
    {
        // Arrange
        LibraryScanEntity libraryScan = _libraryScanEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(libraryScan, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryScanNotFound, result.FirstError);
    }
}
