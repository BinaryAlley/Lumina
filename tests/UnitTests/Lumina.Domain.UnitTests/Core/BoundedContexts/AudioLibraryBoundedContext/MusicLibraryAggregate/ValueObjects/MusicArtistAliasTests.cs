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
/// Contains unit tests for the <see cref="MusicArtistAlias"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistAliasTests
{
    private readonly MusicArtistAliasFixture _musicArtistAliasFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateAliasWithAllPropertiesSet()
    {
        // Arrange
        DateOnly beginDate = new(1970, 1, 1);
        DateOnly endDate = new(1991, 1, 1);

        // Act
        Result<MusicArtistAlias> result = MusicArtistAlias.Create(
            "Freddie Mercury",
            Optional<string>.Some("Mercury, Freddie"),
            Optional<string>.Some("Artist name"),
            Optional<string>.Some("en"),
            isPrimary: true,
            Optional<DateOnly>.Some(beginDate),
            Optional<DateOnly>.Some(endDate),
            isEnded: true);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Freddie Mercury", result.Value.Name);
        Assert.Equal(Optional<string>.Some("Mercury, Freddie"), result.Value.SortName);
        Assert.Equal(Optional<string>.Some("Artist name"), result.Value.Type);
        Assert.Equal(Optional<string>.Some("en"), result.Value.Locale);
        Assert.True(result.Value.IsPrimary);
        Assert.Equal(Optional<DateOnly>.Some(beginDate), result.Value.BeginDate);
        Assert.Equal(Optional<DateOnly>.Some(endDate), result.Value.EndDate);
        Assert.True(result.Value.IsEnded);
    }

    [Fact]
    public void Create_WhenNameHasSurroundingWhitespace_ShouldTrimIt()
    {
        // Act
        Result<MusicArtistAlias> result = MusicArtistAlias.Create(
            "  Freddie Mercury  ",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            isPrimary: false,
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            isEnded: false);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Freddie Mercury", result.Value.Name);
    }

    [Theory]
    [InlineData(null)] // null alias name
    [InlineData("")] // empty alias name
    [InlineData("   ")] // whitespace alias name
    public void Create_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Act
        Result<MusicArtistAlias> result = MusicArtistAlias.Create(
            name!,
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            isPrimary: false,
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            isEnded: false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.ArtistAliasNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WhenOptionalValuesAreAbsent_ShouldCreateAliasWithoutThem()
    {
        // Act
        Result<MusicArtistAlias> result = MusicArtistAlias.Create(
            "Freddie Mercury",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            isPrimary: false,
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            isEnded: false);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.SortName.HasValue);
        Assert.False(result.Value.Type.HasValue);
        Assert.False(result.Value.Locale.HasValue);
        Assert.False(result.Value.BeginDate.HasValue);
        Assert.False(result.Value.EndDate.HasValue);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create();

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(
            name: firstAlias.Name,
            sortName: firstAlias.SortName,
            type: firstAlias.Type,
            locale: firstAlias.Locale,
            isPrimary: firstAlias.IsPrimary,
            beginDate: firstAlias.BeginDate,
            endDate: firstAlias.EndDate,
            isEnded: firstAlias.IsEnded);

        // Assert
        Assert.Equal(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(name: "Freddie Mercury");

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(name: "Farrokh Bulsara");

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentSortName_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(sortName: Optional<string>.Some("Mercury, Freddie"));

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(sortName: Optional<string>.Some("Bulsara, Farrokh"));

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(type: Optional<string>.Some("Artist name"));

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(type: Optional<string>.Some("Search hint"));

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentLocale_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(locale: Optional<string>.Some("en"));

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(locale: Optional<string>.Some("fr"));

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentIsPrimary_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(isPrimary: true);

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(isPrimary: false);

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentBeginDate_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(beginDate: Optional<DateOnly>.Some(new DateOnly(1970, 1, 1)));

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(beginDate: Optional<DateOnly>.Some(new DateOnly(1980, 1, 1)));

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentEndDate_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(endDate: Optional<DateOnly>.Some(new DateOnly(1991, 1, 1)));

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(endDate: Optional<DateOnly>.Some(new DateOnly(2000, 1, 1)));

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }

    [Fact]
    public void Equals_WithDifferentIsEnded_ShouldReturnFalse()
    {
        // Arrange
        MusicArtistAlias firstAlias = _musicArtistAliasFixture.Create(isEnded: true);

        // Act
        MusicArtistAlias secondAlias = _musicArtistAliasFixture.Create(isEnded: false);

        // Assert
        Assert.NotEqual(firstAlias, secondAlias);
    }
}
