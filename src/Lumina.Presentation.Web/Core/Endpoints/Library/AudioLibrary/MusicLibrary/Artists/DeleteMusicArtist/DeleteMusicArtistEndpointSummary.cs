#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.DeleteMusicArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteMusicArtistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicArtistEndpointSummary : Summary<DeleteMusicArtistEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicArtistEndpointSummary"/> class.
    /// </summary>
    public DeleteMusicArtistEndpointSummary()
    {
        Summary = "Deletes a music artist.";
        Description = "Deletes the music artist identified by the route, together with its albums and tracks.";

        Response(200, "The music artist was deleted.");
    }
}
