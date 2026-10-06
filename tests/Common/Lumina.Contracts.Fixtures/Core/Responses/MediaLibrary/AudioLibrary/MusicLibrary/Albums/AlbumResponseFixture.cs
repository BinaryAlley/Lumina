#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="AlbumResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicAlbumMetadataDtoFixture _musicAlbumMetadataDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly TrackResponseFixture _trackResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the album.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to.</param>
    /// <param name="libraryId">Optional. The Id of the media library the album belongs to.</param>
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
    /// <param name="createdOnUtc">Optional. The date and time when the album was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the album was updated.</param>
    /// <param name="contributors">Optional. The list of contributor references of the album.</param>
    /// <param name="ratings">Optional. The list of ratings of the album.</param>
    /// <param name="tracks">Optional. The list of tracks of the album.</param>
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePackaging">Whether the packaging should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCatalogNumbers">Whether the catalog number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLabel">Whether the label should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTracks">Whether the tracks should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AlbumResponse"/>.</returns>
    public AlbumResponse Create(
        Guid? id = null,
        Guid? artistId = null,
        Guid? libraryId = null,
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
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        List<TrackResponse>? tracks = null,
        bool includeMediaFormat = true,
        bool includePackaging = true,
        bool includeBarcode = true,
        bool includeCatalogNumbers = true,
        bool includeLabel = true,
        bool includeAsin = true,
        bool includeMusicBrainzReleaseId = true,
        bool includeMusicBrainzReleaseGroupId = true,
        bool includeMusicBrainzReleaseArtistId = true,
        bool includeUpdatedOnUtc = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeTracks = true)
    {
        return new AlbumResponse(
            id ?? Guid.NewGuid(),
            artistId ?? Guid.NewGuid(),
            libraryId ?? Guid.NewGuid(),
            metadata ?? _musicAlbumMetadataDtoFixture.Create(),
            includeMediaFormat ? (mediaFormat ?? _faker.PickRandom<MusicMediaFormat>()) : null,
            includePackaging ? (packaging ?? _faker.PickRandom<MusicReleasePackaging>()) : null,
            includeBarcode ? (barcode ?? _faker.Random.String2(13, "0123456789")) : null,
            includeCatalogNumbers ? (catalogNumbers ?? [_faker.Random.AlphaNumeric(10)]) : null,
            includeLabel ? (label ?? _faker.Company.CompanyName()) : null,
            includeAsin ? (asin ?? _faker.Random.AlphaNumeric(10)) : null,
            includeMusicBrainzReleaseId ? (musicBrainzReleaseId ?? Guid.NewGuid()) : null,
            includeMusicBrainzReleaseGroupId ? (musicBrainzReleaseGroupId ?? Guid.NewGuid()) : null,
            includeMusicBrainzReleaseArtistId ? (musicBrainzReleaseArtistId ?? Guid.NewGuid()) : null,
            createdOnUtc ?? _faker.Date.Past().ToUniversalTime(),
            includeUpdatedOnUtc ? (updatedOnUtc ?? _faker.Date.Recent().ToUniversalTime()) : null,
            includeContributors ? (contributors ?? _mediaContributorReferenceDtoFixture.CreateMany(1)) : null,
            includeRatings ? (ratings ?? _audioRatingDtoFixture.CreateMany(1)) : null,
            includeTracks ? (tracks ?? _trackResponseFixture.CreateMany(1)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AlbumResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
