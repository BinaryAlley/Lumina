#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;

/// <summary>
/// Command for restoring a deleted album by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this album belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album to restore, taken from the route.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record RestoreAlbumCommand(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId
) : ICommand;
