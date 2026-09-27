#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="UpdateAlbumCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCommandMappingTests
{
    private readonly UpdateAlbumCommandFixture _updateAlbumCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId) = CreateDomainArtist(libraryId);
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: albumId.ToString());

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(artist, result.Value);
        Album updatedAlbum = result.Value.Albums.First(album => album.Id.Value == albumId);
        Assert.Equal(command.Metadata!.Title, updatedAlbum.Metadata.Title);
        Assert.Equal(command.MediaFormat, updatedAlbum.MediaFormat.Value);
        Assert.Equal(command.Barcode, updatedAlbum.Barcode.Value.Value);
        Assert.Equal(command.CatalogNumber, updatedAlbum.CatalogNumber.Value);
        Assert.Equal(command.MusicBrainzReleaseId, updatedAlbum.MusicBrainzReleaseId.Value.Value);
        Assert.Equal(command.MusicBrainzReleaseGroupId, updatedAlbum.MusicBrainzReleaseGroupId.Value.Value);
        Assert.Equal(command.MusicBrainzReleaseArtistId, updatedAlbum.MusicBrainzReleaseArtistId.Value.Value);
        Assert.Equal(command.Contributors!.Count, updatedAlbum.Contributors.Count);
        Assert.Equal(command.Ratings!.Count, updatedAlbum.Ratings.Count);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredPropertiesAndNones()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId) = CreateDomainArtist(libraryId);
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            includeMediaFormat: false,
            includeBarcode: false,
            includeCatalogNumber: false,
            includeMusicBrainzReleaseId: false,
            includeMusicBrainzReleaseGroupId: false,
            includeMusicBrainzReleaseArtistId: false,
            contributors: [],
            ratings: []);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Album updatedAlbum = result.Value.Albums.First(album => album.Id.Value == albumId);
        Assert.Equal(command.Metadata!.Title, updatedAlbum.Metadata.Title);
        Assert.False(updatedAlbum.MediaFormat.HasValue);
        Assert.False(updatedAlbum.Barcode.HasValue);
        Assert.False(updatedAlbum.CatalogNumber.HasValue);
        Assert.False(updatedAlbum.MusicBrainzReleaseId.HasValue);
        Assert.False(updatedAlbum.MusicBrainzReleaseGroupId.HasValue);
        Assert.False(updatedAlbum.MusicBrainzReleaseArtistId.HasValue);
        Assert.Empty(updatedAlbum.Contributors);
        Assert.Empty(updatedAlbum.Ratings);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId) = CreateDomainArtist(libraryId);
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenRatingCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId) = CreateDomainArtist(libraryId);
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenBarcodeCannotBeEmpty_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId) = CreateDomainArtist(libraryId);
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            barcode: string.Empty);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.BarcodeValueCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenBarcodeHasInvalidFormat_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId) = CreateDomainArtist(libraryId);
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            barcode: "not-a-barcode");

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.InvalidFormatForBarcode, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumIsNotPartOfTheArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid _) = CreateDomainArtist(libraryId);
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: Guid.NewGuid().ToString());

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
    }

    /// <summary>
    /// Creates a domain artist that owns a single album belonging to <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <returns>The created domain artist, together with the Id of its album.</returns>
    private (Artist artist, Guid albumId) CreateDomainArtist(Guid libraryId)
    {
        Guid albumId = Guid.NewGuid();
        AlbumEntity albumEntity = _albumEntityFixture.Create(id: albumId, libraryId: libraryId, includeTracks: false);
        ArtistEntity artistEntity = _artistEntityFixture.Create(libraryId: libraryId, albums: [albumEntity]);
        Result<Artist> artistResult = artistEntity.ToDomainEntity();
        return (artistResult.Value, albumId);
    }
}
