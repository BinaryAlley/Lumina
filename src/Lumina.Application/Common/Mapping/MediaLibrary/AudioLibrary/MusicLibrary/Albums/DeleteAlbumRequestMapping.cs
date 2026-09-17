#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="DeleteAlbumRequest"/>.
/// </summary>
public static class DeleteAlbumRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="DeleteAlbumCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static DeleteAlbumCommand ToCommand(this DeleteAlbumRequest request)
    {
        return new DeleteAlbumCommand(
            request.AlbumId);
    }
}
