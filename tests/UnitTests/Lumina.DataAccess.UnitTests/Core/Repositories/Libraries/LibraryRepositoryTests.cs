#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.Repositories.Libraries;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
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

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Libraries;

/// <summary>
/// Contains unit tests for the <see cref="LibraryRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly LibraryRepository _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryRepositoryTests"/> class.
    /// </summary>
    public LibraryRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new LibraryRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenLibraryDoesNotExist_ShouldAddLibraryToContextAndReturnCreated()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(library, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<LibraryEntity>? addedLibrary = _mockContext.ChangeTracker.Entries<LibraryEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Added && entityEntry.Entity.Id == library.Id);
        Assert.NotNull(addedLibrary);
    }

    [Fact]
    public async Task InsertAsync_WhenLibraryAlreadyExists_ShouldReturnError()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create();
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(library, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<LibraryEntity>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenLibraryExists_ShouldReturnLibraryWithContentLocations()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create();
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<LibraryEntity?> result = await _sut.GetByIdAsync(library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(library.Id, result.Value!.Id);
        Assert.Equal(library.ContentLocations.Count, result.Value.ContentLocations.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLibraryDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<LibraryEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetAllEnabledAsync_WhenCalled_ShouldReturnOnlyEnabledLibraries()
    {
        // Arrange
        LibraryEntity enabledLibrary = _libraryEntityFixture.Create(isEnabled: true);
        LibraryEntity disabledLibrary = _libraryEntityFixture.Create(isEnabled: false);
        _mockContext.Libraries.AddRange(enabledLibrary, disabledLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<LibraryEntity>> result = await _sut.GetAllEnabledAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryEntity retrievedLibrary = Assert.Single(result.Value);
        Assert.Equal(enabledLibrary.Id, retrievedLibrary.Id);
    }

    [Fact]
    public async Task GetAllEnabledAndUnlockedAsync_WhenCalled_ShouldReturnOnlyEnabledAndUnlockedLibraries()
    {
        // Arrange
        LibraryEntity enabledUnlockedLibrary = _libraryEntityFixture.Create(isEnabled: true, isLocked: false);
        LibraryEntity enabledLockedLibrary = _libraryEntityFixture.Create(isEnabled: true, isLocked: true);
        LibraryEntity disabledUnlockedLibrary = _libraryEntityFixture.Create(isEnabled: false, isLocked: false);
        _mockContext.Libraries.AddRange(enabledUnlockedLibrary, enabledLockedLibrary, disabledUnlockedLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<LibraryEntity>> result = await _sut.GetAllEnabledAndUnlockedAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryEntity retrievedLibrary = Assert.Single(result.Value);
        Assert.Equal(enabledUnlockedLibrary.Id, retrievedLibrary.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllLibraries()
    {
        // Arrange
        List<LibraryEntity> libraries = _libraryEntityFixture.CreateMany(3);
        _mockContext.Libraries.AddRange(libraries);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<LibraryEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(libraries, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenLibraryExists_ShouldUpdateScalarPropertiesAndContentLocations()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: ["/old/path"]);
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            title: "Updated Title",
            libraryType: library.LibraryType,
            contentLocations: ["/new/path"]);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        LibraryEntity? retrievedLibrary = await _mockContext.Libraries.FindAsync(library.Id);
        Assert.NotNull(retrievedLibrary);
        Assert.Equal("Updated Title", retrievedLibrary!.Title);
        Assert.Single(retrievedLibrary.ContentLocations);
        Assert.Equal("/new/path", retrievedLibrary.ContentLocations.First().Path);
    }

    [Fact]
    public async Task UpdateAsync_WhenLibraryDoesNotExist_ShouldReturnError()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(library, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenLibraryExists_ShouldRemoveItFromContextAndReturnDeleted()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create();
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        EntityEntry<LibraryEntity>? deletedLibrary = _mockContext.ChangeTracker.Entries<LibraryEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Deleted && entityEntry.Entity.Id == library.Id);
        Assert.NotNull(deletedLibrary);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenLibraryDoesNotExist_ShouldReturnError()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
    }
}
