#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Command for deleting a track by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this track belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album of the track belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track to delete, taken from the route.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record DeleteTrackCommand(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    string? TrackId
) : ICommand;
