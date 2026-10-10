#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;

/// <summary>
/// Entity for a track, the individual song of an album.
/// </summary>
[DebuggerDisplay("{Id}: {Metadata.Title}")]
public sealed class Track : Entity<TrackId>
{
    private readonly List<Mood> _moods;
    private readonly List<Isrc> _isrcs;
    private readonly List<MusicMediaContributor> _contributors;
    private readonly List<AudioRating> _ratings;

    /// <summary>
    /// Gets the file system path of the track.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Gets the audio metadata of the track.
    /// </summary>
    public AudioMetadata Metadata { get; private set; }

    /// <summary>
    /// Gets the disambiguation comment of the track, used to distinguish tracks with the same title, if applicable.
    /// </summary>
    public Optional<string> Disambiguation { get; private set; }

    /// <summary>
    /// Gets the number of the track on its disc.
    /// </summary>
    public int TrackNumber { get; private set; }

    /// <summary>
    /// Gets the number of the disc the track belongs to, if applicable.
    /// </summary>
    public Optional<int> DiscNumber { get; private set; }

    /// <summary>
    /// Gets the script used by the language of the track, if applicable.
    /// </summary>
    public Optional<string> Script { get; private set; }

    /// <summary>
    /// Gets the musical key of the track, if applicable.
    /// </summary>
    public Optional<MusicKey> Key { get; private set; }

