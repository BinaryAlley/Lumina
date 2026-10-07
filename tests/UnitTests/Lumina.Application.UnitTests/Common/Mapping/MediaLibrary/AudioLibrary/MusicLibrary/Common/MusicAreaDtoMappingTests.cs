#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MusicAreaDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAreaDtoMappingTests
{
    private readonly MusicAreaDtoFixture _musicAreaDtoFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingValidMusicAreaDto_ShouldMapCorrectly()
    {
        // Arrange
        MusicAreaDto dto = _musicAreaDtoFixture.Create(name: "United Kingdom", iso3166Code: "GB");

        // Act
        Result<Optional<MusicArea>> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value.HasValue);
        MusicArea area = result.Value.Value;
        Assert.Equal(dto.MusicBrainzAreaId, area.MusicBrainzAreaId.Value);
        Assert.Equal("United Kingdom", area.Name);
        Assert.Equal("GB", area.Iso3166Code.Value);
    }

    [Fact]
    public void ToDomainValueObject_WhenAreaDtoIsNull_ShouldReturnNoArea()
    {
        // Arrange
        MusicAreaDto? dto = null;

        // Act
        Result<Optional<MusicArea>> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.HasValue);
    }

    [Fact]
    public void ToDomainValueObject_WhenMusicBrainzAreaIdIsMissing_ShouldReturnNoArea()
    {
        // Arrange
        MusicAreaDto dto = _musicAreaDtoFixture.Create(includeMusicBrainzAreaId: false);

        // Act
        Result<Optional<MusicArea>> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.HasValue);
    }

    [Theory]
    [InlineData(null)] // missing name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void ToDomainValueObject_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Arrange
        MusicAreaDto dto = _musicAreaDtoFixture.Create(name: name, includeName: name is not null);

        // Act
        Result<Optional<MusicArea>> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AreaNameCannotBeEmpty, result.FirstError);
    }
}
