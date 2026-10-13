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
/// Fixture class for the <see cref="MusicArtistAliasDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistAliasDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicArtistAliasDto"/> instance with randomized test data.
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
    /// <returns>A configured <see cref="MusicArtistAliasDto"/> instance.</returns>
    public MusicArtistAliasDto Create(
        string? name = null,
        string? sortName = null,
        string? type = null,
        string? locale = null,
        bool isPrimary = false,
        DateOnly? beginDate = null,
        DateOnly? endDate = null,
        bool isEnded = false,
        bool includeName = true,
        bool includeSortName = true,
        bool includeType = true,
        bool includeLocale = true,
        bool includeBeginDate = true,
        bool includeEndDate = true)
    {
        DateOnly resolvedBeginDate = beginDate ?? DateOnly.FromDateTime(_faker.Date.Past());
        DateOnly resolvedEndDate = endDate ?? DateOnly.FromDateTime(_faker.Date.Future());

        return new MusicArtistAliasDto
        {
            Name = includeName ? (name ?? _faker.Name.FullName()) : null,
            SortName = includeSortName ? (sortName ?? _faker.Name.LastName()) : null,
            Type = includeType ? (type ?? _faker.Lorem.Word()) : null,
            Locale = includeLocale ? (locale ?? _faker.Random.String2(2, "abcdefghijklmnopqrstuvwxyz")) : null,
            IsPrimary = isPrimary,
            BeginDate = includeBeginDate ? resolvedBeginDate : null,
            EndDate = includeEndDate ? resolvedEndDate : null,
            IsEnded = isEnded
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicArtistAliasDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicArtistAliasDto"/> instances.</returns>
    public List<MusicArtistAliasDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
