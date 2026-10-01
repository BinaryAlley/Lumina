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
/// Fixture class for the <see cref="MusicBrainzMediumResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzMediumResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzTrackResponseFixture _musicBrainzTrackResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzMediumResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="position">Optional. The position of the medium within its release.</param>
    /// <param name="title">Optional. The title of the medium.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="format">Optional. The format of the medium.</param>
    /// <param name="includeFormat">Whether the format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="trackCount">Optional. The number of tracks of the medium.</param>
    /// <param name="tracks">Optional. The tracks of the medium.</param>
    /// <returns>A configured <see cref="MusicBrainzMediumResponse"/> instance.</returns>
    public MusicBrainzMediumResponse Create(
        int? position = null,
        string? title = null,
        bool includeTitle = true,
        string? format = null,
        bool includeFormat = true,
        int? trackCount = null,
        List<MusicBrainzTrackResponse>? tracks = null)
    {
        List<MusicBrainzTrackResponse> resolvedTracks = tracks ?? [.. _musicBrainzTrackResponseFixture.CreateMany(2)];
        return new MusicBrainzMediumResponse
        {
            Position = position ?? _faker.Random.Int(1, 4),
            Title = includeTitle ? title ?? _faker.Lorem.Sentence(2) : null,
            Format = includeFormat ? format ?? _faker.PickRandom("CD", "Vinyl", "Digital Media") : null,
            TrackCount = trackCount ?? resolvedTracks.Count,
            Tracks = resolvedTracks
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzMediumResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzMediumResponse"/> instances.</returns>
    public List<MusicBrainzMediumResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(index => Create(position: index + 1))];
    }
}
