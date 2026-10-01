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
/// Fixture class for the <see cref="MusicBrainzRecordingResponse"/> record.
/// The releases of the recording default to an empty list, because a recording embedded in a release does not carry them.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzRecordingResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzArtistCreditResponseFixture _musicBrainzArtistCreditResponseFixture = new();
    private readonly MusicBrainzRelationResponseFixture _musicBrainzRelationResponseFixture = new();
    private readonly MusicBrainzTagResponseFixture _musicBrainzTagResponseFixture = new();
    private readonly MusicBrainzRatingResponseFixture _musicBrainzRatingResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzRecordingResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the recording.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="title">Optional. The title of the recording.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the recording.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="length">Optional. The length of the recording in milliseconds.</param>
    /// <param name="includeLength">Whether the length should be included, or forced to <see langword="null"/>.</param>
    /// <param name="isVideo">Optional. Whether the recording is a video recording.</param>
    /// <param name="includeVideo">Whether the video flag should be included, or forced to <see langword="null"/>.</param>
    /// <param name="firstReleaseDate">Optional. The date the recording was first released.</param>
    /// <param name="includeFirstReleaseDate">Whether the first release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="artistCredit">Optional. The artist credit of the recording.</param>
    /// <param name="isrcs">Optional. The ISRCs of the recording.</param>
    /// <param name="releases">Optional. The releases the recording appears on.</param>
    /// <param name="tags">Optional. The tags of the recording.</param>
    /// <param name="genres">Optional. The genres of the recording.</param>
    /// <param name="relations">Optional. The relationships of the recording to other entities.</param>
    /// <param name="rating">Optional. The aggregated rating of the recording.</param>
    /// <param name="includeRating">Whether the rating should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzRecordingResponse"/> instance.</returns>
    public MusicBrainzRecordingResponse Create(
        string? id = null,
        bool includeId = true,
        string? title = null,
        bool includeTitle = true,
        string? disambiguation = null,
        bool includeDisambiguation = true,
        long? length = null,
        bool includeLength = true,
        bool? isVideo = null,
        bool includeVideo = true,
        string? firstReleaseDate = null,
        bool includeFirstReleaseDate = true,
        List<MusicBrainzArtistCreditResponse>? artistCredit = null,
        List<string>? isrcs = null,
        List<MusicBrainzReleaseResponse>? releases = null,
        List<MusicBrainzTagResponse>? tags = null,
        List<MusicBrainzTagResponse>? genres = null,
        List<MusicBrainzRelationResponse>? relations = null,
        MusicBrainzRatingResponse? rating = null,
        bool includeRating = false)
    {
        return new MusicBrainzRecordingResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Title = includeTitle ? title ?? _faker.Lorem.Sentence(3) : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            Length = includeLength ? length ?? _faker.Random.Long(30_000, 600_000) : null,
            IsVideo = includeVideo ? isVideo ?? _faker.Random.Bool() : null,
            FirstReleaseDate = includeFirstReleaseDate ? firstReleaseDate ?? _faker.Date.Past(40).ToString("yyyy-MM-dd") : null,
            ArtistCredit = artistCredit ?? [.. _musicBrainzArtistCreditResponseFixture.CreateMany(2)],
            Isrcs = isrcs ?? [$"US{_faker.Random.AlphaNumeric(10).ToUpperInvariant()}"],
            Releases = releases ?? [],
            Tags = tags ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Genres = genres ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Relations = relations ?? [.. _musicBrainzRelationResponseFixture.CreateMany(2)],
            Rating = includeRating ? rating ?? _musicBrainzRatingResponseFixture.Create() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzRecordingResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzRecordingResponse"/> instances.</returns>
    public List<MusicBrainzRecordingResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
