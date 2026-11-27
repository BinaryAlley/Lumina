#region ========================================================================= USING =====================================================================================================================================================
using Bogus;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="MusicBrainzReleaseResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzReleaseResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzArtistCreditResponseFixture _musicBrainzArtistCreditResponseFixture = new();
    private readonly MusicBrainzReleaseGroupResponseFixture _musicBrainzReleaseGroupResponseFixture = new();
    private readonly MusicBrainzLabelInfoResponseFixture _musicBrainzLabelInfoResponseFixture = new();
    private readonly MusicBrainzMediumResponseFixture _musicBrainzMediumResponseFixture = new();
    private readonly MusicBrainzReleaseEventResponseFixture _musicBrainzReleaseEventResponseFixture = new();
    private readonly MusicBrainzTagResponseFixture _musicBrainzTagResponseFixture = new();
    private readonly MusicBrainzTextRepresentationResponseFixture _musicBrainzTextRepresentationResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzReleaseResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="title">Optional. The title of the release.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the release.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="status">Optional. The status of the release.</param>
    /// <param name="includeStatus">Whether the status should be included, or forced to <see langword="null"/>.</param>
    /// <param name="date">Optional. The date the release was issued.</param>
    /// <param name="includeDate">Whether the date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="country">Optional. The ISO 3166-1 alpha-2 code of the country the release was issued in.</param>
    /// <param name="includeCountry">Whether the country should be included, or forced to <see langword="null"/>.</param>
    /// <param name="releaseEvents">Optional. The release events of the release.</param>
    /// <param name="barcode">Optional. The barcode of the release.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="packaging">Optional. The outermost packaging of the release.</param>
    /// <param name="includePackaging">Whether the packaging should be included, or forced to <see langword="null"/>.</param>
    /// <param name="textRepresentation">Optional. The language and script of the titles of the release.</param>
    /// <param name="includeTextRepresentation">Whether the text representation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="artistCredit">Optional. The artist credit of the release.</param>
    /// <param name="releaseGroup">Optional. The release group the release belongs to.</param>
    /// <param name="includeReleaseGroup">Whether the release group should be included, or forced to <see langword="null"/>.</param>
    /// <param name="labelInfo">Optional. The label information of the release.</param>
    /// <param name="media">Optional. The media of the release.</param>
    /// <param name="asin">Optional. The ASIN of the release.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="tags">Optional. The tags of the release.</param>
    /// <param name="genres">Optional. The genres of the release.</param>
    /// <param name="relations">Optional. The relationships of the release to other entities.</param>
    /// <returns>A configured <see cref="MusicBrainzReleaseResponse"/> instance.</returns>
    public MusicBrainzReleaseResponse Create(
        string? id = null,
        bool includeId = true,
        string? title = null,
        bool includeTitle = true,
        string? disambiguation = null,
        bool includeDisambiguation = true,
        string? status = null,
        bool includeStatus = true,
        string? date = null,
        bool includeDate = true,
        string? country = null,
        bool includeCountry = true,
        List<MusicBrainzReleaseEventResponse>? releaseEvents = null,
        string? barcode = null,
        bool includeBarcode = true,
        string? packaging = null,
        bool includePackaging = true,
        MusicBrainzTextRepresentationResponse? textRepresentation = null,
        bool includeTextRepresentation = true,
        List<MusicBrainzArtistCreditResponse>? artistCredit = null,
        MusicBrainzReleaseGroupResponse? releaseGroup = null,
        bool includeReleaseGroup = true,
        List<MusicBrainzLabelInfoResponse>? labelInfo = null,
        List<MusicBrainzMediumResponse>? media = null,
        string? asin = null,
        bool includeAsin = true,
        List<MusicBrainzTagResponse>? tags = null,
        List<MusicBrainzTagResponse>? genres = null,
        List<MusicBrainzRelationResponse>? relations = null)
    {
        return new MusicBrainzReleaseResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Title = includeTitle ? title ?? _faker.Commerce.ProductName() : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            Status = includeStatus ? status ?? _faker.PickRandom("Official", "Promotion", "Bootleg") : null,
            Date = includeDate ? date ?? _faker.Date.Past(40).ToString("yyyy-MM-dd") : null,
            Country = includeCountry ? country ?? _faker.Address.CountryCode() : null,
            ReleaseEvents = releaseEvents ?? [.. _musicBrainzReleaseEventResponseFixture.CreateMany(1)],
            Barcode = includeBarcode ? barcode ?? _faker.Random.AlphaNumeric(12) : null,
            Packaging = includePackaging ? packaging ?? _faker.PickRandom("Jewel Case", "Digipak") : null,
            TextRepresentation = includeTextRepresentation ? textRepresentation ?? _musicBrainzTextRepresentationResponseFixture.Create() : null,
            ArtistCredit = artistCredit ?? [.. _musicBrainzArtistCreditResponseFixture.CreateMany(2)],
            ReleaseGroup = includeReleaseGroup ? releaseGroup ?? _musicBrainzReleaseGroupResponseFixture.Create() : null,
            LabelInfo = labelInfo ?? [.. _musicBrainzLabelInfoResponseFixture.CreateMany(1)],
            Media = media ?? [.. _musicBrainzMediumResponseFixture.CreateMany(1)],
            Asin = includeAsin ? asin ?? _faker.Random.AlphaNumeric(10) : null,
            Tags = tags ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Genres = genres ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Relations = relations ?? []
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzReleaseResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzReleaseResponse"/> instances.</returns>
    public List<MusicBrainzReleaseResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
