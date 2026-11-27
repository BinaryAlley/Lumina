#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;

/// <summary>
/// Command for restoring a deleted track by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this track belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album of the track belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track to restore, taken from the route.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record RestoreTrackCommand(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    string? TrackId
) : ICommand;
