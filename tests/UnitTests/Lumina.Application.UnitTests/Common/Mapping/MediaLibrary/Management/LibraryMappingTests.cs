#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Id.Should().Be(library.Id.Value);
        result.UserId.Should().Be(library.UserId.Value);
        result.Title.Should().Be(library.Title);
        result.LibraryType.Should().Be(library.LibraryType);
        result.ContentLocations.Select(l => l.Path)
            .Should().BeEquivalentTo(library.ContentLocations.Select(l => l.Path));
        result.CoverImage.Should().BeEquivalentTo(library.CoverImage);
        result.CreatedOnUtc.Should().Be(library.CreatedOnUtc);
        result.UpdatedOnUtc.Should().Be(library.UpdatedOnUtc.HasValue ? library.UpdatedOnUtc : null);
        result.IsEnabled.Should().BeTrue();
        result.IsLocked.Should().BeFalse();
        result.DownloadMedatadaFromWeb.Should().BeTrue();
        result.SaveMetadataInMediaDirectories.Should().BeFalse();
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
        result.Should().NotBeNull();
        result.ContentLocations.Should().BeEmpty();
        result.CoverImage.Should().BeEquivalentTo(library.CoverImage);
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
        result.Should().NotBeNull();
        result.LibraryType.Should().Be(libraryType);
        result.CoverImage.Should().BeEquivalentTo(library.CoverImage);
        result.IsEnabled.Should().BeTrue();
        result.IsLocked.Should().BeFalse();
        result.DownloadMedatadaFromWeb.Should().BeTrue();
        result.SaveMetadataInMediaDirectories.Should().BeFalse();
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
        result.Should().NotBeNull();
        result.ContentLocations.Select(l => l.Path)
            .Should().BeEquivalentTo(library.ContentLocations.Select(l => l.Path));
        result.CoverImage.Should().BeEquivalentTo(library.CoverImage);
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
        result.Should().NotBeNull();
        result.ContentLocations.Select(l => l.Path)
            .Should().BeEquivalentTo(library.ContentLocations.Select(l => l.Path));
        result.CoverImage.Should().BeNull();
    }
}
