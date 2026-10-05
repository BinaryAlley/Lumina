#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="LibraryEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryEntityMappingTests
{
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryPathTemplatePartEntityFixture _libraryPathTemplatePartEntityFixture = new();

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

    [Fact]
    public void ToResponse_WhenLibraryHasPathTemplateParts_ShouldOrderByPositionAndMapProperties()
    {
        // Arrange
        LibraryPathTemplatePartEntity firstPart = _libraryPathTemplatePartEntityFixture.Create(
            position: 2,
            kind: LibraryPathPartKind.Artist,
            representation: "{0}",
            isOptional: true);
        LibraryPathTemplatePartEntity secondPart = _libraryPathTemplatePartEntityFixture.Create(
            position: 0,
            kind: LibraryPathPartKind.Literal,
            representation: "Various Artists");
        LibraryPathTemplatePartEntity thirdPart = _libraryPathTemplatePartEntityFixture.Create(
            position: 1,
            kind: LibraryPathPartKind.Separator,
            representation: string.Empty);
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books"],
            pathTemplateParts: [firstPart, secondPart, thirdPart]);

        // Act
        LibraryResponse result = entity.ToResponse();

        // Assert
        Assert.Equal(3, result.PathTemplateParts.Count);
        Assert.Equal(LibraryPathPartKind.Literal.ToString(), result.PathTemplateParts[0].Kind);
        Assert.Equal("Various Artists", result.PathTemplateParts[0].Representation);
        Assert.False(result.PathTemplateParts[0].IsOptional);
        Assert.Equal(LibraryPathPartKind.Separator.ToString(), result.PathTemplateParts[1].Kind);
        Assert.Equal(string.Empty, result.PathTemplateParts[1].Representation);
        Assert.False(result.PathTemplateParts[1].IsOptional);
        Assert.Equal(LibraryPathPartKind.Artist.ToString(), result.PathTemplateParts[2].Kind);
        Assert.Equal("{0}", result.PathTemplateParts[2].Representation);
        Assert.True(result.PathTemplateParts[2].IsOptional);
    }

    [Fact]
    public void ToDomainEntity_WhenLibraryHasPathTemplateParts_ShouldBuildTemplateFromOrderedParts()
    {
        // Arrange
        LibraryPathTemplatePartEntity firstPart = _libraryPathTemplatePartEntityFixture.Create(
            position: 1,
            kind: LibraryPathPartKind.Artist,
            representation: "{0}",
            isOptional: true);
        LibraryPathTemplatePartEntity secondPart = _libraryPathTemplatePartEntityFixture.Create(
            position: 0,
            kind: LibraryPathPartKind.Literal,
            representation: "Various Artists");
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books"],
            coverImage: "D:/cover.jpg",
            pathTemplateParts: [firstPart, secondPart]);

        // Act
        Result<Library> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(entity.Id, result.Value.Id.Value);
        Assert.Equal(entity.UserId, result.Value.UserId.Value);
        Assert.Equal(entity.Title, result.Value.Title);
        Assert.Equal(entity.LibraryType, result.Value.LibraryType);
        Assert.Equal("D:/cover.jpg", result.Value.CoverImage.Value);
        Assert.Equal(2, result.Value.PathTemplate.Parts.Count);
        Assert.Equal(LibraryPathPartKind.Literal, result.Value.PathTemplate.Parts[0].Kind);
        Assert.Equal("Various Artists", result.Value.PathTemplate.Parts[0].Representation);
        Assert.False(result.Value.PathTemplate.Parts[0].IsOptional);
        Assert.Equal(LibraryPathPartKind.Artist, result.Value.PathTemplate.Parts[1].Kind);
        Assert.Equal("{0}", result.Value.PathTemplate.Parts[1].Representation);
        Assert.True(result.Value.PathTemplate.Parts[1].IsOptional);
    }

    [Fact]
    public void ToDomainEntity_WhenStoredTypedPartHasEmptyRepresentation_ShouldReturnPathTemplateLiteralCannotBeEmpty()
    {
        // Arrange
        LibraryPathTemplatePartEntity invalidPart = _libraryPathTemplatePartEntityFixture.Create(
            position: 0,
            kind: LibraryPathPartKind.Artist,
            representation: string.Empty);
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books"],
            pathTemplateParts: [invalidPart]);

        // Act
        Result<Library> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplateLiteralCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenStoredTypedPartHasNoPlaceholder_ShouldReturnPathTemplateValuePartMustContainSinglePlaceholder()
    {
        // Arrange
        LibraryPathTemplatePartEntity invalidPart = _libraryPathTemplatePartEntityFixture.Create(
            position: 0,
            kind: LibraryPathPartKind.TrackNumber,
            representation: "Track");
        LibraryEntity entity = _libraryEntityFixture.Create(
            title: "My Library",
            libraryType: LibraryType.Book,
            contentLocations: ["C:/Books"],
            pathTemplateParts: [invalidPart]);

        // Act
        Result<Library> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplateValuePartMustContainSinglePlaceholder, result.FirstError);
    }
}
