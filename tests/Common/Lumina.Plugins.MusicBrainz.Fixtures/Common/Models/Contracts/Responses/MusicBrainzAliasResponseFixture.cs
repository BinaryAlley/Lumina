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
/// Fixture class for the <see cref="MusicBrainzAliasResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzAliasResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzAliasResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional. The name of the alias.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="sortName">Optional. The sort name of the alias.</param>
    /// <param name="includeSortName">Whether the sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="type">Optional. The type of the alias.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="locale">Optional. The locale the alias is used in.</param>
    /// <param name="includeLocale">Whether the locale should be included, or forced to <see langword="null"/>.</param>
    /// <param name="isPrimary">Optional. Whether this is the primary alias.</param>
    /// <param name="includePrimary">Whether the primary flag should be included, or forced to <see langword="null"/>.</param>
    /// <param name="begin">Optional. The begin date of the alias.</param>
    /// <param name="includeBegin">Whether the begin date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="end">Optional. The end date of the alias.</param>
    /// <param name="includeEnd">Whether the end date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="isEnded">Optional. Whether the alias has ended.</param>
    /// <param name="includeEnded">Whether the ended flag should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzAliasResponse"/> instance.</returns>
    public MusicBrainzAliasResponse Create(
        string? name = null,
        bool includeName = true,
        string? sortName = null,
        bool includeSortName = true,
        string? type = null,
        bool includeType = true,
        string? locale = null,
        bool includeLocale = true,
        bool? isPrimary = null,
        bool includePrimary = true,
        string? begin = null,
        bool includeBegin = true,
        string? end = null,
        bool includeEnd = true,
        bool? isEnded = null,
        bool includeEnded = true)
    {
        return new MusicBrainzAliasResponse
        {
            Name = includeName ? name ?? _faker.Name.FullName() : null,
            SortName = includeSortName ? sortName ?? _faker.Name.FullName() : null,
            Type = includeType ? type ?? _faker.PickRandom("Artist name", "Legal name") : null,
            Locale = includeLocale ? locale ?? _faker.PickRandom("en", "de", "fr") : null,
            IsPrimary = includePrimary ? isPrimary ?? _faker.Random.Bool() : null,
            Begin = includeBegin ? begin ?? _faker.Date.Past(50).ToString("yyyy-MM-dd") : null,
            End = includeEnd ? end ?? _faker.Date.Past(5).ToString("yyyy-MM-dd") : null,
            IsEnded = includeEnded ? isEnded ?? _faker.Random.Bool() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzAliasResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzAliasResponse"/> instances.</returns>
    public List<MusicBrainzAliasResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
