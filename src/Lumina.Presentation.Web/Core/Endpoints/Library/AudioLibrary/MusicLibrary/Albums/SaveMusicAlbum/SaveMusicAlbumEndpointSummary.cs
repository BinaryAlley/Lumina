#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="SaveMusicAlbumEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicAlbumEndpointSummary : Summary<SaveMusicAlbumEndpoint, UpdateMusicAlbumRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicAlbumEndpointSummary"/> class.
    /// </summary>
    public SaveMusicAlbumEndpointSummary()
    {
        Summary = "Updates a music album.";
        Description = "Updates the music album identified by the route, together with the tracks of the album.";

        Response(200, "The music album was updated.");
    }
}
