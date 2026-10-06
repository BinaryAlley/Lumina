#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="UpdateAlbumRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumRequestFixture
{
    private readonly Faker _faker = new();
    private readonly MusicAlbumMetadataDtoFixture _musicAlbumMetadataDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to update an album.
    /// </summary>
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
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePackaging">Whether the packaging should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCatalogNumbers">Whether the catalog number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLabel">Whether the label should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to update an album.</returns>
    public UpdateAlbumRequest Create(
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
        bool includeMetadata = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeMediaFormat = true,
        bool includePackaging = true,
        bool includeBarcode = true,
        bool includeCatalogNumbers = true,
        bool includeLabel = true,
        bool includeAsin = true,
        bool includeMusicBrainzReleaseId = true,
        bool includeMusicBrainzReleaseGroupId = true,
        bool includeMusicBrainzReleaseArtistId = true)
    {
        return new UpdateAlbumRequest(
            includeMetadata ? (metadata ?? _musicAlbumMetadataDtoFixture.Create()) : null,
            includeMediaFormat ? (mediaFormat ?? _faker.PickRandom<MusicMediaFormat>()) : null,
            includePackaging ? (packaging ?? _faker.PickRandom<MusicReleasePackaging>()) : null,
            includeBarcode ? (barcode ?? _faker.Random.String2(13, "0123456789")) : null,
            includeCatalogNumbers ? (catalogNumbers ?? [_faker.Random.AlphaNumeric(10)]) : null,
            includeLabel ? (label ?? _faker.Company.CompanyName()) : null,
            includeAsin ? (asin ?? _faker.Random.AlphaNumeric(10)) : null,
            includeMusicBrainzReleaseId ? (musicBrainzReleaseId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseGroupId ? (musicBrainzReleaseGroupId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseArtistId ? (musicBrainzReleaseArtistId ?? _faker.Random.Guid()) : null,
            includeContributors ? (contributors ?? _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeRatings ? (ratings ?? _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateAlbumRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateAlbumRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
