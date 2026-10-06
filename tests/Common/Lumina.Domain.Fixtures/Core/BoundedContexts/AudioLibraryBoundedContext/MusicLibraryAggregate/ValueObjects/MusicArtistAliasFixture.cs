#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MusicArtistAlias"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistAliasFixture
{
    private const int MINIMUM_ALIAS_YEAR = 1900;
    private const int MAXIMUM_ALIAS_YEAR = 2026;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicArtistAlias"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the alias.</param>
    /// <param name="sortName">Optional. The sort name of the alias.</param>
    /// <param name="type">Optional. The type of the alias.</param>
    /// <param name="locale">Optional. The locale the alias is used in.</param>
    /// <param name="isPrimary">Optional. Whether this is the primary alias of the artist.</param>
    /// <param name="beginDate">Optional. The date the alias began being used.</param>
    /// <param name="endDate">Optional. The date the alias stopped being used.</param>
    /// <param name="isEnded">Optional. Whether the alias is no longer used.</param>
    /// <returns>The created <see cref="MusicArtistAlias"/>.</returns>
    public MusicArtistAlias Create(
        string? name = null,
        Optional<string>? sortName = null,
        Optional<string>? type = null,
        Optional<string>? locale = null,
        bool? isPrimary = null,
        Optional<DateOnly>? beginDate = null,
        Optional<DateOnly>? endDate = null,
        bool? isEnded = null)
    {
        int beginYear = Random.Shared.Next(MINIMUM_ALIAS_YEAR, MAXIMUM_ALIAS_YEAR);
        int endYear = Random.Shared.Next(beginYear, Math.Max(beginYear + 1, MAXIMUM_ALIAS_YEAR));
        Result<MusicArtistAlias> aliasResult = MusicArtistAlias.Create(
            name ?? _faker.Name.FullName(),
            sortName ?? Optional<string>.Some(_faker.Name.FullName()),
            type ?? Optional<string>.Some(_faker.Random.ArrayElement(["Artist name", "Search hint"])),
            locale ?? Optional<string>.Some(_faker.Random.String2(2)),
            isPrimary ?? _faker.Random.Bool(),
            beginDate ?? Optional<DateOnly>.Some(new DateOnly(beginYear, 1, 1)),
            endDate ?? Optional<DateOnly>.Some(new DateOnly(endYear, 1, 1)),
            isEnded ?? _faker.Random.Bool());

        if (aliasResult.IsFailure)
            throw new InvalidOperationException("Failed to create MusicArtistAlias: " + string.Join(", ", aliasResult.Errors));
        return aliasResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MusicArtistAlias"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicArtistAlias"/> instances.</returns>
    public List<MusicArtistAlias> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
