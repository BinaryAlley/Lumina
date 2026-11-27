#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="AlbumMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumMetadataDtoMappingTests
{
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();

    [Fact]
    public void ApplyTo_WhenMappingCompleteDto_ShouldApplyAllPropertiesCorrectly()
    {
        // Arrange
        (Artist artist, Album album) = CreateDomainArtist();
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create();
        List<MusicMediaContributor> contributors = _musicMediaContributorFixture.CreateMany(2);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, contributors);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.Title, album.Metadata.Title);
        Assert.True(album.Metadata.OriginalTitle.HasValue);
        Assert.Equal(dto.OriginalTitle, album.Metadata.OriginalTitle.Value);
        Assert.True(album.Metadata.Description.HasValue);
        Assert.Equal(dto.Description, album.Metadata.Description.Value);
        Assert.Equal(dto.TotalTracks, album.Metadata.TotalTracks);
        Assert.Equal(dto.ReleaseTypes!.Count, album.Metadata.ReleaseTypes.Count);
        Assert.True(album.Metadata.ReleaseStatus.HasValue);
        Assert.Equal(dto.ReleaseStatus, album.Metadata.ReleaseStatus.Value);
        Assert.True(album.Disambiguation.HasValue);
        Assert.Equal(dto.Disambiguation, album.Disambiguation.Value);
        Assert.True(album.Script.HasValue);
        Assert.Equal(dto.Script, album.Script.Value);
        Assert.True(album.Barcode.HasValue);
        Assert.Equal(dto.Barcode, album.Barcode.Value.Value);
        Assert.NotEmpty(album.CatalogNumbers);
        Assert.Equal(dto.CatalogNumbers, album.CatalogNumbers);
        Assert.True(album.Label.HasValue);
        Assert.Equal(dto.Label, album.Label.Value);
        Assert.True(album.ASIN.HasValue);
        Assert.Equal(dto.ASIN, album.ASIN.Value);
        Assert.True(album.MusicBrainzReleaseId.HasValue);
        Assert.Equal(dto.MusicBrainzReleaseId, album.MusicBrainzReleaseId.Value.Value);
        Assert.True(album.MusicBrainzReleaseGroupId.HasValue);
        Assert.Equal(dto.MusicBrainzReleaseGroupId, album.MusicBrainzReleaseGroupId.Value.Value);
        Assert.True(album.MusicBrainzReleaseArtistId.HasValue);
        Assert.Equal(dto.MusicBrainzReleaseArtistId, album.MusicBrainzReleaseArtistId.Value.Value);
        Assert.Equal(contributors.Count, album.Contributors.Count);
        Assert.Equal(dto.Ratings!.Count, album.Ratings.Count);
    }

    [Fact]
    public void ApplyTo_WhenBarcodeIsMissing_ShouldMapTheBarcodeToNone()
    {
        // Arrange
        (Artist artist, Album album) = CreateDomainArtist();
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(includeBarcode: false);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(album.Barcode.HasValue);
    }

    [Fact]
    public void ApplyTo_WhenTheMetadataHasAReleaseTitle_ShouldApplyIt()
    {
        // Arrange
        (Artist artist, Album album) = CreateDomainArtist();
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(releaseTitle: "Greatest Hits I");

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(album.Metadata.ReleaseTitle.HasValue);
        Assert.Equal("Greatest Hits I", album.Metadata.ReleaseTitle.Value);
    }

    [Fact]
    public void ApplyTo_WhenTheMetadataHasNoReleaseDate_ShouldKeepTheStoredReleaseDate()
    {
        // Arrange
        (Artist artist, Album album) = CreateDomainArtist();
        DateOnly storedReleaseDate = album.Metadata.ReleaseInfo.OriginalReleaseDate.Value;
        int storedReleaseYear = album.Metadata.ReleaseInfo.OriginalReleaseYear.Value;
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(includeReleaseInfo: false);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(album.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue);
        Assert.Equal(storedReleaseDate, album.Metadata.ReleaseInfo.OriginalReleaseDate.Value);
        Assert.True(album.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue);
        Assert.Equal(storedReleaseYear, album.Metadata.ReleaseInfo.OriginalReleaseYear.Value);
    }

    [Fact]
    public void ApplyTo_WhenReleaseInfoIsInvalid_ShouldReturnError()
    {
        // Arrange
        (Artist artist, Album album) = CreateDomainArtist();
        ReleaseInfoDto releaseInfo = _releaseInfoDtoFixture.Create(
            originalReleaseDate: new DateOnly(2000, 1, 1),
            originalReleaseYear: 1999);
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(releaseInfo: releaseInfo);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void ApplyTo_WhenGenreIsInvalid_ShouldReturnError()
    {
        // Arrange
        (Artist artist, Album album) = CreateDomainArtist();
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]);

        // Act
        Result<Updated> result = dto.ApplyTo(artist, album, []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ApplyTo_WhenAlbumDoesNotBelongToArtist_ShouldReturnAlbumNotFound()
    {
        // Arrange
        (Artist artist, Album _) = CreateDomainArtist();
        (Artist _, Album foreignAlbum) = CreateDomainArtist();
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create();

        // Act
        Result<Updated> result = dto.ApplyTo(artist, foreignAlbum, []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
    }

    /// <summary>
    /// Creates a domain artist that owns a single album, which in turn owns a single track.
    /// </summary>
    /// <returns>The created domain artist, together with its album.</returns>
    private (Artist artist, Album album) CreateDomainArtist()
    {
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity albumEntity = _albumEntityFixture.Create(
            id: albumId,
            libraryId: libraryId,
            tracks: [_trackEntityFixture.Create(albumId: albumId, libraryId: libraryId)]);
        ArtistEntity artistEntity = _artistEntityFixture.Create(libraryId: libraryId, albums: [albumEntity]);
        Artist artist = artistEntity.ToDomainEntity().Value;
        return (artist, artist.Albums.First());
    }
}
