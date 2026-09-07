#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
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
    private readonly List<Isrc> _isrcs;
    private readonly List<Mood> _moods;
    private readonly List<MediaContributorCredit> _credits;
    private readonly List<AudioRating> _ratings;

    /// <summary>
    /// Gets the Id of the media library this track belongs to.
    /// </summary>
    public LibraryId LibraryId { get; private set; }

    /// <summary>
    /// Gets the file system path of the track.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Gets the audio metadata of the track.
    /// </summary>
    public AudioMetadata Metadata { get; private set; }

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
    /// Gets the title of the work the track is a recording of, if applicable.
    /// </summary>
    public Optional<string> Work { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the recording, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzRecordingId { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the track, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzTrackId { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the work, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzWorkId { get; private set; }

    /// <summary>
    /// Gets the list of moods of the track.
    /// </summary>
    public IReadOnlyCollection<Mood> Moods => _moods.AsReadOnly();

    /// <summary>
    /// Gets the list of ISRC (International Standard Recording Code) of the track.
    /// </summary>
    public IReadOnlyCollection<Isrc> Isrcs => _isrcs.AsReadOnly();

    /// <summary>
    /// Gets the list of the credits of the media contributors of the track.
    /// </summary>
    public IReadOnlyCollection<MediaContributorCredit> Credits => _credits.AsReadOnly();

    /// <summary>
    /// Gets the list of ratings for this track.
    /// </summary>
    public IReadOnlyCollection<AudioRating> Ratings => _ratings.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Track"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the track.</param>
    /// <param name="libraryId">The Id of the media library this track belongs to.</param>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="isrcs">The list of ISRC of the track.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="moods">The list of moods of the track.</param>
    /// <param name="work">The optional title of the work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <param name="musicBrainzWorkId">The optional MusicBrainz identifier of the work.</param>
    /// <param name="credits">The list of the credits of the media contributors of the track.</param>
    /// <param name="ratings">The list of ratings for the track.</param>
    private Track(
        TrackId id,
        LibraryId libraryId,
        string path,
        AudioMetadata metadata,
        int trackNumber,
        Optional<int> discNumber,
        List<Isrc> isrcs,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        List<Mood> moods,
        Optional<string> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        Optional<MusicBrainzId> musicBrainzWorkId,
        List<MediaContributorCredit> credits,
        List<AudioRating> ratings) : base(id)
    {
        Id = id;
        LibraryId = libraryId;
        Path = path;
        Metadata = metadata;
        TrackNumber = trackNumber;
        DiscNumber = discNumber;
        _isrcs = isrcs;
        _moods = moods;
        Script = script;
        Key = key;
        Bpm = bpm;
        Work = work;
        MusicBrainzRecordingId = musicBrainzRecordingId;
        MusicBrainzTrackId = musicBrainzTrackId;
        MusicBrainzWorkId = musicBrainzWorkId;
        _credits = credits;
        _ratings = ratings;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Track"/> class.
    /// </summary>
    /// <param name="libraryId">The Id of the media library this track belongs to.</param>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="isrcs">The list of ISRC of the track.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="moods">The list of moods of the track.</param>
    /// <param name="work">The optional title of the work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <param name="musicBrainzWorkId">The optional MusicBrainz identifier of the work.</param>
    /// <param name="credits">The list of the credits of the media contributors of the track.</param>
    /// <param name="ratings">The list of ratings for the track.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Track"/>, or an error message.
    /// </returns>
    public static Result<Track> Create(
        LibraryId libraryId,
        string path,
        AudioMetadata metadata,
        int trackNumber,
        Optional<int> discNumber,
        List<Isrc> isrcs,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        List<Mood> moods,
        Optional<string> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        Optional<MusicBrainzId> musicBrainzWorkId,
        List<MediaContributorCredit> credits,
        List<AudioRating> ratings)
    {
        return new Track(
            TrackId.CreateUnique(),
            libraryId,
            path,
            metadata,
            trackNumber,
            discNumber,
            isrcs,
            script,
            key,
            bpm,
            moods,
            work,
            musicBrainzRecordingId,
            musicBrainzTrackId,
            musicBrainzWorkId,
            credits,
            ratings);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Track"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the track.</param>
    /// <param name="libraryId">The Id of the media library this track belongs to.</param>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="isrcs">The list of ISRC of the track.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="moods">The list of moods of the track.</param>
    /// <param name="work">The optional title of the work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <param name="musicBrainzWorkId">The optional MusicBrainz identifier of the work.</param>
    /// <param name="credits">The list of the credits of the media contributors of the track.</param>
    /// <param name="ratings">The list of ratings for the track.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Track"/>, or an error message.
    /// </returns>
    public static Result<Track> Create(
        TrackId id,
        LibraryId libraryId,
        string path,
        AudioMetadata metadata,
        int trackNumber,
        Optional<int> discNumber,
        List<Isrc> isrcs,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        List<Mood> moods,
        Optional<string> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        Optional<MusicBrainzId> musicBrainzWorkId,
        List<MediaContributorCredit> credits,
        List<AudioRating> ratings)
    {
        return new Track(
            id,
            libraryId,
            path,
            metadata,
            trackNumber,
            discNumber,
            isrcs,
            script,
            key,
            bpm,
            moods,
            work,
            musicBrainzRecordingId,
            musicBrainzTrackId,
            musicBrainzWorkId,
            credits,
            ratings);
    }

    /// <summary>
    /// Replaces the credits of the media contributors of the track with the provided <paramref name="credits"/>.
    /// </summary>
    /// <param name="credits">The credits of the media contributors of the track.</param>
    public void UpdateCredits(IReadOnlyCollection<MediaContributorCredit> credits)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the entity
        _credits.Clear();
        _credits.AddRange(credits);
    }
}
