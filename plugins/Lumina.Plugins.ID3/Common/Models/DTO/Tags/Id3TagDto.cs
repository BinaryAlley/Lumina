#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.ID3.Common.Models.DTO.Tags;

/// <summary>
/// Snapshot of the metadata read from the embedded tags of a single audio file.
/// Every audio property that the tags do not carry is left empty, so that the local values already extracted from the file are never overwritten with blanks.
/// </summary>
internal sealed class Id3TagDto
{
    /// <summary>
    /// Gets or initializes the title of the track.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or initializes the title of the album the track belongs to.
    /// </summary>
    public string? Album { get; init; }

    /// <summary>
    /// Gets or initializes the artists credited on the track.
    /// </summary>
    public List<string> TrackArtists { get; init; } = [];

    /// <summary>
    /// Gets or initializes the artists credited on the album.
    /// </summary>
    public List<string> AlbumArtists { get; init; } = [];

    /// <summary>
    /// Gets or initializes the sort name of the album artist.
    /// </summary>
    public string? AlbumArtistSortName { get; init; }

    /// <summary>
    /// Gets or initializes the sort name of the track artist.
    /// </summary>
    public string? TrackArtistSortName { get; init; }

    /// <summary>
    /// Gets or initializes the number of the track on its disc.
    /// </summary>
    public uint TrackNumber { get; init; }

    /// <summary>
    /// Gets or initializes the number of tracks on the disc, as reported by the tags.
    /// </summary>
    public uint TrackCount { get; init; }

    /// <summary>
    /// Gets or initializes the number of the disc the track belongs to.
    /// </summary>
    public uint DiscNumber { get; init; }

    /// <summary>
    /// Gets or initializes the number of discs of the release, as reported by the tags.
    /// </summary>
    public uint DiscCount { get; init; }

    /// <summary>
    /// Gets or initializes the year the track was released.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Gets or initializes the original release date of the track.
    /// </summary>
    public DateOnly? OriginalReleaseDate { get; init; }

    /// <summary>
    /// Gets or initializes the original release year of the track.
    /// </summary>
    public int? OriginalReleaseYear { get; init; }

    /// <summary>
    /// Gets or initializes the genres the tags associate with the track.
    /// </summary>
    public List<string> Genres { get; init; } = [];

    /// <summary>
    /// Gets or initializes the composers credited on the track.
    /// </summary>
    public List<string> Composers { get; init; } = [];

    /// <summary>
    /// Gets or initializes the lyricists credited on the track.
    /// </summary>
    public List<string> Lyricists { get; init; } = [];

    /// <summary>
    /// Gets or initializes the conductors credited on the track.
    /// </summary>
    public List<string> Conductors { get; init; } = [];

    /// <summary>
    /// Gets or initializes the remixers credited on the track.
    /// </summary>
    public List<string> Remixers { get; init; } = [];

    /// <summary>
    /// Gets or initializes the people credited on the track, each with the free-form role read from the tags.
    /// </summary>
    public List<(string Role, string Name)> InvolvedPeople { get; init; } = [];

    /// <summary>
    /// Gets or initializes the language of the track.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets or initializes the script used by the language of the track.
    /// </summary>
    public string? Script { get; init; }

    /// <summary>
    /// Gets or initializes the label that issued the album.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Gets or initializes the catalog numbers of the album.
    /// </summary>
    public List<string> CatalogNumbers { get; init; } = [];

    /// <summary>
    /// Gets or initializes the barcode of the album.
    /// </summary>
    public string? Barcode { get; init; }

    /// <summary>
    /// Gets or initializes the types of the release, as read from the tags.
    /// </summary>
    public List<string> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Gets or initializes the status of the release.
    /// </summary>
    public string? ReleaseStatus { get; init; }

    /// <summary>
    /// Gets or initializes the country the release was issued in.
    /// </summary>
    public string? ReleaseCountry { get; init; }

    /// <summary>
    /// Gets or initializes the physical or digital medium of the release.
    /// </summary>
    public string? MediaFormat { get; init; }

    /// <summary>
    /// Gets or initializes the outermost packaging of the release.
    /// </summary>
    public string? Packaging { get; init; }

    /// <summary>
    /// Gets or initializes the ASIN of the album.
    /// </summary>
    public string? Asin { get; init; }

    /// <summary>
    /// Gets or initializes the ISRCs of the track.
    /// </summary>
    public List<string> Isrcs { get; init; } = [];

    /// <summary>
    /// Gets or initializes the moods of the track.
    /// </summary>
    public List<string> Moods { get; init; } = [];

    /// <summary>
    /// Gets or initializes the title of the work the track is a recording of.
    /// </summary>
    public string? WorkTitle { get; init; }

    /// <summary>
    /// Gets or initializes the musical key of the track.
    /// </summary>
    public string? MusicKey { get; init; }

    /// <summary>
    /// Gets or initializes the tempo of the track in beats per minute.
    /// </summary>
    public int? Bpm { get; init; }

    /// <summary>
    /// Gets or initializes the duration of the audio of the track in seconds.
    /// </summary>
    public int? DurationInSeconds { get; init; }

    /// <summary>
    /// Gets or initializes the sample rate of the audio of the track in Hz.
    /// </summary>
    public int? SampleRate { get; init; }

    /// <summary>
    /// Gets or initializes the number of audio channels of the track.
    /// </summary>
    public int? Channels { get; init; }

    /// <summary>
    /// Gets or initializes the bit depth of the audio of the track.
    /// </summary>
    public int? BitDepth { get; init; }

    /// <summary>
    /// Gets or initializes the bitrate of the audio of the track in kbps.
    /// </summary>
    public int? Bitrate { get; init; }

    /// <summary>
    /// Gets or initializes the description of the audio codec of the track.
    /// </summary>
    public string? AudioCodec { get; init; }

    /// <summary>
    /// Gets or initializes the official website associated with the artist.
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Gets or initializes the MusicBrainz identifier of the artist.
    /// </summary>
    public string? MusicBrainzArtistId { get; init; }

    /// <summary>
    /// Gets or initializes the MusicBrainz identifier of the release artist.
    /// </summary>
    public string? MusicBrainzReleaseArtistId { get; init; }

    /// <summary>
    /// Gets or initializes the MusicBrainz identifier of the release group.
    /// </summary>
    public string? MusicBrainzReleaseGroupId { get; init; }

    /// <summary>
    /// Gets or initializes the MusicBrainz identifier of the release.
    /// </summary>
    public string? MusicBrainzReleaseId { get; init; }

    /// <summary>
    /// Gets or initializes the MusicBrainz identifier of the recording of the track.
    /// </summary>
    public string? MusicBrainzRecordingId { get; init; }

    /// <summary>
    /// Gets or initializes the MusicBrainz identifier of the track on its release.
    /// </summary>
    public string? MusicBrainzReleaseTrackId { get; init; }

    /// <summary>
    /// Gets or initializes the MusicBrainz identifier of the work the track is a recording of.
    /// </summary>
    public string? MusicBrainzWorkId { get; init; }
}
