#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMusicArtistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistEndpointSummary : Summary<GetMusicArtistEndpoint, GetMusicArtistViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistEndpointSummary"/> class.
    /// </summary>
    public GetMusicArtistEndpointSummary()
    {
        Summary = "Retrieves the full details of a music artist.";
        Description = "Retrieves the full details of the music artist identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist to retrieve. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the artist belongs to. Required.");

        Response(200, "The full details of the music artist are retrieved.");
    }
}
