#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="WrittenContentMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentMetadataDtoMappingTests
{
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingCompleteWrittenContentMetadataDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        WrittenContentMetadataDto dto = _writtenContentMetadataDtoFixture.Create();

        // Act
        Result<WrittenContentMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.Title, result.Value.Title);
        Assert.True(result.Value.OriginalTitle.HasValue);
        Assert.Equal(dto.OriginalTitle, result.Value.OriginalTitle.Value);
        Assert.True(result.Value.Description.HasValue);
        Assert.Equal(dto.Description, result.Value.Description.Value);
        Assert.Equal(dto.Genres!.Count, result.Value.Genres.Count);
        Assert.Equal(dto.Tags!.Count, result.Value.Tags.Count);
        Assert.True(result.Value.Language.HasValue);
        Assert.Equal(dto.Language!.LanguageCode!.ToLowerInvariant(), result.Value.Language.Value.LanguageCode);
        Assert.True(result.Value.OriginalLanguage.HasValue);
        Assert.Equal(dto.OriginalLanguage!.LanguageCode!.ToLowerInvariant(), result.Value.OriginalLanguage.Value.LanguageCode);
        Assert.True(result.Value.Publisher.HasValue);
        Assert.Equal(dto.Publisher, result.Value.Publisher.Value);
        Assert.True(result.Value.PageCount.HasValue);
        Assert.Equal(dto.PageCount!.Value, result.Value.PageCount.Value);
    }

    [Fact]
    public void ToDomainValueObject_WhenOptionalValuesAreMissing_ShouldMapWithoutOptionalValues()
    {
        // Arrange
        WrittenContentMetadataDto dto = _writtenContentMetadataDtoFixture.Create(
            includeOriginalTitle: false,
            includeDescription: false,
            includeLanguage: false,
            includeOriginalLanguage: false,
            includePublisher: false,
            includePageCount: false);

        // Act
        Result<WrittenContentMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.OriginalTitle.HasValue);
        Assert.False(result.Value.Description.HasValue);
        Assert.False(result.Value.Language.HasValue);
        Assert.False(result.Value.OriginalLanguage.HasValue);
        Assert.False(result.Value.Publisher.HasValue);
        Assert.False(result.Value.PageCount.HasValue);
    }

    [Fact]
    public void ToDomainValueObject_WhenReleaseInfoCreationFails_ShouldReturnError()
    {
        // Arrange
        WrittenContentMetadataDto dto = _writtenContentMetadataDtoFixture.Create(
            releaseInfo: _releaseInfoDtoFixture.Create(
                originalReleaseDate: new DateOnly(2025, 1, 1),
                originalReleaseYear: 2024));

        // Act
        Result<WrittenContentMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainValueObject_WhenGenreCreationFails_ShouldReturnError()
    {
        // Arrange
        WrittenContentMetadataDto dto = _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: "")]);

        // Act
        Result<WrittenContentMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainValueObject_WhenTagCreationFails_ShouldReturnError()
    {
        // Arrange
        WrittenContentMetadataDto dto = _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: "")]);

        // Act
        Result<WrittenContentMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
    }
}
