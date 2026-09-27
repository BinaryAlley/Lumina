#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="UpdateArtistAlbumRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistAlbumRequestFixture
{
    private readonly Faker _faker = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly UpdateArtistTrackRequestFixture _updateArtistTrackRequestFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to update an album of an artist.
    /// </summary>
    /// <param name="albumId">Optional. The Id of the album, when the album already exists.</param>
    /// <param name="metadata">Optional. The album metadata of the album.</param>
    /// <param name="mediaFormat">Optional. The physical or digital medium of the album.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="catalogNumber">Optional. The catalog number of the album.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz identifier of the release artist.</param>
    /// <param name="contributors">Optional. The media contributors that performed on the album.</param>
    /// <param name="ratings">Optional. The ratings of the album.</param>
    /// <param name="tracks">Optional. The tracks of the album.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCatalogNumber">Whether the catalog number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTracks">Whether the tracks list should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to update an album of an artist.</returns>
    public UpdateArtistAlbumRequest Create(
        Guid? albumId = null,
        AlbumMetadataDto? metadata = null,
        MusicMediaFormat? mediaFormat = null,
        string? barcode = null,
        string? catalogNumber = null,
        Guid? musicBrainzReleaseId = null,
        Guid? musicBrainzReleaseGroupId = null,
        Guid? musicBrainzReleaseArtistId = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        List<UpdateArtistTrackRequest>? tracks = null,
        bool includeAlbumId = true,
        bool includeMetadata = true,
        bool includeMediaFormat = true,
        bool includeBarcode = true,
        bool includeCatalogNumber = true,
        bool includeMusicBrainzReleaseId = true,
        bool includeMusicBrainzReleaseGroupId = true,
        bool includeMusicBrainzReleaseArtistId = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeTracks = true)
    {
        return new UpdateArtistAlbumRequest(
            includeAlbumId ? (albumId ?? _faker.Random.Guid()) : null,
            includeMetadata ? (metadata ?? _albumMetadataDtoFixture.Create()) : null,
            includeMediaFormat ? (mediaFormat ?? _faker.PickRandom<MusicMediaFormat>()) : null,
            includeBarcode ? (barcode ?? _faker.Random.String2(13, "0123456789")) : null,
            includeCatalogNumber ? (catalogNumber ?? _faker.Random.String2(_faker.Random.Number(1, 50))) : null,
            includeMusicBrainzReleaseId ? (musicBrainzReleaseId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseGroupId ? (musicBrainzReleaseGroupId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseArtistId ? (musicBrainzReleaseArtistId ?? _faker.Random.Guid()) : null,
            includeContributors ? (contributors ?? _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeRatings ? (ratings ?? _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeTracks ? (tracks ?? _updateArtistTrackRequestFixture.CreateMany(_faker.Random.Int(1, 3))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateArtistAlbumRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateArtistAlbumRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
