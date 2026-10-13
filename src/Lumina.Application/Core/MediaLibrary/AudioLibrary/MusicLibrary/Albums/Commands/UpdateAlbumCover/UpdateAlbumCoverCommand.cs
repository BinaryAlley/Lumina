#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;

/// <summary>
/// Command for updating the cover image of an existing album.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the album belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album whose cover image is updated, taken from the route.</param>
/// <param name="Cover">The stream of the uploaded cover image file.</param>
/// <param name="FileName">The name of the uploaded cover image file.</param>
[DebuggerDisplay("AlbumId: {AlbumId} FileName: {FileName}")]
public record UpdateAlbumCoverCommand(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    Stream? Cover,
    string? FileName
) : ICommand;
