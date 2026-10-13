#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="UpdateMusicAlbumRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateMusicAlbumRequestFixture
{
    private readonly Faker _faker = new();
    private readonly MusicAlbumMetadataDtoFixture _musicAlbumMetadataDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="UpdateMusicAlbumRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="metadata">Optional. The album metadata of the album.</param>
    /// <param name="mediaFormat">Optional. The physical or digital medium of the album.</param>
    /// <param name="packaging">Optional. The outermost physical packaging of the album.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="catalogNumbers">Optional. The catalog numbers of the album.</param>
    /// <param name="label">Optional. The name of the label that issued the album.</param>
    /// <param name="asin">Optional. The ASIN of the album.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz identifier of the release artist.</param>
    /// <param name="contributors">Optional. The references to the media contributors that performed on the album.</param>
    /// <param name="ratings">Optional. The ratings of the album.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePackaging">Whether the packaging should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCatalogNumbers">Whether the catalog numbers should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLabel">Whether the label should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="UpdateMusicAlbumRequest"/> instance.</returns>
    public UpdateMusicAlbumRequest Create(
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
        bool includeRatings = true)
    {
        return new UpdateMusicAlbumRequest(
            Metadata: includeMetadata ? (metadata ?? _musicAlbumMetadataDtoFixture.Create()) : null,
            MediaFormat: includeMediaFormat ? (mediaFormat ?? _faker.PickRandom<MusicMediaFormat>()) : null,
            Packaging: includePackaging ? (packaging ?? _faker.PickRandom<MusicReleasePackaging>()) : null,
            Barcode: includeBarcode ? (barcode ?? _faker.Random.String2(13, "0123456789")) : null,
            CatalogNumbers: includeCatalogNumbers ? (catalogNumbers ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 3)).Select(_ => _faker.Random.String2(10))]) : null,
            Label: includeLabel ? (label ?? _faker.Company.CompanyName()) : null,
            ASIN: includeAsin ? (asin ?? _faker.Random.String2(10)) : null,
            MusicBrainzReleaseId: includeMusicBrainzReleaseId ? (musicBrainzReleaseId ?? _faker.Random.Guid()) : null,
            MusicBrainzReleaseGroupId: includeMusicBrainzReleaseGroupId ? (musicBrainzReleaseGroupId ?? _faker.Random.Guid()) : null,
            MusicBrainzReleaseArtistId: includeMusicBrainzReleaseArtistId ? (musicBrainzReleaseArtistId ?? _faker.Random.Guid()) : null,
            Contributors: includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Ratings: includeRatings ? (ratings ?? [.. _audioRatingDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="UpdateMusicAlbumRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateMusicAlbumRequest"/> instances.</returns>
    public List<UpdateMusicAlbumRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
