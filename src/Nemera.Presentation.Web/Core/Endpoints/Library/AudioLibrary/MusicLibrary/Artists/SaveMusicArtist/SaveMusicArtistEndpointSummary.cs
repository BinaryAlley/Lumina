#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.SaveMusicArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="SaveMusicArtistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicArtistEndpointSummary : Summary<SaveMusicArtistEndpoint, UpdateMusicArtistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicArtistEndpointSummary"/> class.
    /// </summary>
    public SaveMusicArtistEndpointSummary()
    {
        Summary = "Updates a music artist.";
        Description = "Updates the music artist identified by the route, together with the albums of the artist and the tracks of those albums.";

        Response(200, "The music artist was updated.");
    }
}
