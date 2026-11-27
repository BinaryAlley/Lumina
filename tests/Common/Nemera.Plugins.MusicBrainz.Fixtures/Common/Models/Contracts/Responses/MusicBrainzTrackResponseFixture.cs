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
/// Fixture class for the <see cref="MusicBrainzTrackResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzTrackResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzRecordingResponseFixture _musicBrainzRecordingResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzTrackResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the track.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="number">Optional. The track number, as displayed on the medium.</param>
    /// <param name="includeNumber">Whether the number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="position">Optional. The position of the track within its medium.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="length">Optional. The length of the track in milliseconds.</param>
    /// <param name="includeLength">Whether the length should be included, or forced to <see langword="null"/>.</param>
    /// <param name="recording">Optional. The recording the track is a performance of.</param>
    /// <param name="includeRecording">Whether the recording should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzTrackResponse"/> instance.</returns>
    public MusicBrainzTrackResponse Create(
        string? id = null,
        bool includeId = true,
        string? number = null,
        bool includeNumber = true,
        int? position = null,
        string? title = null,
        bool includeTitle = true,
        long? length = null,
        bool includeLength = true,
        MusicBrainzRecordingResponse? recording = null,
        bool includeRecording = true)
    {
        int resolvedPosition = position ?? _faker.Random.Int(1, 30);
        string? resolvedTitle = includeTitle ? title ?? _faker.Lorem.Sentence(3) : null;
        return new MusicBrainzTrackResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Number = includeNumber ? number ?? resolvedPosition.ToString() : null,
            Position = resolvedPosition,
            Title = resolvedTitle,
            Length = includeLength ? length ?? _faker.Random.Long(30_000, 600_000) : null,
            Recording = includeRecording ? recording ?? _musicBrainzRecordingResponseFixture.Create(title: resolvedTitle) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzTrackResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzTrackResponse"/> instances.</returns>
    public List<MusicBrainzTrackResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(index => Create(position: index + 1))];
    }
}
