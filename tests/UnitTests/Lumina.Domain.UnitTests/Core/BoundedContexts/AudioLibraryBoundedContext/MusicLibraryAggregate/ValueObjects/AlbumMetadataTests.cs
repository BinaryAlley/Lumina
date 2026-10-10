#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="AlbumMetadata"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumMetadataTests
{
    private readonly AlbumMetadataFixture _albumMetadataFixture = new();
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateMetadataWithAllPropertiesSet()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();
        Genre genre = _genreFixture.Create(name: "Rock");
        Tag tag = _tagFixture.Create(name: "classic");
        LanguageInfo language = _languageInfoFixture.Create(languageCode: "en", languageName: "English");
        LanguageInfo originalLanguage = _languageInfoFixture.Create(languageCode: "fr", languageName: "French");

        // Act
        Result<AlbumMetadata> result = AlbumMetadata.Create(
            "A Night at the Opera",
            Optional<string>.Some("A Night at the Opera (original)"),
            Optional<string>.Some("A Night at the Opera (2011 remaster)"),
            Optional<string>.Some("The fourth studio album by the British rock band Queen."),
            releaseInfo,
            [genre],
            [tag],
            Optional<LanguageInfo>.Some(language),
            Optional<LanguageInfo>.Some(originalLanguage),
            [MusicReleaseType.Album],
            Optional<MusicReleaseStatus>.Some(MusicReleaseStatus.Official),
            Optional<int>.Some(2),
            12);

        // Assert
        Assert.False(result.IsFailure);
        AlbumMetadata metadata = result.Value;
        Assert.Equal("A Night at the Opera", metadata.Title);
        Assert.Equal(Optional<string>.Some("A Night at the Opera (original)"), metadata.OriginalTitle);
        Assert.Equal(Optional<string>.Some("A Night at the Opera (2011 remaster)"), metadata.ReleaseTitle);
        Assert.Equal(Optional<string>.Some("The fourth studio album by the British rock band Queen."), metadata.Description);
        Assert.Equal(releaseInfo, metadata.ReleaseInfo);
        Assert.Equal(genre, Assert.Single(metadata.Genres));
        Assert.Equal(tag, Assert.Single(metadata.Tags));
        Assert.Equal(Optional<LanguageInfo>.Some(language), metadata.Language);
        Assert.Equal(Optional<LanguageInfo>.Some(originalLanguage), metadata.OriginalLanguage);
        Assert.Equal(new[] { MusicReleaseType.Album }, metadata.ReleaseTypes);
        Assert.Equal(Optional<MusicReleaseStatus>.Some(MusicReleaseStatus.Official), metadata.ReleaseStatus);
        Assert.Equal(Optional<int>.Some(2), metadata.TotalDiscs);
        Assert.Equal(12, metadata.TotalTracks);
    }

    [Fact]
    public void Create_WhenOptionalValuesAreAbsent_ShouldCreateMetadataWithoutThem()
    {
        // Act
        Result<AlbumMetadata> result = AlbumMetadata.Create(
            "A Night at the Opera",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            _releaseInfoFixture.Create(),
            [],
            [],
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None(),
            [],
            Optional<MusicReleaseStatus>.None(),
            Optional<int>.None(),
            12);

        // Assert
        Assert.False(result.IsFailure);
        AlbumMetadata metadata = result.Value;
        Assert.False(metadata.OriginalTitle.HasValue);
        Assert.False(metadata.ReleaseTitle.HasValue);
        Assert.False(metadata.Description.HasValue);
        Assert.False(metadata.Language.HasValue);
        Assert.False(metadata.OriginalLanguage.HasValue);
        Assert.Empty(metadata.ReleaseTypes);
        Assert.False(metadata.ReleaseStatus.HasValue);
        Assert.False(metadata.TotalDiscs.HasValue);
        Assert.Empty(metadata.Genres);
        Assert.Empty(metadata.Tags);
    }

    [Fact]
    public void Create_WhenCollectionsArePopulated_ShouldPreserveTheGenresAndTags()
    {
        // Arrange
        Genre firstGenre = _genreFixture.Create(name: "Rock");
        Genre secondGenre = _genreFixture.Create(name: "Progressive Rock");
        Tag firstTag = _tagFixture.Create(name: "classic");
        Tag secondTag = _tagFixture.Create(name: "70s");
        List<Genre> expectedGenres = [firstGenre, secondGenre];
        List<Tag> expectedTags = [firstTag, secondTag];

        // Act
        Result<AlbumMetadata> result = AlbumMetadata.Create(
            "A Night at the Opera",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            _releaseInfoFixture.Create(),
            expectedGenres,
            expectedTags,
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None(),
            [],
            Optional<MusicReleaseStatus>.None(),
            Optional<int>.None(),
            12);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedGenres, result.Value.Genres);
        Assert.Equal(expectedTags, result.Value.Tags);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AlbumMetadata firstMetadata = _albumMetadataFixture.Create(
            title: "A Night at the Opera",
            originalTitle: Optional<string>.None(),
            releaseTitle: Optional<string>.Some("A Night at the Opera (2011 remaster)"),
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            genres: [_genreFixture.Create(name: "Rock")],
            tags: [_tagFixture.Create(name: "classic")],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            releaseTypes: [MusicReleaseType.Album],
            releaseStatus: Optional<MusicReleaseStatus>.Some(MusicReleaseStatus.Official),
            totalDiscs: Optional<int>.Some(2),
            totalTracks: 12);
        AlbumMetadata secondMetadata = _albumMetadataFixture.Create(
            title: "A Night at the Opera",
            originalTitle: Optional<string>.None(),
            releaseTitle: Optional<string>.Some("A Night at the Opera (2011 remaster)"),
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            genres: [_genreFixture.Create(name: "Rock")],
            tags: [_tagFixture.Create(name: "classic")],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            releaseTypes: [MusicReleaseType.Album],
            releaseStatus: Optional<MusicReleaseStatus>.Some(MusicReleaseStatus.Official),
            totalDiscs: Optional<int>.Some(2),
            totalTracks: 12);

        // Assert
        Assert.Equal(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentTitle_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AlbumMetadata firstMetadata = _albumMetadataFixture.Create(title: "A Night at the Opera", releaseInfo: releaseInfo);
        AlbumMetadata secondMetadata = _albumMetadataFixture.Create(title: "A Day at the Races", releaseInfo: releaseInfo);

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentReleaseType_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AlbumMetadata firstMetadata = _albumMetadataFixture.Create(
            title: "A Night at the Opera",
            releaseInfo: releaseInfo,
            releaseTypes: [MusicReleaseType.Album]);
        AlbumMetadata secondMetadata = _albumMetadataFixture.Create(
            title: "A Night at the Opera",
            releaseInfo: releaseInfo,
            releaseTypes: [MusicReleaseType.Single]);

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentTotalTracks_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AlbumMetadata firstMetadata = _albumMetadataFixture.Create(
            title: "A Night at the Opera",
            releaseInfo: releaseInfo,
            totalTracks: 12);
        AlbumMetadata secondMetadata = _albumMetadataFixture.Create(
            title: "A Night at the Opera",
            releaseInfo: releaseInfo,
            totalTracks: 11);

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }
}
