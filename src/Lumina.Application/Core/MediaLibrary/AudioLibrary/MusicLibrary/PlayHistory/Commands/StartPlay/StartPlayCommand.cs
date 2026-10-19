#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.StartPlay;

/// <summary>
/// Command for recording the start of a play of a track by the current user.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library that contains the track, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist that produced the track, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album that contains the track, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track that started playing, taken from the route.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record StartPlayCommand(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    string? TrackId
) : ICommand;
