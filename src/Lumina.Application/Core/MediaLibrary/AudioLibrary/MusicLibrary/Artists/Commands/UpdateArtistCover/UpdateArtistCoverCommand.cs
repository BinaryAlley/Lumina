#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;

/// <summary>
/// Command for updating the cover image of an existing artist.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this artist belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist whose cover image is updated, taken from the route.</param>
/// <param name="Cover">The stream of the uploaded cover image file.</param>
/// <param name="FileName">The name of the uploaded cover image file.</param>
[DebuggerDisplay("ArtistId: {ArtistId} FileName: {FileName}")]
public record UpdateArtistCoverCommand(
    string? LibraryId,
    string? ArtistId,
    Stream? Cover,
    string? FileName
) : ICommand;
