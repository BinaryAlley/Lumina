#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="MusicBrainzReleaseGroupResponse"/> record.
/// The releases of the release group default to an empty list, because they are only returned by a dedicated browse request.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzReleaseGroupResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzArtistCreditResponseFixture _musicBrainzArtistCreditResponseFixture = new();
    private readonly MusicBrainzTagResponseFixture _musicBrainzTagResponseFixture = new();
    private readonly MusicBrainzRatingResponseFixture _musicBrainzRatingResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzReleaseGroupResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="title">Optional. The title of the release group.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the release group.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="primaryType">Optional. The primary type of the release group.</param>
    /// <param name="includePrimaryType">Whether the primary type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="secondaryTypes">Optional. The secondary types of the release group.</param>
    /// <param name="firstReleaseDate">Optional. The date the release group was first released.</param>
    /// <param name="includeFirstReleaseDate">Whether the first release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="artistCredit">Optional. The artist credit of the release group.</param>
    /// <param name="releases">Optional. The releases of the release group.</param>
    /// <param name="tags">Optional. The tags of the release group.</param>
    /// <param name="genres">Optional. The genres of the release group.</param>
    /// <param name="rating">Optional. The aggregated rating of the release group.</param>
    /// <param name="includeRating">Whether the rating should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzReleaseGroupResponse"/> instance.</returns>
    public MusicBrainzReleaseGroupResponse Create(
        string? id = null,
        bool includeId = true,
        string? title = null,
        bool includeTitle = true,
        string? disambiguation = null,
        bool includeDisambiguation = true,
        string? primaryType = null,
        bool includePrimaryType = true,
        List<string>? secondaryTypes = null,
        string? firstReleaseDate = null,
        bool includeFirstReleaseDate = true,
        List<MusicBrainzArtistCreditResponse>? artistCredit = null,
        List<MusicBrainzReleaseResponse>? releases = null,
        List<MusicBrainzTagResponse>? tags = null,
        List<MusicBrainzTagResponse>? genres = null,
        MusicBrainzRatingResponse? rating = null,
        bool includeRating = false)
    {
        return new MusicBrainzReleaseGroupResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Title = includeTitle ? title ?? _faker.Commerce.ProductName() : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            PrimaryType = includePrimaryType ? primaryType ?? _faker.PickRandom("Album", "Single", "EP") : null,
            SecondaryTypes = secondaryTypes ?? [],
            FirstReleaseDate = includeFirstReleaseDate ? firstReleaseDate ?? _faker.Date.Past(40).ToString("yyyy-MM-dd") : null,
            ArtistCredit = artistCredit ?? [.. _musicBrainzArtistCreditResponseFixture.CreateMany(2)],
            Releases = releases ?? [],
            Tags = tags ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Genres = genres ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Rating = includeRating ? rating ?? _musicBrainzRatingResponseFixture.Create() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzReleaseGroupResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzReleaseGroupResponse"/> instances.</returns>
    public List<MusicBrainzReleaseGroupResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
