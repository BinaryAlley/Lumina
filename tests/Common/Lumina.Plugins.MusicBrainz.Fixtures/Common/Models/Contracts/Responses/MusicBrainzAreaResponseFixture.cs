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
/// Fixture class for the <see cref="MusicBrainzAreaResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzAreaResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzAreaResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the area.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="name">Optional. The name of the area.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="sortName">Optional. The sort name of the area.</param>
    /// <param name="includeSortName">Whether the sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the area.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="type">Optional. The MusicBrainz type of the area.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="iso3166Part1Codes">Optional. The ISO 3166-1 codes of the area.</param>
    /// <param name="iso3166Part2Codes">Optional. The ISO 3166-2 codes of the area.</param>
    /// <returns>A configured <see cref="MusicBrainzAreaResponse"/> instance.</returns>
    public MusicBrainzAreaResponse Create(
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
        List<string>? iso3166Part1Codes = null,
        List<string>? iso3166Part2Codes = null)
    {
        return new MusicBrainzAreaResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Name = includeName ? name ?? _faker.Address.Country() : null,
            SortName = includeSortName ? sortName ?? _faker.Address.Country() : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            Type = includeType ? type ?? _faker.PickRandom("Country", "City", "Area") : null,
            Iso3166Part1Codes = iso3166Part1Codes ?? [_faker.Address.CountryCode()],
            Iso3166Part2Codes = iso3166Part2Codes ?? [_faker.Random.String2(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzAreaResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzAreaResponse"/> instances.</returns>
    public List<MusicBrainzAreaResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
