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
/// Fixture class for the <see cref="MusicBrainzLifeSpanResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzLifeSpanResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzLifeSpanResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="begin">Optional. The begin date of the life span.</param>
    /// <param name="includeBegin">Whether the begin date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="end">Optional. The end date of the life span.</param>
    /// <param name="includeEnd">Whether the end date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="isEnded">Optional. Whether the life span has ended.</param>
    /// <param name="includeEnded">Whether the ended flag should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzLifeSpanResponse"/> instance.</returns>
    public MusicBrainzLifeSpanResponse Create(
        string? begin = null,
        bool includeBegin = true,
        string? end = null,
        bool includeEnd = true,
        bool? isEnded = null,
        bool includeEnded = true)
    {
        return new MusicBrainzLifeSpanResponse
        {
            Begin = includeBegin ? begin ?? _faker.Date.Past(60).ToString("yyyy-MM-dd") : null,
            End = includeEnd ? end ?? _faker.Date.Past(5).ToString("yyyy-MM-dd") : null,
            IsEnded = includeEnded ? isEnded ?? _faker.Random.Bool() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzLifeSpanResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzLifeSpanResponse"/> instances.</returns>
    public List<MusicBrainzLifeSpanResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
