#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;

/// <summary>
/// Command for restoring a deleted artist by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this artist belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist to restore, taken from the route.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record RestoreArtistCommand(
    string? LibraryId,
    string? ArtistId
) : ICommand;
