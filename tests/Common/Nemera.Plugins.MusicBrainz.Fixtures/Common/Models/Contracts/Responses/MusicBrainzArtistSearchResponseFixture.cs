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
/// Fixture class for the <see cref="MusicBrainzArtistSearchResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzArtistSearchResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzArtistResponseFixture _musicBrainzArtistResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzArtistSearchResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="count">Optional. The total number of matching artists.</param>
    /// <param name="offset">Optional. The offset of the returned artists.</param>
    /// <param name="artists">Optional. The matching artists.</param>
    /// <returns>A configured <see cref="MusicBrainzArtistSearchResponse"/> instance.</returns>
    public MusicBrainzArtistSearchResponse Create(
        int? count = null,
        int? offset = null,
        List<MusicBrainzArtistResponse>? artists = null)
    {
        List<MusicBrainzArtistResponse> resolvedArtists = artists ?? [.. _musicBrainzArtistResponseFixture.CreateMany(2)];
        return new MusicBrainzArtistSearchResponse
        {
            Count = count ?? resolvedArtists.Count,
            Offset = offset ?? _faker.Random.Int(0, 100),
            Artists = resolvedArtists
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzArtistSearchResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzArtistSearchResponse"/> instances.</returns>
    public List<MusicBrainzArtistSearchResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
