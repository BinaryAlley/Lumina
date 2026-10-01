#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="MusicBrainzRecordingListResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzRecordingListResponseFixture
{
    private readonly MusicBrainzRecordingResponseFixture _musicBrainzRecordingResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzRecordingListResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="recordings">Optional. The recordings that carry the looked up ISRC.</param>
    /// <returns>A configured <see cref="MusicBrainzRecordingListResponse"/> instance.</returns>
    public MusicBrainzRecordingListResponse Create(List<MusicBrainzRecordingResponse>? recordings = null)
    {
        return new MusicBrainzRecordingListResponse
        {
            Recordings = recordings ?? [.. _musicBrainzRecordingResponseFixture.CreateMany(2)]
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzRecordingListResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzRecordingListResponse"/> instances.</returns>
    public List<MusicBrainzRecordingListResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
