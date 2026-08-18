#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="LibraryMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMappingTests
{
    private readonly LibraryFixture _libraryFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingValidLibrary_ShouldMapCorrectly()
    {
        // Arrange
        Library library = _libraryFixture.Create(
            title: "Test Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books", "D:/Media/Books"],
            coverImage: "D:/myPoster.jpg",
            isEnabled: true,
            isLocked: false,
            downloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false,
            scanIds: [Guid.NewGuid()]);

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(library.Id.Value, result.Id);
        Assert.Equal(library.UserId.Value, result.UserId);
        Assert.Equal(library.Title, result.Title);
        Assert.Equal(library.LibraryType, result.LibraryType);
        Assert.Equal(library.ContentLocations.Select(l => l.Path), result.ContentLocations.Select(l => l.Path));
        Assert.Equal(library.CoverImage, result.CoverImage);
        Assert.Equal(library.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(library.UpdatedOnUtc, result.UpdatedOnUtc);
        Assert.True(result.IsEnabled);
        Assert.False(result.IsLocked);
        Assert.True(result.DownloadMetadataFromWeb);
        Assert.False(result.ShouldSaveMetadataInMediaDirectories);
        Assert.Equal(library.ScanIds.Select(scanId => scanId.Value), result.LibraryScans.Select(libraryScan => libraryScan.Id));
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingLibraryWithEmptyContentLocations_ShouldMapCorrectly()
    {
        // Arrange
        Library library = _libraryFixture.Create(
            title: "Empty Library",
            libraryType: LibraryType.Book,
            contentLocations: [],
            coverImage: "D:/myPoster.jpg");

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.ContentLocations);
        Assert.Equal(library.CoverImage, result.CoverImage);
    }

    [Theory]
    [InlineData(LibraryType.Book)]
    [InlineData(LibraryType.Movie)]
    [InlineData(LibraryType.TvShow)]
    [InlineData(LibraryType.Music)]
    public void ToRepositoryEntity_WhenMappingDifferentLibraryTypes_ShouldMapCorrectly(LibraryType libraryType)
    {
        // Arrange
        Library library = _libraryFixture.Create(
            title: "Test Library",
            libraryType: libraryType,
            contentLocations: ["C:/Media"],
            coverImage: "D:/myPoster.jpg",
            isEnabled: true,
            isLocked: false,
            downloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false);

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryType, result.LibraryType);
        Assert.Equal(library.CoverImage, result.CoverImage);
        Assert.True(result.IsEnabled);
        Assert.False(result.IsLocked);
        Assert.True(result.DownloadMetadataFromWeb);
        Assert.False(result.ShouldSaveMetadataInMediaDirectories);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingMultipleContentLocations_ShouldMapAllCorrectly()
    {
        // Arrange
        List<string> contentLocations =
        [
            "C:/Media/Books",
            "D:/Books",
            "E:/Digital Library/Books",
            "F:/Reading Material"
        ];

        Library library = _libraryFixture.Create(
            title: "Test Library",
            libraryType: LibraryType.Book,
            contentLocations: contentLocations,
            coverImage: "D:/myPoster.jpg");

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(library.ContentLocations.Select(l => l.Path), result.ContentLocations.Select(l => l.Path));
        Assert.Equal(library.CoverImage, result.CoverImage);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingNullCoverimage_ShouldMapCorrectly()
    {
        // Arrange
        List<string> contentLocations =
        [
            "C:/Media/Books",
            "D:/Books"
        ];

        Library library = _libraryFixture.Create(
            title: "Test Library",
            libraryType: LibraryType.Book,
            contentLocations: contentLocations,
            includeCoverImage: false);

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(library.ContentLocations.Select(l => l.Path), result.ContentLocations.Select(l => l.Path));
        Assert.Null(result.CoverImage);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingEmptyScanList_ShouldMapCorrectly()
    {
        // Arrange
        List<string> contentLocations =
        [
            "C:/Media/Books",
            "D:/Books"
        ];

        Library library = _libraryFixture.Create(
            title: "Test Library",
            libraryType: LibraryType.Book,
            contentLocations: contentLocations,
            includeCoverImage: false,
            scanIds: []);

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(library.ScanIds);
        Assert.NotNull(result.LibraryScans);
    }
}
