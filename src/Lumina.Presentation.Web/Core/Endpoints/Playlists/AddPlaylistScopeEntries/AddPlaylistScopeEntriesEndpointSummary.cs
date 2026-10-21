#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistScopeEntries;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddPlaylistScopeEntriesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesEndpointSummary : Summary<AddPlaylistScopeEntriesEndpoint, AddPlaylistScopeEntriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesEndpointSummary"/> class.
    /// </summary>
    public AddPlaylistScopeEntriesEndpointSummary()
    {
        Summary = "Adds every track of a library, artist or album to a playlist of the current user.";
        Description = "Proxies the request that appends every track of a music library, an artist or an album to a playlist to the API server, in catalogue order. The server resolves the scope in one set-based statement. An empty scope appends nothing and reports a zero count.";

        Params["playlistId"] = "The Id of the playlist to add the tracks to.";

        ExampleRequest = new AddPlaylistScopeEntriesRequest(
            LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
            ArtistId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
            AlbumId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f")
        );
        RequestParam(request => request.LibraryId, "The Id of the music library the scope belongs to. Required.");
        RequestParam(request => request.ArtistId, "The Id of the artist the scope is limited to. Optional.");
        RequestParam(request => request.AlbumId, "The Id of the album the scope is limited to. Optional.");

        Response(200, "The scope was appended to the playlist and the number of added entries is returned.",
            example: new SuccessResponse<PlaylistEntriesAddedDto>(true, default)
        );
    }
}
