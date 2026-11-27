#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.DeletedMusicItems;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeletedMusicItemsViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedMusicItemsViewEndpointSummary : Summary<DeletedMusicItemsViewEndpoint, GetMusicArtistsViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedMusicItemsViewEndpointSummary"/> class.
    /// </summary>
    public DeletedMusicItemsViewEndpointSummary()
    {
        Summary = "Renders the deleted items page of a music library.";
        Description = "Renders the page that lists the artists, albums and tracks the user deleted from a music library and lets the user restore them.";

        Response(200, "The deleted items page is rendered.");
    }
}
