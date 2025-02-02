#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
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
    [Fact]
    public void ToRepositoryEntity_WhenMappingValidLibrary_ShouldMapCorrectly()
    {
        // Arrange
        ErrorOr<Library> libraryResult = Library.Create(
            Guid.NewGuid(),
            "Test Library",
            LibraryType.Book,
            ["C:/Books", "D:/Media/Books"],
            "D:/myPoster.jpg",
            true,
            false,
            true,
            false
        );
        Library library = libraryResult.Value;

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
        Assert.True(result.DownloadMedatadaFromWeb);
        Assert.False(result.SaveMetadataInMediaDirectories);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingLibraryWithEmptyContentLocations_ShouldMapCorrectly()
    {
        // Arrange
        ErrorOr<Library> libraryResult = Library.Create(
            Guid.NewGuid(),
            "Empty Library",
            LibraryType.Book,
            [],
            "D:/myPoster.jpg",
            true,
            false,
            true,
            false
        );
        Library library = libraryResult.Value;

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
        ErrorOr<Library> libraryResult = Library.Create(
            Guid.NewGuid(),
            "Test Library",
            libraryType,
            ["C:/Media"],
            "D:/myPoster.jpg",
            true,
            false,
            true,
            false
        );
        Library library = libraryResult.Value;

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryType, result.LibraryType);
        Assert.Equal(library.CoverImage, result.CoverImage);
        Assert.True(result.IsEnabled);
        Assert.False(result.IsLocked);
        Assert.True(result.DownloadMedatadaFromWeb);
        Assert.False(result.SaveMetadataInMediaDirectories);
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

        ErrorOr<Library> libraryResult = Library.Create(
            Guid.NewGuid(),
            "Test Library",
            LibraryType.Book,
            contentLocations,
            "D:/myPoster.jpg",
            true,
            false,
            true,
            false
        );
        Library library = libraryResult.Value;

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

        ErrorOr<Library> libraryResult = Library.Create(
            Guid.NewGuid(),
            "Test Library",
            LibraryType.Book,
            contentLocations,
            null,
            true,
            false,
            true,
            false
        );
        Library library = libraryResult.Value;

        // Act
        LibraryEntity result = library.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(library.ContentLocations.Select(l => l.Path), result.ContentLocations.Select(l => l.Path));
        Assert.Null(result.CoverImage);
    }
}
