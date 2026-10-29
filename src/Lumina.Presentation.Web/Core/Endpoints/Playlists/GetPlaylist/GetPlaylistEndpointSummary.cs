#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaylistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointSummary : Summary<GetPlaylistEndpoint, GetPlaylistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointSummary"/> class.
    /// </summary>
    public GetPlaylistEndpointSummary()
    {
        Summary = "Gets a playlist of the current user.";
        Description = "Proxies the request that gets a playlist and a page of its ordered entries to the API server. The currentPage and perPage query parameters select the page, and entries.count reports the total number of entries.";

        Params["playlistId"] = "The Id of the playlist to get.";

        ExampleRequest = new GetPlaylistRequest(
            CurrentPage: 1,
            PerPage: 200
        );
        RequestParam(request => request.CurrentPage, "The one based page of entries to return. Optional; defaults to 1.");
        RequestParam(request => request.PerPage, "The number of entries per page. Optional; defaults to 200.");

        Response(200, "The playlist and its ordered entries are returned.",
            example: new SuccessResponse<PlaylistDto>(true, new PlaylistDto(
                Id: Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"),
                Name: "Evening listening",
                Description: "Quiet tracks for the end of the day.",
                Type: PlaylistType.Music,
                Entries: new PaginatedDto<PlaylistEntryDto>(
                    Data:
                    [
                        new PlaylistEntryDto(
                            Id: Guid.Parse("2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c6d"),
                            MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                            MediaItemType: PlaylistMediaItemType.MusicTrack,
                            Position: 0,
                            LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                            CollectionId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f"),
                            CreatorId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
                            MediaItemTitle: "The Show Must Go On",
                            CollectionTitle: "Innuendo",
                            CreatorName: "Queen",
                            DurationInSeconds: 262,
                            CoverPath: "/artwork/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/cover.jpg")
                    ],
                    CurrentPage: 1,
                    PerPage: 200,
                    Count: 1,
                    NumberOfPages: 1),
                CreatedOnUtc: new DateTime(2026, 10, 19, 18, 0, 0, DateTimeKind.Utc),
                UpdatedOnUtc: new DateTime(2026, 10, 19, 19, 30, 0, DateTimeKind.Utc)))
        );
    }
}
