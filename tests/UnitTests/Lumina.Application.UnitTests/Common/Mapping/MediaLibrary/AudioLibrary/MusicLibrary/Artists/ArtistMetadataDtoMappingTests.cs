#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="ArtistMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistMetadataDtoMappingTests
{
    private readonly ArtistMetadataDtoFixture _artistMetadataDtoFixture = new();
    private readonly MusicAreaDtoFixture _musicAreaDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();

    [Fact]
    public void ApplyTo_WhenMappingCompleteDto_ShouldApplyAllPropertiesCorrectly()
    {
        // Arrange
        Artist artist = CreateDomainArtist();
        ArtistMetadataDto dto = _artistMetadataDtoFixture.Create();
        List<MusicMediaContributor> contributors = _musicMediaContributorFixture.CreateMany(2);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, contributors);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.Name, artist.Name);
        Assert.True(artist.SortName.HasValue);
        Assert.Equal(dto.SortName, artist.SortName.Value);
        Assert.True(artist.Disambiguation.HasValue);
        Assert.Equal(dto.Disambiguation, artist.Disambiguation.Value);
        Assert.True(artist.Country.HasValue);
        Assert.Equal(dto.Country, artist.Country.Value);
        Assert.True(artist.Area.HasValue);
        Assert.Equal(dto.Area!.Name, artist.Area.Value.Name);
        Assert.True(artist.BeginArea.HasValue);
        Assert.True(artist.EndArea.HasValue);
        Assert.True(artist.LifeSpanBegin.HasValue);
        Assert.Equal(dto.LifeSpanBegin, artist.LifeSpanBegin.Value);
        Assert.True(artist.LifeSpanEnd.HasValue);
        Assert.Equal(dto.LifeSpanEnd, artist.LifeSpanEnd.Value);
        Assert.Equal(dto.IsEnded, artist.IsEnded);
        Assert.True(artist.Website.HasValue);
        Assert.Equal(dto.Website, artist.Website.Value);
        Assert.True(artist.MusicBrainzArtistId.HasValue);
        Assert.Equal(dto.MusicBrainzArtistId, artist.MusicBrainzArtistId.Value.Value);
        Assert.Equal(dto.Ipis!.Count, artist.Ipis.Count);
        Assert.Equal(dto.Isnis!.Count, artist.Isnis.Count);
        Assert.Equal(dto.Aliases!.Count, artist.Aliases.Count);
        Assert.Equal(dto.Genres!.Count, artist.Genres.Count);
        Assert.Equal(dto.Tags!.Count, artist.Tags.Count);
        Assert.Equal(dto.Ratings!.Count, artist.Ratings.Count);
        Assert.Equal(contributors.Count, artist.Contributors.Count);
    }

    [Fact]
    public void ApplyTo_WhenNameAndWebsiteAreMissing_ShouldPreserveTheStoredValues()
    {
        // Arrange
        Artist artist = CreateDomainArtist();
        string storedName = artist.Name;
        bool hadStoredWebsite = artist.Website.HasValue;
        string? storedWebsite = artist.Website.HasValue ? artist.Website.Value : null;
        ArtistMetadataDto dto = _artistMetadataDtoFixture.Create(includeName: false, includeWebsite: false);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(storedName, artist.Name);
        Assert.Equal(hadStoredWebsite, artist.Website.HasValue);
        if (hadStoredWebsite)
            Assert.Equal(storedWebsite, artist.Website.Value);
    }

    [Fact]
    public void ApplyTo_WhenAreaIsMissingItsIdentifier_ShouldMapTheAreaToNone()
    {
        // Arrange
        Artist artist = CreateDomainArtist();
        ArtistMetadataDto dto = _artistMetadataDtoFixture.Create(area: _musicAreaDtoFixture.Create(includeMusicBrainzAreaId: false));

        // Act
        Result<Updated> result = dto.ApplyTo(artist, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(artist.Area.HasValue);
    }

    [Fact]
    public void ApplyTo_WhenGenreIsInvalid_ShouldReturnError()
    {
        // Arrange
        Artist artist = CreateDomainArtist();
        ArtistMetadataDto dto = _artistMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    /// <summary>
    /// Creates a domain artist that owns a single album, which in turn owns a single track.
    /// </summary>
    /// <returns>The created domain artist.</returns>
    private Artist CreateDomainArtist()
    {
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity albumEntity = _albumEntityFixture.Create(
            id: albumId,
            libraryId: libraryId,
            tracks: [_trackEntityFixture.Create(albumId: albumId, libraryId: libraryId)]);
        ArtistEntity artistEntity = _artistEntityFixture.Create(libraryId: libraryId, albums: [albumEntity]);
        return artistEntity.ToDomainEntity().Value;
    }
}
