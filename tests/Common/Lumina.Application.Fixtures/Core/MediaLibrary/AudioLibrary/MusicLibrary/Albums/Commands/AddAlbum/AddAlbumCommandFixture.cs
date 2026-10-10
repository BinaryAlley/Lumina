#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;

/// <summary>
/// Fixture class for the <see cref="AddAlbumCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddAlbumCommandFixture
{
    private readonly Faker _faker = new();
    private readonly MusicAlbumMetadataDtoFixture _musicAlbumMetadataDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();

    /// <summary>
    /// Creates a random valid command to add an album.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the album belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to, taken from the route.</param>
    /// <param name="metadata">Optional. The album metadata of the album.</param>
    /// <param name="mediaFormat">Optional. The physical or digital medium of the album.</param>
    /// <param name="packaging">Optional. The outermost physical packaging of the album.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="catalogNumber">Optional. The catalog number of the album.</param>
    /// <param name="label">Optional. The name of the label that issued the album.</param>
    /// <param name="asin">Optional. The ASIN of the album.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz identifier of the release artist.</param>
    /// <param name="contributors">Optional. The media contributors that performed on the album.</param>
    /// <param name="ratings">Optional. The ratings of the album.</param>
    /// <param name="tracks">Optional. The tracks of the album.</param>
    /// <param name="albumId">Optional. The Id of the album, when it already exists.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePackaging">Whether the packaging should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCatalogNumbers">Whether the catalog number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLabel">Whether the label should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTracks">Whether the tracks should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created command to add an album.</returns>
    public AddAlbumCommand Create(
        string? libraryId = null,
        string? artistId = null,
        MusicAlbumMetadataDto? metadata = null,
        MusicMediaFormat? mediaFormat = null,
        MusicReleasePackaging? packaging = null,
        string? barcode = null,
        List<string>? catalogNumbers = null,
        string? label = null,
        string? asin = null,
        Guid? musicBrainzReleaseId = null,
        Guid? musicBrainzReleaseGroupId = null,
        Guid? musicBrainzReleaseArtistId = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        List<AddTrackCommand>? tracks = null,
        Guid? albumId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeMetadata = true,
        bool includeMediaFormat = true,
        bool includePackaging = true,
        bool includeBarcode = true,
        bool includeCatalogNumbers = true,
        bool includeLabel = true,
        bool includeAsin = true,
        bool includeMusicBrainzReleaseId = true,
        bool includeMusicBrainzReleaseGroupId = true,
        bool includeMusicBrainzReleaseArtistId = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeTracks = true,
        bool includeAlbumId = true)
    {
        string? resolvedLibraryId = includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null;
        string? resolvedArtistId = includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null;

        return new AddAlbumCommand(
            resolvedLibraryId,
            resolvedArtistId,
            includeMetadata ? (metadata ?? _musicAlbumMetadataDtoFixture.Create()) : null,
            includeMediaFormat ? (mediaFormat ?? _faker.PickRandom<MusicMediaFormat>()) : null,
            includePackaging ? (packaging ?? _faker.PickRandom<MusicReleasePackaging>()) : null,
            includeBarcode ? (barcode ?? _faker.Random.String2(_faker.Random.Int(12, 13), "0123456789")) : null,
            includeCatalogNumbers ? (catalogNumbers ?? [_faker.Random.AlphaNumeric(_faker.Random.Int(1, 50))]) : null,
            includeLabel ? (label ?? _faker.Company.CompanyName()) : null,
            includeAsin ? (asin ?? _faker.Random.AlphaNumeric(10)) : null,
            includeMusicBrainzReleaseId ? (musicBrainzReleaseId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseGroupId ? (musicBrainzReleaseGroupId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseArtistId ? (musicBrainzReleaseArtistId ?? _faker.Random.Guid()) : null,
            includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeRatings ? (ratings ?? [.. _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeTracks ? (tracks ?? GenerateTracks(resolvedLibraryId, resolvedArtistId)) : null,
            includeAlbumId ? (albumId ?? _faker.Random.Guid()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddAlbumCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddAlbumCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Generates the tracks of the album, tied to the album's library and artist identifiers.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the tracks belong to.</param>
    /// <param name="artistId">The Id of the artist the tracks belong to.</param>
    /// <returns>The generated tracks.</returns>
    private List<AddTrackCommand> GenerateTracks(string? libraryId, string? artistId)
    {
        return [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(_ => _addTrackCommandFixture.Create(libraryId: libraryId, artistId: artistId))];
    }
}