    /// <summary>
    /// Gets the tempo of the track in beats per minute, if applicable.
    /// </summary>
    public Optional<int> Bpm { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the recording of the track is a video recording.
    /// </summary>
    public bool IsVideo { get; private set; }

    /// <summary>
    /// Gets the work the track is a recording of, if applicable.
    /// </summary>
    public Optional<MusicWork> Work { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the recording, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzRecordingId { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the track, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzTrackId { get; private set; }

    /// <summary>
    /// Gets the list of moods of the track.
    /// </summary>
    public IReadOnlyCollection<Mood> Moods => _moods.AsReadOnly();

    /// <summary>
    /// Gets the list of ISRC (International Standard Recording Code) of the track.
    /// </summary>
    public IReadOnlyCollection<Isrc> Isrcs => _isrcs.AsReadOnly();

    /// <summary>
    /// Gets the list of the media contributors of the track.
    /// </summary>
    public IReadOnlyCollection<MusicMediaContributor> Contributors => _contributors.AsReadOnly();

    /// <summary>
    /// Gets the list of ratings for this track.
    /// </summary>
    public IReadOnlyCollection<AudioRating> Ratings => _ratings.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Track"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the track.</param>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="moods">The list of moods of the track.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="isVideo">Whether the recording of the track is a video recording.</param>
    /// <param name="isrcs">The list of ISRC of the track.</param>
    /// <param name="work">The optional work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <param name="contributors">The list of the media contributors of the track.</param>
    /// <param name="ratings">The list of ratings of the track.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    private Track(
        TrackId id,
        string path,
        AudioMetadata metadata,
        Optional<string> disambiguation,
        int trackNumber,
        Optional<int> discNumber,
        List<Mood> moods,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        bool isVideo,
        List<Isrc> isrcs,
        Optional<MusicWork> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        List<MusicMediaContributor> contributors,
        List<AudioRating> ratings,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        Path = path;
        Metadata = metadata;
        Disambiguation = disambiguation;
        TrackNumber = trackNumber;
        DiscNumber = discNumber;
        _moods = moods;
        _isrcs = isrcs;
        Script = script;
        Key = key;
        Bpm = bpm;
        IsVideo = isVideo;
        Work = work;
        MusicBrainzRecordingId = musicBrainzRecordingId;
        MusicBrainzTrackId = musicBrainzTrackId;
        _contributors = contributors;
        _ratings = ratings;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Track"/> class.
    /// </summary>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="moods">The list of moods of the track.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="isVideo">Whether the recording of the track is a video recording.</param>
    /// <param name="isrcs">The list of ISRC of the track.</param>
    /// <param name="work">The optional work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <param name="contributors">The list of the media contributors of the track.</param>
    /// <param name="ratings">The list of ratings of the track.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Track"/>, or an error message.
    /// </returns>
    public static Result<Track> Create(
        string path,
        AudioMetadata metadata,
        Optional<string> disambiguation,
        int trackNumber,
        Optional<int> discNumber,
        List<Mood> moods,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        bool isVideo,
        List<Isrc> isrcs,
        Optional<MusicWork> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        List<MusicMediaContributor> contributors,
        List<AudioRating> ratings)
    {
        return new Track(
            TrackId.CreateUnique(),
            path,
            metadata,
            disambiguation,
            trackNumber,
            discNumber,
            moods,
            script,
            key,
            bpm,
            isVideo,
            isrcs,
            work,
            musicBrainzRecordingId,
            musicBrainzTrackId,
            contributors,
            ratings,
            DateTime.UtcNow, // TODO: should be IDateTimeProvider
            Optional<DateTime>.None());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Track"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the track.</param>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="moods">The list of moods of the track.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="isVideo">Whether the recording of the track is a video recording.</param>
    /// <param name="isrcs">The list of ISRC of the track.</param>
    /// <param name="work">The optional work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <param name="contributors">The list of the media contributors of the track.</param>
    /// <param name="ratings">The list of ratings of the track.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Track"/>, or an error message.
    /// </returns>
    public static Result<Track> Create(
        TrackId id,
        string path,
        AudioMetadata metadata,
        Optional<string> disambiguation,
        int trackNumber,
        Optional<int> discNumber,
        List<Mood> moods,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        bool isVideo,
        List<Isrc> isrcs,
        Optional<MusicWork> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        List<MusicMediaContributor> contributors,
        List<AudioRating> ratings,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc)
    {
        return new Track(
            id,
            path,
            metadata,
            disambiguation,
            trackNumber,
            discNumber,
            moods,
            script,
            key,
            bpm,
            isVideo,
            isrcs,
            work,
            musicBrainzRecordingId,
            musicBrainzTrackId,
            contributors,
            ratings,
            createdOnUtc,
            updatedOnUtc);
    }

    /// <summary>
    /// Replaces the moods of the track with the provided <paramref name="moods"/>.
    /// </summary>
    /// <param name="moods">The moods of the track.</param>
    internal void UpdateMoods(IReadOnlyCollection<Mood> moods)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the entity
        _moods.Clear();
        _moods.AddRange(moods);
    }

    /// <summary>
    /// Replaces the ISRCs of the track with the provided <paramref name="isrcs"/>.
    /// </summary>
    /// <param name="isrcs">The ISRCs of the track.</param>
    internal void UpdateIsrcs(IReadOnlyCollection<Isrc> isrcs)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the entity
        _isrcs.Clear();
        _isrcs.AddRange(isrcs);
    }

    /// <summary>
    /// Replaces the media contributors of the track with the provided <paramref name="contributors"/>.
    /// </summary>
    /// <param name="contributors">The media contributors of the track.</param>
    internal void UpdateContributors(IReadOnlyCollection<MusicMediaContributor> contributors)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the entity
        _contributors.Clear();
        _contributors.AddRange(contributors);
    }

    /// <summary>
    /// Replaces the ratings of the track with the provided <paramref name="ratings"/>.
    /// </summary>
    /// <param name="ratings">The ratings of the track.</param>
    internal void UpdateRatings(IReadOnlyCollection<AudioRating> ratings)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the entity
        _ratings.Clear();
        _ratings.AddRange(ratings);
    }

    /// <summary>
    /// Updates the details of the track, without touching the moods, the ISRCs, the media contributors, and the ratings.
    /// </summary>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="isVideo">Whether the recording of the track is a video recording.</param>
    /// <param name="work">The optional work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    internal Result<Updated> UpdateDetails(
        string path,
        AudioMetadata metadata,
        int trackNumber,
        Optional<int> discNumber,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        bool isVideo,
        Optional<MusicWork> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId)
    {
        Path = path;
        Metadata = metadata;
        TrackNumber = trackNumber;
        DiscNumber = discNumber;
        Script = script;
        Key = key;
        Bpm = bpm;
        IsVideo = isVideo;
        Work = work;
        MusicBrainzRecordingId = musicBrainzRecordingId;
        MusicBrainzTrackId = musicBrainzTrackId;
        UpdatedOnUtc = Optional<DateTime>.Some(DateTime.UtcNow);
        return Result.Updated;
    }
}
