#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="AddArtistRequest"/>.
/// </summary>
public static class AddArtistRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddArtistCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the artist is added to, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static AddArtistCommand ToCommand(this AddArtistRequest request, string? libraryId)
    {
        return new AddArtistCommand(
            libraryId,
            request.Name,
            request.Website,
            request.MusicBrainzArtistId,
            request.Contributors,
            request.Albums?.ToCommands(libraryId, null).ToList());
    }
}
