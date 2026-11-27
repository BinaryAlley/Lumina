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
/// Fixture class for the <see cref="MusicBrainzReleaseEventResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzReleaseEventResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzAreaResponseFixture _musicBrainzAreaResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzReleaseEventResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="date">Optional. The date of the release event.</param>
    /// <param name="includeDate">Whether the date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="area">Optional. The area of the release event.</param>
    /// <param name="includeArea">Whether the area should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzReleaseEventResponse"/> instance.</returns>
    public MusicBrainzReleaseEventResponse Create(
        string? date = null,
        bool includeDate = true,
        MusicBrainzAreaResponse? area = null,
        bool includeArea = true)
    {
        return new MusicBrainzReleaseEventResponse
        {
            Date = includeDate ? date ?? _faker.Date.Past(40).ToString("yyyy-MM-dd") : null,
            Area = includeArea ? area ?? _musicBrainzAreaResponseFixture.Create() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzReleaseEventResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzReleaseEventResponse"/> instances.</returns>
    public List<MusicBrainzReleaseEventResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
