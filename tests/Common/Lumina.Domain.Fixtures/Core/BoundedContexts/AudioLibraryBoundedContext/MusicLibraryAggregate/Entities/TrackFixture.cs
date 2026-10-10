#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;

/// <summary>
/// Fixture class for the <see cref="Track"/> domain entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackFixture
{
    private readonly Faker _faker = new();
    private readonly AudioMetadataFixture _audioMetadataFixture = new();
    private readonly MoodFixture _moodFixture = new();
    private readonly IsrcFixture _isrcFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly MusicWorkFixture _musicWorkFixture = new();
    private readonly AudioRatingFixture _audioRatingFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="Track"/> domain entity.
    /// </summary>
    /// <param name="path">Optional. The file system path of the track.</param>
    /// <param name="metadata">Optional. The audio metadata of the track.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="moods">Optional. The moods of the track.</param>
    /// <param name="script">Optional. The script used by the language of the track.</param>
    /// <param name="key">Optional. The musical key of the track.</param>
    /// <param name="bpm">Optional. The tempo of the track in beats per minute.</param>
    /// <param name="isVideo">Optional. Whether the recording of the track is a video recording.</param>
    /// <param name="isrcs">Optional. The ISRCs of the track.</param>
    /// <param name="work">Optional. The work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">Optional. The MusicBrainz identifier of the track.</param>
    /// <param name="contributors">Optional. The media contributors of the track.</param>
    /// <param name="ratings">Optional. The ratings of the track.</param>
    /// <returns>The created <see cref="Track"/> domain entity.</returns>
    public Track Create(
        string? path = null,
        AudioMetadata? metadata = null,
        Optional<string>? disambiguation = null,
        int? trackNumber = null,
        Optional<int>? discNumber = null,
        List<Mood>? moods = null,
        Optional<string>? script = null,
        Optional<MusicKey>? key = null,
        Optional<int>? bpm = null,
        bool? isVideo = null,
        List<Isrc>? isrcs = null,
        Optional<MusicWork>? work = null,
        Optional<MusicBrainzId>? musicBrainzRecordingId = null,
        Optional<MusicBrainzId>? musicBrainzTrackId = null,
        List<MusicMediaContributor>? contributors = null,
        List<AudioRating>? ratings = null)
    {
        Result<Track> trackResult = Track.Create(
            path ?? _faker.System.FilePath(),
            metadata ?? _audioMetadataFixture.Create(),
            disambiguation ?? Optional<string>.None(),
            trackNumber ?? _faker.Random.Int(1, 30),
            discNumber ?? Optional<int>.Some(_faker.Random.Int(1, 2)),
            moods ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(_ => _moodFixture.Create())],
            script ?? Optional<string>.Some(_faker.Lorem.Word()),
            key ?? Optional<MusicKey>.Some(_faker.PickRandom<MusicKey>()),
            bpm ?? Optional<int>.Some(_faker.Random.Int(60, 220)),
            isVideo ?? _faker.Random.Bool(),
            isrcs ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _isrcFixture.Create())],
            work ?? Optional<MusicWork>.Some(_musicWorkFixture.Create()),
            musicBrainzRecordingId ?? Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create()),
            musicBrainzTrackId ?? Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create()),
            contributors ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(_ => _musicMediaContributorFixture.Create())],
            ratings ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _audioRatingFixture.Create())]);

        if (trackResult.IsFailure)
            throw new InvalidOperationException("Failed to create Track: " + string.Join(", ", trackResult.Errors));
        return trackResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Track"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Track"/> instances.</returns>
    public List<Track> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
