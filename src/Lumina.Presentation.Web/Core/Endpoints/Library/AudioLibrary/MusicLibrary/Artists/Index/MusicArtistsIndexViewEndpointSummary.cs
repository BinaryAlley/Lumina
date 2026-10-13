#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.Index;

/// <summary>
/// Class used for providing a textual description for the <see cref="MusicArtistsIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistsIndexViewEndpointSummary : Summary<MusicArtistsIndexViewEndpoint, GetMusicArtistsViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtistsIndexViewEndpointSummary"/> class.
    /// </summary>
    public MusicArtistsIndexViewEndpointSummary()
    {
        Summary = "Renders the music artists browsing view.";
        Description = "Renders the view for browsing the music artists of the media library identified by the query.";

        RequestParam(r => r.LibraryId, "The unique identifier of the media library whose artists are browsed. Optional.");

        ExampleRequest = new GetMusicArtistsViewRequest(
            LibraryId: Guid.NewGuid()
        );

        Response(200, "The view for browsing the music artists of the media library is rendered.");
    }
}
