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
/// Fixture class for the <see cref="MusicBrainzReleaseGroupSearchResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzReleaseGroupSearchResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzReleaseGroupResponseFixture _musicBrainzReleaseGroupResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzReleaseGroupSearchResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="count">Optional. The total number of matching release groups.</param>
    /// <param name="offset">Optional. The offset of the returned release groups.</param>
    /// <param name="releaseGroups">Optional. The matching release groups.</param>
    /// <returns>A configured <see cref="MusicBrainzReleaseGroupSearchResponse"/> instance.</returns>
    public MusicBrainzReleaseGroupSearchResponse Create(
        int? count = null,
        int? offset = null,
        List<MusicBrainzReleaseGroupResponse>? releaseGroups = null)
    {
        List<MusicBrainzReleaseGroupResponse> resolvedReleaseGroups = releaseGroups ?? [.. _musicBrainzReleaseGroupResponseFixture.CreateMany(2)];
        return new MusicBrainzReleaseGroupSearchResponse
        {
            Count = count ?? resolvedReleaseGroups.Count,
            Offset = offset ?? _faker.Random.Int(0, 100),
            ReleaseGroups = resolvedReleaseGroups
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzReleaseGroupSearchResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzReleaseGroupSearchResponse"/> instances.</returns>
    public List<MusicBrainzReleaseGroupSearchResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
