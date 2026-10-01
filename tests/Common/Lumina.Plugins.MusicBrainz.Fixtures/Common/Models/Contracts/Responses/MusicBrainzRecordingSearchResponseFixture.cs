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
/// Fixture class for the <see cref="MusicBrainzRecordingSearchResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzRecordingSearchResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzRecordingResponseFixture _musicBrainzRecordingResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzRecordingSearchResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="count">Optional. The total number of matching recordings.</param>
    /// <param name="offset">Optional. The offset of the returned recordings.</param>
    /// <param name="recordings">Optional. The matching recordings.</param>
    /// <returns>A configured <see cref="MusicBrainzRecordingSearchResponse"/> instance.</returns>
    public MusicBrainzRecordingSearchResponse Create(
        int? count = null,
        int? offset = null,
        List<MusicBrainzRecordingResponse>? recordings = null)
    {
        List<MusicBrainzRecordingResponse> resolvedRecordings = recordings ?? [.. _musicBrainzRecordingResponseFixture.CreateMany(2)];
        return new MusicBrainzRecordingSearchResponse
        {
            Count = count ?? resolvedRecordings.Count,
            Offset = offset ?? _faker.Random.Int(0, 100),
            Recordings = resolvedRecordings
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzRecordingSearchResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzRecordingSearchResponse"/> instances.</returns>
    public List<MusicBrainzRecordingSearchResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
