#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="LibraryEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryEntityMappingTests
{
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidLibraryEntity_ShouldMapCorrectly()
    {
        // Arrange
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books", "D:/Media/Books"],
            coverImage: "D:/myPoster.jpg");

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.UserId, result.UserId);
        Assert.Equal(entity.Title, result.Title);
        Assert.Equal(entity.LibraryType, result.LibraryType);
        Assert.Equal(entity.ContentLocations.Select(l => l.Path), result.ContentLocations);
        Assert.Equal(entity.CoverImage, result.CoverImage);
        Assert.Equal(entity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, result.UpdatedOnUtc);
    }

    [Theory]
    [InlineData(LibraryType.Book)]
    [InlineData(LibraryType.Movie)]
    [InlineData(LibraryType.TvShow)]
    [InlineData(LibraryType.Music)]
    public void ToResponse_WhenMappingDifferentLibraryTypes_ShouldMapCorrectly(LibraryType libraryType)
    {
        // Arrange
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: libraryType,
            contentLocations: ["C:/Media"],
            coverImage: "D:/myPoster.jpg");

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryType, result.LibraryType);
    }

    [Fact]
    public void ToResponse_WhenMappingMultipleContentLocations_ShouldMapAllCorrectly()
    {
        // Arrange
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Media/Books", "D:/Books", "E:/Digital Library/Books", "F:/Reading Material"],
            coverImage: "D:/myPoster.jpg");

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.ContentLocations.Select(l => l.Path), result.ContentLocations);
    }

    [Fact]
    public void ToResponse_WhenMappingWithUpdatedDateTime_ShouldMapCorrectly()
    {
        // Arrange
        DateTime updated = DateTime.UtcNow.AddDays(-1);
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books"],
            coverImage: "D:/myPoster.jpg");
        entity.UpdatedOnUtc = updated;

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updated, result.UpdatedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenMappingWithNullCoverImage_ShouldMapCorrectly()
    {
        // Arrange
        DateTime updated = DateTime.UtcNow.AddDays(-1);
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books"]);
        entity.CoverImage = null;
        entity.UpdatedOnUtc = updated;

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CoverImage);
    }
}
