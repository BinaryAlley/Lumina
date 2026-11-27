#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for an album metadata lookup.
/// </summary>
/// <param name="LibraryId">The Id of the media library the album belongs to.</param>
/// <param name="Path">The file system path of a track of the album.</param>
/// <param name="MusicBrainzReleaseGroupId">The MusicBrainz identifier of the release group of the album, if applicable.</param>
/// <param name="MusicBrainzReleaseId">The MusicBrainz identifier of the release of the album, if applicable.</param>
/// <param name="Title">The title of the album, if applicable.</param>
/// <param name="ArtistName">The name of the artist of the album, if applicable.</param>
/// <param name="ReleaseYear">The year the album was released, if applicable.</param>
/// <param name="TrackCount">The number of tracks the local album has, if applicable. It is used to select a release whose tracklist matches the local files.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record AlbumMetadataLookupDto(
    Guid LibraryId,
    string Path,
    Guid? MusicBrainzReleaseGroupId = null,
    Guid? MusicBrainzReleaseId = null,
    string? Title = null,
    string? ArtistName = null,
    int? ReleaseYear = null,
    int? TrackCount = null
) : MetadataLookupDto;
