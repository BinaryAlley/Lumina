#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.DeleteMusicArtist;

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
        Description = "Soft deletes the music artist identified by the route, so its metadata, artwork and track paths are kept and the artist can be restored.";

        Response(200, "The music artist was deleted.");
    }
}
