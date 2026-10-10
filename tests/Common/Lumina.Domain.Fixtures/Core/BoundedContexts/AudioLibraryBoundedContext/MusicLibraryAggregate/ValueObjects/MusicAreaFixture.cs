#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MusicArea"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAreaFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicArea"/>.
    /// </summary>
    /// <param name="musicBrainzAreaId">Optional. The MusicBrainz identifier of the area.</param>
    /// <param name="name">Optional. The name of the area.</param>
    /// <param name="sortName">Optional. The sort name of the area.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the area.</param>
    /// <param name="type">Optional. The MusicBrainz type of the area.</param>
    /// <param name="iso3166Code">Optional. The ISO 3166 code of the area.</param>
    /// <returns>The created <see cref="MusicArea"/>.</returns>
    public MusicArea Create(
        MusicBrainzId? musicBrainzAreaId = null,
        string? name = null,
        Optional<string>? sortName = null,
        Optional<string>? disambiguation = null,
        Optional<string>? type = null,
        Optional<string>? iso3166Code = null)
    {
        Result<MusicArea> areaResult = MusicArea.Create(
            musicBrainzAreaId ?? _musicBrainzIdFixture.Create(),
            name ?? _faker.Address.Country(),
            sortName ?? Optional<string>.Some(_faker.Address.Country()),
            disambiguation ?? Optional<string>.Some(_faker.Lorem.Sentence()),
            type ?? Optional<string>.Some(_faker.Random.ArrayElement(["Country", "City", "Area"])),
            iso3166Code ?? Optional<string>.Some(_faker.Address.CountryCode()));

        if (areaResult.IsFailure)
            throw new System.InvalidOperationException("Failed to create MusicArea: " + string.Join(", ", areaResult.Errors));
        return areaResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MusicArea"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicArea"/> instances.</returns>
    public List<MusicArea> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
