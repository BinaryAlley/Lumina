#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
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
/// Contains unit tests for the <see cref="LibraryRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly LibraryRepository _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryPathTemplatePartEntityFixture _libraryPathTemplatePartEntityFixture = new();

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
        Result<LibraryEntity?> result = await _sut.GetByIdAsync(library.Id, cancellationToken: CancellationToken.None);

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
        Result<LibraryEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), cancellationToken: CancellationToken.None);

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
        Result<PaginatedResultDto<LibraryEntity>> result = await _sut.GetAllAsync<BaseFilterDto>(cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(libraries, result.Value.Data);
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

    [Fact]
    public async Task InsertAsync_WhenLibraryHasPathTemplateParts_ShouldAddLibraryAndItsPartsToContext()
    {
        // Arrange
        LibraryPathTemplatePartEntity firstPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity secondPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Separator, representation: "/", isOptional: false);
        LibraryPathTemplatePartEntity thirdPart = _libraryPathTemplatePartEntityFixture.Create(position: 2, kind: LibraryPathPartKind.Series, representation: "{0}", isOptional: true);
        LibraryEntity library = _libraryEntityFixture.Create(
            contentLocations: [],
            pathTemplateParts: [firstPart, secondPart, thirdPart],
            pathTemplateFingerprint: "path-template-fingerprint");

        // Act
        Result<Created> result = await _sut.InsertAsync(library, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<LibraryEntity>? addedLibrary = _mockContext.ChangeTracker.Entries<LibraryEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Added && entityEntry.Entity.Id == library.Id);
        Assert.NotNull(addedLibrary);
        Assert.Equal("path-template-fingerprint", addedLibrary!.Entity.PathTemplateFingerprint);

        List<EntityEntry<LibraryPathTemplatePartEntity>> partEntries = [.. _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .Where(entityEntry => entityEntry.State == EntityState.Added)];
        Assert.Equal(3, partEntries.Count);
        Assert.Equal([0, 1, 2], partEntries.Select(entityEntry => entityEntry.Entity.Position).OrderBy(position => position));

        LibraryPathTemplatePartEntity storedFirstPart = partEntries.Single(entityEntry => entityEntry.Entity.Position == 0).Entity;
        Assert.Equal(LibraryPathPartKind.Artist, storedFirstPart.Kind);
        Assert.Equal("{0}", storedFirstPart.Representation);
        Assert.False(storedFirstPart.IsOptional);

        LibraryPathTemplatePartEntity storedSecondPart = partEntries.Single(entityEntry => entityEntry.Entity.Position == 1).Entity;
        Assert.Equal(LibraryPathPartKind.Separator, storedSecondPart.Kind);
        Assert.Equal("/", storedSecondPart.Representation);
        Assert.False(storedSecondPart.IsOptional);

        LibraryPathTemplatePartEntity storedThirdPart = partEntries.Single(entityEntry => entityEntry.Entity.Position == 2).Entity;
        Assert.Equal(LibraryPathPartKind.Series, storedThirdPart.Kind);
        Assert.Equal("{0}", storedThirdPart.Representation);
        Assert.True(storedThirdPart.IsOptional);
    }

    [Fact]
    public async Task UpdateAsync_WhenPathTemplatePartRepresentationChanges_ShouldReplaceThatPart()
    {
        // Arrange
        LibraryPathTemplatePartEntity keptPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity replacedPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Title, representation: "{0}", isOptional: false);
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: [], pathTemplateParts: [keptPart, replacedPart]);
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryPathTemplatePartEntity replacementPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Title, representation: "{1}", isOptional: false);
        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            contentLocations: [],
            pathTemplateParts: [keptPart, replacementPart]);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<LibraryPathTemplatePartEntity>? replacedEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, replacedPart));
        Assert.NotNull(replacedEntry);
        Assert.Equal(EntityState.Deleted, replacedEntry!.State);

        EntityEntry<LibraryPathTemplatePartEntity>? keptEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, keptPart));
        Assert.NotNull(keptEntry);
        Assert.Equal(EntityState.Unchanged, keptEntry!.State);

        Result<LibraryEntity?> retrievedResult = await _sut.GetByIdAsync(library.Id, cancellationToken: CancellationToken.None);
        Assert.False(retrievedResult.IsFailure);
        LibraryEntity retrievedLibrary = Assert.IsType<LibraryEntity>(retrievedResult.Value);
        Assert.Equal(2, retrievedLibrary.PathTemplateParts.Count);
        Assert.Equal("{0}", retrievedLibrary.PathTemplateParts.Single(part => part.Position == 0).Representation);
        Assert.Equal("{1}", retrievedLibrary.PathTemplateParts.Single(part => part.Position == 1).Representation);
    }

    [Fact]
    public async Task UpdateAsync_WhenPathTemplatePartKindChanges_ShouldReplaceThatPart()
    {
        // Arrange
        LibraryPathTemplatePartEntity keptPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity replacedPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Title, representation: "{0}", isOptional: false);
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: [], pathTemplateParts: [keptPart, replacedPart]);
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryPathTemplatePartEntity replacementPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Series, representation: "{0}", isOptional: false);
        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            contentLocations: [],
            pathTemplateParts: [keptPart, replacementPart]);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<LibraryPathTemplatePartEntity>? replacedEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, replacedPart));
        Assert.NotNull(replacedEntry);
        Assert.Equal(EntityState.Deleted, replacedEntry!.State);

        Result<LibraryEntity?> retrievedResult = await _sut.GetByIdAsync(library.Id, cancellationToken: CancellationToken.None);
        LibraryEntity retrievedLibrary = Assert.IsType<LibraryEntity>(retrievedResult.Value);
        Assert.Equal(LibraryPathPartKind.Series, retrievedLibrary.PathTemplateParts.Single(part => part.Position == 1).Kind);
    }

    [Fact]
    public async Task UpdateAsync_WhenPathTemplatePartIsOptionalChanges_ShouldReplaceThatPart()
    {
        // Arrange
        LibraryPathTemplatePartEntity keptPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity replacedPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Series, representation: "{0}", isOptional: false);
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: [], pathTemplateParts: [keptPart, replacedPart]);
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryPathTemplatePartEntity replacementPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Series, representation: "{0}", isOptional: true);
        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            contentLocations: [],
            pathTemplateParts: [keptPart, replacementPart]);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<LibraryPathTemplatePartEntity>? replacedEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, replacedPart));
        Assert.NotNull(replacedEntry);
        Assert.Equal(EntityState.Deleted, replacedEntry!.State);

        Result<LibraryEntity?> retrievedResult = await _sut.GetByIdAsync(library.Id, cancellationToken: CancellationToken.None);
        LibraryEntity retrievedLibrary = Assert.IsType<LibraryEntity>(retrievedResult.Value);
        Assert.True(retrievedLibrary.PathTemplateParts.Single(part => part.Position == 1).IsOptional);
    }

    [Fact]
    public async Task UpdateAsync_WhenPathTemplatePartPositionIsRemoved_ShouldRemoveThatPart()
    {
        // Arrange
        LibraryPathTemplatePartEntity firstPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity removedPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Title, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity thirdPart = _libraryPathTemplatePartEntityFixture.Create(position: 2, kind: LibraryPathPartKind.Series, representation: "{0}", isOptional: true);
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: [], pathTemplateParts: [firstPart, removedPart, thirdPart]);
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            contentLocations: [],
            pathTemplateParts: [firstPart, thirdPart]);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<LibraryPathTemplatePartEntity>? removedEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, removedPart));
        Assert.NotNull(removedEntry);
        Assert.Equal(EntityState.Deleted, removedEntry!.State);

        Result<LibraryEntity?> retrievedResult = await _sut.GetByIdAsync(library.Id, cancellationToken: CancellationToken.None);
        LibraryEntity retrievedLibrary = Assert.IsType<LibraryEntity>(retrievedResult.Value);
        Assert.Equal(2, retrievedLibrary.PathTemplateParts.Count);
        Assert.Equal([0, 2], retrievedLibrary.PathTemplateParts.Select(part => part.Position).OrderBy(position => position));
    }

    [Fact]
    public async Task UpdateAsync_WhenPathTemplatePartPositionIsAdded_ShouldAddThatPart()
    {
        // Arrange
        LibraryPathTemplatePartEntity firstPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity secondPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Title, representation: "{0}", isOptional: false);
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: [], pathTemplateParts: [firstPart, secondPart]);
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryPathTemplatePartEntity addedPart = _libraryPathTemplatePartEntityFixture.Create(position: 2, kind: LibraryPathPartKind.Series, representation: "{0}", isOptional: true);
        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            contentLocations: [],
            pathTemplateParts: [firstPart, secondPart, addedPart]);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<LibraryPathTemplatePartEntity>? addedEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, addedPart));
        Assert.NotNull(addedEntry);
        Assert.Equal(EntityState.Added, addedEntry!.State);

        Result<LibraryEntity?> retrievedResult = await _sut.GetByIdAsync(library.Id, cancellationToken: CancellationToken.None);
        LibraryEntity retrievedLibrary = Assert.IsType<LibraryEntity>(retrievedResult.Value);
        Assert.Equal(3, retrievedLibrary.PathTemplateParts.Count);
        Assert.Equal([0, 1, 2], retrievedLibrary.PathTemplateParts.Select(part => part.Position).OrderBy(position => position));

        LibraryPathTemplatePartEntity storedAddedPart = retrievedLibrary.PathTemplateParts.Single(part => part.Position == 2);
        Assert.Equal(LibraryPathPartKind.Series, storedAddedPart.Kind);
        Assert.Equal("{0}", storedAddedPart.Representation);
        Assert.True(storedAddedPart.IsOptional);
    }

    [Fact]
    public async Task UpdateAsync_WhenPathTemplatePartsAreUnchanged_ShouldKeepExistingParts()
    {
        // Arrange
        LibraryPathTemplatePartEntity firstPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity secondPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Title, representation: "{0}", isOptional: false);
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: [], pathTemplateParts: [firstPart, secondPart]);
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryPathTemplatePartEntity equivalentFirstPart = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity equivalentSecondPart = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.Title, representation: "{0}", isOptional: false);
        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            contentLocations: [],
            pathTemplateParts: [equivalentFirstPart, equivalentSecondPart]);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        Assert.DoesNotContain(
            _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>(),
            entityEntry => entityEntry.State == EntityState.Added || entityEntry.State == EntityState.Deleted);

        EntityEntry<LibraryPathTemplatePartEntity>? firstEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, firstPart));
        Assert.NotNull(firstEntry);
        Assert.Equal(EntityState.Unchanged, firstEntry!.State);

        EntityEntry<LibraryPathTemplatePartEntity>? secondEntry = _mockContext.ChangeTracker.Entries<LibraryPathTemplatePartEntity>()
            .SingleOrDefault(entityEntry => ReferenceEquals(entityEntry.Entity, secondPart));
        Assert.NotNull(secondEntry);
        Assert.Equal(EntityState.Unchanged, secondEntry!.State);

        Result<LibraryEntity?> retrievedResult = await _sut.GetByIdAsync(library.Id, cancellationToken: CancellationToken.None);
        LibraryEntity retrievedLibrary = Assert.IsType<LibraryEntity>(retrievedResult.Value);
        Assert.Equal(2, retrievedLibrary.PathTemplateParts.Count);
        Assert.Same(firstPart, retrievedLibrary.PathTemplateParts.Single(part => part.Position == 0));
        Assert.Same(secondPart, retrievedLibrary.PathTemplateParts.Single(part => part.Position == 1));
    }

    [Fact]
    public async Task UpdateAsync_WhenPathTemplateFingerprintChanges_ShouldUpdateIt()
    {
        // Arrange
        LibraryEntity library = _libraryEntityFixture.Create(contentLocations: [], pathTemplateParts: [], pathTemplateFingerprint: "old-fingerprint");
        _mockContext.Libraries.Add(library);
        await _mockContext.SaveChangesAsync();

        LibraryEntity updatedLibrary = _libraryEntityFixture.Create(
            id: library.Id,
            userId: library.UserId,
            contentLocations: [],
            pathTemplateParts: [],
            pathTemplateFingerprint: "new-fingerprint");

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedLibrary, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        LibraryEntity? retrievedLibrary = await _mockContext.Libraries.FindAsync(library.Id);
        Assert.NotNull(retrievedLibrary);
        Assert.Equal("new-fingerprint", retrievedLibrary!.PathTemplateFingerprint);
    }
}
