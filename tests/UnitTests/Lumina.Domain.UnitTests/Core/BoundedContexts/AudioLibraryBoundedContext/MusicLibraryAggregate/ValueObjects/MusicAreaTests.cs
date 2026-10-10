#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MusicArea"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAreaTests
{
    private readonly MusicAreaFixture _musicAreaFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateMusicAreaWithAllPropertiesSet()
    {
        // Arrange
        MusicBrainzId areaId = _musicBrainzIdFixture.Create(value: Guid.Parse("f0e9c1a2-0000-0000-0000-000000000000"));

        // Act
        Result<MusicArea> result = MusicArea.Create(
            areaId,
            "United Kingdom",
            Optional<string>.Some("UK"),
            Optional<string>.Some("The United Kingdom of Great Britain and Northern Ireland"),
            Optional<string>.Some("Country"),
            Optional<string>.Some("GB"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(areaId, result.Value.MusicBrainzAreaId);
        Assert.Equal("United Kingdom", result.Value.Name);
        Assert.Equal(Optional<string>.Some("UK"), result.Value.SortName);
        Assert.Equal(Optional<string>.Some("The United Kingdom of Great Britain and Northern Ireland"), result.Value.Disambiguation);
        Assert.Equal(Optional<string>.Some("Country"), result.Value.Type);
        Assert.Equal(Optional<string>.Some("GB"), result.Value.Iso3166Code);
    }

    [Fact]
    public void Create_WhenNameHasSurroundingWhitespace_ShouldTrimIt()
    {
        // Act
        Result<MusicArea> result = MusicArea.Create(
            _musicBrainzIdFixture.Create(),
            "  United Kingdom  ",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("United Kingdom", result.Value.Name);
    }

    [Theory]
    [InlineData(null)] // null area name
    [InlineData("")] // empty area name
    [InlineData("   ")] // whitespace area name
    public void Create_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Act
        Result<MusicArea> result = MusicArea.Create(
            _musicBrainzIdFixture.Create(),
            name!,
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.AreaNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WhenOptionalValuesAreAbsent_ShouldCreateMusicAreaWithoutThem()
    {
        // Act
        Result<MusicArea> result = MusicArea.Create(
            _musicBrainzIdFixture.Create(),
            "United Kingdom",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.SortName.HasValue);
        Assert.False(result.Value.Disambiguation.HasValue);
        Assert.False(result.Value.Type.HasValue);
        Assert.False(result.Value.Iso3166Code.HasValue);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MusicArea firstArea = _musicAreaFixture.Create();

        // Act
        MusicArea secondArea = _musicAreaFixture.Create(
            musicBrainzAreaId: firstArea.MusicBrainzAreaId,
            name: firstArea.Name,
            sortName: firstArea.SortName,
            disambiguation: firstArea.Disambiguation,
            type: firstArea.Type,
            iso3166Code: firstArea.Iso3166Code);

        // Assert
        Assert.Equal(firstArea, secondArea);
    }

    [Fact]
    public void Equals_WithDifferentMusicBrainzAreaId_ShouldReturnFalse()
    {
        // Arrange
        MusicArea firstArea = _musicAreaFixture.Create(musicBrainzAreaId: _musicBrainzIdFixture.Create(value: Guid.Parse("11111111-1111-1111-1111-111111111111")));

        // Act
        MusicArea secondArea = _musicAreaFixture.Create(musicBrainzAreaId: _musicBrainzIdFixture.Create(value: Guid.Parse("22222222-2222-2222-2222-222222222222")));

        // Assert
        Assert.NotEqual(firstArea, secondArea);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        MusicArea firstArea = _musicAreaFixture.Create(name: "United Kingdom");

        // Act
        MusicArea secondArea = _musicAreaFixture.Create(name: "France");

        // Assert
        Assert.NotEqual(firstArea, secondArea);
    }

    [Fact]
    public void Equals_WithDifferentSortName_ShouldReturnFalse()
    {
        // Arrange
        MusicArea firstArea = _musicAreaFixture.Create(sortName: Optional<string>.Some("UK"));

        // Act
        MusicArea secondArea = _musicAreaFixture.Create(sortName: Optional<string>.Some("GB"));

        // Assert
        Assert.NotEqual(firstArea, secondArea);
    }

    [Fact]
    public void Equals_WithDifferentDisambiguation_ShouldReturnFalse()
    {
        // Arrange
        MusicArea firstArea = _musicAreaFixture.Create(disambiguation: Optional<string>.Some("The country"));

        // Act
        MusicArea secondArea = _musicAreaFixture.Create(disambiguation: Optional<string>.Some("The region"));

        // Assert
        Assert.NotEqual(firstArea, secondArea);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        MusicArea firstArea = _musicAreaFixture.Create(type: Optional<string>.Some("Country"));

        // Act
        MusicArea secondArea = _musicAreaFixture.Create(type: Optional<string>.Some("City"));

        // Assert
        Assert.NotEqual(firstArea, secondArea);
    }

    [Fact]
    public void Equals_WithDifferentIso3166Code_ShouldReturnFalse()
    {
        // Arrange
        MusicArea firstArea = _musicAreaFixture.Create(iso3166Code: Optional<string>.Some("GB"));

        // Act
        MusicArea secondArea = _musicAreaFixture.Create(iso3166Code: Optional<string>.Some("FR"));

        // Assert
        Assert.NotEqual(firstArea, secondArea);
    }
}
