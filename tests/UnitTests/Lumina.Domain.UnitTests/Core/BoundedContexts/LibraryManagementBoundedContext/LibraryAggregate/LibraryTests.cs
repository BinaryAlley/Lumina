#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Library"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryTests
{
    private readonly LibraryFixture _libraryFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateLibraryWithAllPropertiesSet()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        string title = "My Books";
        LibraryType libraryType = LibraryType.Book;
        List<string> contentLocations = ["C:/Media/Books", "D:/Books"];
        string coverImageSourcePath = "C:/Cover/cover.jpg";
        List<ScanId> scanIds = [_scanIdFixture.Create(), _scanIdFixture.Create()];

        // Act
        Result<Library> result = Library.Create(
            userId,
            title,
            libraryType,
            contentLocations,
            coverImageSourcePath,
            isEnabled: false,
            isLocked: true,
            canDownloadMetadataFromWeb: false,
            shouldSaveMetadataInMediaDirectories: true,
            shouldSkipUnchangedDirectoriesDuringScan: true,
            scanIds);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(title, result.Value.Title);
        Assert.Equal(libraryType, result.Value.LibraryType);
        Assert.Equal(2, result.Value.ContentLocations.Count);
        Assert.Equal(coverImageSourcePath, result.Value.CoverImage);
        Assert.False(result.Value.IsEnabled);
        Assert.True(result.Value.IsLocked);
        Assert.False(result.Value.CanDownloadMetadataFromWeb);
        Assert.True(result.Value.ShouldSaveMetadataInMediaDirectories);
        Assert.True(result.Value.ShouldSkipUnchangedDirectoriesDuringScan);
        Assert.Equal(scanIds, result.Value.ScanIds);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingId_ShouldCreateLibraryWithThatId()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        List<ScanId> scanIds = [_scanIdFixture.Create()];

        // Act
        Result<Library> result = Library.Create(
            _libraryIdFixture.Create(id),
            _userIdFixture.Create(),
            "My Library",
            LibraryType.Movie,
            ["C:/Media"],
            null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false,
            scanIds);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id.Value);
    }

    [Fact]
    public void Create_WhenGivenPreExistingIdAndInvalidContentLocation_ShouldReturnError()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<Library> result = Library.Create(
            _libraryIdFixture.Create(id),
            _userIdFixture.Create(),
            "My Library",
            LibraryType.Book,
            [""],
            null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false,
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Lumina.Domain.Common.Errors.Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldLeaveCoverImageSourcePathUnset()
    {
        // Arrange
        Library library = _libraryFixture.Create();

        // Act
        string? coverImageSourcePath = library.CoverImageSourcePath;

        // Assert
        Assert.Null(coverImageSourcePath);
    }

    [Fact]
    public void Create_WhenCalledWithContentLocations_ShouldCreateFileSystemPathIdForEachLocation()
    {
        // Act
        Result<Library> result = Library.Create(
            _userIdFixture.Create(),
            "My Library",
            LibraryType.Book,
            ["C:/Media/Books", "D:/Books"],
            null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false,
            []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.ContentLocations.Count);
        Assert.Contains(result.Value.ContentLocations, location => location.Path == "C:/Media/Books");
        Assert.Contains(result.Value.ContentLocations, location => location.Path == "D:/Books");
    }

    [Theory]
    [InlineData(null)] // null content location
    [InlineData("")] // empty content location
    [InlineData("   ")] // whitespace content location
    public void Create_WhenContentLocationIsInvalid_ShouldReturnError(string? contentLocation)
    {
        // Act
        Result<Library> result = Library.Create(
            _userIdFixture.Create(),
            "My Library",
            LibraryType.Book,
            [contentLocation!],
            null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false,
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Lumina.Domain.Common.Errors.Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void Save_WhenCalled_ShouldRaiseLibrarySavedDomainEvent()
    {
        // Arrange
        Library library = _libraryFixture.Create();

        // Act
        library.Save();

        // Assert
        List<IDomainEvent> domainEvents = library.GetDomainEvents();
        LibrarySavedDomainEvent savedEvent = Assert.IsType<LibrarySavedDomainEvent>(Assert.Single(domainEvents));
        Assert.Equal(library, savedEvent.Library);
        Assert.NotEqual(default, savedEvent.Id);
        Assert.NotEqual(default, savedEvent.OccurredOnUtc);
    }

    [Fact]
    public void Delete_WhenCalled_ShouldRaiseLibraryDeletedDomainEvent()
    {
        // Arrange
        Library library = _libraryFixture.Create();

        // Act
        Result<Deleted> result = library.Delete();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        List<IDomainEvent> domainEvents = library.GetDomainEvents();
        LibraryDeletedDomainEvent deletedEvent = Assert.IsType<LibraryDeletedDomainEvent>(Assert.Single(domainEvents));
        Assert.Equal(library, deletedEvent.Library);
    }

    [Fact]
    public void Delete_WhenCalledAfterSave_ShouldClearPreviousEventsAndRaiseOnlyDeletedEvent()
    {
        // Arrange
        Library library = _libraryFixture.Create();
        library.Save();

        // Act
        library.Delete();

        // Assert
        List<IDomainEvent> domainEvents = library.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<LibraryDeletedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void Delete_WhenCalledMultipleTimes_ShouldRaiseOnlyOneDeletedEvent()
    {
        // Arrange
        Library library = _libraryFixture.Create();

        // Act
        library.Delete();
        library.Delete();

        // Assert
        List<IDomainEvent> domainEvents = library.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<LibraryDeletedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void SetInternalLibraryCoverImagePath_WhenCalled_ShouldUpdateCoverImage()
    {
        // Arrange
        Library library = _libraryFixture.Create();
        string internalPath = "C:/Internal/Covers/cover.jpg";

        // Act
        library.SetInternalLibraryCoverImagePath(internalPath);

        // Assert
        Assert.Equal(internalPath, library.CoverImage);
    }
}
