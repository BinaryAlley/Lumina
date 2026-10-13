#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetAlbumTracks;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetAlbumTracksEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksEndpointSummary : Summary<GetAlbumTracksEndpoint, GetAlbumTracksLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksEndpointSummary"/> class.
    /// </summary>
    public GetAlbumTracksEndpointSummary()
    {
        Summary = "Retrieves the lightweight details of the tracks of an album.";
        Description = "Retrieves the lightweight read models of the tracks of the album identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album whose tracks are retrieved. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the album belongs to. Required.");
        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of tracks to retrieve per page. Optional.");

        ExampleRequest = new GetAlbumTracksLiteRequest(
            ArtistId: Guid.NewGuid().ToString(),
            AlbumId: Guid.NewGuid().ToString(),
            LibraryId: Guid.NewGuid(),
            CurrentPage: 1,
            PerPage: 10
        );

        Response(200, "The lightweight details of the tracks of the album are retrieved.");
    }
}
