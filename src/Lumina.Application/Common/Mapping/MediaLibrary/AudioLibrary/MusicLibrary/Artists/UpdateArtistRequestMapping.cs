#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="UpdateArtistRequest"/>.
/// </summary>
public static class UpdateArtistRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateArtistCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the artist belongs to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist to update, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static UpdateArtistCommand ToCommand(this UpdateArtistRequest request, string? libraryId, string? artistId)
    {
        return new UpdateArtistCommand(
            libraryId,
            artistId,
            request.Metadata,
            request.Website,
            request.MusicBrainzArtistId,
            request.Ipis,
            request.Isnis,
            request.Contributors,
            request.Ratings,
            request.Albums?.ToCommands(libraryId, artistId).ToList());
    }
}
