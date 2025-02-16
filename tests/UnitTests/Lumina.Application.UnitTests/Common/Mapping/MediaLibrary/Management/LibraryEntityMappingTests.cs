#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
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
    [Fact]
    public void ToResponse_WhenMappingValidLibraryEntity_ShouldMapCorrectly()
    {
        // Arrange
        LibraryEntity entity = new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "My Library",
            LibraryType = LibraryType.Book,
            ContentLocations =
            [
                new() { Path = "C:/Books" },
                new() { Path = "D:/Media/Books" }
            ],
            CoverImage = "D:/myPoster.jpg",
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null,
            LibraryScans = []
        };

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
        LibraryEntity entity = new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "My Library",
            LibraryType = libraryType,
            ContentLocations = [new() { Path = "C:/Media" }],
            CoverImage = "D:/myPoster.jpg",
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };

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
        LibraryEntity entity = new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "My Library",
            LibraryType = LibraryType.Book,
            ContentLocations =
            [
                new() { Path = "C:/Media/Books" },
                new() { Path = "D:/Books" },
                new() { Path = "E:/Digital Library/Books" },
                new() { Path = "F:/Reading Material" }
            ],
            CoverImage = "D:/myPoster.jpg",
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };

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
        LibraryEntity entity = new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "My Library",
            LibraryType = LibraryType.Book,
            ContentLocations = [new() { Path = "C:/Books" }],
            CoverImage = "D:/myPoster.jpg",
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = updated,
            UpdatedBy = Guid.NewGuid()
        };

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
        LibraryEntity entity = new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "My Library",
            LibraryType = LibraryType.Book,
            ContentLocations = [new() { Path = "C:/Books" }],
            CoverImage = null,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = updated,
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CoverImage);
    }
}
