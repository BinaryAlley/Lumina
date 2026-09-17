#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="UpdateAlbumRequest"/>.
/// </summary>
public static class UpdateAlbumRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateAlbumCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the album belongs to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist the album belongs to, taken from the route.</param>
    /// <param name="albumId">The Id of the album to update, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static UpdateAlbumCommand ToCommand(this UpdateAlbumRequest request, string? libraryId, string? artistId, string? albumId)
    {
        return new UpdateAlbumCommand(
            libraryId,
            artistId,
            albumId,
            request.Metadata,
            request.MediaFormat,
            request.Barcode,
            request.CatalogNumber,
            request.MusicBrainzReleaseId,
            request.MusicBrainzReleaseGroupId,
            request.MusicBrainzReleaseArtistId,
            request.Contributors,
            request.Ratings);
    }
}
