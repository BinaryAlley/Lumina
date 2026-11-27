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
/// Fixture class for the <see cref="MusicBrainzReleaseBrowseResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzReleaseBrowseResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzReleaseResponseFixture _musicBrainzReleaseResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzReleaseBrowseResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="releases">Optional. The releases returned by the browse request.</param>
    /// <param name="releaseCount">Optional. The total number of releases linked to the browsed entity.</param>
    /// <param name="releaseOffset">Optional. The offset of the returned releases.</param>
    /// <returns>A configured <see cref="MusicBrainzReleaseBrowseResponse"/> instance.</returns>
    public MusicBrainzReleaseBrowseResponse Create(
        List<MusicBrainzReleaseResponse>? releases = null,
        int? releaseCount = null,
        int? releaseOffset = null)
    {
        List<MusicBrainzReleaseResponse> resolvedReleases = releases ?? [.. _musicBrainzReleaseResponseFixture.CreateMany(2)];
        return new MusicBrainzReleaseBrowseResponse
        {
            Releases = resolvedReleases,
            ReleaseCount = releaseCount ?? resolvedReleases.Count,
            ReleaseOffset = releaseOffset ?? _faker.Random.Int(0, 100)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzReleaseBrowseResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzReleaseBrowseResponse"/> instances.</returns>
    public List<MusicBrainzReleaseBrowseResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
