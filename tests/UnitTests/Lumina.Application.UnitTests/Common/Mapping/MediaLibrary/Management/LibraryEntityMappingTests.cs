#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
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
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.UserId.Should().Be(entity.UserId);
        result.Title.Should().Be(entity.Title);
        result.LibraryType.Should().Be(entity.LibraryType);
        result.ContentLocations.Should().BeEquivalentTo(entity.ContentLocations.Select(l => l.Path));
        result.CreatedOnUtc.Should().Be(entity.CreatedOnUtc);
        result.UpdatedOnUtc.Should().Be(entity.UpdatedOnUtc);
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
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.LibraryType.Should().Be(libraryType);
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
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.ContentLocations.Should().BeEquivalentTo(entity.ContentLocations.Select(l => l.Path));
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
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = updated,
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.UpdatedOnUtc.Should().Be(updated);
    }
}
