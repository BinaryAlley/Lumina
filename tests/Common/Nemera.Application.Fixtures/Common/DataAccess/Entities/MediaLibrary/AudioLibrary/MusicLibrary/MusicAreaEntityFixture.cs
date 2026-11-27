#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicAreaEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAreaEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicAreaEntity"/>.
    /// </summary>
    /// <param name="musicBrainzAreaId">Optional. The MusicBrainz identifier of the area.</param>
    /// <param name="name">Optional. The name of the area.</param>
    /// <param name="sortName">Optional. The sort name of the area.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the area.</param>
    /// <param name="type">Optional. The MusicBrainz type of the area.</param>
    /// <param name="iso3166Code">Optional. The ISO 3166 code of the area.</param>
    /// <param name="includeSortName">Whether the sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIso3166Code">Whether the ISO 3166 code should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MusicAreaEntity"/>.</returns>
    public MusicAreaEntity Create(
        Guid? musicBrainzAreaId = null,
        string? name = null,
        string? sortName = null,
        string? disambiguation = null,
        string? type = null,
        string? iso3166Code = null,
        bool includeSortName = true,
        bool includeDisambiguation = true,
        bool includeType = true,
        bool includeIso3166Code = true)
    {
        return new MusicAreaEntity(
            musicBrainzAreaId ?? _faker.Random.Guid(),
            name ?? _faker.Address.Country(),
            includeSortName ? (sortName ?? _faker.Address.Country()) : null,
            includeDisambiguation ? (disambiguation ?? _faker.Lorem.Sentence()) : null,
            includeType ? (type ?? _faker.Random.ArrayElement(["Country", "City", "Area"])) : null,
            includeIso3166Code ? (iso3166Code ?? _faker.Address.CountryCode()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="MusicAreaEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MusicAreaEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
