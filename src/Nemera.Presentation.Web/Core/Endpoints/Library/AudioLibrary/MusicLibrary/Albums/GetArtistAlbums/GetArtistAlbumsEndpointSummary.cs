#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetArtistAlbums;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetArtistAlbumsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsEndpointSummary : Summary<GetArtistAlbumsEndpoint, GetArtistAlbumsLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsEndpointSummary"/> class.
    /// </summary>
    public GetArtistAlbumsEndpointSummary()
    {
        Summary = "Retrieves the lightweight details of the albums of a music artist.";
        Description = "Retrieves the lightweight read models of the albums of the music artist identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist whose albums are retrieved. Required.");
        RequestParam(r => r.LibraryId, "The Id of the media library the artist belongs to. Required.");
        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of albums to retrieve per page. Optional.");

        ExampleRequest = new GetArtistAlbumsLiteRequest(
            ArtistId: Guid.NewGuid().ToString(),
            LibraryId: Guid.NewGuid(),
            CurrentPage: 1,
            PerPage: 10
        );

        Response(200, "The lightweight details of the albums of the music artist are retrieved.");
    }
}
