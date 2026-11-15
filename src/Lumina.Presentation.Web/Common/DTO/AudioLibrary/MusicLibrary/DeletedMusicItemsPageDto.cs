#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object carrying the data needed by the deleted items page of a music library.
/// </summary>
/// <param name="Library">The media library whose deleted music items are shown.</param>
/// <param name="DeletedArtists">The deleted artists of the media library, ordered by the time they were deleted, most recent first.</param>
/// <param name="DeletedAlbums">The deleted albums of the media library, ordered by the time they were deleted, most recent first.</param>
/// <param name="DeletedTracks">The deleted tracks of the media library, ordered by the time they were deleted, most recent first.</param>
public sealed record DeletedMusicItemsPageDto(
    LibraryDto Library,
    IReadOnlyList<DeletedMusicArtistDto> DeletedArtists,
    IReadOnlyList<DeletedMusicAlbumDto> DeletedAlbums,
    IReadOnlyList<DeletedMusicTrackDto> DeletedTracks
);
