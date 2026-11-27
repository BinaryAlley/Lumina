#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.DeleteArtist;

/// <summary>
/// Command for deleting an artist by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this artist belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist to delete, taken from the route.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record DeleteArtistCommand(
    string? LibraryId,
    string? ArtistId
) : ICommand;
