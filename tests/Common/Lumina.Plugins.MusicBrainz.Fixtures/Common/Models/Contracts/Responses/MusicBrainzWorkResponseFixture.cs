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
/// Fixture class for the <see cref="MusicBrainzWorkResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzWorkResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzRelationResponseFixture _musicBrainzRelationResponseFixture = new();
    private readonly MusicBrainzTagResponseFixture _musicBrainzTagResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzWorkResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the work.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="title">Optional. The title of the work.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="type">Optional. The MusicBrainz type of the work.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the work.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="languages">Optional. The languages of the work.</param>
    /// <param name="iswcs">Optional. The ISWCs of the work.</param>
    /// <param name="relations">Optional. The relationships of the work to other entities.</param>
    /// <param name="tags">Optional. The tags of the work.</param>
    /// <param name="genres">Optional. The genres of the work.</param>
    /// <returns>A configured <see cref="MusicBrainzWorkResponse"/> instance.</returns>
    public MusicBrainzWorkResponse Create(
        string? id = null,
        bool includeId = true,
        string? title = null,
        bool includeTitle = true,
        string? type = null,
        bool includeType = true,
        string? disambiguation = null,
        bool includeDisambiguation = true,
        List<string>? languages = null,
        List<string>? iswcs = null,
        List<MusicBrainzRelationResponse>? relations = null,
        List<MusicBrainzTagResponse>? tags = null,
        List<MusicBrainzTagResponse>? genres = null)
    {
        return new MusicBrainzWorkResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Title = includeTitle ? title ?? _faker.Lorem.Sentence(3) : null,
            Type = includeType ? type ?? _faker.PickRandom("Song", "Aria", "Symphony") : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            Languages = languages ?? [_faker.PickRandom("eng", "deu", "fra")],
            Iswcs = iswcs ?? [$"T-{_faker.Random.Int(100, 999)}.{_faker.Random.Int(100, 999)}.{_faker.Random.Int(100, 999)}-{_faker.Random.Int(1, 9)}"],
            Relations = relations ?? [.. _musicBrainzRelationResponseFixture.CreateMany(2)],
            Tags = tags ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)],
            Genres = genres ?? [.. _musicBrainzTagResponseFixture.CreateMany(2)]
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzWorkResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzWorkResponse"/> instances.</returns>
    public List<MusicBrainzWorkResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
