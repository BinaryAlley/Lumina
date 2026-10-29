#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// Class used for providing a textual description for the <see cref="RemovePlaylistEntryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryEndpointSummary : Summary<RemovePlaylistEntryEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpointSummary"/> class.
    /// </summary>
    public RemovePlaylistEntryEndpointSummary()
    {
        Summary = "Removes an entry from a playlist of the current user.";
        Description = "Proxies the request that removes an entry from a playlist to the API server. The page removes the row from its current window without reading the playlist back.";

        Params["playlistId"] = "The Id of the playlist the entry belongs to.";
        Params["playlistEntryId"] = "The Id of the entry to remove.";

        Response(200, "The entry was removed.",
            example: new SuccessResponse(true)
        );
    }
}
