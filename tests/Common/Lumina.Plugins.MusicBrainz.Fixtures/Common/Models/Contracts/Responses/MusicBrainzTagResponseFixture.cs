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
/// Fixture class for the <see cref="MusicBrainzTagResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzTagResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzTagResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of a genre tag.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="name">Optional. The name of the tag.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of a genre tag.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="count">Optional. The number of votes the tag received.</param>
    /// <returns>A configured <see cref="MusicBrainzTagResponse"/> instance.</returns>
    public MusicBrainzTagResponse Create(
        string? id = null,
        bool includeId = true,
        string? name = null,
        bool includeName = true,
        string? disambiguation = null,
        bool includeDisambiguation = true,
        int? count = null)
    {
        return new MusicBrainzTagResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Name = includeName ? name ?? _faker.Lorem.Word() : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            Count = count ?? _faker.Random.Int(0, 100)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzTagResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzTagResponse"/> instances.</returns>
    public List<MusicBrainzTagResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
