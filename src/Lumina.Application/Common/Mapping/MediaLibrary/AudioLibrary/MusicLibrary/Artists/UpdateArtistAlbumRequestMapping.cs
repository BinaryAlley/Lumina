#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="UpdateArtistAlbumRequest"/>.
/// </summary>
public static class UpdateArtistAlbumRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddAlbumCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the album belongs to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist the album belongs to, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static AddAlbumCommand ToCommand(this UpdateArtistAlbumRequest request, string? libraryId, string? artistId)
    {
        return new AddAlbumCommand(
            libraryId,
            artistId,
            request.Metadata,
            request.MediaFormat,
            request.Barcode,
            request.CatalogNumber,
            request.MusicBrainzReleaseId,
            request.MusicBrainzReleaseGroupId,
            request.MusicBrainzReleaseArtistId,
            request.Contributors,
            request.Ratings,
            request.Tracks?.ToCommands(libraryId, artistId, null).ToList(),
            request.AlbumId);
    }

    /// <summary>
    /// Converts <paramref name="requests"/> to a collection of <see cref="AddAlbumCommand"/>.
    /// </summary>
    /// <param name="requests">The requests to be converted.</param>
    /// <param name="libraryId">The Id of the library the albums belong to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist the albums belong to, taken from the route.</param>
    /// <returns>The converted commands.</returns>
    public static IEnumerable<AddAlbumCommand> ToCommands(this IEnumerable<UpdateArtistAlbumRequest> requests, string? libraryId, string? artistId)
    {
        return requests.Select(request => request.ToCommand(libraryId, artistId));
    }
}
