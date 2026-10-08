#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a track, the individual song of an album.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class TrackEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the track.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the album the track belongs to.
    /// </summary>
    public required Guid AlbumId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library this track belongs to.
    /// </summary>
    public required Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the file system path of the track.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the title of the track.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the original title of the track, if applicable.
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets the description of the track, if applicable.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the disambiguation comment of the track, used to distinguish tracks with the same title, if applicable.
    /// </summary>
    public string? Disambiguation { get; set; }

    /// <summary>
    /// Gets or sets the original release date of the track, if applicable.
    /// </summary>
    public DateOnly? OriginalReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the original release year of the track, if applicable.
    /// </summary>
    public int? OriginalReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the re-release or reissue date of the track, if applicable.
    /// </summary>
    public DateOnly? ReReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the re-release or reissue year of the track, if applicable.
    /// </summary>
    public int? ReReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the country or region of release, if applicable.
    /// </summary>
    public ReleaseCountry? ReleaseCountry { get; set; }

    /// <summary>
    /// Gets or sets the release version or edition of the track, if applicable.
    /// </summary>
    public string? ReleaseVersion { get; set; }

    /// <summary>
    /// Gets or sets the ISO 639-1 two-letter language code of the track, if applicable.
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the full name of the language of the track in English, if applicable.
    /// </summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// Gets or sets the native name of the language of the track, if applicable.
    /// </summary>
    public string? LanguageNativeName { get; set; }

    /// <summary>
    /// Gets or sets the ISO 639-1 two-letter original language code of the track, if applicable.
    /// </summary>
    public string? OriginalLanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the full name of the original language of the track in English, if applicable.
    /// </summary>
    public string? OriginalLanguageName { get; set; }

    /// <summary>
    /// Gets or sets the native name of the original language of the track, if applicable.
    /// </summary>
    public string? OriginalLanguageNativeName { get; set; }

    /// <summary>
    /// Gets or sets the duration of the audio in seconds.
    /// </summary>
    public int DurationInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the sample rate of the audio in Hz.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the number of audio channels.
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Gets or sets the bit depth of the audio, if applicable.
    /// </summary>
    public int? BitDepth { get; set; }

    /// <summary>
    /// Gets or sets the audio codec used, if applicable.
    /// </summary>
    public string? AudioCodec { get; set; }

    /// <summary>
    /// Gets or sets the bitrate of the audio in kbps, if applicable.
    /// </summary>
    public int? Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the AcoustID fingerprint identifier of the audio file, if applicable.
    /// </summary>
    public string? AcoustId { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track gain in decibels, if applicable.
    /// </summary>
    public decimal? ReplayGainTrackGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track peak, if applicable.
    /// </summary>
    public decimal? ReplayGainTrackPeak { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album gain in decibels, if applicable.
    /// </summary>
    public decimal? ReplayGainAlbumGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album peak, if applicable.
    /// </summary>
    public decimal? ReplayGainAlbumPeak { get; set; }

    /// <summary>
    /// Gets or sets the number of the track on its disc.
    /// </summary>
    public int TrackNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of the disc the track belongs to, if applicable.
    /// </summary>
    public int? DiscNumber { get; set; }

    /// <summary>
    /// Gets or sets the script used by the language of the track, if applicable.
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Gets or sets the musical key of the track, if applicable.
    /// </summary>
    public MusicKey? Key { get; set; }

    /// <summary>
    /// Gets or sets the tempo of the track in beats per minute, if applicable.
    /// </summary>
    public int? Bpm { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the recording of the track is a video recording.
    /// </summary>
    public bool IsVideo { get; set; }

    /// <summary>
    /// Gets or sets the title of the work the track is a recording of, if applicable.
    /// </summary>
    public string? WorkTitle { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz type of the work the track is a recording of, if applicable.
    /// </summary>
    public string? WorkType { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the recording, if applicable.
    /// </summary>
    public Guid? MusicBrainzRecordingId { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the track, if applicable.
    /// </summary>
    public Guid? MusicBrainzTrackId { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the work, if applicable.
    /// </summary>
    public Guid? MusicBrainzWorkId { get; set; }

    /// <summary>
    /// Gets or sets the album the track belongs to.
    /// </summary>
    public AlbumEntity? Album { get; set; }

    /// <summary>
    /// Gets or sets the list of moods of the track.
    /// </summary>
    public List<TrackMoodEntity> Moods { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of ISRC (International Standard Recording Code) of the track.
    /// </summary>
    public List<TrackIsrcEntity> Isrcs { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of the languages of the work the track is a recording of.
    /// </summary>
    public List<TrackWorkLanguageEntity> WorkLanguages { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of ISWC (International Standard Musical Work Code) of the work the track is a recording of.
    /// </summary>
    public List<TrackWorkIswcEntity> WorkIswcs { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of the media contributors of the track.
    /// </summary>
    public List<TrackContributorEntity> Contributors { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of ratings for this track.
    /// </summary>
    public List<AudioRatingEntity> Ratings { get; set; } = [];

    /// <summary>
    /// Gets or sets the genres of the track.
    /// </summary>
    public HashSet<GenreEntity> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags of the track.
    /// </summary>
    public HashSet<TagEntity> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the status of the metadata enrichment of the track.
    /// </summary>
    public MetadataStatus MetadataStatus { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the metadata of the track was last enriched.
    /// </summary>
    public DateTime? LastMetadataUpdateUtc { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin that enriched the metadata of the track.
    /// </summary>
    public string? MetadataProvider { get; set; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public required DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public required Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public required Guid? UpdatedBy { get; set; }
}
