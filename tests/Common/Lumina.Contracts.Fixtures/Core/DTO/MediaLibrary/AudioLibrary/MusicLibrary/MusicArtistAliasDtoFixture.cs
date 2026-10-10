#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicArtistAliasDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistAliasDtoFixture
{
    private const int MINIMUM_ALIAS_YEAR = 1900;
    private const int MAXIMUM_ALIAS_YEAR = 2026;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicArtistAliasDto"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the alias.</param>
    /// <param name="sortName">Optional. The sort name of the alias.</param>
    /// <param name="type">Optional. The type of the alias.</param>
    /// <param name="locale">Optional. The locale the alias is used in.</param>
    /// <param name="isPrimary">Optional. Whether this is the primary alias of the artist.</param>
    /// <param name="beginDate">Optional. The date the alias began being used.</param>
    /// <param name="endDate">Optional. The date the alias stopped being used.</param>
    /// <param name="isEnded">Optional. Whether the alias is no longer used.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSortName">Whether the sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLocale">Whether the locale should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBeginDate">Whether the begin date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeEndDate">Whether the end date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MusicArtistAliasDto"/>.</returns>
    public MusicArtistAliasDto Create(
        string? name = null,
        string? sortName = null,
        string? type = null,
        string? locale = null,
        bool? isPrimary = null,
        DateOnly? beginDate = null,
        DateOnly? endDate = null,
        bool? isEnded = null,
        bool includeName = true,
        bool includeSortName = true,
        bool includeType = true,
        bool includeLocale = true,
        bool includeBeginDate = true,
        bool includeEndDate = true)
    {
        int beginYear = Random.Shared.Next(MINIMUM_ALIAS_YEAR, MAXIMUM_ALIAS_YEAR);
        int endYear = Random.Shared.Next(beginYear, Math.Max(beginYear + 1, MAXIMUM_ALIAS_YEAR));
        return new MusicArtistAliasDto(
            includeName ? (name ?? _faker.Name.FullName()) : null,
            includeSortName ? (sortName ?? _faker.Name.FullName()) : null,
            includeType ? (type ?? _faker.Random.ArrayElement(["Artist name", "Search hint"])) : null,
            includeLocale ? (locale ?? _faker.Random.String2(2)) : null,
            isPrimary ?? _faker.Random.Bool(),
            includeBeginDate ? (beginDate ?? new DateOnly(beginYear, 1, 1)) : null,
            includeEndDate ? (endDate ?? new DateOnly(endYear, 1, 1)) : null,
            isEnded ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="MusicArtistAliasDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MusicArtistAliasDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
