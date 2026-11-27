#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetMusicAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMusicAlbumEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicAlbumEndpointSummary : Summary<GetMusicAlbumEndpoint, GetMusicAlbumViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicAlbumEndpointSummary"/> class.
    /// </summary>
    public GetMusicAlbumEndpointSummary()
    {
        Summary = "Retrieves the full details of a music album.";
        Description = "Retrieves the full details of the music album identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album to retrieve. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the album belongs to. Required.");

        Response(200, "The full details of the music album are retrieved.");
    }
}
