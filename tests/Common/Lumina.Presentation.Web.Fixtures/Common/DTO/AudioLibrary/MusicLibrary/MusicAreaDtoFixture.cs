#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicAreaDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAreaDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicAreaDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="musicBrainzAreaId">Optional. The MusicBrainz identifier of the area.</param>
    /// <param name="name">Optional. The name of the area.</param>
    /// <param name="sortName">Optional. The sort name of the area.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the area.</param>
    /// <param name="type">Optional. The MusicBrainz type of the area.</param>
    /// <param name="iso3166Code">Optional. The ISO 3166 code of the area.</param>
    /// <param name="includeMusicBrainzAreaId">Whether the MusicBrainz area Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSortName">Whether the sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIso3166Code">Whether the ISO 3166 code should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicAreaDto"/> instance.</returns>
    public MusicAreaDto Create(
        Guid? musicBrainzAreaId = null,
        string? name = null,
        string? sortName = null,
        string? disambiguation = null,
        string? type = null,
        string? iso3166Code = null,
        bool includeMusicBrainzAreaId = true,
        bool includeName = true,
        bool includeSortName = true,
        bool includeDisambiguation = true,
        bool includeType = true,
        bool includeIso3166Code = true)
    {
        return new MusicAreaDto
        {
            MusicBrainzAreaId = includeMusicBrainzAreaId ? (musicBrainzAreaId ?? _faker.Random.Guid()) : null,
            Name = includeName ? (name ?? _faker.Address.Country()) : null,
            SortName = includeSortName ? (sortName ?? _faker.Address.Country()) : null,
            Disambiguation = includeDisambiguation ? (disambiguation ?? _faker.Lorem.Word()) : null,
            Type = includeType ? (type ?? _faker.Lorem.Word()) : null,
            Iso3166Code = includeIso3166Code ? (iso3166Code ?? _faker.Address.CountryCode()) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicAreaDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicAreaDto"/> instances.</returns>
    public List<MusicAreaDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
