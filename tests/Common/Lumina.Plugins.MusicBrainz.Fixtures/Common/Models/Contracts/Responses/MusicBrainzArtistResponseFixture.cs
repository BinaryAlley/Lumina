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
/// Fixture class for the <see cref="MusicBrainzArtistResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzArtistResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzAreaResponseFixture _musicBrainzAreaResponseFixture = new();
    private readonly MusicBrainzAliasResponseFixture _musicBrainzAliasResponseFixture = new();
    private readonly MusicBrainzRelationResponseFixture _musicBrainzRelationResponseFixture = new();
    private readonly MusicBrainzTagResponseFixture _musicBrainzTagResponseFixture = new();
    private readonly MusicBrainzLifeSpanResponseFixture _musicBrainzLifeSpanResponseFixture = new();
    private readonly MusicBrainzRatingResponseFixture _musicBrainzRatingResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzArtistResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="sortName">Optional. The sort name of the artist.</param>
    /// <param name="includeSortName">Whether the sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the artist.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="type">Optional. The MusicBrainz type of the artist.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="gender">Optional. The gender of the artist.</param>
    /// <param name="includeGender">Whether the gender should be included, or forced to <see langword="null"/>.</param>
    /// <param name="country">Optional. The ISO 3166-1 alpha-2 code of the country of the artist.</param>
    /// <param name="includeCountry">Whether the country should be included, or forced to <see langword="null"/>.</param>
    /// <param name="area">Optional. The area the artist is primarily identified with.</param>
    /// <param name="includeArea">Whether the area should be included, or forced to <see langword="null"/>.</param>
    /// <param name="beginArea">Optional. The area the artist began in.</param>
    /// <param name="includeBeginArea">Whether the begin area should be included, or forced to <see langword="null"/>.</param>
    /// <param name="endArea">Optional. The area the artist ended in.</param>
    /// <param name="includeEndArea">Whether the end area should be included, or forced to <see langword="null"/>.</param>
    /// <param name="lifeSpan">Optional. The life span of the artist.</param>
    /// <param name="includeLifeSpan">Whether the life span should be included, or forced to <see langword="null"/>.</param>
    /// <param name="isnis">Optional. The ISNI codes of the artist.</param>
    /// <param name="ipis">Optional. The IPI codes of the artist.</param>
    /// <param name="aliases">Optional. The alternative names of the artist.</param>
    /// <param name="tags">Optional. The tags of the artist.</param>
    /// <param name="genres">Optional. The genres of the artist.</param>
    /// <param name="relations">Optional. The relationships of the artist to other entities.</param>
    /// <param name="rating">Optional. The aggregated rating of the artist.</param>
    /// <param name="includeRating">Whether the rating should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzArtistResponse"/> instance.</returns>
    public MusicBrainzArtistResponse Create(
        string? id = null,
        bool includeId = true,
        string? name = null,
        bool includeName = true,
        string? sortName = null,
        bool includeSortName = true,
        string? disambiguation = null,
        bool includeDisambiguation = true,
        string? type = null,
        bool includeType = true,
        string? gender = null,
        bool includeGender = true,
        string? country = null,
        bool includeCountry = true,
        MusicBrainzAreaResponse? area = null,
        bool includeArea = false,
        MusicBrainzAreaResponse? beginArea = null,
        bool includeBeginArea = false,
        MusicBrainzAreaResponse? endArea = null,
        bool includeEndArea = false,
        MusicBrainzLifeSpanResponse? lifeSpan = null,
        bool includeLifeSpan = false,
        List<string>? isnis = null,
        List<string>? ipis = null,
        List<MusicBrainzAliasResponse>? aliases = null,
        List<MusicBrainzTagResponse>? tags = null,
        List<MusicBrainzTagResponse>? genres = null,
        List<MusicBrainzRelationResponse>? relations = null,
        MusicBrainzRatingResponse? rating = null,
        bool includeRating = false)
    {
        return new MusicBrainzArtistResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Name = includeName ? name ?? _faker.Name.FullName() : null,
            SortName = includeSortName ? sortName ?? _faker.Name.FullName() : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            Type = includeType ? type ?? _faker.PickRandom("Person", "Group", "Orchestra") : null,
            Gender = includeGender ? gender ?? _faker.PickRandom("Male", "Female") : null,
            Country = includeCountry ? country ?? _faker.Address.CountryCode() : null,
            Area = includeArea ? area ?? _musicBrainzAreaResponseFixture.Create() : null,
            BeginArea = includeBeginArea ? beginArea ?? _musicBrainzAreaResponseFixture.Create() : null,
            EndArea = includeEndArea ? endArea ?? _musicBrainzAreaResponseFixture.Create() : null,
            LifeSpan = includeLifeSpan ? lifeSpan ?? _musicBrainzLifeSpanResponseFixture.Create() : null,
            Isnis = isnis ?? [_faker.Random.String2(16, "0123456789")],
            Ipis = ipis ?? [_faker.Random.String2(11, "0123456789")],
            Aliases = aliases ?? [.. _musicBrainzAliasResponseFixture.CreateMany(2)],
            Tags = tags ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Genres = genres ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Relations = relations ?? [.. _musicBrainzRelationResponseFixture.CreateMany(2)],
            Rating = includeRating ? rating ?? _musicBrainzRatingResponseFixture.Create() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzArtistResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzArtistResponse"/> instances.</returns>
    public List<MusicBrainzArtistResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
